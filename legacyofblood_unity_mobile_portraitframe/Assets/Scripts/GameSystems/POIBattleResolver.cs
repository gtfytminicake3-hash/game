using System;
using System.Collections.Generic;
using LegendOfBlood.Combat;
using UnityEngine;

namespace LegendOfBlood
{
    public class POIBattleResolution
    {
        public bool DidWin;
        public float WinChance;
        public float Roll;
        public int PlayerCombatPower;
        public int EnemyCombatPower;
        public ExpeditionReport Report;
    }

    public static class POIBattleResolver
    {
        public static POIBattleResolution Resolve(
            POIData poi,
            SubStageNode node,
            IReadOnlyList<string> squadHeroIds,
            ProceduralDifficulty difficulty,
            ExpeditionManager expeditionManager)
        {
            List<HeroData> squad = BuildSquad(squadHeroIds);
            List<HeroData> enemies = BuildEnemies(node, difficulty);

            int playerCp = SumCombatPower(squad);
            int enemyCp = Mathf.Max(1, SumCombatPower(enemies));
            bool isCombatNode = IsCombatNode(node);
            float chance = isCombatNode ? CalculateWinChance(playerCp, enemyCp) : 1f;
            float roll = UnityEngine.Random.value;
            bool didWin = !isCombatNode || roll <= chance;

            CombatResult result = BuildCombatResult(squad, enemies, didWin, chance, roll, playerCp, enemyCp);
            LootData loot = didWin ? BuildLoot(poi, node, expeditionManager) : new LootData();
            int exp = didWin ? BuildExperience(poi, node, expeditionManager) : 0;

            // --- LưU TRẠNG THÁI QUÁI VẬT VÀO NODE ---
            if (node != null && IsCombatNode(node))
            {
                if (didWin)
                {
                    // Thắng: xóa dữ liệu quái cũ để node được coi là Cleared
                    node.SurvivingEnemies = null;
                }
                else
                {
                    // Thua: lưu lại danh sách quái còn sống. Do POIBattleResolver dùng CP-based
                    // chứ không simulate từng turn nên chúớng ta cần tính HP còn lại theo tỉ lệ.
                    // Tỉ lệ thiệt hại của địch = sức mạnh đội quân / (sức mạnh địch + sức mạnh đội quân)
                    float damageFraction = Mathf.Clamp01((float)playerCp / (playerCp + enemyCp));
                    var survivingEnemyClones = new List<HeroData>();
                    foreach (var enemy in enemies)
                    {
                        var clone = enemy.Clone();
                        float maxHp = clone.GetFinalStats().hp;
                        // Giảm HP của quái theo tỉ lệ damage, nhưng không xuống dưới 1
                        clone.currentHp = Mathf.Max(1f, maxHp * (1f - damageFraction));
                        survivingEnemyClones.Add(clone);
                    }
                    node.SurvivingEnemies = survivingEnemyClones;
                }
            }
            // -------------------------------------------

            return new POIBattleResolution
            {
                DidWin = didWin,
                WinChance = chance,
                Roll = roll,
                PlayerCombatPower = playerCp,
                EnemyCombatPower = enemyCp,
                Report = new ExpeditionReport
                {
                    poiId = poi?.poiId,
                    poiName = poi?.poiName,
                    combatResult = result,
                    loot = loot,
                    experienceGained = exp
                }
            };
        }

        private static List<HeroData> BuildSquad(IReadOnlyList<string> squadHeroIds)
        {
            var squad = new List<HeroData>();
            if (squadHeroIds == null || DataManager.Instance == null) return squad;

            foreach (string id in squadHeroIds)
            {
                HeroData hero = DataManager.Instance.GetHeroByID(id);
                if (hero != null) squad.Add(hero.Clone());
            }
            return squad;
        }

        private static List<HeroData> BuildEnemies(SubStageNode node, ProceduralDifficulty difficulty)
        {
            var enemies = new List<HeroData>();
            if (node == null || DataManager.Instance == null) return enemies;

            // Nếu node đã có danh sách quái còn sống từ lần thua trước -> dùng lại với HP đã bị cào
            if (node.SurvivingEnemies != null && node.SurvivingEnemies.Count > 0)
            {
                // Clone lại để thống nhất: không sửa trực tiếp dữ liệu gốc
                foreach (var enemy in node.SurvivingEnemies)
                {
                    enemies.Add(enemy.Clone());
                }
                return enemies;
            }

            // Không có dữ liệu cũ -> tạo mới từ định nghĩa node như bình thường
            int effectiveDifficulty = Mathf.Max(1, (int)difficulty + 1 + Mathf.Max(0, node.Floor / 3));
            foreach (string monsterId in ExpandMonsterIds(node.ExpectedMonsters))
            {
                HeroData monster = DataManager.Instance.GetMonsterByID(monsterId, effectiveDifficulty);
                if (monster != null) enemies.Add(monster);
            }

            if (enemies.Count == 0 && IsCombatNode(node))
            {
                HeroData fallback = DataManager.Instance.GetMonsterByID("Goblin", effectiveDifficulty);
                if (fallback != null) enemies.Add(fallback);
            }

            return enemies;
        }

        private static IEnumerable<string> ExpandMonsterIds(IEnumerable<string> expectedMonsters)
        {
            if (expectedMonsters == null) yield break;

            foreach (string entry in expectedMonsters)
            {
                if (string.IsNullOrWhiteSpace(entry)) continue;

                string monsterId = entry.Trim();
                int count = 1;
                int marker = monsterId.LastIndexOf(" x", StringComparison.OrdinalIgnoreCase);
                if (marker >= 0)
                {
                    string countText = monsterId.Substring(marker + 2).Trim();
                    monsterId = monsterId.Substring(0, marker).Trim();
                    int.TryParse(countText, out count);
                    count = Mathf.Clamp(count, 1, 12);
                }

                for (int i = 0; i < count; i++)
                {
                    yield return monsterId;
                }
            }
        }

        private static bool IsCombatNode(SubStageNode node)
        {
            return node != null && (node.Type == SubStageNodeType.Combat || node.Type == SubStageNodeType.Elite || node.Type == SubStageNodeType.Boss);
        }

        private static int SumCombatPower(IEnumerable<HeroData> heroes)
        {
            int total = 0;
            if (heroes == null) return total;
            foreach (HeroData hero in heroes)
            {
                if (hero != null) total += hero.GetCombatPower();
            }
            return total;
        }

        private static float CalculateWinChance(int playerCp, int enemyCp)
        {
            if (playerCp <= 0) return 0f;
            float raw = playerCp / Mathf.Max(1f, playerCp + enemyCp);
            return Mathf.Clamp(raw, 0.05f, 0.95f);
        }

        private static CombatResult BuildCombatResult(List<HeroData> squad, List<HeroData> enemies, bool didWin, float chance, float roll, int playerCp, int enemyCp)
        {
            var log = new List<string>
            {
                $"POI battle resolved by CP chance. PlayerCP={playerCp}, EnemyCP={enemyCp}, WinChance={chance:P0}, Roll={roll:P0}.",
                didWin ? "Victory: stage cleared." : "Defeat: stage progress stays at the current node."
            };

            return new CombatResult
            {
                DidPlayerWin = didWin,
                TotalTurns = 1,
                CombatLog = log,
                EventLog = new List<CombatEvent>(),
                PlayerSurvivors = didWin ? CloneList(squad) : new List<HeroData>(),
                PlayerCasualties = didWin ? new List<HeroData>() : CloneList(squad),
                EnemySurvivors = didWin ? new List<HeroData>() : CloneList(enemies),
                EnemyCasualties = didWin ? CloneList(enemies) : new List<HeroData>(),
                InitialPositions = new List<CombatantPosition>()
            };
        }

        private static List<HeroData> CloneList(IEnumerable<HeroData> heroes)
        {
            var result = new List<HeroData>();
            if (heroes == null) return result;
            foreach (HeroData hero in heroes)
            {
                if (hero != null) result.Add(hero.Clone());
            }
            return result;
        }

        private static LootData BuildLoot(POIData poi, SubStageNode node, ExpeditionManager expeditionManager)
        {
            if (expeditionManager != null)
            {
                if (node != null && node.Type == SubStageNodeType.Boss)
                {
                    string bossId = FirstMonsterId(node) ?? "BOSS_01";
                    return expeditionManager.CalculateBossLoot(bossId, true);
                }

                if (poi != null) return expeditionManager.CalculateLoot(poi, true);
            }

            return new LootData { gold = 100 + Mathf.Max(1, poi?.difficultyLevel ?? 1) * 25 };
        }

        private static int BuildExperience(POIData poi, SubStageNode node, ExpeditionManager expeditionManager)
        {
            if (node != null && node.Type == SubStageNodeType.Boss) return 500;
            if (expeditionManager != null && poi != null) return expeditionManager.CalculateExperience(poi, true);
            return 50;
        }

        private static string FirstMonsterId(SubStageNode node)
        {
            foreach (string id in ExpandMonsterIds(node?.ExpectedMonsters))
            {
                return id;
            }
            return null;
        }
    }
}

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

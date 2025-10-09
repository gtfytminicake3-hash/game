namespace LegendOfBlood
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Chứa logic mô phỏng các trận đấu (Thường và Boss).
    /// Hoàn toàn độc lập với UI, chỉ xử lý dữ liệu và trả về kết quả.
    /// </summary>
    public class CombatSystem
    {
        // Hằng số ID của Trait đặc biệt
        private const string TRAIT_ROYAL_GUARD = "SS_08";
        private const string TRAIT_REBIRTH = "S_01";
        private const string TRAIT_PREDATOR = "SS_06";
        private const string TRAIT_LEADERSHIP = "A_05"; // Chưa triển khai logic hàng

        /// <summary>
        /// Mô phỏng một trận đấu thường.
        /// </summary>
        /// <param name="playerHeroes">Đội hình hero của người chơi</param>
        /// <param name="enemyHeroes">Đội hình kẻ địch</param>
        /// <returns>Đối tượng CombatResult chứa kết quả trận đấu</returns>
        public CombatResult SimulateNormalBattle(List<HeroData> playerHeroes, List<HeroData> enemyHeroes)
        {
            var combatLog = new List<string>();
            var playerCombatants = playerHeroes.Select(h => new Combatant(h, true)).ToList();
            var enemyCombatants = enemyHeroes.Select(h => new Combatant(h, false)).ToList();

            combatLog.Add("<b>Trận đấu bắt đầu!</b>");

            // --- GIAI ĐOẠN ĐẦU TRẬN (ÁP DỤNG AURA) ---
            ApplyPreBattleAuras(playerCombatants, enemyCombatants, combatLog);
            ApplyPreBattleAuras(enemyCombatants, playerCombatants, combatLog);

            // --- VÒNG LẶP CHIẾN ĐẤU ---
            int turn = 1;
            while (IsTeamAlive(playerCombatants) && IsTeamAlive(enemyCombatants))
            {
                combatLog.Add($"\n<color=yellow>--- Vòng {turn} ---</color>");

                var turnOrder = playerCombatants.Concat(enemyCombatants)
                                                 .Where(c => c.IsAlive())
                                                 .OrderByDescending(c => c.Spd)
                                                 .ToList();

                foreach (var attacker in turnOrder)
                {
                    if (!attacker.IsAlive()) continue; // Bị hạ gục trong cùng một lượt

                    var targetTeam = attacker.IsPlayerTeam ? enemyCombatants : playerCombatants;
                    var target = GetRandomLivingTarget(targetTeam);
                    if (target == null) break; // Toàn bộ team địch đã bị hạ gục

                    // Tấn công
                    PerformAttack(attacker, target, combatLog);
                }
                turn++;
                if (turn > 50) { combatLog.Add("Trận đấu quá dài, kết quả hòa!"); break; } // Chống lặp vô hạn
            }

            // --- GIAI ĐOẠN KẾT THÚC TRẬN ---
            bool playerWon = IsTeamAlive(playerCombatants) && !IsTeamAlive(enemyCombatants);
            combatLog.Add(playerWon ? "\n<color=green><b>CHIẾN THẮNG!</b></color>" : "\n<color=red><b>THẤT BẠI!</b></color>");

            return new CombatResult
            {
                DidPlayerWin = playerWon,
                PlayerSurvivors = playerCombatants.Where(c => c.IsAlive()).Select(c => c.HeroRef).ToList(),
                PlayerCasualties = playerCombatants.Where(c => !c.IsAlive()).Select(c => c.HeroRef).ToList(),
                CombatLog = combatLog
            };
        }
        
        /// <summary>
        /// Xử lý một lượt tấn công cơ bản.
        /// </summary>
        private void PerformAttack(Combatant attacker, Combatant target, List<string> log)
        {
            // Công thức sát thương cơ bản từ GDD_03
            int damageFloor = Mathf.FloorToInt(attacker.Atk * 0.1f);
            int damageDealt = Mathf.Max(damageFloor, Mathf.FloorToInt(attacker.Atk - target.Def));

            string logMessage = $"{attacker.HeroRef.heroName} tấn công {target.HeroRef.heroName}.";

            // Áp dụng sát thương
            int remainingDamage = damageDealt;
            if (target.CurrentShield > 0)
            {
                int shieldDamage = Mathf.Min(remainingDamage, target.CurrentShield);
                target.CurrentShield -= shieldDamage;
                remainingDamage -= shieldDamage;
                logMessage += $" Phá <color=cyan>{shieldDamage}</color> giáp.";
            }

            if (remainingDamage > 0)
            {
                target.CurrentHp -= remainingDamage;
                logMessage += $" Gây <color=red>{remainingDamage}</color> sát thương.";
            }

            log.Add(logMessage + $" ({target.HeroRef.heroName} còn {target.CurrentHp} HP)");

            // Xử lý khi mục tiêu bị hạ gục
            if (!target.IsAlive())
            {
                HandleTargetDefeated(attacker, target, log);
            }
        }

        /// <summary>
        /// Xử lý các sự kiện khi một mục tiêu bị hạ gục.
        /// </summary>
        private void HandleTargetDefeated(Combatant attacker, Combatant defeatedTarget, List<string> log)
        {
            log.Add($"<color=grey>{defeatedTarget.HeroRef.heroName} đã bị hạ gục!</color>");

            // Xử lý Trait Tái Sinh (S_01)
            if (defeatedTarget.TraitIDs.Contains(TRAIT_REBIRTH) && Random.value < 0.5f) // 50% cơ hội
            {
                int revivedHp = Mathf.FloorToInt(defeatedTarget.MaxHp * 0.25f);
                defeatedTarget.CurrentHp = revivedHp;
                log.Add($"<color=green>...Nhưng {defeatedTarget.HeroRef.heroName} đã Tái Sinh với {revivedHp} HP!</color>");
                return; // Không kích hoạt các hiệu ứng "khi hạ gục" khác
            }

            // Xử lý Trait Kẻ Săn Mồi (SS_06) của kẻ tấn công
            if (attacker.TraitIDs.Contains(TRAIT_PREDATOR))
            {
                float oldAtk = attacker.Atk;
                attacker.Atk *= 1.20f; // Tăng 20% ATK
                log.Add($"<color=orange>{attacker.HeroRef.heroName} kích hoạt Kẻ Săn Mồi, ATK tăng từ {oldAtk:F0} lên {attacker.Atk:F0}!</color>");
            }
        }

        /// <summary>
        /// Áp dụng các hiệu ứng Aura đầu trận.
        /// </summary>
        private void ApplyPreBattleAuras(List<Combatant> team, List<Combatant> enemyTeam, List<string> log)
        {
            foreach (var combatant in team)
            {
                // Trait Hộ Vệ Hoàng Gia (SS_08)
                if (combatant.TraitIDs.Contains(TRAIT_ROYAL_GUARD))
                {
                    int shieldAmount = Mathf.FloorToInt(combatant.MaxHp * 0.15f);
                    // Áp dụng cho toàn đội
                    foreach (var ally in team)
                    {
                        ally.CurrentShield += shieldAmount;
                    }
                    log.Add($"<color=cyan>{combatant.HeroRef.heroName} kích hoạt Hộ Vệ Hoàng Gia, tạo {shieldAmount} giáp cho toàn đội!</color>");
                }
                
                // TODO: Trait Lãnh Đạo (A_05) cần logic về "hàng"
            }
        }

        #region Helper Methods
        private bool IsTeamAlive(List<Combatant> team) => team.Any(c => c.IsAlive());

        private Combatant GetRandomLivingTarget(List<Combatant> team)
        {
            var livingTargets = team.Where(c => c.IsAlive()).ToList();
            if (livingTargets.Count == 0) return null;
            return livingTargets[Random.Range(0, livingTargets.Count)];
        }
        #endregion

        // TODO: Viết hàm SimulateBossBattle dựa trên GDD
        public CombatResult SimulateBossBattle(List<HeroData> vanguardSquad, List<HeroData> coreSquad, List<HeroData> supportSquad, BossData boss)
        {
            // ... Logic cho trận đấu Boss sẽ được triển khai ở đây
            return new CombatResult { DidPlayerWin = false, CombatLog = new List<string> { "Chức năng đấu Boss chưa được triển khai." } };
        }
    }
}
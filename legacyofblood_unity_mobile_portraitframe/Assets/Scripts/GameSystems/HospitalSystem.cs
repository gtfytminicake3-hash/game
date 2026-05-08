namespace LegendOfBlood
{
    using System;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Quản lý trạng thái bị thương, thời gian hồi phục và chi phí chữa trị cho các hero.
    /// </summary>
    public class HospitalSystem
    {
        private const long SEVERE_INJURY_DURATION_MS = 8 * 60 * 60 * 1000; // 8 giờ

        // Events để UI có thể lắng nghe
        public static event Action<HeroData> OnHeroHealed;
        public static event Action<HeroData> OnHeroPerished; // Khi hero chết vĩnh viễn

        #region Public API (Methods called by other systems like Combat or UI)

        /// <summary>
        /// Admits a hero to the hospital system after being a casualty in an expedition.
        /// This will inflict a severe injury.
        /// </summary>
        public void AdmitHero(string heroId)
        {
            var hero = DataManager.Instance.GetHeroByID(heroId);
            if (hero != null)
            {
                AdmitForSevereInjury(hero);
            }
            else
            {
                Debug.LogWarning($"Attempted to admit a hero with ID {heroId} who could not be found.");
            }
        }

        public void InflictLightInjury(HeroData hero)
        {
            float maxHp = hero.GetFinalStats().hp;
            float missingHp = maxHp - hero.currentHp;
            if (missingHp <= 0) return;

            // Get Hospital level to calculate healing rate
            int hospitalLevel = 1;
            if (DataManager.Instance != null && DataManager.Instance.AllBuildings != null)
            {
                var hospital = DataManager.Instance.AllBuildings.FirstOrDefault(b => b.id == "Hospital");
                if (hospital != null && hospital.level > 0)
                {
                    hospitalLevel = hospital.level;
                }
            }

            // Tốc độ hồi máu = 1 HP/s * Cấp độ bệnh viện
            float healRatePerSecond = 1f * hospitalLevel;
            
            // Thời gian hồi phục = Số máu mất / Tốc độ hồi máu
            long recoveryDurationMs = (long)((missingHp / healRatePerSecond) * 1000);

            hero.isLightlyInjured = true;
            hero.lightInjuryEndTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + recoveryDurationMs;
            Debug.Log($"{hero.heroName} bị thương nhẹ (thiếu {missingHp} HP). Cần {recoveryDurationMs / 1000}s để hồi phục (Tốc độ: {healRatePerSecond} HP/s).");
        }

        /// <summary>
        /// Đưa hero vào trạng thái bị thương nặng.
        /// </summary>
        public void AdmitForSevereInjury(HeroData hero)
        {
            hero.isSeverelyInjured = true;
            hero.injuryEndTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + SEVERE_INJURY_DURATION_MS;
            Debug.LogWarning($"{hero.heroName} bị thương nặng! Sẽ biến mất vĩnh viễn sau {SEVERE_INJURY_DURATION_MS / 1000 / 3600} giờ nếu không được cứu chữa.");
        }

        /// <summary>
        /// Hồi phục ngay lập tức vết thương nhẹ bằng cách trả Vàng.
        /// </summary>
        /// <returns>True nếu hồi phục thành công, False nếu không đủ tiền.</returns>
        public bool HealLightInjuryInstantly(HeroData hero)
        {
            int cost = CalculateLightHealCost(hero);
            
            bool success = true; // Assume success if InventoryManager is missing (Test environment)
            if (GameManager.Instance != null && GameManager.Instance.InventoryManager != null)
            {
                success = GameManager.Instance.InventoryManager.SpendResource(ResourceType.Gold, cost);
            }

            if (success)
            {
                Debug.Log($"{hero.heroName} đã hồi phục vết thương nhẹ ngay lập tức với giá {cost} vàng.");
                ClearLightInjury(hero);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Cứu chữa vết thương nặng bằng cách trả Vàng.
        /// </summary>
        /// <returns>True nếu cứu chữa thành công, False nếu không đủ tiền.</returns>
        public bool HealSevereInjury(HeroData hero)
        {
            int cost = CalculateSevereHealCost(hero);
            
            bool success = true; // Assume success if InventoryManager is missing (Test environment)
            if (GameManager.Instance != null && GameManager.Instance.InventoryManager != null)
            {
                success = GameManager.Instance.InventoryManager.SpendResource(ResourceType.Gold, cost);
            }
            
            if (success)
            {
                Debug.Log($"{hero.heroName} đã được cứu chữa khỏi vết thương nặng với giá {cost} vàng.");
                ClearSevereInjury(hero);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Cứu chữa vết thương nặng MIỄN PHÍ.
        /// </summary>
        public void HealSevereInjuryFree(HeroData hero)
        {
            Debug.Log($"{hero.heroName} đã được cứu chữa khỏi vết thương nặng MIỄN PHÍ.");
            ClearSevereInjury(hero);
        }

        /// <summary>
        /// Rút ngắn thời gian hồi phục bằng vật phẩm.
        /// </summary>
        public bool SpeedUpHealing(HeroData hero, string itemId)
        {
            if (hero == null) return false;
            if (!hero.isLightlyInjured && !hero.isSeverelyInjured) return false;

            var item = DataManager.Instance.AllItems.TryGetValue(itemId, out var itemData) ? itemData : null;
            if (item == null || item.type != ItemType.SpeedUp) return false;

            bool success = true; // Assume success if InventoryManager is missing (Test environment)
            if (GameManager.Instance != null && GameManager.Instance.InventoryManager != null)
            {
                success = GameManager.Instance.InventoryManager.UseItem(itemId, 1);
            }

            if (success)
            {
                long speedUpMs = item.speedUpValueInSeconds * 1000;
                if (hero.isLightlyInjured) hero.lightInjuryEndTime -= speedUpMs;
                if (hero.isSeverelyInjured) hero.injuryEndTime -= speedUpMs;
                
                Debug.Log($"{hero.heroName} đã sử dụng {item.itemName} để giảm {item.speedUpValueInSeconds}s thời gian hồi phục.");
                return true;
            }
            return false;
        }

        #endregion

        #region Core Logic (Ticked by GameManager)

        /// <summary>
        /// Hàm tick được gọi bởi GameManager để xử lý hồi phục tự động và cái chết vĩnh viễn.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (DataManager.Instance == null || DataManager.Instance.Player == null) return;

            var allHeroes = DataManager.Instance.AllHeroes;
            if (allHeroes == null || allHeroes.Count == 0) return;

            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Dùng ToList() để tạo bản sao, cho phép xóa hero khỏi danh sách gốc một cách an toàn
            foreach (var hero in allHeroes.ToList())
            {
                // Xử lý hồi phục vết thương nhẹ
                if (hero.isLightlyInjured && currentTime >= hero.lightInjuryEndTime)
                {
                    Debug.Log($"{hero.heroName} đã tự động hồi phục vết thương nhẹ.");
                    ClearLightInjury(hero);
                }

                // Xử lý cái chết vĩnh viễn do vết thương nặng
                if (hero.isSeverelyInjured && currentTime >= hero.injuryEndTime)
                {
                    Debug.LogError($"{hero.heroName} đã không được cứu chữa kịp thời và biến mất vĩnh viễn!");
                    OnHeroPerished?.Invoke(hero);
                    DataManager.Instance.RemoveHero(hero.id); // Xóa hero khỏi game
                }
            }
        }

        #endregion

        #region Helper Methods

        // Xóa trạng thái bị thương nhẹ và hồi đầy máu
        private void ClearLightInjury(HeroData hero)
        {
            hero.isLightlyInjured = false;
            hero.currentHp = hero.GetFinalStats().hp; // Hồi đầy máu
            OnHeroHealed?.Invoke(hero);
            DataManager.TriggerHeroAvailabilityChanged(hero);
            DataManager.TriggerHeroStatsChanged(hero);
            DataManager.Instance?.SavePlayerData();
        }

        // Xóa trạng thái bị thương nặng và hồi đầy máu
        private void ClearSevereInjury(HeroData hero)
        {
            hero.isSeverelyInjured = false;
            hero.currentHp = hero.GetFinalStats().hp; // Hồi đầy máu
            OnHeroHealed?.Invoke(hero);
            DataManager.TriggerHeroAvailabilityChanged(hero);
            DataManager.TriggerHeroStatsChanged(hero);
            DataManager.Instance?.SavePlayerData();
        }
        
        // Công thức tính chi phí hồi phục vết thương nhẹ từ GDD_02
        private int CalculateLightHealCost(HeroData hero)
        {
            // Chi phí: floor(CP / 50) + 10 Vàng
            return Mathf.FloorToInt(hero.GetCombatPower() / 50f) + 10;
        }

        // Công thức tính chi phí hồi phục vết thương nặng từ GDD_02
        private int CalculateSevereHealCost(HeroData hero)
        {
            // Chi phí: floor(CP / 10) + 50 Vàng
            return Mathf.FloorToInt(hero.GetCombatPower() / 10f) + 50;
        }

        #endregion
    }
}
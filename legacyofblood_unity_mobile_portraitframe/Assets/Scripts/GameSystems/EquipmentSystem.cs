using System;
using System.Collections.Generic;
using UnityEngine;

namespace LegendOfBlood
{
    public static class EquipmentSystem
    {
        public const int MAX_LEVEL = 100;
        public const int EXP_THRESHOLD_SOFT_CAP = 40;

        /// <summary>
        /// Sinh ra đồ ngẫu nhiên từ tháp
        /// </summary>
        public static EquipmentData GenerateRandomEquipment(int floorDifficulty)
        {
            // Level rớt ra từ Tầng 1 đến 40 phụ thuộc theo độ khó của tầng tháp.
            int minLevel = Mathf.Clamp(floorDifficulty - 5, 1, 40);
            int maxLevel = Mathf.Clamp(floorDifficulty + 2, 1, 40);
            int dropLevel = UnityEngine.Random.Range(minLevel, maxLevel + 1);

            EquipmentSlot dropSlot = (UnityEngine.Random.value > 0.5f) ? EquipmentSlot.Weapon : EquipmentSlot.Armor;
            string dropName = dropSlot == EquipmentSlot.Weapon ? "Vũ Khí Cổ Đại" : "Giáp Cổ Đại";
            string id = Guid.NewGuid().ToString();

            EquipmentData newEquip = new EquipmentData(id, dropName, dropSlot, dropLevel);
            
            // Random độ hiếm (quyết định sức mạnh cơ bản)
            newEquip.rarity = UnityEngine.Random.Range(1, 4); 

            // Scale Base Stats theo Level
            ScaleBaseStats(newEquip);

            // Sinh 2 dòng Bonus Stats
            GenerateBonusStats(newEquip);

            return newEquip;
        }

        private static void ScaleBaseStats(EquipmentData equip)
        {
            float levelMultiplier = 1f + (equip.level * 0.1f * equip.rarity);
            
            if (equip.slot == EquipmentSlot.Weapon)
            {
                equip.atkBonus = Mathf.Round(10f * levelMultiplier);
            }
            else if (equip.slot == EquipmentSlot.Armor)
            {
                equip.hpBonus = Mathf.Round(50f * levelMultiplier);
                equip.defBonus = Mathf.Round(5f * levelMultiplier);
            }
        }

        private static void GenerateBonusStats(EquipmentData equip)
        {
            // Reset modifiers
            equip.hpMultiplier = 0; equip.atkMultiplier = 0; equip.defMultiplier = 0; equip.spdMultiplier = 0;
            equip.critChanceBonus = 0; equip.critDamageBonus = 0;

            string[] possibleBonusDesc = new string[2];

            for (int i = 0; i < 2; i++)
            {
                int roll = UnityEngine.Random.Range(0, 4);
                float value = 0;
                switch (roll)
                {
                    case 0: // +% ATK
                        value = UnityEngine.Random.Range(0.05f, 0.15f);
                        equip.atkMultiplier += value;
                        possibleBonusDesc[i] = $"+{Mathf.Round(value * 100)}% ATK";
                        break;
                    case 1: // +% HP
                        value = UnityEngine.Random.Range(0.05f, 0.15f);
                        equip.hpMultiplier += value;
                        possibleBonusDesc[i] = $"+{Mathf.Round(value * 100)}% HP";
                        break;
                    case 2: // Crit Chance
                        value = UnityEngine.Random.Range(0.02f, 0.08f);
                        equip.critChanceBonus += value;
                        possibleBonusDesc[i] = $"+{Mathf.Round(value * 100)}% Tỉ lệ Chí Mạng";
                        break;
                    case 3: // Speed Flat
                        value = UnityEngine.Random.Range(2f, 10f);
                        equip.spdBonus += value;
                        possibleBonusDesc[i] = $"+{Mathf.Round(value)} Tốc độ";
                        break;
                }
            }

            equip.bonusStat1Description = possibleBonusDesc[0];
            equip.bonusStat2Description = possibleBonusDesc[1];
        }

        /// <summary>
        /// Tính exp cần thiết để lên level tiếp theo.
        /// Đặc tả: Lên level 1-40 rất dễ, từ 41-100 cực khó (đường cong mũ - Exponential).
        /// </summary>
        public static int GetExpRequiredForLevel(int currentLevel)
        {
            if (currentLevel >= MAX_LEVEL) return int.MaxValue;

            if (currentLevel < EXP_THRESHOLD_SOFT_CAP)
            {
                // Tuyến tính dễ ẹc: 10, 20, 30...
                return currentLevel * 10;
            }
            else
            {
                // Thang siêu khó (Cấp độ x Cấp độ x Mũ)
                int extraLevels = currentLevel - EXP_THRESHOLD_SOFT_CAP + 1; 
                return (int)(500 * Mathf.Pow(1.2f, extraLevels)); 
            }
        }

        /// <summary>
        /// Trả về số exp cống hiến khi đem trang bị này đi làm vật liệu
        /// </summary>
        public static int GetExpYield(EquipmentData material)
        {
            // Cơ bản cho 5 exp. Cộng thêm exp từ level của vật liệu.
            return 5 + (material.level * 2) + (material.currentExp / 2);
        }

        /// <summary>
        /// Thực hiện cường hóa, ăn vật liệu để tăng cấp cho target
        /// </summary>
        public static bool UpgradeEquipment(EquipmentData target, List<EquipmentData> materials)
        {
            if (target.level >= MAX_LEVEL) return false;
            
            int totalExpGained = 0;
            foreach (var mat in materials)
            {
                totalExpGained += GetExpYield(mat);
            }

            target.currentExp += totalExpGained;

            // Xử lý Lên Cấp
            bool leveledUp = false;
            while (target.level < MAX_LEVEL && target.currentExp >= GetExpRequiredForLevel(target.level))
            {
                target.currentExp -= GetExpRequiredForLevel(target.level);
                target.level++;
                leveledUp = true;
            }

            if (leveledUp)
            {
                ScaleBaseStats(target); // Cập nhật lại sức mạnh
            }

            return true; // Có EXP thêm vào
        }
    }
}

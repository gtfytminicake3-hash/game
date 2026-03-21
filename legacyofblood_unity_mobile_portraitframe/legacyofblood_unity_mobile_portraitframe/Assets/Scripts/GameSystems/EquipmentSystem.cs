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

            // Random 1 trong 6 slot
            Array slots = Enum.GetValues(typeof(EquipmentSlot));
            EquipmentSlot dropSlot = (EquipmentSlot)slots.GetValue(UnityEngine.Random.Range(0, slots.Length));
            
            string dropName = "Trang Bị Cổ Đại";
            switch (dropSlot)
            {
                case EquipmentSlot.Weapon: dropName = "Vũ Khí Cổ Đại"; break;
                case EquipmentSlot.Armor: dropName = "Giáp Cổ Đại"; break;
                case EquipmentSlot.Helm: dropName = "Mũ Cổ Đại"; break;
                case EquipmentSlot.Boots: dropName = "Giày Cổ Đại"; break;
                case EquipmentSlot.Ring1: 
                case EquipmentSlot.Ring2: dropName = "Nhẫn Cổ Đại"; break;
            }
            string id = Guid.NewGuid().ToString();

            // Random Profession restriction. (Warrior, Archer, Mage, Healer)
            // Index 1 to 4 because 0 is None
            Profession dropClass = Profession.None;
            if (dropSlot != EquipmentSlot.Ring1 && dropSlot != EquipmentSlot.Ring2)
            {
                Array classes = Enum.GetValues(typeof(Profession));
                // We assume enum is: None=0, Warrior=1, Archer=2, Mage=3, Healer=4
                dropClass = (Profession)classes.GetValue(UnityEngine.Random.Range(1, classes.Length));
            }

            EquipmentData newEquip = new EquipmentData(id, dropName, dropSlot, dropLevel, EquipmentTier.D, dropClass);
            
            // Random độ hiếm (quyết định sức mạnh cơ bản)
            // Sinh ngẫu nhiên phẩm chất từ D đến SSS
            Array tiers = Enum.GetValues(typeof(EquipmentTier));
            // Tạo trọng số để đổ mỡ: D dễ ra nhất, SSS khó ra nhất.
            float roll = UnityEngine.Random.value;
            EquipmentTier droppedTier = EquipmentTier.D;
            if (roll > 0.99f) droppedTier = EquipmentTier.SSS;
            else if (roll > 0.95f) droppedTier = EquipmentTier.SS;
            else if (roll > 0.85f) droppedTier = EquipmentTier.S;
            else if (roll > 0.70f) droppedTier = EquipmentTier.A;
            else if (roll > 0.40f) droppedTier = EquipmentTier.B;
            else if (roll > 0.15f) droppedTier = EquipmentTier.C;
            
            newEquip.tier = droppedTier;

            // Scale Base Stats theo Level và Tier
            ScaleBaseStats(newEquip);

            // Sinh 2 dòng Bonus Stats
            GenerateBonusStats(newEquip);

            return newEquip;
        }

        private static void ScaleBaseStats(EquipmentData equip)
        {
            // Map Tier to a multiplier factor: D=1, C=1.5, B=2, A=3, S=4, SS=5, SSS=7 
            float tierMultiplier = 1f;
            switch(equip.tier) {
                case EquipmentTier.D: tierMultiplier = 1.0f; break;
                case EquipmentTier.C: tierMultiplier = 1.5f; break;
                case EquipmentTier.B: tierMultiplier = 2.0f; break;
                case EquipmentTier.A: tierMultiplier = 3.0f; break;
                case EquipmentTier.S: tierMultiplier = 4.0f; break;
                case EquipmentTier.SS: tierMultiplier = 5.0f; break;
                case EquipmentTier.SSS: tierMultiplier = 7.0f; break;
            }

            float levelMultiplier = 1f + (equip.level * 0.1f * tierMultiplier);
            
            switch (equip.slot)
            {
                case EquipmentSlot.Weapon:
                    equip.atkBonus = Mathf.Round(10f * levelMultiplier);
                    break;
                case EquipmentSlot.Armor:
                    equip.hpBonus = Mathf.Round(25f * levelMultiplier);
                    equip.defBonus = Mathf.Round(5f * levelMultiplier);
                    break;
                case EquipmentSlot.Helm:
                    equip.hpBonus = Mathf.Round(50f * levelMultiplier);
                    break;
                case EquipmentSlot.Boots:
                    equip.spdBonus = Mathf.Round(2f * levelMultiplier);
                    break;
                case EquipmentSlot.Ring1:
                case EquipmentSlot.Ring2:
                    // Main stat random for Ring: %ATK, %Crit Chance, or %Crit Damage
                    int randomRingStat = UnityEngine.Random.Range(0, 3);
                    float ringValue = 0.05f * levelMultiplier; // Base 5%
                    if (randomRingStat == 0) equip.atkMultiplier += ringValue;
                    else if (randomRingStat == 1) equip.critChanceBonus += (ringValue / 2f); // Crit chance scales slower
                    else equip.critDamageBonus += ringValue;
                    break;
            }
        }

        private static void GenerateBonusStats(EquipmentData equip)
        {
            // Reset modifiers
            equip.hpMultiplier = 0; equip.atkMultiplier = 0; equip.defMultiplier = 0; equip.spdMultiplier = 0;
            equip.critChanceBonus = 0; equip.critDamageBonus = 0;
            equip.evasionBonus = 0; equip.damageReductionBonus = 0; equip.damageIncreaseBonus = 0;

            string[] possibleBonusDesc = new string[2];

            for (int i = 0; i < 2; i++)
            {
                int roll = GetRandomSubstatRollForSlot(equip.slot);
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
                        // Since boots has main speed, we might want flat speed as substat for others.
                        value = UnityEngine.Random.Range(2f, 10f);
                        equip.spdBonus += value;
                        possibleBonusDesc[i] = $"+{Mathf.Round(value)} Tốc độ";
                        break;
                    case 4: // +% DEF
                        value = UnityEngine.Random.Range(0.05f, 0.15f);
                        equip.defMultiplier += value;
                        possibleBonusDesc[i] = $"+{Mathf.Round(value * 100)}% DEF";
                        break;
                    case 5: // +% Damage Reduction (Áo, Nhẫn)
                        value = UnityEngine.Random.Range(0.02f, 0.08f);
                        equip.damageReductionBonus += value;
                        possibleBonusDesc[i] = $"+{value * 100:F1}% Giảm Sát Thương";
                        break;
                    case 6: // +% Evasion (Mũ, Giày, Nhẫn)
                        value = UnityEngine.Random.Range(0.02f, 0.08f);
                        equip.evasionBonus += value;
                        possibleBonusDesc[i] = $"+{value * 100:F1}% Né Tránh";
                        break;
                    case 7: // +% Damage Increase (Vũ khí, Nhẫn)
                        value = UnityEngine.Random.Range(0.02f, 0.08f);
                        equip.damageIncreaseBonus += value;
                        possibleBonusDesc[i] = $"+{value * 100:F1}% Sát Thương Gây Ra";
                        break;
                }
            }

            equip.bonusStat1Description = possibleBonusDesc[0];
            equip.bonusStat2Description = possibleBonusDesc[1];
        }

        private static int GetRandomSubstatRollForSlot(EquipmentSlot slot)
        {
            // Pool:
            // 0: ATK%, 1: HP%, 2: Crit%, 3: Speed, 4: DEF%
            // 5: DMG Reduc, 6: Evasion, 7: DMG Increase
            switch (slot)
            {
                case EquipmentSlot.Weapon:
                    {
                        int[] pool = { 0, 2, 7 }; // ATK%, Crit%, DMG Increase
                        return pool[UnityEngine.Random.Range(0, pool.Length)];
                    }
                case EquipmentSlot.Armor:
                    {
                        int[] pool = { 1, 4, 5 }; // HP%, DEF%, DMG Reduction
                        return pool[UnityEngine.Random.Range(0, pool.Length)];
                    }
                case EquipmentSlot.Helm:
                    {
                        int[] pool = { 1, 4, 6 }; // HP%, DEF%, Evasion
                        return pool[UnityEngine.Random.Range(0, pool.Length)];
                    }
                case EquipmentSlot.Boots:
                    {
                        int[] pool = { 0, 1, 4, 6 }; // ATK%, HP%, DEF%, Evasion
                        return pool[UnityEngine.Random.Range(0, pool.Length)];
                    }
                case EquipmentSlot.Ring1:
                case EquipmentSlot.Ring2:
                    return UnityEngine.Random.Range(0, 8); // All substats possible
                default:
                    return 0;
            }
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

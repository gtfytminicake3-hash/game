using System;
using UnityEngine;

namespace LegendOfBlood
{
    public enum EquipmentSlot
    {
        Weapon,
        Armor,
        Accessory
    }

    [Serializable]
    public class EquipmentData
    {
        public string id;
        public string equipmentName;
        public EquipmentSlot slot;
        public int level;
        
        public int currentExp;
        public int rarity; // 1-5 sao/màu sắc (VD: để scale sức mạnh cơ bản)

        // Base & Bonus Stats
        public float hpBonus;
        public float atkBonus;
        public float defBonus;
        public float spdBonus;
        
        // Multipliers (Percentage bonus e.g., 0.1 for 10% increase)
        public float hpMultiplier;
        public float atkMultiplier;
        public float defMultiplier;
        public float spdMultiplier;
        
        public float critChanceBonus;
        public float critDamageBonus;
        
        // Lưu trữ thông tin bonus để hiển thị UI
        public string bonusStat1Description;
        public string bonusStat2Description;

        public EquipmentData() { }

        public EquipmentData(string id, string name, EquipmentSlot slot, int initLevel = 1)
        {
            this.id = id;
            this.equipmentName = name;
            this.slot = slot;
            this.level = initLevel;
            this.currentExp = 0;
            this.rarity = 1;
        }

        public EquipmentData Clone()
        {
            return (EquipmentData)this.MemberwiseClone();
        }
    }
}

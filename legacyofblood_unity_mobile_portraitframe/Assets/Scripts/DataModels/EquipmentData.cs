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
        
        // Bonus stats
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

        public EquipmentData() { }

        public EquipmentData(string id, string name, EquipmentSlot slot)
        {
            this.id = id;
            this.equipmentName = name;
            this.slot = slot;
            this.level = 1;
        }

        public EquipmentData Clone()
        {
            return (EquipmentData)this.MemberwiseClone();
        }
    }
}

using System;
using UnityEngine;

namespace LegendOfBlood
{
    public enum EquipmentSlot
    {
        Weapon,
        Armor,
        Helm,
        Boots,
        Ring1,
        Ring2
    }

    public enum EquipmentTier
    {
        D, C, B, A, S, SS, SSS
    }

    [Serializable]
    public class EquipmentData
    {
        public string id;
        public string equipmentName;
        public EquipmentSlot slot;
        public int level;
        
        public int currentExp;
        public EquipmentTier tier; // D, C, B, A, S, SS, SSS
        public Profession classRestriction; // Nghề nghiệp bị giới hạn. Nếu là Nhẫn thì mặc định là None.
        public bool isLocked; // Cờ khóa trang bị, chống cắn nhầm

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
        
        // Advanced Stats
        public float evasionBonus;
        public float damageReductionBonus;
        public float damageIncreaseBonus;
        
        // Lưu trữ thông tin bonus để hiển thị UI
        public string bonusStat1Description;
        public string bonusStat2Description;

        public EquipmentData() { }

        public EquipmentData(string id, string name, EquipmentSlot slot, int initLevel = 1, EquipmentTier initTier = EquipmentTier.D, Profession restriction = Profession.None)
        {
            this.id = id;
            this.equipmentName = name;
            this.slot = slot;
            this.level = initLevel;
            this.currentExp = 0;
            this.tier = initTier;
            this.classRestriction = restriction;
            this.isLocked = false;
        }

        public EquipmentData Clone()
        {
            EquipmentData clone = new EquipmentData();
            clone.id = this.id;
            clone.equipmentName = this.equipmentName;
            clone.slot = this.slot;
            clone.level = this.level;
            clone.currentExp = this.currentExp;
            clone.tier = this.tier;
            clone.classRestriction = this.classRestriction;
            clone.isLocked = this.isLocked;

            clone.hpBonus = this.hpBonus;
            clone.atkBonus = this.atkBonus;
            clone.defBonus = this.defBonus;
            clone.spdBonus = this.spdBonus;
            
            clone.hpMultiplier = this.hpMultiplier;
            clone.atkMultiplier = this.atkMultiplier;
            clone.defMultiplier = this.defMultiplier;
            clone.spdMultiplier = this.spdMultiplier;
            
            clone.critChanceBonus = this.critChanceBonus;
            clone.critDamageBonus = this.critDamageBonus;
            
            clone.evasionBonus = this.evasionBonus;
            clone.damageReductionBonus = this.damageReductionBonus;
            clone.damageIncreaseBonus = this.damageIncreaseBonus;
            clone.bonusStat1Description = this.bonusStat1Description;
            clone.bonusStat2Description = this.bonusStat2Description;

            return clone;
        }

        public UnityEngine.Sprite GetIcon()
        {
            // Định dạng: {Profession}_{Slot}_{Tier}
            // Chú ý với Nhẫn Ring1, Ring2 ta gom chung thành Ring để Artist đỡ nhọc
            string slotString = slot.ToString();
            if (slot == EquipmentSlot.Ring1 || slot == EquipmentSlot.Ring2) slotString = "Ring";
            
            string path = $"Icons/Equipments/{classRestriction}_{slotString}_{tier}";
            var sprite = UnityEngine.Resources.Load<UnityEngine.Sprite>(path);
            
            if (sprite == null) 
            {
                // Fallback icon để tránh lỗi hiển thị trắng tinh nếu thiếu ảnh
                // UnityEngine.Debug.LogWarning($"[EquipmentData] Thiếu icon tại path: {path}");
                return UnityEngine.Resources.Load<UnityEngine.Sprite>("Icons/Equipments/FallbackIcon");
            }
            return sprite;
        }
    }
}

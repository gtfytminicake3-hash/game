namespace LegendOfBlood
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    // Các enum này được dùng chung nên đặt bên ngoài lớp HeroData
    [Serializable]
    public class HeroStats
    {
        // --- THAY ĐỔI: Chuyển sang float để tính toán chính xác hơn ---
        public float hp;
        public float atk;
        public float def;
        public float spd;
        public float critChance = 0.05f; // Tỉ lệ chí mạng cơ bản 5%
        public float critDamage = 1.5f;  // Sát thương chí mạng cơ bản 150%
    }

    public enum Gender { Male, Female }
    public enum Profession { None, Warrior, Archer, Mage, Healer }

    /// <summary>
    /// Lớp dữ liệu trung tâm, chứa tất cả thông tin về một Anh hùng.
    /// Đây là định nghĩa DUY NHẤT cho HeroData trong toàn bộ dự án.
    /// </summary>
    [Serializable]
    public class HeroData : ISerializationCallbackReceiver
    {
        #region Core Data Fields
        public string id;
        public string heroName;
        public Gender gender;
        public int avatarIndex; 
        public int level;
        public int experience;
        public int potential;
        public HeroStats baseStats;
        public HeroStats addedStats;
        public int freeStatPoints;
        public float evasionRate;
        public float damageReduction;
        public float damageIncrease;
        
        // --- NEW: Equipment ---
        public Dictionary<EquipmentSlot, EquipmentData> Equipments;
        
        // SỬA LỖI TIỀM TÀNG: Đổi currentHp thành float để khớp với HeroStats.hp
        public float currentHp; 

        public List<string> traitIDs;
        public List<string> skillIDs;
        public Profession profession;
        
        // --- NEW: Breeding Limits ---
        public int breedingCount = 0;
        public int maxBreedingCount = 10;
        #endregion

        #region Status & Timers
        public bool isMature;
        public long maturationEndTime;
        public bool isLightlyInjured;
        public long lightInjuryEndTime;
        public bool isSeverelyInjured;
        public long injuryEndTime;
        #endregion

        public static event Action<HeroData> OnHeroLeveledUp;

        #region Constructors
        
        public HeroData(string heroId, string name, Gender heroGender)
        {
            this.id = heroId;
            this.heroName = name;
            this.gender = heroGender;
            this.level = 1;
            this.baseStats = new HeroStats();
            this.addedStats = new HeroStats();
            this.currentHp = this.baseStats.hp; // Khởi tạo máu đầy
            
            traitIDs = new List<string>();
            skillIDs = new List<string>();
            Equipments = new Dictionary<EquipmentSlot, EquipmentData>();

            if (AvatarManager.Instance != null && AvatarManager.Instance.GetAvatar(this.gender, 0) != null)
            {
                this.avatarIndex = AvatarManager.Instance.GetRandomAvatarIndex(this.gender);
            }
            else
            {
                // In Test environments, AvatarManager might not be fully initialized or instantiated
                this.avatarIndex = -1;
            }
        }

        public HeroData()
        {
            traitIDs = new List<string>();
            skillIDs = new List<string>();
            this.addedStats = new HeroStats();
            Equipments = new Dictionary<EquipmentSlot, EquipmentData>();
        }

        public void CalculateBaseStats()
        {
            if (this.baseStats == null) this.baseStats = new HeroStats();
            this.baseStats.hp = this.potential * UnityEngine.Random.Range(8, 11);
            this.baseStats.atk = this.potential * UnityEngine.Random.Range(8, 11);
            this.baseStats.def = this.potential * UnityEngine.Random.Range(8, 11);
            this.baseStats.spd = this.potential * UnityEngine.Random.Range(8, 11);

            if (this.addedStats == null) this.addedStats = new HeroStats();
            this.freeStatPoints = 0;
            this.currentHp = this.GetFinalStats().hp;
        }

        #endregion

        #region Helper Properties & Methods

        public Sprite GetAvatarSprite()
        {
            if (AvatarManager.Instance != null)
                return AvatarManager.Instance.GetAvatar(this.gender, this.avatarIndex);
            
            Debug.LogError("Attempted to get avatar but AvatarManager does not exist.");
            return null;
        }

        public EquipmentData GetEquipment(EquipmentSlot slot)
        {
            if (Equipments != null && Equipments.TryGetValue(slot, out var eq))
                return eq;
            return null;
        }

        public void EquipItem(EquipmentData newEq)
        {
            if (newEq == null) return;
            if (Equipments == null) Equipments = new Dictionary<EquipmentSlot, EquipmentData>();
            Equipments[newEq.slot] = newEq;
        }

        public void UnequipItem(EquipmentSlot slot)
        {
            if (Equipments != null && Equipments.ContainsKey(slot))
            {
                Equipments.Remove(slot);
            }
        }
        
        public bool IsBusy()
        {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            if (!isMature && maturationEndTime > currentTime) return true;
            if (isLightlyInjured && lightInjuryEndTime > currentTime) return true;
            if (isSeverelyInjured && injuryEndTime > currentTime) return true;

            if (GameManager.Instance != null && GameManager.Instance.ExpeditionManager != null)
            {
                if (GameManager.Instance.ExpeditionManager.IsHeroOnExpedition(id))
                {
                    return true;
                }
            }
            return false;
        }

        public HeroStats GetFinalStats()
        {
            var bs = baseStats ?? new HeroStats();
            var as_ = addedStats ?? new HeroStats();

            var finalStats = new HeroStats
            {
                hp = bs.hp + as_.hp,
                atk = bs.atk + as_.atk,
                def = bs.def + as_.def,
                spd = bs.spd + as_.spd,
                critChance = bs.critChance,
                critDamage = bs.critDamage
            };
            float multiplyHp = 1.0f, multiplyAtk = 1.0f, multiplyDef = 1.0f, multiplySpd = 1.0f;
            
            // --- CỘNG DỒN TRANG BỊ ---
            if (Equipments != null)
            {
                foreach (var eq in Equipments.Values)
                {
                    if (eq == null) continue;
                    // Flat stats
                    finalStats.hp += eq.hpBonus;
                    finalStats.atk += eq.atkBonus;
                    finalStats.def += eq.defBonus;
                    finalStats.spd += eq.spdBonus;
                    finalStats.critChance += eq.critChanceBonus;
                    finalStats.critDamage += eq.critDamageBonus;
                    
                    // Multiplier stats
                    multiplyHp += eq.hpMultiplier;
                    multiplyAtk += eq.atkMultiplier;
                    multiplyDef += eq.defMultiplier;
                    multiplySpd += eq.spdMultiplier;
                }
            }

            // --- CỘNG DỒN TRAIT ---
            if (traitIDs != null)
            {
                foreach (string traitId in traitIDs)
                {
                    Trait trait = DataManager.Instance.GetTraitByID(traitId);
                    if (trait == null || trait.effects == null) continue;
                    foreach (var effect in trait.effects)
                    {
                        if (effect.type == TraitEffectType.ADD_STAT)
                        {
                            finalStats.hp += effect.hp;
                            finalStats.atk += effect.atk;
                            finalStats.def += effect.def;
                            finalStats.spd += effect.spd;
                        }
                        else if (effect.type == TraitEffectType.MULTIPLY_STAT)
                        {
                            multiplyHp += effect.hp / 100f;
                            multiplyAtk += effect.atk / 100f;
                            multiplyDef += effect.def / 100f;
                            multiplySpd += effect.spd / 100f;
                        }
                    }
                }
            }
            finalStats.hp = Mathf.FloorToInt(finalStats.hp * multiplyHp);
            finalStats.atk = Mathf.FloorToInt(finalStats.atk * multiplyAtk);
            finalStats.def = Mathf.FloorToInt(finalStats.def * multiplyDef);
            finalStats.spd = Mathf.FloorToInt(finalStats.spd * multiplySpd);
            return finalStats;
        }

        public int GetCombatPower()
        {
            var finalStats = GetFinalStats();
            float cp = finalStats.hp / 10f + finalStats.atk * 2f + finalStats.def * 3f + finalStats.spd * 1.5f;
            return Mathf.FloorToInt(cp);
        }

        /// <summary>
        /// Creates a shallow copy of this HeroData object.
        /// </summary>
        public HeroData Clone()
        {
            var cloned = (HeroData)this.MemberwiseClone();
            if (this.Equipments != null)
            {
                cloned.Equipments = new Dictionary<EquipmentSlot, EquipmentData>();
                foreach (var eq in this.Equipments)
                {
                    cloned.Equipments[eq.Key] = eq.Value?.Clone();
                }
            }
            return cloned;
        }
        
        public void AddExperience(int amount)
        {
            if (amount <= 0) return;
            experience += amount;
            
            var expTable = DataManager.Instance.ExpTable;
            if (expTable == null) return;

            // Loop in case of multiple level-ups from a large XP gain
            while (expTable.ContainsKey(level) && experience >= expTable[level])
            {
                experience -= expTable[level];
                level++;
                
                // Grant free stat points instead of auto-assigning
                this.freeStatPoints += this.potential;
                
                // Restore HP to full after leveling up
                currentHp = GetFinalStats().hp;
                
                OnHeroLeveledUp?.Invoke(this);
                Debug.Log($"{heroName} leveled up to {level}!");

                // If the hero reaches a level not in the table, stop.
                if (!expTable.ContainsKey(level)) break;
            }
        }
        #endregion

        #region Serialization Callbacks
        public void OnAfterDeserialize()
        {
            traitIDs ??= new List<string>();
            skillIDs ??= new List<string>();
            Equipments ??= new Dictionary<EquipmentSlot, EquipmentData>();
        }

        public void OnBeforeSerialize() { }
        #endregion
    }
}
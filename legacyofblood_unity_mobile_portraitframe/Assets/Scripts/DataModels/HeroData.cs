namespace LegendOfBlood
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    // Các enum này được dùng chung nên đặt bên ngoài lớp HeroData
    [Serializable]
    public class HeroStats
    {
        public int hp;
        public int atk;
        public int def;
        public int spd;
    }

    public enum Gender { Male, Female }
    public enum Profession { None, Warrior, Archer, Mage, Healer }

    /// <summary>
    /// Lớp dữ liệu trung tâm, chứa tất cả thông tin về một Anh hùng.
    /// Kế thừa ISerializationCallbackReceiver để khởi tạo list an toàn khi tải từ JSON.
    /// </summary>
    [Serializable]
    public class HeroData : ISerializationCallbackReceiver
    {
        #region Core Data Fields
        public string id;
        public string heroName;
        public Gender gender;

        [Tooltip("Chỉ số của avatar trong pool của AvatarManager, được gán ngẫu nhiên khi tạo hero.")]
        public int avatarIndex; 

        public int level;
        public int experience;
        public int potential; // Tiềm năng

        public HeroStats baseStats;
        public int currentHp;

        public List<string> traitIDs;
        public List<string> skillIDs;
        public Profession profession;
        #endregion

        #region Status & Timers
        // Trạng thái và Thời gian kết thúc (Unix Millisecond Timestamps)
        public bool isMature;
        public long maturationEndTime;

        public bool isLightlyInjured;
        public long lightInjuryEndTime;

        public bool isSeverelyInjured;
        public long injuryEndTime;
        #endregion

        // Sự kiện được phát ra khi hero này lên cấp để các hệ thống khác lắng nghe
        public static event Action<HeroData> OnHeroLeveledUp;

        #region Constructors
        
        /// <summary>
        /// Constructor để tạo một hero HOÀN TOÀN MỚI.
        /// Tự động gán một avatar ngẫu nhiên và lưu lại index của nó.
        /// </summary>
        /// <param name="heroId">ID duy nhất cho hero.</param>
        /// <param name="name">Tên của hero.</param>
        /// <param name="heroGender">Giới tính của hero.</param>
        public HeroData(string heroId, string name, Gender heroGender)
        {
            // Khởi tạo các giá trị cơ bản
            this.id = heroId;
            this.heroName = name;
            this.gender = heroGender;
            this.level = 1;
            this.baseStats = new HeroStats(); // Khởi tạo với chỉ số ban đầu
            
            // Khởi tạo các list để tránh lỗi
            traitIDs = new List<string>();
            skillIDs = new List<string>();

            // GỌI AVATAR MANAGER ĐỂ LẤY INDEX NGẪU NHIÊN VÀ LƯU LẠI
            if (AvatarManager.Instance != null)
            {
                this.avatarIndex = AvatarManager.Instance.GetRandomAvatarIndex(this.gender);
            }
            else
            {
                Debug.LogError("AvatarManager chưa được khởi tạo! Không thể gán avatar ngẫu nhiên.");
                this.avatarIndex = -1; // -1 biểu thị lỗi hoặc chưa được gán
            }
        }

        /// <summary>
        /// Constructor mặc định. Rất QUAN TRỌNG cho việc Deserialization (tải dữ liệu từ JSON/file).
        /// Không nên dùng để tạo hero mới.
        /// </summary>
        public HeroData()
        {
            // Chỉ khởi tạo các list để đảm bảo chúng không bao giờ null sau khi tải
            traitIDs = new List<string>();
            skillIDs = new List<string>();
        }

        #endregion

        #region Helper Properties & Methods

        /// <summary>
        /// Lấy đối tượng Sprite của avatar hero này từ AvatarManager.
        /// Đây là cầu nối giữa dữ liệu (avatarIndex) và asset hình ảnh (Sprite).
        /// </summary>
        /// <returns>Sprite avatar tương ứng.</returns>
        public Sprite GetAvatarSprite()
        {
            if (AvatarManager.Instance != null)
            {
                return AvatarManager.Instance.GetAvatar(this.gender, this.avatarIndex);
            }
            
            Debug.LogError("Cố gắng lấy avatar nhưng AvatarManager không tồn tại.");
            return null; // Trả về null nếu manager không tồn tại để tránh lỗi
        }
        
        /// <summary>
        /// Kiểm tra xem hero có đang trong bất kỳ trạng thái bận nào không.
        /// (Chưa trưởng thành, bị thương, đang đi thám hiểm).
        /// </summary>
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

        /// <summary>
        /// Tính toán chỉ số cuối cùng sau khi áp dụng hiệu ứng từ Trait.
        /// </summary>
        public HeroStats GetFinalStats()
        {
            var finalStats = new HeroStats
            {
                hp = baseStats.hp,
                atk = baseStats.atk,
                def = baseStats.def,
                spd = baseStats.spd
            };
            float multiplyHp = 1.0f, multiplyAtk = 1.0f, multiplyDef = 1.0f, multiplySpd = 1.0f;
            foreach (string traitId in traitIDs)
            {
                Trait trait = DataManager.Instance.GetTraitByID(traitId);
                if (trait == null) continue;
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
            finalStats.hp = Mathf.FloorToInt(finalStats.hp * multiplyHp);
            finalStats.atk = Mathf.FloorToInt(finalStats.atk * multiplyAtk);
            finalStats.def = Mathf.FloorToInt(finalStats.def * multiplyDef);
            finalStats.spd = Mathf.FloorToInt(finalStats.spd * multiplySpd);
            return finalStats;
        }

        /// <summary>
        /// Tính toán Sức mạnh Chiến đấu (CP) của hero.
        /// </summary>
        public int GetCombatPower()
        {
            var finalStats = GetFinalStats();
            float cp = finalStats.hp / 10f + finalStats.atk * 2f + finalStats.def * 3f + finalStats.spd * 1.5f;
            return Mathf.FloorToInt(cp);
        }
        
        /// <summary>
        /// Cho hero nhận kinh nghiệm và xử lý lên cấp.
        /// </summary>
        public void GainExp(int amount)
        {
            experience += amount;
            var expTable = DataManager.Instance.ExpTable;
            if (expTable == null || !expTable.ContainsKey(level)) return;
            while (experience >= expTable[level])
            {
                experience -= expTable[level];
                level++;
                int distributionPoints = Mathf.FloorToInt(potential / 2f);
                baseStats.hp += Mathf.FloorToInt(distributionPoints * 1.5f);
                baseStats.atk += distributionPoints;
                baseStats.def += distributionPoints;
                currentHp = GetFinalStats().hp;
                OnHeroLeveledUp?.Invoke(this);
                Debug.Log($"{heroName} đã lên cấp {level}!");
                if (!expTable.ContainsKey(level)) break;
            }
        }
        #endregion

        #region Serialization Callbacks
        // Được gọi sau khi Unity tải dữ liệu từ JSON.
        // Dùng để đảm bảo các list không bao giờ bị null.
        public void OnAfterDeserialize()
        {
            traitIDs ??= new List<string>();
            skillIDs ??= new List<string>();
        }

        public void OnBeforeSerialize() { } // Không cần làm gì trước khi lưu
        #endregion
    }
}
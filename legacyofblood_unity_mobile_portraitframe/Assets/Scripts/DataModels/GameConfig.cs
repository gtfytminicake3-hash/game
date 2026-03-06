// --- START OF FILE GameConfig.cs (FIXED AGAIN) ---

using UnityEngine;
using System.Collections.Generic;
using LegendOfBlood; // Namespace của Trait, Skill
// using LegendOfBlood.DataModels; // SỬA LỖI: XÓA DÒNG NÀY

// Namespace này là đúng cho GameConfig và các file cấu hình khác
namespace LegendOfBlood.GameConfigs
{
    // Các lớp dữ liệu này chỉ được sử dụng bên trong GameConfig, nên có thể để ở đây.
    [System.Serializable]
    public class EvolutionRewardData
    {
        public int level;
        public string description;
        // Có thể thêm các loại phần thưởng khác ở đây, ví dụ:
        // public int freeStatPoints;
        // public string guaranteedTraitId;
    }

    [System.Serializable]
    public class EvolutionTableData
    {
        public List<EvolutionRewardData> rewards;
    }

    [System.Serializable]
    public class ExperienceData
    {
        public int level;
        public int experienceRequired;
    }

    [System.Serializable]
    public class ProfessionStartingSkills
    {
        public Profession profession; // Giả sử enum Profession được định nghĩa ở đâu đó (ví dụ: trong file HeroData.cs)
        public List<string> startingSkillIDs;
    }

    [System.Serializable]
    public class RarityConfig
    {
        public Trait.RarityRank rank;
        [Range(0f, 100f)] public float dropChance;
        public int minPotential;
        public int maxPotential;
    }

    [System.Serializable]
    public class CombatConfig
    {
        [Tooltip("Hệ số crit phụ thêm cho nghề Archer cơ bản")]
        public float archerBonusCritChance = 0.4f;
        
        [Tooltip("Hệ số sát thương của hiệu ứng Poison dựa trên ATK")]
        public float poisonDamageRatio = 0.2f;
        
        [Tooltip("Giá trị giảm thuộc tính Speed của hiệu ứng Slow")]
        public float slowSpeedReduction = -20f;
        
        [Tooltip("Giá trị tăng thuộc tính Crit khi nhận hiệu ứng CritUp")]
        public float critUpBonus = 0.15f;
        
        [Tooltip("Hệ số giảm thuộc tính Def (dựa trên % DEF gốc) của hiệu ứng DefDown")]
        public float defDownRatio = -0.15f;
        
        [Tooltip("Hệ số hồi phục của hiệu ứng HealOverTime dựa trên ATK")]
        public float healOverTimeRatio = 0.5f;

        // Có thể thêm default cooldown ticket, default action wait,... nếu cần
    }

    [System.Serializable]
    public class TowerFloorConfig
    {
        public int floorIndex;
        public List<string> monsterPool;
        
        [Tooltip("Sử dụng khi random sinh quái. Số lượng = 1 + (floor/5) nếu không override ở đây, hoặc tùy chỉnh lượng quái riêng rẽ theo tầng.")]
        public int customMonsterCount = 0; 
    }

    /// <summary>
    /// ScriptableObject trung tâm chứa tất cả dữ liệu cấu hình của game.
    /// Kéo file asset GameConfig vào DataManager trong Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "LegendOfBlood/Game Configuration", order = 1)]
    public class GameConfig : ScriptableObject
    {
        [Header("Hero & Progression")]
        [Tooltip("Danh sách tất cả các Trait có trong game.")]
        public List<Trait> AllTraits;

        [Tooltip("Danh sách tất cả các Skill có trong game.")]
        public List<Skill> AllSkills;

        [Tooltip("Bảng kinh nghiệm yêu cầu cho mỗi cấp độ.")]
        public List<ExperienceData> ExperienceTable;

        [Tooltip("Cấu hình tiến hóa của hero.")]
        public EvolutionTableData EvolutionTable;

        [Tooltip("Kỹ năng khởi đầu cho từng nghề nghiệp.")]
        public List<ProfessionStartingSkills> StartingSkills;


        [Header("Combat & World")]
        [Tooltip("Cấu hình quái vật cho các điểm quan tâm (POI).")]
        public POIMonsterConfig POIMonsterConfig;

        [Tooltip("Danh sách tất cả các Boss có trong game.")]
        public List<BossData> AllBosses;

        [Tooltip("Danh sách tất cả các Vật phẩm có trong game.")]
        public List<ItemData> AllItems;

        [Header("Global Settings & Balancing")]
        [Tooltip("Cấu hình sức mạnh, tỉ lệ drop cho từng loại Rarity")]
        public List<RarityConfig> RaritySettings;

        [Tooltip("Cấu hình chỉ số hiệu ứng tự động trong CombatSystem")]
        public CombatConfig CombatSettings = new CombatConfig();

        [Header("Game Systems")]
        [Tooltip("Danh sách tất cả các file dữ liệu nâng cấp cho các tòa nhà.")]
        public List<BuildingUpgradeData> BuildingUpgradeDataList;

        [Tooltip("Danh sách tất cả các Quest có trong game.")]
        public List<QuestData> AllQuestData;

        [Tooltip("Cấu hình danh sách quái cho các tầng tháp thử thách.")]
        public List<TowerFloorConfig> TowerConfigs;
    }
}
// --- END OF FILE GameConfig.cs (FIXED AGAIN) ---
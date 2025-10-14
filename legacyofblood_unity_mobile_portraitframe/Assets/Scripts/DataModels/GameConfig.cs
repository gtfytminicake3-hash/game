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

        [Header("Game Systems")]
        [Tooltip("Danh sách tất cả các file dữ liệu nâng cấp cho các tòa nhà.")]
        public List<BuildingUpgradeData> BuildingUpgradeDataList;

        [Tooltip("Danh sách tất cả các Quest có trong game.")]
        public List<QuestData> AllQuestData;
    }
}
// --- END OF FILE GameConfig.cs (FIXED AGAIN) ---
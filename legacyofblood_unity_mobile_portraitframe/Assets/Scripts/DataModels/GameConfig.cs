namespace LegendOfBlood
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// ScriptableObject trung tâm, chứa tham chiếu đến tất cả các
    /// dữ liệu cấu hình tĩnh của game như Traits, Skills, Bosses, bảng EXP, v.v.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "LegendOfBlood/Game Configuration", order = 0)]
    public class GameConfig : ScriptableObject
    {
        [Header("Hero Data")]
        [Tooltip("Danh sách tất cả các Trait có trong game. Kéo các asset Trait vào đây.")]
        public List<Trait> AllTraits;

        [Tooltip("Danh sách tất cả các Skill có trong game. Kéo các asset Skill vào đây.")]
        public List<Skill> AllSkills;
        
        [Tooltip("Bảng kinh nghiệm yêu cầu cho mỗi cấp độ.")]
        public List<LevelExperience> ExperienceTable;

        [Header("Enemy & World Data")]
        [Tooltip("Danh sách tất cả các Boss có trong game.")]
        public List<BossData> AllBosses;

        [Tooltip("Danh sách tất cả các Vật phẩm có trong game.")]
        public List<ItemData> AllItems;
        
        // --- Thêm các cấu hình khác của game ở đây ---
        // Ví dụ:
        // public EvolutionData EvolutionTable;
        // public BuildingData BuildingTable;
    }
    
    /// <summary>
    /// Lớp con để tạo một giao diện chỉnh sửa bảng EXP thân thiện trong Inspector.
    /// </summary>
    [System.Serializable]
    public class LevelExperience
    {
        public int level;
        public int experienceRequired;
    }
}
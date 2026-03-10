// --- START OF FILE MonsterData.cs ---
using UnityEngine;

namespace LegendOfBlood
{
    /// <summary>
    /// Lưu trữ chỉ số cơ bản (Base Stats) cho quái vật thông thường.
    /// Khác với BossData, MonsterData tối giản hơn và sẽ được nhân tỉ lệ theo độ khó (Difficulty) của POI.
    /// </summary>
    [CreateAssetMenu(fileName = "NewMonster", menuName = "LegendOfBlood/Monster Data", order = 4)]
    public class MonsterData : ScriptableObject
    {
        [Header("Basic Information")]
        [Tooltip("ID duy nhất của Quái, ví dụ: 'Goblin', 'Slime'. Phải khớp với chuỗi trong POIMonsterConfig.")]
        public string id;

        [Tooltip("Tên Việt Hóa, ví dụ: 'Yêu Tinh Rừng'.")]
        public string monsterName;

        [Tooltip("Nghề nghiệp cơ bản (quyết định thuật toán lấy skill cơ bản nếu có).")]
        public Profession profession = Profession.Warrior;

        [Header("Base Combat Stats (Level 1)")]
        [Tooltip("Máu cơ bản.")]
        public float baseHp = 150f;
        [Tooltip("Tấn công cơ bản.")]
        public float baseAtk = 15f;
        [Tooltip("Phòng thủ cơ bản.")]
        public float baseDef = 10f;
        [Tooltip("Tốc độ cơ bản.")]
        public float baseSpd = 12f;
        
        [Tooltip("Tỉ lệ chí mạng cơ bản (0.05 = 5%).")]
        public float critChance = 0.05f;
        [Tooltip("Sát thương chí mạng cơ bản (1.5 = 150%).")]
        public float critDamage = 1.5f;

        [ContextMenu("Auto-fill ID from Name")]
        private void AutofillID()
        {
            this.id = this.name;
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                id = this.name;
            }
        }
    }
}
// --- END OF FILE MonsterData.cs ---

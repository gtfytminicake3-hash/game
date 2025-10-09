namespace LegendOfBlood
{
    using UnityEngine;

    /// <summary>
    /// Các loại kỹ năng trong game.
    /// </summary>
    public enum SkillType
    {
        Active,      // Kỹ năng chủ động (cần sử dụng)
        Passive      // Kỹ năng bị động (tự có hiệu lực)
    }

    /// <summary>
    /// Định nghĩa ScriptableObject cho một Skill (Kỹ năng).
    /// </summary>
    [CreateAssetMenu(fileName = "NewSkill", menuName = "LegendOfBlood/Skill", order = 2)]
    public class Skill : ScriptableObject
    {
        [Header("Basic Information")]
        [Tooltip("ID duy nhất của Skill, ví dụ: 'SK_WARRIOR_01'. Không được trùng.")]
        public string id;

        [Tooltip("Tên của Skill sẽ hiển thị trong game.")]
        public string skillName;

        [Tooltip("Mô tả chi tiết về Skill, sẽ hiển thị cho người chơi.")]
        [TextArea(3, 5)]
        public string description;
        
        [Header("Skill Properties")]
        [Tooltip("Loại kỹ năng: Chủ động hay Bị động.")]
        public SkillType type;
        
        [Tooltip("Nghề nghiệp có thể học kỹ năng này.")]
        public Profession requiredProfession;

        // Có thể mở rộng thêm rất nhiều thuộc tính khác ở đây
        // Ví dụ:
        // public int manaCost;
        // public float cooldown;
        // public SkillEffect skillEffect; // Một lớp chứa hiệu ứng thực tế của skill
        // public AnimationClip animation;
        // public GameObject vfxPrefab;

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
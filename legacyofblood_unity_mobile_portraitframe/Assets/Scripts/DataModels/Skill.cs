// --- START OF FILE Skill.cs (FIXED) ---

using UnityEngine;

namespace LegendOfBlood
{
    // Các enum này nên được đặt ở một nơi chung để Skill.cs và CombatSystem.cs đều có thể truy cập
    public enum HeroClass { Warrior, Archer, Mage, Healer }
    public enum SkillType { Active, Passive }
    public enum TargetingType
    {
        SingleFrontEnemy, LowestHpAlly, AllEnemies, Self,
        AdjacentEnemies, RandomEnemy, AllAlliesInRow, AllAllies
    }
    public enum StatusEffectType { None, Poison, Slow, CritUp, DefDown, HealOverTime, Shield }


    /// <summary>
    /// Định nghĩa ScriptableObject cho một Skill (Kỹ năng).
    /// </summary>
    [CreateAssetMenu(fileName = "NewSkill", menuName = "LegendOfBlood/Skill", order = 2)]
    public class Skill : ScriptableObject
    {
        [Header("Thông tin cơ bản")]
        [Tooltip("ID duy nhất của Skill, ví dụ: 'SK_WARRIOR_01'. Không được trùng.")]
        public string id;

        [Tooltip("Tên của Skill sẽ hiển thị trong game.")]
        public string skillName;

        [Tooltip("Mô tả chi tiết về Skill, sẽ hiển thị cho người chơi.")]
        [TextArea(3, 5)]
        public string description;

        [Header("Thuộc tính Kỹ năng")]
        [Tooltip("Loại kỹ năng: Chủ động hay Bị động.")]
        public SkillType type;

        [Tooltip("Nghề nghiệp có thể học kỹ năng này.")]
        public HeroClass requiredProfession; // Đổi tên 'Profession' thành 'HeroClass' cho nhất quán

        // --- PHẦN BỔ SUNG CHO LOGIC CHIẾN ĐẤU ---
        [Header("Logic Chiến Đấu")]
        [Tooltip("Số lượt cần chờ để có thể tái sử dụng kỹ năng.")]
        public int cooldown;

        [Tooltip("Cách kỹ năng này chọn mục tiêu.")]
        public TargetingType targeting;

        [Tooltip("Hệ số sức mạnh. Ví dụ: 2.5 nghĩa là 250% ATK hoặc DEF.")]
        public float powerRatio;

        [Tooltip("Số lần kỹ năng tấn công (ví dụ: Bão Sét đánh 4 lần).")]
        public int hitCount = 1;

        [Header("Hiệu ứng Trạng thái")]
        [Tooltip("Hiệu ứng mà kỹ năng này áp dụng lên mục tiêu.")]
        public StatusEffectType appliedEffect;

        [Tooltip("Số lượt hiệu ứng tồn tại.")]
        [Range(0, 10)]
        public int effectDuration;

        [Tooltip("Tỉ lệ áp dụng hiệu ứng thành công (0 = 0%, 1 = 100%).")]
        [Range(0, 1)]
        public float effectChance;
        // ---------------------------------------------

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
// --- END OF FILE Skill.cs (FIXED) ---
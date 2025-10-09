namespace LegendOfBlood
{
    using UnityEngine;

    /// <summary>
    /// Quản lý hệ thống tiến hóa nghề nghiệp của hero.
    /// Hệ thống này lắng nghe sự kiện OnHeroLeveledUp để tự động kích hoạt.
    /// </summary>
    public class EvolutionSystem
    {
        /// <summary>
        /// Constructor của hệ thống. Ngay khi được tạo, nó sẽ đăng ký lắng nghe sự kiện.
        /// </summary>
        public EvolutionSystem()
        {
            // Đăng ký phương thức HandleHeroLeveledUp vào sự kiện OnHeroLeveledUp của lớp HeroData.
            // Bất cứ khi nào sự kiện này được phát ra từ bất kỳ hero nào, phương thức này sẽ được gọi.
            HeroData.OnHeroLeveledUp += HandleHeroLeveledUp;
            Debug.Log("EvolutionSystem đã được khởi tạo và đang lắng nghe sự kiện lên cấp.");
        }

        /// <summary>
        /// Destructor (Hàm hủy). Cần thiết để hủy đăng ký sự kiện khi đối tượng bị dọn dẹp,
        /// tránh rò rỉ bộ nhớ (memory leaks).
        /// </summary>
        ~EvolutionSystem()
        {
            HeroData.OnHeroLeveledUp -= HandleHeroLeveledUp;
        }

        /// <summary>
        /// Phương thức được gọi mỗi khi một hero lên cấp.
        /// </summary>
        /// <param name="hero">Hero vừa lên cấp</param>
        private void HandleHeroLeveledUp(HeroData hero)
        {
            // Kiểm tra các mốc tiến hóa dựa trên GDD_02_Progression_And_Status.md
            switch (hero.level)
            {
                case 30:
                    GrantEvolutionReward(hero, 30);
                    break;
                case 50:
                    GrantEvolutionReward(hero, 50);
                    break;
                case 70:
                    GrantEvolutionReward(hero, 70);
                    break;
                case 100:
                    GrantEvolutionReward(hero, 100);
                    break;
                // Không phải mốc tiến hóa, không làm gì cả
                default:
                    return;
            }
        }

        /// <summary>
        /// Lấy phần thưởng từ DataManager và gán cho hero.
        /// </summary>
        private void GrantEvolutionReward(HeroData hero, int levelMilestone)
        {
            // Giả định rằng GameConfig sẽ có một cấu trúc dữ liệu để lưu phần thưởng tiến hóa
            // và DataManager có một phương thức để truy xuất nó.
            // Ví dụ: EvolutionReward reward = DataManager.Instance.GetEvolutionRewardFor(hero.profession, levelMilestone);
            
            // --- PHẦN GIẢ LẬP DỮ LIỆU PHẦN THƯỞNG (sẽ được thay bằng GameConfig thật) ---
            string rewardId = GetPlaceholderRewardId(hero.profession, levelMilestone);
            // --- KẾT THÚC PHẦN GIẢ LẬP ---

            if (string.IsNullOrEmpty(rewardId))
            {
                Debug.LogWarning($"Không tìm thấy phần thưởng tiến hóa cho nghề {hero.profession} ở cấp {levelMilestone}.");
                return;
            }

            // Phân loại phần thưởng dựa trên ID (ví dụ: SK_ là Skill, TR_ là Trait)
            if (rewardId.StartsWith("SK_")) // Đây là một Skill
            {
                if (!hero.skillIDs.Contains(rewardId))
                {
                    hero.skillIDs.Add(rewardId);
                    // Lấy tên Skill từ DataManager để log cho đẹp
                    string skillName = DataManager.Instance.GetSkillByID(rewardId)?.skillName ?? rewardId;
                    Debug.Log($"<color=cyan>Tiến Hóa!</color> {hero.heroName} đã học được kỹ năng mới: [{skillName}] ở cấp {levelMilestone}.");
                }
            }
            else if (rewardId.StartsWith("TR_")) // Đây là một Trait
            {
                if (!hero.traitIDs.Contains(rewardId))
                {
                    hero.traitIDs.Add(rewardId);
                    string traitName = DataManager.Instance.GetTraitByID(rewardId)?.traitName ?? rewardId;
                    Debug.Log($"<color=cyan>Tiến Hóa!</color> {hero.heroName} đã nhận được đặc tính mới: [{traitName}] ở cấp {levelMilestone}.");
                }
            }
        }

        /// <summary>
        /// HÀM GIẢ LẬP: Thay thế bằng logic lấy dữ liệu từ GameConfig/DataManager.
        /// </summary>
        private string GetPlaceholderRewardId(Profession profession, int level)
        {
            switch (profession)
            {
                case Profession.Warrior:
                    if (level == 30) return "SK_WARRIOR_02";
                    if (level == 50) return "TR_WARRIOR_PASSIVE";
                    break;
                case Profession.Archer:
                    if (level == 30) return "SK_ARCHER_02";
                    break;
            }
            return null;
        }
    }
}
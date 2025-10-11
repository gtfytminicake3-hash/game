namespace LegendOfBlood
{
    using System.Linq;
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
            // Thay vì kiểm tra các mốc cố định, giờ chúng ta sẽ kiểm tra xem có phần thưởng nào ở cấp độ hiện tại không.
            GrantEvolutionReward(hero);
        }

        /// <summary>
        /// Lấy phần thưởng từ DataManager và gán cho hero.
        /// </summary>
        private void GrantEvolutionReward(HeroData hero)
        {
            // Lấy dữ liệu phần thưởng từ DataManager thay vì dùng hàm giả lập
            var reward = DataManager.Instance.EvolutionRewards.FirstOrDefault(r => 
                r.profession == hero.profession && r.requiredLevel == hero.level
            );

            // Nếu tìm thấy phần thưởng cho cấp độ và nghề nghiệp này
            if (reward != null)
            {
                string rewardId = reward.rewardID;

                if (string.IsNullOrEmpty(rewardId))
                {
                    Debug.LogWarning($"Tìm thấy phần thưởng tiến hóa cho nghề {hero.profession} ở cấp {hero.level}, nhưng rewardID rỗng.");
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
                        Debug.Log($"<color=cyan>Tiến Hóa!</color> {hero.heroName} đã học được kỹ năng mới: [{skillName}] ở cấp {hero.level}.");
                    }
                }
                else if (rewardId.StartsWith("TR_")) // Đây là một Trait
                {
                    if (!hero.traitIDs.Contains(rewardId))
                    {
                        hero.traitIDs.Add(rewardId);
                        string traitName = DataManager.Instance.GetTraitByID(rewardId)?.traitName ?? rewardId;
                        Debug.Log($"<color=cyan>Tiến Hóa!</color> {hero.heroName} đã nhận được đặc tính mới: [{traitName}] ở cấp {hero.level}.");
                    }
                }
            }
        }
    }
}
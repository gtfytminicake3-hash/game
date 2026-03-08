namespace LegendOfBlood
{
    using System;
    using System.Linq; // Cần thiết cho Enum.GetValues
    using UnityEngine;

    /// <summary>
    /// Quản lý quá trình trưởng thành của các hero sơ sinh.
    /// Hệ thống này được "tick" bởi GameManager.
    /// </summary>
    public class MaturationSystem
    {
        // Event để thông báo cho UI hoặc các hệ thống khác khi một hero đã trưởng thành.
        public static event Action<HeroData> OnHeroMatured;

        /// <summary>
        /// Rút ngắn thời gian trưởng thành bằng vật phẩm.
        /// </summary>
        public bool SpeedUpMaturation(HeroData hero, string itemId)
        {
            if (hero == null || hero.isMature) return false;

            var item = DataManager.Instance.AllItems.TryGetValue(itemId, out var itemData) ? itemData : null;
            if (item == null || item.type != ItemType.SpeedUp) return false;

            bool success = GameManager.Instance.InventoryManager.UseItem(itemId, 1);
            if (success)
            {
                long speedUpMs = item.speedUpValueInSeconds * 1000;
                hero.maturationEndTime -= speedUpMs;
                Debug.Log($"{hero.heroName} đã sử dụng {item.itemName} để giảm {item.speedUpValueInSeconds}s thời gian trưởng thành.");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Hàm tick được gọi bởi GameManager mỗi frame để cập nhật logic.
        /// </summary>
        public void Tick(float deltaTime)
        {
            var allHeroes = DataManager.Instance.AllHeroes;
            if (allHeroes == null || allHeroes.Count == 0) return;

            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Sử dụng ToList() để tạo một bản sao, tránh lỗi khi thay đổi collection đang duyệt.
            foreach (var hero in allHeroes.ToList())
            {
                // Chỉ xử lý những hero chưa trưởng thành và đã đến giờ
                if (!hero.isMature && currentTime >= hero.maturationEndTime)
                {
                    MatureHero(hero);
                }
            }
        }

        /// <summary>
        /// Thực hiện quá trình "Thức tỉnh" cho một hero.
        /// Dựa trên GDD_02_Progression_And_Status.md.
        /// </summary>
        private void MatureHero(HeroData hero)
        {
            hero.isMature = true;

            // 1. Gán ngẫu nhiên 1 trong 4 nghề nghiệp
            var professions = Enum.GetValues(typeof(Profession)).Cast<Profession>().ToList();
            professions.Remove(Profession.None); // Loại bỏ trạng thái "None"
            hero.profession = professions[UnityEngine.Random.Range(0, professions.Count)];

            // 2. Gán ngẫu nhiên một kỹ năng khởi đầu của nghề đó
            var startingSkills = DataManager.Instance.GetStartingSkills(hero.profession);
            if(startingSkills != null && startingSkills.Count > 0)
            {
                string randomSkillID = startingSkills[UnityEngine.Random.Range(0, startingSkills.Count)];
                hero.skillIDs.Add(randomSkillID);
                Debug.Log($"Hero đã học được kỹ năng khởi đầu: {randomSkillID}");
            }

            Debug.Log($"Hero {hero.heroName} (ID: {hero.id}) đã trưởng thành! Thức tỉnh thành nghề: {hero.profession}.");

            // Phát sự kiện để UI có thể cập nhật
            OnHeroMatured?.Invoke(hero);
        }
    }
}
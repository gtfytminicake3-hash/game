using System;
using System.Collections.Generic;
using UnityEngine;

namespace LegendOfBlood.GameConfigs
{
    [Serializable]
    public class PassRewardItem
    {
        [Tooltip("Loại tài nguyên hoặc ID vật phẩm. Tạm thời hỗ trợ Gold, Wood, Stone, Diamond. Để trống nếu là Item ID.")]
        public ResourceType resourceType;
        
        [Tooltip("Nếu nhận Vật phẩm thì điền ItemID vào đây. Nếu nhận Resource thì để trống.")]
        public string itemID;

        [Tooltip("Số lượng quà nhận được.")]
        public int amount;
    }

    [Serializable]
    public class PassLevelData
    {
        [Tooltip("Cấp độ Pass")]
        public int level;
        
        [Tooltip("Lượng EXP cần để đạt cấp độ này. VD: Cấp 2 cần tổng 150 EXP kể từ cấp 1")]
        public int requiredExp;
        
        [Tooltip("Phần thưởng miễn phí (Free Pass)")]
        public PassRewardItem freeReward;
        
        [Tooltip("Phần thưởng đặc biệt (Premium Pass)")]
        public PassRewardItem premiumReward;
    }

    [CreateAssetMenu(fileName = "KingGodPassConfig", menuName = "LegendOfBlood/King God Pass Config", order = 2)]
    public class KingGodPassConfig : ScriptableObject
    {
        [Header("Danh Sách Phần Thưởng Pass")]
        public List<PassLevelData> levels = new List<PassLevelData>();

        /// <summary>
        /// Lấy lượng Max Exp cần thiết cho một cấp độ cụ thể.
        /// </summary>
        public int GetRequiredExpForLevel(int level)
        {
            var match = levels.Find(x => x.level == level);
            if (match != null)
            {
                return match.requiredExp;
            }
            // Mặc định an toàn
            return 100 + level * 50;
        }

        /// <summary>
        /// Lấy Data của một Level.
        /// </summary>
        public PassLevelData GetLevelData(int level)
        {
            return levels.Find(x => x.level == level);
        }

        /// <summary>
        /// Lấy cấp độ Pass tối đa (Phần thưởng cuối cùng)
        /// </summary>
        public int GetMaxLevel()
        {
            if (levels == null || levels.Count == 0) return 1;
            return levels[levels.Count - 1].level;
        }
    }
}

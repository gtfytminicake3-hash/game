using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace LegendOfBlood.UI
{
    public class KingGodPassPanel : UIPanel
    {
        [Header("References")]
        public Transform contentTransform;
        public GameObject rowPrefab;

        [Header("Premium Pass Upgrade")]
        public Button upgradePremiumBtn;
        public GameObject premiumActiveTag; // Hiện khi đã mua vip

        private List<PassRewardRowUI> _rows = new List<PassRewardRowUI>();

        private void Awake()
        {
            
            // Xóa rác mockup (Mặc định khi clone panel)
            foreach (Transform child in contentTransform)
            {
                Destroy(child.gameObject);
            }

            if (upgradePremiumBtn != null)
            {
                upgradePremiumBtn.onClick.AddListener(OnUpgradePremiumClicked);
            }
        }

        private void OnEnable()
        {
            InventoryManager.OnPassExpChanged += RefreshUI;
            RefreshUI(0, 0); // Kích hoạt bằng tay
        }

        private void OnDisable()
        {
            InventoryManager.OnPassExpChanged -= RefreshUI;
        }

        private void RefreshUI(int currentLevel, int currentExp)
        {
            var config = GameManager.Instance.DataManager.GameConfig?.KingGodPassConfig;
            var player = GameManager.Instance.DataManager.Player;

            if (config == null || player == null) return;

            // Xử lý nút mua Premium
            bool hasPremium = player.isPremiumPassUnlocked;
            if (upgradePremiumBtn != null) upgradePremiumBtn.gameObject.SetActive(!hasPremium);
            if (premiumActiveTag != null) premiumActiveTag.SetActive(hasPremium);

            // Xử lý tạo hoặc làm mới các dòng Row
            // Để tối ưu, nếu _rows chưa đủ số lượng thì ta phải Instantiate thêm
            int targetCount = config.levels.Count;
            while (_rows.Count < targetCount)
            {
                GameObject obj = Instantiate(rowPrefab, contentTransform);
                obj.SetActive(true);
                var rowUi = obj.GetComponent<PassRewardRowUI>();
                if (rowUi != null) _rows.Add(rowUi);
            }

            // Cập nhật từng dòng
            for (int i = 0; i < targetCount; i++)
            {
                var levelData = config.levels[i];
                bool isReached = player.passLevel >= levelData.level;
                bool isFreeClaimed = player.claimedFreePassLevels.Contains(levelData.level);
                bool isPremiumClaimed = player.claimedPremiumPassLevels.Contains(levelData.level);

                _rows[i].Initialize(levelData, isReached, isFreeClaimed, isPremiumClaimed, hasPremium);
            }
        }

        private void OnUpgradePremiumClicked()
        {
            // Trong thực tế, bạn sẽ gọi chức năng Purchase của IAP ở đây
            // Tôi sẽ cấu hình mua trực tiếp thành công để Test
            var player = GameManager.Instance.DataManager.Player;
            if (player != null && !player.isPremiumPassUnlocked)
            {
                player.isPremiumPassUnlocked = true;
                GameManager.Instance.DataManager.SavePlayerData();
                
                // Gọi Refresh ngay lập tức bằng dữ liệu hiện tại
                RefreshUI(player.passLevel, player.passExp);
                Debug.Log("Đã mở khóa thẻ Premium King God Pass!");
            }
        }
    }
}

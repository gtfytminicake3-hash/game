namespace LegendOfBlood
{
    using UnityEngine;
    using TMPro;

    /// <summary>
    /// Điều khiển thanh hiển thị tài nguyên của người chơi (Vàng, Gỗ, Đá).
    /// Tự động cập nhật bằng cách lắng nghe các sự kiện từ InventoryManager.
    /// </summary>
    public class UIResourceBar : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI woodText;
        [SerializeField] private TextMeshProUGUI stoneText;
        [SerializeField] private TextMeshProUGUI diamondText;

        #region Unity Lifecycle & Event Subscription

        private void OnEnable()
        {
            // Kết nối TRỰC TIẾP với kênh sóng "Analog" C# của Manager thay vì EventManager số hóa.
            InventoryManager.OnResourceChanged += HandleResourceChange;
            DataManager.OnPlayerDataLoaded += UpdateAllResources;
        }

        private void OnDisable()
        {
            InventoryManager.OnResourceChanged -= HandleResourceChange;
            DataManager.OnPlayerDataLoaded -= UpdateAllResources;
        }

        private void Start()
        {
            // Cập nhật ngay khi bắt đầu phòng trường hợp dữ liệu đã có sẵn.
            UpdateAllResources();
        }

        #endregion

        #region Logic

        /// <summary>
        /// Được gọi khi sự kiện OnResourceChanged được phát ra.
        /// </summary>
        private void HandleResourceChange(ResourceType type, int newAmount)
        {
            // Cập nhật chỉ text của loại tài nguyên đã thay đổi.
            switch (type)
            {
                case ResourceType.Gold:
                    UpdateGoldText(newAmount);
                    break;
                case ResourceType.Wood:
                    UpdateWoodText(newAmount);
                    break;
                case ResourceType.Stone:
                    UpdateStoneText(newAmount);
                    break;
                case ResourceType.Diamond:
                    UpdateDiamondText(newAmount);
                    break;
            }
        }

        /// <summary>
        /// Lấy và cập nhật tất cả các giá trị tài nguyên từ InventoryManager.
        /// </summary>
        private void UpdateAllResources()
        {
            if (InventoryManager.Instance == null) return;

            UpdateGoldText(InventoryManager.Instance.GetResourceAmount(ResourceType.Gold));
            UpdateWoodText(InventoryManager.Instance.GetResourceAmount(ResourceType.Wood));
            UpdateStoneText(InventoryManager.Instance.GetResourceAmount(ResourceType.Stone));
            UpdateDiamondText(InventoryManager.Instance.GetResourceAmount(ResourceType.Diamond));
        }

        // Các hàm cập nhật text riêng biệt để code sạch sẽ hơn.
        private void UpdateGoldText(int amount)
        {
            if (goldText != null) goldText.text = amount.ToString();
        }

        private void UpdateWoodText(int amount)
        {
            if (woodText != null) woodText.text = amount.ToString();
        }

        private void UpdateStoneText(int amount)
        {
            if (stoneText != null) stoneText.text = amount.ToString();
        }

        private void UpdateDiamondText(int amount)
        {
            if (diamondText != null) diamondText.text = amount.ToString();
        }

        #endregion
    }
}
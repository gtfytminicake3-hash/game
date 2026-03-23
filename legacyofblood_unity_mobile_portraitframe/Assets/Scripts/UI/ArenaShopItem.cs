namespace LegendOfBlood
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class ArenaShopItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Button buyButton;

        private string _itemName;
        private int _price;

        public void Setup(string itemName, int price)
        {
            _itemName = itemName;
            _price = price;

            if (itemNameText != null) itemNameText.text = _itemName;
            if (priceText != null) priceText.text = _price.ToString() + " Khuyển"; // Đơn vị tiền tệ (ví dụ)

            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(OnBuyClicked);
            }
        }

        private void OnBuyClicked()
        {
            // Tạm thời In log, sau này sẽ móc với InventoryManager (Trừ tiền, cộng đồ)
            Debug.Log($"[ArenaShop] Đã mua thành công {_itemName} với giá {_price}");
        }
    }
}

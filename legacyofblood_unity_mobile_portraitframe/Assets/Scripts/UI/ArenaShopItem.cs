namespace LegendOfBlood
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class ArenaShopItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage; // Dành cho mảng Image UI sau này bạn nhúng Hình vào
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Button buyButton;

        private ArenaShopGood _shopGood;
        private System.Action<ArenaShopGood> _onBuyAction;

        public void Setup(ArenaShopGood good, System.Action<ArenaShopGood> onBuyCallback)
        {
            _shopGood = good;
            _onBuyAction = onBuyCallback;

            if (itemNameText != null) itemNameText.text = _shopGood.displayName;
            if (priceText != null) priceText.text = _shopGood.price.ToString() + " Khuyển";

            // Tự động rà quét Component Image nếu người dùng chưa mớm vào Inspector
            if (iconImage == null)
            {
                Image[] foundImages = GetComponentsInChildren<Image>();
                // Bỏ qua Image đầu tiên (thường là Background nền khung). Lấy Image thứ 2 (chính là ô vuông xám). 
                if (foundImages.Length > 1) iconImage = foundImages[1];
                else if (foundImages.Length == 1) iconImage = foundImages[0];
            }

            if (iconImage != null)
            {
                if (_shopGood.icon != null)
                {
                    iconImage.sprite = _shopGood.icon;
                    iconImage.color = Color.white; // Phục hồi màu
                }
                else
                {
                    iconImage.color = new Color(1, 1, 1, 0.3f); // Mờ mờ nếu chưa cấu hình Icon
                }
            }

            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners();
                
                if (_shopGood.isPurchased && !_shopGood.isInfinite)
                {
                    buyButton.interactable = false;
                    var btnText = buyButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null) btnText.text = "Đã Bán";
                    if (iconImage != null) iconImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f); // Làm xám icon
                }
                else
                {
                    buyButton.interactable = true;
                    buyButton.onClick.AddListener(OnBuyClicked);
                    var btnText = buyButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null) btnText.text = "Mua";
                }
            }
        }

        // Tương thích lùi cho Code cũ nếu cần (nhưng ta sẽ dùng Setup mới)
        public void Setup(string itemName, int price)
        {
            if (itemNameText != null) itemNameText.text = itemName;
            if (priceText != null) priceText.text = price.ToString() + " Khuyển";
        }

        private void OnBuyClicked()
        {
            Debug.Log($"[ArenaShopItem] Nút Mua vật phẩm '{_shopGood?.displayName}' vừa được NHẤP!");
            if (_shopGood != null && _onBuyAction != null)
            {
                _onBuyAction.Invoke(_shopGood);
            }
            else
            {
                Debug.LogWarning("[ArenaShopItem] Lỗi: Nút mua không có dữ liệu hàng hóa (ShopGood) hoặc Callback bị Null!");
            }
        }
    }
}

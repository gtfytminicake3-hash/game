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
            EnsureReferences();
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

        private void EnsureReferences()
        {
            RectTransform rootRt = GetComponent<RectTransform>();
            if (rootRt == null) rootRt = gameObject.AddComponent<RectTransform>();
            if (GetComponent<Image>() == null)
            {
                Image bg = gameObject.AddComponent<Image>();
                bg.color = new Color(0.12f, 0.1f, 0.08f, 0.92f);
            }

            if (iconImage == null)
            {
                Transform icon = transform.Find("Icon_Runtime");
                if (icon == null)
                {
                    GameObject iconObj = new GameObject("Icon_Runtime");
                    iconObj.transform.SetParent(transform, false);
                    RectTransform rt = iconObj.AddComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 1f);
                    rt.anchorMax = new Vector2(0.5f, 1f);
                    rt.pivot = new Vector2(0.5f, 1f);
                    rt.sizeDelta = new Vector2(110f, 110f);
                    rt.anchoredPosition = new Vector2(0f, -24f);
                    iconImage = iconObj.AddComponent<Image>();
                    iconImage.preserveAspect = true;
                }
                else
                {
                    iconImage = icon.GetComponent<Image>();
                }
            }

            if (itemNameText == null) itemNameText = CreateText("Name_Runtime", new Vector2(0.08f, 0.42f), new Vector2(0.92f, 0.66f), 24f);
            if (priceText == null) priceText = CreateText("Price_Runtime", new Vector2(0.08f, 0.25f), new Vector2(0.92f, 0.4f), 22f);

            if (buyButton == null)
            {
                Transform existing = transform.Find("BuyButton_Runtime");
                GameObject btnObj = existing != null ? existing.gameObject : new GameObject("BuyButton_Runtime");
                btnObj.transform.SetParent(transform, false);
                RectTransform rt = btnObj.GetComponent<RectTransform>() ?? btnObj.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.15f, 0.05f);
                rt.anchorMax = new Vector2(0.85f, 0.2f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                Image image = btnObj.GetComponent<Image>() ?? btnObj.AddComponent<Image>();
                image.color = new Color(0.65f, 0.18f, 0.08f, 0.95f);
                buyButton = btnObj.GetComponent<Button>() ?? btnObj.AddComponent<Button>();

                TextMeshProUGUI text = btnObj.GetComponentInChildren<TextMeshProUGUI>(true);
                if (text == null)
                {
                    text = CreateText("Text", Vector2.zero, Vector2.one, 22f, btnObj.transform);
                    text.text = "Mua";
                    text.color = Color.white;
                }
            }
        }

        private TextMeshProUGUI CreateText(string objectName, Vector2 anchorMin, Vector2 anchorMax, float fontSize, Transform parentOverride = null)
        {
            GameObject textObj = new GameObject(objectName);
            textObj.transform.SetParent(parentOverride != null ? parentOverride : transform, false);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = fontSize;
            text.color = Color.white;
            RectTransform rt = text.rectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return text;
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

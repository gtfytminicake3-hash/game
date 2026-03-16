using UnityEngine;
using UnityEngine.UI;

namespace LegendOfBlood
{
    public class ArenaShopPanel : UIPanel
    {
        [Header("UI References")]
        [SerializeField] private Button closeButton;
        public Button adFreebieButton; 
        [SerializeField] private Transform itemContainer;
        [SerializeField] private GameObject shopItemPrefab;

        private void Awake()
        {
            PanelType = UIPanelType.ArenaShop;
            if (closeButton != null) closeButton.onClick.AddListener(() => GameManager.Instance.UIManager.GoBack());
            if (adFreebieButton != null) adFreebieButton.onClick.AddListener(OnAdFreebieClicked);
        }

        protected virtual void Start()
        {
            base.Start();
        }

        private void OnEnable()
        {
            RefreshShop();
        }

        private void RefreshShop()
        {
            // Clear existing items
            foreach (Transform child in itemContainer)
            {
                Destroy(child.gameObject);
            }

            // Populate with new items (mock data for now)
            // In a real implementation, this would come from a config file or ScriptableObject
            CreateShopItem(global::LocalizationSystem.GetText("item_IT_FERTILITY_POTION_name"), 5000);
            CreateShopItem(global::LocalizationSystem.GetText("item_IT_EXP_BOOK_S_name"), 10000);
            CreateShopItem(global::LocalizationSystem.GetText("item_IT_SPEEDUP_1H_name"), 200);
        }

        private void CreateShopItem(string itemName, int price)
        { 
            if (shopItemPrefab == null) return;
            GameObject itemGO = Instantiate(shopItemPrefab, itemContainer);
            ArenaShopItem item = itemGO.GetComponent<ArenaShopItem>();
            if (item != null)
            {
                item.Setup(itemName, price);
            }
            Debug.Log($"Created shop item: {itemName} for {price} Arena Coins.");
        }

        private void OnAdFreebieClicked()
        {
            if (Managers.AdRewardGateway.Instance != null && DataManager.Instance != null)
            {
                if (DataManager.Instance.Player.dailyShopFreebieAdsWatched < 3)
                {
                    Managers.AdRewardGateway.Instance.RequestAd(Managers.RewardType.ShopFreebie, () => {
                        // Cập nhật trạng thái nút nếu full lượt (Tùy chọn)
                        if (DataManager.Instance.Player.dailyShopFreebieAdsWatched >= 3)
                        {
                             if (adFreebieButton != null) adFreebieButton.gameObject.SetActive(false);
                        }
                    });
                }
                else
                {
                    GameManager.Instance.UINotificationManager.ShowNotification(global::LocalizationSystem.GetText("ad_limit_reached") ?? "Hôm nay bạn đã hết lượt nhận xu miễn phí!");
                }
            }
        }
    }
}

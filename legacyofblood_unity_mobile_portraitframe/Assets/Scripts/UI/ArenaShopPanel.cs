using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LegendOfBlood
{
    public enum ShopGoodType { Item, Equipment, Resource }

    [System.Serializable]
    public class ArenaShopGood
    {
        public ShopGoodType type;
        public string refId; // Ví dụ: "IT_EXP_BOOK_S", "Knight's Helm", "GOLD"
        public string displayName;
        public int price;
        public int amount; // Số lượng nhận được
        
        // Hình ảnh (Tự kéo thả trong Inspector) - Sẽ null khi tải từ file lưu, cần gán lại
        [System.NonSerialized]
        public Sprite icon;

        // Dành riêng cho trang bị
        public EquipmentTier equipTier = EquipmentTier.D;
        public EquipmentSlot equipSlot = EquipmentSlot.Weapon;
        public Profession equipRestriction = Profession.None;
        
        // Trạng thái mua bán
        public bool isPurchased = false;
        public bool isInfinite = false; // Mua thoải mái (ví dụ vàng bèo bọt)
    }

    public class ArenaShopPanel : UIPanel
    {
        [Header("UI References")]
        [SerializeField] private Button closeButton;
        public Button adFreebieButton; 
        [SerializeField] private Transform itemContainer;
        [SerializeField] private GameObject shopItemPrefab;

        [Header("Shop Configuration")]
        [SerializeField] private System.Collections.Generic.List<ArenaShopGood> dailyGoods;

        [Header("Top Currencies (Kéo 3 cuộn giấy vào đây)")]
        [SerializeField] private RectTransform goldBanner;
        [SerializeField] private RectTransform stoneBanner;
        [SerializeField] private RectTransform gemBanner;
        private bool _missingLayoutWarned;

        private void Awake()
        {
            PanelType = UIPanelType.ArenaShop;
            EnsureReferences();
            if (closeButton != null) closeButton.onClick.AddListener(() => GameManager.Instance.UIManager.GoBack());
            if (adFreebieButton != null) adFreebieButton.onClick.AddListener(OnAdFreebieClicked);
            
            // Đã xóa bỏ thuật toán rò tìm Auto-hack dính tên chuỗi và tọa độ X.
            // Bắt buộc phải kéo thả goldBanner, stoneBanner, gemBanner vào Inspector để tránh lỗi xóa nhầm Component (CleanupBadObjects).
        }

        private void OnEnable()
        {
            EnsureReferences();
            CheckAndRefreshDailyGoods();
            SetupTopBanners();
            UpdateCurrencies();
            RefreshShop();
        }

        private void EnsureReferences()
        {
            if (itemContainer == null)
            {
                ScrollRect scrollRect = GetComponentInChildren<ScrollRect>(true);
                if (scrollRect != null && scrollRect.content != null)
                {
                    itemContainer = scrollRect.content;
                }
            }

            if (itemContainer == null)
            {
                ArenaShopItem firstItem = GetComponentInChildren<ArenaShopItem>(true);
                if (firstItem != null && firstItem.transform.parent != null)
                {
                    itemContainer = firstItem.transform.parent;
                }
            }

            if (itemContainer == null)
            {
                Transform content = FindChildByName(transform, "Content") ?? FindChildByName(transform, "ItemContainer");
                if (content != null) itemContainer = content;
            }

            if (itemContainer == null)
            {
                GameObject containerObj = new GameObject("RuntimeItemContainer");
                containerObj.transform.SetParent(transform, false);
                RectTransform rt = containerObj.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.06f, 0.12f);
                rt.anchorMax = new Vector2(0.94f, 0.78f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                GridLayoutGroup grid = containerObj.AddComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(290f, 360f);
                grid.spacing = new Vector2(24f, 24f);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 2;
                itemContainer = containerObj.transform;
            }

            if (shopItemPrefab == null)
            {
                ArenaShopItem firstItem = itemContainer != null ? itemContainer.GetComponentInChildren<ArenaShopItem>(true) : GetComponentInChildren<ArenaShopItem>(true);
                if (firstItem != null) shopItemPrefab = firstItem.gameObject;
            }
        }

        private Transform FindChildByName(Transform root, string targetName)
        {
            foreach (Transform child in root)
            {
                if (child.name == targetName) return child;
                Transform nested = FindChildByName(child, targetName);
                if (nested != null) return nested;
            }
            return null;
        }

        private void UpdateCurrencies()
        {
            if (DataManager.Instance == null || DataManager.Instance.Player == null) return;
            var r = DataManager.Instance.Player.resources;
            
            if (goldBanner != null) {
                var txt = goldBanner.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (txt != null) txt.text = r.gold.ToString();
            }
            if (stoneBanner != null) {
                var txt = stoneBanner.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (txt != null) txt.text = r.stone.ToString();
            }
            if (gemBanner != null) {
                var txt = gemBanner.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (txt != null) txt.text = r.diamond.ToString();
            }
        }

        private void SetupTopBanners()
        {
            SetupSingleBanner(goldBanner, "Icons/mainscreen/Gold", new Vector2(0.15f, 0.15f), new Vector2(0.4f, 0.85f));
            SetupSingleBanner(stoneBanner, "Icons/mainscreen/Stone", new Vector2(0.15f, 0.1f), new Vector2(0.45f, 0.9f));
            SetupSingleBanner(gemBanner, "Icons/mainscreen/Gems", new Vector2(0.15f, 0.1f), new Vector2(0.45f, 0.9f));
        }

        private void SetupSingleBanner(RectTransform banner, string iconPath, Vector2 iconMin, Vector2 iconMax)
        {
            if (banner == null) return;

            // KIểm tra Text
            var txt = banner.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (txt == null)
            {
                GameObject txtObj = new GameObject("TextValue");
                txtObj.transform.SetParent(banner, false);
                txt = txtObj.AddComponent<TMPro.TextMeshProUGUI>();
                
                txt.alignment = TMPro.TextAlignmentOptions.Center;
                txt.fontSize = 28; // To và rõ ràng
                txt.color = new Color(0.2f, 0.1f, 0f);
                txt.fontStyle = TMPro.FontStyles.Bold;
                
                RectTransform rectT = txt.GetComponent<RectTransform>();
                // Canh lề chữ ở nửa Rộng bên PHẢI cuộn giấy
                rectT.anchorMin = new Vector2(0.4f, 0.1f);
                rectT.anchorMax = new Vector2(0.9f, 0.9f);
                rectT.offsetMin = Vector2.zero;
                rectT.offsetMax = Vector2.zero;
            }

            // Kiểm tra Icon
            Image iconImg = null;
            foreach (Transform child in banner) {
                if (child.name == "Icon") iconImg = child.GetComponent<Image>();
            }
            
            if (iconImg == null)
            {
                GameObject iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(banner, false);
                iconImg = iconObj.AddComponent<Image>();
                iconImg.preserveAspect = true; // Chống biến dạng hình
                
                RectTransform rectI = iconImg.GetComponent<RectTransform>();
                // Cắt về nửa bé bên TRÁI cuộn giấy, lọt lòng trong viền cuộn giấy
                rectI.anchorMin = iconMin;
                rectI.anchorMax = iconMax;
                rectI.offsetMin = Vector2.zero;
                rectI.offsetMax = Vector2.zero;
            }

            if (iconImg.sprite == null)
            {
                Sprite sp = UnityEngine.Resources.Load<Sprite>(iconPath);
                if (sp != null) iconImg.sprite = sp;
            }
        }

        private void CheckAndRefreshDailyGoods()
        {
            if (DataManager.Instance == null || DataManager.Instance.Player == null) return;
            
            var player = DataManager.Instance.Player;
            long currentTime = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Check if it's a new day (UTC)
            System.DateTime lastRefreshTime = System.DateTimeOffset.FromUnixTimeMilliseconds(player.lastArenaShopRefreshTimestamp).UtcDateTime;
            System.DateTime currentDateTime = System.DateTimeOffset.UtcNow.UtcDateTime;

            bool needsRefresh = false;
            // Nếu chưa có mảng Data hoặc bị rỗng
            if (player.currentArenaShopGoods == null || player.currentArenaShopGoods.Count == 0)
            {
                needsRefresh = true;
            }
            // Nếu qua ngày mới
            else if (currentDateTime.Date > lastRefreshTime.Date)
            {
                needsRefresh = true;
            }

            if (needsRefresh)
            {
                RollNewGoods(player);
                player.lastArenaShopRefreshTimestamp = currentTime;
                DataManager.Instance.SavePlayerData();
            }

            // Tự động săn tìm Hình ảnh cho mặt hàng vì Sprite không serialize
            foreach (var good in player.currentArenaShopGoods)
            {
                FetchIconForGood(good);
            }
        }

        private void RollNewGoods(PlayerData player)
        {
            player.currentArenaShopGoods = new System.Collections.Generic.List<ArenaShopGood>();
            
            // Nếu có cấu hình Pool từ GameConfig
            if (DataManager.Instance != null && DataManager.Instance.GameConfig != null && DataManager.Instance.GameConfig.ArenaShopPool != null && DataManager.Instance.GameConfig.ArenaShopPool.Count > 0)
            {
                var pool = DataManager.Instance.GameConfig.ArenaShopPool;
                int totalWeight = 0;
                foreach(var p in pool) totalWeight += p.weight;
                if (totalWeight <= 0)
                {
                    Debug.LogWarning("[ArenaShop] ArenaShopPool has no positive weights. Using fallback goods.", this);
                }

                int itemsToRoll = 6; // Số ô trong shop
                for (int i = 0; totalWeight > 0 && i < itemsToRoll; i++)
                {
                    int roll = Random.Range(0, totalWeight);
                    int currentWeight = 0;
                    GameConfigs.ArenaShopPoolItem selected = null;

                    foreach(var p in pool)
                    {
                        currentWeight += p.weight;
                        if (roll < currentWeight)
                        {
                            selected = p;
                            break;
                        }
                    }

                    if (selected != null)
                    {
                        ArenaShopGood newGood = new ArenaShopGood
                        {
                            type = selected.type,
                            refId = selected.refId,
                            displayName = string.IsNullOrEmpty(selected.displayName) ? GetDefaultName(selected.refId) : selected.displayName,
                            price = Random.Range(selected.priceMin, selected.priceMax + 1),
                            amount = Random.Range(selected.amountMin, selected.amountMax + 1),
                            equipTier = selected.equipTier,
                            equipSlot = selected.equipSlot,
                            equipRestriction = selected.equipRestriction,
                            isPurchased = false,
                            isInfinite = selected.isInfinite
                        };
                        player.currentArenaShopGoods.Add(newGood);
                    }
                }
            }
            else
            {
                // Fallback nếu người dùng chưa thiết lập mảng ArenaShopPool: Dùng dailyGoods cứng
                if (dailyGoods == null || dailyGoods.Count == 0)
                {
                    dailyGoods = new System.Collections.Generic.List<ArenaShopGood>
                    {
                        new ArenaShopGood { type = ShopGoodType.Item, refId = "ITEM_FERTILITY_POTION", displayName = global::LocalizationSystem.GetText("item_IT_FERTILITY_POTION_name") ?? "Thuốc Sinh Sản", price = 5000, amount = 1 },
                        new ArenaShopGood { type = ShopGoodType.Item, refId = "ITEM_EXP_BOOK_S", displayName = global::LocalizationSystem.GetText("item_IT_EXP_BOOK_S_name") ?? "Sách Kinh Nghiệm Nhỏ", price = 1000, amount = 5 },
                        new ArenaShopGood { type = ShopGoodType.Item, refId = "ITEM_SPEEDUP_1H", displayName = global::LocalizationSystem.GetText("item_IT_SPEEDUP_1H_name") ?? "Tua Nhanh 1H", price = 200, amount = 1 },
                        new ArenaShopGood { type = ShopGoodType.Equipment, refId = "EQ_KNIGHT_HELM", displayName = "Mũ Hiệp Sĩ", price = 1200, amount = 1, equipTier = EquipmentTier.B, equipSlot = EquipmentSlot.Helm, equipRestriction = Profession.Warrior },
                        new ArenaShopGood { type = ShopGoodType.Item, refId = "ITEM_SUMMON_SCROLL", displayName = "Cuộn Triệu Hồi", price = 3000, amount = 1 },
                        new ArenaShopGood { type = ShopGoodType.Resource, refId = "GOLD", displayName = "Gói 10,000 Vàng", price = 800, amount = 10000, isInfinite = true } // Vàng cho mua thả ga
                    };
                }
                foreach(var g in dailyGoods)
                {
                    // Copy sang save data để không bị reference đè
                    player.currentArenaShopGoods.Add(new ArenaShopGood {
                        type = g.type, refId = g.refId, displayName = g.displayName, 
                        price = g.price, amount = g.amount, equipTier = g.equipTier, 
                        equipSlot = g.equipSlot, equipRestriction = g.equipRestriction, 
                        isPurchased = false, isInfinite = g.isInfinite
                    });
                }
            }

            if (player.currentArenaShopGoods.Count == 0)
            {
                player.currentArenaShopGoods.Add(new ArenaShopGood { type = ShopGoodType.Item, refId = "ITEM_EXP_BOOK_S", displayName = "EXP Book", price = 1000, amount = 5 });
                player.currentArenaShopGoods.Add(new ArenaShopGood { type = ShopGoodType.Resource, refId = "GOLD", displayName = "Gold Pack", price = 800, amount = 10000, isInfinite = true });
            }
        }

        private string GetDefaultName(string refId)
        {
            var localized = global::LocalizationSystem.GetText("item_" + refId + "_name");
            return !string.IsNullOrEmpty(localized) ? localized : refId;
        }

        private void FetchIconForGood(ArenaShopGood good)
        {
            if (good.icon == null)
            {
                if (good.type == ShopGoodType.Equipment)
                {
                    string slotString = good.equipSlot.ToString();
                    if (good.equipSlot == EquipmentSlot.Ring1 || good.equipSlot == EquipmentSlot.Ring2) slotString = "Ring";
                    string equipIconPath = $"Icons/Equipments/{good.equipRestriction}_{slotString}_{good.equipTier}";
                    good.icon = UnityEngine.Resources.Load<Sprite>(equipIconPath);
                    
                    if (good.icon == null) good.icon = UnityEngine.Resources.Load<Sprite>($"Icons/Equipments/Warrior_Helm_B");
                }
                else if (good.type == ShopGoodType.Item)
                {
                    if (DataManager.Instance != null && DataManager.Instance.AllItems != null && DataManager.Instance.AllItems.ContainsKey(good.refId))
                    {
                        good.icon = DataManager.Instance.AllItems[good.refId].icon;
                    }
                    if (good.icon == null) good.icon = UnityEngine.Resources.Load<Sprite>($"Icons/Items/{good.refId}");
                    
                    // FALLBACK CHO CÁC MÓN ITEM MỚI MÀ USER CHƯA CHÈN KHUNG ẢNH
                    if (good.icon == null)
                    {
                        if (good.refId.Contains("EXP_BOOK")) good.icon = UnityEngine.Resources.Load<Sprite>($"Icons/Items/ITEM_EXP_BOOK_S");
                        else if (good.refId.Contains("SPEEDUP")) good.icon = UnityEngine.Resources.Load<Sprite>($"Icons/Items/ITEM_SPEEDUP_1H");
                        else if (good.refId.Contains("SUMMON_SCROLL")) good.icon = UnityEngine.Resources.Load<Sprite>($"Icons/Items/ITEM_SUMMON_SCROLL");
                    }
                }
                else if (good.type == ShopGoodType.Resource)
                {
                    if (good.refId == "GOLD") good.icon = UnityEngine.Resources.Load<Sprite>("Icons/mainscreen/Gold");
                    else if (good.refId == "STONE") good.icon = UnityEngine.Resources.Load<Sprite>("Icons/mainscreen/Stone");
                    else if (good.refId == "WOOD") good.icon = UnityEngine.Resources.Load<Sprite>("Icons/mainscreen/Stone"); // Chưa có hình gỗ, mượn tạm cục đá
                }
            }
        }

        protected override void Start()
        {
            base.Start();
        }

        private void RefreshShop()
        {
            EnsureReferences();
            CheckAndRefreshDailyGoods();
            if (DataManager.Instance == null || DataManager.Instance.Player == null) return;
            if (itemContainer == null)
            {
                if (!_missingLayoutWarned)
                {
                    Debug.LogError("[ArenaShop] itemContainer is missing and could not be rebuilt.", this);
                    _missingLayoutWarned = true;
                }
                return;
            }

            var currentGoods = DataManager.Instance.Player.currentArenaShopGoods;
            if (currentGoods == null) return;

            ArenaShopItem[] staticItems = itemContainer.GetComponentsInChildren<ArenaShopItem>(true);

            for (int i = 0; i < staticItems.Length; i++)
            {
                if (i < currentGoods.Count)
                {
                    staticItems[i].gameObject.SetActive(true);
                    staticItems[i].Setup(currentGoods[i], AttemptPurchase);
                }
                else
                {
                    staticItems[i].gameObject.SetActive(false); // Ẩn bớt nếu dư ô
                }
            }
            
            // Nếu không đủ ô tĩnh, tự sinh thêm
            for (int i = staticItems.Length; i < currentGoods.Count; i++)
            {
                CreateShopItem(currentGoods[i]);
            }
        }

        private void CreateShopItem(ArenaShopGood good)
        { 
            if (shopItemPrefab == null)
            {
                GameObject fallback = new GameObject("ArenaShopItem_Runtime");
                fallback.transform.SetParent(itemContainer, false);
                RectTransform rt = fallback.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(290f, 360f);
                ArenaShopItem fallbackItem = fallback.AddComponent<ArenaShopItem>();
                fallbackItem.Setup(good, AttemptPurchase);
                return;
            }
            GameObject itemGO = Instantiate(shopItemPrefab, itemContainer);
            ArenaShopItem item = itemGO.GetComponent<ArenaShopItem>();
            if (item != null)
            {
                item.Setup(good, AttemptPurchase);
            }
        }

        private void AttemptPurchase(ArenaShopGood good)
        {
            if (DataManager.Instance == null || DataManager.Instance.Player == null) return;

            PlayerData player = DataManager.Instance.Player;

            // Kiểm tra tài chính (Xu Đấu Trường)
            if (player.arenaCoins < good.price)
            {
                Debug.LogWarning($"[ArenaShop] Không đủ tiền! Món đồ {good.displayName} giá {good.price} nhưng bạn chỉ có {player.arenaCoins} Khuyển.");
                GameManager.Instance.UINotificationManager.ShowNotification("Không đủ Khuyển (Arena Coins)!");
                return;
            }

            // Thanh toán
            player.arenaCoins -= good.price;

            // Giao hàng
            switch (good.type)
            {
                case ShopGoodType.Item:
                    InventoryManager.Instance.AddItem(good.refId, good.amount);
                    break;
                case ShopGoodType.Equipment:
                    // Tạo một định dạng trang bị mới
                    EquipmentData newEquip = new EquipmentData(System.Guid.NewGuid().ToString(), good.displayName, good.equipSlot, 1, good.equipTier, good.equipRestriction);
                    InventoryManager.Instance.AddEquipment(newEquip);
                    break;
                case ShopGoodType.Resource:
                    if (good.refId == "GOLD") player.resources.gold += good.amount;
                    else if (good.refId == "WOOD") player.resources.wood += good.amount;
                    break;
            }

            // Đánh dấu đã mua nếu không phải vật phẩm Infinite (mua vô tận)
            if (!good.isInfinite)
            {
                good.isPurchased = true;
            }

            // Lưu Dữ liệu
            DataManager.Instance.SavePlayerData();

            // Hiệu ứng Báo Cáo
            GameManager.Instance.UINotificationManager.ShowNotification($"Mua thành công {good.displayName}!");
            Debug.Log($"[ArenaShop] Giao dịch hoàn tất: Trừ {good.price} ArenaCoins. Còn lại: {player.arenaCoins}");
            
            // Làm tươi lại gian hàng
            RefreshShop();
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

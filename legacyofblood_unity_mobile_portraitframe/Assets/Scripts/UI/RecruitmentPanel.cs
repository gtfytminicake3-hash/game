namespace LegendOfBlood
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class RecruitmentPanel : UIPanel
    {
        [Header("Action Buttons")]
        [SerializeField] private Button recruitOneButton;
        [SerializeField] private Button recruitTenButton;
        public Button recruitAdButton;
        [SerializeField] private Button closeButton;

        [Header("Resource Display")]
        [SerializeField] private TextMeshProUGUI ticketCountText;
        [SerializeField] private TextMeshProUGUI diamondCountText;
        [SerializeField] private TextMeshProUGUI populationText;

        [Header("Price Labels")]
        [SerializeField] private TextMeshProUGUI priceOneText;
        [SerializeField] private TextMeshProUGUI priceTenText;

        [Header("Result Display")]
        [SerializeField] private GameObject resultOverlay;
        [SerializeField] private Transform resultCardContainer;
        [SerializeField] private GameObject resultCardPrefab;
        [SerializeField] private Button resultCloseButton;
        [SerializeField] private TextMeshProUGUI resultTitleText;

        [Header("Gacha Config")]
        [SerializeField] private int singleCostDiamond = 150;
        [SerializeField] private int tenCostDiamond = 1500;

        [Header("Carousel Settings (Summon x10)")]
        [SerializeField] public float multiCardWidth = 540f;
        [SerializeField] public float multiCardHeight = 750f;
        [SerializeField] public float multiCardSpacing = -350f;
        [SerializeField] public float multiCenterScale = 1.15f;
        [SerializeField] public float multiNormalScale = 0.75f;

        private const string GACHA_TICKET_ID = "IT_GACHA_TICKET";

        private bool _isProcessing;

        private void Awake()
        {
            PanelType = UIPanelType.Recruitment;
            // Xóa bỏ AutoBindRuntime. Tất cả các nút bấm (recruitOneButton, recruitTenButton, resultOverlay...)
            // BẮT BUỘC phải được kéo thả trong Unity Inspector.
        }

        // Đã loại bỏ các hàm rò tìm string (FindChild, FindChildComponent) để tránh lỗi null khi thay đổi tên UI.

        protected override void Start()
        {
            base.Start();

            SetupAnimatedBackground();
            BindButtons();
            UpdatePriceLabels();

            if (resultOverlay != null) resultOverlay.SetActive(false);

            Debug.Log("[RecruitmentPanel] Initialized & Bound successfully");
        }

        private void EnsureClickable(Button btn)
        {
            if (btn == null) return;
            var graphic = btn.GetComponent<UnityEngine.UI.Graphic>();
            if (graphic == null)
            {
                graphic = btn.gameObject.AddComponent<Image>();
                graphic.color = new Color(0, 0, 0, 0); // Transparent
            }
            graphic.raycastTarget = true;
            if (btn.targetGraphic == null) btn.targetGraphic = graphic;
        }

        private void OnEnable()
        {
            InventoryManager.OnResourceChanged += OnResourceChanged;
            InventoryManager.OnItemChanged += OnItemChanged;
            _isProcessing = false; // Reset state in case it was stuck
            RefreshUI();
        }

        private void OnDisable()
        {
            InventoryManager.OnResourceChanged -= OnResourceChanged;
            InventoryManager.OnItemChanged -= OnItemChanged;
        }

        private void OnDestroy()
        {
            if (recruitOneButton != null) recruitOneButton.onClick.RemoveAllListeners();
            if (recruitTenButton != null) recruitTenButton.onClick.RemoveAllListeners();
            if (recruitAdButton != null) recruitAdButton.onClick.RemoveAllListeners();
            if (closeButton != null) closeButton.onClick.RemoveAllListeners();
            if (resultCloseButton != null) resultCloseButton.onClick.RemoveAllListeners();
        }

        #region Setup

        private void SetupAnimatedBackground()
        {
            // bgImage nên được gán qua SerializeField hoặc tự động lấy từ chính object này.
            if (bgImage == null)
            {
                bgImage = GetComponent<Image>();
            }

            if (bgImage != null)
            {
                var animBg = bgImage.gameObject.GetComponent<LegendOfBlood.UI.AnimatedUIBackground>();
                if (animBg == null)
                {
                    animBg = bgImage.gameObject.AddComponent<LegendOfBlood.UI.AnimatedUIBackground>();
                    animBg.resourceFolderPath = "UI/RecruitmentBG";
                    animBg.fps = 24f;
                }
            }
        }

        private void BindButtons()
        {
            if (recruitOneButton != null) recruitOneButton.onClick.AddListener(OnRecruitOne);
            if (recruitTenButton != null) recruitTenButton.onClick.AddListener(OnRecruitTen);
            if (recruitAdButton != null) recruitAdButton.onClick.AddListener(OnRecruitAd);
            if (closeButton != null) closeButton.onClick.AddListener(() => GameManager.Instance.UIManager.GoBack());
            if (resultCloseButton != null) resultCloseButton.onClick.AddListener(CloseResultOverlay);
        }

        private void UpdatePriceLabels()
        {
            if (priceOneText != null) priceOneText.text = $"1 vé / {singleCostDiamond} 💎";
            if (priceTenText != null) priceTenText.text = $"10 vé / {tenCostDiamond} 💎";
        }

        #endregion

        #region UI Refresh

        private void OnResourceChanged(ResourceType type, int newAmount)
        {
            if (type == ResourceType.Diamond) RefreshDiamondDisplay();
        }

        private void OnItemChanged(string itemID, int newCount)
        {
            if (itemID == GACHA_TICKET_ID) RefreshTicketDisplay();
        }

        private void RefreshUI()
        {
            RefreshTicketDisplay();
            RefreshDiamondDisplay();
            RefreshPopulationDisplay();
            RefreshAdButton();
            RefreshButtonInteractable();
        }

        private void RefreshTicketDisplay()
        {
            if (ticketCountText == null) return;
            int tickets = GameManager.Instance?.InventoryManager?.GetItemCount(GACHA_TICKET_ID) ?? 0;
            ticketCountText.text = $"🎫 {tickets}";
        }

        private void RefreshDiamondDisplay()
        {
            if (diamondCountText == null) return;
            int diamonds = GameManager.Instance?.InventoryManager?.GetResourceAmount(ResourceType.Diamond) ?? 0;
            diamondCountText.text = $"💎 {diamonds}";
        }

        private void RefreshPopulationDisplay()
        {
            if (populationText == null) return;
            if (DataManager.Instance?.Player == null) return;

            int current = DataManager.Instance.Player.Heroes.Count;
            int max = DataManager.Instance.GetPopulationCapacity();
            populationText.text = $"👥 {current}/{max}";

            bool isFull = current >= max;
            if (populationText != null)
            {
                populationText.color = isFull ? new Color(1f, 0.3f, 0.3f) : Color.white;
            }
        }

        private void RefreshAdButton()
        {
            if (recruitAdButton == null) return;
            bool canWatchAd = false;
            if (DataManager.Instance?.Player != null)
            {
                canWatchAd = DataManager.Instance.Player.dailyFreeSummonsWatched == 0;
            }
            recruitAdButton.gameObject.SetActive(canWatchAd);
        }

        private void RefreshButtonInteractable()
        {
            bool populationFull = DataManager.Instance?.IsPopulationFull() ?? true;
            if (recruitOneButton != null) recruitOneButton.interactable = !populationFull && !_isProcessing;
            if (recruitTenButton != null) recruitTenButton.interactable = !populationFull && !_isProcessing;
            if (recruitAdButton != null) recruitAdButton.interactable = !populationFull && !_isProcessing;
        }

        #endregion

        #region Recruit Actions

        private void OnRecruitOne()
        {
            if (_isProcessing) return;

            if (!ValidatePopulation()) return;
            if (!TryPayment(1, singleCostDiamond)) return;

            _isProcessing = true;
            RefreshButtonInteractable();

            var newHeroes = GameManager.Instance.RecruitmentSystem.PerformRecruitment(1);
            OnRecruitmentComplete(newHeroes);
        }

        private void OnRecruitTen()
        {
            if (_isProcessing) return;

            // Kiểm tra trước sức chứa để không trừ oan tiền của người chơi
            int currentPop = DataManager.Instance.Player.Heroes.Count;
            int maxPop = DataManager.Instance.GetPopulationCapacity();
            
            if (maxPop - currentPop < 10)
            {
                Debug.LogWarning($"[RecruitmentPanel] Dân số chỉ còn {maxPop - currentPop} chỗ trống, không thể chiêu mộ x10!");
                ShowNotification("notification_population_full"); // Hoặc popup báo "Cần dọn dẹp kho"
                return;
            }

            if (!ValidatePopulation()) return;
            if (!TryPayment(10, tenCostDiamond)) return;

            _isProcessing = true;
            RefreshButtonInteractable();

            var newHeroes = GameManager.Instance.RecruitmentSystem.PerformRecruitment(10);
            OnRecruitmentComplete(newHeroes);
        }

        private void OnRecruitAd()
        {
            if (_isProcessing) return;

            if (!ValidatePopulation()) return;

            if (LegendOfBlood.Managers.AdRewardGateway.Instance == null)
            {
                // Fallback: quảng cáo chưa sẵn sàng, cho quay miễn phí luôn (dev mode)
                Debug.LogWarning("[RecruitmentPanel] AdRewardGateway chưa sẵn sàng. Cho quay miễn phí (DEV).");
                ExecuteFreeRecruitment();
                return;
            }

            _isProcessing = true;
            RefreshButtonInteractable();

            LegendOfBlood.Managers.AdRewardGateway.Instance.RequestAd(
                LegendOfBlood.Managers.RewardType.DailySummon,
                () =>
                {
                    ExecuteFreeRecruitment();
                });
        }

        private void ExecuteFreeRecruitment()
        {
            var newHeroes = GameManager.Instance.RecruitmentSystem.PerformRecruitment(1);
            OnRecruitmentComplete(newHeroes);
            RefreshAdButton();
        }

        #endregion

        #region Payment Logic

        private bool ValidatePopulation()
        {
            if (DataManager.Instance.IsPopulationFull())
            {
                Debug.LogWarning("[RecruitmentPanel] Dân số đã đầy! Không thể chiêu mộ.");
                ShowNotification("notification_population_full");
                return false;
            }
            return true;
        }

        private bool TryPayment(int ticketCount, int diamondCost)
        {
            var inv = GameManager.Instance.InventoryManager;

            // Priority 1: Gacha Tickets
            if (inv.GetItemCount(GACHA_TICKET_ID) >= ticketCount)
            {
                inv.UseItem(GACHA_TICKET_ID, ticketCount);
                Debug.Log($"[RecruitmentPanel] Thanh toán bằng {ticketCount} vé.");
                return true;
            }

            // Priority 2: Diamonds
            if (inv.HasEnoughResources(ResourceType.Diamond, diamondCost))
            {
                inv.SpendResource(ResourceType.Diamond, diamondCost);
                Debug.Log($"[RecruitmentPanel] Thanh toán bằng {diamondCost} kim cương.");
                return true;
            }

            Debug.LogWarning($"[RecruitmentPanel] KHÔNG ĐỦ TIỀN! Yêu cầu: {ticketCount} vé hoặc {diamondCost} gem. (Số gem hiện có: {inv.GetResourceAmount(ResourceType.Diamond)})");
            ShowNotification("notification_not_enough_currency");

#if UNITY_EDITOR
            // DEV HOTFIX: Nếu chơi ở Editor mà hết tiền thì cho mượn tiền quay luôn để test animation!
            Debug.Log("<color=yellow>[DEV MODE] Cho phép quay vi phạm chi phí để test animation!</color>");
            return true;
#else
            return false;
#endif
        }

        #endregion

        #region Post-Recruitment

        private void OnRecruitmentComplete(List<HeroData> newHeroes)
        {
            _isProcessing = false;

            // Save game data
            GameManager.Instance.DataManager.SavePlayerData();

            // Fire events
            EventManager.TriggerEvent(GameEvents.OnHeroListChanged);

            // Refresh displays
            RefreshUI();

            if (newHeroes == null || newHeroes.Count == 0)
            {
                ShowNotification("notification_population_full");
                return;
            }

            // Show result popup
            ShowRecruitmentResults(newHeroes);

            // Show summary notification
            if (newHeroes.Count == 1)
            {
                string msgTemplate = LocalizationSystem.GetText("notification_recruit_success");
                string msg = string.Format(msgTemplate, newHeroes[0].heroName);
                GameManager.Instance.UINotificationManager.ShowNotification(msg);
            }
            else
            {
                string msgTemplate = LocalizationSystem.GetText("notification_recruit_multi_success");
                string msg = string.Format(msgTemplate, newHeroes.Count);
                GameManager.Instance.UINotificationManager.ShowNotification(msg);
            }

            // Grant King God Pass EXP per hero recruited
            if (GameManager.Instance.InventoryManager != null)
            {
                GameManager.Instance.InventoryManager.AddPassExp(newHeroes.Count * 10);
                GameManager.Instance.InventoryManager.AddPlayerExp(newHeroes.Count * 5);
            }

            Debug.Log($"[RecruitmentPanel] Recruited {newHeroes.Count} heroes successfully.");
        }

        #endregion

        #region Result Overlay

        private void ShowRecruitmentResults(List<HeroData> heroes)
        {
            if (resultOverlay == null)
            {
                Debug.LogWarning("[RecruitmentPanel] resultOverlay chưa được gán. Bỏ qua hiển thị kết quả.");
                return;
            }

            resultOverlay.SetActive(true);

            // Set title
            if (resultTitleText != null)
            {
                resultTitleText.text = heroes.Count == 1
                    ? "✨ CHIÊU MỘ THÀNH CÔNG ✨"
                    : $"✨ {heroes.Count} ANH HÙNG MỚI ✨";
            }

            // Clear old cards
            if (resultCardContainer != null)
            {
                foreach (Transform child in resultCardContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            // Spawn result cards with animation
            StartCoroutine(AnimateCardsRoutine(heroes));
        }

        private IEnumerator AnimateCardsRoutine(List<HeroData> heroes)
        {
            if (resultCardContainer == null) yield break;

            bool isMulti = heroes.Count > 1;

            float targetWidth = isMulti ? 200 : 720;
            float targetHeight = isMulti ? 280 : 1000;
            float glowSize = isMulti ? 400 : 1400;
            float titleSize = isMulti ? 20 : 56;
            float statSize = isMulti ? 14 : 42;

            // Chuyển đổi Layout Group dựa trên số lượng Card
            var hlg = resultCardContainer.GetComponent<HorizontalLayoutGroup>();
            var glg = resultCardContainer.GetComponent<GridLayoutGroup>();

            var parentCanvas = resultCardContainer.GetComponentInParent<Canvas>();
            int baseSortingLayer = parentCanvas != null ? parentCanvas.sortingLayerID : 0;
            int baseSortingOrder = parentCanvas != null ? parentCanvas.sortingOrder + 10 : 100;

            if (isMulti)
            {
                if (hlg == null) hlg = resultCardContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
                if (glg != null) DestroyImmediate(glg);

                targetWidth = multiCardWidth;
                targetHeight = multiCardHeight;
                hlg.spacing = multiCardSpacing; // Overlap for deck effect (adjusted for bigger cards)
                hlg.childAlignment = TextAnchor.MiddleCenter;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;

                // Thêm ContentSizeFitter để tự động tính toán lại kích thước Content (bắt buộc để ScrollRect hoạt động)
                var fitter = resultCardContainer.GetComponent<ContentSizeFitter>();
                if (fitter == null) fitter = resultCardContainer.gameObject.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

                // Configure ScrollRect for dragging
                var scrollRect = resultCardContainer.parent.GetComponent<ScrollRect>();
                if (scrollRect == null) 
                {
                    scrollRect = resultCardContainer.parent.gameObject.AddComponent<ScrollRect>();
                }
                scrollRect.content = resultCardContainer.GetComponent<RectTransform>();
                scrollRect.horizontal = true;
                scrollRect.vertical = false;
                scrollRect.movementType = ScrollRect.MovementType.Clamped; // Fix shaking at edges
                scrollRect.inertia = true;

                // Thêm logic Highlight thẻ ở giữa màn hình (CoverFlow)
                var carousel = resultCardContainer.GetComponent<CoverFlowCarousel>();
                if (carousel == null) carousel = resultCardContainer.gameObject.AddComponent<CoverFlowCarousel>();
                carousel.baseSortingLayer = baseSortingLayer;
                carousel.baseSortingOrder = baseSortingOrder;
                carousel.centerScale = multiCenterScale;
                carousel.normalScale = multiNormalScale;
            }
            else
            {
                if (glg != null) DestroyImmediate(glg);
                if (hlg == null) hlg = resultCardContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
                hlg.spacing = 20;
                hlg.childAlignment = TextAnchor.MiddleCenter;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;
            }

            foreach (var hero in heroes)
            {
                // Create a Wrapper for LayoutGroup so Card and Glow can overlap freely
                GameObject wrapperObj = new GameObject($"Wrapper_{hero.heroName}");
                wrapperObj.layer = LayerMask.NameToLayer("UI");
                wrapperObj.transform.SetParent(resultCardContainer, false);
                var layout = wrapperObj.AddComponent<LayoutElement>();
                layout.preferredWidth = targetWidth;  
                layout.preferredHeight = targetHeight; 
                
                var canvas = wrapperObj.AddComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingLayerID = baseSortingLayer;
                canvas.sortingOrder = baseSortingOrder;
                // Bỏ GraphicRaycaster để không cản trở việc vuốt của ScrollRect

                GameObject cardObj = null;
                if (resultCardPrefab != null)
                {
                    cardObj = SpawnResultCard(hero);
                    cardObj.transform.SetParent(wrapperObj.transform, false);
                }
                else
                {
                    cardObj = CreateSimpleResultCard(hero, targetWidth, targetHeight, titleSize, statSize);
                    cardObj.transform.SetParent(wrapperObj.transform, false);
                }

                if (cardObj != null)
                {
                    // Create Edge Glow Behind Card
                    GameObject glowObj = new GameObject("EdgeGlow");
                    glowObj.transform.SetParent(wrapperObj.transform, false);
                    glowObj.transform.SetAsFirstSibling(); // Put it just behind the card
                    
                    var glowImg = glowObj.AddComponent<Image>();
                    glowImg.color = new Color(1f, 1f, 1f, 0f); 
                    
                    var rect = glowObj.GetComponent<RectTransform>();
                    rect.anchoredPosition = Vector2.zero;
                    rect.sizeDelta = new Vector2(targetWidth + 25, targetHeight + 25); // Viền bọc ngoài thẻ

                    // Fix Card Anchors inside Wrapper
                    var cRt = cardObj.GetComponent<RectTransform>();
                    if (cRt != null)
                    {
                        cRt.anchoredPosition = Vector2.zero;
                        // For prefabs that might have predefined sizes
                        cRt.sizeDelta = new Vector2(targetWidth, targetHeight);
                    }

                    // Start hidden
                    wrapperObj.transform.localScale = Vector3.zero;

                    // Pop & Flip animation!
                    float duration = 0.4f;
                    float elapsed = 0f;
                    while (elapsed < duration)
                    {
                        elapsed += Time.deltaTime;
                        float t = elapsed / duration;
                        
                        // Easing
                        float easeOut = Mathf.Sin(t * Mathf.PI * 0.5f);
                        float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.3f * (1f - t); // Bounce
                        
                        wrapperObj.transform.localScale = Vector3.one * scale;
                        
                        // Fade in glow based on rarity
                        Color targetGlowColor = GetRarityColor(hero.potential);
                        targetGlowColor.a = Mathf.Lerp(0f, 1f, easeOut);
                        glowImg.color = targetGlowColor;

                        // Flip (Card spins from 270 degrees to 0 around Y axis)
                        float yRot = Mathf.Lerp(270f, 0f, easeOut);
                        wrapperObj.transform.localRotation = Quaternion.Euler(0, yRot, 0);

                        yield return null;
                    }
                    wrapperObj.transform.localScale = Vector3.one;
                    wrapperObj.transform.localRotation = Quaternion.identity;
                    
                    // Let the edge glow pulse indefinitely
                    StartCoroutine(PulseGlowRoutine(glowImg, GetRarityColor(hero.potential)));
                }

                yield return new WaitForSeconds(0.15f); // Delay before next card for a cascade effect
            }
        }

        private IEnumerator PulseGlowRoutine(Image glowImg, Color baseColor)
        {
            if (glowImg == null) yield break;
            while (glowImg != null)
            {
                float alpha = 0.6f + Mathf.PingPong(Time.time * 1.5f, 0.4f);
                Color c = baseColor;
                c.a = alpha;
                glowImg.color = c;
                yield return null;
            }
        }

        private GameObject SpawnResultCard(HeroData hero)
        {
            GameObject card = Instantiate(resultCardPrefab, resultCardContainer);

            // Try to populate using HeroCard component if available
            var heroCard = card.GetComponent<HeroCard>();
            if (heroCard != null)
            {
                heroCard.Setup(hero);
                return card;
            }

            // Fallback: manually set text/image fields
            SetupResultCardManually(card, hero);
            return card;
        }

        private GameObject CreateSimpleResultCard(HeroData hero, float width, float height, float titleSize, float statSize)
        {
            GameObject cardObj = new GameObject($"ResultCard_{hero.heroName}");
            cardObj.transform.SetParent(resultCardContainer, false);

            var rt = cardObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(width, height);

            // Background
            var bg = cardObj.AddComponent<Image>();
            bg.color = GetRarityColor(hero.potential);

            // Name label
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(cardObj.transform, false);
            var nameRt = nameObj.AddComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0, 0);
            nameRt.anchorMax = new Vector2(1, 0.2f);
            nameRt.offsetMin = Vector2.zero;
            nameRt.offsetMax = Vector2.zero;

            var nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
            nameTMP.text = hero.heroName;
            nameTMP.fontSize = titleSize; 
            nameTMP.alignment = TextAlignmentOptions.Center;
            nameTMP.color = Color.white;

            // Stats label
            GameObject statsObj = new GameObject("Stats");
            statsObj.transform.SetParent(cardObj.transform, false);
            var statsRt = statsObj.AddComponent<RectTransform>();
            statsRt.anchorMin = new Vector2(0, 0.2f);
            statsRt.anchorMax = new Vector2(1, 0.6f);
            statsRt.offsetMin = new Vector2(width * 0.05f, 0);
            statsRt.offsetMax = new Vector2(-width * 0.05f, 0);

            string rarityLabel = GetRarityLabel(hero.potential);
            var stats = hero.GetFinalStats();

            var statsTMP = statsObj.AddComponent<TextMeshProUGUI>();
            statsTMP.text = $"Rarity: {rarityLabel}\n\n" +
                            $"HP: {stats.hp:F0}\n" +
                            $"ATK: {stats.atk:F0}";
            statsTMP.fontSize = statSize;
            statsTMP.alignment = TextAlignmentOptions.Center;
            statsTMP.color = Color.white;

            // Profession label
            GameObject profObj = new GameObject("Profession");
            profObj.transform.SetParent(cardObj.transform, false);
            var profRt = profObj.AddComponent<RectTransform>();
            profRt.anchorMin = new Vector2(0, 0.6f);
            profRt.anchorMax = new Vector2(1, 0.8f);
            profRt.offsetMin = Vector2.zero;
            profRt.offsetMax = Vector2.zero;

            var profTMP = profObj.AddComponent<TextMeshProUGUI>();
            profTMP.text = hero.profession.ToString();
            profTMP.fontSize = 14;
            profTMP.alignment = TextAlignmentOptions.Center;
            profTMP.color = Color.yellow;

            // Avatar
            if (hero.GetAvatarSprite() != null)
            {
                GameObject avatarObj = new GameObject("Avatar");
                avatarObj.transform.SetParent(cardObj.transform, false);
                var avatarRt = avatarObj.AddComponent<RectTransform>();
                avatarRt.anchorMin = new Vector2(0.15f, 0.75f);
                avatarRt.anchorMax = new Vector2(0.85f, 1f);
                avatarRt.offsetMin = Vector2.zero;
                avatarRt.offsetMax = Vector2.zero;
                var avatarImg = avatarObj.AddComponent<Image>();
                avatarImg.sprite = hero.GetAvatarSprite();
                avatarImg.preserveAspect = true;
            }

            return cardObj;
        }

        private void SetupResultCardManually(GameObject card, HeroData hero)
        {
            // Try to find common child elements by name
            var nameText = FindChildTMP(card, "Name");
            var levelText = FindChildTMP(card, "Level");
            var potText = FindChildTMP(card, "Potential");
            var profText = FindChildTMP(card, "Profession");
            var avatarImage = FindChildImage(card, "Avatar");

            if (nameText != null) nameText.text = hero.heroName;
            if (levelText != null) levelText.text = $"Lv.{hero.level}";
            if (potText != null) potText.text = $"POT {hero.potential}";
            if (profText != null) profText.text = hero.profession.ToString();
            if (avatarImage != null) avatarImage.sprite = hero.GetAvatarSprite();
        }

        private void CloseResultOverlay()
        {
            if (resultOverlay != null) resultOverlay.SetActive(false);
        }

        #endregion

        #region Helpers

        private void ShowNotification(string locKey)
        {
            string text = LocalizationSystem.GetText(locKey);
            GameManager.Instance.UINotificationManager.ShowNotification(text);
        }

        private TextMeshProUGUI FindChildTMP(GameObject parent, string childName)
        {
            // Bỏ FindChild để tránh crash khi đổi hierarchy. Khuyến khích dùng HeroCard component.
            return parent.GetComponentInChildren<TextMeshProUGUI>();
        }

        private Image FindChildImage(GameObject parent, string childName)
        {
            return parent.GetComponentInChildren<Image>();
        }

        private Color GetRarityColor(int potential)
        {
            if (potential >= 25) return new Color(0f, 1f, 1f, 0.9f);          // SSS-rank: Cyan
            if (potential >= 21) return new Color(1f, 0.2f, 0.2f, 0.9f);      // SS-rank: Red
            if (potential >= 15) return new Color(1f, 0.84f, 0f, 0.9f);       // S-rank: Gold
            if (potential >= 12) return new Color(0.6f, 0.2f, 0.8f, 0.9f);    // A-rank: Purple
            if (potential >= 8) return new Color(0.2f, 0.5f, 1f, 0.9f);       // B-rank: Blue
            if (potential >= 5) return new Color(0.3f, 0.8f, 0.3f, 0.9f);     // C-rank: Green
            return new Color(0.5f, 0.5f, 0.5f, 0.9f);                         // D-rank: Gray
        }

        private string GetRarityLabel(int potential)
        {
            if (potential >= 25) return "👑 SSS 👑";
            if (potential >= 21) return "🔥 SS 🔥";
            if (potential >= 15) return "🌟 S 🌟";
            if (potential >= 12) return "⭐ A ⭐";
            if (potential >= 8)  return "🔸 B 🔸";
            if (potential >= 5)  return "C";
            return "D";
        }

        #endregion
    }

    public class CoverFlowCarousel : MonoBehaviour
    {
        private ScrollRect scrollRect;
        private RectTransform viewport;
        private RectTransform content;
        
        public float centerScale = 1.15f;
        public float normalScale = 0.75f;

        public int baseSortingLayer = 0;
        public int baseSortingOrder = 100;
        
        private bool IsDragging => Input.touchCount > 0 || Input.GetMouseButton(0);

        void Start()
        {
            scrollRect = transform.parent.GetComponent<ScrollRect>();
            if (scrollRect != null) 
            {
                viewport = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>();
                content = scrollRect.content;
            }
        }

        void Update()
        {
            if (scrollRect == null || viewport == null || content == null) return;
            if (transform.childCount == 0) return;

            Vector3[] vCorners = new Vector3[4];
            viewport.GetWorldCorners(vCorners);
            float viewCenterWorldX = (vCorners[0].x + vCorners[2].x) * 0.5f;
            float maxDist = (vCorners[2].x - vCorners[0].x) * 0.5f;

            float minCenterDist = float.MaxValue;
            int centerIndex = 0;

            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                var rt = child.GetComponent<RectTransform>();
                if (rt == null) continue;

                Vector3[] cCorners = new Vector3[4];
                rt.GetWorldCorners(cCorners);
                float childCenterWorldX = (cCorners[0].x + cCorners[2].x) * 0.5f;

                float dist = Mathf.Abs(viewCenterWorldX - childCenterWorldX);
                if (dist < minCenterDist)
                {
                    minCenterDist = dist;
                    centerIndex = i;
                }

                float t = Mathf.Clamp01(1f - (dist / maxDist));
                t = t * t * (3f - 2f * t); // Smoothstep
                
                float scale = Mathf.Lerp(normalScale, centerScale, t);
                
                // Cập nhật scale của thẻ
                child.localScale = Vector3.one * scale;

                // Tự động đẩy thẻ to nhất lên lớp hiển thị trên cùng
                var canvas = child.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.sortingLayerID = baseSortingLayer;
                    canvas.sortingOrder = baseSortingOrder + Mathf.RoundToInt(t * 100);
                }
            }

            // Snapping vào giữa khi người dùng nhả tay và tốc độ cuộn thấp
            if (!IsDragging && Mathf.Abs(scrollRect.velocity.x) < 50f)
            {
                scrollRect.inertia = false; // Tắt trượt tự do để snap mượt
                scrollRect.velocity = Vector2.zero;
                
                var targetChild = transform.GetChild(centerIndex).GetComponent<RectTransform>();
                Vector3[] cCorners = new Vector3[4];
                targetChild.GetWorldCorners(cCorners);
                float childCenterWorldX = (cCorners[0].x + cCorners[2].x) * 0.5f;
                
                float offset = viewCenterWorldX - childCenterWorldX;
                
                if (Mathf.Abs(offset) > 0.5f)
                {
                    Vector3 newPos = content.position;
                    newPos.x += offset * Time.deltaTime * 10f; // Smooth lerp
                    content.position = newPos;
                }
            }
            else if (IsDragging)
            {
                scrollRect.inertia = true; // Bật lại trượt tự do khi người dùng vuốt
            }
        }
    }
}

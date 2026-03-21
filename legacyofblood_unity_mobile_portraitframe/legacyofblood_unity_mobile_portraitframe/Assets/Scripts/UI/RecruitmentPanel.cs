namespace LegendOfBlood
{
    using UnityEngine;
    using UnityEngine.UI;

    public class RecruitmentPanel : UIPanel
    {
        [SerializeField] private Button recruitOneButton;
        [SerializeField] private Button recruitTenButton;
        public Button recruitAdButton; // NEW: Nút quay miễn phí
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            PanelType = UIPanelType.Recruitment;
        }

        protected override void Start()
        {
            base.Start();

            // Set up animated background
            Image bgImage = GetComponent<Image>();
            if (bgImage == null)
            {
                Transform bgTransform = transform.Find("Background");
                if (bgTransform != null) bgImage = bgTransform.GetComponent<Image>();
            }

            if (bgImage != null)
            {
                LegendOfBlood.UI.AnimatedUIBackground animBg = bgImage.gameObject.GetComponent<LegendOfBlood.UI.AnimatedUIBackground>();
                if (animBg == null)
                {
                    animBg = bgImage.gameObject.AddComponent<LegendOfBlood.UI.AnimatedUIBackground>();
                    animBg.resourceFolderPath = "UI/RecruitmentBG";
                    animBg.fps = 24f; 
                }
            }

            if (recruitOneButton != null) recruitOneButton.onClick.AddListener(OnRecruitOne);
            if (recruitTenButton != null) recruitTenButton.onClick.AddListener(OnRecruitTen);
            if (recruitAdButton != null) recruitAdButton.onClick.AddListener(OnRecruitAd);
            if (closeButton != null) closeButton.onClick.AddListener(() => GameManager.Instance.UIManager.GoBack());

            Debug.Log("RecruitmentPanel Initialized");
        }

        private void OnEnable()
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (recruitAdButton != null)
            {
                bool canWatchAd = false;
                if (DataManager.Instance != null && DataManager.Instance.Player != null)
                {
                    canWatchAd = DataManager.Instance.Player.dailyFreeSummonsWatched == 0;
                }
                recruitAdButton.gameObject.SetActive(canWatchAd);
            }
        }

        private void OnRecruitOne()
        {
            Debug.Log("Attempting to recruit 1 hero.");
            if (DataManager.Instance.IsPopulationFull())
            {
                GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("notification_population_full"));
                return;
            }

            // Gacha Logic Payment
            if (GameManager.Instance.InventoryManager.GetItemCount("IT_GACHA_TICKET") >= 1)
            {
                GameManager.Instance.InventoryManager.UseItem("IT_GACHA_TICKET", 1);
            }
            else if (GameManager.Instance.InventoryManager.HasEnoughResources(ResourceType.Diamond, 100))
            {
                GameManager.Instance.InventoryManager.SpendResource(ResourceType.Diamond, 100);
            }
            else
            {
                GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("notification_not_enough_currency"));
                return;
            }

            var newHeroes = GameManager.Instance.RecruitmentSystem.PerformRecruitment(1);
            // TODO: Show hero results UI
            if (newHeroes.Count > 0)
            {
                string msgTemplate = LocalizationSystem.GetText("notification_recruit_success");
                GameManager.Instance.UINotificationManager.ShowNotification(string.Format(msgTemplate, newHeroes[0].heroName));
            }
        }

        private void OnRecruitTen()
        {
            Debug.Log("Attempting to recruit 10 heroes.");
            if (DataManager.Instance.IsPopulationFull())
            {
                GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("notification_population_full"));
                return;
            }

            // Gacha Logic Payment
            if (GameManager.Instance.InventoryManager.GetItemCount("IT_GACHA_TICKET") >= 10)
            {
                GameManager.Instance.InventoryManager.UseItem("IT_GACHA_TICKET", 10);
            }
            else if (GameManager.Instance.InventoryManager.HasEnoughResources(ResourceType.Diamond, 900))
            {
                GameManager.Instance.InventoryManager.SpendResource(ResourceType.Diamond, 900);
            }
            else
            {
                GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("notification_not_enough_currency"));
                return;
            }

            var newHeroes = GameManager.Instance.RecruitmentSystem.PerformRecruitment(10);
            // TODO: Show hero results UI
            if (newHeroes.Count > 0)
            {
                string msgTemplate = LocalizationSystem.GetText("notification_recruit_multi_success");
                GameManager.Instance.UINotificationManager.ShowNotification(string.Format(msgTemplate, newHeroes.Count));
            }
        }

        private void OnRecruitAd()
        {
            Debug.Log("Attempting to recruit 1 hero via Ad.");
            if (DataManager.Instance.IsPopulationFull())
            {
                GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("notification_population_full"));
                return;
            }

            if (LegendOfBlood.Managers.AdRewardGateway.Instance != null)
            {
                LegendOfBlood.Managers.AdRewardGateway.Instance.RequestAd(LegendOfBlood.Managers.RewardType.DailySummon, () => {
                    var newHeroes = GameManager.Instance.RecruitmentSystem.PerformRecruitment(1);
                    if (newHeroes.Count > 0)
                    {
                        string msgTemplate = LocalizationSystem.GetText("notification_recruit_success");
                        GameManager.Instance.UINotificationManager.ShowNotification(string.Format(msgTemplate, newHeroes[0].heroName));
                    }
                    RefreshUI();
                });
            }
        }

        private void OnDestroy()
        {
            if (recruitOneButton != null) recruitOneButton.onClick.RemoveListener(OnRecruitOne);
            if (recruitTenButton != null) recruitTenButton.onClick.RemoveListener(OnRecruitTen);
            if (recruitAdButton != null) recruitAdButton.onClick.RemoveListener(OnRecruitAd);
        }
    }
}

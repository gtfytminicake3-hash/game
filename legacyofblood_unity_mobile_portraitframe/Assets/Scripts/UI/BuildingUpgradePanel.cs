namespace LegendOfBlood
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class BuildingUpgradePanel : UIPanel
    {
        [Header("UI References")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI infoText;
        [SerializeField] private TextMeshProUGUI costText;
        public TextMeshProUGUI upgradeTimerText;
        public TextMeshProUGUI benefitText;
        public Button speedUpAdButton; // NEW: Nút tắt thời gian bằng Ad


        private string currentBuildingId;

        private void Awake()
        {
            PanelType = UIPanelType.BuildingUpgrade;
            if (closeButton != null) 
            {
                closeButton.onClick.AddListener(() => GameManager.Instance.UIManager.HidePanel(PanelType));
                var closeText = closeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (closeText != null) closeText.text = global::LocalizationSystem.GetText("btn_close");
            }
            if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgradeClicked);
            if (speedUpAdButton != null) speedUpAdButton.onClick.AddListener(OnSpeedUpAdClicked);
        }

        public void Setup(string buildingId)
        {
            Debug.Log($"[BuildingUpgradePanel] Setup called with ID: '{buildingId}'");
            currentBuildingId = buildingId;
            RefreshUI();
        }

        private void Update()
        {
            if (string.IsNullOrEmpty(currentBuildingId)) return;
            var building = System.Linq.Enumerable.FirstOrDefault(DataManager.Instance.AllBuildings, b => b.id == currentBuildingId);
            if (building != null && building.isUnderConstruction)
            {
                long remainingMillis = building.constructionEndTime - System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (remainingMillis <= 0)
                {
                    // Optionally refresh when complete, but BuildingSystem handles completion.
                    if (upgradeTimerText != null) upgradeTimerText.text = global::LocalizationSystem.GetText("ready");
                }
                else
                {
                    System.TimeSpan ts = System.TimeSpan.FromMilliseconds(remainingMillis);
                    if (upgradeTimerText != null)
                    {
                        upgradeTimerText.gameObject.SetActive(true);
                        string fmt = global::LocalizationSystem.GetText("time_left_format");
                        if (string.IsNullOrEmpty(fmt) || fmt == "time_left_format") fmt = "{0}";
                        string timeString = string.Format("{0:D2}:{1:D2}:{2:D2}", ts.Hours, ts.Minutes, ts.Seconds);
                        upgradeTimerText.text = string.Format(fmt, timeString);
                    }
                }
            }
            else
            {
                if (upgradeTimerText != null) upgradeTimerText.gameObject.SetActive(false);
            }
        }

        private void RefreshUI()
        {
            var building = System.Linq.Enumerable.FirstOrDefault(DataManager.Instance.AllBuildings, b => b.id == currentBuildingId);
            if (building == null)
            {
                Debug.LogWarning($"[BuildingUpgradePanel] Cannot find building with ID: '{currentBuildingId}' in DataManager");
                return;
            }

            if (titleText != null) titleText.text = string.Format(global::LocalizationSystem.GetText("building_upgrade_title_format"), global::LocalizationSystem.GetText($"building_{building.id}"));
            if (infoText != null) infoText.text = string.Format(global::LocalizationSystem.GetText("building_upgrade_info_format"), building.level, building.level + 1);
            
            var upgradeConfig = DataManager.Instance.GetBuildingUpgradeData(currentBuildingId);
            if (upgradeConfig == null)
            {
                Debug.LogWarning($"[BuildingUpgradePanel] No upgrade configuration found for ID: '{currentBuildingId}'");
                return;
            }

            var levelData = upgradeConfig.GetLevelData(building.level + 1);
            if (levelData == null) 
            {
                Debug.Log($"[BuildingUpgradePanel] Max level reached for '{currentBuildingId}'. Hiding upgrade button.");
                if (costText != null) costText.text = global::LocalizationSystem.GetText("building_max_level");
                if (upgradeButton != null) upgradeButton.gameObject.SetActive(false);
                return;
            }

            var goldCost = System.Linq.Enumerable.FirstOrDefault(levelData.costs, c => c.resourceId.ToLower() == "gold");
            int costGold = goldCost != null ? goldCost.amount : 0;

            var woodCost = System.Linq.Enumerable.FirstOrDefault(levelData.costs, c => c.resourceId.ToLower() == "wood");
            int costWood = woodCost != null ? woodCost.amount : 0;

            var stoneCost = System.Linq.Enumerable.FirstOrDefault(levelData.costs, c => c.resourceId.ToLower() == "stone");
            int costStone = stoneCost != null ? stoneCost.amount : 0;

            string costString = "";
            if (costGold > 0) costString += $"Vàng: {costGold}\n";
            if (costWood > 0) costString += $"Gỗ: {costWood}\n";
            if (costStone > 0) costString += $"Đá: {costStone}";

            if (costText != null) costText.text = costString;
            
            bool canUpgrade = GameManager.Instance.InventoryManager.HasEnoughResources(ResourceType.Gold, costGold)
                           && GameManager.Instance.InventoryManager.HasEnoughResources(ResourceType.Wood, costWood)
                           && GameManager.Instance.InventoryManager.HasEnoughResources(ResourceType.Stone, costStone);
            if (upgradeButton != null) 
            {
                upgradeButton.gameObject.SetActive(!building.isUnderConstruction);
                upgradeButton.interactable = canUpgrade;
                var upgText = upgradeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (upgText != null) upgText.text = global::LocalizationSystem.GetText("btn_upgrade");
            }
            
            // Hiện nút Ad Speed Up nếu đang nâng cấp
            if (speedUpAdButton != null)
            {
                speedUpAdButton.gameObject.SetActive(building.isUnderConstruction);
            }

            // Benefits Logic
            if (benefitText != null)
            {
                benefitText.gameObject.SetActive(true);
                string benefitStr = "";
                if (building.id == "TownHall" || building.id == "Barrack" || building.type == BuildingType.TownHall || building.type == BuildingType.Barracks)
                {
                    int currentPop = 50 + ((building.level > 0 ? building.level - 1 : 0) * 5);
                    int nextPop = 50 + (((building.level + 1) > 0 ? (building.level + 1) - 1 : 0) * 5);
                    benefitStr = $"Sức chứa Dân Số: {currentPop} -> {nextPop}";
                }
                else if (building.id == "Hospital" || building.type == BuildingType.Hospital)
                {
                    benefitStr = $"Tăng tốc độ Hồi Phục -> Cấp {building.level + 1}";
                }
                else if (building.id == "BreedingPen" || building.type == BuildingType.BreedingPen)
                {
                    benefitStr = $"Mở khoá bậc Lai Tạo -> Cấp {building.level + 1}";
                }
                else
                {
                    benefitStr = $"Cải thiện Tính năng -> Cấp {building.level + 1}";
                }
                benefitText.text = benefitStr;
            }
        }

        private void OnUpgradeClicked()
        {
            if (GameManager.Instance.BuildingSystem.StartUpgrade(currentBuildingId))
            {
                GameManager.Instance.UINotificationManager.ShowNotification(string.Format(global::LocalizationSystem.GetText("building_upgrade_started_format"), global::LocalizationSystem.GetText($"building_{currentBuildingId}")));
                RefreshUI();
            }
        }

        private void OnSpeedUpAdClicked()
        {
            if (LegendOfBlood.Managers.AdRewardGateway.Instance != null)
            {
                LegendOfBlood.Managers.AdRewardGateway.Instance.RequestAd(LegendOfBlood.Managers.RewardType.BuildingSpeedUp, () => {
                    // Giảm 20 phút (1,200,000 ms)
                    GameManager.Instance.BuildingSystem.SpeedUpConstructionMs(currentBuildingId, 1200000);
                    RefreshUI();
                });
            }
        }
    }
}

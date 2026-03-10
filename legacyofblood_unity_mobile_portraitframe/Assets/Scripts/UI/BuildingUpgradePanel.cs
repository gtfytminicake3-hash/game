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
        }

        public void Setup(string buildingId)
        {
            Debug.Log($"[BuildingUpgradePanel] Setup called with ID: '{buildingId}'");
            currentBuildingId = buildingId;
            RefreshUI();
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

            if (costText != null) costText.text = string.Format(global::LocalizationSystem.GetText("building_upgrade_cost_format"), costGold);
            
            bool canUpgrade = GameManager.Instance.InventoryManager.HasEnoughResources(ResourceType.Gold, costGold);
            if (upgradeButton != null) 
            {
                upgradeButton.interactable = canUpgrade && !building.isUnderConstruction;
                var upgText = upgradeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (upgText != null) upgText.text = global::LocalizationSystem.GetText("btn_upgrade");
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
    }
}

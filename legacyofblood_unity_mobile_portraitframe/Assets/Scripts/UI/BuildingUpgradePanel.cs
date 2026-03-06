namespace LegendOfBlood
{
    using UnityEngine;
    using UnityEngine.UI;

    public class BuildingUpgradePanel : UIPanel
    {
        [Header("UI References")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private UnityEngine.UI.Text titleText;
        [SerializeField] private UnityEngine.UI.Text infoText;
        [SerializeField] private UnityEngine.UI.Text costText;

        private string currentBuildingId;

        private void Awake()
        {
            PanelType = UIPanelType.BuildingUpgrade;
            if (closeButton != null) closeButton.onClick.AddListener(() => GameManager.Instance.UIManager.HidePanel(PanelType));
            if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }

        public void Setup(string buildingId)
        {
            currentBuildingId = buildingId;
            RefreshUI();
        }

        private void RefreshUI()
        {
            var building = System.Linq.Enumerable.FirstOrDefault(DataManager.Instance.AllBuildings, b => b.id == currentBuildingId);
            if (building == null) return;

            if (titleText != null) titleText.text = $"Nâng cấp {building.id}";
            if (infoText != null) infoText.text = $"Cấp độ hiện tại: {building.level}\nCấp độ tiếp theo: {building.level + 1}";
            
            var upgradeConfig = DataManager.Instance.GetBuildingUpgradeData(currentBuildingId);
            if (upgradeConfig == null) return;
            var levelData = upgradeConfig.GetLevelData(building.level + 1);
            if (levelData == null) 
            {
                if (costText != null) costText.text = "Đã đạt cấp tối đa";
                if (upgradeButton != null) upgradeButton.gameObject.SetActive(false);
                return;
            }

            var goldCost = System.Linq.Enumerable.FirstOrDefault(levelData.costs, c => c.resourceId.ToLower() == "gold");
            int costGold = goldCost != null ? goldCost.amount : 0;

            if (costText != null) costText.text = $"Giá: {costGold} Vàng";
            
            bool canUpgrade = GameManager.Instance.InventoryManager.HasEnoughResources(ResourceType.Gold, costGold);
            if (upgradeButton != null) upgradeButton.interactable = canUpgrade && !building.isUnderConstruction;
        }

        private void OnUpgradeClicked()
        {
            if (GameManager.Instance.BuildingSystem.StartUpgrade(currentBuildingId))
            {
                GameManager.Instance.UINotificationManager.ShowNotification($"Đã yêu cầu cấu trúc {currentBuildingId}");
                RefreshUI();
            }
        }
    }
}

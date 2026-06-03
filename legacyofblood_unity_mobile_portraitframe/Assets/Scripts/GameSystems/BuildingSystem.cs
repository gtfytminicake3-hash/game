namespace LegendOfBlood
{
    using System;
    using System.Linq;
    using UnityEngine;
    using LegendOfBlood.GameConfigs;

    public class BuildingSystem
    {
        public static BuildingSystem Instance => GameManager.Instance != null ? GameManager.Instance.BuildingSystem : null;

        public static event Action<Building> OnBuildingUpgradeStarted;
        public static event Action<Building> OnBuildingUpgradeCompleted;

        public bool StartUpgrade(string buildingId)
        {
            var building = DataManager.Instance.AllBuildings.FirstOrDefault(b => b.id == buildingId);

            if (building == null)
            {
                Debug.LogError($"Building with ID: {buildingId} not found for the player.");
                return false;
            }

            if (building.isUnderConstruction)
            {
                GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("notification_building_already_upgrading"));
                return false;
            }

            int nextLevel = building.level + 1;

            // REFACTOR: Use BuildingUpgradeData ScriptableObject
            BuildingUpgradeData upgradeConfig = DataManager.Instance.GetBuildingUpgradeData(building.id);
            if (upgradeConfig == null)
            {
                Debug.LogError($"No upgrade configuration found for building ID: {building.id}");
                return false;
            }

            var levelData = upgradeConfig.GetLevelData(nextLevel);
            if (levelData == null)
            {
                GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("notification_building_max_level"));
                return false;
            }

            // REFACTOR: Check and spend resources based on the new data structure
            if (!CanAfford(levelData.costs))
            {
                GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("notification_not_enough_resources"));
                return false;
            }
            
            foreach (var cost in levelData.costs)
            {
                // SỬA LỖI: Chuyển đổi string ID thành enum ResourceType một cách an toàn
                if (Enum.TryParse<ResourceType>(cost.resourceId, true, out var resourceType))
                {
                    InventoryManager.Instance.SpendResource(resourceType, cost.amount);
                }
            }
            
            long durationMs = levelData.duration * 1000;
            building.isUnderConstruction = true;
            building.constructionEndTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + durationMs;

            Debug.Log($"Starting upgrade for {building.id} to level {nextLevel}. Completion in {levelData.duration} seconds.");
            
            OnBuildingUpgradeStarted?.Invoke(building);
            return true;
        }

        public bool SpeedUpConstruction(string buildingId, string itemId)
        {
            var building = DataManager.Instance.AllBuildings.FirstOrDefault(b => b.id == buildingId);
            if (building == null || !building.isUnderConstruction) return false;

            var item = DataManager.Instance.AllItems.TryGetValue(itemId, out var itemData) ? itemData : null;
            if (item == null || item.type != ItemType.SpeedUp) return false;

            // In GameManager context, typically we use GameManager.Instance.InventoryManager
            bool success = GameManager.Instance.InventoryManager.UseItem(itemId, 1);
            if (success)
            {
                long speedUpMs = item.speedUpValueInSeconds * 1000;
                building.constructionEndTime -= speedUpMs;
                Debug.Log($"Building {building.id} construction sped up by {item.speedUpValueInSeconds}s using {item.itemName}.");
                return true;
            }
            return false;
        }

        public bool SpeedUpConstructionMs(string buildingId, long timeInMs)
        {
            var building = DataManager.Instance.AllBuildings.FirstOrDefault(b => b.id == buildingId);
            if (building == null || !building.isUnderConstruction) return false;

            building.constructionEndTime -= timeInMs;
            Debug.Log($"Building {building.id} construction sped up by {timeInMs}ms.");
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (DataManager.Instance == null) return;
            var buildings = DataManager.Instance.AllBuildings;
            if (buildings == null || buildings.Count == 0) return;

            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Use a copy to avoid modification issues during iteration
            foreach (var building in buildings.ToList())
            {
                if (building.isUnderConstruction && currentTime >= building.constructionEndTime)
                {
                    CompleteConstruction(building);
                }
            }
        }

        private void CompleteConstruction(Building building)
        {
            building.isUnderConstruction = false;
            building.level++;
            
            Debug.Log($"<color=green>Upgrade Complete!</color> {building.id} has been upgraded to level {building.level}.");
            OnBuildingUpgradeCompleted?.Invoke(building);
        }

        // REFACTOR: Updated CanAfford to use the new UpgradeCost list
        private bool CanAfford(System.Collections.Generic.List<UpgradeCost> costs)
        {
            if (costs == null) return true; // If no costs are defined, we can afford it

            foreach (var cost in costs)
            {
                // SỬA LỖI: Chuyển đổi string ID thành enum ResourceType một cách an toàn
                if (Enum.TryParse<ResourceType>(cost.resourceId, true, out var resourceType))
                {
                    if (InventoryManager.Instance.GetResourceAmount(resourceType) < cost.amount)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}

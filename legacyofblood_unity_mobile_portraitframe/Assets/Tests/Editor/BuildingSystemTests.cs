using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using LegendOfBlood;
using LegendOfBlood.GameConfigs;
using System;

namespace LegendOfBlood.Tests
{
    [TestFixture]
    public class BuildingSystemTests
    {
        private BuildingSystem _buildingSystem;
        private GameObject _dataManagerObj;
        private DataManager _dataManager;
        private GameObject _inventoryManagerObj;
        private InventoryManager _inventoryManager;

        [SetUp]
        public void Setup()
        {
            var avatarManagerObj = new UnityEngine.GameObject("TestAvatarManager");
            avatarManagerObj.AddComponent<LegendOfBlood.AvatarManager>();
            _buildingSystem = new BuildingSystem();

            _dataManagerObj = new GameObject("TestDataManager");
            _dataManager = _dataManagerObj.AddComponent<DataManager>();

            _inventoryManagerObj = new GameObject("TestInventoryManager");
            _inventoryManager = _inventoryManagerObj.AddComponent<InventoryManager>();

            var gameConfig = ScriptableObject.CreateInstance<GameConfig>();
            gameConfig.EvolutionTable = new LegendOfBlood.GameConfigs.EvolutionTableData();
            gameConfig.EvolutionTable.rewards = new System.Collections.Generic.List<LegendOfBlood.GameConfigs.EvolutionRewardData>();
            gameConfig.AllTraits = new List<Trait>();
            gameConfig.AllSkills = new List<Skill>();
            gameConfig.ExperienceTable = new List<ExperienceData>();
            
            // Setup building mock data
            
            var buildingUpgradeData = ScriptableObject.CreateInstance<BuildingUpgradeData>();
            buildingUpgradeData.buildingId = "B_TAVERN";
            buildingUpgradeData.levels = new List<BuildingLevelData>
            {
                    new BuildingLevelData
                    {
                        level = 1,
                        duration = 3600,
                        costs = new List<UpgradeCost> { new UpgradeCost { resourceId = "Gold", amount = 500 } }
                    },
                    new BuildingLevelData
                    {
                        level = 2,
                        duration = 60, // 60 seconds
                        costs = new List<UpgradeCost> { new UpgradeCost { resourceId = "Gold", amount = 500 } }
                    }
            };
            
            gameConfig.BuildingUpgradeDataList = new List<BuildingUpgradeData> { buildingUpgradeData };

            var configField = typeof(DataManager).GetField("_gameConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (configField != null) configField.SetValue(_dataManager, gameConfig);

            typeof(DataManager).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).SetValue(null, _dataManager);
            _dataManager.InitializeDataManager();
            
            // Use reflection to set the private setter of AllBuildings
            var allBuildingsField = typeof(DataManager).GetProperty("AllBuildings", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (allBuildingsField != null) allBuildingsField.SetValue(_dataManager, new List<Building> { new Building { id = "B_TAVERN", level = 1, isUnderConstruction = false } });

            // Ensure player has gold for upgrading
            _inventoryManager.AddResource(ResourceType.Gold, 1000);
        }

        [TearDown]
        public void Teardown()
        {
            if (_dataManagerObj != null) UnityEngine.Object.DestroyImmediate(_dataManagerObj);
            if (_inventoryManagerObj != null) UnityEngine.Object.DestroyImmediate(_inventoryManagerObj);
        }

        [Test]
        public void BuildingSystem_StartUpgrade_DeductsResourcesAndSetsTimer()
        {
            int initialGold = _inventoryManager.GetResourceAmount(ResourceType.Gold);
            
            bool success = _buildingSystem.StartUpgrade("B_TAVERN");

            Assert.IsTrue(success, "Upgrade should start successfully if resources are met.");
            
            var building = _dataManager.AllBuildings.Find(b => b.id == "B_TAVERN");
            Assert.IsTrue(building.isUnderConstruction, "Building should be marked as under construction.");
            Assert.IsTrue(_inventoryManager.GetResourceAmount(ResourceType.Gold) < initialGold, "Gold should be deducted.");
        }

        [Test]
        public void BuildingSystem_Tick_CompletesConstructionOverTime()
        {
            var building = _dataManager.AllBuildings.Find(b => b.id == "B_TAVERN");
            building.isUnderConstruction = true;
            building.level = 1;
            // Force construction to finish immediately
            building.constructionEndTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 1000;

            _buildingSystem.Tick(1.0f);

            Assert.IsFalse(building.isUnderConstruction, "Building construction flag should be cleared.");
            Assert.AreEqual(2, building.level, "Building level should be increased by 1.");
        }
    }
}

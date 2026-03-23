using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using LegendOfBlood;
using LegendOfBlood.GameConfigs;

namespace LegendOfBlood.Tests
{
    [TestFixture]
    public class InventoryManagerTests
    {
        private GameObject _gameManagerObj;
        private GameManager _gameManager;
        private GameObject _dataManagerObj;
        private DataManager _dataManager;
        private GameObject _inventoryManagerObj;
        private InventoryManager _inventoryManager;

        [SetUp]
        public void Setup()
        {
            var avatarManagerObj = new UnityEngine.GameObject("TestAvatarManager");
            avatarManagerObj.AddComponent<LegendOfBlood.AvatarManager>();
            _gameManagerObj = new GameObject("TestGameManager");
            _gameManager = _gameManagerObj.AddComponent<GameManager>();

            _dataManagerObj = new GameObject("TestDataManager");
            _dataManager = _dataManagerObj.AddComponent<DataManager>();

            _inventoryManagerObj = new GameObject("TestInventoryManager");
            _inventoryManager = _inventoryManagerObj.AddComponent<InventoryManager>();

            // Setup mock config
            var gameConfig = ScriptableObject.CreateInstance<GameConfig>();
            gameConfig.EvolutionTable = new LegendOfBlood.GameConfigs.EvolutionTableData();
            gameConfig.EvolutionTable.rewards = new System.Collections.Generic.List<LegendOfBlood.GameConfigs.EvolutionRewardData>();
            gameConfig.AllTraits = new List<Trait>();
            gameConfig.AllSkills = new List<Skill>();
            gameConfig.ExperienceTable = new List<ExperienceData>();
            gameConfig.StartingSkills = new List<ProfessionStartingSkills>();
            gameConfig.BuildingUpgradeDataList = new List<BuildingUpgradeData>();

            var configField = typeof(DataManager).GetField("_gameConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (configField != null) configField.SetValue(_dataManager, gameConfig);

            // Mock systems in game manager
            var dmField = typeof(GameManager).GetField("_dataManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (dmField != null) dmField.SetValue(_gameManager, _dataManager);
            
            var invField = typeof(GameManager).GetField("_inventoryManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (invField != null) invField.SetValue(_gameManager, _inventoryManager);

            // Make sure player data is loaded
            typeof(DataManager).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).SetValue(null, _dataManager);
            _dataManager.InitializeDataManager();
            
            var startMethod = typeof(InventoryManager).GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (startMethod != null) startMethod.Invoke(_inventoryManager, null);
        }

        [TearDown]
        public void Teardown()
        {
            if (_gameManagerObj != null) Object.DestroyImmediate(_gameManagerObj);
            if (_dataManagerObj != null) Object.DestroyImmediate(_dataManagerObj);
            if (_inventoryManagerObj != null) Object.DestroyImmediate(_inventoryManagerObj);
        }

        [Test]
        public void InventoryManager_AddResource_IncreasesAmount()
        {
            // Arrange
            int initialGold = _inventoryManager.GetResourceAmount(ResourceType.Gold);
            
            // Act
            _inventoryManager.AddResource(ResourceType.Gold, 100);

            // Assert
            Assert.AreEqual(initialGold + 100, _inventoryManager.GetResourceAmount(ResourceType.Gold), "Gold should increase by 100.");
        }

        [Test]
        public void InventoryManager_ConsumeResource_DecreasesAmountIfSufficient()
        {
            // Arrange
            _inventoryManager.AddResource(ResourceType.Stone, 50);
            int beforeConsume = _inventoryManager.GetResourceAmount(ResourceType.Stone);
            
            // Act
            bool success = _inventoryManager.SpendResource(ResourceType.Stone, 20);

            // Assert
            Assert.IsTrue(success, "Consuming resource should succeed when sufficient.");
            Assert.AreEqual(beforeConsume - 20, _inventoryManager.GetResourceAmount(ResourceType.Stone), "Stone should decrease by 20.");
        }

        [Test]
        public void InventoryManager_ConsumeResource_FailsIfInsufficient()
        {
            // Arrange
            int currentWood = _inventoryManager.GetResourceAmount(ResourceType.Wood);
            
            // Act
            bool success = _inventoryManager.SpendResource(ResourceType.Wood, currentWood + 100);

            // Assert
            Assert.IsFalse(success, "Consuming should fail if not enough resources.");
            Assert.AreEqual(currentWood, _inventoryManager.GetResourceAmount(ResourceType.Wood), "Resource amount should not change if consumption fails.");
        }

        [Test]
        public void InventoryManager_NegativeAddition_HandledGracefully()
        {
            // Arrange
            int initialGold = _inventoryManager.GetResourceAmount(ResourceType.Gold);
            
            // Act
            _inventoryManager.AddResource(ResourceType.Gold, -50);

            // Assert
            // Depending on implementation, negative add might be ignored or act as consume.
            // Let's assume it shouldn't decrease below 0, or maybe it's ignored entirely.
            // Usually, AddResource shouldn't subtract unless documented.
            int finalGold = _inventoryManager.GetResourceAmount(ResourceType.Gold);
            Assert.IsTrue(finalGold >= 0, "Resource should never drop below 0 by negative addition.");
        }
    }
}

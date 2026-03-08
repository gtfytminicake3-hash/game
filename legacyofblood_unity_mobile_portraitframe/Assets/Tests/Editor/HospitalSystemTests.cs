using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using LegendOfBlood;
using LegendOfBlood.GameConfigs;
using System;

namespace LegendOfBlood.Tests
{
    [TestFixture]
    public class HospitalSystemTests
    {
        private HospitalSystem _hospitalSystem;
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
            _hospitalSystem = new HospitalSystem();

            _gameManagerObj = new GameObject("TestGameManager");
            _gameManager = _gameManagerObj.AddComponent<GameManager>();

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
            gameConfig.StartingSkills = new List<ProfessionStartingSkills>();
            gameConfig.BuildingUpgradeDataList = new List<BuildingUpgradeData>();

            var configField = typeof(DataManager).GetField("_gameConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (configField != null) configField.SetValue(_dataManager, gameConfig);

            var dmField = typeof(GameManager).GetField("_dataManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (dmField != null) dmField.SetValue(_gameManager, _dataManager);

            var invField = typeof(GameManager).GetField("_inventoryManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (invField != null) invField.SetValue(_gameManager, _inventoryManager);

            typeof(DataManager).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).SetValue(null, _dataManager);
            _dataManager.InitializeDataManager();
            
            // Give some gold for healing tests
            _inventoryManager.AddResource(ResourceType.Gold, 2000);
            _dataManager.Player.Heroes.Clear();
        }

        [TearDown]
        public void Teardown()
        {
            if (_gameManagerObj != null) UnityEngine.Object.DestroyImmediate(_gameManagerObj);
            if (_dataManagerObj != null) UnityEngine.Object.DestroyImmediate(_dataManagerObj);
            if (_inventoryManagerObj != null) UnityEngine.Object.DestroyImmediate(_inventoryManagerObj);
        }

        [Test]
        public void HospitalSystem_AdmitHero_SetsSevereInjury()
        {
            HeroData hero = new HeroData("H_1", "Test", Gender.Male);
            _dataManager.AddHero(hero);

            _hospitalSystem.AdmitHero(hero.id);

            Assert.IsTrue(hero.isSeverelyInjured, "Hero should be severely injured after admission.");
            Assert.IsTrue(hero.injuryEndTime > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "Injury end time should be set in the future.");
        }

        [Test]
        public void HospitalSystem_HealLightInjuryInstantly_SpendsGold()
        {
            HeroData hero = new HeroData("H_1", "Test", Gender.Male) { level = 10, potential = 15 };
            _hospitalSystem.InflictLightInjury(hero);
            
            int initialGold = _inventoryManager.GetResourceAmount(ResourceType.Gold);
            
            bool success = _hospitalSystem.HealLightInjuryInstantly(hero);

            Assert.IsTrue(success, "Healing should succeed if there is enough gold.");
            Assert.IsFalse(hero.isLightlyInjured, "Light injury flag should be cleared.");
            Assert.IsTrue(_inventoryManager.GetResourceAmount(ResourceType.Gold) < initialGold, "Gold should be deducted.");
        }

        [Test]
        public void HospitalSystem_SevereInjuryTimeout_HeroPerishes()
        {
            HeroData hero = new HeroData("H_DEAD", "Doomed", Gender.Male);
            _dataManager.AddHero(hero);
            _hospitalSystem.AdmitForSevereInjury(hero);

            // Force the timeout to the past to simulate time passing
            hero.injuryEndTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 1000;

            // Tick the hospital system
            LogAssert.Expect(LogType.Error, "Doomed đã không được cứu chữa kịp thời và biến mất vĩnh viễn!");
            _hospitalSystem.Tick(1.0f);

            Assert.IsNull(_dataManager.GetHeroByID("H_DEAD"), "Hero should be removed from DataManager after perishing.");
        }
    }
}

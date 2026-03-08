using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using LegendOfBlood;
using LegendOfBlood.Combat;
using LegendOfBlood.GameConfigs;
using System;

namespace LegendOfBlood.Tests
{
    [TestFixture]
    public class ExpeditionManagerTests
    {
        private GameObject _gameManagerObj;
        private GameManager _gameManager;
        private GameObject _dataManagerObj;
        private DataManager _dataManager;
        private GameObject _expeditionManagerObj;
        private ExpeditionManager _expeditionManager;

        [SetUp]
        public void Setup()
        {
            var avatarManagerObj = new UnityEngine.GameObject("TestAvatarManager");
            avatarManagerObj.AddComponent<LegendOfBlood.AvatarManager>();
            _gameManagerObj = new GameObject("TestGameManager");
            _gameManager = _gameManagerObj.AddComponent<GameManager>();

            _dataManagerObj = new GameObject("TestDataManager");
            _dataManager = _dataManagerObj.AddComponent<DataManager>();

            _expeditionManagerObj = new GameObject("TestExpeditionManager");
            _expeditionManager = _expeditionManagerObj.AddComponent<ExpeditionManager>();

            var gameConfig = ScriptableObject.CreateInstance<GameConfig>();
            gameConfig.EvolutionTable = new LegendOfBlood.GameConfigs.EvolutionTableData();
            gameConfig.EvolutionTable.rewards = new System.Collections.Generic.List<LegendOfBlood.GameConfigs.EvolutionRewardData>();
            gameConfig.AllTraits = new List<Trait>();
            gameConfig.AllSkills = new List<Skill>();
            gameConfig.ExperienceTable = new List<ExperienceData>();
            gameConfig.StartingSkills = new List<LegendOfBlood.GameConfigs.ProfessionStartingSkills>();
            gameConfig.CombatSettings = new CombatConfig { archerBonusCritChance = 0.2f };
            gameConfig.POIMonsterConfig = ScriptableObject.CreateInstance<POIMonsterConfig>();

            var configField = typeof(DataManager).GetField("_gameConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (configField != null) configField.SetValue(_dataManager, gameConfig);

            var dmField = typeof(GameManager).GetField("_dataManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (dmField != null) dmField.SetValue(_gameManager, _dataManager);

            // Initialize Combat System manually for Expedition Manager to use
            var combatSystem = new CombatSystem(1, new List<Skill>());
            var csProp = typeof(GameManager).GetProperty("CombatSystem", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (csProp != null) csProp.SetValue(_gameManager, combatSystem);

            typeof(DataManager).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).SetValue(null, _dataManager);
            _dataManager.InitializeDataManager();
            
            // Reassign active expeditions correctly since DataManager could be fresh
            _dataManager.Player.ActiveExpeditions.Clear();
            _dataManager.Player.Heroes.Clear();
            _dataManager.Player.UnclaimedReports.Clear();

            // Run Start method using reflection as it's private
            var startMethod = typeof(ExpeditionManager).GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (startMethod != null) startMethod.Invoke(_expeditionManager, null);
        }

        [TearDown]
        public void Teardown()
        {
            if (_gameManagerObj != null) UnityEngine.Object.DestroyImmediate(_gameManagerObj);
            if (_dataManagerObj != null) UnityEngine.Object.DestroyImmediate(_dataManagerObj);
            if (_expeditionManagerObj != null) UnityEngine.Object.DestroyImmediate(_expeditionManagerObj);
        }

        [Test]
        public void ExpeditionManager_StartExpedition_AddsActiveExpedition()
        {
            HeroData hero = new HeroData("H_EXP", "Explorer", Gender.Male) { level = 10, potential = 20 };
            hero.currentHp = hero.GetFinalStats().hp;
            _dataManager.AddHero(hero);

            POIData destination = new POIData
            {
                poiId = "POI_1",
                poiName = "Goblin Cave",
                type = POIType.Dungeon,
                difficultyLevel = 1,
                monsterIDs = new List<string>() // empty monster list means instant win
            };

            _expeditionManager.StartExpedition(new List<string> { hero.id }, destination);

            Assert.AreEqual(1, _dataManager.Player.ActiveExpeditions.Count, "An active expedition should be added to PlayerData.");
            Assert.IsTrue(_expeditionManager.IsHeroOnExpedition(hero.id), "Hero should be marked as being on an expedition.");
        }

        [Test]
        public void ExpeditionManager_Tick_CompletesExpeditionAndDeliversReport()
        {
            HeroData hero = new HeroData("H_EXP2", "Explorer2", Gender.Female);
            _dataManager.AddHero(hero);

            POIData destination = new POIData { poiId = "POI_2", type = POIType.Dungeon, monsterIDs = new List<string>() };
            
            _expeditionManager.StartExpedition(new List<string> { hero.id }, destination);

            // Force time to expire
            _dataManager.Player.ActiveExpeditions[0].completionTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 1000;

            _expeditionManager.Tick();

            Assert.AreEqual(0, _dataManager.Player.ActiveExpeditions.Count, "Active expedition should be removed after completion.");
            Assert.AreEqual(1, _dataManager.Player.UnclaimedReports.Count, "A combat report should be delivered to the mailbox.");
            Assert.IsFalse(_expeditionManager.IsHeroOnExpedition(hero.id), "Hero should no longer be marked as busy.");
        }
    }
}

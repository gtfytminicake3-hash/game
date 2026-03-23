using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using LegendOfBlood;

namespace LegendOfBlood.Tests
{
    [TestFixture]
    public class ArenaSystemTests
    {
        private GameObject _arenaSystemObj;
        private ArenaSystem _arenaSystem;
        private GameObject _dataManagerObj;
        private DataManager _dataManager;

        [SetUp]
        public void Setup()
        {
            var avatarManagerObj = new UnityEngine.GameObject("TestAvatarManager");
            avatarManagerObj.AddComponent<LegendOfBlood.AvatarManager>();
            _arenaSystemObj = new GameObject("TestArenaSystem");
            _arenaSystem = _arenaSystemObj.AddComponent<ArenaSystem>();

            _dataManagerObj = new GameObject("TestDataManager");
            _dataManager = _dataManagerObj.AddComponent<DataManager>();

            var gameConfig = ScriptableObject.CreateInstance<LegendOfBlood.GameConfigs.GameConfig>();
            gameConfig.EvolutionTable = new LegendOfBlood.GameConfigs.EvolutionTableData();
            gameConfig.EvolutionTable.rewards = new System.Collections.Generic.List<LegendOfBlood.GameConfigs.EvolutionRewardData>();
            // Just basic setup to prevent null refs
            gameConfig.AllTraits = new List<LegendOfBlood.Trait>();
            gameConfig.AllSkills = new List<LegendOfBlood.Skill>();
            gameConfig.ExperienceTable = new List<LegendOfBlood.GameConfigs.ExperienceData>();
            gameConfig.BuildingUpgradeDataList = new List<LegendOfBlood.GameConfigs.BuildingUpgradeData>();

            gameConfig.StartingSkills = new List<LegendOfBlood.GameConfigs.ProfessionStartingSkills>();
            gameConfig.EvolutionTable = new LegendOfBlood.GameConfigs.EvolutionTableData();
            gameConfig.EvolutionTable.rewards = new List<LegendOfBlood.GameConfigs.EvolutionRewardData>();
            var configField = typeof(DataManager).GetField("_gameConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (configField != null) configField.SetValue(_dataManager, gameConfig);

            typeof(DataManager).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).SetValue(null, _dataManager);
            _dataManager.InitializeDataManager();
        }

        [TearDown]
        public void Teardown()
        {
            if (_arenaSystemObj != null) Object.DestroyImmediate(_arenaSystemObj);
            if (_dataManagerObj != null) Object.DestroyImmediate(_dataManagerObj);
        }

        [Test]
        public void ArenaSystem_ProcessMatchResult_Win_IncreasesPointsAndCoins()
        {
            _dataManager.Player.arenaPoints = 1000;
            _dataManager.Player.arenaCoins = 0;
            _dataManager.Player.arenaTickets = 5;

            _arenaSystem.ProcessMatchResult(victory: true, playerPoints: 1000, opponentPoints: 1000);

            Assert.AreEqual(4, _dataManager.Player.arenaTickets, "Should deduct 1 ticket.");
            Assert.AreEqual(30, _dataManager.Player.arenaCoins, "Win should grant 30 arena coins.");
            Assert.IsTrue(_dataManager.Player.arenaPoints > 1000, "Win should heavily increase points.");
        }

        [Test]
        public void ArenaSystem_ProcessMatchResult_Loss_DecreasesPoints()
        {
            _dataManager.Player.arenaPoints = 1000;
            _dataManager.Player.arenaCoins = 0;
            _dataManager.Player.arenaTickets = 5;

            _arenaSystem.ProcessMatchResult(victory: false, playerPoints: 1000, opponentPoints: 1000);

            Assert.AreEqual(4, _dataManager.Player.arenaTickets, "Should deduct 1 ticket.");
            Assert.AreEqual(10, _dataManager.Player.arenaCoins, "Loss still grants 10 consolation coins.");
            Assert.IsTrue(_dataManager.Player.arenaPoints < 1000, "Loss should drop arena points.");
        }

        [Test]
        public void ArenaSystem_FindOpponentSquad_ReturnsValidSquad()
        {
            var squad = _arenaSystem.FindOpponentSquad(1500);

            Assert.IsNotNull(squad);
            Assert.AreEqual(5, squad.Count, "Arena squad should always consist of exactly 5 bots.");
            Assert.IsTrue(squad[0].level > 1, "At 1500 points, bots should be scaled up beyond level 1.");
            Assert.IsTrue(squad[0].baseStats.hp > 100, "Stats should be scaled up.");
        }
    }
}

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
    public class RecruitmentSystemTests
    {
        private RecruitmentSystem _recruitmentSystem;
        private GameObject _dataManagerObj;
        private DataManager _dataManager;
        private GameObject _gameManagerObj;
        private GameManager _gameManager;
        private GameObject _notificationManagerObj;
        private UINotificationManager _notificationManager;

        [SetUp]
        public void Setup()
        {
            var avatarManagerObj = new UnityEngine.GameObject("TestAvatarManager");
            avatarManagerObj.AddComponent<LegendOfBlood.AvatarManager>();
            _recruitmentSystem = new RecruitmentSystem();

            _gameManagerObj = new GameObject("TestGameManager");
            _gameManager = _gameManagerObj.AddComponent<GameManager>();

            _dataManagerObj = new GameObject("TestDataManager");
            _dataManager = _dataManagerObj.AddComponent<DataManager>();

            _notificationManagerObj = new GameObject("TestNotificationManager");
            _notificationManager = _notificationManagerObj.AddComponent<UINotificationManager>();

            // Mock GameConfig
            var gameConfig = ScriptableObject.CreateInstance<GameConfig>();
            gameConfig.EvolutionTable = new LegendOfBlood.GameConfigs.EvolutionTableData();
            gameConfig.EvolutionTable.rewards = new System.Collections.Generic.List<LegendOfBlood.GameConfigs.EvolutionRewardData>();
            gameConfig.AllTraits = new List<Trait>
            {
                CreateMockTrait("S_01", Trait.RarityRank.B, "trait_b1"),
                CreateMockTrait("S_02", Trait.RarityRank.A, "trait_a1")
            };

            // Rarity drop rates setup
            gameConfig.RaritySettings = new List<RarityConfig>
            {
                new RarityConfig { rank = Trait.RarityRank.S, dropChance = 10f, minPotential = 17, maxPotential = 20 },
                new RarityConfig { rank = Trait.RarityRank.A, dropChance = 90f, minPotential = 10, maxPotential = 15 } // 100% total
            };

            gameConfig.AllSkills = new List<Skill>();
            gameConfig.ExperienceTable = new List<ExperienceData>();
            gameConfig.StartingSkills = new List<ProfessionStartingSkills>();
            gameConfig.BuildingUpgradeDataList = new List<BuildingUpgradeData>();

            var configField = typeof(DataManager).GetField("_gameConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (configField != null) configField.SetValue(_dataManager, gameConfig);

            // Link managers
            var dmField = typeof(GameManager).GetField("_dataManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (dmField != null) dmField.SetValue(_gameManager, _dataManager);

            var nmField = typeof(GameManager).GetField("_uiNotificationManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (nmField != null) nmField.SetValue(_gameManager, _notificationManager);

            typeof(DataManager).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).SetValue(null, _dataManager);
            _dataManager.InitializeDataManager();
            // Empty heroes list to ensure no population limits are hit naturally
            _dataManager.Player.Heroes.Clear();
        }

        private Trait CreateMockTrait(string id, Trait.RarityRank rank, string name)
        {
            var trait = ScriptableObject.CreateInstance<Trait>();
            trait.id = id;
            trait.rank = rank;
            trait.traitName = name;
            return trait;
        }

        [TearDown]
        public void Teardown()
        {
            if (_gameManagerObj != null) Object.DestroyImmediate(_gameManagerObj);
            if (_dataManagerObj != null) Object.DestroyImmediate(_dataManagerObj);
            if (_notificationManagerObj != null) Object.DestroyImmediate(_notificationManagerObj);
        }

        [Test]
        public void RecruitmentSystem_PerformRecruitment_ReturnsNewHeroes()
        {
            var newHeroes = _recruitmentSystem.PerformRecruitment(2);

            Assert.AreEqual(2, newHeroes.Count, "Should recruit exactly 2 heroes.");
            Assert.IsTrue(newHeroes[0].potential >= 10 && newHeroes[0].potential <= 20, "Potential should be within config bounds.");
            Assert.AreEqual(1, newHeroes[0].level, "New heroes should start at level 1.");
            Assert.IsNotNull(newHeroes[0].traitIDs, "Traits list should be initialized.");
        }

        [Test]
        public void RecruitmentSystem_PopulationFull_StopsRecruitment()
        {
            // Set building level for Barracks to 1, assuming max population config is tied to it.
            // If IsPopulationFull acts on a low limit (e.g., 2 by default in mock data):
            while (!_dataManager.IsPopulationFull())
            {
                _dataManager.AddHero(new HeroData("H_FILL", "Filler", Gender.Male));
                
                // Safety break to prevent infinite loops if limit is too high or buggy
                if (_dataManager.Player.Heroes.Count > 100) break;
            }

            var newHeroes = _recruitmentSystem.PerformRecruitment(1);

            Assert.AreEqual(0, newHeroes.Count, "If population is full, recruitment should return 0 heroes.");
        }

        [Test]
        public void RecruitmentSystem_AssignsCorrectStats()
        {
            var newHeroes = _recruitmentSystem.PerformRecruitment(1);
            var hero = newHeroes[0];

            Assert.IsTrue(hero.baseStats.hp > 0, "Hero should have generated base HP.");
            Assert.IsTrue(hero.baseStats.atk > 0, "Hero should have generated base ATK.");
            Assert.AreEqual(hero.currentHp, hero.GetFinalStats().hp, "Hero should start with max HP.");
        }
    }
}

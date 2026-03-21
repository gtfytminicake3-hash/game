using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using LegendOfBlood;
using LegendOfBlood.GameConfigs;
using System.Linq;

namespace LegendOfBlood.Tests
{
    [TestFixture]
    public class EvolutionSystemTests
    {
        private EvolutionSystem _evolutionSystem;
        private GameObject _dataManagerObj;
        private DataManager _dataManager;

        [SetUp]
        public void Setup()
        {
            var avatarManagerObj = new UnityEngine.GameObject("TestAvatarManager");
            avatarManagerObj.AddComponent<LegendOfBlood.AvatarManager>();
            _evolutionSystem = new EvolutionSystem();

            _dataManagerObj = new GameObject("TestDataManager");
            _dataManager = _dataManagerObj.AddComponent<DataManager>();

            var gameConfig = ScriptableObject.CreateInstance<GameConfig>();
            gameConfig.EvolutionTable = new LegendOfBlood.GameConfigs.EvolutionTableData();
            gameConfig.EvolutionTable.rewards = new System.Collections.Generic.List<LegendOfBlood.GameConfigs.EvolutionRewardData>();
            gameConfig.AllTraits = new List<Trait>
            {
                CreateMockTrait("S_01", "F_01", Trait.RarityRank.S, "trait_s1"),
                CreateMockTrait("A_01", "F_02", Trait.RarityRank.A, "trait_a1")
            };
            
            gameConfig.RaritySettings = new List<RarityConfig>
            {
                new RarityConfig { rank = Trait.RarityRank.S, dropChance = 50f },
                new RarityConfig { rank = Trait.RarityRank.A, dropChance = 50f }
            };

            gameConfig.AllSkills = new List<Skill>();
            gameConfig.ExperienceTable = new List<ExperienceData>();
            for(int i = 1; i <= 100; i++)
            {
                gameConfig.ExperienceTable.Add(new ExperienceData { level = i, experienceRequired = i * 100 });
            }
            gameConfig.StartingSkills = new List<ProfessionStartingSkills>();
            gameConfig.BuildingUpgradeDataList = new List<BuildingUpgradeData>();

            var configField = typeof(DataManager).GetField("_gameConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (configField != null) configField.SetValue(_dataManager, gameConfig);

            typeof(DataManager).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).SetValue(null, _dataManager);
            _dataManager.InitializeDataManager();
        }

        private Trait CreateMockTrait(string id, string familyId, Trait.RarityRank rank, string name)
        {
            var trait = ScriptableObject.CreateInstance<Trait>();
            trait.id = id;
            trait.familyId = familyId;
            trait.rank = rank;
            trait.traitName = name;
            return trait;
        }

        [TearDown]
        public void Teardown()
        {
            if (_dataManagerObj != null) Object.DestroyImmediate(_dataManagerObj);
            
            // Unsubscribe evolution system to prevent memory leaks in test runner
            _evolutionSystem = null; 
        }

        [Test]
        public void EvolutionSystem_Level30_GrantsNewTrait()
        {
            HeroData hero = new HeroData("H_EVO", "EvoTest", Gender.Male) { level = 29 };
            int initialTraitsCount = hero.traitIDs.Count;

            // Simulate gaining EXP to reach level 30
            hero.AddExperience(2900); // 29 * 100 = 2900, enough to reach 30 as per mock ExpTable

            Assert.AreEqual(30, hero.level, "Hero should have leveled up to 30.");
            Assert.IsTrue(hero.traitIDs.Count > initialTraitsCount, "Hero should have gained a new trait at level 30 evolution milestone.");
        }
        
        [Test]
        public void EvolutionSystem_Level70_GrantsNewTrait()
        {
            HeroData hero = new HeroData("H_EVO_70", "EvoTest70", Gender.Female) { level = 69 };
            int initialTraitsCount = hero.traitIDs.Count;

            hero.AddExperience(6900); // Level up to 70

            Assert.AreEqual(70, hero.level, "Hero should have leveled up to 70.");
            Assert.IsTrue(hero.traitIDs.Count > initialTraitsCount, "Hero should have gained a new trait at level 70 evolution milestone.");
        }
    }
}

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
    public class BreedingSystemTests
    {
        private BreedingSystem _breedingSystem;
        private GameObject _dataManagerObj;
        private DataManager _dataManager;

        [SetUp]
        public void Setup()
        {
            var avatarManagerObj = new UnityEngine.GameObject("TestAvatarManager");
            avatarManagerObj.AddComponent<LegendOfBlood.AvatarManager>();
            _breedingSystem = new BreedingSystem();

            _dataManagerObj = new GameObject("TestDataManager");
            _dataManager = _dataManagerObj.AddComponent<DataManager>();

            // Setup mock config for Traits
            var gameConfig = ScriptableObject.CreateInstance<GameConfig>();
            gameConfig.EvolutionTable = new LegendOfBlood.GameConfigs.EvolutionTableData();
            gameConfig.EvolutionTable.rewards = new System.Collections.Generic.List<LegendOfBlood.GameConfigs.EvolutionRewardData>();
            gameConfig.AllTraits = new List<Trait>
            {
                CreateMockTrait("S_01", Trait.RarityRank.B, "trait_b1"),
                CreateMockTrait("S_02", Trait.RarityRank.A, "trait_a1"),
                CreateMockTrait("S_04", Trait.RarityRank.S, "trait_s_twins"),      // Twins
                CreateMockTrait("SS_07", Trait.RarityRank.S, "trait_ss_elite") // Elite Bloodline
            };
            // Other mandatory lists
            gameConfig.AllSkills = new List<Skill>();
            gameConfig.ExperienceTable = new List<ExperienceData> { new ExperienceData { level = 1, experienceRequired = 100 } };
            gameConfig.StartingSkills = new List<ProfessionStartingSkills>();
            gameConfig.BuildingUpgradeDataList = new List<BuildingUpgradeData>();

            var configField = typeof(DataManager).GetField("_gameConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (configField != null) configField.SetValue(_dataManager, gameConfig);

            typeof(DataManager).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).SetValue(null, _dataManager);
            _dataManager.InitializeDataManager();
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
            if (_dataManagerObj != null) Object.DestroyImmediate(_dataManagerObj);
        }

        [Test]
        public void BreedingSystem_SameGender_ReturnsEmptyList()
        {
            HeroData father = new HeroData("H1", "Boy1", Gender.Male);
            HeroData father2 = new HeroData("H2", "Boy2", Gender.Male);

            LogAssert.Expect(LogType.Error, "breeding_error_same_gender");
            var result = _breedingSystem.Breed(father, father2);

            Assert.AreEqual(0, result.Count, "Breeding same gender should fail and return empty.");
        }

        [Test]
        public void BreedingSystem_NormalBreeding_ReturnsOneChild()
        {
            HeroData father = new HeroData("H1", "Boy1", Gender.Male) { potential = 80 };
            HeroData mother = new HeroData("H2", "Girl1", Gender.Female) { potential = 80 };

            var result = _breedingSystem.Breed(father, mother);

            Assert.AreEqual(1, result.Count, "Normal breeding should return exactly 1 child.");
            Assert.AreEqual(80, result[0].potential, "Child potential should be average of parents.");
            Assert.AreEqual(1, result[0].level, "Child level should be 1.");
            Assert.IsFalse(result[0].isMature, "Child should not be mature at birth.");
        }

        [Test]
        public void BreedingSystem_TwinsTrait_MayReturnTwoChildren()
        {
            // We force a high number of breedings to catch the 2% chance of twins
            int twinsCount = 0;
            HeroData father = new HeroData("H1", "Boy1", Gender.Male) { traitIDs = new List<string> { "S_04" } };
            HeroData mother = new HeroData("H2", "Girl1", Gender.Female);

            // Run 500 breedings to mathematically guarantee finding twins (2% = 1/50)
            for (int i = 0; i < 500; i++)
            {
                var result = _breedingSystem.Breed(father, mother);
                if (result.Count == 2) twinsCount++;
            }

            Assert.IsTrue(twinsCount > 0, "With Twins trait, there should be at least one occurrence of 2 children over 500 attempts.");
        }

        [Test]
        public void BreedingSystem_EliteBloodline_IncreasesStats()
        {
            // Arrange
            HeroData father = new HeroData("H1", "Boy1", Gender.Male) { potential = 100, traitIDs = new List<string> { "SS_07" } };
            HeroData mother = new HeroData("H2", "Girl1", Gender.Female) { potential = 100 };

            int eliteHits = 0;
            for (int i = 0; i < 200; i++)
            {
                var children = _breedingSystem.Breed(father, mother);
                var child = children[0];
                
                // Normal max stat at 100 potential is 100 * 10 = 1000
                // If it hits EliteBloodline (10% chance), it gets multiplied by 1.05.
                // 100 * 10 * 1.05 = 1050.
                if (child.baseStats.hp > 1000) eliteHits++;
            }

            Assert.IsTrue(eliteHits > 0, "Elite bloodline should occasionally push stats beyond normal max limits.");
        }

        [Test]
        public void BreedingSystem_Options_GuaranteesTrait()
        {
            HeroData father = new HeroData("H1", "Boy1", Gender.Male);
            HeroData mother = new HeroData("H2", "Girl1", Gender.Female);

            BreedingOptions options = new BreedingOptions { GuaranteedTraitID = "S_01" };

            var children = _breedingSystem.Breed(father, mother, options);

            Assert.IsTrue(children[0].traitIDs.Contains("S_01"), "Child must inherit the guaranteed trait from BreedingOptions.");
        }
    }
}

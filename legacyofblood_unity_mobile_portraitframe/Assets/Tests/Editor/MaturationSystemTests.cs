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
    public class MaturationSystemTests
    {
        private MaturationSystem _maturationSystem;
        private GameObject _dataManagerObj;
        private DataManager _dataManager;

        [SetUp]
        public void Setup()
        {
            var avatarManagerObj = new UnityEngine.GameObject("TestAvatarManager");
            avatarManagerObj.AddComponent<LegendOfBlood.AvatarManager>();
            _maturationSystem = new MaturationSystem();

            _dataManagerObj = new GameObject("TestDataManager");
            _dataManager = _dataManagerObj.AddComponent<DataManager>();

            var gameConfig = ScriptableObject.CreateInstance<GameConfig>();
            gameConfig.EvolutionTable = new LegendOfBlood.GameConfigs.EvolutionTableData();
            gameConfig.EvolutionTable.rewards = new System.Collections.Generic.List<LegendOfBlood.GameConfigs.EvolutionRewardData>();
            gameConfig.AllTraits = new List<Trait>();
            gameConfig.AllSkills = new List<Skill>();
            gameConfig.ExperienceTable = new List<ExperienceData>();
            
            // Setup Starting Skills for maturation logic
            gameConfig.StartingSkills = new List<ProfessionStartingSkills>
            {
                new ProfessionStartingSkills { profession = Profession.Warrior, startingSkillIDs = new List<string> { "WarriorStrike" } },
                new ProfessionStartingSkills { profession = Profession.Mage, startingSkillIDs = new List<string> { "Fireball" } },
                new ProfessionStartingSkills { profession = Profession.Archer, startingSkillIDs = new List<string> { "CriticalShot" } },
                new ProfessionStartingSkills { profession = Profession.Healer, startingSkillIDs = new List<string> { "HealLight" } }
            };
            
            gameConfig.BuildingUpgradeDataList = new List<BuildingUpgradeData>();

            var configField = typeof(DataManager).GetField("_gameConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (configField != null) configField.SetValue(_dataManager, gameConfig);

            typeof(DataManager).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).SetValue(null, _dataManager);
            _dataManager.InitializeDataManager();
            _dataManager.Player.Heroes.Clear();
        }

        [TearDown]
        public void Teardown()
        {
            if (_dataManagerObj != null) UnityEngine.Object.DestroyImmediate(_dataManagerObj);
        }

        [Test]
        public void MaturationSystem_Tick_MaturesInfantsOverTime()
        {
            HeroData infant = new HeroData("BABY_1", "Tiny", Gender.Male)
            {
                isMature = false,
                // Set maturation time slightly in the past so it triggers immediately on Tick
                maturationEndTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 1000,
                profession = Profession.None
            };
            
            _dataManager.AddHero(infant);

            _maturationSystem.Tick(1.0f);

            Assert.IsTrue(infant.isMature, "Hero should be flagged as mature after timeout.");
            Assert.AreNotEqual(Profession.None, infant.profession, "Hero should be assigned a random profession.");
            Assert.IsTrue(infant.skillIDs.Count > 0, "Hero should have learned a starting skill related to their new profession.");
        }

        [Test]
        public void MaturationSystem_Tick_IgnoresFutureMaturations()
        {
            HeroData infant = new HeroData("BABY_2", "Future", Gender.Female)
            {
                isMature = false,
                // Time in the future
                maturationEndTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 50000,
                profession = Profession.None
            };
            
            _dataManager.AddHero(infant);

            _maturationSystem.Tick(1.0f);

            Assert.IsFalse(infant.isMature, "Hero should NOT mature before their time has come.");
            Assert.AreEqual(Profession.None, infant.profession, "Profession should remain None.");
        }
    }
}

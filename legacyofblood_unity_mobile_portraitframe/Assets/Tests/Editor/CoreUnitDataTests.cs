using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using LegendOfBlood;

namespace LegendOfBlood.Tests
{
    [TestFixture]
    public class CoreUnitDataTests
    {
        private GameObject _dataManagerObj;
        private DataManager _dataManager;

        [SetUp]
        public void Setup()
        {
            var avatarManagerObj = new UnityEngine.GameObject("TestAvatarManager");
            avatarManagerObj.AddComponent<LegendOfBlood.AvatarManager>();
            _dataManagerObj = new GameObject("TestDataManager");
            _dataManager = _dataManagerObj.AddComponent<DataManager>();

            // Config is minimal because we are testing core math on the HeroData object itself
            var gameConfig = ScriptableObject.CreateInstance<LegendOfBlood.GameConfigs.GameConfig>();
            gameConfig.EvolutionTable = new LegendOfBlood.GameConfigs.EvolutionTableData();
            gameConfig.EvolutionTable.rewards = new System.Collections.Generic.List<LegendOfBlood.GameConfigs.EvolutionRewardData>();
            gameConfig.AllTraits = new List<LegendOfBlood.Trait>();
            gameConfig.AllSkills = new List<LegendOfBlood.Skill>();
            gameConfig.ExperienceTable = new List<LegendOfBlood.GameConfigs.ExperienceData>();
            gameConfig.BuildingUpgradeDataList = new List<LegendOfBlood.GameConfigs.BuildingUpgradeData>();

            gameConfig.StartingSkills = new List<LegendOfBlood.GameConfigs.ProfessionStartingSkills>();
            var configField = typeof(DataManager).GetField("_gameConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (configField != null) configField.SetValue(_dataManager, gameConfig);

            typeof(DataManager).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).SetValue(null, _dataManager);
            _dataManager.InitializeDataManager();
        }

        [TearDown]
        public void Teardown()
        {
            if (_dataManagerObj != null) Object.DestroyImmediate(_dataManagerObj);
        }

        [Test]
        public void HeroData_CalculateStats_ScalesWithLevelAndPotential()
        {
            HeroData weakHero = new HeroData("H_WEAK", "Weakling", Gender.Male) { level = 10, potential = 5 };
            weakHero.CalculateBaseStats();
            HeroData strongHero = new HeroData("H_STRONG", "Chad", Gender.Male) { level = 10, potential = 25 };
            strongHero.CalculateBaseStats();

            Assert.IsTrue(strongHero.baseStats.hp > weakHero.baseStats.hp, "Higher potential should yield higher HP at same level.");
            Assert.IsTrue(strongHero.baseStats.atk > weakHero.baseStats.atk, "Higher potential should yield higher ATK at same level.");
        }

        [Test]
        public void HeroData_GetFinalStats_CombinesBaseAndAddedStats()
        {
            HeroData hero = new HeroData("H_MATH", "Math", Gender.Female) { level = 1, potential = 10 };
            hero.CalculateBaseStats();
            
            float baseHp = hero.baseStats.hp;
            
            // Add manual stats (e.g., from potions, evolutions)
            hero.addedStats.hp += 500;
            
            // Add equipment stats (Mocking equipment logic)
            EquipmentData dummySword = new EquipmentData { id = "EQ_1", equipmentName = "Sword", atkBonus = 100 };
            hero.Equipments.Add(EquipmentSlot.Weapon, dummySword);

            HeroStats finalStats = hero.GetFinalStats();

            Assert.AreEqual(baseHp + 500, finalStats.hp, "Final HP should combine base and added stats.");
            Assert.AreEqual(hero.baseStats.atk + 100, finalStats.atk, "Final ATK should incorporate equipment stats.");
        }
    }
}

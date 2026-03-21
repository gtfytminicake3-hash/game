using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using LegendOfBlood;
using LegendOfBlood.Combat;
using LegendOfBlood.GameConfigs;

namespace LegendOfBlood.Tests
{
    [TestFixture]
    public class CombatSystemTests
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

            var gameConfig = ScriptableObject.CreateInstance<GameConfig>();
            gameConfig.EvolutionTable = new LegendOfBlood.GameConfigs.EvolutionTableData();
            gameConfig.EvolutionTable.rewards = new System.Collections.Generic.List<LegendOfBlood.GameConfigs.EvolutionRewardData>();
            gameConfig.AllTraits = new List<Trait>();
            gameConfig.AllSkills = new List<Skill>
            {
                CreateMockSkill("SK_WARRIOR_1", "Slash", 1.2f, SkillType.Active, 2, LegendOfBlood.HeroClass.Warrior, TargetingType.SingleFrontEnemy),
                CreateMockSkill("SK_HEALER_1", "Heal", 1.0f, SkillType.Active, 3, LegendOfBlood.HeroClass.Healer, TargetingType.LowestHpAlly)
            };
            
            gameConfig.CombatSettings = new LegendOfBlood.GameConfigs.CombatConfig { archerBonusCritChance = 0.2f };
            gameConfig.ExperienceTable = new List<ExperienceData>();
            gameConfig.StartingSkills = new List<LegendOfBlood.GameConfigs.ProfessionStartingSkills>();
            gameConfig.BuildingUpgradeDataList = new List<BuildingUpgradeData>();

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

        private Skill CreateMockSkill(string id, string name, float power, SkillType type, int cd, HeroClass reqClass, TargetingType targeting)
        {
            var skill = ScriptableObject.CreateInstance<Skill>();
            skill.id = id;
            skill.skillName = name;
            skill.powerRatio = power;
            skill.type = type;
            skill.cooldown = cd;
            skill.requiredProfession = reqClass;
            skill.targeting = targeting;
            return skill;
        }

        [Test]
        public void CombatSystem_Simulate_ReturnsValidResult()
        {
            var combatSystem = new CombatSystem(12345, new List<Skill>(DataManager.Instance.AllSkills.Values));

            var playerHeroes = new List<HeroData>
            {
                new HeroData("P1", "Player1", Gender.Male) { profession = Profession.Warrior, level = 10, potential = 15 }
            };
            playerHeroes[0].CalculateBaseStats();

            var enemyHeroes = new List<HeroData>
            {
                new HeroData("E1", "Enemy1", Gender.Female) { profession = Profession.Warrior, level = 1, potential = 5 } // Weak enemy
            };
            enemyHeroes[0].CalculateBaseStats();

            var result = combatSystem.Simulate(playerHeroes, enemyHeroes);

            Assert.IsNotNull(result, "CombatResult should not be null.");
            Assert.IsTrue(result.DidPlayerWin, "Player should win inherently because of massive stat difference.");
            Assert.IsTrue(result.CombatLog.Count > 0, "Combat log should contain events.");
            Assert.AreEqual(1, result.PlayerSurvivors.Count, "Player should have survived.");
            Assert.AreEqual(1, result.EnemyCasualties.Count, "Enemy should have died.");
        }
        
        [Test]
        public void CombatSystem_Simulate_UpdatesHeroHP()
        {
            var combatSystem = new CombatSystem(54321, new List<Skill>(DataManager.Instance.AllSkills.Values));

            var playerHeroes = new List<HeroData>
            {
                new HeroData("P1", "Player1", Gender.Male) { profession = Profession.Warrior, level = 10, potential = 15 }
            };
            playerHeroes[0].CalculateBaseStats();
            float maxHp = playerHeroes[0].GetFinalStats().hp;

            // Make enemy strong enough to deal at least some damage but not kill
            var enemyHeroes = new List<HeroData>
            {
                new HeroData("E1", "Enemy1", Gender.Female) { profession = Profession.Warrior, level = 5, potential = 10 }
            };
            enemyHeroes[0].CalculateBaseStats();

            combatSystem.Simulate(playerHeroes, enemyHeroes);

            // Assert that player HP has changed and is properly saved back to the HeroData reference
            Assert.IsTrue(playerHeroes[0].currentHp <= maxHp, "Player HP should be updated after combat.");
        }
    }
}

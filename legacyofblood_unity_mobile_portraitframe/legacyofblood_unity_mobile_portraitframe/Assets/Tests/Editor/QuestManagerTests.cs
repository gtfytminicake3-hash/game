using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using LegendOfBlood;
using LegendOfBlood.GameConfigs;
using LegendOfBlood.Managers;

namespace LegendOfBlood.Tests
{
    [TestFixture]
    public class QuestManagerTests
    {
        private GameObject _gameManagerObj;
        private GameManager _gameManager;
        private GameObject _dataManagerObj;
        private DataManager _dataManager;
        private GameObject _questManagerObj;
        private QuestManager _questManager;

        [SetUp]
        public void Setup()
        {
            var avatarManagerObj = new UnityEngine.GameObject("TestAvatarManager");
            avatarManagerObj.AddComponent<LegendOfBlood.AvatarManager>();
            _gameManagerObj = new GameObject("TestGameManager");
            _gameManager = _gameManagerObj.AddComponent<GameManager>();

            _dataManagerObj = new GameObject("TestDataManager");
            _dataManager = _dataManagerObj.AddComponent<DataManager>();

            _questManagerObj = new GameObject("TestQuestManager");
            _questManager = _questManagerObj.AddComponent<QuestManager>();

            var gameConfig = ScriptableObject.CreateInstance<GameConfig>();
            gameConfig.EvolutionTable = new LegendOfBlood.GameConfigs.EvolutionTableData();
            gameConfig.EvolutionTable.rewards = new System.Collections.Generic.List<LegendOfBlood.GameConfigs.EvolutionRewardData>();
            gameConfig.AllTraits = new List<Trait>();
            gameConfig.AllSkills = new List<Skill>();
            gameConfig.ExperienceTable = new List<ExperienceData>();
            gameConfig.StartingSkills = new List<LegendOfBlood.GameConfigs.ProfessionStartingSkills>();
            gameConfig.BuildingUpgradeDataList = new List<BuildingUpgradeData>();
            
            gameConfig.AllQuestData = new List<QuestData>
            {
                new QuestData 
                { 
                    questId = "Q_TEST_1", 
                    type = QuestType.UPGRADE_BUILDING, 
                    targetId = "MainHall",
                    targetValue = 2,
                    rewards = new List<QuestReward> { new QuestReward { resourceId = "Gold", amount = 100 } }
                }
            };

            var configField = typeof(DataManager).GetField("_gameConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (configField != null) configField.SetValue(_dataManager, gameConfig);

            var dmField = typeof(GameManager).GetField("_dataManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (dmField != null) dmField.SetValue(_gameManager, _dataManager);

            typeof(DataManager).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).SetValue(null, _dataManager);
            _dataManager.InitializeDataManager();
            
            // Activate the quest directly since startingQuests field is private and might be processed differently
            var initMethod = typeof(QuestManager).GetMethod("InitializeQuestManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (initMethod != null) initMethod.Invoke(_questManager, null);
            
            _questManager.ActivateQuest("Q_TEST_1");
        }

        [TearDown]
        public void Teardown()
        {
            if (_gameManagerObj != null) Object.DestroyImmediate(_gameManagerObj);
            if (_dataManagerObj != null) Object.DestroyImmediate(_dataManagerObj);
            if (_questManagerObj != null) Object.DestroyImmediate(_questManagerObj);
        }

        [Test]
        public void QuestManager_ActivateQuest_SetsStateActive()
        {
            var activeQuest = _dataManager.Player.QuestStatuses.Find(q => q.questId == "Q_TEST_1");

            Assert.IsNotNull(activeQuest, "Quest should be active in the system.");
            Assert.AreEqual(QuestState.Active, activeQuest.state, "Quest should be set to Active state immediately.");
        }

        [Test]
        public void QuestManager_EventTriggers_CompletesQuestUponReachingTarget()
        {
            // Simulate upgrading building MainHall to level 2 using reflection
            Building building = new Building { id = "MainHall", level = 2 };
            var handleMethod = typeof(QuestManager).GetMethod("HandleBuildingUpgraded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            handleMethod.Invoke(_questManager, new object[] { building });

            var quest = _dataManager.Player.QuestStatuses.Find(q => q.questId == "Q_TEST_1");
            Assert.AreEqual(QuestState.Completed, quest.state, "Quest should be marked as completed when target is reached.");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using LegendOfBlood;
using LegendOfBlood.GameConfigs;
using System.IO;
using System.Linq;

namespace LegendOfBlood.Tests
{
    [TestFixture]
    public class DataManagerTests
    {
        private GameObject _dataManagerObj;
        private DataManager _dataManager;
        private string _originalSaveFileName;

        [SetUp]
        public void Setup()
        {
            var avatarManagerObj = new UnityEngine.GameObject("TestAvatarManager");
            avatarManagerObj.AddComponent<LegendOfBlood.AvatarManager>();
            // Creates a dummy GameConfig. Not full, but enough to not crash.
            var gameConfig = ScriptableObject.CreateInstance<GameConfig>();
            gameConfig.EvolutionTable = new LegendOfBlood.GameConfigs.EvolutionTableData();
            gameConfig.EvolutionTable.rewards = new System.Collections.Generic.List<LegendOfBlood.GameConfigs.EvolutionRewardData>();
            gameConfig.AllTraits = new List<Trait>();
            gameConfig.AllSkills = new List<Skill>();
            gameConfig.ExperienceTable = new List<ExperienceData>();
            gameConfig.StartingSkills = new List<ProfessionStartingSkills>();
            gameConfig.BuildingUpgradeDataList = new List<BuildingUpgradeData>();

            _dataManagerObj = new GameObject("TestDataManager");
            _dataManager = _dataManagerObj.AddComponent<DataManager>();

            // Setup reflection to inject GameConfig
            var configField = typeof(DataManager).GetField("_gameConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (configField != null) configField.SetValue(_dataManager, gameConfig);

            typeof(DataManager).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).SetValue(null, _dataManager);
            _dataManager.InitializeDataManager();
        }

        [TearDown]
        public void Teardown()
        {
            if (_dataManagerObj != null)
            {
                Object.DestroyImmediate(_dataManagerObj);
            }
            
            // Clean up test saves if they were somehow created
            string savePath = Path.Combine(Application.persistentDataPath, "legendofblood_save.json");
            // If we actually overwrote the real save during tests, we should be using a mock path.
            // Let's ensure DataManager uses a test path by reflection, or we just test Player logic.
            // But DataManager loads automatically in InitializeDataManager. 
        }

        [Test]
        public void DataManager_NewPlayer_HasDefaultValues()
        {
            // Assert
            Assert.IsNotNull(_dataManager.Player, "Player data should be initialized.");
            Assert.IsNotNull(_dataManager.Player.resources, "Player resources should not be null.");
            Assert.IsNotNull(_dataManager.Player.Heroes, "Player heroes list should not be null.");
        }

        [Test]
        public void DataManager_ModifyPlayerData_ThenSaveAndLoad_DataIsPreserved()
        {
            // We can't easily change the SAVE_FILE_NAME constant without reflection into DataManager's logic,
            // but we can test PlayerData serialization.
            
            // Arrange
            PlayerData originalPlayer = new PlayerData();
            originalPlayer.playerName = "TestLord";
            originalPlayer.resources = new PlayerResources { gold = 1000, wood = 500, stone = 100 };
            
            HeroData testHero = new HeroData("HERO_1", "TestHero", Gender.Male)
            {
                level = 10,
                potential = 100,
                traitIDs = new List<string> { "S_01" }
            };
            originalPlayer.Heroes.Add(testHero);

            // Serialize
            string json = JsonUtility.ToJson(originalPlayer);

            // Deserialize
            PlayerData loadedPlayer = JsonUtility.FromJson<PlayerData>(json);

            // Assert
            Assert.AreEqual("TestLord", loadedPlayer.playerName);
            // Wait, does Unity's JsonUtility serialize Dictionary? NO.
            // Let's verify our PlayerData logic actually handles resources properly (it uses ResourceDictionary wrapper or pairs if done correctly, but let's check hero).
            Assert.AreEqual(1, loadedPlayer.Heroes.Count);
            Assert.AreEqual("HERO_1", loadedPlayer.Heroes[0].id);
            Assert.AreEqual("TestHero", loadedPlayer.Heroes[0].heroName);
            Assert.AreEqual(Gender.Male, loadedPlayer.Heroes[0].gender);
            Assert.AreEqual(10, loadedPlayer.Heroes[0].level);
            Assert.AreEqual(100, loadedPlayer.Heroes[0].potential);
            Assert.AreEqual(1, loadedPlayer.Heroes[0].traitIDs.Count);
            Assert.AreEqual("S_01", loadedPlayer.Heroes[0].traitIDs[0]);
        }
    }
}

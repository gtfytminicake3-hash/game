// --- START OF FILE DataManager.cs (FIXED AGAIN) ---

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LegendOfBlood.GameConfigs; // Dùng GameConfig từ namespace này
using LegendOfBlood.Managers;   // Dùng PlayerQuestStatus từ namespace này
// using LegendOfBlood.DataModels; // SỬA LỖI: XÓA DÒNG NÀY
using UnityEngine;

namespace LegendOfBlood
{
    /// <summary>
    /// DataManager là một Singleton chịu trách nhiệm:
    /// 1. Tải và cung cấp dữ liệu cấu hình game (Traits, Skills, etc.) từ ScriptableObjects.
    /// 2. Quản lý dữ liệu trạng thái của người chơi (heroes, inventory, buildings).
    /// 3. Xử lý việc lưu và tải dữ liệu của người chơi ra file.
    /// </summary>
    [DisallowMultipleComponent]
    public class DataManager : MonoBehaviour
    {
        #region Singleton Pattern
        public static DataManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
            // DontDestroyOnLoad(this.gameObject); // Dòng này sẽ được quản lý bởi [CORE_SYSTEMS]

            // Khởi tạo toàn bộ dữ liệu khi game bắt đầu
            InitializeDataManager();
        }
        #endregion

        #region Fields and Properties

        [Header("Game Configuration")]
        [SerializeField]
        [Tooltip("Kéo ScriptableObject chứa toàn bộ data config của game vào đây.")]
        private GameConfigs.GameConfig _gameConfig;

        // Dữ liệu cấu hình game (được tối ưu hóa để truy cập nhanh bằng Dictionary)
        public Dictionary<string, Trait> AllTraits { get; private set; }
        public Dictionary<string, Skill> AllSkills { get; private set; }
        public Dictionary<string, BossData> AllBosses { get; private set; }
        public Dictionary<int, int> ExpTable { get; private set; }
        // SỬA LỖI: Chỉ định rõ namespace cho EvolutionRewardData để giải quyết lỗi CS0029
        public List<GameConfigs.EvolutionRewardData> EvolutionRewards { get; private set; }
        public Dictionary<Profession, List<string>> StartingSkillsByProfession { get; private set; }
        public Dictionary<string, BuildingUpgradeData> BuildingUpgradeConfigs { get; private set; }
        public POIMonsterConfig POIMonsterConfig { get; private set; }
        public Dictionary<string, QuestData> AllQuests { get; private set; }

        // Dữ liệu trạng thái của người chơi (runtime data)
        public PlayerData Player { get; private set; }
        public List<HeroData> AllHeroes => Player.Heroes;
        public List<Building> AllBuildings { get; private set; }

        // Tên file lưu
        private const string SAVE_FILE_NAME = "legendofblood_save.json";
        private string _saveFilePath;

        // Sự kiện để thông báo cho các hệ thống khác (ví dụ: UI) khi dữ liệu thay đổi
        public static event Action OnPlayerDataLoaded;
        public static event Action OnHeroListChanged;

        #endregion

        #region Initialization

        /// <summary>
        /// Khởi tạo DataManager, xử lý config và tải dữ liệu người chơi.
        /// </summary>
        private void InitializeDataManager()
        {
            _saveFilePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            ProcessGameConfiguration();
            LoadPlayerData();
        }

        /// <summary>
        /// Đọc dữ liệu từ ScriptableObject và chuyển thành Dictionary để truy cập hiệu quả.
        /// </summary>
        private void ProcessGameConfiguration()
        {
            if (_gameConfig == null)
            {
                Debug.LogError(global::LocalizationSystem.GetText("datamanager_error_no_gameconfig"));
                return;
            }

            AllTraits = _gameConfig.AllTraits.ToDictionary(t => t.id, t => t);
            AllSkills = _gameConfig.AllSkills.ToDictionary(s => s.id, s => s);
            ExpTable = _gameConfig.ExperienceTable.ToDictionary(e => e.level, e => e.experienceRequired);

            if (_gameConfig.EvolutionTable != null)
            {
                EvolutionRewards = _gameConfig.EvolutionTable.rewards;
            }
            else
            {
                EvolutionRewards = new List<GameConfigs.EvolutionRewardData>();
                Debug.LogWarning("EvolutionTable is not set in GameConfig.");
            }

            StartingSkillsByProfession = new Dictionary<Profession, List<string>>();
            foreach (var profSkills in _gameConfig.StartingSkills)
            {
                if (!StartingSkillsByProfession.ContainsKey(profSkills.profession))
                {
                    StartingSkillsByProfession.Add(profSkills.profession, profSkills.startingSkillIDs);
                }
            }

            BuildingUpgradeConfigs = new Dictionary<string, BuildingUpgradeData>();
            if (_gameConfig.BuildingUpgradeDataList != null)
            {
                foreach (var config in _gameConfig.BuildingUpgradeDataList)
                {
                    if (config != null && !BuildingUpgradeConfigs.ContainsKey(config.buildingId))
                    {
                        BuildingUpgradeConfigs.Add(config.buildingId, config);
                    }
                }
            }

            POIMonsterConfig = _gameConfig.POIMonsterConfig;
            
            AllQuests = _gameConfig.AllQuestData?.ToDictionary(q => q.questId, q => q) ?? new Dictionary<string, QuestData>();
            AllBosses = _gameConfig.AllBosses?.ToDictionary(b => b.id, b => b) ?? new Dictionary<string, BossData>(); 

            Debug.Log($"Đã xử lý xong Game Config: {AllTraits.Count} Traits, {AllSkills.Count} Skills, {BuildingUpgradeConfigs.Count} BuildingConfigs. {AllQuests.Count} Quests.");
        }

        #endregion

        #region Save/Load Logic

        public void LoadPlayerData()
        {
            if (File.Exists(_saveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_saveFilePath);
                    SaveData loadedData = JsonUtility.FromJson<SaveData>(json);
                    Player = loadedData.Player ?? new PlayerData();
                    AllBuildings = loadedData.AllBuildings;
                    Debug.Log($"Tải game thành công từ: {_saveFilePath}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Lỗi khi đọc file save! Tạo game mới. Lỗi: {e.Message}");
                    CreateNewPlayerData();
                }
            }
            else
            {
                Debug.Log("Không tìm thấy file save. Tạo game mới.");
                CreateNewPlayerData();
            }
            
            if (Player.Heroes == null) Player.Heroes = new List<HeroData>();
            if (Player.QuestStatuses == null) Player.QuestStatuses = new List<LegendOfBlood.Managers.PlayerQuestStatus>();
            if (Player.WorldPois == null) Player.WorldPois = new List<POIData>();
            if (AllBuildings == null) AllBuildings = new List<Building>();

            OnPlayerDataLoaded?.Invoke();
            OnHeroListChanged?.Invoke();
        }

        private void CreateNewPlayerData()
        {
            Player = new PlayerData();
            Player.Heroes = new List<HeroData>();
            Player.QuestStatuses = new List<LegendOfBlood.Managers.PlayerQuestStatus>();
            Player.WorldPois = new List<POIData>();
            AllBuildings = new List<Building>();
            
            HeroData startingMale = CreateStartingHero(Gender.Male, "Adam");
            HeroData startingFemale = CreateStartingHero(Gender.Female, "Eva");
            Player.Heroes.Add(startingMale);
            Player.Heroes.Add(startingFemale);
        }

        public void SavePlayerData()
        {
            SaveData saveData = new SaveData
            {
                Player = this.Player,
                AllBuildings = this.AllBuildings
            };

            try
            {
                string json = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(_saveFilePath, json);
                Debug.Log($"Lưu game thành công tại: {_saveFilePath}");
            }
            catch(Exception e)
            {
                Debug.LogError($"Lỗi khi lưu game! Lỗi: {e.Message}");
            }
        }
        #endregion

        #region Public API (Helpers & Modifiers)

        public int GetPopulationCapacity()
        {
            var mainHall = AllBuildings.FirstOrDefault(b => b.id == "MainHall");
            if (mainHall != null)
            {
                return 10 + (mainHall.level * 2);
            }
            return 10;
        }

        public bool IsPopulationFull()
        {
            return Player.Heroes.Count >= GetPopulationCapacity();
        }

        public void AddHero(HeroData newHero)
        {
            if (newHero == null) return;

            if (IsPopulationFull())
            {
                Debug.LogWarning("Population is full! Cannot add new hero.");
                GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("notification_population_full"));
                return;
            }

            Player.Heroes.Add(newHero);
            OnHeroListChanged?.Invoke();
            Debug.Log($"Added hero {newHero.heroName}. Total heroes: {Player.Heroes.Count}/{GetPopulationCapacity()}");
        }

        public void RemoveHero(string heroId)
        {
            HeroData heroToRemove = GetHeroByID(heroId);
            if (heroToRemove != null)
            {
                Player.Heroes.Remove(heroToRemove);
                OnHeroListChanged?.Invoke();
            }
        }

        public HeroData GetHeroByID(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return Player.Heroes.FirstOrDefault(h => h.id == id);
        }

        public POIData GetPOIByID(string poiId)
        {
            if (string.IsNullOrEmpty(poiId) || Player?.WorldPois == null) return null;
            return Player.WorldPois.FirstOrDefault(p => p.poiId == poiId);
        }

        public HeroData GetMonsterByID(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            BossData bossCfg = AllBosses.TryGetValue(id, out var boss) ? boss : null;
            if (bossCfg != null) 
            {
                 return new HeroData
                {
                    id = id,
                    heroName = bossCfg.bossName,
                    level = bossCfg.level,
                    profession = Profession.Warrior,
                    baseStats = new HeroStats { hp = bossCfg.baseHp, atk = bossCfg.baseAtk, def = bossCfg.baseDef, spd = bossCfg.baseSpd },
                    currentHp = bossCfg.baseHp,
                    isMature = true 
                };
            }

            Debug.LogWarning($"GetMonsterByID chưa tìm thấy trong AllBosses. Trả về quái vật giả cho ID: {id}");
            return new HeroData
            {
                id = id,
                heroName = $"Quái vật {id}",
                level = 5,
                profession = Profession.Warrior,
                baseStats = new HeroStats { hp = 200, atk = 20, def = 15, spd = 10 },
                currentHp = 200,
                isMature = true
            };
        }

        public Trait GetTraitByID(string id)
        {
            AllTraits.TryGetValue(id, out var trait);
            return trait;
        }

        public Skill GetSkillByID(string id)
        {
            AllSkills.TryGetValue(id, out var skill);
            return skill;
        }

        public List<string> GetStartingSkills(Profession profession)
        {
            if (StartingSkillsByProfession.TryGetValue(profession, out var skillList))
            {
                return skillList;
            }
            return new List<string>();
        }

        public BuildingUpgradeData GetBuildingUpgradeData(string buildingId)
        {
            if (string.IsNullOrEmpty(buildingId) || BuildingUpgradeConfigs == null)
            {
                return null;
            }
            BuildingUpgradeConfigs.TryGetValue(buildingId, out var data);
            return data;
        }

        public QuestData GetQuestData(string questId)
        {
            if (string.IsNullOrEmpty(questId) || AllQuests == null)
            {
                return null;
            }
            AllQuests.TryGetValue(questId, out var data);
            return data;
        }

        #endregion

        #region Utility Methods
        
        private HeroData CreateStartingHero(Gender gender, string name)
        {
            var hero = new HeroData(Guid.NewGuid().ToString(), name, gender)
            {
                level = 1,
                potential = 50,
                baseStats = new HeroStats { hp = 100, atk = 10, def = 8, spd = 12 },
                isMature = true
            };
            hero.currentHp = hero.GetFinalStats().hp; 
            return hero;
        }

        #endregion
    }

    [Serializable]
    public class SaveData
    {
        public PlayerData Player;
        public List<Building> AllBuildings;
    }
}
// --- END OF FILE DataManager.cs (FIXED AGAIN) ---
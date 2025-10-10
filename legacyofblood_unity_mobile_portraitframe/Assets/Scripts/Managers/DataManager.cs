namespace LegendOfBlood
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq; // THÊM using này để có thể dùng .FirstOrDefault()
    using UnityEngine;

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
        private GameConfig _gameConfig;

        // Dữ liệu cấu hình game (được tối ưu hóa để truy cập nhanh bằng Dictionary)
        public Dictionary<string, Trait> AllTraits { get; private set; }
        public Dictionary<string, Skill> AllSkills { get; private set; }
        public Dictionary<string, BossData> AllBosses { get; private set; }
        public Dictionary<int, int> ExpTable { get; private set; }

        // Dữ liệu trạng thái của người chơi (runtime data)
        public PlayerData Player { get; private set; }
        public List<HeroData> AllHeroes { get; private set; }
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

            AllTraits = new Dictionary<string, Trait>();
            foreach (var trait in _gameConfig.AllTraits)
            {
                if (!AllTraits.ContainsKey(trait.id))
                    AllTraits.Add(trait.id, trait);
                else
                    Debug.LogWarning($"Tìm thấy Trait ID trùng lặp: {trait.id}");
            }

            AllSkills = new Dictionary<string, Skill>();
            // ... (code xử lý AllSkills của bạn)

            AllBosses = new Dictionary<string, BossData>();
            // ... (code xử lý AllBosses của bạn)

            ExpTable = new Dictionary<int, int>();
            // ... (code xử lý ExpTable của bạn)

            Debug.Log($"Đã xử lý xong Game Config: {AllTraits.Count} Traits, {AllSkills.Count} Skills.");
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
                    Player = loadedData.Player;
                    AllHeroes = loadedData.AllHeroes;
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
            
            OnPlayerDataLoaded?.Invoke();
            OnHeroListChanged?.Invoke();
        }

        private void CreateNewPlayerData()
        {
            Player = new PlayerData();
            AllBuildings = new List<Building>();
            
            AllHeroes = new List<HeroData>();
            HeroData startingMale = CreateStartingHero(Gender.Male, "Adam");
            HeroData startingFemale = CreateStartingHero(Gender.Female, "Eva");
            AllHeroes.Add(startingMale);
            AllHeroes.Add(startingFemale);
        }

        public void SavePlayerData()
        {
            SaveData saveData = new SaveData
            {
                Player = this.Player,
                AllHeroes = this.AllHeroes,
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

        // --- HERO MANAGEMENT ---

        public void AddHero(HeroData newHero)
        {
            if (newHero == null) return;
            AllHeroes.Add(newHero);
            OnHeroListChanged?.Invoke();
        }

        public void RemoveHero(string heroId)
        {
            HeroData heroToRemove = GetHeroByID(heroId); // Tái sử dụng hàm GetHeroByID
            if (heroToRemove != null)
            {
                AllHeroes.Remove(heroToRemove);
                OnHeroListChanged?.Invoke();
            }
        }

        // --- BỔ SUNG CÁC HÀM TRUY CẬP DỮ LIỆU CẦN THIẾT ---

        /// <summary>
        /// Lấy một hero từ danh sách AllHeroes bằng ID.
        /// </summary>
        public HeroData GetHeroByID(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return AllHeroes.FirstOrDefault(h => h.id == id);
        }

        /// <summary>
        /// Lấy dữ liệu của một quái vật bằng ID.
        /// </summary>
        public HeroData GetMonsterByID(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            // TODO: Hoàn thiện logic này sau khi bạn có dữ liệu quái vật trong GameConfig.
            // Ví dụ: MonsterConfigData monsterCfg = _gameConfig.AllMonsters.FirstOrDefault(m => m.id == id);
            // if (monsterCfg != null) { return ConvertMonsterToHeroData(monsterCfg); }

            // Tạm thời trả về một quái vật giả để các hệ thống khác không bị lỗi.
            Debug.LogWarning($"GetMonsterByID chưa được triển khai đầy đủ. Trả về quái vật giả cho ID: {id}");
            return new HeroData
            {
                id = id,
                heroName = $"Quái vật {id}",
                level = 5,
                profession = Profession.Warrior, // Giả sử quái vật cũng có profession
                baseStats = new HeroStats { hp = 200, atk = 20, def = 15, spd = 10 },
                currentHp = 200,
                isMature = true // Quái vật luôn sẵn sàng chiến đấu
            };
        }

        // --- CONFIG DATA ACCESSORS ---

        public Trait GetTraitByID(string id)
        {
            if (string.IsNullOrEmpty(id) || !AllTraits.ContainsKey(id))
            {
                return null;
            }
            return AllTraits[id];
        }

        public Skill GetSkillByID(string id)
        {
            if (string.IsNullOrEmpty(id) || !AllSkills.ContainsKey(id))
            {
                return null;
            }
            return AllSkills[id];
        }

        #endregion

        #region Utility Methods
        
        private HeroData CreateStartingHero(Gender gender, string name)
        {
            // Sử dụng constructor có tham số để đảm bảo ID và tên được gán
            var hero = new HeroData(Guid.NewGuid().ToString(), name, gender)
            {
                level = 1,
                potential = 50,
                baseStats = new HeroStats { hp = 100, atk = 10, def = 8, spd = 12 },
                isMature = true
            };
            // Đảm bảo máu hiện tại bằng máu tối đa khi tạo mới
            hero.currentHp = hero.GetFinalStats().hp; 
            return hero;
        }

        #endregion
    }

    [Serializable]
    public class SaveData
    {
        public PlayerData Player;
        public List<HeroData> AllHeroes;
        public List<Building> AllBuildings;
    }
}
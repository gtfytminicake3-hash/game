namespace LegendOfBlood
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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
            // Thiết lập đường dẫn lưu file một cách an toàn trên mọi nền tảng
            _saveFilePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

            // 1. Xử lý dữ liệu cấu hình game trước tiên
            ProcessGameConfiguration();

            // 2. Tải dữ liệu người chơi hoặc tạo mới nếu chưa có
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

            // Chuyển danh sách Trait thành Dictionary
            AllTraits = new Dictionary<string, Trait>();
            foreach (var trait in _gameConfig.AllTraits)
            {
                if (!AllTraits.ContainsKey(trait.id))
                {
                    AllTraits.Add(trait.id, trait);
                }
                else
                {
                    Debug.LogWarning($"Tìm thấy Trait ID trùng lặp: {trait.id}");
                }
            }

            // Tương tự cho Skills, Bosses, ExpTable...
            AllSkills = new Dictionary<string, Skill>();
            // ... (code tương tự)

            AllBosses = new Dictionary<string, BossData>();
            // ... (code tương tự)

            ExpTable = new Dictionary<int, int>();
            // ... (code tương tự)

            Debug.Log($"Đã xử lý xong Game Config: {AllTraits.Count} Traits, {AllSkills.Count} Skills.");
        }

        #endregion

        #region Save/Load Logic

        /// <summary>
        /// Tải dữ liệu người chơi từ file. Nếu file không tồn tại, tạo dữ liệu mới.
        /// </summary>
        public void LoadPlayerData()
        {
            if (File.Exists(_saveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_saveFilePath);
                    SaveData loadedData = JsonUtility.FromJson<SaveData>(json);

                    // Khôi phục dữ liệu từ file đã tải
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
            
            // Phát sự kiện để báo cho UI và các hệ thống khác cập nhật
            OnPlayerDataLoaded?.Invoke();
            OnHeroListChanged?.Invoke();
        }

        /// <summary>
        /// Tạo một bộ dữ liệu mới cho người chơi lần đầu.
        /// </summary>
        private void CreateNewPlayerData()
        {
            Player = new PlayerData(); // Giả sử constructor sẽ khởi tạo tài nguyên ban đầu
            AllBuildings = new List<Building>(); // Khởi tạo các công trình ban đầu
            
            // Tạo 2 hero khởi đầu để người chơi có thể bắt đầu lai tạo
            AllHeroes = new List<HeroData>();
            HeroData startingMale = CreateStartingHero(Gender.Male, "Adam");
            HeroData startingFemale = CreateStartingHero(Gender.Female, "Eva");
            AllHeroes.Add(startingMale);
            AllHeroes.Add(startingFemale);
        }

        /// <summary>
        /// Lưu trạng thái hiện tại của người chơi vào file JSON.
        /// </summary>
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
                string json = JsonUtility.ToJson(saveData, true); // `true` để format JSON cho dễ đọc
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

        /// <summary>
        /// Thêm một hero mới vào danh sách và phát sự kiện.
        /// </summary>
        public void AddHero(HeroData newHero)
        {
            if (newHero == null) return;
            AllHeroes.Add(newHero);
            OnHeroListChanged?.Invoke();
        }

        /// <summary>
        /// Xóa một hero khỏi danh sách bằng ID và phát sự kiện.
        /// </summary>
        public void RemoveHero(string heroId)
        {
            HeroData heroToRemove = AllHeroes.Find(h => h.id == heroId);
            if (heroToRemove != null)
            {
                AllHeroes.Remove(heroToRemove);
                OnHeroListChanged?.Invoke();
            }
        }

        // --- CONFIG DATA ACCESSORS ---

        /// <summary>
        /// Lấy một Trait từ config bằng ID.
        /// </summary>
        public Trait GetTraitByID(string id)
        {
            if (string.IsNullOrEmpty(id) || !AllTraits.ContainsKey(id))
            {
                //Debug.LogWarning($"Không tìm thấy Trait với ID: {id}");
                return null;
            }
            return AllTraits[id];
        }

        /// <summary>
        /// Lấy một Skill từ config bằng ID.
        /// </summary>
        public Skill GetSkillByID(string id)
        {
            // ... (Tương tự GetTraitByID)
            return null;
        }

        #endregion

        #region Utility Methods
        
        /// <summary>
        /// Hàm tiện ích để tạo hero khởi đầu.
        /// </summary>
        private HeroData CreateStartingHero(Gender gender, string name)
        {
            return new HeroData
            {
                id = Guid.NewGuid().ToString(),
                heroName = name,
                gender = gender,
                level = 1,
                potential = 50, // Tiềm năng trung bình
                baseStats = new HeroStats { hp = 100, atk = 10, def = 8, spd = 12 },
                currentHp = 100,
                isMature = true
            };
        }

        #endregion
    }

    /// <summary>
    /// Lớp bao bọc (wrapper class) để gom tất cả dữ liệu cần lưu vào một đối tượng.
    /// Điều này giúp việc chuyển đổi sang JSON dễ dàng hơn với JsonUtility.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public PlayerData Player;
        public List<HeroData> AllHeroes;
        public List<Building> AllBuildings;
    }
}
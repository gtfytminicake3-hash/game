// Dùng namespace để tổ chức code tốt hơn
namespace LegendOfBlood
{
    using LegendOfBlood.Combat;
    using LegendOfBlood.Managers;
    using System;
    using System.Collections.Generic;
    using System.Linq; // THÊM using này
    using UnityEngine;

    /// <summary>
    /// GameManager là lớp Singleton cốt lõi, hoạt động như trung tâm điều khiển và
    /// bộ định vị dịch vụ (Service Locator) cho tất cả các hệ thống chính trong game.
    /// Nó tồn tại xuyên suốt các scene và quản lý vòng đời của game.
    /// </summary>
    [DisallowMultipleComponent] // Ngăn không cho gắn script này nhiều lần trên cùng một GameObject
    public class GameManager : MonoBehaviour
    {
        #region Singleton Pattern
        
        // Thể hiện (instance) tĩnh duy nhất của GameManager
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            // Kiểm tra xem đã có Instance nào tồn tại chưa
            if (Instance != null && Instance != this)
            {
                // Nếu có, và đó không phải là cái này, thì phá hủy GameObject này.
                // Điều này đảm bảo chỉ có một GameManager duy nhất.
                Debug.LogWarning("Một GameManager khác đã tồn tại. Hủy bỏ bản sao này.");
#if UNITY_EDITOR
                if (!Application.isPlaying) { DestroyImmediate(this.gameObject); return; }
#endif
                Destroy(this.gameObject);
            }
            else
            {
                // Nếu chưa có, gán Instance cho chính nó
                Instance = this;

                // Giữ cho phần Root chứa GameManager không bị phá hủy khi chuyển scene (nếu cần)
                DontDestroyOnLoad(this.transform.root.gameObject);

                // Sau khi thiết lập Singleton, tiến hành khởi tạo các hệ thống
                InitializeSystems();
            }
        }
        
        #endregion

        #region Game State Management

        // Định nghĩa các trạng thái có thể có của game
        public enum GameState
        {
            Initializing, // Đang khởi tạo
            MainMenu,     // Ở màn hình chính
            Playing,      // Đang trong gameplay
            Paused        // Đang tạm dừng
        }

        // Trạng thái hiện tại của game, chỉ có thể đọc từ bên ngoài
        public GameState CurrentState { get; private set; }
        
        // Event được phát ra mỗi khi trạng thái game thay đổi
        public static event Action<GameState> OnGameStateChanged;

        /// <summary>
        /// Cập nhật trạng thái game và thông báo cho các hệ thống khác.
        /// </summary>
        /// <param name="newState">Trạng thái mới</param>
        public void UpdateGameState(GameState newState)
        {
            if (CurrentState == newState) return;

            CurrentState = newState;

            switch (newState)
            {
                case GameState.Initializing:
                    // Có thể thực hiện các hành động khi bắt đầu khởi tạo
                    break;
                case GameState.MainMenu:
                    // Ví dụ: Kích hoạt UI màn hình chính
                    Time.timeScale = 1f;
                    break;
                case GameState.Playing:
                    // Ví dụ: Bắt đầu gameplay
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    // Tạm dừng game
                    Time.timeScale = 0f;
                    break;
            }

            // Phát sự kiện, các lớp khác có thể lắng nghe sự kiện này
            OnGameStateChanged?.Invoke(newState);
            Debug.Log($"Game State changed to: {newState}");
        }

        #endregion

        #region System References (Service Locator)

        [Header("Core Managers & Systems")]
        [Tooltip("Kéo các GameObject hoặc Prefab chứa các script hệ thống vào đây.")]
        
        [SerializeField] private DataManager _dataManager;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private ExpeditionManager _expeditionManager;
        [SerializeField] private UIManager _uiManager;
        [SerializeField] private UINotificationManager _uiNotificationManager;
        [SerializeField] private ArenaSystem _arenaSystem; // Thêm dòng này
        [SerializeField] private AdManager _adManager; // NEW: Thêm dòng này
        [SerializeField] private QuestManager _questManager;

        // Các lớp logic không nhất thiết phải là MonoBehaviour
        // Sử dụng Lazy Initialization để chống lại lỗi mất dữ liệu khi Unity Assembly Reload (Hot Reload)

        private BreedingSystem _breedingSystem;
        public BreedingSystem BreedingSystem => _breedingSystem ??= new BreedingSystem();

        private EvolutionSystem _evolutionSystem;
        public EvolutionSystem EvolutionSystem => _evolutionSystem ??= new EvolutionSystem();

        private HospitalSystem _hospitalSystem;
        public HospitalSystem HospitalSystem => _hospitalSystem ??= new HospitalSystem();

        private MaturationSystem _maturationSystem;
        public MaturationSystem MaturationSystem => _maturationSystem ??= new MaturationSystem();

        private BuildingSystem _buildingSystem;
        public BuildingSystem BuildingSystem => _buildingSystem ??= new BuildingSystem();

        private RecruitmentSystem _recruitmentSystemManager;
        public RecruitmentSystem RecruitmentSystem => _recruitmentSystemManager ??= new RecruitmentSystem();

        private CombatSystem _combatSystem;
        public CombatSystem CombatSystem 
        {
            get 
            {
                if (_combatSystem == null)
                {
                    var skills = (_dataManager != null && _dataManager.AllSkills != null) ? _dataManager.AllSkills.Values.ToList() : new List<LegendOfBlood.Skill>();
                    _combatSystem = new CombatSystem(Environment.TickCount, skills);
                }
                return _combatSystem;
            }
        }
        
        // Public accessors để các script khác có thể truy cập an toàn
        public DataManager DataManager => _dataManager;
        public InventoryManager InventoryManager => _inventoryManager;
        public ExpeditionManager ExpeditionManager => _expeditionManager;
        public UIManager UIManager => _uiManager;
        public UINotificationManager UINotificationManager => _uiNotificationManager;
        public ArenaSystem ArenaSystem => _arenaSystem; // Thêm dòng này
        public AdManager AdManager => _adManager; // NEW: Thêm thuộc tính này
        public QuestManager QuestManager => _questManager;

        #endregion

        #region Initialization and Main Loop

        private void Start()
        {
            // Thiết lập trạng thái ban đầu của game sau khi mọi thứ đã được khởi tạo
            UpdateGameState(GameState.Playing); // Hoặc GameState.MainMenu nếu có

            // Gọi kiểm tra vé đấu trường hàng ngày
            _arenaSystem.CheckDailyTicketRefresh();
        }

        /// <summary>
        /// Khởi tạo tất cả các hệ thống con theo đúng thứ tự.
        /// Được gọi trong Awake().
        /// </summary>
        private void InitializeSystems()
        {
            // Tự động tìm các Manager nếu chưa được gán bằng tay trong Inspector (Auto-wiring)
            if (_dataManager == null) { _dataManager = transform.root.GetComponentInChildren<DataManager>(true); if (_dataManager == null) _dataManager = gameObject.AddComponent<DataManager>(); }
            if (_inventoryManager == null) { _inventoryManager = transform.root.GetComponentInChildren<InventoryManager>(true); if (_inventoryManager == null) _inventoryManager = gameObject.AddComponent<InventoryManager>(); }
            if (_expeditionManager == null) { _expeditionManager = transform.root.GetComponentInChildren<ExpeditionManager>(true); if (_expeditionManager == null) _expeditionManager = gameObject.AddComponent<ExpeditionManager>(); }
            if (_uiManager == null) { _uiManager = transform.root.GetComponentInChildren<UIManager>(true); if (_uiManager == null) _uiManager = gameObject.AddComponent<UIManager>(); }
            if (_uiNotificationManager == null) { _uiNotificationManager = transform.root.GetComponentInChildren<UINotificationManager>(true); if (_uiNotificationManager == null) _uiNotificationManager = gameObject.AddComponent<UINotificationManager>(); }
            if (_adManager == null) { _adManager = transform.root.GetComponentInChildren<AdManager>(true); if (_adManager == null) _adManager = gameObject.AddComponent<AdManager>(); } // NEW
            if (gameObject.GetComponent<AdRewardGateway>() == null) { gameObject.AddComponent<AdRewardGateway>(); } // NEW: Gateway Part
            if (_arenaSystem == null) { _arenaSystem = transform.root.GetComponentInChildren<ArenaSystem>(true); if (_arenaSystem == null) _arenaSystem = gameObject.AddComponent<ArenaSystem>(); }
            if (_questManager == null) { _questManager = transform.root.GetComponentInChildren<QuestManager>(true); if (_questManager == null) _questManager = gameObject.AddComponent<QuestManager>(); }

            // Khởi tạo các hệ thống logic CƠ BẢN TRƯỚC (POCO) để hệ thống không bao giờ Null (Đã chuyển sang Lazy Init trên Property)

            try
            {
                // Khởi tạo hệ thống dịch thuật
                global::LocalizationSystem.LoadLocalizedText((global::Language)LanguageManager.CurrentLanguage);

                // GỌI KHỞI TẠO DATA
                if (_dataManager != null)
                {
                    _dataManager.InitializeDataManager();
                }

                // GỌI KHỞI TẠO QUẢNG CÁO
                if (_adManager != null)
                {
                    _adManager.InitializeSystem();
                }

                // Gọi một Get để ép khởi tạo sớm
                var _ = CombatSystem;
                
                Debug.Log("Tất cả các hệ thống đã được khởi tạo thành công.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameManager] CÓ LỖI XẢY RA TRONG LÚC KHỞI TẠO! {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// Vòng lặp Update chính của GameManager.
        /// Dùng để điều khiển các hệ thống cần cập nhật theo thời gian.
        /// </summary>
        private void Update()
        {
            // Xử lý nút Back trên Android (Escape)
            // LƯU Ý: Đã bị comment lại do xung đột với Input System Package mới!
            // Cần được viết lại bằng UnityEngine.InputSystem nếu cần.
            /*
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_uiManager != null)
                {
                    bool handled = _uiManager.GoBack();
                    if (!handled)
                    {
                        // Nếu không còn panel nào để back, tự động save và thoát game
                        SaveGame();
                        Application.Quit();
                    }
                }
            }
            */

            // Không chạy update nếu game đang không ở trạng thái Playing
            if (CurrentState != GameState.Playing) return;
            if (DataManager.Instance == null) return;

            // Cung cấp một "tick" cho các hệ thống cần nó
            // Điều này giúp tập trung logic cập nhật vào một nơi thay vì có nhiều
            // script MonoBehaviour với hàm Update() riêng lẻ.
            float deltaTime = Time.deltaTime;
            MaturationSystem.Tick(deltaTime);
            HospitalSystem.Tick(deltaTime);
            BuildingSystem.Tick(deltaTime);
            DataManager.Instance.Tick(deltaTime);

#if UNITY_EDITOR
            if (UnityEngine.Input.GetKeyDown(KeyCode.F9))
            {
                if (DataManager.Instance != null && DataManager.Instance.Player != null)
                {
                    if (DataManager.Instance.Player.items.ContainsKey("IT_GACHA_TICKET"))
                        DataManager.Instance.Player.items["IT_GACHA_TICKET"] += 100;
                    else
                        DataManager.Instance.Player.items.Add("IT_GACHA_TICKET", 100);
                        
                    DataManager.Instance.SavePlayerData();
                    if (ToastNotificationManager.Instance != null)
                        ToastNotificationManager.Instance.ShowToast("Hack: +100 Bùa Ước Nguyện!");
                }
            }
#endif
            ExpeditionManager.Tick();
        }

        #endregion

        #region Save & Load

        /// <summary>
        /// Kích hoạt hành động lưu game.
        /// </summary>
        public void SaveGame()
        {
            Debug.Log("GameManager: Yêu cầu lưu game...");
            _dataManager.SavePlayerData();
        }

        /// <summary>
        /// Kích hoạt hành động tải game.
        /// </summary>
        public void LoadGame()
        {
            Debug.Log("GameManager: Yêu cầu tải game...");
            _dataManager.LoadPlayerData();
        }

        // Xử lý lưu game tự động khi thoát
        private void OnApplicationQuit()
        {
            SaveGame();
        }

        // Đảm bảo dữ liệu được lưu khi app bị ẩn/đẩy xuống background trên mobile
        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                SaveGame();
            }
        }

        #endregion
    }
}
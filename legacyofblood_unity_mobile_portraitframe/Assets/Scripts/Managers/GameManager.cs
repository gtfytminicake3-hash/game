// Dùng namespace để tổ chức code tốt hơn
namespace LegendOfBlood
{
    using System;
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
                Destroy(this.gameObject);
            }
            else
            {
                // Nếu chưa có, gán Instance cho chính nó
                Instance = this;

                // Giữ cho GameManager không bị phá hủy khi chuyển scene
                DontDestroyOnLoad(this.gameObject);

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

        // Các lớp logic không nhất thiết phải là MonoBehaviour
        // Chúng ta sẽ tạo các thể hiện của chúng
        public BreedingSystem BreedingSystem { get; private set; }
        public CombatSystem CombatSystem { get; private set; }
        public EvolutionSystem EvolutionSystem { get; private set; }
        public HospitalSystem HospitalSystem { get; private set; }
        public MaturationSystem MaturationSystem { get; private set; }
        public BuildingSystem BuildingSystem { get; private set; }
        
        // Public accessors để các script khác có thể truy cập an toàn
        public DataManager DataManager => _dataManager;
        public InventoryManager InventoryManager => _inventoryManager;
        public ExpeditionManager ExpeditionManager => _expeditionManager;
        public UIManager UIManager => _uiManager;
        public UINotificationManager UINotificationManager => _uiNotificationManager;

        #endregion

        #region Initialization and Main Loop

        private void Start()
        {
            // Thiết lập trạng thái ban đầu của game sau khi mọi thứ đã được khởi tạo
            UpdateGameState(GameState.Playing); // Hoặc GameState.MainMenu nếu có
        }

        /// <summary>
        /// Khởi tạo tất cả các hệ thống con theo đúng thứ tự.
        /// Được gọi trong Awake().
        /// </summary>
        private void InitializeSystems()
        {
            // Khởi tạo hệ thống dịch thuật
            global::LocalizationSystem.LoadLocalizedText((global::Language)LanguageManager.CurrentLanguage);

            // Xác thực rằng tất cả các tham chiếu đã được gán trong Inspector
            if (_dataManager == null || _inventoryManager == null || _expeditionManager == null || _uiManager == null || _uiNotificationManager == null)
            {
                Debug.LogError("GAME MANAGER: Một hoặc nhiều Manager chưa được gán trong Inspector!");
                // Vô hiệu hóa component để tránh lỗi NullReferenceException
                this.enabled = false;
                return;
            }

            // Khởi tạo các hệ thống logic (POCO - Plain Old C# Object)
            BreedingSystem = new BreedingSystem();
            CombatSystem = new CombatSystem();
            EvolutionSystem = new EvolutionSystem();
            HospitalSystem = new HospitalSystem();
            MaturationSystem = new MaturationSystem();
            BuildingSystem = new BuildingSystem();
            
            Debug.Log("Tất cả các hệ thống đã được khởi tạo thành công.");
        }
        
        /// <summary>
        /// Vòng lặp Update chính của GameManager.
        /// Dùng để điều khiển các hệ thống cần cập nhật theo thời gian.
        /// </summary>
        private void Update()
        {
            // Không chạy update nếu game đang không ở trạng thái Playing
            if (CurrentState != GameState.Playing) return;

            // Cung cấp một "tick" cho các hệ thống cần nó
            // Điều này giúp tập trung logic cập nhật vào một nơi thay vì có nhiều
            // script MonoBehaviour với hàm Update() riêng lẻ.
            float deltaTime = Time.deltaTime;
            MaturationSystem.Tick(deltaTime);
            HospitalSystem.Tick(deltaTime);
            BuildingSystem.Tick(deltaTime);
            ExpeditionManager.Tick(deltaTime);
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

        #endregion
    }
}
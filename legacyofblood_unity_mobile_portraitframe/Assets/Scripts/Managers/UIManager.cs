namespace LegendOfBlood
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.SceneManagement; // Thêm vào để quản lý scene

    /// <summary>
    /// Định danh các loại Panel UI chính trong game.
    /// </summary>
    public enum UIPanelType
    {
        None,
        MainScreen,      // Màn hình chính/Doanh trại
        HeroInfo,
        Breeding,
        Hospital,
        Arena,
        WorldMap,
        SquadSelection,
        HeroPicker,
        Inventory,
        ArenaShop,
        ProfessionSelection,
        Recruitment,
        Settings,
        BuildingUpgrade,
        Tutorial,
        Bootloader,
        Mailbox,
        Barrack,
        Battle,
        PopulationManager,
        Quest,
        BossBattle,
        Tower,
        POI_Info,
        Menu,
        KingGodPass
    }

    /// <summary>
    /// Quản lý việc hiển thị và ẩn các Panel UI chính. Tự động tìm và đăng ký
    /// tất cả các UIPanel trong scene khi scene được tải.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        // Dictionary để truy cập nhanh các panel bằng Type
        private readonly Dictionary<UIPanelType, GameObject> _panelDictionary = new Dictionary<UIPanelType, GameObject>();
        
        // Stack để lưu lịch sử các panel đã mở, hữu ích cho nút "Back"
        private readonly Stack<UIPanelType> _history = new Stack<UIPanelType>();
        
        private UIPanelType _currentPanel = UIPanelType.None;
        public UIPanelType CurrentPanel => _currentPanel;
        
        private int _topSortingOrder = 100;

        // --- Feature Lock ---
        private bool IsArenaUnlocked { get; set; } = false;
        // private bool IsTowerUnlocked { get; set; } = false; // Example for future features

        #region Scene Management & Panel Registration

        private void Awake()
        {
            // Đăng ký toàn bộ Panel tĩnh từ đầu do quy về 1 Scene duy nhất
            RegisterAllPanelsInScene();
        }

        private void OnEnable()
        {
            // Đăng ký lắng nghe sự kiện khi ngôn ngữ thay đổi
            global::LocalizationSystem.OnLanguageChanged += UpdateAllVisiblePanelsText;
            DataManager.OnPlayerDataLoaded += UpdateFeatureLocks;
            BuildingSystem.OnBuildingUpgradeCompleted += HandleBuildingUpgradeCompleted;
        }

        private void Start()
        {
            // Kiểm tra lock ngay khi start để đảm bảo UI đồng bộ
            UpdateFeatureLocks();
        }

        private void OnDisable()
        {
            // Hủy đăng ký sự kiện ngôn ngữ
            global::LocalizationSystem.OnLanguageChanged -= UpdateAllVisiblePanelsText;
            DataManager.OnPlayerDataLoaded -= UpdateFeatureLocks;
            BuildingSystem.OnBuildingUpgradeCompleted -= HandleBuildingUpgradeCompleted;
        }

        private void HandleBuildingUpgradeCompleted(Building building)
        {
            // Nếu là Nhà lính nâng cấp xong, cập nhật lại toàn bộ khóa tính năng
            if (building.id == "Barracks" || building.type == BuildingType.Barracks)
            {
                UpdateFeatureLocks();
            }
        }

        /// <summary>
        /// Tìm và đăng ký tất cả các đối tượng có script UIPanel trong dự án (kể cả bị ẩn).
        /// </summary>
        private void RegisterAllPanelsInScene()
        {
            _panelDictionary.Clear();
            _history.Clear();
            _currentPanel = UIPanelType.None;

            // Truy quét MỌI UIPanel bị ẩn và không ẩn trên cảnh
            UIPanel[] allPanels = FindObjectsByType<UIPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            
            Debug.Log($"[UIManager] Tìm thấy {allPanels.Length} panel (Kể cả bị ẩn), đang tiến hành đăng ký...");

            foreach (UIPanel panel in allPanels)
            {
                Debug.Log($"[UIManager] Quét thấy Panel: {panel.name} | Class: {panel.GetType().Name} | PanelType Ban Đầu: {panel.PanelType}");

                // Tự động gắn PanelType cho các Panel mới sinh ra từ code nếu bị Unity đánh mất tham chiếu trong Editor
                if (panel.PanelType == UIPanelType.None)
                {
                    string className = panel.GetType().Name;
                    string fullClassName = panel.GetType().FullName;
                    Debug.Log($"[UIManager] 🔍 Kiểm tra fallback cho {panel.name} (Class: {fullClassName})");
                    
                    if (fullClassName.Contains("QuestPanel")) panel.PanelType = UIPanelType.Quest;
                    else if (fullClassName.Contains("InventoryPanel")) panel.PanelType = UIPanelType.Inventory;
                    else if (fullClassName.Contains("RecruitmentPanel")) panel.PanelType = UIPanelType.Recruitment;
                    else if (fullClassName.Contains("MenuPanel")) panel.PanelType = UIPanelType.Menu;
                    else if (fullClassName.Contains("ProfessionSelectionPanel")) panel.PanelType = UIPanelType.ProfessionSelection;
                    else if (fullClassName.Contains("BuildingUpgradePanel")) panel.PanelType = UIPanelType.BuildingUpgrade;
                    else if (fullClassName.Contains("BossBattlePanel")) panel.PanelType = UIPanelType.BossBattle;
                    else if (fullClassName.Contains("TowerPanel")) panel.PanelType = UIPanelType.Tower;
                    else if (fullClassName.Contains("HeroInfoPanel")) panel.PanelType = UIPanelType.HeroInfo;
                    else if (fullClassName.Contains("HospitalPanel")) panel.PanelType = UIPanelType.Hospital;
                    else if (fullClassName.Contains("BreedingUIController") || fullClassName.Contains("BreedingPanel")) panel.PanelType = UIPanelType.Breeding;
                    else if (fullClassName.Contains("ArenaPanel")) panel.PanelType = UIPanelType.Arena;
                    else if (fullClassName.Contains("POI_InfoPanel")) panel.PanelType = UIPanelType.POI_Info;
                    else if (fullClassName.Contains("MailboxPanel")) panel.PanelType = UIPanelType.Mailbox;
                    else if (fullClassName.Contains("SquadSelectionPanel")) panel.PanelType = UIPanelType.SquadSelection;
                    else if (fullClassName.Contains("BarrackPanel")) panel.PanelType = UIPanelType.Barrack;
                    else if (fullClassName.Contains("ArenaShopPanel")) panel.PanelType = UIPanelType.ArenaShop;
                    else if (fullClassName.Contains("HeroPickerPanel")) panel.PanelType = UIPanelType.HeroPicker;
                    else if (fullClassName.Contains("CombatVisualizerPanel")) panel.PanelType = UIPanelType.Battle;
                    else if (fullClassName.Contains("SettingsPanel")) panel.PanelType = UIPanelType.Settings;
                    else if (fullClassName.Contains("KingGodPassPanel")) panel.PanelType = UIPanelType.KingGodPass;
                    else if (fullClassName.Contains("TutorialPanel")) panel.PanelType = UIPanelType.Tutorial;
                    
                    if (panel.PanelType == UIPanelType.None) 
                    {
                        Debug.LogWarning($"[UIManager] 🚨 Bỏ qua {panel.name} vì PanelType vẫn là None sau khi check Fallback!");
                        continue; 
                    }
                    else
                    {
                        Debug.Log($"[UIManager] 🛠️ Đã dùng Fallback tự gán PanelType.{panel.PanelType} cho {panel.name}");
                    }
                }

                if (!_panelDictionary.ContainsKey(panel.PanelType))
                {
                    Debug.Log($"[UIManager] ✅ Đăng ký thành công {panel.PanelType} -> {panel.name}");
                    _panelDictionary.Add(panel.PanelType, panel.gameObject);
                    
                    // Ngoại lệ sống còn: Không được ẩn Bootloader vì nó phải chạy ngay khi bật game
                    if (panel.PanelType != UIPanelType.Bootloader)
                    {
                        panel.gameObject.SetActive(false); 
                        Debug.Log($"[UIManager] 🛑 Đã giấu đi Panel: {panel.name}");
                    }
                    else
                    {
                        // CHỈ CHỈNH SỬA TẠI ĐÂY: Ép bật Bootloader và đưa lên hàng đầu nếu nó lỡ bị tắt trong Editor
                        panel.gameObject.SetActive(true);
                        _currentPanel = UIPanelType.Bootloader;
                        panel.transform.SetAsLastSibling();
                    }
                }
                else
                {
                    Debug.LogWarning($"[UIManager] ⏭️ Panel với type {panel.PanelType} đã tồn tại. Bỏ qua và TẮT LUÔN Panel thừa: {panel.name}");
                    panel.gameObject.SetActive(false); // Ẩn luôn panel thừa để tránh nằm lỳ trên màn hình
                }
            }

            // Sau khi đăng ký xong, có thể hiển thị một panel mặc định nếu cần
            // Ví dụ: nếu là scene chính thì không làm gì, để màn hình làng hiện ra.
            // Nếu là scene bản đồ thì có thể tự động mở panel bản đồ.
        }

        #endregion

        public void BackToVillageView()
        {
            // Quay về màn hình chính, ẩn tất cả các panel khác
            ShowPanel(UIPanelType.MainScreen, true); 
        }
        #region Public API

        /// <summary>
        /// Hiển thị một panel và tùy chọn ẩn panel hiện tại.
        /// </summary>
        /// <param name="panelType">Loại panel cần hiển thị</param>
        /// <param name="hideCurrent">True: Ẩn panel đang mở. False: Hiển thị panel mới đè lên (dùng cho popup)</param>
        public void ShowPanel(UIPanelType panelType, bool hideCurrent, bool addToHistory)
        {
            ShowPanel(panelType, hideCurrent);
        }

        public void ShowPanel(UIPanelType panelType, bool hideCurrent = true)
        {
            if (panelType == UIPanelType.None) return;

            // TỰ PHỤC HỒI: Nếu không tìm thấy trong dictionary, thử quét lại scene một lần nữa
            if (!_panelDictionary.ContainsKey(panelType))
            {
                Debug.LogWarning($"[UIManager] ⚠️ Không tìm thấy {panelType} trong register. Đang tiến hành quét lại toàn bộ Scene (Re-scan)...");
                RegisterAllPanelsInScene();
            }

            if (panelType == _currentPanel && _panelDictionary.TryGetValue(panelType, out GameObject existingPanel))
            {
                existingPanel.SetActive(true);
                existingPanel.transform.SetAsLastSibling();
                return;
            }

            // Check feature lock before showing panel
            if (!IsFeatureUnlocked(panelType))
            {
                GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("notification_feature_locked"));
                return;
            }

            if (_panelDictionary.TryGetValue(panelType, out GameObject panelToShow))
            {
                UIPanelType previousPanel = _currentPanel;

                // Thêm panel hiện tại vào lịch sử *trước khi* ẩn nó đi
                if (previousPanel != UIPanelType.None)
                {
                    _history.Push(previousPanel);
                }

                // Nếu cần ẩn panel hiện tại và có một panel đang mở, VÀ ĐẶC BIỆT KHÔNG ĐƯỢC ẨN MAINSCREEN (Làng)
                if (hideCurrent && previousPanel != UIPanelType.None && previousPanel != UIPanelType.MainScreen)
                {
                    HidePanel(previousPanel);
                }

                panelToShow.SetActive(true);
                
                // Logic Layer: Đẩy Panel lên trên cùng (Che lấp các cái khác) để dễ tương tác,
                // Nhưng riêng MainScreen (Làng/Doanh trại gốc) thì phải nằm dưới cùng (Chỉ trên Bootloader).
                if (panelType == UIPanelType.MainScreen)
                {
                    panelToShow.transform.SetSiblingIndex(1);
                }
                else
                {
                    panelToShow.transform.SetAsLastSibling();
                    
                    // NATIVE SORTING ENFORCEMENT
                    var cv = panelToShow.GetComponent<Canvas>();
                    if(!cv) { 
                        cv = panelToShow.gameObject.AddComponent<Canvas>(); 
                        panelToShow.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>(); 
                    }
                    cv.overrideSorting = true;
                    // Tăng _topSortingOrder để đảm bảo panel MỚI NHẤT luôn nằm trên
                    _topSortingOrder += 10;
                    cv.sortingOrder = _topSortingOrder;
                    
                    if (panelType == UIPanelType.Barrack) cv.sortingOrder = 50;
                    else if (panelType == UIPanelType.HeroInfo) cv.sortingOrder = _topSortingOrder + 50; // Pop-up thông tin hero TUYỆT ĐỐI NẰM TRÊN
                    else if (panelType == UIPanelType.MainScreen) cv.overrideSorting = false;
                }

                _currentPanel = panelType;
            }
            else
            {
                string registeredPanels = string.Join(", ", _panelDictionary.Keys);
                Debug.LogError($"[UIManager] 🚨 THẤT BẠI: Vẫn không tìm thấy panel [{panelType}].\n" +
                               $"Danh sách panel đã đăng ký thành công: {registeredPanels}.\n" +
                               "HƯỚNG DẪN: Bạn hãy kéo Prefab tương ứng vào Hierarchy và đảm bảo nó nằm trong một Canvas!");
            }
        }

        /// <summary>
        /// Ẩn một panel cụ thể.
        /// </summary>
        public void HidePanel(UIPanelType type)
        {
            if (_panelDictionary.TryGetValue(type, out GameObject panelObj))
            {
                panelObj.SetActive(false);
                if (_currentPanel == type)
                {
                    // Nếu panel bị ẩn là panel hiện tại, ta reset trạng thái về "không có panel nào"
                    _currentPanel = UIPanelType.None;
                }
            }
        }

        /// <summary>
        /// Retrieves a panel of type T if it's registered.
        /// </summary>
        public T GetPanel<T>(UIPanelType panelType) where T : Component
        {
            if (_panelDictionary.TryGetValue(panelType, out GameObject panelObj))
            {
                return panelObj.GetComponent<T>();
            }
            return null;
        }
        
        /// <summary>
        /// Đóng panel hiện tại và quay lại panel trước đó trong lịch sử.
        /// Thường được gọi bởi nút "Back" hoặc "Close".
        /// </summary>
        public bool GoBack()
        {
            if (_currentPanel != UIPanelType.None && _currentPanel != UIPanelType.MainScreen)
            {
                var closingPanel = _currentPanel;
                HidePanel(closingPanel);

                while (_history.Count > 0)
                {
                    var previous = _history.Pop();
                    if (!_panelDictionary.TryGetValue(previous, out GameObject prevObj) || prevObj == null)
                    {
                        continue;
                    }

                    prevObj.SetActive(true);
                    prevObj.transform.SetAsLastSibling();
                    _currentPanel = previous;
                    return true;
                }

                _topSortingOrder = 100;
                ShowPanel(UIPanelType.MainScreen, false);
                return true;
            }
            return false;
        }

        public void RefreshAllActivePanels()
        {
            foreach (var kvp in _panelDictionary)
            {
                if (kvp.Value != null && kvp.Value.activeInHierarchy)
                {
                    kvp.Value.SetActive(false);
                    kvp.Value.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Yêu cầu tất cả các panel đang hiển thị cập nhật lại text theo ngôn ngữ mới.
        /// </summary>
        private void UpdateAllVisiblePanelsText()
        {
            Debug.Log("UIManager: Nhận được tín hiệu thay đổi ngôn ngữ, đang cập nhật UI...");
            foreach (var panelEntry in _panelDictionary)
            {
                // Chỉ cập nhật những panel đang được kích hoạt (visible)
                if (panelEntry.Value != null && panelEntry.Value.activeInHierarchy)
                {
                    // Lấy tất cả các component có khả năng dịch thuật trên panel và con của nó
                    var localizableComponents = panelEntry.Value.GetComponentsInChildren<ILocalizable>(true);
                    foreach (var component in localizableComponents)
                    {
                        component.UpdateLocalizedText();
                    }
                }
            }
        }
        #endregion

        #region Feature Lock Logic

        private void UpdateFeatureLocks()
        {
            if (DataManager.Instance == null || DataManager.Instance.AllBuildings == null) return;

            // Arena Unlock Condition
            var barracks = DataManager.Instance.AllBuildings.FirstOrDefault(b => b.id == "Barracks");
            if (barracks != null && barracks.level >= 5)
            {
                IsArenaUnlocked = true;
            }

            // TODO: Add other feature lock conditions here (e.g., Tower)
            Debug.Log($"Feature Lock Status: Arena Unlocked = {IsArenaUnlocked}");
            // After updating locks, you might want to refresh the main UI to show/hide buttons.
            // This can be done via an event.
        }

        private bool IsFeatureUnlocked(UIPanelType panelType)
        {
            switch (panelType)
            {
                // case UIPanelType.Tower: // Example
                //     return IsTowerUnlocked;
                default:
                    return true; // All other panels are unlocked by default
            }
        }

        #endregion
    }
}

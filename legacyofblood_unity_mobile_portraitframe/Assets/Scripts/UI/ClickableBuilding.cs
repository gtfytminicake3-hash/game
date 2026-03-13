using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Để dùng IPointerDownHandler, IPointerUpHandler vv
using UnityEngine.SceneManagement; 

[RequireComponent(typeof(Button))]
public class ClickableBuilding : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // Enum để quyết định hành động của tòa nhà
    public enum ClickActionType { OpenPanel, LoadScene }

    [Header("Building Action")]
    [SerializeField] private ClickActionType actionType;

    [Header("Panel Settings (If ActionType is OpenPanel)")]
    [SerializeField] private LegendOfBlood.UIPanelType panelToOpen;

    [Header("Scene Settings (If ActionType is LoadScene)")]
    [SerializeField] private string sceneNameToLoad;
    
    [Header("Upgrade Settings")]
    [SerializeField] private string buildingIdToUpgrade = ""; // Thêm Id để bind với BuildingUpgradePanel
    public float longPressDuration = 1.0f;
    
    [Header("Visual Notice Settings")]
    public bool enableUpgradeNotice = true;
    public float breatheSpeed = 2.0f; // Tốc độ nhịp thở
    public float maxBreatheScale = 1.05f; // Độ phóng to tối đa
    public float pressedScale = 0.95f;   // Lún xuống 5% khi bấm
    
    // Internal states
    private Button _button;
    private Image _image;
    private bool _isPointerDown = false;
    private bool _longPressTriggered = false;
    private float _timePressStarted;
    private Vector3 _originalScale;
    
    // Upgrade Notice State
    private bool _canUpgrade = false;
    private float _checkUpgradeTimer = 0f;
    private const float CHECK_UPGRADE_INTERVAL = 1.0f;

    // Timer State
    private Text _constructionTimerText;
    private GameObject _timerBackground;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _image = GetComponent<Image>();
        _originalScale = transform.localScale;

        if (_button != null)
        {
            // Bỏ sự kiện click gốc, chuyển sang dùng PointerUp để xử lý cả Click lẫn LongPress
            _button.onClick.RemoveAllListeners();
        }

        if (_image != null)
        {
            // Tối ưu click hình dáng thực của ảnh (Bỏ qua vùng trong suốt)
            // Nhớ bật Read/Write Enabled ở ảnh Sprite gốc trong phần Project!
            _image.alphaHitTestMinimumThreshold = 0.1f;
        }
    }

    private void Update()
    {
        HandleLongPress();
        HandleUpgradeNotice();
        UpdateConstructionTimer();
    }
    
    private void UpdateConstructionTimer()
    {
        if (string.IsNullOrEmpty(buildingIdToUpgrade) || LegendOfBlood.GameManager.Instance == null) 
        {
            HideConstructionTimer();
            return;
        }

        var dataManager = LegendOfBlood.DataManager.Instance;
        if (dataManager == null) return;
        
        var building = System.Linq.Enumerable.FirstOrDefault(dataManager.AllBuildings, b => b.id == buildingIdToUpgrade);
        if (building == null) return;

        if (building.isUnderConstruction)
        {
            long remainingMillis = building.constructionEndTime - System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            if (remainingMillis > 0)
            {
                // Chia 1000 để đổi từ milliseconsd sang seconds phục vụ logic hàm tính phút/giây bên dưới
                ShowConstructionTimer(remainingMillis / 1000f);
            }
            else
            {
                // Hết giờ nhưng GameManager bên ngoài chưa quét tới để clear cờ
                ShowConstructionTimer(0);
            }
        }
        else
        {
            HideConstructionTimer();
        }
    }

    private void ShowConstructionTimer(float remainingSeconds)
    {
        if (_constructionTimerText == null)
        {
            CreateTimerUI();
        }

        if (_timerBackground != null && !_timerBackground.activeSelf)
            _timerBackground.SetActive(true);

        int minutes = Mathf.FloorToInt(remainingSeconds / 60F);
        int seconds = Mathf.FloorToInt(remainingSeconds - minutes * 60);
        string niceTime = string.Format("{0:00}:{1:00}", minutes, seconds);
        
        _constructionTimerText.text = niceTime;
    }

    private void HideConstructionTimer()
    {
        if (_timerBackground != null && _timerBackground.activeSelf)
        {
            _timerBackground.SetActive(false);
        }
    }

    private void CreateTimerUI()
    {
        // 1. Tạo một GameObject làm nền (Background)
        _timerBackground = new GameObject("ConstructionTimer_BG");
        _timerBackground.transform.SetParent(this.transform, false);
        
        var bgImage = _timerBackground.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f); // Nền đen mờ 70%
        
        var bgRect = _timerBackground.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 1f); // Neo ở cạnh trên ở giữa
        bgRect.anchorMax = new Vector2(0.5f, 1f);
        bgRect.pivot = new Vector2(0.5f, 0f);
        bgRect.anchoredPosition = new Vector2(0, 10); // Cách đỉnh nhà 10 pixel
        bgRect.sizeDelta = new Vector2(100, 30); // Kích thước khung nền

        // 2. Tạo đồng hồ Text ghép vào khung nền
        GameObject textObj = new GameObject("TimerText");
        textObj.transform.SetParent(_timerBackground.transform, false);
        
        _constructionTimerText = textObj.AddComponent<Text>();
        _constructionTimerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // Cập nhật font theo chuẩn Unity mới
        _constructionTimerText.fontSize = 20;
        _constructionTimerText.color = Color.white;
        _constructionTimerText.alignment = TextAnchor.MiddleCenter;
        
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero; // Phóng đầy khung background
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    private void HandleLongPress()
    {
        if (_isPointerDown && !_longPressTriggered)
        {
            if (Time.time - _timePressStarted >= longPressDuration)
            {
                _longPressTriggered = true;
                
                // Trả lại scale ban đầu khi đã kích hoạt long press
                if (enableUpgradeNotice && !_canUpgrade)
                    transform.localScale = _originalScale;
                    
                OnLongPress();
            }
        }
    }
    
    private void HandleUpgradeNotice()
    {
        if (!enableUpgradeNotice) return;
        
        // Cập nhật trạng thái Can Upgrade mỗi 1 giây để đỡ tốn hiệu năng
        _checkUpgradeTimer += Time.deltaTime;
        if (_checkUpgradeTimer >= CHECK_UPGRADE_INTERVAL)
        {
            _checkUpgradeTimer = 0f;
            CheckUpgradability();
        }

        // Hiệu ứng "Thở" (Zoom in/out) nếu có thể nâng cấp và không bị người dùng đè chuột
        if (_canUpgrade && !_isPointerDown)
        {
            float scaleFactor = 1.0f + (maxBreatheScale - 1.0f) * (Mathf.Sin(Time.time * breatheSpeed * Mathf.PI) + 1f) / 2f;
            transform.localScale = _originalScale * scaleFactor;
        }
    }

    private void CheckUpgradability()
    {
        _canUpgrade = false;
        if (string.IsNullOrEmpty(buildingIdToUpgrade) || LegendOfBlood.GameManager.Instance == null) return;
        
        var dataManager = LegendOfBlood.DataManager.Instance;
        if (dataManager == null) return;

        // XỬ LÝ ĐẶC BIỆT CHO ARENA: Chỉ thở 1 lần khi Barracks đạt cấp 5, và tắt vĩnh viễn sau khi bị click.
        if (buildingIdToUpgrade == "Arena")
        {
            var barracks = System.Linq.Enumerable.FirstOrDefault(dataManager.AllBuildings, b => b.id == "Barracks");
            if (barracks != null && barracks.level >= 5)
            {
                // Kiểm tra xem người chơi đã click vào Arena lần nào kể từ lúc mở khoá chưa
                if (PlayerPrefs.GetInt("ArenaUnlockedClicked", 0) == 0)
                {
                    _canUpgrade = true; // Bật cờ cho phép thở
                }
            }
            return; // Trả về luôn, Arena không tốn tài nguyên nâng cấp
        }

        var building = System.Linq.Enumerable.FirstOrDefault(dataManager.AllBuildings, b => b.id == buildingIdToUpgrade);
        if (building == null) return;
        
        // NẾU ĐANG NÂNG CẤP DỞ -> Cho thở tiếp luôn!
        if (building.isUnderConstruction) 
        {
            _canUpgrade = true;
            return;
        }

        var upgradeConfig = dataManager.GetBuildingUpgradeData(buildingIdToUpgrade);
        if (upgradeConfig == null) return;

        var levelData = upgradeConfig.GetLevelData(building.level + 1);
        if (levelData == null) return; // Đã đạt max level

        // Kiểm tra tài nguyên
        var goldCost = System.Linq.Enumerable.FirstOrDefault(levelData.costs, c => c.resourceId.ToLower() == "gold");
        int costGold = goldCost != null ? goldCost.amount : 0;

        var woodCost = System.Linq.Enumerable.FirstOrDefault(levelData.costs, c => c.resourceId.ToLower() == "wood");
        int costWood = woodCost != null ? woodCost.amount : 0;

        var stoneCost = System.Linq.Enumerable.FirstOrDefault(levelData.costs, c => c.resourceId.ToLower() == "stone");
        int costStone = stoneCost != null ? stoneCost.amount : 0;

        bool hasResources = LegendOfBlood.GameManager.Instance.InventoryManager.HasEnoughResources(LegendOfBlood.ResourceType.Gold, costGold)
                         && LegendOfBlood.GameManager.Instance.InventoryManager.HasEnoughResources(LegendOfBlood.ResourceType.Wood, costWood)
                         && LegendOfBlood.GameManager.Instance.InventoryManager.HasEnoughResources(LegendOfBlood.ResourceType.Stone, costStone);

        _canUpgrade = hasResources;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPointerDown = true;
        _longPressTriggered = false;
        _timePressStarted = Time.time;
        
        if (enableUpgradeNotice)
        {
            transform.localScale = _originalScale * pressedScale;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Bỏ qua nếu đã đi chuột/kéo tay ra ngoài khỏi toà nhà (lệnh click bị huỷ)
        if (!_isPointerDown) return;
        
        _isPointerDown = false;
        
        if (enableUpgradeNotice && !_canUpgrade)
        {
            // Trả về kích thước gốc
            transform.localScale = _originalScale;
        }
        
        // Nếu nhả chuột ra MÀ CHƯA KÍP kích hoạt Long Press -> Tính là Click bình thường
        if (!_longPressTriggered)
        {
            if (buildingIdToUpgrade == "Arena" && _canUpgrade)
            {
                // Đánh dấu Arena đã được người chơi Click (đã chú ý tới việc mở khoá)
                PlayerPrefs.SetInt("ArenaUnlockedClicked", 1);
                PlayerPrefs.Save();
                _canUpgrade = false; // Tắt cờ báo hiệu
                transform.localScale = _originalScale; // Trả về hình dạng tĩnh ngay lập tức
            }

            PerformAction();
        }
    }

    private void OnLongPress()
    {
        Debug.Log($"[ClickableBuilding] Long press detected on {gameObject.name}! Opening Upgrade Panel.");
        if (LegendOfBlood.GameManager.Instance?.UIManager != null && !string.IsNullOrEmpty(buildingIdToUpgrade))
        {
            LegendOfBlood.GameManager.Instance.UIManager.ShowPanel(LegendOfBlood.UIPanelType.BuildingUpgrade, true);
            var upgradePanel = LegendOfBlood.GameManager.Instance.UIManager.GetPanel<LegendOfBlood.BuildingUpgradePanel>(LegendOfBlood.UIPanelType.BuildingUpgrade);
            if (upgradePanel != null)
            {
                upgradePanel.Setup(buildingIdToUpgrade);
            }
        }
        else if (string.IsNullOrEmpty(buildingIdToUpgrade))
        {
            Debug.LogWarning($"[ClickableBuilding] Chưa thiết lập buildingIdToUpgrade cho {gameObject.name}");
        }
    }

    private void PerformAction()
    {
        switch (actionType)
        {
            case ClickActionType.OpenPanel:
                OpenAssociatedPanel();
                break;
            case ClickActionType.LoadScene:
                LoadAssociatedScene();
                break;
        }
    }

    private void OpenAssociatedPanel()
    {
        if (LegendOfBlood.GameManager.Instance?.UIManager != null)
        {
            LegendOfBlood.GameManager.Instance.UIManager.ShowPanel(panelToOpen);
        }
    }

    private void LoadAssociatedScene()
    {
        if (!string.IsNullOrEmpty(sceneNameToLoad))
        {
            if (sceneNameToLoad.Contains("WorldMap"))
            {
                if (LegendOfBlood.GameManager.Instance?.UIManager != null)
                {
                    LegendOfBlood.GameManager.Instance.UIManager.ShowPanel(LegendOfBlood.UIPanelType.WorldMap);
                }
            }
            else if (sceneNameToLoad.Contains("Barrack") || sceneNameToLoad.Contains("Hero"))
            {
                if (LegendOfBlood.GameManager.Instance?.UIManager != null)
                {
                    LegendOfBlood.GameManager.Instance.UIManager.ShowPanel(LegendOfBlood.UIPanelType.Barrack);
                }
            }
            else
            {
                Debug.LogWarning($"[ClickableBuilding] Yêu cầu chuyển sang {sceneNameToLoad} bị chặn lại vì game đang chạy ở chế độ Single Scene.");
            }
        }
        else
        {
            Debug.LogError("Scene Name to Load is empty!", this);
        }
    }
}
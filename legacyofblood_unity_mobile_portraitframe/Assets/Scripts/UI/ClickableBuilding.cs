using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Để dùng IPointerDownHandler, IPointerUpHandler
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
    
    // Internal states
    private Button _button;
    private bool _isPointerDown = false;
    private bool _longPressTriggered = false;
    private float _timePressStarted;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null)
        {
            // Bỏ sự kiện click gốc, chuyển sang dùng PointerUp để xử lý cả Click lẫn LongPress
            _button.onClick.RemoveAllListeners();
        }
    }

    private void Update()
    {
        if (_isPointerDown && !_longPressTriggered)
        {
            if (Time.time - _timePressStarted >= longPressDuration)
            {
                _longPressTriggered = true;
                OnLongPress();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPointerDown = true;
        _longPressTriggered = false;
        _timePressStarted = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPointerDown = false;
        
        // Nếu nhả chuột ra MÀ CHƯA KÍP kích hoạt Long Press -> Tính là Click bình thường
        if (!_longPressTriggered)
        {
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
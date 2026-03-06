using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // <-- THÊM DÒNG NÀY

[RequireComponent(typeof(Button))]
public class ClickableBuilding : MonoBehaviour
{
    // Enum để quyết định hành động của tòa nhà
    public enum ClickActionType { OpenPanel, LoadScene }

    [Header("Building Action")]
    [SerializeField] private ClickActionType actionType;

    [Header("Panel Settings (If ActionType is OpenPanel)")]
    [SerializeField] private LegendOfBlood.UIPanelType panelToOpen;

    [Header("Scene Settings (If ActionType is LoadScene)")]
    [SerializeField] private string sceneNameToLoad;
    
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null)
        {
            _button.onClick.AddListener(PerformAction);
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
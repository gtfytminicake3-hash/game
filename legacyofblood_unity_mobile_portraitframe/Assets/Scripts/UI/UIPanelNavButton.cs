using UnityEngine;
using UnityEngine.UI;

namespace LegendOfBlood {
    [RequireComponent(typeof(Button))]
    public class UIPanelNavButton : MonoBehaviour {
        public UIPanelType targetPanel;
        public bool isBackButton = false;
        public bool clearHistory = false;
        
        void Start() {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }
        
        void OnClick() {
            if (isBackButton) {
                if (GameManager.Instance != null && GameManager.Instance.UIManager != null) {
                    GameManager.Instance.UIManager.GoBack();
                }
                return;
            }

            if (targetPanel == UIPanelType.None) {
                if (GameManager.Instance != null && GameManager.Instance.UINotificationManager != null) {
                    GameManager.Instance.UINotificationManager.ShowNotification("Tính năng này đang phát triển!");
                }
                return;
            }

            if (GameManager.Instance != null && GameManager.Instance.UIManager != null) {
                UIPanelType actualTarget = targetPanel;
                if (actualTarget == UIPanelType.PopulationManager) 
                {
                    actualTarget = UIPanelType.Barrack;
                }
                GameManager.Instance.UIManager.ShowPanel(actualTarget, true);
            }
        }
    }
}

namespace LegendOfBlood
{
    using UnityEngine;
    using UnityEngine.UI;

    public class TutorialPanel : UIPanel
    {
        [Header("UI References")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button prevButton;
        [SerializeField] private UnityEngine.UI.Text tutorialText;

        private int currentStep = 0;
        private string[] tutorialSteps = new string[]
        {
            "Chào mừng đến với Legend of Blood! Đây là doanh trại của bạn.",
            "Tại đây bạn có thể xây dựng công trình, thu thập tài nguyên.",
            "Dùng Trại Lính để tuyển mộ anh hùng và Tháp Huấn Luyện để tăng kĩ năng.",
            "Hãy gửi các anh hùng đi Thám Hiểm để nhận về Vàng và Trang bị nhé!"
        };

        private void Awake()
        {
            PanelType = UIPanelType.Tutorial;
            if (closeButton != null) closeButton.onClick.AddListener(() => GameManager.Instance.UIManager.HidePanel(PanelType));
            if (nextButton != null) nextButton.onClick.AddListener(NextStep);
            if (prevButton != null) prevButton.onClick.AddListener(PrevStep);
        }

        private void OnEnable()
        {
            currentStep = 0;
            UpdateUI();
        }

        private void NextStep()
        {
            if (currentStep < tutorialSteps.Length - 1)
            {
                currentStep++;
                UpdateUI();
            }
            else
            {
                GameManager.Instance.UIManager.HidePanel(PanelType);
            }
        }

        private void PrevStep()
        {
            if (currentStep > 0)
            {
                currentStep--;
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            if (tutorialText != null) tutorialText.text = tutorialSteps[currentStep];
            if (prevButton != null) prevButton.gameObject.SetActive(currentStep > 0);
            
            if (nextButton != null)
            {
                var btnText = nextButton.GetComponentInChildren<UnityEngine.UI.Text>();
                if (btnText != null)
                {
                    btnText.text = (currentStep == tutorialSteps.Length - 1) ? "ĐÓNG" : "TIẾP THEO";
                }
            }
        }
    }
}

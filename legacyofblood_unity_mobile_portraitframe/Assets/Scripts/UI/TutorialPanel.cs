namespace LegendOfBlood
{
    using UnityEngine;
    using UnityEngine.UI;

    public class TutorialPanel : UIPanel
    {
        [Header("UI References")]
        [SerializeField] private UnityEngine.UI.Text panelTitleText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private UnityEngine.UI.Text nextButtonText;
        [SerializeField] private Button prevButton;
        [SerializeField] private UnityEngine.UI.Text prevButtonText;
        [SerializeField] private UnityEngine.UI.Text tutorialText;

        private int currentStep = 0;
        private string[] GetTutorialSteps()
        {
            return new string[]
            {
                global::LocalizationSystem.GetText("tutorial_step_1"),
                global::LocalizationSystem.GetText("tutorial_step_2"),
                global::LocalizationSystem.GetText("tutorial_step_3"),
                global::LocalizationSystem.GetText("tutorial_step_4")
            };
        }

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
            if (currentStep < GetTutorialSteps().Length - 1)
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
            var tutorialSteps = GetTutorialSteps();
            if (tutorialText != null) tutorialText.text = tutorialSteps[currentStep];
            if (prevButton != null) prevButton.gameObject.SetActive(currentStep > 0);
            
            if (panelTitleText != null) panelTitleText.text = global::LocalizationSystem.GetText("panel_title_tutorial");

            if (nextButtonText != null)
            {
                nextButtonText.text = (currentStep == tutorialSteps.Length - 1) ? global::LocalizationSystem.GetText("btn_close") : global::LocalizationSystem.GetText("btn_next");
            }

            if (prevButtonText != null)
            {
                prevButtonText.text = global::LocalizationSystem.GetText("btn_prev");
            }
        }
    }
}

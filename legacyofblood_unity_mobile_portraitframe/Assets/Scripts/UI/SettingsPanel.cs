namespace LegendOfBlood
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class SettingsPanel : UIPanel
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Button changeLanguageButton;

        private void Awake()
        {
            PanelType = UIPanelType.Settings;
            if (closeButton != null) closeButton.onClick.AddListener(() => GameManager.Instance.UIManager.HidePanel(PanelType));
            if (changeLanguageButton != null) changeLanguageButton.onClick.AddListener(OnChangeLanguage);
            if (volumeSlider != null) volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        private void OnEnable()
        {
            if (volumeSlider != null)
            {
                volumeSlider.value = AudioListener.volume;
            }
            if (titleText != null)
            {
                titleText.text = global::LocalizationSystem.GetText("settings_title");
            }

            if (closeButton != null)
            {
                var txt = closeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = global::LocalizationSystem.GetText("btn_close");
            }

            if (changeLanguageButton != null)
            {
                var txt = changeLanguageButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = global::LocalizationSystem.GetText("btn_change_language");
            }
        }

        private void OnVolumeChanged(float value)
        {
            AudioListener.volume = value;
        }

        private void OnChangeLanguage()
        {
            Language currentLang = LanguageManager.CurrentLanguage;
            Language nextLang = currentLang == Language.English ? Language.Vietnamese : Language.English;
            LanguageManager.CurrentLanguage = nextLang;
            global::LocalizationSystem.LoadLocalizedText((global::Language)(int)nextLang);
            
            // Force refresh all active UI panels to apply new language
            if (GameManager.Instance != null && GameManager.Instance.UIManager != null)
            {
                GameManager.Instance.UIManager.RefreshAllActivePanels();
            }
        }
    }
}

namespace LegendOfBlood
{
    using UnityEngine;
    using UnityEngine.UI;

    public class SettingsPanel : UIPanel
    {
        [Header("UI References")]
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
        }
    }
}

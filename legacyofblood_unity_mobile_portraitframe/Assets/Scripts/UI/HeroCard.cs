namespace LegendOfBlood
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class HeroCard : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI combatPowerText;
        [SerializeField] private Image genderIcon;
        [SerializeField] private Image professionIcon;
        [SerializeField] private Image avatarImage;
        [SerializeField] private Image cardFrame;
        [SerializeField] private GameObject busyIndicator;
        [SerializeField] private Button cardButton;

        [Header("Asset References")]
        [SerializeField] private Sprite maleIcon;
        [SerializeField] private Sprite femaleIcon;

        [Header("Class Icons")]
        [SerializeField] private Sprite warriorIcon;
        [SerializeField] private Sprite archerIcon;
        [SerializeField] private Sprite mageIcon;
        [SerializeField] private Sprite healerIcon;

        private HeroData _heroData;
        private bool _missingRefWarningLogged;

        public void Setup(HeroData heroData)
        {
            if (heroData == null)
            {
                Debug.LogError("Co gang thiet lap HeroCard voi du lieu null!");
                gameObject.SetActive(false);
                return;
            }

            EnsureReferences();
            _heroData = heroData;
            UpdateUI();

            if (cardButton != null)
            {
                cardButton.onClick.RemoveListener(OnCardClicked);
                cardButton.onClick.AddListener(OnCardClicked);
            }
        }

        public void Clear()
        {
            _heroData = null;
            if (nameText != null) nameText.text = "???";
            if (levelText != null) levelText.text = "";
            if (combatPowerText != null) combatPowerText.text = "";
            if (genderIcon != null) genderIcon.sprite = null;
            if (professionIcon != null) professionIcon.sprite = null;
            if (avatarImage != null)
            {
                avatarImage.sprite = null;
                avatarImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
            if (cardFrame != null) cardFrame.color = new Color(0.2f, 0.2f, 0.3f, 1f);
            if (busyIndicator != null) busyIndicator.SetActive(false);
            if (cardButton != null) cardButton.onClick.RemoveListener(OnCardClicked);
        }

        private void UpdateUI()
        {
            EnsureReferences();

            if (nameText != null) nameText.text = _heroData.heroName;
            if (levelText != null) levelText.text = string.Format(global::LocalizationSystem.GetText("level_format_short"), _heroData.level);
            if (combatPowerText != null) combatPowerText.text = string.Format(global::LocalizationSystem.GetText("cp_format_short"), _heroData.GetCombatPower());
            if (genderIcon != null) genderIcon.sprite = GetGenderSprite(_heroData.gender);
            if (professionIcon != null) professionIcon.sprite = GetProfessionSprite(_heroData.profession);
            if (avatarImage != null) avatarImage.sprite = _heroData.GetAvatarSprite();
            if (cardFrame != null) cardFrame.color = GetRarityColor(_heroData.potential);
            if (busyIndicator != null) busyIndicator.SetActive(_heroData.IsBusy());
        }

        private void OnCardClicked()
        {
            Debug.Log($"Clicked on Hero: {_heroData.heroName} (ID: {_heroData.id})");
            GameManager.Instance.UIManager.ShowPanel(UIPanelType.HeroInfo, false);
            EventManager.TriggerEvent(GameEvents.OnHeroCardClicked, _heroData);
        }

        private Sprite GetGenderSprite(Gender gender)
        {
            switch (gender)
            {
                case Gender.Male: return maleIcon;
                case Gender.Female: return femaleIcon;
                default: return null;
            }
        }

        private Sprite GetProfessionSprite(Profession profession)
        {
            switch (profession)
            {
                case Profession.Warrior: return warriorIcon;
                case Profession.Archer: return archerIcon;
                case Profession.Mage: return mageIcon;
                case Profession.Healer: return healerIcon;
                default: return null;
            }
        }

        private Color GetRarityColor(int potential)
        {
            if (potential >= 25) return new Color(0f, 1f, 1f, 1f);
            if (potential >= 21) return new Color(1f, 0.2f, 0.2f, 1f);
            if (potential >= 15) return new Color(1f, 0.84f, 0f, 1f);
            if (potential >= 12) return new Color(0.6f, 0.2f, 0.8f, 1f);
            if (potential >= 8) return new Color(0.2f, 0.5f, 1f, 1f);
            if (potential >= 5) return new Color(0.3f, 0.8f, 0.3f, 1f);
            return new Color(0.5f, 0.5f, 0.5f, 1f);
        }

        private void Awake()
        {
            EnsureReferences();
            LogMissingReferencesOnce();
        }

        private void OnValidate()
        {
            EnsureReferences();
        }

        private void EnsureReferences()
        {
            if (cardButton == null) cardButton = GetComponent<Button>();
            if (nameText == null) nameText = FindTMP("name");
            if (levelText == null) levelText = FindTMP("level");
            if (combatPowerText == null) combatPowerText = FindTMP("combat") ?? FindTMP("cp");
            if (professionIcon == null) professionIcon = FindImage("profession") ?? FindImage("class");

            if (Application.isPlaying)
            {
                if (combatPowerText == null) combatPowerText = CreateRuntimeText("CombatPowerText_Runtime", new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.16f), 22f);
                if (professionIcon == null) professionIcon = CreateRuntimeImage("ProfessionIcon_Runtime", new Vector2(0.72f, 0.72f), new Vector2(0.95f, 0.95f));
            }
        }

        private TextMeshProUGUI FindTMP(string keyword)
        {
            var texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null && texts[i].name.ToLowerInvariant().Contains(keyword)) return texts[i];
            }
            return null;
        }

        private Image FindImage(string keyword)
        {
            var images = GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] != null && images[i].name.ToLowerInvariant().Contains(keyword)) return images[i];
            }
            return null;
        }

        private void LogMissingReferencesOnce()
        {
            if (_missingRefWarningLogged) return;
            if (combatPowerText == null) Debug.LogWarning($"[HeroCard] combatPowerText chua duoc gan tren {name}", this);
            if (professionIcon == null) Debug.LogWarning($"[HeroCard] professionIcon chua duoc gan tren {name}", this);
            _missingRefWarningLogged = true;
        }

        private TextMeshProUGUI CreateRuntimeText(string objectName, Vector2 anchorMin, Vector2 anchorMax, float fontSize)
        {
            GameObject textObj = new GameObject(objectName);
            textObj.transform.SetParent(transform, false);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.raycastTarget = false;
            RectTransform rt = text.rectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return text;
        }

        private Image CreateRuntimeImage(string objectName, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject imageObj = new GameObject(objectName);
            imageObj.transform.SetParent(transform, false);
            Image image = imageObj.AddComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;
            RectTransform rt = image.rectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return image;
        }
    }
}

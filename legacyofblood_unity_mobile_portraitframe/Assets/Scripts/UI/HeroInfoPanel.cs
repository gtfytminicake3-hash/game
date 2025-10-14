namespace LegendOfBlood
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class HeroInfoPanel : MonoBehaviour
    {
        [Header("Main Info References")]
        [SerializeField] private TextMeshProUGUI heroNameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI professionText;
        [SerializeField] private TextMeshProUGUI genderText;
        [SerializeField] private Image heroAvatarImage;

        [Header("Stats References")]
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI atkText;
        [SerializeField] private TextMeshProUGUI defText;
        [SerializeField] private TextMeshProUGUI spdText;
        [SerializeField] private TextMeshProUGUI potentialText;
        
        [Header("Traits & Skills References")]
        [SerializeField] private GameObject infoItemPrefab;
        [SerializeField] private Transform traitsContainer;
        [SerializeField] private Transform skillsContainer;
        
        [Header("Actions")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button statAllocationButton;
        [SerializeField] private Button traitUpgradeButton;

        [Header("Panels")]
        [SerializeField] private StatAllocationPanel statAllocationPanel;
        [SerializeField] private TraitUpgradePanel traitUpgradePanel;

        private HeroData _currentHero;
        private List<GameObject> _instantiatedInfoItems = new List<GameObject>();

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
            if (statAllocationButton != null) statAllocationButton.onClick.AddListener(OpenStatAllocationPanel);
            if (traitUpgradeButton != null) traitUpgradeButton.onClick.AddListener(OpenTraitUpgradePanel);
        }
        
        private void OnEnable()
        {
            EventManager.StartListening<HeroData>(GameEvents.OnHeroCardClicked, OnHeroSelected);
        }

        private void OnDisable()
        {
            EventManager.StopListening<HeroData>(GameEvents.OnHeroCardClicked, OnHeroSelected);
        }
        
        private void OnHeroSelected(HeroData heroData)
        {
            if (!gameObject.activeInHierarchy)
            {
                GameManager.Instance.UIManager.ShowPanel(UIPanelType.HeroInfo, false);
            }
            PopulateData(heroData);
        }
        
        private void PopulateData(HeroData hero)
        {
            _currentHero = hero;
            if (_currentHero == null)
            {
                ClosePanel();
                return;
            }

            heroNameText.text = _currentHero.heroName;
            levelText.text = string.Format(global::LocalizationSystem.GetText("level_format"), _currentHero.level);
            professionText.text = string.Format(global::LocalizationSystem.GetText("profession_format"), _currentHero.profession);
            genderText.text = string.Format(global::LocalizationSystem.GetText("gender_format"), _currentHero.gender);

            HeroStats baseStats = _currentHero.baseStats;
            HeroStats finalStats = _currentHero.GetFinalStats();
            
            hpText.text = string.Format(global::LocalizationSystem.GetText("stats_hp_format_detailed"), baseStats.hp, finalStats.hp - baseStats.hp);
            atkText.text = string.Format(global::LocalizationSystem.GetText("stats_atk_format_detailed"), baseStats.atk, finalStats.atk - baseStats.atk);
            defText.text = string.Format(global::LocalizationSystem.GetText("stats_def_format_detailed"), baseStats.def, finalStats.def - baseStats.def);
            spdText.text = string.Format(global::LocalizationSystem.GetText("stats_spd_format_detailed"), baseStats.spd, finalStats.spd - baseStats.spd);
            potentialText.text = string.Format(global::LocalizationSystem.GetText("stats_potential_format"), _currentHero.potential);

            statAllocationButton.gameObject.SetActive(_currentHero.freeStatPoints > 0);
            traitUpgradeButton.gameObject.SetActive(_currentHero.level == 60 || _currentHero.level == 100);

            ClearInfoItems();
            PopulateInfoList(traitsContainer, _currentHero.traitIDs, true);
            PopulateInfoList(skillsContainer, _currentHero.skillIDs, false);
        }

        private void PopulateInfoList(Transform container, List<string> idList, bool isTrait)
        {
            if (infoItemPrefab == null) return;

            foreach (string id in idList)
            {
                string itemName = global::LocalizationSystem.GetText("unknown");
                string itemDescription = global::LocalizationSystem.GetText("description_not_found");

                if (isTrait)
                {
                    Trait trait = DataManager.Instance.GetTraitByID(id);
                    if (trait != null) { itemName = trait.traitName; itemDescription = trait.description; }
                }
                else
                {
                    Skill skill = DataManager.Instance.GetSkillByID(id);
                    if (skill != null) { itemName = skill.skillName; itemDescription = skill.description; }
                }

                GameObject itemInstance = Instantiate(infoItemPrefab, container);
                var texts = itemInstance.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 2) { texts[0].text = itemName; texts[1].text = itemDescription; }
                _instantiatedInfoItems.Add(itemInstance);
            }
        }

        private void ClearInfoItems()
        {
            foreach (var item in _instantiatedInfoItems) { Destroy(item); }
            _instantiatedInfoItems.Clear();
        }

        private void OpenStatAllocationPanel()
        {
            if (statAllocationPanel != null && _currentHero != null)
            {
                statAllocationPanel.Show(_currentHero);
            }
        }

        private void OpenTraitUpgradePanel()
        {
            if (traitUpgradePanel != null && _currentHero != null)
            {
                traitUpgradePanel.Show(_currentHero);
            }
        }

        private void ClosePanel()
        {
            GameManager.Instance.UIManager.HidePanel(UIPanelType.HeroInfo);
        }
    }
}

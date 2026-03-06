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
        [SerializeField] private TextMeshProUGUI evasionText; // THÊM DÒNG NÀY
        [SerializeField] private TextMeshProUGUI dmgReductionText; // THÊM DÒNG NÀY
        [SerializeField] private TextMeshProUGUI dmgIncreaseText; // THÊM DÒNG NÀY
        
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
            // AUTO-WIRE: Tự động đánh hơi tìm các Nút bị rớt (không được gắn trong Inspector)
            if (statAllocationButton == null || traitUpgradeButton == null)
            {
                Button[] allButtons = GetComponentsInChildren<Button>(true);
                foreach(var btn in allButtons)
                {
                    string btnName = btn.gameObject.name.ToLower();
                    if (statAllocationButton == null && (btnName.Contains("stat") || btnName.Contains("alloc")))
                        statAllocationButton = btn;
                    if (traitUpgradeButton == null && (btnName.Contains("trait") || btnName.Contains("upgrade")))
                        traitUpgradeButton = btn;
                }
            }

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

            // THÊM CÁC DÒNG NÀY ĐỂ HIỂN THỊ CHỈ SỐ MỚI (CÓ BỌC CHỐNG NULL)
            if (evasionText != null) 
                evasionText.text = string.Format(global::LocalizationSystem.GetText("stats_evasion_format"), _currentHero.evasionRate);
            if (dmgReductionText != null) 
                dmgReductionText.text = string.Format(global::LocalizationSystem.GetText("stats_dmg_reduction_format"), _currentHero.damageReduction);
            if (dmgIncreaseText != null) 
                dmgIncreaseText.text = string.Format(global::LocalizationSystem.GetText("stats_dmg_increase_format"), _currentHero.damageIncrease);

            if (statAllocationButton != null)
                statAllocationButton.gameObject.SetActive(_currentHero.freeStatPoints > 0);
            if (traitUpgradeButton != null)
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
            if (statAllocationPanel == null)
            {
                statAllocationPanel = FindFirstObjectByType<StatAllocationPanel>(FindObjectsInactive.Include);
                if (statAllocationPanel == null) 
                {
                    Debug.LogError("Chưa tạo Panel tên là `StatAllocationPanel` trong Scene. Hãy tạo ra 1 Panel rỗng và dán script `StatAllocationPanel` vào!");
                    return;
                }
            }

            if (_currentHero != null)
            {
                statAllocationPanel.Show(_currentHero);
            }
        }

        private void OpenTraitUpgradePanel()
        {
            if (traitUpgradePanel == null)
            {
                traitUpgradePanel = FindFirstObjectByType<TraitUpgradePanel>(FindObjectsInactive.Include);
                if (traitUpgradePanel == null) 
                {
                    Debug.LogError("Chưa tạo Panel tên là `TraitUpgradePanel` trong Scene. Hãy tạo ra 1 Panel rỗng và dán script `TraitUpgradePanel` vào!");
                    return;
                }
            }

            if (_currentHero != null)
            {
                traitUpgradePanel.Show(_currentHero);
            }
        }

        private void ClosePanel()
        {
            GameManager.Instance.UIManager.GoBack();
        }
    }
}

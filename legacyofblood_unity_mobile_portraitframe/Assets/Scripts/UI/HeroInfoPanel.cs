namespace LegendOfBlood
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class HeroInfoPanel : UIPanel
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
        
        [Header("Equipment Slots")]
        [SerializeField] private HeroEquipmentSlot[] equipmentSlots;
        
        [Header("Actions")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button statAllocationButton;
        [SerializeField] private Button traitUpgradeButton;
        [SerializeField] private Button useExpItemButton; 

        [Header("Panels")]
        [SerializeField] private StatAllocationPanel statAllocationPanel;
        [SerializeField] private TraitUpgradePanel traitUpgradePanel;

        private HeroData _currentHero;
        private List<GameObject> _instantiatedInfoItems = new List<GameObject>();

        private void Awake()
        {
            PanelType = UIPanelType.HeroInfo;
            // AUTO-WIRE: Tự động đánh hơi tìm các Nút bị rớt (không được gắn trong Inspector)
            if (statAllocationButton == null || traitUpgradeButton == null || useExpItemButton == null)
            {
                Button[] allButtons = GetComponentsInChildren<Button>(true);
                foreach(var btn in allButtons)
                {
                    string btnName = btn.gameObject.name.ToLower();
                    if (statAllocationButton == null && (btnName.Contains("stat") || btnName.Contains("alloc")))
                        statAllocationButton = btn;
                    if (traitUpgradeButton == null && (btnName.Contains("trait") || btnName.Contains("upgrade")))
                        traitUpgradeButton = btn;
                    if (useExpItemButton == null && (btnName.Contains("exp") || btnName.Contains("item")))
                        useExpItemButton = btn;
                }
            }

            if (equipmentSlots == null || equipmentSlots.Length == 0)
            {
                equipmentSlots = GetComponentsInChildren<HeroEquipmentSlot>(true);
            }

            if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
            if (statAllocationButton != null) statAllocationButton.onClick.AddListener(OpenStatAllocationPanel);
            if (traitUpgradeButton != null) traitUpgradeButton.onClick.AddListener(OpenTraitUpgradePanel);
            if (useExpItemButton != null) useExpItemButton.onClick.AddListener(OnUseExpItemClicked);
        }
        
        private void OnEnable()
        {
            EventManager.StartListening<HeroData>(GameEvents.OnHeroCardClicked, OnHeroSelected);
            DataManager.OnHeroStatsChanged += OnHeroStatsChangedEvent;
        }

        private void OnDisable()
        {
            EventManager.StopListening<HeroData>(GameEvents.OnHeroCardClicked, OnHeroSelected);
            DataManager.OnHeroStatsChanged -= OnHeroStatsChangedEvent;
        }

        private void OnHeroStatsChangedEvent(HeroData hero)
        {
            if (gameObject.activeInHierarchy && _currentHero != null && hero != null && hero.id == _currentHero.id)
            {
                PopulateData(_currentHero);
            }
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
            
            if (heroAvatarImage != null)
            {
                heroAvatarImage.sprite = _currentHero.GetAvatarSprite();
            }

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
            
            if (useExpItemButton != null)
            {
                // Only active if hero is mature and not max level
                bool canUseExp = _currentHero.isMature && _currentHero.level < 100;
                useExpItemButton.gameObject.SetActive(canUseExp);
            }
            
            // --- CẬP NHẬT GIAO DIỆN TRANG BỊ ---
            if (equipmentSlots != null)
            {
                foreach (var slot in equipmentSlots)
                {
                    if (slot != null) slot.Setup(_currentHero);
                }
            }

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
                Sprite itemIcon = null;

                if (isTrait)
                {
                    Trait trait = DataManager.Instance.GetTraitByID(id);
                    if (trait != null) { itemName = global::LocalizationSystem.GetText(trait.traitName); itemDescription = global::LocalizationSystem.GetText(trait.description); itemIcon = trait.icon; }
                }
                else
                {
                    Skill skill = DataManager.Instance.GetSkillByID(id);
                    if (skill != null) { 
                        itemName = global::LocalizationSystem.GetText(skill.skillName); 
                        itemDescription = global::LocalizationSystem.GetText(skill.description); 
                        itemIcon = skill.icon;
                        Debug.Log($"[DEBUG] Skill loaded: {skill.id}, icon is null: {skill.icon == null}");
                    }
                    else
                    {
                        Debug.LogWarning($"[DEBUG] Skill with ID {id} was completely null!");
                    }
                }

                GameObject itemInstance = Instantiate(infoItemPrefab, container);
                
                Image iconImage = itemInstance.GetComponent<Image>();
                if (iconImage != null && itemIcon != null)
                {
                    iconImage.sprite = itemIcon;
                }

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

        private void OnUseExpItemClicked()
        {
            if (_currentHero == null) return;
            
            // Simple logic: Use the largest EXP book available
            // In a full game, this would open an item selection panel
            string[] expPotions = { "IT_EXP_BOOK_L", "IT_EXP_BOOK_M", "IT_EXP_BOOK_S" };
            
            bool itemUsed = false;
            foreach(string itemId in expPotions)
            {
                ItemData item = null;
                if(DataManager.Instance.AllItems.TryGetValue(itemId, out item) && item.type == ItemType.ExpPotion)
                {
                    if (GameManager.Instance.InventoryManager.UseItem(itemId, 1))
                    {
                        // Assume expValue is stored in speedUpValueInSeconds or similar field for generic items, 
                        // or we hardcode the values based on ID if the struct doesn't have an exp field.
                        int expGained = itemId == "IT_EXP_BOOK_L" ? 1000 : (itemId == "IT_EXP_BOOK_M" ? 500 : 100);
                        _currentHero.AddExperience(expGained);
                        Debug.Log($"Used {item.itemName} on {_currentHero.heroName}. Gained {expGained} EXP.");
                        
                        GameManager.Instance.UINotificationManager?.ShowNotification($"Sử dụng {item.itemName} thành công!");
                        PopulateData(_currentHero); // Refresh UI
                        itemUsed = true;
                        break;
                    }
                }
            }

            if (!itemUsed)
            {
                GameManager.Instance.UINotificationManager?.ShowNotification("Không có Vật phẩm Kinh nghiệm nào trong Túi!");
            }
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

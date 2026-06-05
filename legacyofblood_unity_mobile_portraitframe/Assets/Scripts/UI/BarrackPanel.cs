namespace LegendOfBlood
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using LegendOfBlood.Utils;

    public class BarrackPanel : UIPanel
    {
        [Header("UI References")]
        [SerializeField] private TMPro.TextMeshProUGUI panelTitleText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button populationManagerButton;
        [SerializeField] private Transform heroListContainer;

        [Header("Upgrade Feature")]
        [SerializeField] private Button upgradeBuildingButton;
        [SerializeField] private string associatedBuildingId = "Barracks";

        [Header("Prefabs")]
        [SerializeField] private GameObject heroCardPrefab;

        private UIListPooler<HeroData, HeroCard> _heroPooler;
        private readonly HashSet<string> _selectedDismissHeroIds = new HashSet<string>();
        private Button _dismissSelectedButton;
        private TextMeshProUGUI _dismissSelectedButtonText;

        private enum SortType { LevelDesc, LevelAsc, CPDesc, CPAsc }
        private LegendOfBlood.Profession? _currentFilter = null;
        private SortType _currentSort = SortType.LevelDesc;

        [Header("Filters & Status")]
        [SerializeField] private Button sortButton;
        [SerializeField] private TMPro.TextMeshProUGUI sortButtonText;
        [SerializeField] private Button filterButton;
        [SerializeField] private TMPro.TextMeshProUGUI filterButtonText;
        [SerializeField] private TMPro.TextMeshProUGUI populationCountText;

        private void Awake()
        {
            PanelType = UIPanelType.Barrack;
            _heroPooler = new UIListPooler<HeroData, HeroCard>(heroCardPrefab, heroListContainer, (card, data) => {
                card.Setup(data);
                SetupDismissToggle(card, data);
            });
            EnsureDismissControls();
        }

        protected override void Start()
        {
            base.Start();
            EnsureUpgradeButtonReference();
            EnsureSortFilterReferences();
            if (panelTitleText != null) panelTitleText.text = LocalizationSystem.GetText("panel_title_barrack");
            
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }

            if (sortButton != null)
            {
                sortButton.onClick.AddListener(OnSortClicked);
                UpdateSortText();
            }

            if (filterButton != null)
            {
                filterButton.onClick.AddListener(OnFilterClicked);
                UpdateFilterText();
            }

            if (upgradeBuildingButton != null)
            {
                upgradeBuildingButton.onClick.RemoveAllListeners();
                upgradeBuildingButton.onClick.AddListener(OpenBarracksUpgradePanel);
            }

            if (populationManagerButton != null)
            {
                populationManagerButton.onClick.RemoveAllListeners();
                populationManagerButton.onClick.AddListener(() => GameManager.Instance.UIManager.ShowPanel(UIPanelType.PopulationManager, false));
            }
        }

        private void OnSortClicked()
        {
            int nextSort = ((int)_currentSort + 1) % 4;
            _currentSort = (SortType)nextSort;
            UpdateSortText();
            RefreshHeroList();
        }

        private void OnFilterClicked()
        {
            if (_currentFilter == null) _currentFilter = LegendOfBlood.Profession.Warrior;
            else if (_currentFilter == LegendOfBlood.Profession.Warrior) _currentFilter = LegendOfBlood.Profession.Archer;
            else if (_currentFilter == LegendOfBlood.Profession.Archer) _currentFilter = LegendOfBlood.Profession.Mage;
            else if (_currentFilter == LegendOfBlood.Profession.Mage) _currentFilter = LegendOfBlood.Profession.Healer;
            else _currentFilter = null;
            UpdateFilterText();
            RefreshHeroList();
        }

        private void UpdateSortText()
        {
            if (sortButtonText != null)
            {
                string sortName = _currentSort switch
                {
                    SortType.LevelDesc => "Lv \u25BC",
                    SortType.LevelAsc => "Lv \u25B2",
                    SortType.CPDesc => "CP \u25BC",
                    SortType.CPAsc => "CP \u25B2",
                    _ => "Sort"
                };
                sortButtonText.text = $"Lọc:\n{sortName}";
            }
        }
        
        private void UpdateFilterText()
        {
            if (filterButtonText != null)
            {
                string filterName = _currentFilter.HasValue ? _currentFilter.Value.ToString() : "Tất cả";
                filterButtonText.text = $"Hệ:\n{filterName}";
            }
        }

        private void OnEnable()
        {
            EnsureDismissControls();
            UpdatePopulationDisplay();
            EventManager.StartListening(GameEvents.OnPlayerDataLoaded, RefreshHeroList);
            EventManager.StartListening(GameEvents.OnHeroListChanged, RefreshHeroList);
            RefreshHeroList();
        }

        private void OnDisable()
        {
            EventManager.StopListening(GameEvents.OnPlayerDataLoaded, RefreshHeroList);
            EventManager.StopListening(GameEvents.OnHeroListChanged, RefreshHeroList);
        }

        private void UpdatePopulationDisplay()
        {
            if (populationCountText != null && DataManager.Instance != null)
            {
                int maxCapacity = DataManager.Instance.GetPopulationCapacity();
                int currentCount = DataManager.Instance.AllHeroes?.Count ?? 0;
                populationCountText.text = $"Heroes: {currentCount} / {maxCapacity}";
            }
        }

        public void Hide()
        {
            GameManager.Instance.UIManager.GoBack();
        }

        private void OpenBarracksUpgradePanel()
        {
            if (GameManager.Instance == null || GameManager.Instance.BuildingSystem == null)
            {
                GameManager.Instance?.UINotificationManager?.ShowNotification("BuildingSystem chua san sang.");
                return;
            }

            Building target = null;
            if (DataManager.Instance != null && DataManager.Instance.AllBuildings != null)
            {
                target = DataManager.Instance.AllBuildings.Find(b => b != null && (b.type == BuildingType.Barracks || b.id == "Barracks" || b.id == "Barrack"));
                if (target == null)
                {
                    target = DataManager.Instance.AllBuildings.Find(b => b != null && b.id == associatedBuildingId);
                }
            }

            if (target == null)
            {
                GameManager.Instance.UINotificationManager?.ShowNotification("Khong tim thay nha Barracks.");
                return;
            }

            GameManager.Instance.UIManager.ShowPanel(UIPanelType.BuildingUpgrade, false);
            var panel = GameManager.Instance.UIManager.GetPanel<BuildingUpgradePanel>(UIPanelType.BuildingUpgrade);
            if (panel != null) panel.Setup(target.id);
        }

        private void EnsureUpgradeButtonReference()
        {
            if (upgradeBuildingButton != null) return;

            Button[] buttons = GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                if (button == null) continue;

                string buttonName = button.gameObject.name.ToLowerInvariant();
                if (buttonName.Contains("upgrade"))
                {
                    upgradeBuildingButton = button;
                    return;
                }
            }
        }

        private void EnsureSortFilterReferences()
        {
            if (sortButton == null) sortButton = FindOrCreateButton("SortButton", "Sort");
            if (filterButton == null) filterButton = FindOrCreateButton("FilterButton", "Filter");
            if (sortButtonText == null) sortButtonText = FindOrCreateButtonText(sortButton);
            if (filterButtonText == null) filterButtonText = FindOrCreateButtonText(filterButton);
        }

        private Button FindOrCreateButton(params string[] names)
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            for (int n = 0; n < names.Length; n++)
            {
                string targetName = names[n].ToLowerInvariant();
                for (int i = 0; i < buttons.Length; i++)
                {
                    Button button = buttons[i];
                    if (button != null && button.gameObject.name.ToLowerInvariant().Contains(targetName))
                    {
                        return button;
                    }
                }
            }

            Transform target = FindTransformByName(names);
            if (target != null)
            {
                Image image = target.GetComponent<Image>();
                if (image == null) image = target.gameObject.AddComponent<Image>();
                image.raycastTarget = true;
                return target.gameObject.AddComponent<Button>();
            }

            return null;
        }

        private TextMeshProUGUI FindOrCreateButtonText(Button button)
        {
            if (button == null) return null;

            TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (text != null) return text;

            GameObject textObj = new GameObject("Text_Runtime");
            textObj.transform.SetParent(button.transform, false);
            text = textObj.AddComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 28f;
            text.fontStyle = FontStyles.Bold;
            text.color = Color.white;
            text.raycastTarget = false;

            RectTransform rt = text.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return text;
        }

        private Transform FindTransformByName(params string[] names)
        {
            Transform[] children = GetComponentsInChildren<Transform>(true);
            for (int n = 0; n < names.Length; n++)
            {
                string targetName = names[n].ToLowerInvariant();
                for (int i = 0; i < children.Length; i++)
                {
                    Transform child = children[i];
                    if (child != null && child.gameObject.name.ToLowerInvariant().Contains(targetName))
                    {
                        return child;
                    }
                }
            }

            return null;
        }

        private void EnsureDismissControls()
        {
            if (_dismissSelectedButton != null) return;

            Transform parent = panelTitleText != null ? panelTitleText.transform.parent : transform;
            GameObject buttonObj = new GameObject("Btn_DismissSelected_Runtime");
            buttonObj.transform.SetParent(parent, false);
            buttonObj.transform.SetAsLastSibling();

            RectTransform rt = buttonObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(300f, 86f);
            rt.anchoredPosition = new Vector2(-32f, -32f);

            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.65f, 0.12f, 0.12f, 0.95f);

            _dismissSelectedButton = buttonObj.AddComponent<Button>();
            _dismissSelectedButton.onClick.AddListener(DismissSelectedHeroes);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            _dismissSelectedButtonText = textObj.AddComponent<TextMeshProUGUI>();
            _dismissSelectedButtonText.alignment = TextAlignmentOptions.Center;
            _dismissSelectedButtonText.color = Color.white;
            _dismissSelectedButtonText.fontSize = 30f;
            _dismissSelectedButtonText.fontStyle = FontStyles.Bold;

            RectTransform textRt = _dismissSelectedButtonText.rectTransform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            UpdateDismissButtonState();
        }

        private void SetupDismissToggle(HeroCard card, HeroData hero)
        {
            if (card == null || hero == null) return;

            Toggle toggle = null;
            Toggle[] toggles = card.GetComponentsInChildren<Toggle>(true);
            for (int i = 0; i < toggles.Length; i++)
            {
                if (toggles[i] != null && toggles[i].gameObject.name == "DismissToggle_Runtime")
                {
                    toggle = toggles[i];
                    break;
                }
            }

            if (toggle == null)
            {
                toggle = CreateDismissToggle(card.transform);
            }

            ConfigureDismissToggle(toggle);
            bool canDismiss = !hero.IsBusy();
            toggle.gameObject.SetActive(true);
            toggle.transform.SetAsLastSibling();
            toggle.interactable = canDismiss;
            toggle.onValueChanged.RemoveAllListeners();
            toggle.SetIsOnWithoutNotify(canDismiss && _selectedDismissHeroIds.Contains(hero.id));
            toggle.onValueChanged.AddListener(isOn => {
                if (isOn) _selectedDismissHeroIds.Add(hero.id);
                else _selectedDismissHeroIds.Remove(hero.id);
                UpdateDismissButtonState();
            });
        }

        private Toggle CreateDismissToggle(Transform parent)
        {
            GameObject toggleObj = new GameObject("DismissToggle_Runtime");
            toggleObj.transform.SetParent(parent, false);
            toggleObj.transform.SetAsLastSibling();

            RectTransform rt = toggleObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(72f, 72f);
            rt.anchoredPosition = new Vector2(-14f, -14f);

            Image background = toggleObj.AddComponent<Image>();
            background.color = new Color(0.02f, 0.02f, 0.02f, 0.88f);
            toggleObj.AddComponent<Outline>().effectColor = new Color(1f, 0.85f, 0.2f, 1f);

            Toggle toggle = toggleObj.AddComponent<Toggle>();
            toggle.targetGraphic = background;

            GameObject checkObj = new GameObject("Checkmark");
            checkObj.transform.SetParent(toggleObj.transform, false);
            TextMeshProUGUI check = checkObj.AddComponent<TextMeshProUGUI>();
            check.text = "✓";
            check.alignment = TextAlignmentOptions.Center;
            check.fontSize = 48f;
            check.fontStyle = FontStyles.Bold;
            check.color = new Color(1f, 0.88f, 0.25f, 1f);

            RectTransform checkRt = check.rectTransform;
            checkRt.anchorMin = Vector2.zero;
            checkRt.anchorMax = Vector2.one;
            checkRt.offsetMin = Vector2.zero;
            checkRt.offsetMax = Vector2.zero;

            toggle.graphic = check;
            return toggle;
        }

        private void ConfigureDismissToggle(Toggle toggle)
        {
            if (toggle == null) return;

            RectTransform rt = toggle.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
                rt.sizeDelta = new Vector2(72f, 72f);
                rt.anchoredPosition = new Vector2(-14f, -14f);
            }

            Image background = toggle.GetComponent<Image>();
            if (background != null)
            {
                background.raycastTarget = true;
                background.color = new Color(0.02f, 0.02f, 0.02f, 0.88f);
            }
        }

        private void DismissSelectedHeroes()
        {
            if (_selectedDismissHeroIds.Count == 0 || DataManager.Instance == null) return;

            List<string> ids = new List<string>(_selectedDismissHeroIds);
            int removedCount = DataManager.Instance.RemoveHeroes(ids);
            _selectedDismissHeroIds.Clear();

            if (removedCount > 0)
            {
                DataManager.Instance.SavePlayerData();
                GameManager.Instance.UINotificationManager?.ShowNotification($"Dismissed {removedCount} heroes.");
            }
            else
            {
                GameManager.Instance.UINotificationManager?.ShowNotification("No selectable heroes were dismissed.");
            }

            RefreshHeroList();
        }

        private void UpdateDismissButtonState()
        {
            if (_dismissSelectedButtonText != null)
            {
                _dismissSelectedButtonText.text = $"Dismiss ({_selectedDismissHeroIds.Count})";
            }

            if (_dismissSelectedButton != null)
            {
                _dismissSelectedButton.interactable = _selectedDismissHeroIds.Count > 0;
            }
        }

        private void RefreshHeroList()
        {
            if (DataManager.Instance == null || DataManager.Instance.AllHeroes == null) return;

            List<HeroData> allHeroes = DataManager.Instance.AllHeroes;
            List<HeroData> filteredHeroes = new List<HeroData>();
            
            UpdatePopulationDisplay();

            // Filter
            foreach(var h in allHeroes) 
            {
                if (_currentFilter == null) filteredHeroes.Add(h);
                else if (_currentFilter.Value == h.profession) filteredHeroes.Add(h);
            }

            // Sort
            switch (_currentSort)
            {
                case SortType.LevelDesc:
                    filteredHeroes.Sort((h1, h2) => h2.level.CompareTo(h1.level));
                    break;
                case SortType.LevelAsc:
                    filteredHeroes.Sort((h1, h2) => h1.level.CompareTo(h2.level));
                    break;
                case SortType.CPDesc:
                    filteredHeroes.Sort((h1, h2) => h2.GetCombatPower().CompareTo(h1.GetCombatPower()));
                    break;
                case SortType.CPAsc:
                    filteredHeroes.Sort((h1, h2) => h1.GetCombatPower().CompareTo(h2.GetCombatPower()));
                    break;
            }

            _heroPooler.Refresh(filteredHeroes);
            Debug.Log($"[BarrackPanel] Refreshed list with {_heroPooler.ActiveComponents.Count} pooled objects.");
        }
    }
}

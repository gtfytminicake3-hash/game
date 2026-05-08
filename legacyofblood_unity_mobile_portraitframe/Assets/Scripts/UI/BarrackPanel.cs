namespace LegendOfBlood
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class BarrackPanel : UIPanel
    {
        [Header("UI References")]
        [SerializeField] private TMPro.TextMeshProUGUI panelTitleText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button populationManagerButton; // Sẽ được tái sử dụng để hiển thị số lượng
        [SerializeField] private Transform heroListContainer;

        [Header("Upgrade Feature")]
        [SerializeField] private Button upgradeBuildingButton;
        [SerializeField] private string associatedBuildingId = "Barracks";

        [Header("Prefabs")]
        [SerializeField] private GameObject heroCardPrefab;

        private List<GameObject> _instantiatedHeroCards = new List<GameObject>();

        private enum SortType { LevelDesc, LevelAsc, CPDesc, CPAsc }
        private LegendOfBlood.Profession? _currentFilter = null;

        private SortType _currentSort = SortType.LevelDesc;

        private Button _sortButton;
        private TMPro.TextMeshProUGUI _sortButtonText;
        private Button _filterButton;
        private TMPro.TextMeshProUGUI _filterButtonText;
        private TMPro.TextMeshProUGUI _populationCountText;

        private void Awake()
        {
            PanelType = UIPanelType.Barrack;
        }

        protected override void Start()
        {
            base.Start();
            if (panelTitleText != null) panelTitleText.text = LocalizationSystem.GetText("panel_title_barrack");
            
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }
            if (populationManagerButton != null)
            {
                // Xóa bỏ các listener cũ mở panel PopulationManager
                populationManagerButton.onClick.RemoveAllListeners();
            }

            // Luôn cố gắng tìm PopulationText_Auto đệ quy
            if (_populationCountText == null)
            {
                var allTexts = GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                foreach(var t in allTexts)
                {
                    if (t.gameObject.name == "PopulationText_Auto")
                    {
                        _populationCountText = t;
                        break;
                    }
                }

                if (_populationCountText == null && populationManagerButton != null)
                    _populationCountText = populationManagerButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            }

            if (upgradeBuildingButton != null) 
            {
                upgradeBuildingButton.onClick.AddListener(OnUpgradeBuildingClicked);
                var upgTxt = upgradeBuildingButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (upgTxt != null) upgTxt.text = global::LocalizationSystem.GetText("btn_upgrade");
            }

            // Hook Sort and Filter buttons dynamically
            Transform sortBtnTr = transform.Find("SafeArea/ActionButtonsRow/SortButton");
            if (sortBtnTr != null)
            {
                _sortButton = sortBtnTr.GetComponent<Button>();
                if (_sortButton == null) _sortButton = sortBtnTr.gameObject.AddComponent<Button>();
                
                _sortButtonText = sortBtnTr.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (_sortButtonText == null)
                {
                    GameObject txtObj = new GameObject("Text");
                    txtObj.transform.SetParent(sortBtnTr, false);
                    _sortButtonText = txtObj.AddComponent<TMPro.TextMeshProUGUI>();
                    _sortButtonText.fontSize = 36;
                    _sortButtonText.alignment = TMPro.TextAlignmentOptions.Center;
                    _sortButtonText.color = Color.white;
                    RectTransform rt = txtObj.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                }

                _sortButton.onClick.AddListener(OnSortClicked);
                UpdateSortText();
            }

            Transform filterBtnTr = transform.Find("SafeArea/ActionButtonsRow/FilterButton");
            if (filterBtnTr != null)
            {
                _filterButton = filterBtnTr.GetComponent<Button>();
                if (_filterButton == null) _filterButton = filterBtnTr.gameObject.AddComponent<Button>();
                
                _filterButtonText = filterBtnTr.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (_filterButtonText == null)
                {
                    GameObject txtObj = new GameObject("Text");
                    txtObj.transform.SetParent(filterBtnTr, false);
                    _filterButtonText = txtObj.AddComponent<TMPro.TextMeshProUGUI>();
                    _filterButtonText.fontSize = 36;
                    _filterButtonText.alignment = TMPro.TextAlignmentOptions.Center;
                    _filterButtonText.color = Color.white;
                    RectTransform rt = txtObj.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                }

                _filterButton.onClick.AddListener(OnFilterClicked);
                UpdateFilterText();
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
            if (_sortButtonText != null)
            {
                string sortName = _currentSort switch
                {
                    SortType.LevelDesc => "Lv \u25BC", // Level Down
                    SortType.LevelAsc => "Lv \u25B2",  // Level Up
                    SortType.CPDesc => "CP \u25BC",    // CP Down
                    SortType.CPAsc => "CP \u25B2",     // CP Up
                    _ => "Sort"
                };
                _sortButtonText.text = $"Lọc:\n{sortName}";
            }
        }
        
        private void UpdateFilterText()
        {
            if (_filterButtonText != null)
            {
                string filterName = _currentFilter.HasValue ? _currentFilter.Value.ToString() : "Tất cả";
                _filterButtonText.text = $"Hệ:\n{filterName}";
            }
        }

        private void OnUpgradeBuildingClicked()
        {
            GameManager.Instance.UIManager.ShowPanel(UIPanelType.BuildingUpgrade, true);
            var upgradePanel = GameManager.Instance.UIManager.GetPanel<BuildingUpgradePanel>(UIPanelType.BuildingUpgrade);
            if (upgradePanel != null && !string.IsNullOrEmpty(associatedBuildingId))
            {
                upgradePanel.Setup(associatedBuildingId);
            }
            else
            {
                Debug.LogWarning($"[BarrackPanel] Cannot open BuildingUpgradePanel! upgradePanel null? {upgradePanel == null}, associatedBuildingId empty? {string.IsNullOrEmpty(associatedBuildingId)}");
            }
        }

        private void OnEnable()
        {
            RefreshHeroList();
            
            // Ép cập nhật lại số lượng dân số mỗi khi bật panel
            if (_populationCountText == null)
            {
                var allTexts = GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                foreach (var t in allTexts)
                {
                    if (t.gameObject.name == "PopulationText_Auto")
                    {
                        _populationCountText = t;
                        break;
                    }
                }
            }

            if (_populationCountText != null)
            {
                int maxCapacity = DataManager.Instance.GetPopulationCapacity();
                var allHeroes = DataManager.Instance.AllHeroes;
                _populationCountText.text = $"Heroes: {allHeroes.Count} / {maxCapacity}";
            }

            EventManager.StartListening(GameEvents.OnPlayerDataLoaded, RefreshHeroList);
            EventManager.StartListening(GameEvents.OnHeroListChanged, RefreshHeroList);
            DataManager.OnHeroListChanged += RefreshHeroList; // Cả Event C# gốc để an toàn
            RefreshHeroList();
        }

        private void OnDisable()
        {
            EventManager.StopListening(GameEvents.OnPlayerDataLoaded, RefreshHeroList);
            EventManager.StopListening(GameEvents.OnHeroListChanged, RefreshHeroList);
            DataManager.OnHeroListChanged -= RefreshHeroList;
        }

        public void Hide()
        {
            GameManager.Instance.UIManager.GoBack();
        }

        private void RefreshHeroList()
        {
            if (DataManager.Instance == null || DataManager.Instance.AllHeroes == null)
            {
                return;
            }

            foreach (GameObject card in _instantiatedHeroCards)
            {
                Destroy(card);
            }
            _instantiatedHeroCards.Clear();

            List<HeroData> allHeroes = DataManager.Instance.AllHeroes;
            List<HeroData> filteredHeroes = new List<HeroData>();
            
            // Cập nhật số lượng hero (Population count)
            if (_populationCountText != null)
            {
                int maxCapacity = DataManager.Instance.GetPopulationCapacity();
                _populationCountText.text = $"Heroes: {allHeroes.Count} / {maxCapacity}";
            }

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

            foreach (HeroData hero in filteredHeroes)
            {
                GameObject cardInstance = Instantiate(heroCardPrefab, heroListContainer);
                cardInstance.SetActive(true); // Đảm bảo thẻ Tướng hiển thị
                HeroCard heroCardScript = cardInstance.GetComponent<HeroCard>();
                if (heroCardScript != null)
                {
                    heroCardScript.Setup(hero);
                    _instantiatedHeroCards.Add(cardInstance);
                }
                else
                {
                    Debug.LogError("Hero Card Prefab không chứa script HeroCard!", this);
                }
            }
            Debug.Log($"[BarrackPanel] Đã làm mới danh sách, hiển thị {filteredHeroes.Count} heroes.");
        }
    }
}

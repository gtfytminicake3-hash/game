namespace LegendOfBlood
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class PopulationManagerPanel : UIPanel
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private Transform listContainer;
        [SerializeField] private GameObject heroCardPrefab;
        [SerializeField] private TextMeshProUGUI populationCountText;

        protected override void Start()
        {
            base.Start();
            PanelType = UIPanelType.PopulationManager;
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() => GameManager.Instance.UIManager.HidePanel(UIPanelType.PopulationManager));
            }
        }

        [SerializeField] private Button sortButton;
        [SerializeField] private Button filterButton;

        private enum SortType { LevelDesc, LevelAsc, CPDesc, CPAsc }
        private SortType _currentSort = SortType.LevelDesc;
        private LegendOfBlood.Profession? _currentFilter = null;

        private void OnEnable()
        {
            RefreshList();
            DataManager.OnHeroListChanged += RefreshList;
            
            if (sortButton != null)
            {
                sortButton.onClick.RemoveAllListeners();
                sortButton.onClick.AddListener(OnSortClicked);
            }
            if (filterButton != null)
            {
                filterButton.onClick.RemoveAllListeners();
                filterButton.onClick.AddListener(OnFilterClicked);
            }
        }

        private void OnDisable()
        {
            DataManager.OnHeroListChanged -= RefreshList;
        }

        private void OnSortClicked()
        {
            // Rotate sort type
            int nextSort = ((int)_currentSort + 1) % 4;
            _currentSort = (SortType)nextSort;
            
            string sortName = _currentSort switch
            {
                SortType.LevelDesc => "Lv \u25BC", // Level Down
                SortType.LevelAsc => "Lv \u25B2",  // Level Up
                SortType.CPDesc => "CP \u25BC",    // CP Down
                SortType.CPAsc => "CP \u25B2",     // CP Up
                _ => "Sort"
            };
            
            var txt = sortButton.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = $"Lọc:\n{sortName}";
            
            RefreshList();
        }

        private void OnFilterClicked()
        {
            // Rotate filter type: null -> Warrior -> Archer -> Mage -> Healer -> null
            if (_currentFilter == null) _currentFilter = LegendOfBlood.Profession.Warrior;
            else if (_currentFilter == LegendOfBlood.Profession.Warrior) _currentFilter = LegendOfBlood.Profession.Archer;
            else if (_currentFilter == LegendOfBlood.Profession.Archer) _currentFilter = LegendOfBlood.Profession.Mage;
            else if (_currentFilter == LegendOfBlood.Profession.Mage) _currentFilter = LegendOfBlood.Profession.Healer;
            else _currentFilter = null;

            string filterName = _currentFilter.HasValue ? _currentFilter.Value.ToString() : "Tất cả";
            
            var txt = filterButton.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = $"Hệ:\n{filterName}";
            
            RefreshList();
        }

        private void RefreshList()
        {
            if (listContainer == null || heroCardPrefab == null) return;
            // Thêm kiểm tra an toàn vì OnEnable() có thể chạy trước lúc Game Khởi tạo xong Singletons
            if (DataManager.Instance == null || DataManager.Instance.Player == null) return;

            // Clear current items
            foreach (Transform child in listContainer)
            {
                Destroy(child.gameObject);
            }

            var heroes = DataManager.Instance.AllHeroes;
            int capacity = DataManager.Instance.GetPopulationCapacity();

            if (populationCountText != null)
            {
                populationCountText.text = string.Format(LocalizationSystem.GetText("population_count_format"), heroes.Count, capacity);
            }

            // Apply filter
            System.Collections.Generic.IEnumerable<HeroData> filteredHeroes = heroes;
            if (_currentFilter.HasValue)
            {
                filteredHeroes = System.Linq.Enumerable.Where(filteredHeroes, h => h.profession == _currentFilter.Value);
            }

            // Apply sort
            System.Collections.Generic.List<HeroData> sortedHeroes = System.Linq.Enumerable.ToList(filteredHeroes);
            switch (_currentSort)
            {
                case SortType.LevelDesc:
                    sortedHeroes.Sort((a, b) => b.level.CompareTo(a.level));
                    break;
                case SortType.LevelAsc:
                    sortedHeroes.Sort((a, b) => a.level.CompareTo(b.level));
                    break;
                case SortType.CPDesc:
                    sortedHeroes.Sort((a, b) => b.GetCombatPower().CompareTo(a.GetCombatPower()));
                    break;
                case SortType.CPAsc:
                    sortedHeroes.Sort((a, b) => a.GetCombatPower().CompareTo(b.GetCombatPower()));
                    break;
            }

            foreach (var hero in sortedHeroes)
            {
                GameObject cardObj = Instantiate(heroCardPrefab, listContainer);
                PopulationHeroCard cardScript = cardObj.GetComponent<PopulationHeroCard>();
                if (cardScript != null)
                {
                    cardScript.Setup(hero, HandleDismissHero);
                }
            }
        }

        private void HandleDismissHero(HeroData heroToDismiss)
        {
            if (heroToDismiss == null) return;

            // Optional: You could show a confirmation popup here before actually removing
            // For now, removing directly
            DataManager.Instance.RemoveHero(heroToDismiss.id);
            GameManager.Instance.UINotificationManager.ShowNotification(
                string.Format(LocalizationSystem.GetText("hero_dismissed_success"), heroToDismiss.heroName)
            );
        }
    }
}

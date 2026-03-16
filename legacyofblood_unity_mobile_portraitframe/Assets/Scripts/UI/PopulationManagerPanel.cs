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

        private void OnEnable()
        {
            RefreshList();
            DataManager.OnHeroListChanged += RefreshList;
        }

        private void OnDisable()
        {
            DataManager.OnHeroListChanged -= RefreshList;
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

            foreach (var hero in heroes)
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

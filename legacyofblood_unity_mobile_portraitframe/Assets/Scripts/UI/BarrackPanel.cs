namespace LegendOfBlood
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class BarrackPanel : UIPanel
    {
        [Header("UI References")]
        [SerializeField] private TMPro.TextMeshProUGUI panelTitleText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button populationManagerButton;
        [SerializeField] private Transform heroListContainer;

        [Header("Upgrade Feature")]
        [SerializeField] private Button upgradeBuildingButton;
        [SerializeField] private string associatedBuildingId = "TownHall";

        [Header("Prefabs")]
        [SerializeField] private GameObject heroCardPrefab;

        private List<GameObject> _instantiatedHeroCards = new List<GameObject>();

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
                populationManagerButton.onClick.AddListener(() => GameManager.Instance.UIManager.ShowPanel(UIPanelType.PopulationManager, true));
                var popTxt = populationManagerButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (popTxt != null) popTxt.text = global::LocalizationSystem.GetText("btn_manage_population");
            }
            if (upgradeBuildingButton != null) 
            {
                upgradeBuildingButton.onClick.AddListener(OnUpgradeBuildingClicked);
                var upgTxt = upgradeBuildingButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (upgTxt != null) upgTxt.text = global::LocalizationSystem.GetText("btn_upgrade");
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
            EventManager.StartListening(GameEvents.OnPlayerDataLoaded, RefreshHeroList);
            EventManager.StartListening(GameEvents.OnHeroListChanged, RefreshHeroList);
            RefreshHeroList();
        }

        private void OnDisable()
        {
            EventManager.StopListening(GameEvents.OnPlayerDataLoaded, RefreshHeroList);
            EventManager.StopListening(GameEvents.OnHeroListChanged, RefreshHeroList);
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
            allHeroes.Sort((h1, h2) => h2.GetCombatPower().CompareTo(h1.GetCombatPower()));

            foreach (HeroData hero in allHeroes)
            {
                GameObject cardInstance = Instantiate(heroCardPrefab, heroListContainer);
                cardInstance.SetActive(true); // Đảm bảo thẻ Tướng hiển thị, chống tàng hình từ Prefab
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
            Debug.Log($"[BarrackPanel] Đã làm mới danh sách, hiển thị {allHeroes.Count} heroes.");
        }
    }
}

namespace LegendOfBlood
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class BarrackPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button populationManagerButton;
        [SerializeField] private Transform heroListContainer;

        [Header("Prefabs")]
        [SerializeField] private GameObject heroCardPrefab;

        private List<GameObject> _instantiatedHeroCards = new List<GameObject>();

        private void Start()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }
            if (populationManagerButton != null)
            {
                populationManagerButton.onClick.AddListener(() => GameManager.Instance.UIManager.ShowPanel(UIPanelType.PopulationManager, true));
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

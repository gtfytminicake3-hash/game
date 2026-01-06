namespace LegendOfBlood
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Điều khiển màn hình chính (Doanh trại), chịu trách nhiệm hiển thị danh sách hero.
    /// </summary>
    public class UIMainController : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Đối tượng cha chứa các HeroCard. Thường là một panel có Layout Group.")]
        [SerializeField] private Transform heroListContainer;

        [Tooltip("Prefab của thẻ bài hero.")]
        [SerializeField] private GameObject heroCardPrefab;

        [Header("Navigation Buttons")]
        //[SerializeField] private Button breedingButton;
        //[SerializeField] private Button hospitalButton;
        //[SerializeField] private Button worldMapButton;
        [SerializeField] private Button backToVillageButton;
        [Header("Popups")]
        [SerializeField] private ProfessionSelectionPanel professionSelectionPanel;

        private List<GameObject> _instantiatedHeroCards = new List<GameObject>();

        #region Unity Lifecycle & Event Subscription

        private void OnEnable()
        {
            EventManager.StartListening(GameEvents.OnPlayerDataLoaded, RefreshHeroList);
            EventManager.StartListening(GameEvents.OnHeroListChanged, RefreshHeroList);
            EventManager.StartListening<HeroData>(GameEvents.OnProfessionSelectionRequested, ShowProfessionSelection);
           // breedingButton.onClick.AddListener(OnBreedingClicked);
            //hospitalButton.onClick.AddListener(OnHospitalClicked);
           // worldMapButton.onClick.AddListener(OnWorldMapClicked);
            if (backToVillageButton != null) // Kiểm tra để tránh lỗi nếu quên kéo vào
            {
                backToVillageButton.onClick.AddListener(BackToVillageView);
            }
        }

        private void OnDisable()
        {
            EventManager.StopListening(GameEvents.OnPlayerDataLoaded, RefreshHeroList);
            EventManager.StopListening(GameEvents.OnHeroListChanged, RefreshHeroList);
            EventManager.StopListening<HeroData>(GameEvents.OnProfessionSelectionRequested, ShowProfessionSelection);
           // breedingButton.onClick.RemoveListener(OnBreedingClicked);
           // hospitalButton.onClick.RemoveListener(OnHospitalClicked);
            //worldMapButton.onClick.RemoveListener(OnWorldMapClicked);
            if (backToVillageButton != null)
            {
                backToVillageButton.onClick.RemoveListener(BackToVillageView);
            }
        }

        private void Start()
        {
            RefreshHeroList();
        }

        #endregion

        #region Core Logic

        private void RefreshHeroList()
        {
            if (DataManager.Instance == null || DataManager.Instance.AllHeroes == null)
            {
                Debug.LogWarning("UIMainController: Đang chờ DataManager sẵn sàng...");
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
            Debug.Log($"Đã làm mới danh sách, hiển thị {allHeroes.Count} heroes.");
        }

        #endregion

        #region UI Callbacks (Hàm được gọi từ các nút bấm)

        // --- CÁC HÀM NÀY LÀ PRIVATE VÌ CHỈ ĐƯỢC GỌI BẰNG CODE ---
        private void OnBreedingClicked()
        {
            GameManager.Instance.UIManager.ShowPanel(UIPanelType.Breeding);
        }

        private void OnHospitalClicked()
        {
            GameManager.Instance.UIManager.ShowPanel(UIPanelType.Hospital);
        }

        private void OnWorldMapClicked()
        {
            SceneManager.LoadScene("WorldMap_Scene");
        }

        // --- HÀM NÀY LÀ PUBLIC VÌ SẼ ĐƯỢC GỌI TỪ INSPECTOR ---
        private void BackToVillageView()
        {
            GameManager.Instance.UIManager.HidePanel(UIPanelType.MainScreen);
        }

        private void ShowProfessionSelection(HeroData hero)
        {
            if (professionSelectionPanel != null)
            {
                professionSelectionPanel.Show(hero, () => {
                    // Refresh UI logic if needed, e.g. update Hero Info panel if open
                    Debug.Log("Profession selection completed.");
                });
            }
            else
            {
                Debug.LogError("ProfessionSelectionPanel reference is missing in UIMainController!");
            }
        }

        #endregion
    }
}
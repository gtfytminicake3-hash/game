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
        // Các nút bấm điều hướng và popups đã được giữ nguyên

        [Header("Navigation Buttons")]
        //[SerializeField] private Button breedingButton;
        //[SerializeField] private Button hospitalButton;
        //[SerializeField] private Button worldMapButton;
        [SerializeField] private Button backToVillageButton;
        [SerializeField] private Button mailboxButton;
        [SerializeField] private GameObject mailboxRedDot;
        [Header("Popups")]
        [SerializeField] private ProfessionSelectionPanel professionSelectionPanel;



        #region Unity Lifecycle & Event Subscription

        private void OnEnable()
        {
            EventManager.StartListening<HeroData>(GameEvents.OnProfessionSelectionRequested, ShowProfessionSelection);
           // breedingButton.onClick.AddListener(OnBreedingClicked);
            //hospitalButton.onClick.AddListener(OnHospitalClicked);
           // worldMapButton.onClick.AddListener(OnWorldMapClicked);
            if (backToVillageButton != null) // Kiểm tra để tránh lỗi nếu quên kéo vào
            {
                backToVillageButton.onClick.AddListener(BackToVillageView);
            }
            if (mailboxButton != null)
            {
                mailboxButton.onClick.AddListener(OpenMailbox);
            }
        }

        private void OnDisable()
        {
            EventManager.StopListening<HeroData>(GameEvents.OnProfessionSelectionRequested, ShowProfessionSelection);
           // breedingButton.onClick.RemoveListener(OnBreedingClicked);
           // hospitalButton.onClick.RemoveListener(OnHospitalClicked);
            //worldMapButton.onClick.RemoveListener(OnWorldMapClicked);
            if (backToVillageButton != null)
            {
                backToVillageButton.onClick.RemoveListener(BackToVillageView);
            }
            if (mailboxButton != null)
            {
                mailboxButton.onClick.RemoveListener(OpenMailbox);
            }
        }

        private void Start()
        {
            UpdateMailboxNotification();
        }

        #endregion

        #region Core Logic

        // Render Hero list moved to BarrackPanel.cs

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
            GameManager.Instance.UIManager.ShowPanel(UIPanelType.WorldMap);
        }

        // --- HÀM NÀY LÀ PUBLIC VÌ SẼ ĐƯỢC GỌI TỪ INSPECTOR ---
        private void BackToVillageView()
        {
            GameManager.Instance.UIManager.BackToVillageView();
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

        private void OpenMailbox()
        {
            GameManager.Instance.UIManager.ShowPanel(UIPanelType.Mailbox);
            UpdateMailboxNotification();
        }

        public void UpdateMailboxNotification()
        {
            if (mailboxRedDot != null && DataManager.Instance != null && DataManager.Instance.Player != null)
            {
                var reports = DataManager.Instance.Player.UnclaimedReports;
                bool hasMail = reports != null && reports.Count > 0;
                mailboxRedDot.SetActive(hasMail);
            }
        }

        #endregion
    }
}
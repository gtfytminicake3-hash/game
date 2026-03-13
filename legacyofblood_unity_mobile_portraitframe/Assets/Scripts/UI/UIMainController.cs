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
        [SerializeField] private Button watchAdButton; // Nút xem quảng cáo cũ
        
        [Header("New Ad Placements")]
        public Button mysticChestAdButton; // Vị trí 4: Rương bí ẩn

        [Header("Popups")]
        public ProfessionSelectionPanel professionSelectionPanel;




        #region Unity Lifecycle & Event Subscription

        private void OnEnable()
        {
            EventManager.StartListening<HeroData>(GameEvents.OnProfessionSelectionRequested, ShowProfessionSelection);
           // breedingButton.onClick.AddListener(OnBreedingClicked);
            //hospitalButton.onClick.AddListener(OnHospitalClicked);
           // worldMapButton.onClick.AddListener(OnWorldMapClicked);
            if (watchAdButton != null) // NEW
            {
                watchAdButton.onClick.AddListener(OnWatchAdClicked);
            }
            if (mysticChestAdButton != null) mysticChestAdButton.onClick.AddListener(OnMysticChestAdClicked);
        }

        private void OnDisable()
        {
            EventManager.StopListening<HeroData>(GameEvents.OnProfessionSelectionRequested, ShowProfessionSelection);
           // breedingButton.onClick.RemoveListener(OnBreedingClicked);
           // hospitalButton.onClick.RemoveListener(OnHospitalClicked);
            //worldMapButton.onClick.RemoveListener(OnWorldMapClicked);
            if (watchAdButton != null) // NEW
            {
                watchAdButton.onClick.RemoveListener(OnWatchAdClicked);
            }

            if (mysticChestAdButton != null) mysticChestAdButton.onClick.RemoveListener(OnMysticChestAdClicked);
        }

        private float _mysticChestTimer = 0f;
        private const float MYSTIC_CHEST_INTERVAL = 3600f; // 1 tiếng xuất hiện 1 lần (đơn vị: giây)

        private void Start()
        {
            
            if (mysticChestAdButton != null) 
            {
                mysticChestAdButton.gameObject.SetActive(false);
                _mysticChestTimer = MYSTIC_CHEST_INTERVAL; // Sẵn sàng spawn rương sau 1 khoảng thời gian
            }
        }

        private void Update()
        {
            // Logic cho Mystic Chest
            if (mysticChestAdButton != null && !mysticChestAdButton.gameObject.activeSelf)
            {
                _mysticChestTimer -= Time.deltaTime;
                if (_mysticChestTimer <= 0)
                {
                    if (DataManager.Instance.Player.dailyMysticChestAdsWatched < 3) 
                    {
                        mysticChestAdButton.gameObject.SetActive(true);
                        // Có thể thêm hiệu ứng rung lắc nhẹ hoặc hạt (particle) ở đây
                    }
                    _mysticChestTimer = MYSTIC_CHEST_INTERVAL; // Reset timer bất kể có spawn hay không
                }
            }
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


        private void OnWatchAdClicked() // NEW
        {
            if (Managers.AdRewardGateway.Instance != null && DataManager.Instance != null)
            {
                // Giới hạn 3 lần mỗi ngày
                if (DataManager.Instance.Player.dailySummonAdsWatched < 3)
                {
                    Managers.AdRewardGateway.Instance.RequestAd(Managers.RewardType.DailySummon);
                }
                else
                {
                    GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("ad_limit_reached") ?? "Hôm nay bạn đã hết lượt xem quảng cáo nhận vé!");
                }
            }
            else
            {
                Debug.LogError("AdRewardGateway is missing!");
            }
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

        // --- NEW: Vị Trí 4 - Mystic Chest ---
        private void OnMysticChestAdClicked()
        {
            if (Managers.AdRewardGateway.Instance != null && DataManager.Instance != null)
            {
                Managers.AdRewardGateway.Instance.RequestAd(Managers.RewardType.MysticChest, () => {
                    // Tặng ngẫu nhiên Vàng hoặc Sách Kinh nghiệp
                    if (Random.value > 0.5f)
                    {
                        GameManager.Instance.InventoryManager.AddGold(5000);
                        GameManager.Instance.UINotificationManager.ShowNotification("Rương bí ẩn mở ra 5000 Vàng!");
                    }
                    else
                    {
                        GameManager.Instance.InventoryManager.AddItem("IT_EXP_BOOK_S", 5);
                        GameManager.Instance.UINotificationManager.ShowNotification("Rương bí ẩn mở ra 5 Quyển Sách EXP (Nhỏ)!");
                    }

                    // Ẩn nút đi và bắt đầu đếm thời gian lại
                    if (mysticChestAdButton != null) mysticChestAdButton.gameObject.SetActive(false);
                    _mysticChestTimer = MYSTIC_CHEST_INTERVAL;
                });
            }
        }


        #endregion
    }
}
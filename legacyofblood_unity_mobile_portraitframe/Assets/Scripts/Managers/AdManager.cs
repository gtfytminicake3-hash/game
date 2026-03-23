namespace LegendOfBlood.Managers
{
    using UnityEngine;
    using GoogleMobileAds.Api;
    using System;
    using LegendOfBlood;

    /// <summary>
    /// Singleton Component quản lý Google Mobile Ads (Đặc biệt là Rewarded Video).
    /// </summary>
    public class AdManager : MonoBehaviour
    {
        // ID Quảng Cáo THỬ NGHIỆM (Test ID) của Google. 
        // 🚨 QUAN TRỌNG: Thay bằng ID thật trên AdMob khi Build lên Store.
#if UNITY_ANDROID
        private const string REWARDED_AD_UNIT_ID = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
        private const string REWARDED_AD_UNIT_ID = "ca-app-pub-3940256099942544/1712485313";
#else
        private const string REWARDED_AD_UNIT_ID = "unused"; // Test id for Editor
#endif

        private RewardedAd _rewardedAd;
        private Action _onRewardEarnedCallback;

        // Trạng thái cờ để biết đã init SDK xong chưa
        public bool IsInitialized { get; private set; } = false;

        public void InitializeSystem()
        {
            if (IsInitialized) return;

            Debug.Log("[AdManager] Đang khởi tạo Google Mobile Ads SDK...");

            // Khởi tạo SDK
            MobileAds.Initialize((InitializationStatus initStatus) =>
            {
                IsInitialized = true;
                Debug.Log($"[AdManager] Khởi tạo SDK Thành Công. Status: {initStatus}");

                // Chỉ chạy trên Main Thread để gọi Unity APIs
                ExecuteOnMainThread(() => LoadRewardedAd());
            });
        }

        /// <summary>
        /// Yêu cầu Google tải sẵn một dòng quảng cáo
        /// </summary>
        public void LoadRewardedAd()
        {
            if (!IsInitialized) return;

            // Dọn dẹp Ad cũ nếu có
            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }

            Debug.Log("[AdManager] Đang tải Rewarded Ad...");

            // Gửi yêu cầu tải quảng cáo
            AdRequest adRequest = new AdRequest();
            RewardedAd.Load(REWARDED_AD_UNIT_ID, adRequest, (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError($"[AdManager] Tải Rewarded Ad Thất Bại: {error}");
                    return;
                }

                Debug.Log("[AdManager] Tải Rewarded Ad Thành Công!");
                _rewardedAd = ad;

                // Đăng ký các sự kiện (Callbacks) vòng đời của quảng cáo
                RegisterEventHandlers(_rewardedAd);
            });
        }

        /// <summary>
        /// Gọi hàm này từ UI khi người chơi bấm nút "Xem Quảng Cáo"
        /// </summary>
        /// <param name="onSuccess">Hàm sẽ được chạy nếu người dùng xem hết video nhận thưởng</param>
        public void ShowRewardedAd(Action onSuccess)
        {
            // Kiểm tra _rewardedAd đã tải xong chưa (CanShowAd)
            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                _onRewardEarnedCallback = onSuccess;
                Debug.Log("[AdManager] Đang hiển thị Rewarded Ad... Dừng thời gian game.");

                // --- PAUSE GAME: Ngừng thời gian và âm thanh ---
                Time.timeScale = 0f;
                AudioListener.pause = true;

                _rewardedAd.Show((Reward reward) =>
                {
                    Debug.Log($"[AdManager] Kéo Reward Callback về MainThread. Thưởng: {reward.Type} - {reward.Amount}");

                    // Cực kỳ quan trọng: Gọi callback trả thưởng trên MainThread vì nó đụng chạm tới InventoryUI
                    ExecuteOnMainThread(() =>
                    {
                        _onRewardEarnedCallback?.Invoke();
                        _onRewardEarnedCallback = null;
                    });
                });
            }
            else
            {
                Debug.LogWarning("[AdManager] Quảng cáo chưa tải xong hoặc không khả dụng. Tiến hành tải lại.");
                
                // Mẹo: CÓ THỂ Hiển thị popup "Quảng cáo chưa sẵn sàng, vui lòng thử lại sau" ở đây
                if (GameManager.Instance != null && GameManager.Instance.UINotificationManager != null)
                {
                    GameManager.Instance.UINotificationManager.ShowNotification("Quảng cáo chưa sẵn sàng. Đang tải, vui lòng thử lại sau ít giây.");
                }

                // Nếu bạn spam quá nhiều yêu cầu load, có thể Google sẽ giới hạn, do đó nên cẩn thận.
                // Ở đây mình cố tình gọi lại cho chắc.
                LoadRewardedAd();
            }
        }

        private void RegisterEventHandlers(RewardedAd ad)
        {
            // Được phát ra khi Quảng cáo đóng lại (Dù tắt giữa chừng hay xem hết)
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("[AdManager] Quảng cáo đã đóng. Bật lại game và tải quảng cáo mới.");
                
                // --- RESUME GAME: Bật lại thời gian và âm thanh (Bắt buộc chạy trên MainThread) ---
                ExecuteOnMainThread(() => 
                {
                    Time.timeScale = 1f;
                    AudioListener.pause = false;
                    LoadRewardedAd();
                });
            };

            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError($"[AdManager] Lỗi khi hiển thị Quảng cáo: {error}");
                // Nếu lỗi khi hiện, cũng phải load lại và bật game
                ExecuteOnMainThread(() => 
                {
                    Time.timeScale = 1f;
                    AudioListener.pause = false;
                    LoadRewardedAd();
                });
            };
        }

        // ==========================================
        // UTILITY: MAIN THREAD DISPATCHER (RẤT QUAN TRỌNG VỚI GOOGLE ADS)
        // SDK Google trả về kết quả ở một thread ngầm, mà Unity không cho phép sửa thông số Transform, Text...
        // từ thread ngầm đó. Ta đẩy Action về hàm Update() của class này.
        // ==========================================
        
        private readonly System.Collections.Concurrent.ConcurrentQueue<Action> _executionQueue = new System.Collections.Concurrent.ConcurrentQueue<Action>();

        public void ExecuteOnMainThread(Action action)
        {
            _executionQueue.Enqueue(action);
        }

        private void Update()
        {
            // Tháo các Action ra khỏi hàng đợi và thực thi trên Update cycle (Main Thread)
            while (_executionQueue.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }
    }
}

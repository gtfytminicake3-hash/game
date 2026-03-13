namespace LegendOfBlood.Managers
{
    using UnityEngine;
    using System;

    /// <summary>
    /// Các loại phần thưởng khi người dùng xem hết quảng cáo.
    /// </summary>
    public enum RewardType
    {
        DailySummon,    // Tặng vé quay tướng miễn phí / Quay hàng ngày
        DoubleGold,     // X2 Vàng sau khi thắng trận
        ReviveTeam,     // Hồi sinh đội hình
        MysticChest,    // Mở rương bí ẩn
        BuildingSpeedUp, // Tăng tốc xây nhà
        FreeHeal,       // Chữa bệnh miễn phí
        ArenaTicket,    // Thêm vé đấu trường
        BreedingMutation, // Lai tạo đột biến
        TowerCooldownSkip, // Xóa thời gian chờ tháp
        ShopFreebie     // Quà tặng cửa hàng
    }

    /// <summary>
    /// Gateway trung gian điểm tiếp nhận yêu cầu xem quảng cáo từ UI.
    /// Nó sẽ gọi qua AdManager, và khi xem xong sẽ xử lý trả thưởng tương ứng.
    /// </summary>
    public class AdRewardGateway : MonoBehaviour
    {
        public static AdRewardGateway Instance { get; private set; }

        private RewardType _currentRewardType;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private Action _currentOnRewardCallback;

        /// <summary>
        /// Gọi hàm này từ UI thay vì gọi trực tiếp AdManager.
        /// </summary>
        public void RequestAd(RewardType type, Action onReward = null)
        {
            _currentRewardType = type;
            _currentOnRewardCallback = onReward;

            if (GameManager.Instance.AdManager != null)
            {
                // Gọi sang AdManager để bật quảng cáo
                GameManager.Instance.AdManager.ShowRewardedAd(OnAdFinished);
            }
            else
            {
                Debug.LogError("[AdRewardGateway] Không tìm thấy AdManager!");
            }
        }

        /// <summary>
        /// Xử lý logic cộng thưởng tùy theo RewardType
        /// </summary>
        private void OnAdFinished()
        {
            var player = GameManager.Instance.DataManager.Player;

            switch (_currentRewardType)
            {
                case RewardType.DailySummon:
                    Debug.Log("[AdRewardGateway] Nhận thưởng: Daily Summon");
                    // Tăng biến đếm
                    player.dailySummonAdsWatched++;
                    
                    // Thưởng 1 vé Gacha
                    GameManager.Instance.InventoryManager.AddItem("IT_GACHA_TICKET", 1);
                    GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("ad_reward_gacha_ticket") ?? "Nhận được 1 Vé Chiêu Mộ!");
                    break;

                case RewardType.DoubleGold:
                    Debug.Log("[AdRewardGateway] Nhận thưởng: Double Gold");
                    player.dailyDoubleGoldAdsWatched++;
                    
                    // Dùng thủ thuật lấy Last Match Gold hoặc gọi qua hệ thống phù hợp.
                    // (Bạn sẽ tùy chỉnh phần logic Vàng được nhân đôi ở đây)
                    GameManager.Instance.UINotificationManager.ShowNotification("Nhận thưởng X2 Vàng thành công!");
                    break;

                case RewardType.ReviveTeam:
                     Debug.Log("[AdRewardGateway] Nhận thưởng: Revive Team");
                    // Logic hồi sinh đội hình
                    break;

                case RewardType.MysticChest:
                     Debug.Log("[AdRewardGateway] Nhận thưởng: Mystic Chest");
                    // Logic mở rương
                    break;
                    
                case RewardType.BuildingSpeedUp:
                    Debug.Log("[AdRewardGateway] Nhận thưởng: Building Speed Up");
                    player.dailyBuildingSpeedUpsWatched++;
                    GameManager.Instance.UINotificationManager.ShowNotification("Rút ngắn 20 phút xây dựng!");
                    break;
                    
                case RewardType.FreeHeal:
                    Debug.Log("[AdRewardGateway] Nhận thưởng: Free Heal");
                    player.dailyFreeHealsWatched++; // Giới hạn số lần chữa bệnh miễn phí
                    // Callback sẽ lo phần còn lại
                    break;
                    
                case RewardType.ArenaTicket:
                    Debug.Log("[AdRewardGateway] Nhận thưởng: Arena Ticket");
                    player.dailyArenaTicketAdsWatched++;
                    player.arenaTickets++;
                    GameManager.Instance.UINotificationManager.ShowNotification("Nhận được 1 Vé Đấu Trường!");
                    break;
                    
                case RewardType.BreedingMutation:
                    Debug.Log("[AdRewardGateway] Nhận thưởng: Breeding Mutation");
                    player.dailyMutationAdsWatched++;
                    GameManager.Instance.UINotificationManager.ShowNotification("Tỉ lệ đột biến đã được kích hoạt!");
                    break;

                case RewardType.TowerCooldownSkip:
                    Debug.Log("[AdRewardGateway] Nhận thưởng: Tower Cooldown Skip");
                    player.dailyTowerSkipAdsWatched++;
                    GameManager.Instance.UINotificationManager.ShowNotification("Đã tiến vào Tháp, sẵn sàng chiến đấu!");
                    break;
                    
                case RewardType.ShopFreebie:
                    Debug.Log("[AdRewardGateway] Nhận thưởng: Shop Freebie");
                    player.dailyShopFreebieAdsWatched++;
                    player.arenaCoins += 100; // Thay vì Vàng, Arena Coins hữu dụng cho ArenaShop hơn
                    GameManager.Instance.UINotificationManager.ShowNotification("Đã nhận 100 Xu Đấu Trường từ Quảng Cáo!");
                    break;
            }

            // Gọi logic phụ tùy vào context truyền vào (ví dụ X2 thư)
            _currentOnRewardCallback?.Invoke();
            _currentOnRewardCallback = null;

            // Lưu game sau khi nhận thưởng
            GameManager.Instance.DataManager.SavePlayerData();
        }
    }
}

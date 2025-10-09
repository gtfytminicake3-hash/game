namespace LegendOfBlood
{
    using LegendOfBlood.Combat; // Cần using để thấy CombatResult
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    #region LỚP DỮ LIỆU EXPEDITION (GỘP TỪ EXPEDITION.CS)

    public enum ExpeditionStatus
    {
        Traveling,
        Exploring,
        Returning,
        Finished
    }

    [Serializable]
    public class Expedition
    {
        public string id;
        public List<string> squadHeroIDs;
        public POIData destination;
        public ExpeditionStatus status;
        public long startTime;
        public long endTime;
        
        // Kết quả chiến đấu sẽ được lưu ở đây sau khi đến nơi
        public CombatResult combatResult; 
    }

    #endregion

    /// <summary>
    /// Quản lý trạng thái, thời gian và kết quả của tất cả các chuyến thám hiểm đang hoạt động.
    /// Giao tiếp với các hệ thống khác (Combat, Hospital, Inventory) để xử lý hậu quả.
    /// </summary>
    public class ExpeditionManager : MonoBehaviour
    {
        private List<Expedition> _activeExpeditions = new List<Expedition>();
        private List<Expedition> _expeditionsToRemove = new List<Expedition>();

        // --- EVENTS ---
        public static event Action<Expedition> OnExpeditionStarted;
        public static event Action<Expedition> OnExpeditionReturning;
        public static event Action<Expedition> OnExpeditionFinished;
        public static event Action<POIData> OnPOICleared; // Event mới để báo POI đã hoàn thành

        private void Start()
        {
            // Tải các expedition đang hoạt động từ DataManager khi game bắt đầu
            // (Giả sử DataManager có hàm để lưu/tải _activeExpeditions)
        }

        /// <summary>
        /// Bắt đầu một chuyến thám hiểm mới.
        /// Được gọi bởi UI (ví dụ: WorldMapController).
        /// </summary>
        /// <param name="squadHeroIDs">Danh sách ID của các hero tham gia</param>
        /// <param name="destination">Dữ liệu của POI đích</param>
        public void StartExpedition(List<string> squadHeroIDs, POIData destination)
        {
            // TODO: Kiểm tra xem các hero có rảnh không
            
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long travelTime = CalculateTravelTime(destination.position); // Giả sử có 1 vị trí làng cố định

            var newExpedition = new Expedition
            {
                id = Guid.NewGuid().ToString(),
                squadHeroIDs = squadHeroIDs,
                destination = destination,
                status = ExpeditionStatus.Traveling,
                startTime = currentTime,
                // Thời gian đến nơi
                endTime = currentTime + travelTime 
            };
            
            _activeExpeditions.Add(newExpedition);
            OnExpeditionStarted?.Invoke(newExpedition);
            Debug.Log($"Expedition to {destination.poiName} started. Arrival in {travelTime / 1000f}s.");
        }

        /// <summary>
        /// Hàm tick được gọi bởi GameManager mỗi frame.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (_activeExpeditions.Count == 0) return;
            
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Duyệt qua danh sách để kiểm tra và cập nhật trạng thái
            foreach (var expedition in _activeExpeditions)
            {
                if (currentTime >= expedition.endTime)
                {
                    AdvanceExpeditionState(expedition);
                }
            }

            // Dọn dẹp các expedition đã hoàn thành
            if (_expeditionsToRemove.Count > 0)
            {
                _activeExpeditions.RemoveAll(exp => _expeditionsToRemove.Contains(exp));
                _expeditionsToRemove.Clear();
            }
        }

        /// <summary>
        /// Chuyển expedition sang trạng thái tiếp theo.
        /// </summary>
        private void AdvanceExpeditionState(Expedition expedition)
        {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            switch (expedition.status)
            {
                case ExpeditionStatus.Traveling:
                    // Đã đến nơi, bắt đầu chiến đấu/khám phá
                    Debug.Log($"Expedition {expedition.id} arrived at destination.");
                    expedition.status = ExpeditionStatus.Exploring;
                    
                    // --- THAY ĐỔI: GỌI COMBATSYSTEM ĐỂ MÔ PHỎNG TRẬN ĐẤU ---
                    // 1. Lấy dữ liệu các hero trong đội
                    var heroSquad = expedition.squadHeroIDs
                                        .Select(id => (HeroData)null /*DataManager.Instance.GetHeroByID(id)*/)
                                        .Where(h => h != null && h.currentHp > 0) // Chỉ những hero còn sống mới tham gia
                                        .ToList();

                    // 2. Gọi hàm mô phỏng
                    var combatResult = GameManager.Instance.CombatSystem.Simulate(heroSquad, expedition.destination.monsterIDs);
                    
                    // 3. Lưu kết quả vào expedition để xử lý ở bước sau
                    expedition.combatResult = combatResult;
                    
                    // 4. Trận đấu diễn ra ngay lập tức, không cần chờ. Chuyển ngay sang trạng thái trở về.
                    // Chúng ta vẫn có thể thêm một khoảng chờ nhỏ nếu muốn.
                    long explorationTime = 2000; // 2 giây giả lập "dọn dẹp chiến trường"
                    expedition.endTime = currentTime + explorationTime; 
                    break;
                    
                case ExpeditionStatus.Exploring:
                    // Khám phá xong, bắt đầu trở về
                     Debug.Log($"Expedition {expedition.id} finished exploring, returning home.");
                    expedition.status = ExpeditionStatus.Returning;
                    
                    long travelTime = CalculateTravelTime(expedition.destination.position);
                    expedition.endTime = currentTime + travelTime;

                    OnExpeditionReturning?.Invoke(expedition);
                    break;

                case ExpeditionStatus.Returning:
                    // Đã về đến nhà, kết thúc
                    Debug.Log($"Expedition {expedition.id} has returned home.");
                    expedition.status = ExpeditionStatus.Finished;
                    
                    FinalizeExpedition(expedition);
                    _expeditionsToRemove.Add(expedition); // Đánh dấu để xóa
                    break;
            }
        }

        /// <summary>
        /// Xử lý kết quả cuối cùng của chuyến đi (trao thưởng, xử lý thương vong).
        /// </summary>
        private void FinalizeExpedition(Expedition expedition)
        {
            // --- THAY ĐỔI: XỬ LÝ KẾT QUẢ DỰA TRÊN COMBATRESULT ---
            if (expedition.combatResult == null)
            {
                Debug.LogError($"Expedition {expedition.id} hoàn thành nhưng không có kết quả chiến đấu!");
                return;
            }

            var result = expedition.combatResult;
            if (result.IsVictory)
            {
                // Trao thưởng khi thắng
                int goldReward = expedition.destination.difficultyLevel * 50;
                GameManager.Instance.InventoryManager.AddResource(ResourceType.Gold, goldReward);
                GameManager.Instance.UINotificationManager.ShowNotification($"Thắng trận! Nhận được {goldReward} vàng.");

                // Xử lý hero bị thương nhẹ (nếu HP không đầy)
            }
            else
            {
                // Xử lý hero bị thương nặng khi thua
                // GameManager.Instance.HospitalSystem.AdmitHeroesForSevereInjury(result.CasualtyHeroIDs);
            }
            
            OnExpeditionFinished?.Invoke(expedition);

            // Bắn sự kiện báo rằng POI này đã được hoàn thành
            OnPOICleared?.Invoke(expedition.destination);
        }

        /// <summary>
        /// Kiểm tra xem một hero có đang trong một chuyến đi nào không.
        /// </summary>
        public bool IsHeroOnExpedition(string heroId)
        {
            return _activeExpeditions.Any(exp => exp.squadHeroIDs.Contains(heroId));
        }

        /// <summary>
        /// Placeholder: Tính toán thời gian di chuyển.
        /// </summary>
        private long CalculateTravelTime(Vector2 destination)
        {
            // Giả sử làng ở tọa độ (0,0)
            float distance = Vector2.Distance(Vector2.zero, destination);
            // Mỗi đơn vị khoảng cách tốn 100ms
            return (long)(distance * 100);
        }
    }
}
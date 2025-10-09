namespace LegendOfBlood
{
    using System;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Quản lý trạng thái, thời gian chờ và logic nâng cấp của các công trình.
    /// </summary>
    public class BuildingSystem
    {
        // Events để thông báo cho các hệ thống khác (chủ yếu là UI)
        public static event Action<Building> OnBuildingUpgradeStarted;
        public static event Action<Building> OnBuildingUpgradeCompleted;

        /// <summary>
        /// Bắt đầu quá trình nâng cấp cho một công trình.
        /// Được gọi từ UI khi người chơi nhấn nút "Nâng cấp".
        /// </summary>
        /// <param name="buildingId">ID của công trình cần nâng cấp</param>
        /// <returns>True nếu việc nâng cấp bắt đầu thành công, False nếu thất bại.</returns>
        public bool StartUpgrade(string buildingId)
        {
            var building = DataManager.Instance.AllBuildings.FirstOrDefault(b => b.id == buildingId);

            if (building == null)
            {
                Debug.LogError($"Không tìm thấy công trình với ID: {buildingId}");
                return false;
            }

            if (building.isUnderConstruction)
            {
                GameManager.Instance.UINotificationManager.ShowNotification("Công trình đang được xây dựng!");
                return false;
            }

            int nextLevel = building.level + 1;
            
            // Lấy chi phí và thời gian từ DataManager (giả định có trong GameConfig)
            // BuildingUpgradeInfo info = DataManager.Instance.GetBuildingUpgradeInfo(building.type, nextLevel);
            
            // --- PHẦN GIẢ LẬP DỮ LIỆU NÂNG CẤP ---
            var cost = new PlayerResources { gold = nextLevel * 100, wood = nextLevel * 50 };
            long durationMs = nextLevel * 30 * 1000; // 30 giây mỗi cấp
            // --- KẾT THÚC PHẦN GIẢ LẬP ---

            // Kiểm tra và chi tiêu tài nguyên
            if (!CanAfford(cost))
            {
                GameManager.Instance.UINotificationManager.ShowNotification("Không đủ tài nguyên!");
                return false;
            }
            
            // Chi tiêu tài nguyên
            InventoryManager.Instance.SpendResource(ResourceType.Gold, cost.gold);
            InventoryManager.Instance.SpendResource(ResourceType.Wood, cost.wood);
            
            // Cập nhật trạng thái công trình
            building.isUnderConstruction = true;
            building.constructionEndTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + durationMs;

            Debug.Log($"Bắt đầu nâng cấp {building.type} lên cấp {nextLevel}. Sẽ hoàn thành sau {durationMs / 1000} giây.");
            
            OnBuildingUpgradeStarted?.Invoke(building);
            return true;
        }

        /// <summary>
        /// Hàm tick được gọi bởi GameManager để kiểm tra các công trình đã hoàn thành.
        /// </summary>
        public void Tick(float deltaTime)
        {
            var buildings = DataManager.Instance.AllBuildings;
            if (buildings == null || buildings.Count == 0) return;

            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            foreach (var building in buildings)
            {
                if (building.isUnderConstruction && currentTime >= building.constructionEndTime)
                {
                    CompleteConstruction(building);
                }
            }
        }

        /// <summary>
        /// Hoàn thành việc xây dựng cho một công trình.
        /// </summary>
        private void CompleteConstruction(Building building)
        {
            building.isUnderConstruction = false;
            building.level++;
            
            Debug.Log($"<color=green>Hoàn thành!</color> Công trình {building.type} đã được nâng cấp lên cấp {building.level}.");
            OnBuildingUpgradeCompleted?.Invoke(building);
        }

        /// <summary>
        /// Hàm tiện ích để kiểm tra xem người chơi có đủ tài nguyên hay không.
        /// </summary>
        private bool CanAfford(PlayerResources cost)
        {
            bool hasEnough = true;
            if (InventoryManager.Instance.GetResourceAmount(ResourceType.Gold) < cost.gold) hasEnough = false;
            if (InventoryManager.Instance.GetResourceAmount(ResourceType.Wood) < cost.wood) hasEnough = false;
            if (InventoryManager.Instance.GetResourceAmount(ResourceType.Stone) < cost.stone) hasEnough = false;
            return hasEnough;
        }
    }
}
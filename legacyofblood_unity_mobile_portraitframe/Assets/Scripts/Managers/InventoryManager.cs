namespace LegendOfBlood
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    // Enum để định danh các loại tài nguyên, tránh dùng string "gold", "wood"
    public enum ResourceType
    {
        Gold,
        Wood,
        Stone,
        Diamond
    }

    /// <summary>
    /// InventoryManager là một Singleton quản lý tất cả các tương tác với
    /// tài nguyên (tiền tệ) và vật phẩm của người chơi.
    /// Nó hoạt động trên dữ liệu được cung cấp bởi DataManager.
    /// </summary>
    [DisallowMultipleComponent]
    public class InventoryManager : MonoBehaviour
    {
        // Không cần Singleton Pattern ở đây vì GameManager đã quản lý nó
        // Tuy nhiên, việc có một tham chiếu tĩnh vẫn tiện lợi
        public static InventoryManager Instance { get; private set; }

        // Tham chiếu đến dữ liệu người chơi thực tế, được lấy từ DataManager
        private PlayerData _playerData;

        // --- EVENTS ---
        /// <summary>
        /// Được phát ra khi số lượng của một loại tài nguyên thay đổi.
        /// Tham số 1: Loại tài nguyên.
        /// Tham số 2: Số lượng mới.
        /// </summary>
        public static event Action<ResourceType, int> OnResourceChanged;

        /// <summary>
        /// Được phát ra khi số lượng của một vật phẩm thay đổi.
        /// Tham số 1: ID của vật phẩm.
        /// Tham số 2: Số lượng mới.
        /// </summary>
        public static event Action<string, int> OnItemChanged;
        public static event Action OnEquipmentChanged;
        
        // Sự kiện khi chỉ số King God Pass thay đổi (Level, Exp hiện tại)
        public static event Action<int, int> OnPassExpChanged;

        // Sự kiện khi Player Level/Exp thay đổi
        public static event Action<int, int> OnPlayerExpChanged;

        private void Awake()
        {
            // Thiết lập tham chiếu tĩnh để dễ truy cập
            if (Instance != null && Instance != this)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) { DestroyImmediate(this.gameObject); return; }
#endif
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Đăng ký lắng nghe sự kiện từ DataManager để biết khi nào dữ liệu đã sẵn sàng (Cho những lần load sau)
            DataManager.OnPlayerDataLoaded += LinkToPlayerData;

            // XỬ LÝ LỖI CUỘC ĐUA (RACE CONDITION): 
            // Do GameManager ép DataManager load từ trong Awake() nên Event OnPlayerDataLoaded đã bắn xong trước khi Inventory bật lên (Start).
            // Ta phải gọi nạp dữ liệu bù bằng tay:
            if (DataManager.Instance != null && DataManager.Instance.Player != null)
            {
                LinkToPlayerData();
            }
        }

        private void OnDestroy()
        {
            // Hủy đăng ký để tránh lỗi khi scene bị hủy
            DataManager.OnPlayerDataLoaded -= LinkToPlayerData;
        }

        /// <summary>
        /// Lấy tham chiếu đến dữ liệu người chơi từ DataManager.
        /// </summary>
        private void LinkToPlayerData()
        {
            _playerData = DataManager.Instance.Player;
            if (_playerData == null)
            {
                Debug.LogError(LocalizationSystem.GetText("inventory_error_no_playerdata"));
                return;
            }
            
            // KIỂM TRA TÂN THỦ: Cấp Vốn Khởi Nghiệp (1000 Vàng, 1000 Gỗ)
            if (GetResourceAmount(ResourceType.Gold) == 0 && GetResourceAmount(ResourceType.Wood) == 0)
            {
                Debug.Log("[InventoryManager] Phát hiện Tài khoản Mới! Đang Bơm 1000 Vàng và 1000 Gỗ khởi nghiệp...");
                // Gán trực tiếp qua Data để không kích hoạt quá nhiều event trước khi game load xong
                AddResource(ResourceType.Gold, 1000);
                AddResource(ResourceType.Wood, 1000);
            }

            // Đồng bộ UI ngay lúc đầu cho King God Pass
            OnPassExpChanged?.Invoke(_playerData.passLevel, _playerData.passExp);
        }


        #region Player Level Management
        
        public int GetMaxExpForPlayerLevel(int level)
        {
            return level * 1000;
        }

        public void AddPlayerExp(int amount)
        {
            if (_playerData == null) return;
            if (_playerData.playerLevel >= 50) return; // Đã đạt cấp tối đa
            
            _playerData.playerExp += amount;
            int maxExp = GetMaxExpForPlayerLevel(_playerData.playerLevel);
            bool leveledUp = false;

            while (_playerData.playerExp >= maxExp && _playerData.playerLevel < 50)
            {
                _playerData.playerExp -= maxExp;
                _playerData.playerLevel++;
                leveledUp = true;
                
                if (_playerData.playerLevel >= 50)
                {
                    _playerData.playerExp = 0; // Tràn EXP sẽ bị xóa khi đạt max cấp
                    break;
                }
                
                maxExp = GetMaxExpForPlayerLevel(_playerData.playerLevel);
            }

            if (leveledUp)
            {
                Debug.Log($"[InventoryManager] 💥 Player leveled up to {_playerData.playerLevel}!");
            }
            
            OnPlayerExpChanged?.Invoke(_playerData.playerLevel, _playerData.playerExp);
            GameManager.Instance.DataManager.SavePlayerData();
        }

        #endregion

        #region King God Pass Management

        public int GetMaxExpForPassLevel(int level)
        {
            var config = GameManager.Instance.DataManager.GameConfig?.KingGodPassConfig;
            if (config != null)
            {
                return config.GetRequiredExpForLevel(level);
            }
            // Fallback
            return 100 + level * 50; 
        }

        public void AddPassExp(int amount)
        {
            if (_playerData == null || amount <= 0) return;
            
            _playerData.passExp += amount;
            
            // Cập nhật lại max exp cho cấp tiếp theo
            int maxExp = GetMaxExpForPassLevel(_playerData.passLevel);
            while (_playerData.passExp >= maxExp)
            {
                _playerData.passExp -= maxExp;
                _playerData.passLevel++;
                maxExp = GetMaxExpForPassLevel(_playerData.passLevel);
                Debug.Log($"King God Pass đã lên cấp: {_playerData.passLevel}");
            }
            
            OnPassExpChanged?.Invoke(_playerData.passLevel, _playerData.passExp);
            GameManager.Instance.DataManager.SavePlayerData();
        }

        public void ClaimPassReward(int level, bool isPremium)
        {
            if (_playerData == null) return;
            var config = GameManager.Instance.DataManager.GameConfig?.KingGodPassConfig;
            if (config == null) return;

            var levelData = config.GetLevelData(level);
            if (levelData == null) return;

            // Kiểm tra điều kiện
            if (_playerData.passLevel < level) return; // Chưa đạt cấp
            
            if (isPremium)
            {
                if (!_playerData.isPremiumPassUnlocked) return; // Chưa mua Pass
                if (_playerData.claimedPremiumPassLevels.Contains(level)) return; // Đã nhận
                
                // Trao quà Premium
                GivePassRewardItem(levelData.premiumReward);
                _playerData.claimedPremiumPassLevels.Add(level);
                Debug.Log($"Đã nhận quà Premium King God Pass mốc {level}");
            }
            else
            {
                if (_playerData.claimedFreePassLevels.Contains(level)) return; // Đã nhận
                
                // Trao quà Free
                GivePassRewardItem(levelData.freeReward);
                _playerData.claimedFreePassLevels.Add(level);
                Debug.Log($"Đã nhận quà Free King God Pass mốc {level}");
            }

            // Lưu dữ liệu và báo UI cập nhật
            GameManager.Instance.DataManager.SavePlayerData();
            OnPassExpChanged?.Invoke(_playerData.passLevel, _playerData.passExp);
        }

        private void GivePassRewardItem(LegendOfBlood.GameConfigs.PassRewardItem reward)
        {
            if (reward == null || reward.amount <= 0) return;

            if (string.IsNullOrEmpty(reward.itemID))
            {
                // Là Resource
                AddResource(reward.resourceType, reward.amount);
            }
            else
            {
                // Là Item
                AddItem(reward.itemID, reward.amount);
            }
        }

        #endregion

        #region Resource Management

        /// <summary>
        /// A convenience method to add gold.
        /// </summary>
        public void AddGold(int amount)
        {
            AddResource(ResourceType.Gold, amount);
        }

        public void AddDiamond(int amount)
        {
            AddResource(ResourceType.Diamond, amount);
        }

        /// <summary>
        /// Lấy số lượng hiện tại của một loại tài nguyên.
        /// </summary>
        public int GetResourceAmount(ResourceType type)
        {
            if (_playerData == null) return 0;

            switch (type)
            {
                case ResourceType.Gold: return _playerData.resources.gold;
                case ResourceType.Wood: return _playerData.resources.wood;
                case ResourceType.Stone: return _playerData.resources.stone;
                case ResourceType.Diamond: return _playerData.resources.diamond;
                default: return 0;
            }
        }

        /// <summary>
        /// Cộng thêm một lượng tài nguyên cho người chơi.
        /// </summary>
        public void AddResource(ResourceType type, int amount)
        {
            if (_playerData == null || amount <= 0) return;

            int newAmount = 0;
            switch (type)
            {
                case ResourceType.Gold:
                    _playerData.resources.gold += amount;
                    newAmount = _playerData.resources.gold;
                    break;
                case ResourceType.Wood:
                    _playerData.resources.wood += amount;
                    newAmount = _playerData.resources.wood;
                    break;
                case ResourceType.Stone:
                    _playerData.resources.stone += amount;
                    newAmount = _playerData.resources.stone;
                    break;
                case ResourceType.Diamond:
                    _playerData.resources.diamond += amount;
                    newAmount = _playerData.resources.diamond;
                    break;
            }
            // Phát sự kiện để UI cập nhật
            OnResourceChanged?.Invoke(type, newAmount);
            Debug.Log($"Added {amount} {type}. New total: {newAmount}");
        }

        /// <summary>
        /// Kiểm tra xem người chơi có đủ tài nguyên hay không.
        /// </summary>
        public bool HasEnoughResources(ResourceType type, int amount)
        {
            return GetResourceAmount(type) >= amount;
        }

        /// <summary>
        /// Trừ một lượng tài nguyên của người chơi.
        /// </summary>
        /// <returns>True nếu chi tiêu thành công, False nếu không đủ.</returns>
        public bool SpendResource(ResourceType type, int amount)
        {
            if (_playerData == null || amount <= 0) return false;

            if (!HasEnoughResources(type, amount))
            {
                Debug.LogWarning(string.Format(LocalizationSystem.GetText("inventory_error_not_enough_resource"), type, amount, GetResourceAmount(type)));
                // Có thể gọi UINotificationManager ở đây
                GameManager.Instance.UINotificationManager.ShowNotification(string.Format(LocalizationSystem.GetText("inventory_not_enough_resource_notification"), type));
                return false;
            }

            int newAmount = 0;
            switch (type)
            {
                case ResourceType.Gold:
                    _playerData.resources.gold -= amount;
                    newAmount = _playerData.resources.gold;
                    break;
                case ResourceType.Wood:
                    _playerData.resources.wood -= amount;
                    newAmount = _playerData.resources.wood;
                    break;
                case ResourceType.Stone:
                    _playerData.resources.stone -= amount;
                    newAmount = _playerData.resources.stone;
                    break;
                case ResourceType.Diamond:
                    _playerData.resources.diamond -= amount;
                    newAmount = _playerData.resources.diamond;
                    break;
            }
            
            // Phát sự kiện để UI cập nhật
            OnResourceChanged?.Invoke(type, newAmount);
            Debug.Log($"Spent {amount} {type}. New total: {newAmount}");
            return true;
        }

        #endregion


        #region Item Management

        /// <summary>
        /// Lấy số lượng của một vật phẩm theo ID.
        /// </summary>
        /// <returns>Số lượng vật phẩm, hoặc 0 nếu không có.</returns>
        public int GetItemCount(string itemID)
        {
            if (_playerData == null) return 0;
            
            _playerData.items.TryGetValue(itemID, out int count);
            return count;
        }

        /// <summary>
        /// Thêm một hoặc nhiều vật phẩm vào túi đồ.
        /// </summary>
        public void AddItem(string itemID, int amount)
        {
            if (_playerData == null || string.IsNullOrEmpty(itemID) || amount <= 0) return;
            
            int currentCount = GetItemCount(itemID);
            int newCount = currentCount + amount;
            _playerData.items[itemID] = newCount;
            
            // Phát sự kiện
            OnItemChanged?.Invoke(itemID, newCount);
            Debug.Log($"Added {amount} of item '{itemID}'. New total: {newCount}");
        }

        /// <summary>
        /// Sử dụng (tiêu thụ) một vật phẩm.
        /// </summary>
        /// <returns>True nếu sử dụng thành công, False nếu không có hoặc không đủ.</returns>
        public bool UseItem(string itemID, int amount = 1)
        {
            if (_playerData == null || string.IsNullOrEmpty(itemID) || amount <= 0) return false;

            int currentCount = GetItemCount(itemID);

            if (currentCount < amount)
            {
                Debug.LogWarning(string.Format(LocalizationSystem.GetText("inventory_error_not_enough_item"), itemID, amount, currentCount));
                // Có thể gọi UINotificationManager ở đây
                return false;
            }

            int newCount = currentCount - amount;
            
            if (newCount > 0)
            {
                _playerData.items[itemID] = newCount;
            }
            else
            {
                // Xóa khỏi dictionary nếu hết sạch để giữ cho dictionary gọn gàng
                _playerData.items.Remove(itemID);
            }
            
            // Phát sự kiện
            OnItemChanged?.Invoke(itemID, newCount);
            Debug.Log($"Used {amount} of item '{itemID}'. Remaining: {newCount}");
            return true;
        }

        #endregion

        #region Equipment Management

        /// <summary>
        /// Lấy toàn bộ trang bị hiện có trong túi đồ (chưa mặc)
        /// </summary>
        public List<EquipmentData> GetEquipments()
        {
            if (_playerData == null) return new List<EquipmentData>();
            return _playerData.equipments;
        }

        /// <summary>
        /// Thêm một trang bị mới vào túi đồ.
        /// </summary>
        public void AddEquipment(EquipmentData equipment)
        {
            if (_playerData == null || equipment == null) return;
            
            // Assign a unique ID if one is lacking (just as a safety backup)
            if (string.IsNullOrEmpty(equipment.id))
            {
                equipment.id = System.Guid.NewGuid().ToString();
            }

            _playerData.equipments.Add(equipment);
            GameManager.Instance.DataManager.SavePlayerData();
            OnEquipmentChanged?.Invoke();
            Debug.Log($"Added equipment: {equipment.equipmentName}");
        }

        /// <summary>
        /// Xóa một trang bị khỏi túi đồ (khi bán hoặc ghép đồ).
        /// </summary>
        public bool RemoveEquipment(EquipmentData equipment)
        {
            if (_playerData == null || equipment == null) return false;
            
            bool removed = _playerData.equipments.Remove(equipment);
            if (removed)
            {
                GameManager.Instance.DataManager.SavePlayerData();
                OnEquipmentChanged?.Invoke();
            }
            return removed;
        }

        #endregion
    }
}
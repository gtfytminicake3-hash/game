namespace LegendOfBlood
{
    using System;
    using LegendOfBlood.Managers; // Thêm using để truy cập PlayerQuestStatus
    using System.Collections.Generic;
    using UnityEngine;

    // =====================================================================================
    // NEW DATA STRUCTURES FOR "FIRE-AND-FORGET" EXPEDITION SYSTEM
    // =====================================================================================

    public enum ExpeditionState
    {
        Traveling,
        Exploring,
        Returning
    }

    [Serializable]
    public class LootData
    {
        public Dictionary<string, int> items;
        public List<EquipmentData> equipments;
        // Can be extended with gold, wood, etc. if needed
        public int gold;
        public int wood;
        public int stone;
        public List<HeroData> rescuedHeroes;

        public LootData()
        {
            items = new Dictionary<string, int>();
            equipments = new List<EquipmentData>();
            rescuedHeroes = new List<HeroData>();
            gold = 0;
            wood = 0;
            stone = 0;
        }
    }

    [Serializable]
    public class ExpeditionReport
    {
        public string poiId;
        public string poiName;
        public Combat.CombatResult combatResult;
        public LootData loot;
        public int experienceGained;

        // === BOSS VICTORY TRACKING ===
        // true = trận đánh Boss thắng → kích hoạt hospital + kết thúc expedition
        public bool isBossVictory;
        public string bossNodeId;       // ID của boss node đã bị đánh bại
        public System.Collections.Generic.List<string> squadIds; // IDs của squad đi thám hiểm
        public System.Collections.Generic.List<HeroBattleOutcomeSnapshot> heroOutcomeSnapshots; // Snapshot thương vong sau trận Boss
    }

    [Serializable]
    public class HeroBattleOutcomeSnapshot
    {
        public string heroId;
        public float hpAfterBattle;
        public float maxHp;
        public bool isDead;
        public int injurySeverity; // 0 = None, 1 = Light, 2 = Severe
    }

    [Serializable]
    public class ActiveExpedition
    {
        public string expeditionId;
        public List<string> heroIds;
        public string poiId;
        
        public ExpeditionState currentState;
        public long stateEndTimestamp; // Thời điểm kết thúc trạng thái hiện tại
        public long travelDurationMs;
        public long combatDurationMs;

        // Backward compatibility
        public long completionTimestamp;

        // The most important field: the pre-calculated result
        public ExpeditionReport preCalculatedReport;
    }


    // =====================================================================================
    // EXISTING DATA STRUCTURES
    // =====================================================================================

    [Serializable]
    public class PlayerResources
    {
        public int gold;
        public int wood;
        public int stone;
        public int diamond;
    }

    [Serializable]
    public class PlayerData : ISerializationCallbackReceiver
    {
        public string playerName;
        public PlayerResources resources;
        public Dictionary<string, int> items;
        public List<EquipmentData> equipments;
        public int arenaPoints;
        public int arenaTickets;
        public long lastTicketRefreshTimestamp;
        public int arenaCoins;
        
        public List<POIData> WorldPois;
        
        // --- MODIFIED: Using the new ActiveExpedition structure ---
        public List<ActiveExpedition> ActiveExpeditions;

        // --- NEW: For the mailbox system ---
        public List<ExpeditionReport> UnclaimedReports;

        // --- NEW: Player's core progression data ---
        public List<HeroData> Heroes;
        public List<PlayerQuestStatus> QuestStatuses;
        
        // --- NEW: Quest Reset Timestamps ---
        public long lastDailyResetTimestamp;
        public long lastWeeklyResetTimestamp;

        // --- NEW: Ad Tracking ---
        public int dailySummonAdsWatched;
        public int dailyDoubleGoldAdsWatched;
        public int dailyFreeHealsWatched;            // Mới thêm
        public int dailyFreeSummonsWatched;          // Mới thêm
        public int dailyBuildingSpeedUpsWatched;     // Mới thêm
        public int dailyArenaTicketAdsWatched;       // Mới thêm
        public int dailyMutationAdsWatched;          // Mới thêm
        public int dailyTowerSkipAdsWatched;         // Mới thêm
        
        // --- CÁC VỊ TRÍ CUỐI CÙNG ---
        public int dailyShopFreebieAdsWatched;
        public int dailyCombatReviveAdsWatched;
        public int dailyMysticChestAdsWatched;
        public bool useDynamicUI;

        // --- NEW: Arena Shop State ---
        public long lastArenaShopRefreshTimestamp;
        public List<ArenaShopGood> currentArenaShopGoods;

        // --- NEW: Offline Progression ---
        public long lastOfflineTimestamp;

        // --- NEW: Player Progression ---
        public int playerLevel;
        public int playerExp;

        // --- NEW: King God Pass Progression ---
        public int passLevel;
        public int passExp;
        public bool isPremiumPassUnlocked;
        public List<int> claimedFreePassLevels;
        public List<int> claimedPremiumPassLevels;

        // --- EXISTING: For serialization ---
        public Dictionary<BuildingType, int> BuildingLevels;
        public Dictionary<string, int> ClearedDifficulties;

        [SerializeField] private List<string> _serializedItemIDs = new List<string>();
        [SerializeField] private List<int> _serializedItemCounts = new List<int>();
        [SerializeField] private List<BuildingType> _serializedBuildingTypes = new List<BuildingType>();
        [SerializeField] private List<int> _serializedBuildingLevels = new List<int>();
        [SerializeField] private List<string> _serializedClearedPoiIds = new List<string>();
        [SerializeField] private List<int> _serializedClearedDifficulties = new List<int>();
        
        public PlayerData()
        {
            playerName = "Nhà Lai Tạo";
            resources = new PlayerResources { gold = 500, wood = 100, stone = 100, diamond = 0 };
            items = new Dictionary<string, int>();
            equipments = new List<EquipmentData>();
            WorldPois = new List<POIData>();
            ActiveExpeditions = new List<ActiveExpedition>();
            UnclaimedReports = new List<ExpeditionReport>(); // Initialize the new list
            Heroes = new List<HeroData>(); // Khởi tạo danh sách Heroes
            QuestStatuses = new List<PlayerQuestStatus>(); // Khởi tạo danh sách QuestStatuses
            BuildingLevels = new Dictionary<BuildingType, int>();
            ClearedDifficulties = new Dictionary<string, int>();
            arenaPoints = 0;
            arenaTickets = 5;
            lastTicketRefreshTimestamp = 0;
            arenaCoins = 0;
            dailySummonAdsWatched = 0;
            dailyDoubleGoldAdsWatched = 0;
            lastOfflineTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            useDynamicUI = false;
            
            playerLevel = 1;
            playerExp = 0;

            passLevel = 1;
            passExp = 0;
            isPremiumPassUnlocked = false;
            claimedFreePassLevels = new List<int>();
            claimedPremiumPassLevels = new List<int>();

            lastArenaShopRefreshTimestamp = 0;
            currentArenaShopGoods = new List<ArenaShopGood>();

            // --- QUÀ TÂN THỦ: 10 VÉ CHIÊU MỘ ---
            var welcomeLoot = new LootData();
            welcomeLoot.items.Add("IT_GACHA_TICKET", 10);
            var welcomeMail = new ExpeditionReport
            {
                poiId = "WELCOME_GIFT",
                poiName = "Thư Chào Mừng Tân Thủ",
                combatResult = new Combat.CombatResult { DidPlayerWin = true }, // Đánh lừa UI hiển thị thư xanh
                loot = welcomeLoot,
                experienceGained = 0
            };
            UnclaimedReports.Add(welcomeMail);
        }

        public void OnBeforeSerialize()
        {
            _serializedItemIDs.Clear();
            _serializedItemCounts.Clear();
            _serializedBuildingTypes.Clear();
            _serializedBuildingLevels.Clear();
            _serializedClearedPoiIds.Clear();
            _serializedClearedDifficulties.Clear();

            foreach (var kvp in items)
            {
                _serializedItemIDs.Add(kvp.Key);
                _serializedItemCounts.Add(kvp.Value);
            }

            if (BuildingLevels != null)
            {
                foreach (var kvp in BuildingLevels)
                {
                    _serializedBuildingTypes.Add(kvp.Key);
                    _serializedBuildingLevels.Add(kvp.Value);
                }
            }

            if (ClearedDifficulties != null)
            {
                foreach (var kvp in ClearedDifficulties)
                {
                    _serializedClearedPoiIds.Add(kvp.Key);
                    _serializedClearedDifficulties.Add(kvp.Value);
                }
            }
        }
        
        public void OnAfterDeserialize()
        {
            items = new Dictionary<string, int>();
            equipments ??= new List<EquipmentData>();
            BuildingLevels = new Dictionary<BuildingType, int>();
            ClearedDifficulties = new Dictionary<string, int>();
            
            WorldPois ??= new List<POIData>();
            ActiveExpeditions ??= new List<ActiveExpedition>();
            UnclaimedReports ??= new List<ExpeditionReport>(); // Ensure list is not null after deserialization
            Heroes ??= new List<HeroData>(); // Đảm bảo không null sau khi tải
            QuestStatuses ??= new List<PlayerQuestStatus>(); // Đảm bảo không null sau khi tải
            currentArenaShopGoods ??= new List<ArenaShopGood>();

            if (_serializedItemIDs.Count != _serializedItemCounts.Count)
            {
                Debug.LogError("Item data is corrupt: ID and count lists do not match!");
                return;
            }

            for (int i = 0; i < _serializedItemIDs.Count; i++)
            {
                items.Add(_serializedItemIDs[i], _serializedItemCounts[i]);
            }

            if (_serializedBuildingTypes.Count != _serializedBuildingLevels.Count)
            {
                Debug.LogError("Building level data is corrupt: Type and Level lists do not match!");
                return;
            }

            for (int i = 0; i < _serializedBuildingTypes.Count; i++)
            {
                BuildingLevels.Add(_serializedBuildingTypes[i], _serializedBuildingLevels[i]);
            }

            if (_serializedClearedPoiIds != null && _serializedClearedDifficulties != null && _serializedClearedPoiIds.Count == _serializedClearedDifficulties.Count)
            {
                for (int i = 0; i < _serializedClearedPoiIds.Count; i++)
                {
                    ClearedDifficulties.Add(_serializedClearedPoiIds[i], _serializedClearedDifficulties[i]);
                }
            }
        }
    }
}

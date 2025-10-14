namespace LegendOfBlood
{
    using System;
    using LegendOfBlood.Managers; // Thêm using để truy cập PlayerQuestStatus
    using System.Collections.Generic;
    using UnityEngine;

    // =====================================================================================
    // NEW DATA STRUCTURES FOR "FIRE-AND-FORGET" EXPEDITION SYSTEM
    // =====================================================================================

    [Serializable]
    public class LootData
    {
        public Dictionary<string, int> items;
        // Can be extended with gold, wood, etc. if needed
        public int gold;

        public LootData()
        {
            items = new Dictionary<string, int>();
            gold = 0;
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
        // Add any other data needed for the report
    }

    [Serializable]
    public class ActiveExpedition
    {
        public string expeditionId;
        public List<string> heroIds;
        public string poiId;
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
    }

    [Serializable]
    public class PlayerData : ISerializationCallbackReceiver
    {
        public string playerName;
        public PlayerResources resources;
        public Dictionary<string, int> items;
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

        // --- EXISTING: For serialization ---
        public Dictionary<BuildingType, int> BuildingLevels;

        [SerializeField] private List<string> _serializedItemIDs = new List<string>();
        [SerializeField] private List<int> _serializedItemCounts = new List<int>();
        [SerializeField] private List<BuildingType> _serializedBuildingTypes = new List<BuildingType>();
        [SerializeField] private List<int> _serializedBuildingLevels = new List<int>();
        
        public PlayerData()
        {
            playerName = "Nhà Lai Tạo";
            resources = new PlayerResources { gold = 500, wood = 100, stone = 100 };
            items = new Dictionary<string, int>();
            WorldPois = new List<POIData>();
            ActiveExpeditions = new List<ActiveExpedition>();
            UnclaimedReports = new List<ExpeditionReport>(); // Initialize the new list
            Heroes = new List<HeroData>(); // Khởi tạo danh sách Heroes
            QuestStatuses = new List<PlayerQuestStatus>(); // Khởi tạo danh sách QuestStatuses
            BuildingLevels = new Dictionary<BuildingType, int>();
            arenaPoints = 0;
            arenaTickets = 5;
            lastTicketRefreshTimestamp = 0;
            arenaCoins = 0;
        }

        public void OnBeforeSerialize()
        {
            _serializedItemIDs.Clear();
            _serializedItemCounts.Clear();
            _serializedBuildingTypes.Clear();
            _serializedBuildingLevels.Clear();

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
        }
        
        public void OnAfterDeserialize()
        {
            items = new Dictionary<string, int>();
            BuildingLevels = new Dictionary<BuildingType, int>();
            
            WorldPois ??= new List<POIData>();
            ActiveExpeditions ??= new List<ActiveExpedition>();
            UnclaimedReports ??= new List<ExpeditionReport>(); // Ensure list is not null after deserialization
            Heroes ??= new List<HeroData>(); // Đảm bảo không null sau khi tải
            QuestStatuses ??= new List<PlayerQuestStatus>(); // Đảm bảo không null sau khi tải

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
        }
    }
}

namespace LegendOfBlood
{
    // Bỏ 'using LegendOfBlood.Combat;' nếu không có lớp nào khác cần nó trực tiếp
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    // Các enum và lớp dữ liệu nên được tập trung ở đây để dễ quản lý.
    
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
        
        // --- SỬA LỖI: Chỉ định namespace đầy đủ để loại bỏ mọi sự nhầm lẫn ---
        public Combat.CombatResult combatResult; 
    }

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
        
        public List<POIData> WorldPois;
        public List<Expedition> ActiveExpeditions;
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
            ActiveExpeditions = new List<Expedition>();
            BuildingLevels = new Dictionary<BuildingType, int>();
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
            ActiveExpeditions ??= new List<Expedition>();

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
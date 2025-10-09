namespace LegendOfBlood
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Lớp con để chứa các loại tài nguyên chính, giúp code gọn gàng hơn.
    /// </summary>
    [Serializable]
    public class PlayerResources
    {
        public int gold;
        public int wood;
        public int stone;
    }

    /// <summary>
    /// Chứa tất cả dữ liệu liên quan đến người chơi, như tài nguyên và vật phẩm.
    /// Triển khai ISerializationCallbackReceiver để xử lý việc lưu/tải Dictionary.
    /// </summary>
    [Serializable]
    public class PlayerData : ISerializationCallbackReceiver
    {
        public string playerName;

        // --- Dữ liệu Tài nguyên & Vật phẩm ---
        public PlayerResources resources;
        public Dictionary<string, int> items;

        // --- Dữ liệu Gameplay chính ---
        // Các danh sách này sẽ được quản lý bởi DataManager, nhưng được lưu trữ ở đây.
        // Tuy nhiên, trong thiết kế hiện tại, chúng được lưu trong lớp SaveData.
        // Để tránh nhầm lẫn và dữ liệu trùng lặp, chúng ta nên xóa chúng khỏi đây
        // và chỉ giữ chúng trong lớp SaveData của DataManager.
        // public List<HeroData> AllHeroes; // Sẽ được quản lý bởi DataManager.SaveData
        public List<POIData> WorldPois;
        public List<Expedition> ActiveExpeditions;
        public Dictionary<BuildingType, int> BuildingLevels;

        // --- BIẾN TRUNG GIAN ĐỂ SERIALIZE DICTIONARY ---
        // JsonUtility không thể serialize Dictionary trực tiếp, nên chúng ta dùng các List này.
        // For 'items'
        [SerializeField] private List<string> _serializedItemIDs = new List<string>();
        [SerializeField] private List<int> _serializedItemCounts = new List<int>();

        // For 'BuildingLevels'
        [SerializeField] private List<BuildingType> _serializedBuildingTypes = new List<BuildingType>();
        [SerializeField] private List<int> _serializedBuildingLevels = new List<int>();

        /// <summary>
        /// Constructor cho người chơi mới.
        /// </summary>
        public PlayerData()
        {
            playerName = "Nhà Lai Tạo";
            // Tài nguyên khởi đầu
            resources = new PlayerResources { gold = 500, wood = 100, stone = 100 };
            items = new Dictionary<string, int>();

            // AllHeroes = new List<HeroData>();
            WorldPois = new List<POIData>();
            ActiveExpeditions = new List<Expedition>();
            BuildingLevels = new Dictionary<BuildingType, int>();
        }

        /// <summary>
        /// Được gọi ngay trước khi Unity serialize đối tượng này (ví dụ: khi lưu game).
        /// Chuyển dữ liệu từ Dictionary sang hai List.
        /// </summary>
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

            // BuildingLevels có thể null nếu đây là dữ liệu cũ chưa có, cần kiểm tra
            if (BuildingLevels != null)
            {
                foreach (var kvp in BuildingLevels)
                {
                    _serializedBuildingTypes.Add(kvp.Key);
                    _serializedBuildingLevels.Add(kvp.Value);
                }
            }
        }

        /// <summary>
        /// Được gọi ngay sau khi Unity deserialize đối tượng này (ví dụ: khi tải game).
        /// Xây dựng lại Dictionary từ dữ liệu trong hai List.
        /// </summary>
        public void OnAfterDeserialize()
        {
            items = new Dictionary<string, int>();
            BuildingLevels = new Dictionary<BuildingType, int>();

            // Khởi tạo các list nếu chúng là null (quan trọng khi tải dữ liệu cũ)
            // AllHeroes ??= new List<HeroData>();
            WorldPois ??= new List<POIData>();
            ActiveExpeditions ??= new List<Expedition>();

            if (_serializedItemIDs.Count != _serializedItemCounts.Count)
            {
                Debug.LogError("Dữ liệu vật phẩm bị lỗi: số lượng ID và số đếm không khớp!");
                return;
            }

            for (int i = 0; i < _serializedItemIDs.Count; i++)
            {
                items.Add(_serializedItemIDs[i], _serializedItemCounts[i]);
            }

            if (_serializedBuildingTypes.Count != _serializedBuildingLevels.Count)
            {
                Debug.LogError("Dữ liệu cấp độ công trình bị lỗi: số lượng Type và Level không khớp!");
                return;
            }

            for (int i = 0; i < _serializedBuildingTypes.Count; i++)
            {
                BuildingLevels.Add(_serializedBuildingTypes[i], _serializedBuildingLevels[i]);
            }
        }
    } // End of PlayerData class
} // End of namespace

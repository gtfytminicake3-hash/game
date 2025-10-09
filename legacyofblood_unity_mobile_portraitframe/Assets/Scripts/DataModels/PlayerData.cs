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
        public PlayerResources resources;

        // Dictionary để truy cập vật phẩm hiệu quả.
        // Không được serialize trực tiếp bởi JsonUtility.
        public Dictionary<string, int> items;

        // Hai list này được dùng làm "trung gian" để JsonUtility có thể lưu/tải.
        [SerializeField] private List<string> _serializedItemIDs = new List<string>();
        [SerializeField] private List<int> _serializedItemCounts = new List<int>();

        /// <summary>
        /// Constructor cho người chơi mới.
        /// </summary>
        public PlayerData()
        {
            playerName = "Nhà Lai Tạo";
            // Tài nguyên khởi đầu
            resources = new PlayerResources { gold = 500, wood = 100, stone = 100 };
            items = new Dictionary<string, int>();
        }

        /// <summary>
        /// Được gọi ngay trước khi Unity serialize đối tượng này (ví dụ: khi lưu game).
        /// Chuyển dữ liệu từ Dictionary sang hai List.
        /// </summary>
        public void OnBeforeSerialize()
        {
            _serializedItemIDs.Clear();
            _serializedItemCounts.Clear();

            foreach (var kvp in items)
            {
                _serializedItemIDs.Add(kvp.Key);
                _serializedItemCounts.Add(kvp.Value);
            }
        }

        /// <summary>
        /// Được gọi ngay sau khi Unity deserialize đối tượng này (ví dụ: khi tải game).
        /// Xây dựng lại Dictionary từ dữ liệu trong hai List.
        /// </summary>
        public void OnAfterDeserialize()
        {
            items = new Dictionary<string, int>();

            if (_serializedItemIDs.Count != _serializedItemCounts.Count)
            {
                Debug.LogError("Dữ liệu vật phẩm bị lỗi: số lượng ID và số đếm không khớp!");
                return;
            }

            for (int i = 0; i < _serializedItemIDs.Count; i++)
            {
                items.Add(_serializedItemIDs[i], _serializedItemCounts[i]);
            }
        }
    }
}
namespace LegendOfBlood
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Các loại Điểm ưa thích (Point of Interest) trên bản đồ.
    /// </summary>
    public enum POIType
    {
        Dungeon,
        RescueMission
    }

    /// <summary>
    /// Lớp dữ liệu chứa thông tin về một POI.
    /// [Serializable] để có thể lưu/tải bằng DataManager.
    /// </summary>
    [Serializable]
    public class POIData
    {
        public string poiId;
        public string poiName;
        public POIType type;
        public Vector2 position;
        public int difficultyLevel;

        // --- THÊM MỚI: Danh sách quái vật trong POI ---
        // Giả sử bạn sẽ có một lớp MonsterData tương tự như HeroData.
        // Ở đây chúng ta chỉ cần lưu ID của chúng.
        public List<string> monsterIDs;
    }
}
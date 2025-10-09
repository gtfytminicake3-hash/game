namespace LegendOfBlood
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Loại của một Điểm quan tâm (Point of Interest).
    /// </summary>
    public enum POIType
    {
        Dungeon,
        RescueMission,
        BossLair
    }

    /// <summary>
    /// Lớp dữ liệu chứa thông tin về một địa điểm trên bản đồ thế giới.
    /// </summary>
    [Serializable]
    public class POIData
    {
        public string poiId;
        public string poiName;
        public POIType type;
        
        // Tọa độ trên bản đồ thế giới
        public Vector2 position;

        public int difficultyLevel;

        // Có thể mở rộng để chứa thông tin về phần thưởng, kẻ địch, v.v.
    }
}
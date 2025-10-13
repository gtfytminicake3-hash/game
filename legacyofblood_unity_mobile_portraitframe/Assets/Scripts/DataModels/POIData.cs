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
        RescueMission,
        // --- NEW: Added Tower of Trials --- 
        TowerOfTrials 
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
        public List<string> monsterIDs;

        // --- NEW: Fields specific to the Tower of Trials ---
        public int currentFloor;       // The floor the tower is currently at.
        public long recoveryEndTime;    // Timestamp until which the tower progress is saved.
    }
}

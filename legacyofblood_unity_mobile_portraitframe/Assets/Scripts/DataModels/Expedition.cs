namespace LegendOfBlood
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Các trạng thái có thể có của một chuyến thám hiểm.
    /// </summary>
    public enum ExpeditionStatus
    {
        Traveling,   // Đang trên đường đi
        Exploring,   // Đã đến nơi, đang chiến đấu/khám phá
        Returning,   // Đang trên đường về
        Finished     // Đã hoàn thành và chờ xử lý kết quả
    }

    /// <summary>
    /// Lớp dữ liệu chứa thông tin về một chuyến thám hiểm đang hoạt động.
    /// </summary>
    [Serializable]
    public class Expedition
    {
        public string id;
        public List<string> squadHeroIDs; // Chỉ lưu ID để tránh trùng lặp dữ liệu
        public POIData destination;

        public ExpeditionStatus status;
        
        // Sử dụng Unix Millisecond Timestamps để theo dõi thời gian
        public long startTime;
        public long endTime; // Thời gian dự kiến kết thúc trạng thái hiện tại
    }
}

namespace LegendOfBlood
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Các loại công trình mà người chơi có thể xây dựng.
    /// </summary>
    public enum BuildingType
    {
        TownHall,       // Nhà chính
        Barracks,       // Doanh trại
        Hospital,       // Bệnh viện
        BreedingPen     // Chuồng lai tạo
    }

    /// <summary>
    /// Lớp dữ liệu chứa trạng thái của một công trình mà người chơi sở hữu.
    /// Dữ liệu này sẽ được lưu vào file save của người chơi.
    /// </summary>
    [Serializable]
    public class Building
    {
        [Tooltip("ID duy nhất của công trình này (hữu ích nếu người chơi có thể xây nhiều công trình cùng loại).")]
        public string id;

        [Tooltip("Loại công trình.")]
        public BuildingType type;

        [Tooltip("Cấp độ hiện tại của công trình.")]
        public int level;

        [Tooltip("Cờ cho biết công trình có đang trong quá trình xây dựng/nâng cấp hay không.")]
        public bool isUnderConstruction;

        [Tooltip("Dấu thời gian (Unix Milliseconds) khi quá trình xây dựng/nâng cấp sẽ hoàn thành.")]
        public long constructionEndTime;

        /// <summary>
        /// Constructor để tạo một công trình mới.
        /// </summary>
        /// <param name="type">Loại công trình</param>
        /// <param name="startLevel">Cấp độ khởi đầu, thường là 1</param>
        public Building(BuildingType type, int startLevel = 1)
        {
            this.id = Guid.NewGuid().ToString();
            this.type = type;
            this.level = startLevel;
            this.isUnderConstruction = false;
            this.constructionEndTime = 0;
        }

        // Cần một constructor không tham số để JsonUtility có thể deserialize
        public Building() { }
    }
}
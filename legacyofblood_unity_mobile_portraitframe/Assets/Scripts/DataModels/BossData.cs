namespace LegendOfBlood
{
    using UnityEngine;

    /// <summary>
    /// Lớp con chứa thông tin về cơ chế đặc biệt của Boss.
    /// </summary>
    [System.Serializable]
    public class BossMechanic
    {
        [Tooltip("Tên của cơ chế, ví dụ: 'Da Dày', 'Nổi Giận'.")]
        public string name;
        
        [Tooltip("Mô tả chi tiết cơ chế để designer hiểu rõ.")]
        [TextArea(2, 4)]
        public string description;

        // Có thể thêm các trường dữ liệu cho cơ chế ở đây
        // Ví dụ: đối với 'Da Dày', có thể thêm trường `minAttackToDamage`
        public int value; // Một giá trị số đa dụng
    }

    /// <summary>
    /// Lớp con chứa thông tin về các kỹ năng của Boss.
    /// </summary>
    [System.Serializable]
    public class BossSkills
    {
        [Header("Normal Attack")]
        public string normalAttackName = "Tấn công thường";
        [Tooltip("Hệ số nhân sát thương cho đòn tấn công thường.")]
        public float normalAttackMultiplier = 1.0f;

        [Header("Area of Effect (AOE) Attack")]
        public string aoeAttackName = "Kỹ năng diện rộng";
        [Tooltip("Hệ số nhân sát thương cho đòn tấn công AOE.")]
        public float aoeAttackMultiplier = 0.75f;
    }
    
    /// <summary>
    /// Định nghĩa ScriptableObject cho một Boss.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBoss", menuName = "LegendOfBlood/Boss Data", order = 3)]
    public class BossData : ScriptableObject
    {
        [Header("Basic Information")]
        [Tooltip("ID duy nhất của Boss, ví dụ: 'BOSS_GOLEM_01'.")]
        public string id;

        [Tooltip("Tên của Boss sẽ hiển thị trong game.")]
        public string bossName;

        [Tooltip("Prefab của model Boss để hiển thị trong trận đấu (nếu có).")]
        public GameObject modelPrefab;

        [Header("Combat Stats")]
        [Tooltip("Các chỉ số cơ bản của Boss.")]
        public HeroStats stats;

        [Tooltip("Thông tin về các kỹ năng của Boss.")]
        public BossSkills skills;

        [Header("Special Mechanics")]
        [Tooltip("Danh sách các cơ chế đặc biệt của Boss. Dựa trên GDD_03.")]
        public BossMechanic mechanic;
        
        [ContextMenu("Auto-fill ID from Name")]
        private void AutofillID()
        {
            this.id = this.name;
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                id = this.name;
            }
        }
    }
}
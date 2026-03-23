// --- START OF FILE Trait.cs (FIXED) ---

using System.Collections.Generic;
using UnityEngine;

namespace LegendOfBlood
{
    /// <summary>
    /// Các loại hiệu ứng mà một Trait có thể gây ra.
    /// </summary>
    public enum TraitEffectType
    {
        ADD_STAT,        // Cộng thẳng vào chỉ số (HP, ATK, DEF, SPD)
        MULTIPLY_STAT,   // Nhân chỉ số theo phần trăm
        AURA,            // Hiệu ứng hào quang ảnh hưởng đồng đội/kẻ địch đầu trận
        SPECIAL          // Hiệu ứng đặc biệt kích hoạt theo điều kiện (khi chết, khi hạ gục,...)
    }

    /// <summary>
    /// Lớp con chứa thông tin chi tiết của một hiệu ứng.
    /// [System.Serializable] để nó có thể được hiển thị trong Inspector.
    /// </summary>
    [System.Serializable]
    public class TraitEffect
    {
        [Tooltip("Loại hiệu ứng mà Trait này gây ra.")]
        public TraitEffectType type;

        [Header("For ADD_STAT & MULTIPLY_STAT")]
        [Tooltip("HP: Cộng thẳng giá trị. ATK/DEF/SPD: Cộng thẳng giá trị.")]
        public int hp;
        [Tooltip("ATK: Cộng thẳng giá trị.")]
        public int atk;
        [Tooltip("DEF: Cộng thẳng giá trị.")]
        public int def;
        [Tooltip("SPD: Cộng thẳng giá trị.")]
        public int spd;

        [Header("For SPECIAL & AURA")]
        [Tooltip("Mô tả hiệu ứng để game designer dễ nhận biết.")]
        [TextArea(2, 4)]
        public string effectDescription;
    }

    /// <summary>
    /// Định nghĩa ScriptableObject cho một Trait (Đặc tính).
    /// Cho phép tạo các asset Trait trong Unity Editor.
    /// </summary>
    [CreateAssetMenu(fileName = "NewTrait", menuName = "LegendOfBlood/Trait", order = 1)]
    public class Trait : ScriptableObject
    {
        public enum RarityRank { D, C, B, A, S, SS, SSS }

        [Header("Basic Information")]
        [Tooltip("ID duy nhất của Trait, ví dụ: 'S_04', 'SS_07'. Rất quan trọng, không được trùng.")]
        public string id;

        [Tooltip("Tên của Trait sẽ hiển thị trong game.")]
        public string traitName;

        [Tooltip("Mô tả chi tiết về Trait, sẽ hiển thị cho người chơi.")]
        [TextArea(3, 5)]
        public string description;

        [Tooltip("Icon đại diện cho Đặc tính.")]
        public Sprite icon;

        [Header("Trait Evolution")]
        public RarityRank rank;
        public string familyId; // Ví dụ: "ATK_UP", "HP_ON_HIT"
        public string nextUpgradeTraitID; // ID của Trait kế tiếp trong cùng family

        [Header("Trait Effects")]
        [Tooltip("Danh sách tất cả các hiệu ứng mà Trait này gây ra.")]
        public List<TraitEffect> effects;

        // --- CÁC HÀM TIỆN ÍCH (Tùy chọn nhưng rất hữu ích) ---

        /// <summary>
        /// Tự động điền ID dựa trên tên file của asset.
        /// Chạy khi asset được tạo hoặc khi bạn chọn "Auto-fill ID from Name" trong menu chuột phải.
        /// </summary>
        [ContextMenu("Auto-fill ID from Name")]
        private void AutofillID()
        {
            this.id = this.name;
        }

        /// <summary>
        /// Chạy mỗi khi một giá trị trong Inspector của asset này thay đổi.
        /// Dùng để đảm bảo ID không bao giờ là rỗng.
        /// </summary>
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                // Nếu ID rỗng, tạm thời dùng tên của asset
                id = this.name;
                Debug.LogWarning($"Trait '{this.name}' có ID rỗng. Đã tự động điền bằng tên asset. Hãy đặt một ID duy nhất.", this);
            }
        }
    }
}
// --- END OF FILE Trait.cs (FIXED) ---
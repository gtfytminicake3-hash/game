namespace LegendOfBlood
{
    using UnityEngine;

    /// <summary>
    /// Các loại vật phẩm để phân loại và lọc trong túi đồ.
    /// </summary>
    public enum ItemType
    {
        Consumable,      // Vật phẩm tiêu thụ (thuốc, bùa)
        SpeedUp,         // Vật phẩm tăng tốc
        BreedingMaterial,// Vật phẩm dùng cho lai tạo
        ExpPotion        // Thẻ kinh nghiệm
    }

    /// <summary>
    /// Định nghĩa ScriptableObject cho một Vật phẩm.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "LegendOfBlood/Item Data", order = 4)]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Information")]
        [Tooltip("ID duy nhất của vật phẩm, ví dụ: 'ITEM_MUTATION_POTION'.")]
        public string id;

        [Tooltip("Tên của vật phẩm sẽ hiển thị trong game.")]
        public string itemName;

        [Tooltip("Mô tả chi tiết về công dụng của vật phẩm.")]
        [TextArea(3, 5)]
        public string description;

        [Tooltip("Icon của vật phẩm sẽ hiển thị trong UI.")]
        public Sprite icon;
        
        [Header("Item Properties")]
        [Tooltip("Phân loại vật phẩm.")]
        public ItemType type;
        
        [Tooltip("Vật phẩm có thể cộng dồn trong một ô hay không?")]
        public bool isStackable = true;
        
        [Tooltip("Số lượng tối đa trong một ô (nếu isStackable = true).")]
        [Min(1)]
        public int maxStackSize = 99;
        
        [Tooltip("Giá bán vật phẩm (nếu có thể bán).")]
        public int sellPrice;

        // --- Dữ liệu hiệu ứng ---
        // Phần này có thể được thiết kế phức tạp hơn bằng cách sử dụng các lớp con,
        // nhưng để đơn giản, chúng ta có thể dùng các trường dữ liệu trực tiếp.
        [Header("Effect Data (Contextual)")]
        [Tooltip("Đối với vật phẩm SpeedUp, giá trị này là số giây được giảm đi.")]
        public int speedUpValueInSeconds;
        
        // Có thể thêm các trường khác cho các loại vật phẩm khác
        // public Trait guaranteedTrait; // Ví dụ cho một loại bùa đảm bảo trait

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
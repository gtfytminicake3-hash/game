namespace LegendOfBlood
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class InventoryItemCard : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemCountText;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;
        [SerializeField] private Image itemIcon;
        // Optionally a use button if items can be consumed directly from here, but usually it's from the context menu
        [SerializeField] private Button useButton;

        private ItemData _itemData;

        public void Setup(ItemData itemData, int count)
        {
            _itemData = itemData;

            if (itemNameText != null) itemNameText.text = itemData.itemName;
            if (itemCountText != null) itemCountText.text = $"x{count}";
            if (itemDescriptionText != null) itemDescriptionText.text = itemData.description;
            if (itemIcon != null && itemData.icon != null) itemIcon.sprite = itemData.icon;

            if (useButton != null)
            {
                useButton.onClick.RemoveAllListeners();
                useButton.onClick.AddListener(OnUseClicked);
                
                // Only enable use button if it's a consumable that can be used directly from inventory
                // For now, let's just log it or show a toast
                useButton.interactable = itemData.type == ItemType.Consumable;
            }
        }

        private void OnUseClicked()
        {
            if (_itemData == null) return;
            
            bool success = InventoryManager.Instance.UseItem(_itemData.id, 1);
            if (success)
            {
                // Optionally handle specific logic for simple consumables like IT_EXP_BOOK_S here or via an event
                GameManager.Instance.UINotificationManager.ShowNotification(
                    string.Format(LocalizationSystem.GetText("inventory_item_used_success"), _itemData.itemName)
                );
            }
        }
    }
}

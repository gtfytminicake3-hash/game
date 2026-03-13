using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace LegendOfBlood
{
    public enum InventoryTabType
    {
        Item,
        Equipment
    }

    public class InventoryPanel : UIPanel
    {
        [Header("Tabs")]
        [SerializeField] private Button itemTabButton;
        [SerializeField] private Button equipmentTabButton;
        [SerializeField] private GameObject itemPage;
        [SerializeField] private GameObject equipmentPage;

        [Header("UI Grid - Items")]
        [SerializeField] private Transform itemGridParent;
        [SerializeField] private GameObject itemSlotPrefab;

        [Header("UI Grid - Equipments")]
        [SerializeField] private Transform equipmentGridParent;
        [SerializeField] private GameObject equipmentCardPrefab;

        [Header("Item Detail View")]
        [SerializeField] private GameObject detailView;
        [SerializeField] private Image detailIcon;
        [SerializeField] private TextMeshProUGUI detailNameText;
        [SerializeField] private TextMeshProUGUI detailDescText;
        [SerializeField] private TextMeshProUGUI detailCountText;
        [SerializeField] private Button useButton;

        [Header("Navigation")]
        [SerializeField] private Button closeButton;

        private List<ItemSlot> _activeItemSlots = new List<ItemSlot>();
        private List<InventoryEquipmentCard> _activeEquipmentSlots = new List<InventoryEquipmentCard>();
        private ItemData _selectedItem;

        private void Awake()
        {
            PanelType = UIPanelType.Inventory;

            if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
            if (useButton != null) useButton.onClick.AddListener(OnUseButtonClicked);
            
            if (itemTabButton != null) itemTabButton.onClick.AddListener(() => SwitchTab(InventoryTabType.Item));
            if (equipmentTabButton != null) equipmentTabButton.onClick.AddListener(() => SwitchTab(InventoryTabType.Equipment));
            
            detailView.SetActive(false);
        }

        private void OnEnable()
        {
            SwitchTab(InventoryTabType.Item); // Default to Items
            
            InventoryManager.OnItemChanged += HandleItemChanged;
            InventoryManager.OnEquipmentChanged += HandleEquipmentChanged;
        }

        private void OnDisable()
        {
            InventoryManager.OnItemChanged -= HandleItemChanged;
            InventoryManager.OnEquipmentChanged -= HandleEquipmentChanged;
        }

        private void SwitchTab(InventoryTabType tabType)
        {
            detailView.SetActive(false); // Hide item details when switching

            if (tabType == InventoryTabType.Item)
            {
                if (itemPage != null) itemPage.SetActive(true);
                if (equipmentPage != null) equipmentPage.SetActive(false);
                
                // Highlight item tab
                if (itemTabButton != null) itemTabButton.GetComponent<Image>().color = Color.white;
                if (equipmentTabButton != null) equipmentTabButton.GetComponent<Image>().color = Color.gray;
                
                RefreshItems();
            }
            else
            {
                if (itemPage != null) itemPage.SetActive(false);
                if (equipmentPage != null) equipmentPage.SetActive(true);

                // Highlight equipment tab
                if (itemTabButton != null) itemTabButton.GetComponent<Image>().color = Color.gray;
                if (equipmentTabButton != null) equipmentTabButton.GetComponent<Image>().color = Color.white;
                
                RefreshEquipments();
            }
        }

        private void HandleItemChanged(string itemId, int newCount)
        {
            if (itemPage != null && itemPage.activeSelf) 
            {
                RefreshItems();
            }
            
            if (_selectedItem != null && _selectedItem.id == itemId)
            {
                if (newCount <= 0)
                {
                    detailView.SetActive(false);
                    _selectedItem = null;
                }
                else
                {
                    detailCountText.text = $"Số lượng: {newCount}";
                }
            }
        }

        private void HandleEquipmentChanged()
        {
            if (equipmentPage != null && equipmentPage.activeSelf)
            {
                RefreshEquipments();
            }
        }

        private void RefreshItems()
        {
            // Clear old slots
            foreach (var slot in _activeItemSlots)
            {
                Destroy(slot.gameObject);
            }
            _activeItemSlots.Clear();

            var allItemsConfigs = DataManager.Instance.AllItems;
            if (allItemsConfigs == null || allItemsConfigs.Count == 0 || DataManager.Instance.Player == null) 
                return;

            foreach (var kvp in DataManager.Instance.Player.items)
            {
                string itemId = kvp.Key;
                int amount = kvp.Value;

                if (amount > 0 && allItemsConfigs.TryGetValue(itemId, out ItemData itemData))
                {
                    GameObject slotObj = Instantiate(itemSlotPrefab, itemGridParent);
                    ItemSlot slot = slotObj.GetComponent<ItemSlot>();
                    if (slot != null)
                    {
                        slot.Setup(itemData, amount);
                        slot.OnClicked += HandleItemSlotClicked;
                        _activeItemSlots.Add(slot);
                    }
                }
            }
        }

        private void RefreshEquipments()
        {
            // Clear old equipments
            foreach (var slot in _activeEquipmentSlots)
            {
                Destroy(slot.gameObject);
            }
            _activeEquipmentSlots.Clear();

            var equipments = InventoryManager.Instance.GetEquipments();
            if (equipments == null || equipments.Count == 0 || equipmentCardPrefab == null) return;

            foreach (var equip in equipments)
            {
                GameObject cardObj = Instantiate(equipmentCardPrefab, equipmentGridParent);
                InventoryEquipmentCard card = cardObj.GetComponent<InventoryEquipmentCard>();
                if (card != null)
                {
                    card.Setup(equip, HandleEquipmentClicked);
                    _activeEquipmentSlots.Add(card);
                }
            }
        }

        private void HandleItemSlotClicked(ItemData item, int amount)
        {
            _selectedItem = item;
            
            detailView.SetActive(true);
            detailIcon.sprite = item.icon;
            detailNameText.text = item.itemName;
            detailDescText.text = item.description;
            detailCountText.text = $"Số lượng: {amount}";
            
            useButton.gameObject.SetActive(item.type == ItemType.Consumable);
        }

        private void HandleEquipmentClicked(EquipmentData equip)
        {
            // If the user clicks an equipment, we should show the detail panel popup!
            // First we need to instantiate or toggle it. Since EquipmentDetailPanel was built as a UIPanel...
            
            // To be precise, our UI Manager handles popups, or we might need it manually.
            // But we have Panel_EquipmentDetail in prefab. So UIManager or dynamic instance:
            
            // Wait, let's just find if UIManager has EquipmentDetail as popup, 
            // if not, we can find it in the scene since it was requested in BuildPanels.
            // For now let's just use UINotification for a quick fallback message if not fully hooked.
            
            // Update: Since EquipmentDetailPanel has .Setup(data), we can find it in UIManager if it is an overlay,
            // or we might need to rely on GameEventManager or UIManager direct call. Let's just lookup by type.
            var equipmentDetailPanel = FindObjectOfType<EquipmentDetailPanel>(true);
            if (equipmentDetailPanel != null)
            {
                equipmentDetailPanel.Setup(equip);
            }
            else
            {
                Debug.LogWarning("EquipmentDetailPanel not found in the scene to show details.");
            }
        }

        private void OnUseButtonClicked()
        {
            if (_selectedItem != null)
            {
                bool success = InventoryManager.Instance.UseItem(_selectedItem.id, 1);
                if (success)
                {
                    GameManager.Instance.UINotificationManager.ShowNotification($"Sử dụng {_selectedItem.itemName} thành công");
                }
            }
        }

        private void ClosePanel()
        {
            GameManager.Instance.UIManager.GoBack();
        }
    }
}


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
        private EquipmentData _selectedEquip;

        private bool _isPickMode = false;
        private HeroData _pickModeHero;
        private EquipmentSlot _pickModeSlot;

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
            EventManager.StartListening<EquipSlotClickData>(GameEvents.OnEquipSlotClicked, HandleEquipSlotClicked);
        }

        private void OnDisable()
        {
            InventoryManager.OnItemChanged -= HandleItemChanged;
            InventoryManager.OnEquipmentChanged -= HandleEquipmentChanged;
            EventManager.StopListening<EquipSlotClickData>(GameEvents.OnEquipSlotClicked, HandleEquipSlotClicked);
            _isPickMode = false;
        }

        private void HandleEquipSlotClicked(EquipSlotClickData data)
        {
            if (!gameObject.activeInHierarchy)
            {
                GameManager.Instance.UIManager.ShowPanel(UIPanelType.Inventory, false);
            }
            _isPickMode = true;
            _pickModeHero = data.hero;
            _pickModeSlot = data.slot;

            SwitchTab(InventoryTabType.Equipment);
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

            if (DataManager.Instance == null || DataManager.Instance.Player == null) 
                return;

            var allItemsConfigs = DataManager.Instance.AllItems;
            if (allItemsConfigs == null || allItemsConfigs.Count == 0) 
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

            if (InventoryManager.Instance == null || equipmentCardPrefab == null) return;

            var equipments = InventoryManager.Instance.GetEquipments();
            if (equipments == null || equipments.Count == 0) return;

            foreach (var equip in equipments)
            {
                if (_isPickMode)
                {
                    if (equip.slot != _pickModeSlot) continue;
                    if (equip.classRestriction != Profession.None && equip.classRestriction != _pickModeHero.profession) continue;
                }

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
            _selectedEquip = null;
            
            detailView.SetActive(true);
            detailIcon.sprite = item.icon;
            detailNameText.text = item.itemName;
            detailDescText.text = item.description;
            detailCountText.text = $"Số lượng: {amount}";
            
            useButton.gameObject.SetActive(item.type == ItemType.Consumable);
            var btnText = useButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = "Sử dụng";
        }

        private void HandleEquipmentClicked(EquipmentData equip)
        {
            _selectedEquip = equip;
            _selectedItem = null;

            detailView.SetActive(true);
            detailIcon.sprite = equip.GetIcon();
            detailNameText.text = equip.equipmentName;
            
            string desc = $"<color=#FFD700>Bậc: {equip.tier} | Cấp: {equip.level}</color>\n\n";
            if (equip.atkBonus > 0) desc += $"Sát Thương: +{equip.atkBonus}\n";
            if (equip.defBonus > 0) desc += $"Phòng Thủ: +{equip.defBonus}\n";
            if (equip.hpBonus > 0) desc += $"Sinh Lực: +{equip.hpBonus}\n";
            if (equip.spdBonus > 0) desc += $"Tốc Độ: +{equip.spdBonus}\n";
            if (equip.critChanceBonus > 0) desc += $"Tỉ Lệ Chí Mạng: +{equip.critChanceBonus * 100}%\n";
            
            detailDescText.text = desc;
            
            detailCountText.text = ""; // Trang bị thì không cần đếm số lượng
            if (useButton != null)
            {
                useButton.gameObject.SetActive(true);
                var btnText = useButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = _isPickMode ? "Mặc" : "Nâng cấp"; // Trong Pick mode là Mặc, bình thường có thể chuyển qua nâng cấp
            }
        }

        private void OnUseButtonClicked()
        {
            if (_isPickMode && _selectedEquip != null)
            {
                bool success = EquipmentSystem.EquipItem(_pickModeHero, _selectedEquip);
                if (success)
                {
                    GameManager.Instance.UINotificationManager?.ShowNotification($"Đã trang bị {_selectedEquip.equipmentName}");
                    _isPickMode = false;
                    _selectedEquip = null;
                    ClosePanel(); // Quay lại HeroInfo
                }
            }
            else if (_selectedItem != null && _selectedItem.type == ItemType.Consumable)
            {
                bool success = InventoryManager.Instance.UseItem(_selectedItem.id, 1);
                if (success)
                {
                    GameManager.Instance.UINotificationManager.ShowNotification($"Sử dụng {_selectedItem.itemName} thành công");
                }
            }
            else if (!_isPickMode && _selectedEquip != null)
            {
                // Mở EquipmentDetailPanel cho phép upgrade/lock
                EquipmentDetailPanel detailPanel = FindFirstObjectByType<EquipmentDetailPanel>(FindObjectsInactive.Include);
                if (detailPanel != null)
                {
                    detailPanel.Setup(_selectedEquip);
                }
            }
        }

        private void ClosePanel()
        {
            _isPickMode = false;
            GameManager.Instance.UIManager.GoBack();
        }
    }
}


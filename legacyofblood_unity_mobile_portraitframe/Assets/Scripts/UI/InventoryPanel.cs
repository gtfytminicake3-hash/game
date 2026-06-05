using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using LegendOfBlood.Utils;

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

        private UIListPooler<KeyValuePair<string, int>, ItemSlot> _itemPooler;
        private UIListPooler<EquipmentData, InventoryEquipmentCard> _equipPooler;

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
            
            if (detailView != null)
            {
                if (detailIcon == null) detailIcon = detailView.transform.Find("Icon")?.GetComponent<Image>() ?? detailView.GetComponentInChildren<Image>(true);
                detailView.SetActive(false);
            }

            // Initialize Poolers
            _itemPooler = new UIListPooler<KeyValuePair<string, int>, ItemSlot>(itemSlotPrefab, itemGridParent, (slot, kvp) => {
                if (DataManager.Instance.AllItems.TryGetValue(kvp.Key, out ItemData itemData))
                {
                    slot.Setup(itemData, kvp.Value);
                    slot.OnClicked += HandleItemSlotClicked;
                }
            });

            _equipPooler = new UIListPooler<EquipmentData, InventoryEquipmentCard>(equipmentCardPrefab, equipmentGridParent, (card, data) => {
                card.Setup(data, HandleEquipmentClicked);
            });
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
                
                if (itemTabButton != null) itemTabButton.GetComponent<Image>().color = Color.white;
                if (equipmentTabButton != null) equipmentTabButton.GetComponent<Image>().color = Color.gray;
                
                RefreshItems();
            }
            else
            {
                if (itemPage != null) itemPage.SetActive(false);
                if (equipmentPage != null) equipmentPage.SetActive(true);

                if (itemTabButton != null) itemTabButton.GetComponent<Image>().color = Color.gray;
                if (equipmentTabButton != null) equipmentTabButton.GetComponent<Image>().color = Color.white;
                
                RefreshEquipments();
            }
        }

        private void HandleItemChanged(string itemId, int newCount)
        {
            if (itemPage != null && itemPage.activeSelf) RefreshItems();
            
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
            if (equipmentPage != null && equipmentPage.activeSelf) RefreshEquipments();
        }

        private void RefreshItems()
        {
            if (DataManager.Instance == null || DataManager.Instance.Player == null) return;

            var itemDataList = new List<KeyValuePair<string, int>>();
            foreach (var kvp in DataManager.Instance.Player.items)
            {
                if (kvp.Value > 0) itemDataList.Add(kvp);
            }

            _itemPooler.Refresh(itemDataList);
        }

        private void RefreshEquipments()
        {
            if (InventoryManager.Instance == null || equipmentCardPrefab == null) return;

            var equipments = InventoryManager.Instance.GetEquipments();
            var filteredEquips = new List<EquipmentData>();

            foreach (var equip in equipments)
            {
                if (_isPickMode)
                {
                    if (equip.slot != _pickModeSlot) continue;
                    if (equip.classRestriction != Profession.None && equip.classRestriction != _pickModeHero.profession) continue;
                }
                filteredEquips.Add(equip);
            }

            _equipPooler.Refresh(filteredEquips);
        }

        private void HandleItemSlotClicked(ItemData item, int amount)
        {
            _selectedItem = item;
            _selectedEquip = null;
            
            detailView.SetActive(true);
            if (detailIcon != null) 
            {
                detailIcon.sprite = item.icon;
                detailIcon.enabled = true;
                detailIcon.color = Color.white;
            }
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
            if (detailIcon != null)
            {
                detailIcon.sprite = equip.GetIcon();
                detailIcon.enabled = true;
                detailIcon.color = Color.white;
            }
            detailNameText.text = equip.equipmentName;
            
            string desc = $"<color=#FFD700>Bậc: {equip.tier} | Cấp: {equip.level}</color>\n\n";
            if (equip.atkBonus > 0) desc += $"Sát Thương: +{equip.atkBonus}\n";
            if (equip.defBonus > 0) desc += $"Phòng Thủ: +{equip.defBonus}\n";
            if (equip.hpBonus > 0) desc += $"Sinh Lực: +{equip.hpBonus}\n";
            if (equip.spdBonus > 0) desc += $"Tốc Độ: +{equip.spdBonus}\n";
            if (equip.critChanceBonus > 0) desc += $"Tỉ Lệ Chí Mạng: +{equip.critChanceBonus * 100}%\n";
            
            detailDescText.text = desc;
            detailCountText.text = ""; 
            
            if (useButton != null)
            {
                useButton.gameObject.SetActive(true);
                var btnText = useButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = _isPickMode ? "Mặc" : "Nâng cấp";
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
                    ClosePanel(); 
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
                EquipmentUpgradePanel upgradePanel = FindFirstObjectByType<EquipmentUpgradePanel>(FindObjectsInactive.Include);
                if (upgradePanel != null)
                {
                    upgradePanel.Setup(_selectedEquip, RefreshEquipments);
                }
                else
                {
                    EquipmentDetailPanel detailPanel = FindFirstObjectByType<EquipmentDetailPanel>(FindObjectsInactive.Include);
                    if (detailPanel != null) detailPanel.Setup(_selectedEquip);
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

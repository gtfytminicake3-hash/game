namespace LegendOfBlood
{
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections.Generic;

    public class InventoryPanel : UIPanel
    {
        [SerializeField] private Button closeButton;
        
        [Header("Tabs")]
        [SerializeField] private Button itemTabButton;
        [SerializeField] private Button equipmentTabButton;
        [SerializeField] private Color activeTabColor = Color.white;
        [SerializeField] private Color inactiveTabColor = Color.gray;

        [Header("Item View")]
        [SerializeField] private GameObject itemContainerObj;
        [SerializeField] private Transform itemContent;
        [SerializeField] private GameObject itemCardPrefab;

        [Header("Equipment View")]
        [SerializeField] private GameObject equipmentContainerObj;
        [SerializeField] private Transform equipmentContent;
        [SerializeField] private GameObject equipmentCardPrefab;

        private enum InventoryTab { Items, Equipments }
        private InventoryTab _currentTab = InventoryTab.Items;

        private void Awake()
        {
            PanelType = UIPanelType.Inventory;
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() => GameManager.Instance.UIManager.GoBack());
            }
            else Debug.LogError("[InventoryPanel] 🚨 Close Button is NULL! Run UI Panel Part 2 generator again!");

            if (itemTabButton != null) itemTabButton.onClick.AddListener(() => SwitchTab(InventoryTab.Items));
            else Debug.LogError("[InventoryPanel] 🚨 Item Tab Button is NULL!");

            if (equipmentTabButton != null) equipmentTabButton.onClick.AddListener(() => SwitchTab(InventoryTab.Equipments));
            else Debug.LogError("[InventoryPanel] 🚨 Equipment Tab Button is NULL!");
        }

        private void OnEnable()
        {
            // Reset tab when reopened
            SwitchTab(InventoryTab.Items);
            InventoryManager.OnItemChanged += HandleItemChanged;
            InventoryManager.OnEquipmentChanged += HandleEquipmentChanged;
        }

        private void OnDisable()
        {
            InventoryManager.OnItemChanged -= HandleItemChanged;
            InventoryManager.OnEquipmentChanged -= HandleEquipmentChanged;
        }

        private void SwitchTab(InventoryTab newTab)
        {
            _currentTab = newTab;
            
            // Visual Update
            if (itemTabButton != null) itemTabButton.GetComponent<Image>().color = _currentTab == InventoryTab.Items ? activeTabColor : inactiveTabColor;
            if (equipmentTabButton != null) equipmentTabButton.GetComponent<Image>().color = _currentTab == InventoryTab.Equipments ? activeTabColor : inactiveTabColor;

            // Toggle Containers
            if (itemContainerObj != null) itemContainerObj.SetActive(_currentTab == InventoryTab.Items);
            if (equipmentContainerObj != null) equipmentContainerObj.SetActive(_currentTab == InventoryTab.Equipments);

            if (_currentTab == InventoryTab.Items)
            {
                RefreshItems();
            }
            else
            {
                RefreshEquipments();
            }
        }

        private void RefreshItems()
        {
            if (itemContent == null || itemCardPrefab == null) return;

            foreach (Transform child in itemContent) Destroy(child.gameObject);

            var items = DataManager.Instance?.Player?.items;
            if (items == null || DataManager.Instance.AllItems == null) return;

            foreach (var kvp in items)
            {
                string itemID = kvp.Key;
                int count = kvp.Value;

                if (count <= 0) continue;

                if (DataManager.Instance.AllItems.TryGetValue(itemID, out ItemData itemData))
                {
                    GameObject cardObj = Instantiate(itemCardPrefab, itemContent);
                    InventoryItemCard cardScript = cardObj.GetComponent<InventoryItemCard>();
                    if (cardScript != null)
                    {
                        cardScript.Setup(itemData, count);
                    }
                }
            }
        }

        private void RefreshEquipments()
        {
            if (equipmentContent == null || equipmentCardPrefab == null) return;

            foreach (Transform child in equipmentContent) Destroy(child.gameObject);

            var equipments = DataManager.Instance?.Player?.equipments;
            if (equipments == null) return;

            foreach (var eq in equipments)
            {
                GameObject cardObj = Instantiate(equipmentCardPrefab, equipmentContent);
                InventoryEquipmentCard cardScript = cardObj.GetComponent<InventoryEquipmentCard>();
                if (cardScript != null)
                {
                    cardScript.Setup(eq, OnEquipmentClicked);
                }
            }
        }

        private void OnEquipmentClicked(EquipmentData data)
        {
            Debug.Log($"Clicked Equipment: {data.equipmentName} - Lvl {data.level}");
            // Optional: You can link the Panel Prefab here and show it
            var detailPanel = GameManager.Instance.UIManager.GetPanel<EquipmentDetailPanel>(UIPanelType.None); // Just a generic fetch, this would be set as a true panel Type later
            // For now, if someone drags the EquipmentDetailPanel onto the scene and calls Setup it will work.
        }

        private void HandleItemChanged(string itemId, int newCount)
        {
            if (_currentTab == InventoryTab.Items) RefreshItems();
        }
        
        private void HandleEquipmentChanged()
        {
            if (_currentTab == InventoryTab.Equipments) RefreshEquipments();
        }
    }
}

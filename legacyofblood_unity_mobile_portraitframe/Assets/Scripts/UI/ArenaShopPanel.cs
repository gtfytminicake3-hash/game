using UnityEngine;
using UnityEngine.UI;

namespace LegendOfBlood
{
    [RequireComponent(typeof(UIPanel))]
    public class ArenaShopPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Transform itemContainer; // To hold the shop items
        [SerializeField] private GameObject shopItemPrefab; // Prefab for a single shop item

        private void Awake()
        {
            closeButton.onClick.AddListener(() => GameManager.Instance.UIManager.GoBack());
        }

        private void OnEnable()
        {
            RefreshShop();
        }

        private void RefreshShop()
        {
            // Clear existing items
            foreach (Transform child in itemContainer)
            {
                Destroy(child.gameObject);
            }

            // Populate with new items (mock data for now)
            // In a real implementation, this would come from a config file or ScriptableObject
            CreateShopItem("Thuốc Sức mạnh", 5000);
            CreateShopItem("Sách Khai phá", 10000);
            CreateShopItem("Đồng hồ Cát (1 giờ)", 200);
        }

        private void CreateShopItem(string itemName, int price)
        { 
            if (shopItemPrefab == null) return;
            GameObject itemGO = Instantiate(shopItemPrefab, itemContainer);
            ArenaShopItem item = itemGO.GetComponent<ArenaShopItem>();
            if (item != null)
            {
                item.Setup(itemName, price);
            }
            Debug.Log($"Created shop item: {itemName} for {price} Arena Coins.");
        }
    }
}

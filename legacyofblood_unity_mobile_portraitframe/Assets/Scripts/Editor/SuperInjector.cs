using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif

public class SuperInjectorV2
{
    [MenuItem("🔥 INJECT INVENTORY (BẤM VÀO ĐÂY) 🔥/🚀 Thực hiện gắn 30 món đồ", false, 1)]
    public static void InjectLive()
    {
        GameObject panelInventory = GameObject.Find("Panel_Inventory");
        if (panelInventory == null)
        {
            Debug.LogError("[LỖI] Không tìm thấy Panel_Inventory trong Hierarchy!");
            return;
        }

        ScrollRect scrollRect = panelInventory.GetComponentInChildren<ScrollRect>(true);
        if (scrollRect == null || scrollRect.content == null)
        {
            Debug.LogError("[LỖI] Panel_Inventory không có ScrollRect hoặc Content!");
            return;
        }

        Transform grid = scrollRect.content;
        GridLayoutGroup glg = grid.GetComponent<GridLayoutGroup>();
        if (glg == null) glg = grid.gameObject.AddComponent<GridLayoutGroup>();

        glg.cellSize = new Vector2(150, 150);
        glg.spacing = new Vector2(25, 25);
        glg.padding = new RectOffset(20, 20, 20, 20);
        
        int childCount = grid.childCount;
        for (int i = childCount - 1; i >= 0; i--) {
            Object.DestroyImmediate(grid.GetChild(i).gameObject);
        }
        
        Sprite frameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Items/item_slot_frame.png");
        string[] itemNames = {
            "item_mutation_potion", "item_wish_charm", "item_speed_hourglass",
            "equip_ancient_weapon", "equip_ancient_armor"
        };
        
        for (int i = 0; i < 30; i++) {
            GameObject slot = new GameObject("Slot_" + i);
            slot.transform.SetParent(grid, false);
            
            Image slotImg = slot.AddComponent<Image>();
            slotImg.sprite = frameSprite;
            slotImg.type = Image.Type.Sliced;
            
            // GẮN SCRIPT NGAY BÂY GIỜ
            Button btn = slot.AddComponent<Button>(); // Nút bấm
            InventorySlotUI slotLogic = slot.AddComponent<InventorySlotUI>(); // Logic hiện thông tin
            
            if (Random.value > 0.3f) { // 70% có đồ
                GameObject itemIcon = new GameObject("ItemIcon");
                itemIcon.transform.SetParent(slot.transform, false);
                
                RectTransform rt = itemIcon.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.1f, 0.1f);
                rt.anchorMax = new Vector2(0.9f, 0.9f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                
                Image iconImg = itemIcon.AddComponent<Image>();
                string randItem = itemNames[Random.Range(0, itemNames.Length)];
                iconImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Items/" + randItem + ".png");
                iconImg.preserveAspect = true;
                
                // --- ĐỔ DỮ LIỆU GIẢ LẬP VÀO SCRIPT ---
                slotLogic.itemIcon = iconImg.sprite;
                
                if (randItem == "item_mutation_potion") {
                    slotLogic.itemName = "Thuốc Biến Dị";
                    slotLogic.itemDesc = "Một loại huyết thanh kì bí sủi bọt xanh. Có khả năng kích phát đột biến gen, cung cấp exp Khổng khồ cho các Hero.";
                } else if (randItem == "item_wish_charm") {
                    slotLogic.itemName = "Bùa Ước Nguyện";
                    slotLogic.itemDesc = "Tấm bùa rách nát cổ xưa, phát ra một thứ ánh sáng ma mị. Dùng để mở khóa chức năng Breeding triệu hồi chiến binh.";
                } else if (randItem == "item_speed_hourglass") {
                    slotLogic.itemName = "Đồng Hồ Cát";
                    slotLogic.itemDesc = "Hạt cát bên trong chảy lướt qua thời không. Dùng để gia tốc trứng nở hoặc rút ngắn thời gian thám hiểm của Expedition.";
                } else if (randItem == "equip_ancient_weapon") {
                    slotLogic.itemName = "Kiếm Cổ Thần";
                    slotLogic.itemDesc = "Thanh kiếm rỉ sét mang quyền năng Thần thoại. Tăng Sát thương Vật Lý (ATK) đột biến khi trang bị lên người Hero.";
                } else if (randItem == "equip_ancient_armor") {
                    slotLogic.itemName = "Giáp Rồng Xương";
                    slotLogic.itemDesc = "Tấm áo choàng dệt từ thần kinh của Cổ Long. Tăng cường cực hạn sức phòng thủ (DEF) và sinh tồn cho người mặc.";
                }
            }
            else {
                // Ô trống
                slotLogic.itemName = "Ô trống";
                slotLogic.itemDesc = "Không có vật phẩm nào ở đây cả.";
            }
        }
        
        ContentSizeFitter csf = grid.GetComponent<ContentSizeFitter>();
        if (csf == null) csf = grid.gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        #if UNITY_2021_2_OR_NEWER
        PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
        #else
        PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
        #endif
        
        if (stage != null) {
            EditorSceneManager.MarkSceneDirty(stage.scene);
        } else {
            UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);
        }
        
        Debug.Log("<color=cyan>XONG! Đã bơm 30 ô vuông kèm thông số chi tiết ảo diệu!</color>");
    }
}

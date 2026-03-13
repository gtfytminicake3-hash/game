using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using LegendOfBlood;

public class InventoryPrefabFixer
{
    [MenuItem("Tools/Sửa lỗi Inventory Prefab")]
    public static void FixInventoryPrefab()
    {
        string path = "Assets/Prefabs/Panel_Inventory.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        
        if (prefab == null)
        {
            Debug.LogError("Không tìm thấy Panel_Inventory.prefab ở Assets/Prefabs/");
            // Try looking in root Assets
            path = "Assets/Panel_Inventory.prefab";
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError("Cũng không tìm thấy Panel_Inventory.prefab ở Assets/");
                return;
            }
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        InventoryPanel script = instance.GetComponent<InventoryPanel>();
        if (script == null)
        {
            Debug.LogError("Prefab không có script InventoryPanel");
            Object.DestroyImmediate(instance);
            return;
        }

        // ==========================================
        // KHÔI PHỤC HỆ THỐNG TRANG TRÊN FILE PREFAB
        // ==========================================

        // 1. Phục hồi Tab Group nếu mất / hoặc chưa nối
        Transform tabGroup = instance.transform.Find("TabGroup");
        if (tabGroup == null)
        {
            GameObject tabGroupObj = new GameObject("TabGroup", typeof(RectTransform));
            tabGroupObj.transform.SetParent(instance.transform, false);
            RectTransform tabRect = tabGroupObj.GetComponent<RectTransform>();
            tabRect.anchorMin = new Vector2(0, 1); tabRect.anchorMax = new Vector2(1, 1);
            tabRect.pivot = new Vector2(0.5f, 1); tabRect.sizeDelta = new Vector2(0, 100);
            var tabHlg = tabGroupObj.AddComponent<HorizontalLayoutGroup>();
            tabHlg.childControlWidth = true; tabHlg.childForceExpandWidth = true;
            tabGroup = tabGroupObj.transform;

            CreateButton("Btn_ItemTab", tabGroup, "Items");
            CreateButton("Btn_EquipmentTab", tabGroup, "Equipments");
        }

        Transform btnItem = tabGroup.Find("Btn_ItemTab");
        Transform btnEquip = tabGroup.Find("Btn_EquipmentTab");

        // Gán tham chiếu Tab Button
        AssignPrivateField(script, "itemTabButton", btnItem.GetComponent<Button>());
        AssignPrivateField(script, "equipmentTabButton", btnEquip.GetComponent<Button>());

        // 2. Phục hồi 2 Trang (Pages)
        Transform pageItems = instance.transform.Find("Page_Items");
        if (pageItems == null)
        {
            GameObject itemContainerGO = CreateEmptyStretch("Page_Items", instance.transform);
            pageItems = itemContainerGO.transform;
        }

        Transform pageEquipments = instance.transform.Find("Page_Equipments");
        if (pageEquipments == null)
        {
            GameObject equipContainerGO = CreateEmptyStretch("Page_Equipments", instance.transform);
            pageEquipments = equipContainerGO.transform;
        }

        // 3. Phục hồi danh sách Item (nếu mất) và Cập nhật tham chiếu cho Grid
        Transform itemScrollView = pageItems.Find("Scroll View");
        if (itemScrollView == null)
        {
            RectTransform tempItemContent;
            CreateScrollView("Scroll View", pageItems, out tempItemContent);
            itemScrollView = pageItems.Find("Scroll View");
        }
        Transform actualItemContent = itemScrollView.Find("Viewport/Content");
        if (actualItemContent.GetComponent<GridLayoutGroup>() == null)
        {
            AddGrid(actualItemContent.gameObject, 200, 200);
        }

        // 4. Phục hồi danh sách Equipment (nếu mất) và Cập nhật tham chiếu
        Transform equipScrollView = pageEquipments.Find("Scroll View");
        if (equipScrollView == null)
        {
            RectTransform tempEquipContent;
            CreateScrollView("Scroll View", pageEquipments, out tempEquipContent);
            equipScrollView = pageEquipments.Find("Scroll View");
        }
        Transform actualEquipContent = equipScrollView.Find("Viewport/Content");
        if (actualEquipContent.GetComponent<GridLayoutGroup>() == null)
        {
            // Equipment card chữ nhật dọc
            AddGrid(actualEquipContent.gameObject, 180, 250);
        }

        // 5. Gán Pages & Grid Parent
        AssignPrivateField(script, "itemPage", pageItems.gameObject);
        AssignPrivateField(script, "equipmentPage", pageEquipments.gameObject);
        AssignPrivateField(script, "itemGridParent", actualItemContent);
        AssignPrivateField(script, "equipmentGridParent", actualEquipContent);

        // 6. Gán các tham chiếu tự nhiên còn bị thiếu
        string equipPrefabPath = "Assets/Prefabs/InventoryEquipmentCard_Prefab.prefab";
        GameObject equipPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(equipPrefabPath);
        if (equipPrefab == null) equipPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/InventoryEquipmentCard_Prefab.prefab");
        
        string itemSlotPath = "Assets/Prefabs/InventoryItemCard_Prefab.prefab"; 
        GameObject itemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(itemSlotPath);
        if (itemPrefab == null) itemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/InventoryItemCard_Prefab.prefab");

        AssignPrivateField(script, "equipmentCardPrefab", equipPrefab);
        AssignPrivateField(script, "itemSlotPrefab", itemPrefab);

        // 7. Lưu lại Prefab
        PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);

        Debug.Log("<color=green>Sửa Prefab Panel_Inventory thành công! Đã nối hai Tab và Grid vào InventoryPanel.</color>");
    }

    static void AddGrid(GameObject go, float w, float h)
    {
        var grid = go.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(w, h);
        grid.spacing = new Vector2(20, 20);
        grid.padding = new RectOffset(20, 20, 20, 20);
        
        var csf = go.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    static GameObject CreateEmptyStretch(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return go;
    }

    static Button CreateButton(string name, Transform parent, string btnText)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 80);
        go.GetComponent<Image>().color = Color.gray; 
        Button btn = go.AddComponent<Button>();

        GameObject txtGO = new GameObject("Text (TMP)", typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
        txtGO.transform.SetParent(go.transform, false);
        var tmp = txtGO.GetComponent<TMPro.TextMeshProUGUI>();
        tmp.text = btnText;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = Color.black;
        return btn;
    }

    static void CreateScrollView(string name, Transform parent, out RectTransform contentRect)
    {
        GameObject sv = CreateEmptyStretch(name, parent);
        ScrollRect scrollRect = sv.AddComponent<ScrollRect>();
        scrollRect.horizontal = false; 

        GameObject viewport = CreateEmptyStretch("Viewport", sv.transform);
        viewport.AddComponent<Image>().raycastTarget = false;
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(CanvasRenderer));
        content.transform.SetParent(viewport.transform, false);
        contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0);

        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.content = contentRect;
    }

    static void AssignPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(target, value);
        }
        else
        {
            Debug.LogError($"Field {fieldName} not found in {target.GetType()}");
        }
    }
}

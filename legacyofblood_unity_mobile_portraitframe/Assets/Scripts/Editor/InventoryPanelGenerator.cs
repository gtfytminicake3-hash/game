#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryPanelGenerator : EditorWindow
{
    [MenuItem("UI Tools/Generate Inventory Panel")]
    public static void GenerateUI()
    {
        // 0. Cleanup
        var oldPanels = Object.FindObjectsByType<LegendOfBlood.InventoryPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in oldPanels) {
            Undo.DestroyObjectImmediate(p.gameObject);
        }

        GameObject canvasObj = new GameObject("Panel_Inventory");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        canvasObj.AddComponent<GraphicRaycaster>();

        // 1. Root
        GameObject screenRoot = CreateUIElement("InventoryRoot", canvasObj.transform);
        SetAnchor(screenRoot, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(screenRoot, 0, 0, 0, 0);
        if (System.Type.GetType("SafeArea") != null) {
            screenRoot.AddComponent(System.Type.GetType("SafeArea"));
        }

        GameObject bg = CreateUIElement("BackgroundScene", screenRoot.transform);
        SetAnchor(bg, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(bg, 0, 0, 0, 0);
        bg.AddComponent<Image>().color = new Color(0.12f, 0.1f, 0.15f, 1f);

        // 2. TopTabBar
        GameObject topTabBar = CreateUIElement("TopTabBar", screenRoot.transform);
        SetAnchor(topTabBar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        SetOffsets(topTabBar, 92, 111, 97, -339); // 242 height -> bottom -339

        GameObject tabItemsObj = CreateUIElement("Tab_Items", topTabBar.transform);
        SetAnchor(tabItemsObj, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
        tabItemsObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(9, -5);
        tabItemsObj.GetComponent<RectTransform>().sizeDelta = new Vector2(488, 194);
        tabItemsObj.AddComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f, 1f); // Selected
        Button tabItemsBtn = tabItemsObj.AddComponent<Button>();
        AddTMPText(tabItemsObj, "ITEMS", 40).GetComponent<TextMeshProUGUI>().color = Color.black;

        GameObject tabEquipmentObj = CreateUIElement("Tab_Equipment", topTabBar.transform);
        SetAnchor(tabEquipmentObj, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));
        tabEquipmentObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-30, -5);
        tabEquipmentObj.GetComponent<RectTransform>().sizeDelta = new Vector2(348, 194);
        tabEquipmentObj.AddComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f, 1f); // Unselected
        Button tabEquipBtn = tabEquipmentObj.AddComponent<Button>();
        AddTMPText(tabEquipmentObj, "EQUIP", 40);

        // 3. MainContent
        GameObject mainContent = CreateUIElement("MainContent", screenRoot.transform);
        SetAnchor(mainContent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(mainContent, 10, 10, 348, 182);

        // -- Item Page --
        GameObject itemPage = CreateUIElement("ItemPage", mainContent.transform);
        SetAnchor(itemPage, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(itemPage, 0, 0, 0, 0);

        GameObject leftInvPanel = CreateUIElement("LeftInventoryPanel", itemPage.transform);
        SetAnchor(leftInvPanel, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f));
        leftInvPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(14, -16);
        leftInvPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(601, 1328);
        leftInvPanel.AddComponent<Image>().color = new Color(0,0,0,0.5f);
        
        ScrollRect srL = leftInvPanel.AddComponent<ScrollRect>();
        srL.horizontal = false; srL.vertical = true;
        GameObject viewportL = CreateUIElement("ItemViewport", leftInvPanel.transform);
        SetAnchor(viewportL, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(viewportL, 8, 71, 11, 24);
        viewportL.AddComponent<RectMask2D>();
        srL.viewport = viewportL.GetComponent<RectTransform>();

        GameObject scrollBarTrack = CreateUIElement("LeftScrollbar", leftInvPanel.transform);
        SetAnchor(scrollBarTrack, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f));
        scrollBarTrack.GetComponent<RectTransform>().anchoredPosition = new Vector2(-6, 0);
        scrollBarTrack.GetComponent<RectTransform>().sizeDelta = new Vector2(35, 1267);
        scrollBarTrack.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);

        GameObject itemGridContent = CreateUIElement("ItemGrid", viewportL.transform);
        SetAnchor(itemGridContent, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        SetOffsets(itemGridContent, 0, 0, 0, 0);
        srL.content = itemGridContent.GetComponent<RectTransform>();

        GridLayoutGroup itemGridLayout = itemGridContent.AddComponent<GridLayoutGroup>();
        itemGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        itemGridLayout.constraintCount = 4;
        itemGridLayout.cellSize = new Vector2(118, 118);
        itemGridLayout.spacing = new Vector2(13, 14);
        itemGridLayout.padding = new RectOffset(3, 0, 1, 0);
        
        ContentSizeFitter csfItems = itemGridContent.AddComponent<ContentSizeFitter>();
        csfItems.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // -- Equipment Page --
        GameObject equipPage = CreateUIElement("EquipmentPage", mainContent.transform);
        SetAnchor(equipPage, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(equipPage, 0, 0, 0, 0);
        equipPage.SetActive(false); // Default hide

        GameObject leftEqpPanel = CreateUIElement("LeftEquipmentPanel", equipPage.transform);
        SetAnchor(leftEqpPanel, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f));
        leftEqpPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(14, -16);
        leftEqpPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(601, 1328);
        leftEqpPanel.AddComponent<Image>().color = new Color(0,0,0,0.5f);

        ScrollRect srE = leftEqpPanel.AddComponent<ScrollRect>();
        srE.horizontal = false; srE.vertical = true;
        GameObject viewportE = CreateUIElement("EquipmentViewport", leftEqpPanel.transform);
        SetAnchor(viewportE, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(viewportE, 8, 71, 11, 24);
        viewportE.AddComponent<RectMask2D>();
        srE.viewport = viewportE.GetComponent<RectTransform>();

        GameObject eqpGridContent = CreateUIElement("EquipmentList", viewportE.transform);
        SetAnchor(eqpGridContent, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        SetOffsets(eqpGridContent, 0, 0, 0, 0);
        srE.content = eqpGridContent.GetComponent<RectTransform>();

        VerticalLayoutGroup vlgEqp = eqpGridContent.AddComponent<VerticalLayoutGroup>();
        vlgEqp.spacing = 15;
        vlgEqp.childAlignment = TextAnchor.UpperCenter;
        vlgEqp.childControlWidth = true; vlgEqp.childControlHeight = true;
        vlgEqp.childForceExpandWidth = false; vlgEqp.childForceExpandHeight = false;
        
        ContentSizeFitter csfEqp = eqpGridContent.AddComponent<ContentSizeFitter>();
        csfEqp.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 4. RightDetailPanel (Nằm trong mainContent để 2 page dùng chung hoặc tắt mở)
        GameObject rightDetailPanel = CreateUIElement("RightDetailPanel", mainContent.transform);
        SetAnchor(rightDetailPanel, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f));
        rightDetailPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-54, -14);
        rightDetailPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(355, 1320);
        rightDetailPanel.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        GameObject detailHeader = CreateUIElement("DetailHeader", rightDetailPanel.transform);
        SetAnchor(detailHeader, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        detailHeader.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -4);
        detailHeader.GetComponent<RectTransform>().sizeDelta = new Vector2(333, 92);
        detailHeader.AddComponent<Image>().color = new Color(0.3f, 0.2f, 0.1f, 1f);
        AddTMPText(detailHeader, "DETAIL", 30);

        GameObject detailIconFrame = CreateUIElement("DetailIconFrame", rightDetailPanel.transform);
        SetAnchor(detailIconFrame, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        detailIconFrame.GetComponent<RectTransform>().anchoredPosition = new Vector2(-6, -107);
        detailIconFrame.GetComponent<RectTransform>().sizeDelta = new Vector2(223, 274);
        detailIconFrame.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        GameObject detailIcon = CreateUIElement("ItemIcon", detailIconFrame.transform);
        SetAnchor(detailIcon, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(detailIcon, 10, 10, 10, 10);
        Image detailIconImg = detailIcon.AddComponent<Image>();
        detailIconImg.color = new Color(0.8f, 0.8f, 0.8f, 1f);

        GameObject itemNameTextObj = CreateUIElement("ItemNameText", rightDetailPanel.transform);
        SetAnchor(itemNameTextObj, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        itemNameTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-5, -382);
        itemNameTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(229, 122);
        TextMeshProUGUI tmpName = AddTMPText(itemNameTextObj, "Item Name", 40).GetComponent<TextMeshProUGUI>();

        GameObject qtyTextObj = CreateUIElement("QuantityText", rightDetailPanel.transform);
        SetAnchor(qtyTextObj, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        qtyTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -501);
        qtyTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 49);
        TextMeshProUGUI tmpQty = AddTMPText(qtyTextObj, "x 999", 26).GetComponent<TextMeshProUGUI>();
        tmpQty.color = new Color(0.5f, 0.8f, 1f, 1f);

        GameObject descPanel = CreateUIElement("DescriptionPanel", rightDetailPanel.transform);
        SetAnchor(descPanel, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        descPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-11, -562);
        descPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(242, 434);
        TextMeshProUGUI tmpDesc = AddTMPText(descPanel, "Item Description here...", 22).GetComponent<TextMeshProUGUI>();
        tmpDesc.alignment = TextAlignmentOptions.TopLeft;

        GameObject useBtnObj = CreateUIElement("UseButton", rightDetailPanel.transform);
        SetAnchor(useBtnObj, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        useBtnObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(12, 100);
        useBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(164, 169);
        useBtnObj.AddComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f, 1f);
        Button useBtn = useBtnObj.AddComponent<Button>();
        AddTMPText(useBtnObj, "USE", 30);

        // 5. BottomNavBar (Mock just for Close button logic from Lobby Tab)
        GameObject bottomNavBar = CreateUIElement("BottomNavBar", screenRoot.transform);
        SetAnchor(bottomNavBar, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
        SetOffsets(bottomNavBar, 0, 0, -179, 0);
        bottomNavBar.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.05f, 1f);

        GameObject tabLobby = CreateUIElement("Tab_Lobby", bottomNavBar.transform);
        SetAnchor(tabLobby, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        tabLobby.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 80);
        tabLobby.GetComponent<RectTransform>().sizeDelta = new Vector2(259, 183);
        tabLobby.AddComponent<Image>().color = new Color(0.8f, 0.4f, 0.2f, 1f);
        Button lobbyBtnComp = tabLobby.AddComponent<Button>();
        AddTMPText(tabLobby, "HOME", 32);

        // --- Prefabs Generation ---
        
        // Items
        GameObject tItem = CreateItemSlotPrefab("InventorySlot_Template", itemGridContent.transform);
        string iPath = "Assets/Prefabs/InventorySlot_Auto.prefab";
        if (!System.IO.Directory.Exists("Assets/Prefabs")) System.IO.Directory.CreateDirectory("Assets/Prefabs");
        GameObject pItem = PrefabUtility.SaveAsPrefabAsset(tItem, iPath);
        for(int i=0; i<15; i++) {
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(pItem, itemGridContent.transform);
            inst.name = "Slot_" + i;
        }
        Undo.DestroyObjectImmediate(tItem);

        // Equipments
        GameObject tEquip = CreateEquipmentCardPrefab("EquipmentCard_Template", eqpGridContent.transform);
        string ePath = "Assets/Prefabs/InventoryEquipmentCard_Auto.prefab";
        GameObject pEquip = PrefabUtility.SaveAsPrefabAsset(tEquip, ePath);
        for(int i=0; i<5; i++) {
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(pEquip, eqpGridContent.transform);
            inst.name = "Equip_" + i;
        }
        Undo.DestroyObjectImmediate(tEquip);


        // --- Logic Mapping ---
        LegendOfBlood.UIPanel uiPan = canvasObj.AddComponent<LegendOfBlood.InventoryPanel>();
        uiPan.PanelType = LegendOfBlood.UIPanelType.Inventory;
        LegendOfBlood.InventoryPanel invScript = uiPan as LegendOfBlood.InventoryPanel;

        SerializedObject so = new SerializedObject(invScript);
        so.Update();

        so.FindProperty("itemTabButton").objectReferenceValue = tabItemsBtn;
        so.FindProperty("equipmentTabButton").objectReferenceValue = tabEquipBtn;
        so.FindProperty("itemPage").objectReferenceValue = itemPage;
        so.FindProperty("equipmentPage").objectReferenceValue = equipPage;
        so.FindProperty("itemGridParent").objectReferenceValue = itemGridContent.transform;
        so.FindProperty("itemSlotPrefab").objectReferenceValue = pItem;
        so.FindProperty("equipmentGridParent").objectReferenceValue = eqpGridContent.transform;
        so.FindProperty("equipmentCardPrefab").objectReferenceValue = pEquip;

        so.FindProperty("detailView").objectReferenceValue = rightDetailPanel;
        so.FindProperty("detailIcon").objectReferenceValue = detailIconImg;
        so.FindProperty("detailNameText").objectReferenceValue = tmpName;
        so.FindProperty("detailDescText").objectReferenceValue = tmpDesc;
        so.FindProperty("detailCountText").objectReferenceValue = tmpQty;
        so.FindProperty("useButton").objectReferenceValue = useBtn;
        
        so.FindProperty("closeButton").objectReferenceValue = lobbyBtnComp; // Xài nút Lobby làm back bơi nó về Home

        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(canvasObj, "Generate Inventory Panel");
        Selection.activeGameObject = canvasObj;
        Debug.Log("Inventory Panel Generated Succesfully!");
    }

    private static GameObject CreateItemSlotPrefab(string name, Transform parent) {
        GameObject slot = CreateUIElement(name, parent);
        LayoutElement le = slot.AddComponent<LayoutElement>();
        le.preferredWidth = 119;
        le.preferredHeight = 119;

        LegendOfBlood.ItemSlot logic = slot.AddComponent<LegendOfBlood.ItemSlot>();
        SerializedObject so = new SerializedObject(logic);

        GameObject frame = CreateUIElement("SlotFrame", slot.transform);
        SetAnchor(frame, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(frame, 0, 0, 0, 0);
        frame.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);

        GameObject icon = CreateUIElement("ItemIcon", slot.transform);
        SetAnchor(icon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        icon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 2);
        icon.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 78);
        Image iconImg = icon.AddComponent<Image>();
        iconImg.color = Color.white;
        so.FindProperty("iconImage").objectReferenceValue = iconImg;

        GameObject count = CreateUIElement("CountText", slot.transform);
        SetAnchor(count, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0));
        count.GetComponent<RectTransform>().anchoredPosition = new Vector2(-12, 16);
        count.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 24);
        GameObject cTmp = AddTMPText(count, "99", 20);
        cTmp.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.BottomRight;
        so.FindProperty("amountText").objectReferenceValue = cTmp.GetComponent<TextMeshProUGUI>();

        Button cBtn = slot.AddComponent<Button>();
        so.FindProperty("clickButton").objectReferenceValue = cBtn;

        so.ApplyModifiedProperties();
        return slot;
    }

    private static GameObject CreateEquipmentCardPrefab(string name, Transform parent) {
        GameObject card = CreateUIElement(name, parent);
        LayoutElement le = card.AddComponent<LayoutElement>();
        le.preferredWidth = 500;
        le.preferredHeight = 120;
        card.AddComponent<Image>().color = new Color(0.2f, 0.25f, 0.2f, 1f);

        LegendOfBlood.InventoryEquipmentCard logic = card.AddComponent<LegendOfBlood.InventoryEquipmentCard>();
        SerializedObject so = new SerializedObject(logic);

        GameObject icon = CreateUIElement("IconFrame", card.transform);
        SetAnchor(icon, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        icon.GetComponent<RectTransform>().anchoredPosition = new Vector2(10, 0);
        icon.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        Image border = icon.AddComponent<Image>();
        border.color = Color.gray; // Rarity border
        so.FindProperty("rarityBorder").objectReferenceValue = border;

        GameObject iconImgObj = CreateUIElement("IconImage", icon.transform);
        SetAnchor(iconImgObj, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(iconImgObj, 5, 5, 5, 5);
        Image iconImg = iconImgObj.AddComponent<Image>();
        iconImg.color = Color.white;
        so.FindProperty("iconImage").objectReferenceValue = iconImg;

        GameObject lvTextObj = CreateUIElement("Level", icon.transform);
        SetAnchor(lvTextObj, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        lvTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 15);
        lvTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(90, 30);
        GameObject lvTmp = AddTMPText(lvTextObj, "Lv.50", 20);
        so.FindProperty("levelText").objectReferenceValue = lvTmp.GetComponent<TextMeshProUGUI>();

        GameObject nameObj = CreateUIElement("NameText", card.transform);
        SetAnchor(nameObj, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1));
        SetOffsets(nameObj, 120, 10, 10, -40); // Height 30
        GameObject nTmp = AddTMPText(nameObj, "Legendary Sword", 24);
        nTmp.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        so.FindProperty("nameText").objectReferenceValue = nTmp.GetComponent<TextMeshProUGUI>();

        GameObject mainStatObj = CreateUIElement("MainStat", card.transform);
        SetAnchor(mainStatObj, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1));
        SetOffsets(mainStatObj, 120, 10, 45, -75);
        GameObject mTmp = AddTMPText(mainStatObj, "ATK: +500", 22);
        mTmp.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        mTmp.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0.2f, 1f);
        so.FindProperty("mainStatText").objectReferenceValue = mTmp.GetComponent<TextMeshProUGUI>();

        GameObject subStatObj = CreateUIElement("SubStats", card.transform);
        SetAnchor(subStatObj, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1));
        SetOffsets(subStatObj, 120, 10, 75, -115);
        GameObject sTmp = AddTMPText(subStatObj, "CRIT: +10% \nSPD: +5", 18);
        sTmp.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.TopLeft;
        sTmp.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.7f, 0.7f, 1f);
        so.FindProperty("subStatsText").objectReferenceValue = sTmp.GetComponent<TextMeshProUGUI>();

        Button cBtn = card.AddComponent<Button>();
        so.FindProperty("clickButton").objectReferenceValue = cBtn;

        so.ApplyModifiedProperties();
        return card;
    }

    private static GameObject CreateUIElement(string name, Transform parent) {
        GameObject go = new GameObject(name);
        go.AddComponent<RectTransform>();
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void SetAnchor(GameObject go, Vector2 min, Vector2 max, Vector2 pivot) {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.pivot = pivot;
    }

    private static void SetOffsets(GameObject go, float left, float right, float top, float bottom) {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }

    private static GameObject AddTMPText(GameObject parent, string text, int fontSize) {
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(parent.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        SetAnchor(textObj, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(textObj, 0, 0, 0, 0);
        return textObj;
    }
}
#endif


#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArenaShopPanelGenerator : EditorWindow
{
    [MenuItem("UI Tools/Generate Arena Shop Panel")]
    public static void GenerateUI()
    {
        // 0. Auto-Cleanup Old UI
        var oldPanels = Object.FindObjectsByType<LegendOfBlood.ArenaShopPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in oldPanels) {
            Undo.DestroyObjectImmediate(p.gameObject);
        }

        GameObject canvasObj = new GameObject("ArenaShop_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // 5. ScreenFrame
        GameObject screenFrame = CreateUIElement("ScreenFrame", canvasObj.transform);
        SetAnchor(screenFrame, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(screenFrame, 0, 0, 0, 280); // Chừa 280px đáy cho BottomNav chung
        if (System.Type.GetType("SafeArea") != null) {
            screenFrame.AddComponent(System.Type.GetType("SafeArea"));
        }
        screenFrame.AddComponent<Image>().color = new Color(0.1f, 0.05f, 0.05f, 1f); // Viền gỗ

        GameObject innerParchment = CreateUIElement("InnerParchment", screenFrame.transform);
        SetAnchor(innerParchment, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(innerParchment, 31, 32, 40, 227); // Right: 32, Top: 40, Bottom: 227 -> wait, bottom inside offset of frame. 
        innerParchment.AddComponent<Image>().color = new Color(0.9f, 0.85f, 0.7f, 1f); // Màu giấy

        // 6. TopTitleBanner
        GameObject topTitleBanner = CreateUIElement("TopTitleBanner", innerParchment.transform);
        SetAnchor(topTitleBanner, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        topTitleBanner.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -40);
        topTitleBanner.GetComponent<RectTransform>().sizeDelta = new Vector2(661, 132);
        topTitleBanner.AddComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);

        GameObject titleTextArea = AddTMPText(topTitleBanner, "ARENA SHOP", 40);
        titleTextArea.name = "TitleTextArea";
        SetAnchorCenter(titleTextArea, new Vector2(0, -6), new Vector2(397, 52));

        // Nút Close góc trên phải
        GameObject closeBtn = CreateUIElement("CloseButton", innerParchment.transform);
        SetAnchor(closeBtn, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));
        closeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-20, -20);
        closeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);
        closeBtn.AddComponent<Image>().color = new Color(0.8f, 0.1f, 0.1f, 1f);
        Button closeBtnComp = closeBtn.AddComponent<Button>();
        AddTMPText(closeBtn, "X", 40);

        // Nút Ad Freebie (Bắt buộc ánh xạ trong code, để tạm ở tiêu đề)
        GameObject adBtn = CreateUIElement("AdFreebieButton", innerParchment.transform);
        SetAnchor(adBtn, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
        adBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(20, -20);
        adBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 80);
        adBtn.AddComponent<Image>().color = new Color(0.1f, 0.8f, 0.1f, 1f);
        Button adBtnComp = adBtn.AddComponent<Button>();
        AddTMPText(adBtn, "AD", 30);

        // 7. CurrencyRow
        GameObject currencyRow = CreateUIElement("CurrencyRow", innerParchment.transform);
        SetAnchor(currencyRow, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        currencyRow.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -186);
        currencyRow.GetComponent<RectTransform>().sizeDelta = new Vector2(858, 108);

        GameObject goldBlock = CreateUIElement("GoldBlock", currencyRow.transform);
        SetAnchor(goldBlock, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        goldBlock.GetComponent<RectTransform>().anchoredPosition = new Vector2(6, 0); // User specs X: 6
        goldBlock.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 108);
        goldBlock.AddComponent<Image>().color = new Color(0.8f, 0.7f, 0.2f, 1f);

        GameObject trophyBlock = CreateUIElement("TrophyBlock", currencyRow.transform);
        SetAnchorCenter(trophyBlock, new Vector2(-18, 0), new Vector2(224, 108));
        trophyBlock.AddComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f, 1f);

        GameObject gemBlock = CreateUIElement("GemBlock", currencyRow.transform);
        SetAnchor(gemBlock, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
        gemBlock.GetComponent<RectTransform>().anchoredPosition = new Vector2(-44, 0);
        gemBlock.GetComponent<RectTransform>().sizeDelta = new Vector2(176, 108);
        gemBlock.AddComponent<Image>().color = new Color(0.2f, 0.5f, 1f, 1f);

        // 8. ShopGridPanel
        GameObject shopGridPanel = CreateUIElement("ShopGridPanel", innerParchment.transform);
        SetAnchor(shopGridPanel, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(shopGridPanel, 98, 100, 331, 411);

        // Note: The ArenaShopPanel.cs clears itemContainer content on OnEnable!
        // We will map shopGridPanel as itemContainer because they asked to map logic.
        // It's up to the user to either rewrite ArenaShopPanel.cs or use dynamic layout (GridLayoutGroup).
        // Since the prompt instructs specific absolute coordinates for mock cards, we must build them.
        
        CreateFeaturedCard("Ancient Spellbook", shopGridPanel.transform, new Vector2(19, -107), new Vector2(418, 587));
        CreateShopItemCard("Rusty Sword", shopGridPanel.transform, new Vector2(-69, -106), new Vector2(237, 279), true);
        CreateShopItemCard("Magic Potion", shopGridPanel.transform, new Vector2(-69, -417), new Vector2(237, 276), true);

        GameObject bottomRow = CreateUIElement("BottomItemsRow", shopGridPanel.transform);
        SetAnchor(bottomRow, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        bottomRow.GetComponent<RectTransform>().anchoredPosition = new Vector2(9, -810);
        bottomRow.GetComponent<RectTransform>().sizeDelta = new Vector2(861, 295);

        HorizontalLayoutGroup hlg = bottomRow.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 75;
        hlg.childAlignment = TextAnchor.UpperLeft;
        hlg.childControlWidth = true; hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;

        CreateShopItemCard("Knight's Helm", bottomRow.transform, Vector2.zero, new Vector2(237, 295), false);
        CreateShopItemCard("Summon Scroll", bottomRow.transform, Vector2.zero, new Vector2(237, 295), false);
        CreateShopItemCard("Experience Tome", bottomRow.transform, Vector2.zero, new Vector2(237, 295), false);

        // BottomNavBar is omitted to share with MainScreen.

        // --- Logic Mapping ---
        canvasObj.name = "Panel_ArenaShop";
        LegendOfBlood.ArenaShopPanel shopScript = canvasObj.AddComponent<LegendOfBlood.ArenaShopPanel>();
        shopScript.PanelType = LegendOfBlood.UIPanelType.ArenaShop;

        SerializedObject so = new SerializedObject(shopScript);
        so.Update();

        so.FindProperty("closeButton").objectReferenceValue = closeBtnComp;
        so.FindProperty("adFreebieButton").objectReferenceValue = adBtnComp;
        so.FindProperty("itemContainer").objectReferenceValue = shopGridPanel.transform;

        // Try to find a logical item prefab
        GameObject cardPrefab = null;
        string[] guids = AssetDatabase.FindAssets("t:Prefab ShopItemCard");
        if (guids.Length == 0) guids = AssetDatabase.FindAssets("t:Prefab ArenaShopItem");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
        if (cardPrefab != null) {
            so.FindProperty("shopItemPrefab").objectReferenceValue = cardPrefab;
        }

        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(canvasObj, "Generate Arena Shop Panel UI");
        Selection.activeGameObject = canvasObj;
        Debug.Log("Arena Shop Panel UI Generated Successfully!");
    }

    private static void CreateFeaturedCard(string name, Transform parent, Vector2 pos, Vector2 size) {
        GameObject card = CreateUIElement("FeaturedItem_" + name.Replace(" ", ""), parent);
        SetAnchor(card, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
        card.GetComponent<RectTransform>().anchoredPosition = pos;
        card.GetComponent<RectTransform>().sizeDelta = size;
        
        GameObject frame = CreateUIElement("CardFrame", card.transform);
        SetAnchor(frame, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(frame, 0, 0, 0, 0);
        frame.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.3f, 1f);

        GameObject itemName = CreateUIElement("ItemName", card.transform);
        SetAnchor(itemName, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        itemName.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -40);
        itemName.GetComponent<RectTransform>().sizeDelta = new Vector2(290, 54);
        GameObject nText = AddTMPText(itemName, name, 24);

        GameObject iconArea = CreateUIElement("IconArea", card.transform);
        SetAnchor(iconArea, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        iconArea.GetComponent<RectTransform>().anchoredPosition = new Vector2(-7, 12);
        iconArea.GetComponent<RectTransform>().sizeDelta = new Vector2(238, 245);
        iconArea.AddComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1f);

        GameObject hotBadge = CreateUIElement("HotBadge", card.transform);
        SetAnchor(hotBadge, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
        hotBadge.GetComponent<RectTransform>().anchoredPosition = new Vector2(-58, 32);
        hotBadge.GetComponent<RectTransform>().sizeDelta = new Vector2(96, 95);
        hotBadge.AddComponent<Image>().color = new Color(0.9f, 0.1f, 0.1f, 1f);

        GameObject priceBtn = CreateUIElement("PriceButton", card.transform);
        SetAnchor(priceBtn, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        priceBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 55);
        priceBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(213, 70);
        priceBtn.AddComponent<Image>().color = new Color(0.1f, 0.8f, 0.2f, 1f);
        Button btn = priceBtn.AddComponent<Button>();
        GameObject pText = AddTMPText(priceBtn, "8000", 24);

        // Tự động Add Logic Component vào từng thẻ
        LegendOfBlood.ArenaShopItem itemScript = card.AddComponent<LegendOfBlood.ArenaShopItem>();
        SerializedObject so = new SerializedObject(itemScript);
        so.Update();
        so.FindProperty("itemNameText").objectReferenceValue = nText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("priceText").objectReferenceValue = pText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("buyButton").objectReferenceValue = btn;
        so.ApplyModifiedProperties();
    }

    private static void CreateShopItemCard(string name, Transform parent, Vector2 pos, Vector2 size, bool isAbsolute) {
        GameObject card = CreateUIElement("Item_" + name.Replace(" ", ""), parent);
        if (isAbsolute) {
            SetAnchor(card, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1)); // Top right
            card.GetComponent<RectTransform>().anchoredPosition = pos;
        } else {
            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredWidth = size.x;
            le.preferredHeight = size.y;
        }
        card.GetComponent<RectTransform>().sizeDelta = size;
        
        GameObject frame = CreateUIElement("CardFrame", card.transform);
        SetAnchor(frame, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(frame, 0, 0, 0, 0);
        frame.AddComponent<Image>().color = new Color(0.3f, 0.2f, 0.2f, 1f);

        GameObject itemName = CreateUIElement("ItemName", card.transform);
        SetAnchor(itemName, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        itemName.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -18);
        itemName.GetComponent<RectTransform>().sizeDelta = new Vector2(177, 48);
        GameObject nText = AddTMPText(itemName, name, 18);

        GameObject iconArea = CreateUIElement("IconArea", card.transform);
        SetAnchor(iconArea, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        iconArea.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 5);
        iconArea.GetComponent<RectTransform>().sizeDelta = new Vector2(139, 118);
        iconArea.AddComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1f);

        GameObject priceBtn = CreateUIElement("PriceButton", card.transform);
        SetAnchor(priceBtn, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        priceBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 17);
        priceBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(159, 52);
        priceBtn.AddComponent<Image>().color = new Color(0.1f, 0.8f, 0.2f, 1f);
        Button btn = priceBtn.AddComponent<Button>();
        GameObject pText = AddTMPText(priceBtn, "1200", 18);

        // Tự động Add Logic Component vào từng thẻ nhỏ
        LegendOfBlood.ArenaShopItem itemScript = card.AddComponent<LegendOfBlood.ArenaShopItem>();
        SerializedObject so = new SerializedObject(itemScript);
        so.Update();
        so.FindProperty("itemNameText").objectReferenceValue = nText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("priceText").objectReferenceValue = pText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("buyButton").objectReferenceValue = btn;
        so.ApplyModifiedProperties();
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
    
    private static void SetAnchorCenter(GameObject go, Vector2 pos, Vector2 size) {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
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


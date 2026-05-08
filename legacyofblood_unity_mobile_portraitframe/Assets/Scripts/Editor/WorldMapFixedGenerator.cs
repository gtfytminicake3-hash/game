using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class WorldMapFixedGenerator : MonoBehaviour
{
    [MenuItem("UI Tools/Generate Region World Map")]
    public static void GenerateUI()
    {
        // 0. Cleanup Old
        var oldPanels = Object.FindObjectsByType<LegendOfBlood.WorldMapFixedController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in oldPanels) { Undo.DestroyObjectImmediate(p.gameObject); }
        GameObject broken = GameObject.Find("WorldMap_Canvas");
        if (broken != null) Undo.DestroyObjectImmediate(broken);

        // Tìm và xóa Panel cũ của sếp, lưu lại vị trí (Parent) nếu có
        Transform parentTransform = null;
        GameObject oldExtracted = GameObject.Find("EXTRACTED_WorldMap_Panel");
        if (oldExtracted != null) {
            parentTransform = oldExtracted.transform.parent;
            Undo.DestroyObjectImmediate(oldExtracted);
        }

        GameObject canvasObj = new GameObject("EXTRACTED_WorldMap_Panel");
        if (parentTransform != null) {
            canvasObj.transform.SetParent(parentTransform, false);
        }
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject root = CreateUIElement("WorldMapRoot", canvasObj.transform);
        SetAnchor(root, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(root, 0, 0, 0, 0);

        // --- 1. Map Viewport (Scroll Rect) ---
        GameObject mapViewport = CreateUIElement("MapViewport", root.transform);
        SetAnchor(mapViewport, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(mapViewport, 0, 0, 0, 229); // Chừa chỗ Bottom Nav
        mapViewport.AddComponent<RectMask2D>();
        
        GameObject mapContent = CreateUIElement("MapContent", mapViewport.transform);
        SetAnchorCenter(mapContent, Vector2.zero, new Vector2(2160, 3840)); // Huge map
        Image mapBg = mapContent.AddComponent<Image>();
        mapBg.color = new Color(0.3f, 0.5f, 0.3f, 1f); // Xanh lá placeholder
        
        // Cấu hình ScrollRect
        ScrollRect scroll = mapViewport.AddComponent<ScrollRect>();
        scroll.content = mapContent.GetComponent<RectTransform>();
        scroll.movementType = ScrollRect.MovementType.Clamped;
        
        // Tạo 5 vùng
        GameObject btnSnow = CreateRegionNode(mapContent.transform, "Region_Snow", new Vector2(-400, 800), "Snowy Mountains");
        GameObject btnForest = CreateRegionNode(mapContent.transform, "Region_Forest", new Vector2(0, 300), "Capital Forest");
        GameObject btnTree = CreateRegionNode(mapContent.transform, "Region_Tree", new Vector2(400, 400), "Magic Tree");
        GameObject btnDesert = CreateRegionNode(mapContent.transform, "Region_Desert", new Vector2(-400, -600), "Desert Ruins");
        GameObject btnVolcano = CreateRegionNode(mapContent.transform, "Region_Volcano", new Vector2(400, -800), "Volcanic Lair");

        // --- 2. Top HUD ---
        GameObject topBar = CreateUIElement("TopBar", root.transform);
        SetAnchor(topBar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        SetOffsets(topBar, 0, 0, 0, -120);
        topBar.AddComponent<Image>().color = new Color(0.4f, 0.1f, 0.1f, 1f); // Đỏ sậm
        
        AddTMPText(topBar, "WORLD MAP", 60).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

        // HUD Dưới Top Bar
        GameObject hudArea = CreateUIElement("HUDArea", root.transform);
        SetAnchor(hudArea, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        SetOffsets(hudArea, 0, 0, 120, -320);

        // Portrait
        GameObject portrait = CreateUIElement("CaptainPortrait", hudArea.transform);
        SetAnchor(portrait, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
        portrait.GetComponent<RectTransform>().anchoredPosition = new Vector2(20, -20);
        portrait.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 180);
        portrait.AddComponent<Image>().color = new Color(0.6f, 0.6f, 0.6f, 1f);

        // Resources
        GameObject resBar = CreateUIElement("ResourceBar", hudArea.transform);
        SetAnchor(resBar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        SetOffsets(resBar, 190, 20, 20, -120);
        resBar.AddComponent<Image>().color = new Color(0.8f, 0.7f, 0.5f, 1f); // Vàng nhạt

        // Quests
        GameObject quests = CreateUIElement("ActiveQuests", hudArea.transform);
        SetAnchor(quests, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));
        quests.GetComponent<RectTransform>().anchoredPosition = new Vector2(-20, -140);
        quests.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 200);
        quests.AddComponent<Image>().color = new Color(0.9f, 0.8f, 0.6f, 1f); // Cuộn giấy
        AddTMPText(quests, "ACTIVE QUESTS\nDefeat the Dragon", 24).GetComponent<TMP_Text>().color = Color.black;

        // --- 3. REGION DETAIL POPUP (Modal) ---
        GameObject popupObj = CreateRegionDetailPopup(root.transform);
        popupObj.SetActive(false); // Ẩn mặc định

        // --- 4. Bottom Nav Bar ---
        GameObject bottomNav = CreateUIElement("BottomNavBar", root.transform);
        SetAnchor(bottomNav, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
        SetOffsets(bottomNav, 0, 0, 0, 0);
        bottomNav.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 229);
        bottomNav.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        GameObject lobbyBtn = CreateUIElement("LobbyTab", bottomNav.transform);
        SetAnchorCenter(lobbyBtn, new Vector2(0, 0), new Vector2(200, 200));
        lobbyBtn.AddComponent<Image>().color = new Color(0.8f, 0.6f, 0.2f, 1f);
        Button lobbyButtonComp = lobbyBtn.AddComponent<Button>();

        // --- 5. GẮN SCRIPT & MAPPING ---
        LegendOfBlood.WorldMapFixedController controller = canvasObj.AddComponent<LegendOfBlood.WorldMapFixedController>();
        SerializedObject so = new SerializedObject(controller);
        so.Update();
        so.FindProperty("mapContainer").objectReferenceValue = mapContent.GetComponent<RectTransform>();
        so.FindProperty("regionDetailPopup").objectReferenceValue = popupObj;
        so.FindProperty("btnRegionSnow").objectReferenceValue = btnSnow.GetComponent<Button>();
        so.FindProperty("btnRegionForest").objectReferenceValue = btnForest.GetComponent<Button>();
        so.FindProperty("btnRegionTree").objectReferenceValue = btnTree.GetComponent<Button>();
        so.FindProperty("btnRegionDesert").objectReferenceValue = btnDesert.GetComponent<Button>();
        so.FindProperty("btnRegionVolcano").objectReferenceValue = btnVolcano.GetComponent<Button>();
        so.FindProperty("btnLobby").objectReferenceValue = lobbyButtonComp;
        so.ApplyModifiedProperties();

        // Register UIManager
        canvasObj.AddComponent<LegendOfBlood.UIPanel>().PanelType = LegendOfBlood.UIPanelType.WorldMap;

        Debug.Log("WorldMap (Fixed Region) generated successfully!");
    }

    private static GameObject CreateRegionDetailPopup(Transform parent)
    {
        GameObject modal = CreateUIElement("RegionDetailPopup", parent);
        SetAnchor(modal, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(modal, 0, 0, 0, 0);
        modal.AddComponent<Image>().color = new Color(0, 0, 0, 0.8f);

        GameObject scrollFrame = CreateUIElement("ScrollFrame", modal.transform);
        SetAnchorCenter(scrollFrame, Vector2.zero, new Vector2(900, 1400));
        scrollFrame.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f); // Khung đá
        
        GameObject innerBg = CreateUIElement("InnerMap", scrollFrame.transform);
        SetAnchor(innerBg, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(innerBg, 40, 40, 120, 60);
        innerBg.AddComponent<Image>().color = new Color(0.6f, 0.2f, 0.1f, 1f); // Nền dung nham placeholder

        GameObject title = AddTMPText(scrollFrame, "VOLCANIC LAIR\n<size=30>REGIONAL MAP</size>", 50);
        SetAnchor(title, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        title.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -60);
        title.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 100);

        // Tạo 6 Sub-stages dọc lượn sóng
        CreateSubStageNode(innerBg.transform, "Stage_1", new Vector2(-200, 500), "Lava Coast");
        CreateSubStageNode(innerBg.transform, "Stage_2", new Vector2(200, 300), "Obsidian Vein");
        CreateSubStageNode(innerBg.transform, "Stage_3", new Vector2(-250, 0), "Cultist Outpost");
        CreateSubStageNode(innerBg.transform, "Stage_4", new Vector2(200, -200), "Altar of Fire");
        CreateSubStageNode(innerBg.transform, "Stage_5", new Vector2(-200, -400), "Deeper Fight");
        CreateSubStageNode(innerBg.transform, "Stage_6", new Vector2(150, -600), "Skeletal Dragon");

        // Nút tắt
        GameObject closeBtn = CreateUIElement("CloseButton", scrollFrame.transform);
        SetAnchor(closeBtn, new Vector2(1, 1), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
        closeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-60, -60);
        closeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);
        closeBtn.AddComponent<Image>().color = Color.red;
        closeBtn.AddComponent<Button>();
        AddTMPText(closeBtn, "X", 50);

        return modal;
    }

    private static GameObject CreateRegionNode(Transform parent, string name, Vector2 pos, string labelText)
    {
        GameObject node = CreateUIElement(name, parent);
        SetAnchorCenter(node, pos, new Vector2(120, 120));
        node.AddComponent<Image>().color = new Color(0.9f, 0.8f, 0.2f, 1f); // Nút vuông vàng
        node.AddComponent<Button>();

        GameObject txt = AddTMPText(node, labelText, 36);
        SetAnchorCenter(txt, new Vector2(0, -80), new Vector2(300, 60));
        return node;
    }

    private static GameObject CreateSubStageNode(Transform parent, string name, Vector2 pos, string labelText)
    {
        GameObject node = CreateUIElement(name, parent);
        SetAnchorCenter(node, pos, new Vector2(100, 100));
        node.AddComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f, 1f);
        node.AddComponent<Button>();

        GameObject txt = AddTMPText(node, labelText, 36);
        SetAnchorCenter(txt, new Vector2(0, -70), new Vector2(350, 50));
        txt.GetComponent<TMP_Text>().color = Color.black;
        return node;
    }

    private static GameObject CreateUIElement(string name, Transform parent) {
        GameObject go = new GameObject(name);
        go.AddComponent<RectTransform>();
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void SetAnchor(GameObject go, Vector2 min, Vector2 max, Vector2 pivot) {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = min; rt.anchorMax = max; rt.pivot = pivot;
    }

    private static void SetAnchorCenter(GameObject go, Vector2 pos, Vector2 size) {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static void SetOffsets(GameObject go, float left, float right, float top, float bottom) {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }

    private static GameObject AddTMPText(GameObject parent, string text, int fontSize) {
        GameObject textObj = CreateUIElement("Text", parent.transform);
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


#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopulationManagerPanelGenerator : EditorWindow
{
    [MenuItem("UI Tools/Generate Population Manager Panel")]
    public static void GenerateUI()
    {
        // 0. Auto-Cleanup Old UI
        var oldPanels = Object.FindObjectsByType<LegendOfBlood.PopulationManagerPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in oldPanels) {
            Undo.DestroyObjectImmediate(p.gameObject);
        }

        GameObject canvasObj = new GameObject("PopulationManager_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // 1. Root
        GameObject screenRoot = CreateUIElement("PopulationManagerRoot", canvasObj.transform);
        SetAnchor(screenRoot, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(screenRoot, 0, 0, 0, 0);
        if (System.Type.GetType("SafeArea") != null) {
            screenRoot.AddComponent(System.Type.GetType("SafeArea"));
        }

        // 2. BackgroundParchment
        GameObject backgroundParchment = CreateUIElement("BackgroundParchment", screenRoot.transform);
        SetAnchor(backgroundParchment, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(backgroundParchment, 0, 0, 0, 0);
        backgroundParchment.AddComponent<Image>().color = new Color(0.9f, 0.85f, 0.7f, 1f); // Màu giấy da

        // 3. TopHeader
        GameObject topHeader = CreateUIElement("TopHeader", screenRoot.transform);
        SetAnchor(topHeader, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        SetOffsets(topHeader, 39, 75, 19, -473); // Height 454. Top 19 -> Bottom: -(19+454)=-473

        GameObject titlePlate = CreateUIElement("TitlePlate", topHeader.transform);
        SetAnchor(titlePlate, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        titlePlate.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -13);
        titlePlate.GetComponent<RectTransform>().sizeDelta = new Vector2(943, 168);
        titlePlate.AddComponent<Image>().color = new Color(0.3f, 0.2f, 0.1f, 1f);

        GameObject titleTextObj = CreateUIElement("TitleText", topHeader.transform);
        SetAnchor(titleTextObj, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        titleTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -45);
        titleTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(669, 62);
        GameObject tmpTitle = AddTMPText(titleTextObj, "Population Manager", 45);

        GameObject capacityTextObj = CreateUIElement("CapacityText", topHeader.transform);
        SetAnchor(capacityTextObj, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        capacityTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -220);
        capacityTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 116);
        GameObject tmpCap = AddTMPText(capacityTextObj, "45 / 50", 60);
        tmpCap.GetComponent<TextMeshProUGUI>().color = new Color(0.1f, 0.1f, 0.1f, 1f);

        GameObject capacityBar = CreateUIElement("CapacityBar", topHeader.transform);
        SetAnchor(capacityBar, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        capacityBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -376);
        capacityBar.GetComponent<RectTransform>().sizeDelta = new Vector2(739, 99);

        GameObject barFrame = CreateUIElement("BarFrame", capacityBar.transform);
        SetAnchor(barFrame, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(barFrame, 0, 0, 0, 0);
        barFrame.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);

        GameObject fillArea = CreateUIElement("FillArea", capacityBar.transform);
        SetAnchor(fillArea, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(fillArea, 18, 18, 17, 21); // Left 18, Right 18, Top 17, Bottom 21
        fillArea.AddComponent<Image>().color = new Color(0.1f, 0.8f, 0.2f, 1f); // Màu thanh đầy

        // 4. MemberListPanel
        GameObject memberListPanel = CreateUIElement("MemberListPanel", screenRoot.transform);
        SetAnchor(memberListPanel, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
        SetOffsets(memberListPanel, 39, 61, 562, 152); // Top 562, Bottom 152
        
        ScrollRect sr = memberListPanel.AddComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        
        GameObject scrollViewport = CreateUIElement("ScrollViewport", memberListPanel.transform);
        SetAnchor(scrollViewport, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(scrollViewport, 3, 59, 16, 32); 
        scrollViewport.AddComponent<Image>().color = new Color(0,0,0,0.1f);
        scrollViewport.AddComponent<RectMask2D>();
        sr.viewport = scrollViewport.GetComponent<RectTransform>();

        GameObject content = CreateUIElement("Content", scrollViewport.transform);
        SetAnchor(content, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1)); // Top Stretch
        SetOffsets(content, 0, 0, 0, 0); 
        sr.content = content.GetComponent<RectTransform>();

        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.spacing = 30;
        vlg.childControlWidth = true; vlg.childControlHeight = true;
        vlg.childForceExpandWidth = false; vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Tạo 1 prefab mẫu làm Template
        GameObject templateRow = CreateMemberRowPrefab("MemberRow_Template", content.transform);
        string prefabPath = "Assets/Prefabs/PopulationRow_Auto.prefab";
        if (!System.IO.Directory.Exists("Assets/Prefabs")) System.IO.Directory.CreateDirectory("Assets/Prefabs");
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(templateRow, prefabPath);

        // Tạo 5 mock rows để Editor trông đẹp
        for (int i = 1; i <= 5; i++) {
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(savedPrefab, content.transform);
            inst.name = "MemberRow_0" + i;
        }
        Undo.DestroyObjectImmediate(templateRow);

        // ScrollBar (Right)
        GameObject scrollBarTrack = CreateUIElement("ScrollBarTrack", memberListPanel.transform);
        SetAnchor(scrollBarTrack, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f));
        scrollBarTrack.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        scrollBarTrack.GetComponent<RectTransform>().sizeDelta = new Vector2(19, 0);
        SetOffsets(scrollBarTrack, 0, 0, 0, 0); // Stretch high
        scrollBarTrack.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // 5. BottomCommandBar
        GameObject bottomCommandBar = CreateUIElement("BottomCommandBar", screenRoot.transform);
        SetAnchor(bottomCommandBar, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
        SetOffsets(bottomCommandBar, 0, 0, 0, 0);
        bottomCommandBar.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 131); // Height 131
        bottomCommandBar.AddComponent<Image>().color = new Color(0.15f, 0.1f, 0.1f, 1f);

        GameObject backBtn = CreateUIElement("BackButton", bottomCommandBar.transform);
        SetAnchor(backBtn, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0));
        backBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(42, 20);
        backBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(201, 113);
        backBtn.AddComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);
        Button backBtnComp = backBtn.AddComponent<Button>();
        AddTMPText(backBtn, "BACK", 24);

        GameObject sortBtn = CreateUIElement("SortButton", bottomCommandBar.transform);
        SetAnchor(sortBtn, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        sortBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(60, 20);
        sortBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(168, 113);
        sortBtn.AddComponent<Image>().color = new Color(0.2f, 0.6f, 0.2f, 1f);
        Button sortBtnComp = sortBtn.AddComponent<Button>();
        AddTMPText(sortBtn, "Lọc:\nLv \u25BC", 20);

        GameObject filterBtn = CreateUIElement("FilterButton", bottomCommandBar.transform);
        SetAnchor(filterBtn, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0));
        filterBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-105, 20);
        filterBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(231, 113);
        filterBtn.AddComponent<Image>().color = new Color(0.6f, 0.2f, 0.6f, 1f);
        Button filterBtnComp = filterBtn.AddComponent<Button>();
        AddTMPText(filterBtn, "Hệ:\nTất cả", 20);

        canvasObj.name = "Panel_PopulationManager";
        LegendOfBlood.PopulationManagerPanel popScript = canvasObj.AddComponent<LegendOfBlood.PopulationManagerPanel>();
        popScript.PanelType = LegendOfBlood.UIPanelType.PopulationManager;

        SerializedObject so = new SerializedObject(popScript);
        so.Update();

        so.FindProperty("closeButton").objectReferenceValue = backBtnComp;
        so.FindProperty("listContainer").objectReferenceValue = content.transform;
        so.FindProperty("populationCountText").objectReferenceValue = tmpCap.GetComponent<TextMeshProUGUI>();
        so.FindProperty("sortButton").objectReferenceValue = sortBtnComp;
        so.FindProperty("filterButton").objectReferenceValue = filterBtnComp;

        if (savedPrefab != null) {
            so.FindProperty("heroCardPrefab").objectReferenceValue = savedPrefab;
        }

        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(canvasObj, "Generate Population Manager Panel UI");
        Selection.activeGameObject = canvasObj;
        Debug.Log("Population Manager Panel UI Generated Successfully!");
    }

    private static GameObject CreateMemberRowPrefab(string name, Transform parent) {
        GameObject row = CreateUIElement(name, parent);
        LayoutElement le = row.AddComponent<LayoutElement>();
        le.preferredWidth = 938;
        le.preferredHeight = 214;
        
        LegendOfBlood.PopulationHeroCard cardScript = row.AddComponent<LegendOfBlood.PopulationHeroCard>();
        SerializedObject soCard = new SerializedObject(cardScript);
        
        GameObject rowBg = CreateUIElement("RowBg", row.transform);
        SetAnchor(rowBg, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(rowBg, 0, 0, 0, 0);
        rowBg.AddComponent<Image>().color = new Color(0.85f, 0.8f, 0.7f, 1f);

        GameObject pFrame = CreateUIElement("PortraitFrame", row.transform);
        SetAnchor(pFrame, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        pFrame.GetComponent<RectTransform>().anchoredPosition = new Vector2(19, 0);
        pFrame.GetComponent<RectTransform>().sizeDelta = new Vector2(168, 168);
        pFrame.AddComponent<Image>().color = new Color(0.3f, 0.2f, 0.1f, 1f);

        GameObject portrait = CreateUIElement("Portrait", pFrame.transform);
        SetAnchor(portrait, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        portrait.GetComponent<RectTransform>().sizeDelta = new Vector2(146, 146);
        Image portraitImg = portrait.AddComponent<Image>();
        portraitImg.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        soCard.FindProperty("avatarImage").objectReferenceValue = portraitImg;

        GameObject infoBlock = CreateUIElement("InfoTextBlock", row.transform);
        SetAnchor(infoBlock, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        infoBlock.GetComponent<RectTransform>().anchoredPosition = new Vector2(211, 0);
        infoBlock.GetComponent<RectTransform>().sizeDelta = new Vector2(447, 141);

        GameObject nTxt = CreateUIElement("NameText", infoBlock.transform);
        SetAnchor(nTxt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1));
        SetOffsets(nTxt, 0, 0, 0, -43); 
        GameObject nTmp = AddTMPText(nTxt, "Hero Name", 32);
        nTmp.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        nTmp.GetComponent<TextMeshProUGUI>().color = Color.black;
        soCard.FindProperty("heroNameText").objectReferenceValue = nTmp.GetComponent<TextMeshProUGUI>();

        // Tách Class text và Level Text ra để match với PopulationHeroCard script
        GameObject cTxt = CreateUIElement("LevelText", infoBlock.transform);
        SetAnchor(cTxt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1));
        SetOffsets(cTxt, 0, -100, 47, -84); // Rộng một nửa xíu
        GameObject cTmp = AddTMPText(cTxt, "LV.15", 24);
        cTmp.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        cTmp.GetComponent<TextMeshProUGUI>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
        soCard.FindProperty("levelText").objectReferenceValue = cTmp.GetComponent<TextMeshProUGUI>();

        GameObject profTxt = CreateUIElement("ProfessionText", infoBlock.transform);
        SetAnchor(profTxt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1));
        SetOffsets(profTxt, 100, 0, 47, -84); // Dịch về phải
        GameObject profTmp = AddTMPText(profTxt, "Mage", 24);
        profTmp.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        profTmp.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.2f, 0.2f, 1f);
        soCard.FindProperty("professionText").objectReferenceValue = profTmp.GetComponent<TextMeshProUGUI>();

        GameObject sTxt = CreateUIElement("StatsText", infoBlock.transform);
        SetAnchor(sTxt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1));
        SetOffsets(sTxt, 0, 0, 88, -131); 
        GameObject sTmp = AddTMPText(sTxt, "CP: 450", 20);
        sTmp.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        sTmp.GetComponent<TextMeshProUGUI>().color = new Color(0.1f, 0.1f, 0.4f, 1f);
        soCard.FindProperty("cpText").objectReferenceValue = sTmp.GetComponent<TextMeshProUGUI>();

        GameObject dismissBtn = CreateUIElement("DismissButton", row.transform);
        SetAnchor(dismissBtn, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
        dismissBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-32, 0);
        dismissBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(177, 177);
        dismissBtn.AddComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);
        Button dBtn = dismissBtn.AddComponent<Button>();
        soCard.FindProperty("dismissButton").objectReferenceValue = dBtn;
        AddTMPText(dismissBtn, "X", 60);

        soCard.ApplyModifiedProperties();
        return row;
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


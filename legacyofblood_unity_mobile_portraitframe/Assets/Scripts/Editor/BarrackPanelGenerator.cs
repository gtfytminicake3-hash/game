#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BarrackPanelGenerator : EditorWindow
{
    [MenuItem("UI Tools/Generate Barrack Panel")]
    public static void GenerateUI()
    {
        // 0. Auto-Cleanup Old UI to prevent UIManager conflicts
        var oldPanels = Object.FindObjectsByType<LegendOfBlood.BarrackPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in oldPanels) {
            Undo.DestroyObjectImmediate(p.gameObject);
        }

        GameObject canvasObj = new GameObject("Barrack_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand; // Match width or height
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        GameObject safeAreaObj = CreateUIElement("SafeArea", canvasObj.transform);
        SetAnchor(safeAreaObj, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(safeAreaObj, 0, 0, 0, 0); 
        // Lấy SafeArea Script nếu bạn đã tạo (nếu không có thì hệ thống bỏ qua cảnh báo biên dịch)
        if (System.Type.GetType("SafeArea") != null) {
            safeAreaObj.AddComponent(System.Type.GetType("SafeArea"));
        }

        // 1. TopHeader
        GameObject topHeader = CreateUIElement("TopHeader", safeAreaObj.transform);
        SetAnchor(topHeader, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1)); // Top Stretch
        SetOffsets(topHeader, 0, 0, 0, -109);
        topHeader.AddComponent<Image>().color = new Color(0.12f, 0.15f, 0.18f, 1f);

        GameObject levelBadge = CreateUIElement("LevelBadge", topHeader.transform);
        SetAnchor(levelBadge, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1)); // Top Left
        levelBadge.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        levelBadge.GetComponent<RectTransform>().sizeDelta = new Vector2(130, 179);
        levelBadge.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 1f);

        GameObject titleTextArea = CreateUIElement("TitleTextArea", topHeader.transform);
        SetAnchor(titleTextArea, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1)); // Top Center
        titleTextArea.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        titleTextArea.GetComponent<RectTransform>().sizeDelta = new Vector2(571, 80);
        AddTMPText(titleTextArea, "Barrack Panel", 45);

        // Nút truy cập PopulationManager ở mép phải của Header
        GameObject popBtn = CreateUIElement("PopulationButton", topHeader.transform);
        SetAnchor(popBtn, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1)); // Top Right
        popBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-20, -15);
        popBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 80);
        popBtn.AddComponent<Image>().color = new Color(0.1f, 0.5f, 0.8f, 1f);
        popBtn.AddComponent<Button>();
        
        LegendOfBlood.UIPanelNavButton navPopBtn = popBtn.AddComponent<LegendOfBlood.UIPanelNavButton>();
        navPopBtn.targetPanel = LegendOfBlood.UIPanelType.PopulationManager;
        
        AddTMPText(popBtn, "Bầu Thôn Trưởng", 22);

        // 2. TopStatsRow
        GameObject topStatsRow = CreateUIElement("TopStatsRow", safeAreaObj.transform);
        SetAnchor(topStatsRow, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1)); // Top Stretch
        SetOffsets(topStatsRow, 0, 0, 96, -224); // Height = 128
        
        GameObject hpBar = CreateUIElement("HpBar", topStatsRow.transform);
        SetAnchor(hpBar, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
        hpBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(156, -6);
        hpBar.GetComponent<RectTransform>().sizeDelta = new Vector2(188, 42);
        hpBar.AddComponent<Image>().color = new Color(0.8f, 0.1f, 0.1f, 1f);

        GameObject resourceRow = CreateUIElement("ResourceRow", topStatsRow.transform);
        SetAnchor(resourceRow, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        resourceRow.GetComponent<RectTransform>().anchoredPosition = new Vector2(31, 0);
        resourceRow.GetComponent<RectTransform>().sizeDelta = new Vector2(668, 109);
        
        HorizontalLayoutGroup hlgRes = resourceRow.AddComponent<HorizontalLayoutGroup>();
        hlgRes.childAlignment = TextAnchor.MiddleCenter;
        hlgRes.childControlWidth = true; hlgRes.childControlHeight = true;
        
        GameObject goldBlock = CreateUIElement("GoldBlock", resourceRow.transform);
        LayoutElement leG = goldBlock.AddComponent<LayoutElement>(); leG.preferredWidth = 230; leG.preferredHeight = 99;
        goldBlock.AddComponent<Image>().color = new Color(0.9f, 0.8f, 0.2f, 1f);
        
        GameObject silverBlock = CreateUIElement("SilverBlock", resourceRow.transform);
        LayoutElement leS = silverBlock.AddComponent<LayoutElement>(); leS.preferredWidth = 237; leS.preferredHeight = 99;
        silverBlock.AddComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f, 1f);
        
        GameObject gemBlock = CreateUIElement("BlueGemBlock", resourceRow.transform);
        LayoutElement leB = gemBlock.AddComponent<LayoutElement>(); leB.preferredWidth = 175; leB.preferredHeight = 99;
        gemBlock.AddComponent<Image>().color = new Color(0.2f, 0.5f, 1f, 1f);

        // WallPlatform (Đặt trên HeroShowcase)
        GameObject wallPlatform = CreateUIElement("WallPlatform", safeAreaObj.transform);
        SetAnchor(wallPlatform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        SetOffsets(wallPlatform, 0, 0, 365, -483);
        wallPlatform.AddComponent<Image>().color = new Color(0.25f, 0.2f, 0.15f, 1f);

        // 3. HeroShowcaseRow
        GameObject heroShowcaseRow = CreateUIElement("HeroShowcaseRow", safeAreaObj.transform);
        SetAnchor(heroShowcaseRow, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        heroShowcaseRow.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -224);
        heroShowcaseRow.GetComponent<RectTransform>().sizeDelta = new Vector2(989, 227);
        
        HorizontalLayoutGroup hlgHero = heroShowcaseRow.AddComponent<HorizontalLayoutGroup>();
        hlgHero.spacing = 15;
        hlgHero.childAlignment = TextAnchor.MiddleCenter;

        for (int i=1; i<=6; i++) {
            GameObject heroSlot = CreateUIElement("HeroSlot_0" + i, heroShowcaseRow.transform);
            LayoutElement leHero = heroSlot.AddComponent<LayoutElement>();
            leHero.preferredWidth = 130; leHero.preferredHeight = 175;
            heroSlot.AddComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f, 0.5f);
        }

        // 4. ActionButtonsRow
        GameObject actionButtonsRow = CreateUIElement("ActionButtonsRow", safeAreaObj.transform);
        SetAnchor(actionButtonsRow, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        SetOffsets(actionButtonsRow, 64, 64, 560, -686);

        GameObject sortBtn = CreateUIElement("SortButton", actionButtonsRow.transform);
        SetAnchor(sortBtn, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        sortBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(30, 0);
        sortBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(383, 102);
        sortBtn.AddComponent<Image>().color = new Color(0.2f, 0.6f, 0.2f, 1f);
        AddTMPText(sortBtn, "SORT", 36);

        GameObject filterBtn = CreateUIElement("FilterButton", actionButtonsRow.transform);
        SetAnchor(filterBtn, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
        filterBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-39, 0);
        filterBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(383, 102);
        filterBtn.AddComponent<Image>().color = new Color(0.6f, 0.2f, 0.2f, 1f);
        AddTMPText(filterBtn, "FILTER", 36);

        // 5. CardGridPanel
        GameObject cardGridPanel = CreateUIElement("CardGridPanel", safeAreaObj.transform);
        SetAnchor(cardGridPanel, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
        SetOffsets(cardGridPanel, 32, 36, 732, 191);
        
        GameObject frame = CreateUIElement("Frame", cardGridPanel.transform);
        SetAnchor(frame, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(frame, 0, 0, 0, 0);
        frame.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

        GameObject scrollView = CreateUIElement("ScrollView", cardGridPanel.transform);
        SetAnchor(scrollView, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(scrollView, 0, 0, 0, 0);
        ScrollRect sr = scrollView.AddComponent<ScrollRect>();

        GameObject viewport = CreateUIElement("Viewport", scrollView.transform);
        SetAnchor(viewport, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(viewport, 0, 0, 0, 0);
        viewport.AddComponent<Image>().color = new Color(0,0,0,0);
        viewport.AddComponent<RectMask2D>();
        
        GameObject content = CreateUIElement("Content", viewport.transform);
        SetAnchor(content, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1)); // Top Stretch
        SetOffsets(content, 0, 0, 0, 0); 
        
        GridLayoutGroup glg = content.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(201, 230);
        glg.spacing = new Vector2(39, 26);
        glg.padding = new RectOffset(33, 26, 121, 20); // Top padding 121
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 4;
        
        ContentSizeFitter csfGrid = content.AddComponent<ContentSizeFitter>();
        csfGrid.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        sr.content = content.GetComponent<RectTransform>();
        sr.viewport = viewport.GetComponent<RectTransform>();
        sr.horizontal = false;
        sr.vertical = true;

        for(int i=1; i<=16; i++) {
            CreateCardPrefab("HeroCard_" + i.ToString("00"), content.transform);
        }

        // 6. BottomNavBar ĐÃ BỊ LOẠI BỎ ĐỂ DÙNG CHUNG VỚI MAIN SCREEN

        // --- Thay thế logic cho Panel_Barrack ---
        canvasObj.name = "Panel_Barrack";
        LegendOfBlood.BarrackPanel barrackScript = canvasObj.GetComponent<LegendOfBlood.BarrackPanel>();
        if (barrackScript == null) {
            barrackScript = canvasObj.AddComponent<LegendOfBlood.BarrackPanel>();
        }
        barrackScript.PanelType = LegendOfBlood.UIPanelType.Barrack;

        SerializedObject so = new SerializedObject(barrackScript);
        so.Update();
        
        SerializedProperty containerProp = so.FindProperty("heroListContainer");
        if (containerProp != null) {
            containerProp.objectReferenceValue = content.transform;
        }

        GameObject heroCardPrefab = null;
        string[] guids = AssetDatabase.FindAssets("t:Prefab HeroCard");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            heroCardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
        if (heroCardPrefab != null) {
            SerializedProperty prefabProp = so.FindProperty("heroCardPrefab");
            if (prefabProp != null) prefabProp.objectReferenceValue = heroCardPrefab;
        }

        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(canvasObj, "Generate Barrack Panel UI");
        Selection.activeGameObject = canvasObj;
        Debug.Log("Barrack Panel UI Generated Successfully!");
    }

    private static void CreateCardPrefab(string name, Transform parent) {
        GameObject card = CreateUIElement(name, parent);
        
        GameObject frame = CreateUIElement("CardFrame", card.transform);
        SetAnchor(frame, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(frame, 0, 0, 0, 0);
        frame.AddComponent<Image>().color = new Color(0.3f, 0.2f, 0.1f, 1f);
        
        GameObject portrait = CreateUIElement("Portrait", card.transform);
        SetAnchor(portrait, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        portrait.GetComponent<RectTransform>().offsetMin = new Vector2(14, -166);
        portrait.GetComponent<RectTransform>().offsetMax = new Vector2(-14, -10);
        portrait.AddComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1f);

        GameObject namePlate = CreateUIElement("NamePlate", card.transform);
        SetAnchor(namePlate, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
        namePlate.GetComponent<RectTransform>().offsetMin = new Vector2(4, 14);
        namePlate.GetComponent<RectTransform>().offsetMax = new Vector2(-4, 60);
        Image npImg = namePlate.AddComponent<Image>();
        Sprite scrollSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/ScrollNameBg.png");
        if (scrollSprite != null)
        {
            npImg.sprite = scrollSprite;
            npImg.type = Image.Type.Sliced;
            npImg.color = Color.white;
        }
        else
        {
            npImg.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        }

        GameObject levelBadge = CreateUIElement("LevelBadgeBL", card.transform);
        SetAnchor(levelBadge, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0));
        levelBadge.GetComponent<RectTransform>().anchoredPosition = new Vector2(3, 39);
        levelBadge.GetComponent<RectTransform>().sizeDelta = new Vector2(42, 42);
        levelBadge.AddComponent<Image>().color = new Color(0.8f, 0.8f, 0.2f, 1f);

        GameObject classBadge = CreateUIElement("ClassBadgeBR", card.transform);
        SetAnchor(classBadge, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0));
        classBadge.GetComponent<RectTransform>().anchoredPosition = new Vector2(-6, 40);
        classBadge.GetComponent<RectTransform>().sizeDelta = new Vector2(36, 36);
        classBadge.AddComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f, 1f);

        GameObject affinityBadge = CreateUIElement("AffinityBadgeTR", card.transform);
        SetAnchor(affinityBadge, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));
        affinityBadge.GetComponent<RectTransform>().anchoredPosition = new Vector2(-7, -6);
        affinityBadge.GetComponent<RectTransform>().sizeDelta = new Vector2(34, 34);
        affinityBadge.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.8f, 1f);

        GameObject nameText = AddTMPText(card, name, 20);
        SetAnchor(nameText, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        nameText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 16);
        nameText.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 26);
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


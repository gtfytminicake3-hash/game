#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BreedingPanelGenerator : EditorWindow
{
    [MenuItem("UI Tools/Generate Breeding Panel")]
    public static void GenerateUI()
    {
        // 0. Auto-Cleanup Old UI
        var oldPanels = Object.FindObjectsByType<LegendOfBlood.BreedingUIController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in oldPanels) {
            Undo.DestroyObjectImmediate(p.gameObject);
        }
        
        // Dọn dẹp cả những Canvas lỗi bị sinh ra nửa chừng (Chưa kịp gắn script)
        GameObject broken1 = GameObject.Find("Breeding_Canvas");
        if (broken1 != null) Undo.DestroyObjectImmediate(broken1);
        GameObject broken2 = GameObject.Find("Panel_Breeding");
        if (broken2 != null) Undo.DestroyObjectImmediate(broken2);

        GameObject canvasObj = new GameObject("Breeding_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        GameObject root = CreateUIElement("BreedingWorkshopRoot", canvasObj.transform);
        SetAnchor(root, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(root, 0, 0, 0, 0);

        GameObject bg = CreateUIElement("BackgroundFrame", root.transform);
        SetAnchor(bg, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(bg, 0, 0, 0, 0);
        bg.AddComponent<Image>().color = new Color(0.12f, 0.1f, 0.08f, 1f);

        // --- MÀN HÌNH CHỌN LỰA (SELECTION AREA) ---
        GameObject selectionArea = CreateUIElement("SelectionArea", root.transform);
        SetAnchor(selectionArea, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(selectionArea, 0, 0, 0, 0);

        // 1. Top Header
        GameObject topHeader = CreateUIElement("TopHeader", selectionArea.transform);
        SetAnchor(topHeader, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        SetOffsets(topHeader, 38, 38, 0, -288);

        GameObject titleBanner = CreateUIElement("TitleBanner", topHeader.transform);
        SetAnchorCenter(titleBanner, Vector2.zero, new Vector2(752, 170));
        SetAnchor(titleBanner, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        titleBanner.AddComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);

        GameObject leftCrest = CreateUIElement("LeftCrest", topHeader.transform);
        SetAnchor(leftCrest, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
        leftCrest.GetComponent<RectTransform>().anchoredPosition = new Vector2(-5, -7);
        leftCrest.GetComponent<RectTransform>().sizeDelta = new Vector2(163, 176);
        leftCrest.AddComponent<Image>().color = new Color(0.8f, 0.8f, 0.2f, 1f);

        GameObject rightCrest = CreateUIElement("RightCrest", topHeader.transform);
        SetAnchor(rightCrest, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));
        rightCrest.GetComponent<RectTransform>().anchoredPosition = new Vector2(8, -8);
        rightCrest.GetComponent<RectTransform>().sizeDelta = new Vector2(168, 177);
        rightCrest.AddComponent<Image>().color = new Color(0.2f, 0.5f, 0.8f, 1f);

        GameObject titleTextObj = AddTMPText(topHeader, "Manuscript Breeding Workshop", 36);
        titleTextObj.name = "TitleText";
        SetAnchor(titleTextObj, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        titleTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -35);
        titleTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(431, 112);

        // Nút Back/Close đặt ở ngoài lề góc trên cùng Trái để dễ thấy
        GameObject closeBtn = CreateUIElement("CloseButton", selectionArea.transform);
        SetAnchor(closeBtn, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
        closeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(40, -40);
        closeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        closeBtn.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        Button closeBtnComp = closeBtn.AddComponent<Button>();
        AddTMPText(closeBtn, "<", 60);

        // 2. Workshop Content
        GameObject content = CreateUIElement("WorkshopContent", selectionArea.transform);
        SetAnchor(content, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(content, 59, 59, 194, 338);

        // --- Center Tube ---
        GameObject centerTube = CreateUIElement("CenterTubeGroup", content.transform);
        SetAnchor(centerTube, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        centerTube.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -105);
        centerTube.GetComponent<RectTransform>().sizeDelta = new Vector2(299, 667);

        GameObject connGlow = CreateUIElement("ConnectorGlow", centerTube.transform);
        SetAnchorCenter(connGlow, new Vector2(0, 85), new Vector2(299, 167));
        connGlow.AddComponent<Image>().color = new Color(0.2f, 0.5f, 0.8f, 0.3f);

        GameObject tubeFrame = CreateUIElement("TubeFrame", centerTube.transform);
        SetAnchor(tubeFrame, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        tubeFrame.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        tubeFrame.GetComponent<RectTransform>().sizeDelta = new Vector2(125, 507);
        tubeFrame.AddComponent<Image>().color = new Color(0.6f, 0.6f, 0.6f, 0.5f);

        GameObject tubeFill = CreateUIElement("TubeFill", centerTube.transform);
        SetAnchor(tubeFill, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        tubeFill.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -113);
        tubeFill.GetComponent<RectTransform>().sizeDelta = new Vector2(73, 258);
        Image fillImg = tubeFill.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.8f, 0.8f, 1f);
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Vertical;
        fillImg.fillOrigin = (int)Image.OriginVertical.Bottom;
        fillImg.fillAmount = 0f;

        GameObject progLbl = AddTMPText(centerTube, "Breeding", 20);
        progLbl.name = "ProgressLabel";
        SetAnchor(progLbl, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        progLbl.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 172);
        progLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(136, 43);

        GameObject progTxt = AddTMPText(centerTube, "0%", 20);
        progTxt.name = "ProgressText";
        SetAnchor(progTxt, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        progTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 126);
        progTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(83, 46);

        // --- Left Pedestal (Father) ---
        GameObject leftGrp = CreateUIElement("LeftPedestalGroup", content.transform);
        SetAnchor(leftGrp, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        leftGrp.GetComponent<RectTransform>().anchoredPosition = new Vector2(39, -61);
        leftGrp.GetComponent<RectTransform>().sizeDelta = new Vector2(251, 618);

        GameObject leftPedestal = CreateUIElement("LeftPedestal", leftGrp.transform);
        SetAnchor(leftPedestal, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        leftPedestal.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        leftPedestal.GetComponent<RectTransform>().sizeDelta = new Vector2(251, 351);
        leftPedestal.AddComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f, 1f);

        GameObject leftCardRoot = CreateUIElement("LeftCard_Knight", leftGrp.transform);
        SetAnchor(leftCardRoot, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        leftCardRoot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        leftCardRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(202, 290);
        Button lSelBtn = leftCardRoot.AddComponent<Button>();

        LegendOfBlood.HeroCard fatherCardComp = SetupHeroCardRoot(leftCardRoot);
        AddTMPText(leftCardRoot, "Chọn Cha", 24); // Placeholder click text overlay
        
        // --- Right Pedestal (Mother) ---
        GameObject rightGrp = CreateUIElement("RightPedestalGroup", content.transform);
        SetAnchor(rightGrp, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
        rightGrp.GetComponent<RectTransform>().anchoredPosition = new Vector2(-67, -60);
        rightGrp.GetComponent<RectTransform>().sizeDelta = new Vector2(251, 620);

        GameObject rightPedestal = CreateUIElement("RightPedestal", rightGrp.transform);
        SetAnchor(rightPedestal, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        rightPedestal.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        rightPedestal.GetComponent<RectTransform>().sizeDelta = new Vector2(251, 351);
        rightPedestal.AddComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f, 1f);

        GameObject rightCardRoot = CreateUIElement("RightCard_Mage", rightGrp.transform);
        SetAnchor(rightCardRoot, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        rightCardRoot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        rightCardRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(202, 290);
        Button rSelBtn = rightCardRoot.AddComponent<Button>();

        LegendOfBlood.HeroCard motherCardComp = SetupHeroCardRoot(rightCardRoot);
        AddTMPText(rightCardRoot, "Chọn Mẹ", 24);

        // 3. Start Breeding Button
        GameObject startBtnObj = CreateUIElement("StartBreedingButton", selectionArea.transform);
        SetAnchor(startBtnObj, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        startBtnObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-1, 430);
        startBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(391, 279);
        Button breedBtnComp = startBtnObj.AddComponent<Button>();

        GameObject sealFrame = CreateUIElement("SealFrame", startBtnObj.transform);
        SetAnchor(sealFrame, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        sealFrame.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        sealFrame.GetComponent<RectTransform>().sizeDelta = new Vector2(294, 239);
        sealFrame.AddComponent<Image>().color = new Color(0.8f, 0.3f, 0.3f, 1f);

        GameObject btnTxt = AddTMPText(startBtnObj, "LAI TẠO", 28);
        btnTxt.name = "ButtonText";
        SetAnchorCenter(btnTxt, new Vector2(0, -5), new Vector2(173, 93));

        // 4. Bottom Nav Bar
        GameObject bottomNav = CreateUIElement("BottomNavBar", root.transform);
        SetAnchor(bottomNav, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
        SetOffsets(bottomNav, 0, 0, 0, 0);
        bottomNav.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 229);
        bottomNav.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        bottomNav.AddComponent<LegendOfBlood.UIBottomNavHighlighter>();

        string[] navNames = {"Lobby", "Heroes", "Breed", "Inventory", "Shop"};
        float[] navAnchorsX = {0.1f, 0.3f, 0.5f, 0.7f, 0.9f};
        
        for (int i = 0; i < navNames.Length; i++) {
            GameObject tabObj = CreateUIElement("Tab_" + navNames[i], bottomNav.transform);
            SetAnchor(tabObj, new Vector2(navAnchorsX[i], 0), new Vector2(navAnchorsX[i], 0), new Vector2(0.5f, 0));
            tabObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);
            
            if (navNames[i] == "Breed") {
                tabObj.GetComponent<RectTransform>().sizeDelta = new Vector2(241, 244);
                tabObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 122);
            } else {
                tabObj.GetComponent<RectTransform>().sizeDelta = new Vector2(182, 198);
            }
            tabObj.AddComponent<Image>().color = new Color(0,0,0,0);
            
            tabObj.AddComponent<Button>();
            LegendOfBlood.UIPanelNavButton navBtn = tabObj.AddComponent<LegendOfBlood.UIPanelNavButton>();
            if (navNames[i] == "Lobby") navBtn.targetPanel = LegendOfBlood.UIPanelType.MainScreen;
            else if (navNames[i] == "Heroes") navBtn.targetPanel = LegendOfBlood.UIPanelType.Barrack;
            else if (navNames[i] == "Breed") navBtn.targetPanel = LegendOfBlood.UIPanelType.Breeding;
            else if (navNames[i] == "Inventory") navBtn.targetPanel = LegendOfBlood.UIPanelType.Inventory;
            else if (navNames[i] == "Shop") navBtn.targetPanel = LegendOfBlood.UIPanelType.ArenaShop;
            
            GameObject iconObj = CreateUIElement("Icon", tabObj.transform);
            SetAnchorCenter(iconObj, new Vector2(0, 20), new Vector2(100, 100));
            iconObj.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.5f);
            
            GameObject textObj = AddTMPText(tabObj, navNames[i], 24);
            SetAnchorCenter(textObj, new Vector2(0, -60), new Vector2(180, 50));
        }

        // --- MÀN HÌNH KẾT QUẢ (RESULT AREA) ---
        GameObject resultArea = CreateUIElement("ResultArea", root.transform);
        SetAnchor(resultArea, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(resultArea, 0, 0, 0, 0);
        resultArea.AddComponent<Image>().color = new Color(0,0,0,0.9f);
        resultArea.SetActive(false);

        GameObject resCardRoot = CreateUIElement("ResultCard", resultArea.transform);
        SetAnchor(resCardRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        resCardRoot.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);
        resCardRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 430);
        LegendOfBlood.HeroCard resultCardComp = SetupHeroCardRoot(resCardRoot);

        GameObject statsBox = CreateUIElement("StatsBox", resultArea.transform);
        SetAnchorCenter(statsBox, new Vector2(0, -200), new Vector2(400, 200));
        statsBox.AddComponent<Image>().color = new Color(0.2f,0.2f,0.2f, 1f);
        
        TextMeshProUGUI hpTxt = AddTMPText(statsBox, "HP: 100", 24).GetComponent<TextMeshProUGUI>();
        hpTxt.rectTransform.anchoredPosition = new Vector2(0, 70);
        TextMeshProUGUI atkTxt = AddTMPText(statsBox, "ATK: 10", 24).GetComponent<TextMeshProUGUI>();
        atkTxt.rectTransform.anchoredPosition = new Vector2(0, 30);
        TextMeshProUGUI defTxt = AddTMPText(statsBox, "DEF: 10", 24).GetComponent<TextMeshProUGUI>();
        defTxt.rectTransform.anchoredPosition = new Vector2(0, -10);
        TextMeshProUGUI spdTxt = AddTMPText(statsBox, "SPD: 10", 24).GetComponent<TextMeshProUGUI>();
        spdTxt.rectTransform.anchoredPosition = new Vector2(0, -50);
        TextMeshProUGUI potTxt = AddTMPText(statsBox, "Potential: S", 24).GetComponent<TextMeshProUGUI>();
        potTxt.rectTransform.anchoredPosition = new Vector2(0, -90);

        GameObject confirmBtnObj = CreateUIElement("ConfirmButton", resultArea.transform);
        SetAnchorCenter(confirmBtnObj, new Vector2(0, -350), new Vector2(250, 80));
        confirmBtnObj.AddComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f, 1f);
        Button confirmBtnComp = confirmBtnObj.AddComponent<Button>();
        AddTMPText(confirmBtnObj, "Confirm", 30);

        // --- Logic Mapping ---
        canvasObj.name = "Panel_Breeding";
        LegendOfBlood.BreedingUIController breedingScript = canvasObj.AddComponent<LegendOfBlood.BreedingUIController>();
        
        SerializedObject so = new SerializedObject(breedingScript);
        so.Update();

        so.FindProperty("panelTitleText").objectReferenceValue = titleTextObj.GetComponent<TextMeshProUGUI>();
        so.FindProperty("closeButton").objectReferenceValue = closeBtnComp;
        so.FindProperty("fatherSlot").objectReferenceValue = leftGrp;
        so.FindProperty("selectFatherButton").objectReferenceValue = lSelBtn;
        so.FindProperty("fatherCard").objectReferenceValue = fatherCardComp;
        
        so.FindProperty("motherSlot").objectReferenceValue = rightGrp;
        so.FindProperty("selectMotherButton").objectReferenceValue = rSelBtn;
        so.FindProperty("motherCard").objectReferenceValue = motherCardComp;

        so.FindProperty("breedButton").objectReferenceValue = breedBtnComp;
        so.FindProperty("confirmResultButton").objectReferenceValue = confirmBtnComp;
        
        so.FindProperty("selectionArea").objectReferenceValue = selectionArea;
        so.FindProperty("resultArea").objectReferenceValue = resultArea;
        
        so.FindProperty("newHeroCard_Result").objectReferenceValue = resultCardComp;
        so.FindProperty("hpText_Result").objectReferenceValue = hpTxt;
        so.FindProperty("atkText_Result").objectReferenceValue = atkTxt;
        so.FindProperty("defText_Result").objectReferenceValue = defTxt;
        so.FindProperty("spdText_Result").objectReferenceValue = spdTxt;
        so.FindProperty("potentialText_Result").objectReferenceValue = potTxt;

        // Tiến độ ống nghiệm
        so.FindProperty("progressGroup").objectReferenceValue = centerTube;
        so.FindProperty("progressFill").objectReferenceValue = tubeFill.GetComponent<Image>();
        so.FindProperty("progressText").objectReferenceValue = progTxt.GetComponent<TextMeshProUGUI>();

        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(canvasObj, "Generate Breeding Panel UI");
        Selection.activeGameObject = canvasObj;
        Debug.Log("Breeding Panel UI Generated Successfully!");
    }

    private static LegendOfBlood.HeroCard SetupHeroCardRoot(GameObject parent) {
        GameObject cFrame = CreateUIElement("CardFrame", parent.transform);
        SetAnchor(cFrame, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(cFrame, 0, 0, 0, 0);
        cFrame.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.3f, 1f);

        GameObject port = CreateUIElement("Portrait", parent.transform);
        SetAnchorCenter(port, new Vector2(0, -5), new Vector2(129, 144));
        port.AddComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1f);

        GameObject nameObj = AddTMPText(parent, "Hero Name", 20);
        nameObj.name = "NameText";
        SetAnchor(nameObj, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        nameObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -16);
        nameObj.GetComponent<RectTransform>().sizeDelta = new Vector2(101, 41);

        // Dummy text for Level to satisfy HeroCard.cs requirements
        GameObject levelObj = AddTMPText(parent, "Lv.1", 16);
        levelObj.name = "LevelText";
        SetAnchorCenter(levelObj, new Vector2(-40, -100), new Vector2(50, 30));

        // Dummy text for Combat Power
        GameObject cpObj = AddTMPText(parent, "CP: 100", 16);
        cpObj.name = "CPText";
        SetAnchorCenter(cpObj, new Vector2(30, -100), new Vector2(80, 30));

        LegendOfBlood.HeroCard cardScript = parent.AddComponent<LegendOfBlood.HeroCard>();
        SerializedObject so = new SerializedObject(cardScript);
        so.Update();
        so.FindProperty("nameText").objectReferenceValue = nameObj.GetComponent<TextMeshProUGUI>();
        so.FindProperty("levelText").objectReferenceValue = levelObj.GetComponent<TextMeshProUGUI>();
        so.FindProperty("combatPowerText").objectReferenceValue = cpObj.GetComponent<TextMeshProUGUI>();
        so.FindProperty("avatarImage").objectReferenceValue = port.GetComponent<Image>();
        so.FindProperty("cardFrame").objectReferenceValue = cFrame.GetComponent<Image>();
        so.ApplyModifiedProperties();

        return cardScript;
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


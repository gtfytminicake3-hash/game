#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecruitmentPanelGenerator : EditorWindow
{
    [MenuItem("UI Tools/Generate Recruitment Panel")]
    public static void GenerateUI()
    {
        // 0. Cleanup
        var oldPanels = Object.FindObjectsByType<LegendOfBlood.RecruitmentPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in oldPanels) {
            Undo.DestroyObjectImmediate(p.gameObject);
        }

        GameObject canvasObj = new GameObject("Recruitment_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // 1. Root
        GameObject screenRoot = CreateUIElement("ScreenRoot", canvasObj.transform);
        SetAnchor(screenRoot, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(screenRoot, 0, 0, 0, 0);
        if (System.Type.GetType("SafeArea") != null) {
            screenRoot.AddComponent(System.Type.GetType("SafeArea"));
        }

        // 2. FullBackground
        GameObject fullBg = CreateUIElement("Background", screenRoot.transform);
        SetAnchor(fullBg, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(fullBg, 0, 0, 0, 0);
        fullBg.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 1f);

        // === TOP BAR: Resource Display ===
        GameObject topBar = CreateUIElement("TopBar", screenRoot.transform);
        SetAnchor(topBar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        topBar.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 80);
        topBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -20);
        
        // Ticket Count
        GameObject ticketObj = CreateUIElement("TicketCount", topBar.transform);
        SetAnchor(ticketObj, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        ticketObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(30, 0);
        ticketObj.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 50);
        var ticketTMP = AddTMPText(ticketObj, "🎫 0", 24);
        ticketTMP.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        // Diamond Count
        GameObject diamondObj = CreateUIElement("DiamondCount", topBar.transform);
        SetAnchor(diamondObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        diamondObj.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 50);
        var diamondTMP = AddTMPText(diamondObj, "💎 0", 24);
        diamondTMP.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Population Count
        GameObject popObj = CreateUIElement("PopulationCount", topBar.transform);
        SetAnchor(popObj, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
        popObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-30, 0);
        popObj.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 50);
        var popTMP = AddTMPText(popObj, "👥 0/50", 24);
        popTMP.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

        // 3. CloseButton
        GameObject closeBtn = CreateUIElement("CloseButton", screenRoot.transform);
        SetAnchor(closeBtn, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));
        closeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-27, -110);
        closeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(96, 96);
        closeBtn.AddComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);
        Button closeBtnComp = closeBtn.AddComponent<Button>();
        AddTMPText(closeBtn, "X", 50);

        // 4. VortexEffectArea
        GameObject vortexArea = CreateUIElement("VortexEffectArea", screenRoot.transform);
        SetAnchor(vortexArea, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        vortexArea.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -200);
        vortexArea.GetComponent<RectTransform>().sizeDelta = new Vector2(766, 900);
        vortexArea.AddComponent<Image>().color = new Color(0.1f, 0.0f, 0.2f, 0.5f);

        GameObject smokeShadowBg = CreateUIElement("SmokeShadowBg", vortexArea.transform);
        SetAnchor(smokeShadowBg, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f)); SetOffsets(smokeShadowBg,0,0,0,0);
        smokeShadowBg.AddComponent<Image>().color = new Color(0,0,0, 0.8f);

        GameObject runeRing = CreateUIElement("RuneRing", vortexArea.transform);
        SetAnchor(runeRing, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f)); SetOffsets(runeRing,50,50,50,50);
        runeRing.AddComponent<Image>().color = new Color(0.4f, 0.2f, 0.6f, 1f);

        GameObject blackHoleCore = CreateUIElement("BlackHoleCore", vortexArea.transform);
        SetAnchor(blackHoleCore, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        blackHoleCore.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 300);
        blackHoleCore.AddComponent<Image>().color = Color.black;

        // 5. BottomActionArea
        GameObject bottomActionArea = CreateUIElement("BottomActionArea", screenRoot.transform);
        SetAnchor(bottomActionArea, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
        bottomActionArea.GetComponent<RectTransform>().offsetMin = new Vector2(46, 158);
        bottomActionArea.GetComponent<RectTransform>().offsetMax = new Vector2(-53, 785);

        // ================= SUMMON x1 =================
        GameObject singleCard = CreateUIElement("SummonCard_Single", bottomActionArea.transform);
        SetAnchor(singleCard, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0));
        singleCard.GetComponent<RectTransform>().anchoredPosition = new Vector2(51, 272);
        singleCard.GetComponent<RectTransform>().sizeDelta = new Vector2(253, 478);
        Button singleBtnComp = singleCard.AddComponent<Button>(); 

        GameObject scFrame = CreateUIElement("Frame", singleCard.transform);
        SetAnchor(scFrame, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        scFrame.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        scFrame.GetComponent<RectTransform>().sizeDelta = new Vector2(253, 350);
        scFrame.AddComponent<Image>().color = new Color(0.2f, 0.3f, 0.2f, 1f);

        GameObject bookIcon = CreateUIElement("BookIcon", singleCard.transform);
        SetAnchor(bookIcon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        bookIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(-23, 43);
        bookIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(133, 187);
        bookIcon.AddComponent<Image>().color = new Color(0.6f, 0.4f, 0.2f, 1f);

        // Price Label x1
        GameObject priceOneLabelObj = CreateUIElement("PriceOneLabel", singleCard.transform);
        SetAnchor(priceOneLabelObj, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        priceOneLabelObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);
        priceOneLabelObj.GetComponent<RectTransform>().sizeDelta = new Vector2(230, 40);
        AddTMPText(priceOneLabelObj, "1 vé / 100 💎", 18);

        GameObject tTxt = CreateUIElement("TitleText", singleCard.transform);
        SetAnchor(tTxt, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        tTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 7);
        tTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 42);
        AddTMPText(tTxt, "Summon x1", 24);

        // ================= SUMMON x10 (CENTER) =================
        GameObject tenCard = CreateUIElement("SummonPortal_Center", bottomActionArea.transform);
        SetAnchor(tenCard, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        tenCard.GetComponent<RectTransform>().anchoredPosition = new Vector2(7, 182);
        tenCard.GetComponent<RectTransform>().sizeDelta = new Vector2(430, 680);

        GameObject outerGlow = CreateUIElement("OuterGlow", tenCard.transform);
        SetAnchor(outerGlow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        outerGlow.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 65);
        outerGlow.GetComponent<RectTransform>().sizeDelta = new Vector2(414, 490);
        outerGlow.AddComponent<Image>().color = new Color(0.8f, 0.6f, 0.1f, 0.3f);

        GameObject pFrame = CreateUIElement("PortalFrame", tenCard.transform);
        SetAnchor(pFrame, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        pFrame.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 55);
        pFrame.GetComponent<RectTransform>().sizeDelta = new Vector2(354, 471);
        pFrame.AddComponent<Image>().color = new Color(0.3f, 0.2f, 0.1f, 1f);

        GameObject pCore = CreateUIElement("PortalCore", tenCard.transform);
        SetAnchor(pCore, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        pCore.GetComponent<RectTransform>().anchoredPosition = new Vector2(-1, 84);
        pCore.GetComponent<RectTransform>().sizeDelta = new Vector2(243, 245);
        pCore.AddComponent<Image>().color = new Color(1f, 0.4f, 0.1f, 1f);

        // Price Label x10
        GameObject priceTenLabelObj = CreateUIElement("PriceTenLabel", tenCard.transform);
        SetAnchor(priceTenLabelObj, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        priceTenLabelObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 160);
        priceTenLabelObj.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 40);
        AddTMPText(priceTenLabelObj, "10 vé / 900 💎", 20);

        GameObject btnBase = CreateUIElement("ButtonBase", tenCard.transform);
        SetAnchor(btnBase, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        btnBase.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 60);
        btnBase.GetComponent<RectTransform>().sizeDelta = new Vector2(278, 78);
        btnBase.AddComponent<Image>().color = new Color(0.8f, 0.2f, 0.1f, 1f);
        Button tenBtnComp = btnBase.AddComponent<Button>();

        GameObject btnText = CreateUIElement("ButtonText", tenCard.transform);
        SetAnchor(btnText, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        btnText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 78);
        btnText.GetComponent<RectTransform>().sizeDelta = new Vector2(240, 46);
        AddTMPText(btnText, "SUMMON x10", 30);

        // ================= FREE SUMMON (RIGHT) =================
        GameObject freeCard = CreateUIElement("FreeSummonCard_Right", bottomActionArea.transform);
        SetAnchor(freeCard, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0));
        freeCard.GetComponent<RectTransform>().anchoredPosition = new Vector2(-90, 286);
        freeCard.GetComponent<RectTransform>().sizeDelta = new Vector2(256, 499);
        Button freeBtnComp = freeCard.AddComponent<Button>();

        GameObject cGlow = CreateUIElement("CardGlow", freeCard.transform);
        SetAnchor(cGlow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        cGlow.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 69);
        cGlow.GetComponent<RectTransform>().sizeDelta = new Vector2(248, 350);
        cGlow.AddComponent<Image>().color = new Color(0.8f, 0.8f, 0.2f, 0.4f);

        GameObject fFrame = CreateUIElement("Frame", freeCard.transform);
        SetAnchor(fFrame, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        fFrame.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 76);
        fFrame.GetComponent<RectTransform>().sizeDelta = new Vector2(216, 325);
        fFrame.AddComponent<Image>().color = new Color(0.2f, 0.3f, 0.4f, 1f);

        GameObject vidIcon = CreateUIElement("VideoIcon", freeCard.transform);
        SetAnchor(vidIcon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        vidIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(-2, 81);
        vidIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(119, 118);
        vidIcon.AddComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f, 1f);

        GameObject fTxt = CreateUIElement("TitleText", freeCard.transform);
        SetAnchor(fTxt, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        fTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 49);
        fTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(194, 42);
        AddTMPText(fTxt, "Free Summon", 20);

        // ================= RESULT OVERLAY =================
        GameObject resultOverlay = CreateUIElement("ResultOverlay", screenRoot.transform);
        SetAnchor(resultOverlay, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(resultOverlay, 0, 0, 0, 0);

        // Dark backdrop
        Image overlayBg = resultOverlay.AddComponent<Image>();
        overlayBg.color = new Color(0, 0, 0, 0.85f);

        // Result Title
        GameObject resultTitle = CreateUIElement("ResultTitle", resultOverlay.transform);
        SetAnchor(resultTitle, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        resultTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -80);
        resultTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 80);
        var resultTitleTMP = AddTMPText(resultTitle, "✨ CHIÊU MỘ THÀNH CÔNG ✨", 36);
        resultTitleTMP.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.84f, 0f);

        // Result Card Container (ScrollView area)
        GameObject scrollArea = CreateUIElement("ResultScrollArea", resultOverlay.transform);
        SetAnchor(scrollArea, new Vector2(0, 0.15f), new Vector2(1, 0.85f), new Vector2(0.5f, 0.5f));
        SetOffsets(scrollArea, 40, 40, 0, 0);
        
        // Content container with HorizontalLayoutGroup
        GameObject cardContainer = CreateUIElement("ResultCardContainer", scrollArea.transform);
        SetAnchor(cardContainer, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(cardContainer, 0, 0, 0, 0);
        var hlg = cardContainer.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 15;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.padding = new RectOffset(20, 20, 10, 10);

        // Close Result Button
        GameObject closeResultBtn = CreateUIElement("CloseResultButton", resultOverlay.transform);
        SetAnchor(closeResultBtn, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        closeResultBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);
        closeResultBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 80);
        closeResultBtn.AddComponent<Image>().color = new Color(0.2f, 0.6f, 0.3f, 1f);
        Button closeResultBtnComp = closeResultBtn.AddComponent<Button>();
        AddTMPText(closeResultBtn, "XÁC NHẬN", 28);

        resultOverlay.SetActive(false);

        // --- Logic Mapping ---
        canvasObj.name = "Panel_Recruitment";
        LegendOfBlood.RecruitmentPanel recScript = canvasObj.AddComponent<LegendOfBlood.RecruitmentPanel>();
        recScript.PanelType = LegendOfBlood.UIPanelType.Recruitment;

        SerializedObject so = new SerializedObject(recScript);
        so.Update();

        so.FindProperty("closeButton").objectReferenceValue = closeBtnComp;
        so.FindProperty("recruitOneButton").objectReferenceValue = singleBtnComp;
        so.FindProperty("recruitTenButton").objectReferenceValue = tenBtnComp;
        so.FindProperty("recruitAdButton").objectReferenceValue = freeBtnComp;

        // New fields
        so.FindProperty("ticketCountText").objectReferenceValue = ticketObj.GetComponentInChildren<TextMeshProUGUI>();
        so.FindProperty("diamondCountText").objectReferenceValue = diamondObj.GetComponentInChildren<TextMeshProUGUI>();
        so.FindProperty("populationText").objectReferenceValue = popObj.GetComponentInChildren<TextMeshProUGUI>();
        so.FindProperty("priceOneText").objectReferenceValue = priceOneLabelObj.GetComponentInChildren<TextMeshProUGUI>();
        so.FindProperty("priceTenText").objectReferenceValue = priceTenLabelObj.GetComponentInChildren<TextMeshProUGUI>();
        so.FindProperty("resultOverlay").objectReferenceValue = resultOverlay;
        so.FindProperty("resultCardContainer").objectReferenceValue = cardContainer.transform;
        so.FindProperty("resultCloseButton").objectReferenceValue = closeResultBtnComp;
        so.FindProperty("resultTitleText").objectReferenceValue = resultTitle.GetComponentInChildren<TextMeshProUGUI>();

        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(canvasObj, "Generate Recruitment Panel UI");
        Selection.activeGameObject = canvasObj;
        Debug.Log("Recruitment Panel UI Generated Successfully!");
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


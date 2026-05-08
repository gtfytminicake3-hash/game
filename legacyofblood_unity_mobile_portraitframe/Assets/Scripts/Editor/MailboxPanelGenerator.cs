#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MailboxPanelGenerator : EditorWindow
{
    [MenuItem("UI Tools/Generate Mailbox Panel")]
    public static void GenerateUI()
    {
        // 0. Cleanup
        var oldPanels = Object.FindObjectsByType<LegendOfBlood.MailboxPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in oldPanels) {
            Undo.DestroyObjectImmediate(p.gameObject);
        }

        GameObject canvasObj = new GameObject("Mailbox_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // 1. Root
        GameObject screenRoot = CreateUIElement("MailboxRoot", canvasObj.transform);
        SetAnchor(screenRoot, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(screenRoot, 0, 0, 0, 0);
        if (System.Type.GetType("SafeArea") != null) {
            screenRoot.AddComponent(System.Type.GetType("SafeArea"));
        }

        GameObject bg = CreateUIElement("BackgroundScene", screenRoot.transform);
        SetAnchor(bg, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(bg, 0, 0, 0, 0);
        bg.AddComponent<Image>().color = new Color(0.1f, 0.15f, 0.2f, 1f);

        // 2. TopHeader
        GameObject topHeader = CreateUIElement("TopHeader", screenRoot.transform);
        SetAnchor(topHeader, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        SetOffsets(topHeader, 76, 68, 30, -533); 

        GameObject tBanner = CreateUIElement("TitleBanner", topHeader.transform);
        SetAnchor(tBanner, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        tBanner.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -29);
        tBanner.GetComponent<RectTransform>().sizeDelta = new Vector2(613, 115);
        tBanner.AddComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);

        GameObject tText = CreateUIElement("TitleText", topHeader.transform);
        SetAnchor(tText, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        tText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -43);
        tText.GetComponent<RectTransform>().sizeDelta = new Vector2(315, 67);
        GameObject tmpTitle = AddTMPText(tText, "MAILBOX", 45);

        GameObject closeBtn = CreateUIElement("CloseButton", topHeader.transform);
        SetAnchor(closeBtn, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));
        closeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-8, -19);
        closeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(159, 165);
        closeBtn.AddComponent<Image>().color = new Color(0.8f, 0.8f, 0.2f, 1f);
        Button closeBtnComp = closeBtn.AddComponent<Button>();
        AddTMPText(closeBtn, "X", 60);

        GameObject claimCrest = CreateUIElement("ClaimAllCrest", topHeader.transform);
        SetAnchor(claimCrest, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        claimCrest.GetComponent<RectTransform>().anchoredPosition = new Vector2(6, -136);
        claimCrest.GetComponent<RectTransform>().sizeDelta = new Vector2(486, 282);

        GameObject crestGlow = CreateUIElement("GlowBack", claimCrest.transform);
        SetAnchor(crestGlow, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(crestGlow, 8, 8, 0, 17);
        crestGlow.AddComponent<Image>().color = new Color(0.8f, 0.6f, 0.1f, 0.5f);

        GameObject crestFrame = CreateUIElement("CrestFrame", claimCrest.transform);
        SetAnchor(crestFrame, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        crestFrame.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -4);
        crestFrame.GetComponent<RectTransform>().sizeDelta = new Vector2(438, 230);
        crestFrame.AddComponent<Image>().color = new Color(0.3f, 0.2f, 0.1f, 1f);

        GameObject claimAllBtnObj = CreateUIElement("ClaimAllButton", claimCrest.transform);
        SetAnchor(claimAllBtnObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        claimAllBtnObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 28);
        claimAllBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(336, 113);
        claimAllBtnObj.AddComponent<Image>().color = new Color(0.8f, 0.2f, 0.1f, 1f);
        Button claimAllBtnComp = claimAllBtnObj.AddComponent<Button>();
        GameObject tmpClaimAll = AddTMPText(claimAllBtnObj, "CLAIM ALL", 40);

        // 3. MailListPanel
        GameObject mailListPanel = CreateUIElement("MailListPanel", screenRoot.transform);
        SetAnchor(mailListPanel, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
        SetOffsets(mailListPanel, 96, 84, 550, 8);
        
        ScrollRect sr = mailListPanel.AddComponent<ScrollRect>();
        sr.horizontal = false; sr.vertical = true;
        
        GameObject scrollViewport = CreateUIElement("ScrollViewport", mailListPanel.transform);
        SetAnchor(scrollViewport, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(scrollViewport, 1, 35, 3, 9);
        scrollViewport.AddComponent<Image>().color = new Color(0,0,0,0.2f);
        scrollViewport.AddComponent<RectMask2D>();
        sr.viewport = scrollViewport.GetComponent<RectTransform>();

        GameObject scrollBarTrack = CreateUIElement("ScrollBarTrack", mailListPanel.transform);
        SetAnchor(scrollBarTrack, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f));
        scrollBarTrack.GetComponent<RectTransform>().anchoredPosition = new Vector2(-6, 0);
        scrollBarTrack.GetComponent<RectTransform>().sizeDelta = new Vector2(15, 0);
        SetOffsets(scrollBarTrack, 0, 0, 0, 0);
        scrollBarTrack.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);

        GameObject content = CreateUIElement("Content", scrollViewport.transform);
        SetAnchor(content, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        SetOffsets(content, 0, 0, 0, 0);
        sr.content = content.GetComponent<RectTransform>();

        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 20;
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childControlWidth = true; vlg.childControlHeight = true;
        vlg.childForceExpandWidth = false; vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // --- Prefab Generation ---
        GameObject templateRow = CreateMailRowPrefab("MailRow_Template", content.transform);
        string prefabPath = "Assets/Prefabs/MailRow_Auto.prefab";
        if (!System.IO.Directory.Exists("Assets/Prefabs")) System.IO.Directory.CreateDirectory("Assets/Prefabs");
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(templateRow, prefabPath);

        // 5 Mocks
        for (int i=1; i<=5; i++) {
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(savedPrefab, content.transform);
            inst.name = "MailRow_0" + i;
        }
        Undo.DestroyObjectImmediate(templateRow);

        canvasObj.name = "Panel_Mailbox";
        LegendOfBlood.MailboxPanel mbScript = canvasObj.AddComponent<LegendOfBlood.MailboxPanel>();
        mbScript.PanelType = LegendOfBlood.UIPanelType.Mailbox;

        SerializedObject so = new SerializedObject(mbScript);
        so.Update();

        so.FindProperty("panelTitleText").objectReferenceValue = tmpTitle.GetComponent<TextMeshProUGUI>();
        so.FindProperty("closeButton").objectReferenceValue = closeBtnComp;
        so.FindProperty("claimAllButtonText").objectReferenceValue = tmpClaimAll.GetComponent<TextMeshProUGUI>();
        so.FindProperty("claimAllButton").objectReferenceValue = claimAllBtnComp;
        so.FindProperty("reportItemsContainer").objectReferenceValue = content.transform;
        
        if (savedPrefab != null) {
            so.FindProperty("reportItemPrefab").objectReferenceValue = savedPrefab;
        }

        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(canvasObj, "Generate Mailbox Panel");
        Selection.activeGameObject = canvasObj;
        Debug.Log("Mailbox Panel Generated!");
    }

    private static GameObject CreateMailRowPrefab(string name, Transform parent) {
        GameObject row = CreateUIElement(name, parent);
        LayoutElement le = row.AddComponent<LayoutElement>();
        le.preferredWidth = 864;
        le.preferredHeight = 184;
        row.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f); // Nền report gỗ

        LegendOfBlood.ExpeditionReportItem itemScript = row.AddComponent<LegendOfBlood.ExpeditionReportItem>();
        SerializedObject soItem = new SerializedObject(itemScript);

        // Icon Panel
        GameObject iconPan = CreateUIElement("IconPanel", row.transform);
        SetAnchor(iconPan, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f));
        SetOffsets(iconPan, 0, -200, 0, 0); // Width 200
        iconPan.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 1f);

        GameObject iconImg = CreateUIElement("MainIcon", iconPan.transform);
        SetAnchor(iconImg, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        iconImg.GetComponent<RectTransform>().sizeDelta = new Vector2(134, 125);
        iconImg.AddComponent<Image>().color = new Color(0.5f, 0.1f, 0.1f, 1f); // Sọ đỏ

        // Text Panel
        GameObject txtPan = CreateUIElement("TextPanel", row.transform);
        SetAnchor(txtPan, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
        SetOffsets(txtPan, 200, 250, 0, 0); // Left 200, Right 250 (để trống 250 cho buttons)

        GameObject lblTitle = CreateUIElement("TitleText", txtPan.transform);
        SetAnchor(lblTitle, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1));
        SetOffsets(lblTitle, 10, 0, 10, -50); // Height 40
        GameObject tpoLT = AddTMPText(lblTitle, "Victory: Castle Siege", 28);
        tpoLT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.TopLeft;
        soItem.FindProperty("poiNameText").objectReferenceValue = tpoLT.GetComponent<TextMeshProUGUI>();

        GameObject lblOutcome = CreateUIElement("OutcomeText", txtPan.transform);
        SetAnchor(lblOutcome, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1));
        SetOffsets(lblOutcome, 10, 0, 60, -90); // Height 30
        GameObject tpoOut = AddTMPText(lblOutcome, "Victory", 22);
        tpoOut.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.TopLeft;
        tpoOut.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 0.8f, 0.4f, 1f);
        soItem.FindProperty("outcomeText").objectReferenceValue = tpoOut.GetComponent<TextMeshProUGUI>();

        GameObject lblRewards = CreateUIElement("RewardText", txtPan.transform);
        SetAnchor(lblRewards, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1));
        SetOffsets(lblRewards, 10, 0, 100, -170); // Tiết kiệm chỗ
        GameObject tpoRw = AddTMPText(lblRewards, "100 Gold, 50 Wood", 20);
        tpoRw.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.TopLeft;
        soItem.FindProperty("rewardsText").objectReferenceValue = tpoRw.GetComponent<TextMeshProUGUI>();

        // Button Panel
        GameObject btnPan = CreateUIElement("ButtonPanel", row.transform);
        SetAnchor(btnPan, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f));
        SetOffsets(btnPan, -250, 0, 0, 0); // Right side, Width 250
        
        GridLayoutGroup glg = btnPan.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(110, 80);
        glg.spacing = new Vector2(10, 10);
        glg.padding = new RectOffset(10, 10, 10, 10);

        // Nút Claim
        GameObject btnClm = CreateUIElement("ClaimBtn", btnPan.transform);
        btnClm.AddComponent<Image>().color = new Color(0.2f, 0.6f, 0.2f, 1f);
        Button bcClm = btnClm.AddComponent<Button>();
        soItem.FindProperty("claimButton").objectReferenceValue = bcClm;
        GameObject claimText = AddTMPText(btnClm, "Nhận", 22);
        soItem.FindProperty("claimButtonText").objectReferenceValue = claimText.GetComponent<TextMeshProUGUI>();

        // Nút X2
        GameObject btnX2 = CreateUIElement("ClaimX2Btn", btnPan.transform);
        btnX2.AddComponent<Image>().color = new Color(0.8f, 0.6f, 0.1f, 1f);
        Button bcX2 = btnX2.AddComponent<Button>();
        soItem.FindProperty("claimX2Button").objectReferenceValue = bcX2;
        AddTMPText(btnX2, "Nhận X2", 18);

        // Nút Replay
        GameObject btnRep = CreateUIElement("ReplayBtn", btnPan.transform);
        btnRep.AddComponent<Image>().color = new Color(0.2f, 0.4f, 0.8f, 1f);
        Button bcRep = btnRep.AddComponent<Button>();
        soItem.FindProperty("replayButton").objectReferenceValue = bcRep;
        GameObject repText = AddTMPText(btnRep, "Replay", 18);
        soItem.FindProperty("replayButtonText").objectReferenceValue = repText.GetComponent<TextMeshProUGUI>();

        // Nút Revive
        GameObject btnRev = CreateUIElement("ReviveBtn", btnPan.transform);
        btnRev.AddComponent<Image>().color = new Color(0.6f, 0.2f, 0.6f, 1f);
        Button bcRev = btnRev.AddComponent<Button>();
        soItem.FindProperty("reviveRetryButton").objectReferenceValue = bcRev;
        AddTMPText(btnRev, "Phục Thù", 18);

        soItem.ApplyModifiedProperties();
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


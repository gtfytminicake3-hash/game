#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingUpgradePanelGenerator : EditorWindow
{
    [MenuItem("UI Tools/Generate Building Upgrade Panel")]
    public static void GenerateUI()
    {
        // 0. Cleanup
        var oldPanels = Object.FindObjectsByType<LegendOfBlood.BuildingUpgradePanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in oldPanels) {
            Undo.DestroyObjectImmediate(p.gameObject);
        }

        GameObject canvasObj = new GameObject("Panel_BuildingUpgrade");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50; // Đặt lên cao vì đây là Popup Modal
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        canvasObj.AddComponent<GraphicRaycaster>();

        // 1. Root
        GameObject screenRoot = CreateUIElement("UpgradeBarracksRoot", canvasObj.transform);
        SetAnchor(screenRoot, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(screenRoot, 0, 0, 0, 0);

        GameObject bgLobby = CreateUIElement("BackgroundLobby", screenRoot.transform);
        SetAnchor(bgLobby, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(bgLobby, 0, 0, 0, 0);
        bgLobby.AddComponent<Image>().color = new Color(0, 0, 0, 0); // Vô hình hoặc mờ

        GameObject dimOverlay = CreateUIElement("DimOverlay", screenRoot.transform);
        SetAnchor(dimOverlay, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(dimOverlay, 0, 0, 0, 0);
        dimOverlay.AddComponent<Image>().color = new Color(0, 0, 0, 0.5f);

        // 2. UpgradeModal
        GameObject upgradeModal = CreateUIElement("UpgradeModal", screenRoot.transform);
        SetAnchor(upgradeModal, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        upgradeModal.GetComponent<RectTransform>().anchoredPosition = new Vector2(-3, 1);
        upgradeModal.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 1330);

        GameObject modalFrame = CreateUIElement("ModalFrame", upgradeModal.transform);
        SetAnchor(modalFrame, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(modalFrame, 0, 0, 0, 0);
        modalFrame.AddComponent<Image>().color = new Color(0.18f, 0.14f, 0.1f, 1f); // Nền gỗ

        // 3. CloseButton
        GameObject closeBtn = CreateUIElement("CloseButton", upgradeModal.transform);
        SetAnchor(closeBtn, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
        closeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-3, 0);
        closeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(95, 102);
        closeBtn.AddComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);
        Button closeBtnComp = closeBtn.AddComponent<Button>();
        AddTMPText(closeBtn, "X", 40);

        // 4. TitleBar
        GameObject titleBar = CreateUIElement("TitleBar", upgradeModal.transform);
        SetAnchor(titleBar, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        titleBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(22, -14);
        titleBar.GetComponent<RectTransform>().sizeDelta = new Vector2(662, 133);
        titleBar.AddComponent<Image>().color = new Color(0.3f, 0.2f, 0.15f, 1f);

        GameObject titleTextObj = CreateUIElement("TitleText", titleBar.transform);
        SetAnchor(titleTextObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        titleTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -2);
        titleTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(360, 65);
        TextMeshProUGUI tmpTitle = AddTMPText(titleTextObj, "Upgrade Barracks", 45).GetComponent<TextMeshProUGUI>();

        // 5. LevelInfoBlock
        GameObject levelInfo = CreateUIElement("LevelInfoBlock", upgradeModal.transform);
        SetAnchor(levelInfo, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        levelInfo.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -169);
        levelInfo.GetComponent<RectTransform>().sizeDelta = new Vector2(498, 178);

        GameObject levelTextObj = CreateUIElement("LevelText", levelInfo.transform);
        SetAnchor(levelTextObj, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        levelTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -8);
        levelTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(374, 69);
        TextMeshProUGUI tmpInfo = AddTMPText(levelTextObj, "Lv 1 -> Lv 2", 36).GetComponent<TextMeshProUGUI>();

        GameObject capTextObj = CreateUIElement("CapacityText", levelInfo.transform);
        SetAnchor(capTextObj, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        capTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -85);
        capTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(414, 52);
        TextMeshProUGUI tmpBenefit = AddTMPText(capTextObj, "Capacity: 50 -> 55", 28).GetComponent<TextMeshProUGUI>();
        tmpBenefit.color = new Color(0.2f, 0.8f, 0.2f, 1f);

        // 6. CostParchment
        GameObject costParchment = CreateUIElement("CostParchment", upgradeModal.transform);
        SetAnchor(costParchment, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        costParchment.GetComponent<RectTransform>().anchoredPosition = new Vector2(3, 32);
        costParchment.GetComponent<RectTransform>().sizeDelta = new Vector2(556, 525);

        GameObject parchBg = CreateUIElement("ParchmentBg", costParchment.transform);
        SetAnchor(parchBg, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(parchBg, 0, 0, 0, 0);
        parchBg.AddComponent<Image>().color = new Color(0.9f, 0.85f, 0.7f, 1f); // Giấy da

        // Tạo 3 row tượng trưng, và một cái list text Requirements gộp chung cho dễ map với Script cũ
        GameObject goldRow = CreateCostRow("GoldRow", costParchment.transform, -38, 261, 49, new Color(0.9f, 0.8f, 0.2f, 1f));
        GameObject woodRow = CreateCostRow("WoodRow", costParchment.transform, -104, 232, 48, new Color(0.4f, 0.2f, 0.1f, 1f));
        GameObject stoneRow = CreateCostRow("StoneRow", costParchment.transform, -166, 239, 47, new Color(0.5f, 0.5f, 0.5f, 1f));
        // Requirement Text map thẳng vào thông báo Cost của code cũ
        GameObject reqTextObj = CreateUIElement("RequirementText", costParchment.transform);
        SetAnchor(reqTextObj, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        reqTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 57);
        reqTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(403, 111);
        TextMeshProUGUI tmpCostLog = AddTMPText(reqTextObj, "Vàng: 100\nGỗ: 50\nĐá: 20", 24).GetComponent<TextMeshProUGUI>();
        tmpCostLog.color = new Color(0.2f, 0.1f, 0.1f, 1f);

        // 7. UpgradeButton
        GameObject upgBtn = CreateUIElement("UpgradeButton", upgradeModal.transform);
        SetAnchor(upgBtn, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        upgBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 208);
        upgBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(509, 170);

        GameObject btnFrame = CreateUIElement("ButtonFrame", upgBtn.transform);
        SetAnchor(btnFrame, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(btnFrame, 0, 0, 0, 0);
        btnFrame.AddComponent<Image>().color = new Color(0.2f, 0.6f, 0.2f, 1f);
        Button upgBtnComp = upgBtn.AddComponent<Button>();

        GameObject btnTextObj = CreateUIElement("ButtonText", upgBtn.transform);
        SetAnchor(btnTextObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        btnTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        btnTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 63);
        AddTMPText(btnTextObj, "UPGRADE", 40);

        // 8. Chức năng mở rộng để map vừa in 100% với BuildingUpgradePanel.cs
        GameObject timerTextObj = CreateUIElement("UpgradeTimerText", upgradeModal.transform);
        SetAnchor(timerTextObj, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        timerTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);
        timerTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 50);
        TextMeshProUGUI tmpTimer = AddTMPText(timerTextObj, "Time Left: 00:00", 30).GetComponent<TextMeshProUGUI>();
        tmpTimer.color = new Color(1f, 0.8f, 0.2f, 1f);

        GameObject adBtnObj = CreateUIElement("SpeedUpAdButton", upgradeModal.transform);
        SetAnchor(adBtnObj, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        adBtnObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 30);
        adBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 60);
        adBtnObj.AddComponent<Image>().color = new Color(0.5f, 0.3f, 0.8f, 1f);
        Button adBtnComp = adBtnObj.AddComponent<Button>();
        AddTMPText(adBtnObj, "SPEED UP (AD)", 20);

        // --- Logic Mapping ---
        canvasObj.name = "Panel_BuildingUpgrade";
        LegendOfBlood.UIPanel uiPan = canvasObj.AddComponent<LegendOfBlood.BuildingUpgradePanel>();
        uiPan.PanelType = LegendOfBlood.UIPanelType.BuildingUpgrade;
        LegendOfBlood.BuildingUpgradePanel buScript = uiPan as LegendOfBlood.BuildingUpgradePanel;

        SerializedObject so = new SerializedObject(buScript);
        so.Update();

        so.FindProperty("closeButton").objectReferenceValue = closeBtnComp;
        so.FindProperty("upgradeButton").objectReferenceValue = upgBtnComp;
        so.FindProperty("titleText").objectReferenceValue = tmpTitle;
        so.FindProperty("infoText").objectReferenceValue = tmpInfo;
        so.FindProperty("costText").objectReferenceValue = tmpCostLog;
        so.FindProperty("upgradeTimerText").objectReferenceValue = tmpTimer;
        so.FindProperty("benefitText").objectReferenceValue = tmpBenefit;
        so.FindProperty("speedUpAdButton").objectReferenceValue = adBtnComp;

        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(canvasObj, "Generate Building Upgrade Panel");
        Selection.activeGameObject = canvasObj;
        Debug.Log("Building Upgrade Panel Generated Succesfully!");
    }

    private static GameObject CreateCostRow(string name, Transform parent, float yPos, float width, float height, Color iconColor) {
        GameObject row = CreateUIElement(name, parent);
        SetAnchor(row, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        row.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos);
        row.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);

        GameObject icon = CreateUIElement("Icon", row.transform);
        SetAnchor(icon, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        icon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        icon.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
        icon.AddComponent<Image>().color = iconColor;

        GameObject textObj = CreateUIElement("ValueText", row.transform);
        SetAnchor(textObj, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        textObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(52, 0);
        textObj.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 48);
        TextMeshProUGUI tmp = AddTMPText(textObj, "[Value]", 24).GetComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.color = new Color(0.2f, 0.1f, 0.1f, 1f);
        
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


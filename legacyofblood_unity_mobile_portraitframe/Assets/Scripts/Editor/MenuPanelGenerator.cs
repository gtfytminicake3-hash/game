#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuPanelGenerator : EditorWindow
{
    [MenuItem("UI Tools/Generate Menu Panel")]
    public static void GenerateUI()
    {
        // 0. Cleanup old panels
        var oldPanels = Object.FindObjectsByType<LegendOfBlood.MenuPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in oldPanels) {
            Undo.DestroyObjectImmediate(p.gameObject);
        }

        GameObject canvasObj = new GameObject("Menu_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50; // Menu thường đè lên mọi thứ khác
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // 1. Root
        GameObject screenRoot = CreateUIElement("MenuOverlayRoot", canvasObj.transform);
        SetAnchor(screenRoot, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(screenRoot, 0, 0, 0, 0);
        if (System.Type.GetType("SafeArea") != null) {
            screenRoot.AddComponent(System.Type.GetType("SafeArea"));
        }

        // 2. DimmedMainScreenLeft (hoặc DimLayer full màn tuỳ ý)
        GameObject dimLayer = CreateUIElement("DimmedMainScreenLeft", screenRoot.transform);
        SetAnchor(dimLayer, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f)); // Full màn mờ như gợi ý của user
        SetOffsets(dimLayer, 0, 0, 0, 0);
        dimLayer.AddComponent<Image>().color = new Color(0, 0, 0, 0.45f); // Đen mờ 45%

        // 3. RightMenuPanel
        GameObject rightPanel = CreateUIElement("RightMenuPanel", screenRoot.transform);
        SetAnchor(rightPanel, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f));
        rightPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        rightPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(663, 1920);
        rightPanel.AddComponent<Image>().color = new Color(0.15f, 0.12f, 0.1f, 1f); // Khung gỗ mộc

        // 4. CloseButton
        GameObject closeBtn = CreateUIElement("CloseButton", rightPanel.transform);
        SetAnchor(closeBtn, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
        closeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(8, -20);
        closeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(96, 100);
        closeBtn.AddComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);
        Button closeBtnComp = closeBtn.AddComponent<Button>();
        AddTMPText(closeBtn, "X", 50);

        // 5. MenuListContainer
        GameObject menuListContainer = CreateUIElement("MenuListContainer", rightPanel.transform);
        SetAnchor(menuListContainer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        menuListContainer.GetComponent<RectTransform>().anchoredPosition = new Vector2(15, -21);
        menuListContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(391, 1503);

        VerticalLayoutGroup vlg = menuListContainer.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.spacing = 106;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        // 6. 6 Menu Items
        Button btnBackpack = CreateMenuItem("MenuItem_Backpack", "Backpack", menuListContainer.transform);
        Button btnChronicle = CreateMenuItem("MenuItem_Chronicle", "Chronicle", menuListContainer.transform);
        Button btnLetters = CreateMenuItem("MenuItem_Letters", "Letters", menuListContainer.transform);
        Button btnTreasury = CreateMenuItem("MenuItem_Treasury", "Treasury", menuListContainer.transform);
        Button btnTavern = CreateMenuItem("MenuItem_Tavern", "Tavern", menuListContainer.transform);
        Button btnSettings = CreateMenuItem("MenuItem_Settings", "Settings", menuListContainer.transform);

        canvasObj.name = "Panel_Menu";
        LegendOfBlood.MenuPanel menuScript = canvasObj.AddComponent<LegendOfBlood.MenuPanel>();
        menuScript.PanelType = LegendOfBlood.UIPanelType.Menu;

        SerializedObject so = new SerializedObject(menuScript);
        so.Update();

        so.FindProperty("closeButton").objectReferenceValue = closeBtnComp;
        so.FindProperty("inventoryButton").objectReferenceValue = btnBackpack;
        so.FindProperty("questButton").objectReferenceValue = btnChronicle;
        so.FindProperty("mailboxButton").objectReferenceValue = btnLetters;
        so.FindProperty("shopButton").objectReferenceValue = btnTreasury;
        so.FindProperty("recruitmentButton").objectReferenceValue = btnTavern;
        so.FindProperty("settingButton").objectReferenceValue = btnSettings;

        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(canvasObj, "Generate Menu Panel UI");
        Selection.activeGameObject = canvasObj;
        Debug.Log("Menu Panel UI Generated Successfully!");
    }

    private static Button CreateMenuItem(string gameObjectName, string labelText, Transform parent) {
        GameObject item = CreateUIElement(gameObjectName, parent);
        LayoutElement le = item.AddComponent<LayoutElement>();
        le.preferredWidth = 408;
        le.preferredHeight = 191;
        Button btnComp = item.AddComponent<Button>();

        GameObject scrollBg = CreateUIElement("ScrollBg", item.transform);
        SetAnchor(scrollBg, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(scrollBg, 0, 0, 0, 0);
        scrollBg.AddComponent<Image>().color = new Color(0.9f, 0.85f, 0.7f, 1f); // Màu giấy cuộn
        
        GameObject iconInfo = CreateUIElement("Icon", item.transform);
        SetAnchor(iconInfo, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        iconInfo.GetComponent<RectTransform>().anchoredPosition = new Vector2(38, 0);
        iconInfo.GetComponent<RectTransform>().sizeDelta = new Vector2(74, 74);
        iconInfo.AddComponent<Image>().color = new Color(0.5f, 0.3f, 0.2f, 1f);

        GameObject labelObj = CreateUIElement("LabelText", item.transform);
        SetAnchor(labelObj, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        labelObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(132, 0);
        labelObj.GetComponent<RectTransform>().sizeDelta = new Vector2(210, 82);
        GameObject textObj = AddTMPText(labelObj, labelText, 32);
        textObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        textObj.GetComponent<TextMeshProUGUI>().color = new Color(0.2f, 0.1f, 0.1f, 1f); // Chữ sậm chữ mực

        return btnComp;
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


#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProceduralDetailPanelGenerator : EditorWindow
{
    [MenuItem("UI Tools/Generate Procedural Detail Panel")]
    public static void GenerateUI()
    {
        // 0. Auto-Cleanup Old POI_InfoPanel
        var oldControllers = Object.FindObjectsByType<LegendOfBlood.POI_InfoPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in oldControllers) {
            Undo.DestroyObjectImmediate(c.gameObject);
        }

        GameObject canvasObj = new GameObject("Panel_POI_Info");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        canvasObj.AddComponent<GraphicRaycaster>();

        // 1. Phông nền đen tối
        GameObject darkBg = CreateUIElement("DarkOverlay", canvasObj.transform);
        SetAnchor(darkBg, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(darkBg, 0, 0, 0, 0);
        darkBg.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);

        // Nút tắt Popup chìm (Bấm nền tắt map)
        Button bgBtn = darkBg.AddComponent<Button>();

        // 2. Main Window
        GameObject mainWindow = CreateUIElement("MainWindow", canvasObj.transform);
        SetAnchorCenter(mainWindow, new Vector2(0, 50), new Vector2(950, 1600)); // To gần hết màn hình
        mainWindow.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.15f, 1f); // Nước xám đen

        // 3. Header
        GameObject header = CreateUIElement("Header", mainWindow.transform);
        SetAnchor(header, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        SetOffsets(header, 0, 0, 0, -150);
        header.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 1f);

        GameObject titleText = AddTMPText(header, "CATACOMBS (Lv. 5)", 40);
        titleText.name = "TitleText";
        titleText.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.8f, 0.5f, 1f);

        // Nút Đóng (Góc trên phải)
        GameObject closeBtnObj = CreateUIElement("CloseButton", header.transform);
        SetAnchor(closeBtnObj, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
        closeBtnObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-30, 0);
        closeBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);
        closeBtnObj.AddComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);
        Button closeBtn = closeBtnObj.AddComponent<Button>();
        AddTMPText(closeBtnObj, "X", 40);

        // 4. ScrollView Component cho Thuật Toán Slay the Spire
        GameObject scrollRectObj = CreateUIElement("ScrollFrame", mainWindow.transform);
        SetAnchor(scrollRectObj, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(scrollRectObj, 20, 20, 170, 20); // Chừa lề Header và khung

        ScrollRect scrollRect = scrollRectObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false; 
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.135f;

        // Viewport (Để che khuất nội dung trồi lên trên)
        GameObject viewport = CreateUIElement("Viewport", scrollRectObj.transform);
        SetAnchor(viewport, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(viewport, 0, 0, 0, 0);
        viewport.AddComponent<Image>().color = new Color(0, 0, 0, 0.05f); // Trong suốt mờ
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = true;

        // InnerMap (Chứa dây nhợ và các nút bô Node)
        GameObject innerMap = CreateUIElement("InnerMap", viewport.transform);
        SetAnchor(innerMap, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        SetOffsets(innerMap, 0, 0, 0, -1800); // Bản đồ dài 1800px (10 tầng)
        
        scrollRect.content = innerMap.GetComponent<RectTransform>();
        scrollRect.viewport = viewport.GetComponent<RectTransform>();

        // 5. Nối Script POI_InfoPanel
        LegendOfBlood.POI_InfoPanel infoPan = canvasObj.AddComponent<LegendOfBlood.POI_InfoPanel>();
        infoPan.PanelType = LegendOfBlood.UIPanelType.POI_Info;

        SerializedObject so = new SerializedObject(infoPan);
        so.Update();

        so.FindProperty("poiNameText").objectReferenceValue = titleText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("closeButton").objectReferenceValue = closeBtn;
        
        SerializedProperty innerMapProp = so.FindProperty("mapContentContainer");
        if (innerMapProp == null) {
            Debug.LogError("Chưa cập nhật POI_InfoPanel.cs để chứa biến mapContentContainer! Nhưng UI Layout vẫn sẽ lưu lại.");
        }

        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(canvasObj, "Generate Procedural Detail Panel");
        Selection.activeGameObject = canvasObj;
        Debug.Log("Procedural Detail Panel Generated! Sếp cần cập nhật script POI_InfoPanel.cs nữa nhé!");
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


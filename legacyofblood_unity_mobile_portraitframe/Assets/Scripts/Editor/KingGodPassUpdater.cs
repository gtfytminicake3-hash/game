#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KingGodPassUpdater : EditorWindow
{
    [MenuItem("UI Tools/Update KingGodPass Layout")]
    public static void UpdateLayout()
    {
        // 1. Tìm object KingGodPass trong Scene hiện tại
        GameObject passGroup = GameObject.Find("KingGodPass");
        if (passGroup == null)
        {
            Debug.LogError("Không tìm thấy GameObject tên 'KingGodPass' trong Scene. Hãy chắc chắn bảng MainScreen đang được bật.");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(passGroup, "Update KingGodPass Layout");

        // 2. Xóa các con cũ bên trong (PassIcon, PassFill, PassLabel...)
        int childCount = passGroup.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(passGroup.transform.GetChild(i).gameObject);
        }

        // 3. Chỉnh lại RectTransform của Group chính để khớp tỉ lệ ảnh
        RectTransform rt = passGroup.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = new Vector2(25, -10);
        rt.sizeDelta = new Vector2(350, 120);

        Image bgImg = passGroup.GetComponent<Image>();
        if (bgImg == null) bgImg = passGroup.AddComponent<Image>();
        bgImg.color = new Color(1f, 1f, 1f, 1f); // Reset về trắng để hiển thị đúng màu ảnh

        // 4. Tạo Text LVL hiển thị trên phần mũ (Bên trái)
        GameObject lvlTextObj = new GameObject("PassLevelText");
        lvlTextObj.transform.SetParent(passGroup.transform, false);
        RectTransform lvlRt = lvlTextObj.AddComponent<RectTransform>();
        lvlRt.anchorMin = new Vector2(0, 0.5f);
        lvlRt.anchorMax = new Vector2(0, 0.5f);
        lvlRt.pivot = new Vector2(0.5f, 0.5f);
        lvlRt.anchoredPosition = new Vector2(80, 0); // Đặt ngay giữa trán mũ
        lvlRt.sizeDelta = new Vector2(100, 40);

        TextMeshProUGUI lvlTmp = lvlTextObj.AddComponent<TextMeshProUGUI>();
        lvlTmp.text = "LV.50";
        lvlTmp.fontSize = 24;
        lvlTmp.fontStyle = FontStyles.Bold;
        lvlTmp.alignment = TextAlignmentOptions.Center;
        lvlTmp.color = Color.white;
        lvlTmp.enableWordWrapping = false;
        lvlTmp.outlineWidth = 0.2f;

        // 5. Tạo Container cho Progress Bar (Dọc theo lưỡi kiếm - Bên phải)
        GameObject progressContainer = new GameObject("PassProgressBar");
        progressContainer.transform.SetParent(passGroup.transform, false);
        RectTransform pcRt = progressContainer.AddComponent<RectTransform>();
        pcRt.anchorMin = new Vector2(0, 0.5f);
        pcRt.anchorMax = new Vector2(0, 0.5f);
        pcRt.pivot = new Vector2(0, 0.5f);
        // Căn đúng vị trí của phần thân đỏ của lưỡi gươm
        pcRt.anchoredPosition = new Vector2(120, 2); 
        // Kích thước này bạn có thể kéo giãn cho vừa khớp với tia đỏ của gươm
        pcRt.sizeDelta = new Vector2(180, 25); 

        // 6. Tạo vạch Năng lượng đỏ (PassFill) - sẽ nhận hình thanh gươm đỏ
        GameObject passFill = new GameObject("PassFill");
        passFill.transform.SetParent(progressContainer.transform, false);
        RectTransform fillRt = passFill.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.pivot = new Vector2(0.5f, 0.5f);
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;

        Image fillImg = passFill.AddComponent<Image>();
        // Chuyển màu về Trắng (White) thuần để ko đè màu rực rỡ của ảnh gốc
        fillImg.color = new Color(1f, 1f, 1f, 1f); 
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 0.8f; // Gốc test 80%

        // 7. Tạo Text hiển thị số EXP
        GameObject expTextObj = new GameObject("PassExpText");
        expTextObj.transform.SetParent(progressContainer.transform, false);
        RectTransform expRt = expTextObj.AddComponent<RectTransform>();
        expRt.anchorMin = new Vector2(0, 0.5f);
        expRt.anchorMax = new Vector2(1, 0.5f);
        expRt.pivot = new Vector2(0.5f, 0.5f);
        expRt.anchoredPosition = new Vector2(0, 20); // Dịch nhẹ lên trên thanh gươm
        expRt.sizeDelta = new Vector2(0, 30);

        TextMeshProUGUI expTmp = expTextObj.AddComponent<TextMeshProUGUI>();
        expTmp.text = "180/200";
        expTmp.fontSize = 18;
        expTmp.fontStyle = FontStyles.Bold;
        expTmp.alignment = TextAlignmentOptions.Center;
        expTmp.color = Color.white;
        expTmp.outlineWidth = 0.2f;

        // 8. Tự động gắn và nối dính Controller Logic
        LegendOfBlood.UIKingGodPassController passCtrl = passGroup.GetComponent<LegendOfBlood.UIKingGodPassController>();
        if (passCtrl == null) passCtrl = passGroup.AddComponent<LegendOfBlood.UIKingGodPassController>();
        
        SerializedObject soPass = new SerializedObject(passCtrl);
        soPass.Update();
        soPass.FindProperty("levelText").objectReferenceValue = lvlTmp;
        soPass.FindProperty("expText").objectReferenceValue = expTmp;
        soPass.FindProperty("fillImage").objectReferenceValue = fillImg;
        soPass.ApplyModifiedProperties();

        // 9. Gắn UIPanelNavButton để khi người dùng click vào cây kiếm thì mở bảng KingGodPass
        Button hitBoxBtn = passGroup.GetComponent<Button>();
        if (hitBoxBtn == null) hitBoxBtn = passGroup.AddComponent<Button>();
        
        // CỰC KỲ QUAN TRỌNG: Làm sạch mọi sự kiện cũ (như cái AddExp 50 test cũ)
        for(int i = hitBoxBtn.onClick.GetPersistentEventCount() - 1; i >= 0; i--) {
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(hitBoxBtn.onClick, i);
        }
        
        LegendOfBlood.UIPanelNavButton navBtn = passGroup.GetComponent<LegendOfBlood.UIPanelNavButton>();
        if (navBtn == null) navBtn = passGroup.AddComponent<LegendOfBlood.UIPanelNavButton>();
        
        SerializedObject soNav = new SerializedObject(navBtn);
        soNav.Update();
        soNav.FindProperty("targetPanel").enumValueIndex = (int)LegendOfBlood.UIPanelType.KingGodPass;
        soNav.ApplyModifiedProperties();

        Selection.activeGameObject = passGroup;
        Debug.Log("Đã cập nhật KingGodPass thành công. Bạn hãy thả ảnh vào GameObject này!");
    }
}
#endif

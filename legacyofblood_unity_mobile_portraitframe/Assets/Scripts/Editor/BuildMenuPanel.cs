using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using LegendOfBlood;

public class BuildMenuPanel : EditorWindow
{
    [MenuItem("Tools/Dựng Menu Panel (Chi tiết)")]
    public static void Build()
    {
        // 1. Tạo GameObject Container
        GameObject panelGO = new GameObject("Panel_Menu", typeof(RectTransform), typeof(CanvasRenderer));
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Hình nền tối
        Image bgImg = panelGO.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        // Script Setup
        MenuPanel script = panelGO.AddComponent<MenuPanel>();

        // 2. Tạo Header (Title & Close Button)
        GameObject headerPanel = new GameObject("Header", typeof(RectTransform), typeof(CanvasRenderer));
        headerPanel.transform.SetParent(panelGO.transform, false);
        RectTransform headerRect = headerPanel.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.sizeDelta = new Vector2(0, 150);

        // Nền Header
        Image headerBg = headerPanel.AddComponent<Image>();
        headerBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        // Text Title
        GameObject titleGO = new GameObject("TitleText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        titleGO.transform.SetParent(headerPanel.transform, false);
        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = Vector2.zero; titleRect.anchorMax = Vector2.one;
        titleRect.offsetMin = new Vector2(50, 0); titleRect.offsetMax = new Vector2(-200, 0); // Trừ chỗ cho nút Close
        TextMeshProUGUI titleTxt = titleGO.GetComponent<TextMeshProUGUI>();
        titleTxt.text = "DANH MỤC / MENU";
        titleTxt.fontSize = 60;
        titleTxt.alignment = TextAlignmentOptions.MidlineLeft;
        titleTxt.color = Color.white;
        titleTxt.fontStyle = FontStyles.Bold;

        // Nút Close
        Button closeBtn = CreateButton("Btn_Close", headerPanel.transform, "X");
        RectTransform closeRect = closeBtn.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 0.5f);
        closeRect.anchorMax = new Vector2(1, 0.5f);
        closeRect.pivot = new Vector2(1, 0.5f);
        closeRect.sizeDelta = new Vector2(120, 120);
        closeRect.anchoredPosition = new Vector2(-40, 0);
        closeBtn.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f); // Nút đóng màu đỏ
        closeBtn.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;

        AssignPrivateField(script, "closeButton", closeBtn);

        // 3. Vùng chứa nội dung Grid (Menu Buttons)
        GameObject contentArea = new GameObject("MenuContentArea", typeof(RectTransform));
        contentArea.transform.SetParent(panelGO.transform, false);
        RectTransform contentRect = contentArea.GetComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = new Vector2(80, 80); // Lề dưới, trái
        contentRect.offsetMax = new Vector2(-80, -200); // Lề trên (chừa Header), phải

        GridLayoutGroup glg = contentArea.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(400, 150); 
        glg.spacing = new Vector2(50, 50);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 2; // Xếp thành 2 cột đều nhau
        glg.childAlignment = TextAnchor.UpperCenter;

        // 4. Các nút chức năng
        AssignPrivateField(script, "inventoryButton", CreateMenuButton("Btn_Inventory", contentArea.transform, "TÚI ĐỒ"));
        AssignPrivateField(script, "questButton", CreateMenuButton("Btn_Quest", contentArea.transform, "NHIỆM VỤ"));
        AssignPrivateField(script, "mailboxButton", CreateMenuButton("Btn_Mailbox", contentArea.transform, "HÒM THƯ"));
        AssignPrivateField(script, "shopButton", CreateMenuButton("Btn_Shop", contentArea.transform, "CỬA HÀNG"));
        AssignPrivateField(script, "settingButton", CreateMenuButton("Btn_Setting", contentArea.transform, "CÀI ĐẶT"));
        AssignPrivateField(script, "recruitmentButton", CreateMenuButton("Btn_Recruit", contentArea.transform, "CHIÊU MỘ"));

        // Lưu
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        PrefabUtility.SaveAsPrefabAsset(panelGO, "Assets/Prefabs/Panel_Menu.prefab");
        DestroyImmediate(panelGO);

        Debug.Log("<color=green>Đã tạo xong Panel_Menu cực kì chi tiết và lưu vào Prefabs!</color>");
        AssetDatabase.Refresh();
    }

    static Button CreateButton(string name, Transform parent, string btnText)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        Button btn = go.GetComponent<Button>();
        
        GameObject txtGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        txtGO.transform.SetParent(go.transform, false);
        RectTransform txtRect = txtGO.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero; txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero; txtRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI tmp = txtGO.GetComponent<TextMeshProUGUI>();
        tmp.text = btnText;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontSize = 45;

        return btn;
    }

    static Button CreateMenuButton(string name, Transform parent, string label)
    {
        Button btn = CreateButton(name, parent, label);
        Image img = btn.GetComponent<Image>();
        img.color = new Color(0.25f, 0.35f, 0.45f); // Màu xanh/xám dịu mắt cho các khu vực Menu
        return btn;
    }

    static void AssignPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(target, value);
        }
        else
        {
            Debug.LogError($"Field {fieldName} not found in {target.GetType()}");
        }
    }
}

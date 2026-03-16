using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using LegendOfBlood.Combat;
using LegendOfBlood;
using LegendOfBlood;

public class BattlePanelBuilder : EditorWindow
{
    [MenuItem("Tools/Tạo Hình Ảnh Trận Đánh (9-Grid)")]
    public static void BuildBattlePanel()
    {
        // 1. Dựng Prefab cho Battle Unit (Thẻ Tướng bay lượn)
        GameObject unitPrefab = CreateBattleUnitPrefab();

        // 2. Dựng Panel_Battle mới
        GameObject battlePanel = new GameObject("Panel_Battle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        
        RectTransform bpRect = battlePanel.GetComponent<RectTransform>();
        bpRect.anchorMin = Vector2.zero; bpRect.anchorMax = Vector2.one;
        bpRect.sizeDelta = Vector2.zero;
        battlePanel.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 1f); // Nền xám đen

        UIPanel uiPan = battlePanel.AddComponent<UIPanel>();
        uiPan.PanelType = UIPanelType.Battle;

        CombatVisualizerPanel visualizer = battlePanel.AddComponent<CombatVisualizerPanel>();
        visualizer.battleUnitPrefab = unitPrefab;

        // Tiêu đề
        CreateTextPro("BattleTitle", battlePanel.transform, new Vector2(0, -50), new Vector2(600, 100), "ĐẤU TRƯỜNG HUYỀN THOẠI", 50, TextAlignmentOptions.Center);

        // --- ALLY CONTAINER (9 Ô) ---
        GameObject allyContainer = new GameObject("AllyContainer", typeof(RectTransform), typeof(GridLayoutGroup));
        allyContainer.transform.SetParent(battlePanel.transform, false);
        RectTransform acRect = allyContainer.GetComponent<RectTransform>();
        acRect.anchorMin = new Vector2(0, 0.1f); acRect.anchorMax = new Vector2(0.5f, 0.9f);
        acRect.offsetMin = new Vector2(10, 0); acRect.offsetMax = new Vector2(-10, 0);
        
        GridLayoutGroup allyGrid = allyContainer.GetComponent<GridLayoutGroup>();
        allyGrid.cellSize = new Vector2(180, 240);
        allyGrid.spacing = new Vector2(10, 10);
        allyGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        allyGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
        allyGrid.childAlignment = TextAnchor.MiddleCenter;
        allyGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        allyGrid.constraintCount = 3;

        for (int i = 0; i < 9; i++)
        {
            GameObject slot = new GameObject($"Slot_{i}", typeof(RectTransform));
            slot.transform.SetParent(allyContainer.transform, false);
        }

        // --- ENEMY CONTAINER (9 Ô) ---
        GameObject enemyContainer = new GameObject("EnemyContainer", typeof(RectTransform), typeof(GridLayoutGroup));
        enemyContainer.transform.SetParent(battlePanel.transform, false);
        RectTransform ecRect = enemyContainer.GetComponent<RectTransform>();
        ecRect.anchorMin = new Vector2(0.5f, 0.1f); ecRect.anchorMax = new Vector2(1, 0.9f);
        ecRect.offsetMin = new Vector2(10, 0); ecRect.offsetMax = new Vector2(-10, 0);
        
        GridLayoutGroup enemyGrid = enemyContainer.GetComponent<GridLayoutGroup>();
        enemyGrid.cellSize = new Vector2(180, 240);
        enemyGrid.spacing = new Vector2(10, 10);
        enemyGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        enemyGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
        enemyGrid.childAlignment = TextAnchor.MiddleCenter;
        enemyGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        enemyGrid.constraintCount = 3;

        for (int i = 0; i < 9; i++)
        {
            GameObject slot = new GameObject($"Slot_{i}", typeof(RectTransform));
            slot.transform.SetParent(enemyContainer.transform, false);
        }

        // --- Nút điểu khiển ---
        Button skipBtn = CreateButtonWithText("SkipButton", battlePanel.transform, new Vector2(-150, 50), new Vector2(250, 80), "BỎ QUA (SKIP)");
        skipBtn.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0); skipBtn.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0);
        
        Button speedBtn = CreateButtonWithText("SpeedButton", battlePanel.transform, new Vector2(-450, 50), new Vector2(250, 80), "TỐC ĐỘ: x1");
        speedBtn.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0); speedBtn.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0);
        TextMeshProUGUI speedText = speedBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        // --- Màn hình Victory/Defeat ---
        GameObject victoryScreen = CreatePanelBase("VictoryScreen", battlePanel.transform, new Color(0, 0.5f, 0, 0.9f));
        CreateTextPro("VictoryText", victoryScreen.transform, new Vector2(0, 100), new Vector2(800, 200), "CHIẾN THẮNG!", 100, TextAlignmentOptions.Center);
        Button closeVicBtn = CreateButtonWithText("CloseVictoryBtn", victoryScreen.transform, new Vector2(0, -100), new Vector2(300, 100), "Trở Về Làng");
        victoryScreen.SetActive(false);

        GameObject defeatScreen = CreatePanelBase("DefeatScreen", battlePanel.transform, new Color(0.5f, 0, 0, 0.9f));
        CreateTextPro("DefeatText", defeatScreen.transform, new Vector2(0, 100), new Vector2(800, 200), "THẤT BẠI!", 100, TextAlignmentOptions.Center);
        Button closeDefBtn = CreateButtonWithText("CloseDefeatBtn", defeatScreen.transform, new Vector2(0, -100), new Vector2(300, 100), "Trở Về Làng");
        defeatScreen.SetActive(false);

        // --- Nối dây ---
        FieldAssigner.AssignField(visualizer, "allyContainer", allyContainer.transform);
        FieldAssigner.AssignField(visualizer, "enemyContainer", enemyContainer.transform);
        FieldAssigner.AssignField(visualizer, "skipButton", skipBtn);
        FieldAssigner.AssignField(visualizer, "x2SpeedButton", speedBtn);
        FieldAssigner.AssignField(visualizer, "_speedText", speedText);
        FieldAssigner.AssignField(visualizer, "victoryScreen", victoryScreen);
        FieldAssigner.AssignField(visualizer, "defeatScreen", defeatScreen);
        FieldAssigner.AssignField(visualizer, "closeVictoryButton", closeVicBtn);
        FieldAssigner.AssignField(visualizer, "closeDefeatButton", closeDefBtn);

        // Chờ kết nối xong thì biến thành Prefab
        string pPath = "Assets/Prefabs/Panel_Battle.prefab";
        PrefabUtility.SaveAsPrefabAsset(battlePanel, pPath);
        DestroyImmediate(battlePanel);

        AssetDatabase.Refresh();
        Debug.Log("<color=green>Đã tạo thành công Panel_Battle với lưới 9x9 đúng chuẩn GDD!</color>");
    }

    static GameObject CreateBattleUnitPrefab()
    {
        GameObject unitTemplate = new GameObject("BattleUnit_Prefab", typeof(RectTransform));
        RectTransform utRect = unitTemplate.GetComponent<RectTransform>();
        utRect.sizeDelta = new Vector2(160, 220);
        
        Image avatarImg = new GameObject("Avatar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
        avatarImg.transform.SetParent(unitTemplate.transform, false);
        avatarImg.rectTransform.anchorMin = Vector2.zero; avatarImg.rectTransform.anchorMax = Vector2.one;
        avatarImg.rectTransform.sizeDelta = Vector2.zero;
        avatarImg.color = Color.gray; 

        Slider hpSlider = new GameObject("HpSlider", typeof(RectTransform), typeof(Slider)).GetComponent<Slider>();
        hpSlider.transform.SetParent(unitTemplate.transform, false);
        hpSlider.interactable = false;
        hpSlider.transition = Selectable.Transition.None;
        RectTransform hsRect = hpSlider.GetComponent<RectTransform>();
        hsRect.anchorMin = new Vector2(0, 1); hsRect.anchorMax = new Vector2(1, 1); 
        hsRect.pivot = new Vector2(0.5f, 0); hsRect.anchoredPosition = new Vector2(0, 10);
        hsRect.sizeDelta = new Vector2(0, 25);
        
        Image bgImg = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>(); 
        bgImg.transform.SetParent(hpSlider.transform, false);
        bgImg.color = Color.red;
        bgImg.rectTransform.anchorMin = Vector2.zero; bgImg.rectTransform.anchorMax = Vector2.one; bgImg.rectTransform.sizeDelta = Vector2.zero;
        
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform)); 
        fillArea.transform.SetParent(hpSlider.transform, false);
        RectTransform faRect = fillArea.GetComponent<RectTransform>(); 
        faRect.anchorMin = Vector2.zero; faRect.anchorMax = Vector2.one; faRect.sizeDelta = Vector2.zero;
        
        Image fillImg = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>(); 
        fillImg.transform.SetParent(fillArea.transform, false);
        fillImg.color = Color.green;
        fillImg.rectTransform.anchorMin = Vector2.zero; fillImg.rectTransform.anchorMax = Vector2.one; fillImg.rectTransform.sizeDelta = Vector2.zero;
        hpSlider.fillRect = fillImg.rectTransform;

        TextMeshProUGUI hpTxt = CreateTextPro("HpText", hpSlider.transform, Vector2.zero, new Vector2(160, 25), "100/100", 18, TextAlignmentOptions.Center);
        hpTxt.rectTransform.anchorMin = Vector2.zero; hpTxt.rectTransform.anchorMax = Vector2.one;
        hpTxt.rectTransform.sizeDelta = Vector2.zero;

        GameObject dmgCgObj = new GameObject("DamageTextContainer", typeof(RectTransform), typeof(CanvasGroup));
        dmgCgObj.transform.SetParent(unitTemplate.transform, false);
        RectTransform dmgRect = dmgCgObj.GetComponent<RectTransform>();
        CanvasGroup dmgCg = dmgCgObj.GetComponent<CanvasGroup>();
        dmgRect.anchorMin = new Vector2(0, 0.5f); dmgRect.anchorMax = new Vector2(1, 1.5f);
        dmgRect.offsetMin = Vector2.zero; dmgRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI dmgTxt = CreateTextPro("DamageText", dmgCgObj.transform, Vector2.zero, new Vector2(250, 80), "-999", 36, TextAlignmentOptions.Center);
        dmgTxt.rectTransform.anchorMin = Vector2.zero; dmgTxt.rectTransform.anchorMax = Vector2.one;
        dmgTxt.rectTransform.sizeDelta = Vector2.zero;
        dmgTxt.fontStyle = FontStyles.Bold;

        BattleUnitUI unitScript = unitTemplate.AddComponent<BattleUnitUI>();
        FieldAssigner.AssignField(unitScript, "avatarImage", avatarImg);
        FieldAssigner.AssignField(unitScript, "hpSlider", hpSlider);
        FieldAssigner.AssignField(unitScript, "hpText", hpTxt);
        FieldAssigner.AssignField(unitScript, "damageTextCanvasGroup", dmgCg);
        FieldAssigner.AssignField(unitScript, "damageText", dmgTxt);

        string pPath = "Assets/Prefabs/BattleUnit_Prefab.prefab";
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(unitTemplate, pPath);
        DestroyImmediate(unitTemplate);
        
        return savedPrefab;
    }

    static GameObject CreatePanelBase(string name, Transform parent, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = color;
        return panel;
    }

    static TextMeshProUGUI CreateTextPro(string name, Transform parent, Vector2 anchoredPos, Vector2 size, string text, int fontSize, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;
        
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        return tmp;
    }

    static Button CreateButtonWithText(string name, Transform parent, Vector2 anchoredPos, Vector2 size, string text)
    {
        GameObject btnGO = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(parent, false);
        RectTransform rect = btnGO.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;
        btnGO.GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f, 1f);
        
        TextMeshProUGUI txtTmp = CreateTextPro("Text (TMP)", btnGO.transform, Vector2.zero, size, text, 24, TextAlignmentOptions.Center);
        txtTmp.color = Color.black;
        txtTmp.rectTransform.anchorMin = Vector2.zero; txtTmp.rectTransform.anchorMax = Vector2.one;
        txtTmp.rectTransform.sizeDelta = Vector2.zero;

        return btnGO.GetComponent<Button>();
    }
}

public static class FieldAssigner
{
    public static void AssignField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(target, value);
        }
        else
        {
            Debug.LogError($"Không tìm thấy {fieldName} trong {target.GetType()}");
        }
    }
}

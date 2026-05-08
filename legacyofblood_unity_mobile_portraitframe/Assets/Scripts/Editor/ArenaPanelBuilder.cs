using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using LegendOfBlood.UI;

public class ArenaPanelBuilder
{
    [MenuItem("UI Tools/Create Arena Panel Prefab")]
    public static void BuildArenaPanel()
    {
        // Require a temporary Canvas to build UI elements cleanly
        GameObject tempCanvasObj = new GameObject("TempCanvas");
        Canvas canvas = tempCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // 1. Root Panel
        GameObject root = new GameObject("ArenaPanel");
        root.transform.SetParent(tempCanvasObj.transform, false);
        RectTransform rootRt = root.AddComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.sizeDelta = Vector2.zero;
        rootRt.anchoredPosition = Vector2.zero;
        
        Image bgImg = root.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.25f, 1f); // Dark medieval grey-blue
        
        ArenaPanel arenaPanel = root.AddComponent<ArenaPanel>();

        // Load Default Font
        TMP_FontAsset defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Liberation-Permanent.asset");
        if (defaultFont == null)
            defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

        // --- Helper Function ---
        GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
        }

        TextMeshProUGUI CreateText(string textStr, int fontSize, Transform parent, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            GameObject textObj = CreateUIObject("Text", parent);
            RectTransform rt = textObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = textStr;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            if (defaultFont != null) tmp.font = defaultFont;
            return tmp;
        }

        // 2. TOP BAR
        GameObject topBar = CreateUIObject("TopBar", root.transform);
        RectTransform topBarRt = topBar.GetComponent<RectTransform>();
        topBarRt.anchorMin = new Vector2(0, 1);
        topBarRt.anchorMax = new Vector2(1, 1);
        topBarRt.pivot = new Vector2(0.5f, 1);
        topBarRt.sizeDelta = new Vector2(0, 200);

        // Header "ARENA"
        GameObject headerBanner = CreateUIObject("HeaderBanner", topBar.transform);
        Image headerImg = headerBanner.AddComponent<Image>();
        headerImg.color = new Color(0.9f, 0.8f, 0.6f); // Banner color
        RectTransform headerRt = headerBanner.GetComponent<RectTransform>();
        headerRt.anchorMin = new Vector2(0.3f, 0.5f);
        headerRt.anchorMax = new Vector2(0.7f, 1f);
        headerRt.sizeDelta = new Vector2(0, -20);
        headerRt.anchoredPosition = new Vector2(0, -10);
        CreateText("<b>ARENA</b>", 48, headerBanner.transform);

        // Resources
        GameObject resGroup = CreateUIObject("ResourceGroup", topBar.transform);
        RectTransform resRt = resGroup.GetComponent<RectTransform>();
        resRt.anchorMin = new Vector2(0.1f, 0);
        resRt.anchorMax = new Vector2(0.9f, 0.5f);
        resRt.sizeDelta = Vector2.zero;
        HorizontalLayoutGroup hlg = resGroup.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth = true;
        hlg.childForceExpandWidth = true;
        hlg.spacing = 20;

        GameObject goldObj = CreateUIObject("GoldObj", resGroup.transform);
        goldObj.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        arenaPanel.goldText = CreateText(" 8675/46", 32, goldObj.transform);

        GameObject cupObj = CreateUIObject("CupObj", resGroup.transform);
        cupObj.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        arenaPanel.cupsText = CreateText(" 1307", 32, cupObj.transform);

        GameObject crystalObj = CreateUIObject("CrystalObj", resGroup.transform);
        crystalObj.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        arenaPanel.crystalsText = CreateText(" 96", 32, crystalObj.transform);

        // 3. DEFENSE TEAM (Center Left)
        GameObject defenseArea = CreateUIObject("DefenseArea", root.transform);
        RectTransform defRt = defenseArea.GetComponent<RectTransform>();
        defRt.anchorMin = new Vector2(0, 0.3f);
        defRt.anchorMax = new Vector2(0.55f, 0.7f);
        defRt.sizeDelta = Vector2.zero;

        // Pedestal
        GameObject pedestal = CreateUIObject("Pedestal", defenseArea.transform);
        Image pedImg = pedestal.AddComponent<Image>();
        pedImg.color = new Color(0.5f, 0.5f, 0.5f);
        RectTransform pedRt = pedestal.GetComponent<RectTransform>();
        pedRt.anchorMin = new Vector2(0.1f, 0.1f);
        pedRt.anchorMax = new Vector2(0.9f, 0.4f);
        pedRt.sizeDelta = Vector2.zero;
        arenaPanel.pedestalImage = pedImg;

        // Label
        GameObject defLabel = CreateUIObject("DefenseLabel", defenseArea.transform);
        Image defLabImg = defLabel.AddComponent<Image>();
        defLabImg.color = new Color(0.9f, 0.8f, 0.6f);
        RectTransform defLabRt = defLabel.GetComponent<RectTransform>();
        defLabRt.anchorMin = new Vector2(0.2f, 0.45f);
        defLabRt.anchorMax = new Vector2(0.8f, 0.6f);
        defLabRt.sizeDelta = Vector2.zero;
        CreateText("YOUR DEFENSE TEAM", 26, defLabel.transform).color = Color.black;

        // Characters Container
        GameObject charsGrp = CreateUIObject("CharactersContainer", defenseArea.transform);
        RectTransform charsRt = charsGrp.GetComponent<RectTransform>();
        charsRt.anchorMin = new Vector2(0.1f, 0.65f);
        charsRt.anchorMax = new Vector2(0.9f, 0.95f);
        charsRt.sizeDelta = Vector2.zero;
        HorizontalLayoutGroup charsHlg = charsGrp.AddComponent<HorizontalLayoutGroup>();
        charsHlg.spacing = 10;
        charsHlg.childForceExpandWidth = true;
        arenaPanel.charactersContainer = charsRt;

        for (int i = 0; i < 3; i++)
        {
            GameObject chr = CreateUIObject($"Char_{i}", charsGrp.transform);
            chr.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);
            CreateText("Knight Placeholder", 18, chr.transform);
        }

        // 4. OPPONENT SELECTION (Center Right)
        GameObject oppArea = CreateUIObject("OpponentArea", root.transform);
        RectTransform oppRt = oppArea.GetComponent<RectTransform>();
        oppRt.anchorMin = new Vector2(0.6f, 0.35f);
        oppRt.anchorMax = new Vector2(0.95f, 0.85f);
        oppRt.sizeDelta = Vector2.zero;

        // Opponent Label
        GameObject oppLabel = CreateUIObject("OpponentLabel", oppArea.transform);
        oppLabel.AddComponent<Image>().color = new Color(0.9f, 0.8f, 0.6f);
        RectTransform oppLabRt = oppLabel.GetComponent<RectTransform>();
        oppLabRt.anchorMin = new Vector2(0.1f, 0.85f);
        oppLabRt.anchorMax = new Vector2(0.9f, 1f);
        oppLabRt.sizeDelta = Vector2.zero;
        CreateText("OPPONENT\nSELECTION", 22, oppLabel.transform).color = Color.black;

        // Slots
        GameObject oppSlotsGrp = CreateUIObject("OpponentSlots", oppArea.transform);
        RectTransform oppSlotsRt = oppSlotsGrp.GetComponent<RectTransform>();
        oppSlotsRt.anchorMin = new Vector2(0.1f, 0);
        oppSlotsRt.anchorMax = new Vector2(0.9f, 0.8f);
        oppSlotsRt.sizeDelta = Vector2.zero;
        VerticalLayoutGroup vlg = oppSlotsGrp.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 20;
        vlg.childForceExpandHeight = true;

        arenaPanel.opponentButtons = new Button[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject slot = CreateUIObject($"OpponentSlot_{i}", oppSlotsGrp.transform);
            slot.AddComponent<Image>().color = new Color(0.8f, 0.7f, 0.6f);
            arenaPanel.opponentButtons[i] = slot.AddComponent<Button>();
            CreateText("Enemy Portrait", 20, slot.transform).color = Color.black;
        }

        // 5. BOTTOM RIGHT MENU
        GameObject btmMenu = CreateUIObject("BottomRightMenu", root.transform);
        RectTransform btmMenuRt = btmMenu.GetComponent<RectTransform>();
        btmMenuRt.anchorMin = new Vector2(0.85f, 0.1f);
        btmMenuRt.anchorMax = new Vector2(1f, 0.35f);
        btmMenuRt.sizeDelta = Vector2.zero;
        VerticalLayoutGroup btmVlg = btmMenu.AddComponent<VerticalLayoutGroup>();
        btmVlg.spacing = 15;
        btmVlg.childForceExpandHeight = true;

        arenaPanel.settingsButton = CreateUIObject("BtnSettings", btmMenu.transform).AddComponent<Button>();
        arenaPanel.settingsButton.gameObject.AddComponent<Image>().color = Color.gray;
        CreateText("Settings", 16, arenaPanel.settingsButton.transform);

        arenaPanel.mailButton = CreateUIObject("BtnMail", btmMenu.transform).AddComponent<Button>();
        arenaPanel.mailButton.gameObject.AddComponent<Image>().color = Color.gray;
        CreateText("Mail", 16, arenaPanel.mailButton.transform);

        arenaPanel.questsButton = CreateUIObject("BtnQuests", btmMenu.transform).AddComponent<Button>();
        arenaPanel.questsButton.gameObject.AddComponent<Image>().color = Color.gray;
        CreateText("Quests", 16, arenaPanel.questsButton.transform);

        // 6. MAIN ACTION: FIND MATCH
        GameObject findMatchObj = CreateUIObject("BtnFindMatch", root.transform);
        RectTransform fmRt = findMatchObj.GetComponent<RectTransform>();
        fmRt.anchorMin = new Vector2(0.35f, 0.12f);  // Padded up to avoid bottom HUD
        fmRt.anchorMax = new Vector2(0.65f, 0.28f);
        fmRt.sizeDelta = Vector2.zero;
        
        Image fmImg = findMatchObj.AddComponent<Image>();
        fmImg.color = new Color(0.1f, 0.2f, 0.5f); // Deep Blue Shield Placeholder
        arenaPanel.findMatchButton = findMatchObj.AddComponent<Button>();
        CreateText("<b>FIND MATCH</b>", 40, findMatchObj.transform);

        // Save to prefab
        string path = "Assets/Prefabs/UI/ArenaPanel.prefab";
        System.IO.Directory.CreateDirectory("d:/game/legendofblood/legacyofblood_unity_mobile_portraitframe/Assets/Prefabs");
        System.IO.Directory.CreateDirectory("d:/game/legendofblood/legacyofblood_unity_mobile_portraitframe/Assets/Prefabs/UI");
        
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(tempCanvasObj);

        Debug.Log($"[ArenaPanelBuilder] Successfully built and saved ArenaPanel Prefab to {path}");
    }

    [MenuItem("UI Tools/Inject Arena Panel To Scene")]
    public static void InjectArenaPanelToScene()
    {
        string path = "Assets/Prefabs/UI/ArenaPanel.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError("Prefab not found! Please Create Arena Panel Prefab first.");
            return;
        }

        // Tìm Canvas gốc
        Canvas mainCanvas = Object.FindAnyObjectByType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("No Canvas found in the current scene to inject ArenaPanel!");
            return;
        }

        // Tìm xem ArenaPanel đã được tạo ra trong con của canvas chưa để tránh tạo đúp
        Transform existing = mainCanvas.transform.Find("ArenaPanel");
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        // Instantiate
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, mainCanvas.transform);
        instance.name = "ArenaPanel";
        instance.transform.SetAsLastSibling();

        // Thêm Component Móc Nối (UIPanel) để UIManager nhận diện
        LegendOfBlood.UIPanel uiPanel = instance.GetComponent<LegendOfBlood.UIPanel>();
        if (uiPanel == null)
        {
            uiPanel = instance.AddComponent<LegendOfBlood.UIPanel>();
        }
        uiPanel.PanelType = LegendOfBlood.UIPanelType.Arena;

        // Tắt đi để UIManager quản lý
        instance.SetActive(false);

        // Lưu thay đổi
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log($"<color=green><b>[SUCCESS]</b> Đã tự động nhúng ArenaPanel vào {mainCanvas.name} và sẵn sàng chạy!</color>");
    }
}

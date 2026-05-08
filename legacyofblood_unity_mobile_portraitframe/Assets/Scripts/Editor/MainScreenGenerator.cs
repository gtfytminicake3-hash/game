#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainScreenGenerator : EditorWindow
{
    [MenuItem("UI Tools/Generate Main Screen")]
    public static void GenerateUI()
    {
        // 0. Auto-Cleanup Old UI to prevent UIManager conflicts
        var oldControllers = Object.FindObjectsByType<LegendOfBlood.UIMainController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in oldControllers) {
            Undo.DestroyObjectImmediate(c.gameObject);
        }

        GameObject canvasObj = new GameObject("MainScreen_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        
        canvasObj.AddComponent<GraphicRaycaster>();

        // 1. Safe Area wrapper (Bao bọc toàn bộ theo yêu cầu)
        GameObject safeAreaObj = CreateUIElement("SafeArea", canvasObj.transform);
        SetAnchor(safeAreaObj, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(safeAreaObj, 0, 0, 0, 0); // Stretch Full
        safeAreaObj.AddComponent<SafeArea>(); 

        // 2. Background (nằm NGOÀI Safe Area để full màn hình)
        GameObject bg = CreateUIElement("Background", canvasObj.transform);
        bg.transform.SetAsFirstSibling(); // Đưa xuống dưới cùng (vẽ đầu tiên)
        SetAnchor(bg, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(bg, 0, 0, 0, 0);
        bg.AddComponent<Image>().color = new Color(0.12f, 0.14f, 0.16f, 1f);

        // 3. Top Bar (Top Stretch)
        GameObject topBar = CreateUIElement("TopBar", safeAreaObj.transform);
        SetAnchor(topBar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        topBar.GetComponent<RectTransform>().offsetMin = new Vector2(0, -170); // Bottom offset
        topBar.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);    // Top offset
        topBar.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        // --- Player Level Badge ---
        GameObject playerLevelBadge = CreateUIElement("PlayerLevelBadge", topBar.transform);
        SetAnchor(playerLevelBadge, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
        playerLevelBadge.GetComponent<RectTransform>().anchoredPosition = new Vector2(20, -5);
        playerLevelBadge.GetComponent<RectTransform>().sizeDelta = new Vector2(140, 150);

        GameObject badgeBg = CreateUIElement("BadgeBg", playerLevelBadge.transform);
        SetAnchor(badgeBg, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(badgeBg, 0, 0, 0, 0);
        badgeBg.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f); 

        GameObject textLvlObj = AddTMPText(playerLevelBadge, "LVL", 16);
        textLvlObj.name = "Text_LVL";
        SetAnchor(textLvlObj, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        textLvlObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -18);
        textLvlObj.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 30);
        ColorUtility.TryParseHtmlString("#EAD9A3", out Color colorLvl);
        textLvlObj.GetComponent<TextMeshProUGUI>().color = colorLvl;

        GameObject textLevelObj = AddTMPText(playerLevelBadge, "50", 32);
        textLevelObj.name = "Text_Level";
        SetAnchorCenter(textLevelObj, new Vector2(0, 10), new Vector2(80, 50));

        // --- Resource Group ---
        GameObject resGroup = CreateUIElement("ResourceGroup", topBar.transform);
        SetAnchor(resGroup, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0)); // Bottom Right
        resGroup.GetComponent<RectTransform>().anchoredPosition = new Vector2(-20, 15); // Padding from right & bottom
        
        HorizontalLayoutGroup hlgRes = resGroup.AddComponent<HorizontalLayoutGroup>();
        hlgRes.spacing = 15; 
        hlgRes.childControlWidth = true; 
        hlgRes.childForceExpandWidth = false;
        
        ContentSizeFitter csfRes = resGroup.AddComponent<ContentSizeFitter>();
        csfRes.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        string[] resNames = {"Gold", "Stone", "Diamond"};
        string[] resValues = {"8675", "1307", "96"};
        Color[] resColors = { new Color(1f, 0.8f, 0.2f), new Color(0.6f, 0.6f, 0.6f), new Color(0.2f, 0.5f, 1f) };

        for(int i = 0; i < 3; i++) {
            GameObject resPanel = CreateUIElement("Res_" + resNames[i], resGroup.transform);
            resPanel.AddComponent<Image>().color = new Color(0.9f, 0.85f, 0.7f, 1f); // Nền dạng cuộn giấy
            LayoutElement le = resPanel.AddComponent<LayoutElement>();
            le.preferredWidth = 160;
            le.preferredHeight = 50;

            GameObject resIcon = CreateUIElement("Icon", resPanel.transform);
            SetAnchor(resIcon, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
            resIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(10, 0);
            resIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
            resIcon.AddComponent<Image>().color = resColors[i]; // Placeholder màu tạm thời

            GameObject resText = AddTMPText(resPanel, resValues[i], 26);
            resText.name = "Text_Value";
            SetOffsets(resText, 55, 5, 0, 0); // Text chiếm khoảng trống bên phải icon
            resText.GetComponent<TextMeshProUGUI>().color = new Color(0.1f, 0.1f, 0.1f, 1f); // Chữ màu tối
        }

        // --- Sub Top Bar (Ngay dưới TopBar) ---
        GameObject subTopBar = CreateUIElement("SubTopBar", safeAreaObj.transform);
        SetAnchor(subTopBar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        subTopBar.GetComponent<RectTransform>().offsetMin = new Vector2(0, -290); // Y: 170 + 120 = 290
        subTopBar.GetComponent<RectTransform>().offsetMax = new Vector2(0, -170); // Bắt đầu ở cạnh dưới của TopBar
        
        // King God Pass (Helmet & Sword layout)
        GameObject passGroup = CreateUIElement("KingGodPass", subTopBar.transform);
        SetAnchor(passGroup, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        passGroup.GetComponent<RectTransform>().anchoredPosition = new Vector2(25, -10);
        passGroup.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 120);
        
        // Bật Image với màu trắng để user có thể gắn Sprite hình cây kiếm/mũ bảo hiểm vào
        Image bgImg = passGroup.AddComponent<Image>();
        bgImg.color = new Color(1f, 1f, 1f, 1f); 

        // Text Level hiển thị trên mũ bảo hiểm (Bên trái)
        GameObject passLevelText = AddTMPText(passGroup, "LV.50", 24);
        passLevelText.name = "PassLevelText";
        SetAnchor(passLevelText, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f));
        passLevelText.GetComponent<RectTransform>().anchoredPosition = new Vector2(80, 0); // Canh ngay phần trán mũ bảo hiểm (viên đá xanh)
        passLevelText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        passLevelText.GetComponent<TextMeshProUGUI>().color = Color.white;
        passLevelText.GetComponent<TextMeshProUGUI>().enableWordWrapping = false;
        // Outline cho chữ để dễ đọc trên nền sáng của mũ
        passLevelText.GetComponent<TextMeshProUGUI>().outlineWidth = 0.2f;

        // Progress Bar theo thanh gươm (Bên phải)
        GameObject passProgressBar = CreateUIElement("PassProgressBar", passGroup.transform);
        SetAnchor(passProgressBar, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        passProgressBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(140, 2); // Bắt đầu từ phần lưỡi gươm lòi ra
        passProgressBar.GetComponent<RectTransform>().sizeDelta = new Vector2(170, 16); // Thanh nhỏ nằm gọn giữa lưỡi gươm
        Image barBgImg = passProgressBar.AddComponent<Image>();
        barBgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        GameObject passFill = CreateUIElement("PassFill", passProgressBar.transform);
        SetAnchor(passFill, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
        SetOffsets(passFill, 0, 0, 0, 0); 
        Image fillImg = passFill.AddComponent<Image>();
        fillImg.color = new Color(0.9f, 0.1f, 0.2f, 1f); // Lõi đỏ rực của thanh gươm
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 0.8f; // Test data 80%

        GameObject passText = AddTMPText(passProgressBar, "180/200", 18);
        passText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 20); // Đẩy text lên phía trên lưỡi gươm xíu
        passText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        passText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // Sub Menu Buttons (Bên phải)
        GameObject subRightMenu = CreateUIElement("SubRightMenu", subTopBar.transform);
        SetAnchor(subRightMenu, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
        subRightMenu.GetComponent<RectTransform>().anchoredPosition = new Vector2(-20, 15);
        
        HorizontalLayoutGroup hlgSub = subRightMenu.AddComponent<HorizontalLayoutGroup>();
        hlgSub.spacing = 30;
        hlgSub.childAlignment = TextAnchor.MiddleRight;
        hlgSub.childControlHeight = true;
        hlgSub.childForceExpandWidth = false;
        
        ContentSizeFitter csfSub = subRightMenu.AddComponent<ContentSizeFitter>();
        csfSub.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csfSub.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        string[] subMenuNames = {"Nhiệm Vụ", "Hòm Thư", "Cafe", "Thiết Lập"};
        for(int i = 0; i < subMenuNames.Length; i++) {
            GameObject btnObj = CreateUIElement("Btn_" + subMenuNames[i], subRightMenu.transform);
            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.preferredWidth = 75;
            le.preferredHeight = 75;
            btnObj.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f); // Nền đen thay icon
            Button btnComp = btnObj.AddComponent<Button>();
            
            if (subMenuNames[i] == "Thiết Lập") {
                LegendOfBlood.UIPanelNavButton navBtn = btnObj.AddComponent<LegendOfBlood.UIPanelNavButton>();
                navBtn.targetPanel = LegendOfBlood.UIPanelType.Menu;
            }
            else if (subMenuNames[i] == "Hòm Thư") {
                LegendOfBlood.UIPanelNavButton navBtn = btnObj.AddComponent<LegendOfBlood.UIPanelNavButton>();
                navBtn.targetPanel = LegendOfBlood.UIPanelType.Mailbox;
            }
            
            GameObject btnLabel = AddTMPText(btnObj, subMenuNames[i], 22);
            SetAnchor(btnLabel, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 1));
            SetOffsets(btnLabel, -40, -40, 0, -30); // Giãn text để không tràn chữ
            btnLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            // Chấm đỏ thông báo cho nút "Thiết Lập"
            if (i == subMenuNames.Length - 1) {
                GameObject badgeObj = CreateUIElement("RedBadge", btnObj.transform);
                SetAnchor(badgeObj, new Vector2(1, 1), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
                badgeObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(5, -5);
                badgeObj.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
                badgeObj.AddComponent<Image>().color = new Color(0.9f, 0.1f, 0.1f, 1f);
                AddTMPText(badgeObj, "1", 18);
            }
        }

        // 4. Castle (Middle Center)
        GameObject castle = CreateUIElement("Castle", safeAreaObj.transform);
        SetAnchorCenter(castle, new Vector2(0, 200), new Vector2(800, 700));
        castle.AddComponent<Image>().color = new Color(0.6f, 0.5f, 0.4f, 1f);

        // 5. Character Group (Middle Center)
        GameObject charGroup = CreateUIElement("CharacterGroup", safeAreaObj.transform);
        SetAnchorCenter(charGroup, new Vector2(0, -100), new Vector2(600, 250));
        charGroup.AddComponent<Image>().color = new Color(0.3f, 0.5f, 0.8f, 0.5f);

        // 6. Bottom Bar (Bottom Stretch)
        GameObject bottomBar = CreateUIElement("BottomBar", safeAreaObj.transform);
        SetAnchor(bottomBar, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
        bottomBar.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);     // Bottom offset
        bottomBar.GetComponent<RectTransform>().offsetMax = new Vector2(0, 250);   // Top offset (Height)
        bottomBar.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

        // 7. Sidebars (Middle Left / Right)
        GameObject leftMenu = CreateUIElement("LeftMenu", safeAreaObj.transform);
        SetAnchor(leftMenu, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        leftMenu.GetComponent<RectTransform>().anchoredPosition = new Vector2(20, 0);
        leftMenu.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 400);
        VerticalLayoutGroup vlgL = leftMenu.AddComponent<VerticalLayoutGroup>();
        vlgL.spacing = 20; vlgL.childControlWidth = true; vlgL.childForceExpandHeight = false;
        ContentSizeFitter csfL = leftMenu.AddComponent<ContentSizeFitter>();
        csfL.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        AddMockButtons(leftMenu.transform, 3, "Quest");

        GameObject rightMenu = CreateUIElement("RightMenu", safeAreaObj.transform);
        SetAnchor(rightMenu, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
        rightMenu.GetComponent<RectTransform>().anchoredPosition = new Vector2(-20, 0);
        rightMenu.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 400);
        VerticalLayoutGroup vlgR = rightMenu.AddComponent<VerticalLayoutGroup>();
        vlgR.spacing = 20; vlgR.childControlWidth = true; vlgR.childForceExpandHeight = false;
        ContentSizeFitter csfR = rightMenu.AddComponent<ContentSizeFitter>();
        csfR.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        AddMockButtons(rightMenu.transform, 3, "Event");

        // 8. Sub Buttons
        GameObject btnSummon = CreateUIElement("Btn_Summon", safeAreaObj.transform);
        SetAnchor(btnSummon, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0));
        btnSummon.GetComponent<RectTransform>().anchoredPosition = new Vector2(100, 350);
        btnSummon.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 160);
        btnSummon.AddComponent<Image>().color = new Color(0.2f, 0.6f, 0.8f, 1f);
        btnSummon.AddComponent<Button>();
        LegendOfBlood.UIPanelNavButton navBtnSummon = btnSummon.AddComponent<LegendOfBlood.UIPanelNavButton>();
        navBtnSummon.targetPanel = LegendOfBlood.UIPanelType.Recruitment;
        AddTMPText(btnSummon, "Summon", 36);

        GameObject btnTerritory = CreateUIElement("Btn_Territory", safeAreaObj.transform);
        SetAnchor(btnTerritory, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0));
        btnTerritory.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, 350);
        btnTerritory.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 160);
        btnTerritory.AddComponent<Image>().color = new Color(0.2f, 0.8f, 0.4f, 1f);
        btnTerritory.AddComponent<Button>();
        AddTMPText(btnTerritory, "Territory", 36);

        // 9. Battle Button (Bottom Center)
        GameObject battleBtn = CreateUIElement("BattleButton", safeAreaObj.transform);
        SetAnchor(battleBtn, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        battleBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 250);
        battleBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 220);
        battleBtn.AddComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);
        battleBtn.AddComponent<Button>();
        battleBtn.AddComponent<PulseAnimation>();
        LegendOfBlood.UIPanelNavButton navBtnBattle = battleBtn.AddComponent<LegendOfBlood.UIPanelNavButton>();
        navBtnBattle.targetPanel = LegendOfBlood.UIPanelType.WorldMap;
        AddTMPText(battleBtn, "BATTLE", 70);

        // 10. Bottom Nav Icons Group inside Bottom Bar
        GameObject bottomNav = CreateUIElement("BottomNavIcons", bottomBar.transform);
        SetAnchor(bottomNav, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(bottomNav, 0, 0, 0, 0);
        bottomNav.AddComponent<LegendOfBlood.UIBottomNavHighlighter>();
        
        string[] navNames = {"Shop", "Barracks", "Lobby", "Hospital", "Battlefield"};
        float[] navAnchorsX = {0.1f, 0.3f, 0.5f, 0.7f, 0.9f};
        
        for (int i = 0; i < navNames.Length; i++) {
            // Bao ngoài to để mở rộng vùng dễ bấm
            GameObject tabObj = CreateUIElement("Tab_" + navNames[i], bottomNav.transform);
            SetAnchor(tabObj, new Vector2(navAnchorsX[i], 0), new Vector2(navAnchorsX[i], 0), new Vector2(0.5f, 0));
            tabObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100); 
            tabObj.GetComponent<RectTransform>().sizeDelta = new Vector2(216, 200); 
            tabObj.AddComponent<Image>().color = new Color(0,0,0,0); // Trong suốt
            
            Button tBtn = tabObj.AddComponent<Button>();
            LegendOfBlood.UIPanelNavButton navBtn = tabObj.AddComponent<LegendOfBlood.UIPanelNavButton>();
            string tab = navNames[i];
            if (tab == "Shop") navBtn.targetPanel = LegendOfBlood.UIPanelType.ArenaShop;
            else if (tab == "Barracks") navBtn.targetPanel = LegendOfBlood.UIPanelType.Barrack;
            else if (tab == "Lobby") navBtn.targetPanel = LegendOfBlood.UIPanelType.MainScreen;
            else if (tab == "Hospital") navBtn.targetPanel = LegendOfBlood.UIPanelType.Hospital;
            else if (tab == "Battlefield") navBtn.targetPanel = LegendOfBlood.UIPanelType.Arena;
            
            GameObject iconObj = CreateUIElement("Icon", tabObj.transform);
            SetAnchorCenter(iconObj, new Vector2(0, 20), new Vector2(100, 100)); // Nhỉnh lên so với giữa 1 tí
            iconObj.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.5f);
            
            GameObject textObj = AddTMPText(tabObj, navNames[i], 28);
            SetAnchorCenter(textObj, new Vector2(0, -60), new Vector2(200, 50));
        }

        // --- Thay thế logic cho Panel_MainScreen ---
        canvasObj.name = "Panel_MainScreen";
        LegendOfBlood.UIPanel uiPan = canvasObj.GetComponent<LegendOfBlood.UIPanel>();
        if (uiPan == null) {
            uiPan = canvasObj.AddComponent<LegendOfBlood.UIPanel>();
            uiPan.PanelType = LegendOfBlood.UIPanelType.MainScreen;
        }

        LegendOfBlood.UIMainController mainCtrl = canvasObj.GetComponent<LegendOfBlood.UIMainController>();
        if (mainCtrl == null) {
            mainCtrl = canvasObj.AddComponent<LegendOfBlood.UIMainController>();
        }

        SerializedObject soCtrl = new SerializedObject(mainCtrl);
        soCtrl.Update();
        
        Transform mailboxBtn = canvasObj.transform.Find("SafeArea/SubTopBar/SubRightMenu/Btn_Hòm Thư");
        if (mailboxBtn != null) {
            Button btnComp = mailboxBtn.GetComponent<Button>();
            if (btnComp == null) btnComp = mailboxBtn.gameObject.AddComponent<Button>();
            SerializedProperty mailProp = soCtrl.FindProperty("mailboxButton");
            if (mailProp != null) mailProp.objectReferenceValue = btnComp;
        }
        
        Transform settingsBtn = canvasObj.transform.Find("SafeArea/SubTopBar/SubRightMenu/Btn_Thiết Lập");
        if (settingsBtn != null) {
            Transform redDot = settingsBtn.Find("RedBadge");
            if (redDot != null) {
                SerializedProperty dotProp = soCtrl.FindProperty("mailboxRedDot");
                if (dotProp != null) dotProp.objectReferenceValue = redDot.gameObject;
            }
        }
        soCtrl.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(canvasObj, "Generate Main Screen UI");
        Selection.activeGameObject = canvasObj;
        Debug.Log("Main Screen UI Generated Successfully!");
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

    private static void AddMockButtons(Transform parent, int count, string prefix) {
        for (int i=0; i<count; i++) {
            GameObject btn = CreateUIElement(prefix + "_" + (i+1), parent);
            btn.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            btn.AddComponent<LayoutElement>().minHeight = 100;
            AddTMPText(btn, prefix + " " + (i+1), 32);
        }
    }
}
#endif


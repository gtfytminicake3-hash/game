#if UNITY_EDITOR
namespace LegendOfBlood.Editor
{
    using UnityEngine;
    using UnityEditor;
    using UnityEngine.UI;
    using LegendOfBlood;
    using TMPro;

    public class UIGeneratorTool : EditorWindow
    {
        [MenuItem("Tools/Legend Of Blood/Generate Missing UI Panels")]
        public static void GenerateUIPanels()
        {
            Canvas mainCanvas = FindAnyObjectByType<Canvas>();
            if (mainCanvas == null)
            {
                GameObject canvasObj = new GameObject("MainCanvas");
                mainCanvas = canvasObj.AddComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                Debug.Log("Created a new Canvas because none was found in the Scene.");
            }

            GenerateSettingsPanel(mainCanvas.transform);
            GenerateBuildingUpgradePanel(mainCanvas.transform);
            GenerateTutorialPanel(mainCanvas.transform);
            GenerateMailboxSystem(mainCanvas.transform);
            GenerateBarrackSystem(mainCanvas.transform);
            GenerateBattleSystem(mainCanvas.transform);

            Debug.Log("<color=green>Thành công! Đã tự động tạo Panel Battle cùng các hệ thống khác.</color>");
        }

        private static GameObject CreatePanelBase(string name, Transform parent)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.85f); // Semi-transparent black

            return panel;
        }

        private static Button CreateButton(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, string textStr)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.sizeDelta = size;
            btnRect.anchoredPosition = anchoredPosition;

            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            Button btn = btnObj.AddComponent<Button>();

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = textStr;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.black;
            // text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // TMPro uses its own font assets

            return btn;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, string textStr, int fontSize = 24)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.sizeDelta = size;
            textRect.anchoredPosition = anchoredPosition;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = textStr;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.fontSize = fontSize;

            return text;
        }

        private static void GenerateSettingsPanel(Transform parent)
        {
            GameObject panel = CreatePanelBase("SettingsPanel", parent);
            SettingsPanel script = panel.AddComponent<SettingsPanel>();
            
            CreateText("Title", panel.transform, new Vector2(0, 300), new Vector2(400, 50), "CÀI ĐẶT HỆ THỐNG", 36);
            Button closeBtn = CreateButton("CloseButton", panel.transform, new Vector2(0, -300), new Vector2(200, 50), "ĐÓNG");
            Button langBtn = CreateButton("LanguageButton", panel.transform, new Vector2(0, 100), new Vector2(300, 60), "CHUYỂN ĐỔI NGÔN NGỮ");
            
            // Slider mock
            GameObject sliderObj = new GameObject("VolumeSlider");
            sliderObj.transform.SetParent(panel.transform, false);
            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(300, 20);
            sliderRect.anchoredPosition = new Vector2(0, 0);
            Slider slider = sliderObj.AddComponent<Slider>();

            // Gán reference vào Script (Wire-up component automatically)
            var so = new SerializedObject(script);
            so.FindProperty("closeButton").objectReferenceValue = closeBtn;
            so.FindProperty("changeLanguageButton").objectReferenceValue = langBtn;
            so.FindProperty("volumeSlider").objectReferenceValue = slider;
            so.ApplyModifiedProperties();

            panel.SetActive(false);
        }

        private static void GenerateBuildingUpgradePanel(Transform parent)
        {
            GameObject panel = CreatePanelBase("BuildingUpgradePanel", parent);
            BuildingUpgradePanel script = panel.AddComponent<BuildingUpgradePanel>();

            TextMeshProUGUI title = CreateText("TitleText", panel.transform, new Vector2(0, 300), new Vector2(500, 50), "NÂNG CẤP CÔNG TRÌNH", 36);
            TextMeshProUGUI info = CreateText("InfoText", panel.transform, new Vector2(0, 100), new Vector2(400, 100), "Cấp hiện tại: 1\nCấp tiếp theo: 2", 24);
            TextMeshProUGUI cost = CreateText("CostText", panel.transform, new Vector2(0, 0), new Vector2(400, 50), "Giá: 1000 Vàng", 24);
            
            Button upgradeBtn = CreateButton("UpgradeButton", panel.transform, new Vector2(0, -100), new Vector2(250, 60), "NÂNG CẤP");
            Button closeBtn = CreateButton("CloseButton", panel.transform, new Vector2(0, -300), new Vector2(200, 50), "HỦY BỎ");

            var so = new SerializedObject(script);
            so.FindProperty("closeButton").objectReferenceValue = closeBtn;
            so.FindProperty("upgradeButton").objectReferenceValue = upgradeBtn;
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("infoText").objectReferenceValue = info;
            so.FindProperty("costText").objectReferenceValue = cost;
            so.ApplyModifiedProperties();

            panel.SetActive(false);
        }

        private static void GenerateTutorialPanel(Transform parent)
        {
            GameObject panel = CreatePanelBase("TutorialPanel", parent);
            TutorialPanel script = panel.AddComponent<TutorialPanel>();

            CreateText("TutorialTitle", panel.transform, new Vector2(0, 300), new Vector2(400, 50), "HƯỚNG DẪN TÂN THỦ", 36);
            TextMeshProUGUI info = CreateText("TutorialText", panel.transform, new Vector2(0, 50), new Vector2(600, 200), "Chào mừng đến với Legend of Blood!", 28);
            
            Button prevBtn = CreateButton("PrevButton", panel.transform, new Vector2(-150, -200), new Vector2(150, 60), "LÙI LẠI");
            Button nextBtn = CreateButton("NextButton", panel.transform, new Vector2(150, -200), new Vector2(150, 60), "TIẾP THEO");
            Button closeBtn = CreateButton("CloseButton", panel.transform, new Vector2(0, -350), new Vector2(200, 50), "BỎ QUA");

            var so = new SerializedObject(script);
            so.FindProperty("closeButton").objectReferenceValue = closeBtn;
            so.FindProperty("nextButton").objectReferenceValue = nextBtn;
            so.FindProperty("prevButton").objectReferenceValue = prevBtn;
            so.FindProperty("tutorialText").objectReferenceValue = info;
            so.ApplyModifiedProperties();

            panel.SetActive(false);
        }
        // --- VŨ KHÍ TỐI THƯỢNG: SINH TỰ ĐỘNG HÒM THƯ HIỆN ĐẠI ---
        private static void GenerateMailboxSystem(Transform mainCanvas)
        {
            // 1. TÌM PANEL_MAINSCREEN ĐỂ NHÉT NÚT BẤM VÀO LÀNG
            Transform mainScreen = mainCanvas.Find("Panel_MainScreen");
            if (mainScreen == null) {
                Debug.LogWarning("Không tìm thấy Panel_MainScreen để gắn nút Hòm thư. Đang tạo tạm một nút ngoài Canvas.");
                mainScreen = mainCanvas;
            }

            // A. Tạo Nút Hòm Thư
            Transform existingBtn = mainScreen.Find("MailboxButton");
            if (existingBtn != null) DestroyImmediate(existingBtn.gameObject);

            Button mailBtn = CreateButton("MailboxButton", mainScreen, new Vector2(-100, 100), new Vector2(150, 150), "Hòm Thư\n(Mailbox)");
            RectTransform mailBtnRect = mailBtn.GetComponent<RectTransform>();
            mailBtnRect.anchorMin = new Vector2(1, 0); // Góc dưới bên phải
            mailBtnRect.anchorMax = new Vector2(1, 0);
            mailBtnRect.pivot = new Vector2(1, 0);
            
            // A.1 Tạo Red Dot (Chấm đỏ thông báo) dính vào Nút Hòm Thư
            GameObject redDot = new GameObject("RedDotNotification");
            redDot.transform.SetParent(mailBtn.transform, false);
            RectTransform redDotRect = redDot.AddComponent<RectTransform>();
            redDotRect.sizeDelta = new Vector2(40, 40);
            redDotRect.anchorMin = new Vector2(1, 1);
            redDotRect.anchorMax = new Vector2(1, 1);
            redDotRect.anchoredPosition = new Vector2(-10, -10);
            Image redDotImg = redDot.AddComponent<Image>();
            redDotImg.color = Color.red;

            // Nối dây Tự động vào UIMainController
            UIMainController mainController = mainScreen.GetComponent<UIMainController>();
            if (mainController == null) mainController = mainScreen.gameObject.AddComponent<UIMainController>();
            
            SerializedObject soCtrl = new SerializedObject(mainController);
            soCtrl.Update();
            soCtrl.FindProperty("mailboxButton").objectReferenceValue = mailBtn;
            soCtrl.FindProperty("mailboxRedDot").objectReferenceValue = redDot;
            soCtrl.ApplyModifiedProperties();

            // 2. TẠO PANEL_MAILBOX (Giao diện bảng đen để đọc thư)
            Transform existingPanel = mainCanvas.Find("Panel_Mailbox");
            if (existingPanel != null) DestroyImmediate(existingPanel.gameObject);

            GameObject mailPanel = CreatePanelBase("Panel_Mailbox", mainCanvas);
            UIPanel uiPan = mailPanel.AddComponent<UIPanel>();
            uiPan.PanelType = UIPanelType.Mailbox;
            MailboxPanel mailboxScript = mailPanel.AddComponent<MailboxPanel>();

            TextMeshProUGUI title = CreateText("Title", mailPanel.transform, new Vector2(0, -100), new Vector2(600, 100), "HÒM THƯ (INBOX)", 60);

            Button closeBtn = CreateButton("CloseButton", mailPanel.transform, new Vector2(0, 50), new Vector2(800, 100), "Trở về Làng (Đóng)");
            closeBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0); // Dưới cùng
            closeBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
            
            Button claimBtn = CreateButton("ClaimAllButton", mailPanel.transform, new Vector2(0, 170), new Vector2(800, 100), "Nhận Tất Cả Quà");
            claimBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0); // Ngay trên nút đóng
            claimBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
            claimBtn.GetComponent<Image>().color = new Color(1f, 0.8f, 0.2f); // Nút nhận quà màu vàng!

            // 2.1 Tạo Khay chứa Thư (ScrollView)
            GameObject scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(mailPanel.transform, false);
            RectTransform svRect = scrollView.AddComponent<RectTransform>();
            svRect.anchorMin = new Vector2(0, 0); svRect.anchorMax = new Vector2(1, 1);
            svRect.offsetMin = new Vector2(100, 300); svRect.offsetMax = new Vector2(-100, -200);

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);
            viewport.AddComponent<RectTransform>().anchorMin = Vector2.zero;
            viewport.GetComponent<RectTransform>().anchorMax = Vector2.one;
            viewport.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            viewport.AddComponent<Image>().color = new Color(1, 1, 1, 0.1f);
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1); contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 1000);
            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true; vlg.childControlHeight = false; vlg.childForceExpandHeight = false; vlg.spacing = 20; vlg.padding = new RectOffset(20, 20, 20, 20);
            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
            scrollRect.content = contentRect;
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.horizontal = false;

            // 2.2 TẠO ITEM TEMPLATE (Giao diện 1 lá thư hoàn chỉnh) để chứa làm Prefab Ảo
            GameObject itemTemplate = new GameObject("ReportItemTemplate");
            itemTemplate.transform.SetParent(content.transform, false);
            itemTemplate.SetActive(false); // Ẩn đi vì nó chỉ là khuôn đúc mẫu
            
            RectTransform itemRect = itemTemplate.AddComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(760, 120);
            itemTemplate.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

            // Tên Địa Điểm
            GameObject poiObj = new GameObject("PoiName");
            poiObj.transform.SetParent(itemTemplate.transform, false);
            RectTransform poiRect = poiObj.AddComponent<RectTransform>();
            poiRect.anchorMin = new Vector2(0, 0); poiRect.anchorMax = new Vector2(0.5f, 1);
            poiRect.offsetMin = new Vector2(20, 0); poiRect.offsetMax = new Vector2(0, 0);
            TMPro.TextMeshProUGUI poiTxt = poiObj.AddComponent<TMPro.TextMeshProUGUI>();
            poiTxt.text = "Tên Địa Điểm Khám Phá";
            poiTxt.fontSize = 24;
            poiTxt.alignment = TMPro.TextAlignmentOptions.Left;

            // Kết quả Thắng/Thua
            GameObject outcomeObj = new GameObject("OutcomeText");
            outcomeObj.transform.SetParent(itemTemplate.transform, false);
            RectTransform outRect = outcomeObj.AddComponent<RectTransform>();
            outRect.anchorMin = new Vector2(0.5f, 0); outRect.anchorMax = new Vector2(0.8f, 1);
            outRect.offsetMin = Vector2.zero; outRect.offsetMax = Vector2.zero;
            TMPro.TextMeshProUGUI outTxt = outcomeObj.AddComponent<TMPro.TextMeshProUGUI>();
            outTxt.text = "Kết Quả";
            outTxt.fontSize = 28;
            outTxt.alignment = TMPro.TextAlignmentOptions.Center;

            // Nút Claim (Nhận phần thưởng)
            Button claimItemBtn = CreateButton("ClaimButton", itemTemplate.transform, new Vector2(0, 0), new Vector2(150, 80), "NHẬN");
            RectTransform cibRect = claimItemBtn.GetComponent<RectTransform>();
            cibRect.anchorMin = new Vector2(1, 0.5f); cibRect.anchorMax = new Vector2(1, 0.5f);
            cibRect.pivot = new Vector2(1, 0.5f);
            cibRect.anchoredPosition = new Vector2(-20, 0);

            // Gắn Script ExpeditionReportItem
            ExpeditionReportItem itemScript = itemTemplate.AddComponent<ExpeditionReportItem>();
            SerializedObject soItem = new SerializedObject(itemScript);
            soItem.Update();
            soItem.FindProperty("poiNameText").objectReferenceValue = poiTxt;
            soItem.FindProperty("outcomeText").objectReferenceValue = outTxt;
            soItem.FindProperty("claimButton").objectReferenceValue = claimItemBtn;
            soItem.ApplyModifiedProperties();

            // Nối dây Tự động vào Script MailboxPanel
            SerializedObject soMail = new SerializedObject(mailboxScript);
            soMail.Update();
            soMail.FindProperty("closeButton").objectReferenceValue = closeBtn;
            soMail.FindProperty("claimAllButton").objectReferenceValue = claimBtn;
            soMail.FindProperty("reportItemsContainer").objectReferenceValue = content.transform;
            soMail.FindProperty("reportItemPrefab").objectReferenceValue = itemTemplate;
            soMail.ApplyModifiedProperties();

            // Tắt nó đi theo chuẩn chung
            mailPanel.SetActive(false);
            
            Debug.Log("<color=cyan>HÒM THƯ ĐÃ ĐƯỢC CHẾ TẠO THÀNH CÔNG</color>");
        }

        // --- HỆ THỐNG DANH SÁCH ANH HÙNG (BARRACK) ---
        private static void GenerateBarrackSystem(Transform mainCanvas)
        {
            Transform existingPanel = mainCanvas.Find("Panel_Barrack");
            if (existingPanel != null) DestroyImmediate(existingPanel.gameObject);

            GameObject barrackPanel = CreatePanelBase("Panel_Barrack", mainCanvas);
            UIPanel uiPan = barrackPanel.AddComponent<UIPanel>();
            uiPan.PanelType = UIPanelType.Barrack;
            BarrackPanel barrackScript = barrackPanel.AddComponent<BarrackPanel>();

            TextMeshProUGUI title = CreateText("BarrackTitle", barrackPanel.transform, new Vector2(0, -50), new Vector2(600, 100), "QUẢN LÝ TƯỚNG (BARRACK)", 50);

            Button closeBtn = CreateButton("CloseButton", barrackPanel.transform, new Vector2(0, 50), new Vector2(800, 100), "Trở về Làng (Đóng)");
            closeBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0); // Dưới cùng
            closeBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);

            // Tạo ScrollView chứa Danh sách Tướng
            GameObject scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(barrackPanel.transform, false);
            RectTransform svRect = scrollView.AddComponent<RectTransform>();
            svRect.anchorMin = new Vector2(0, 0); svRect.anchorMax = new Vector2(1, 1);
            svRect.offsetMin = new Vector2(100, 200); svRect.offsetMax = new Vector2(-100, -200);

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);
            viewport.AddComponent<RectTransform>().anchorMin = Vector2.zero;
            viewport.GetComponent<RectTransform>().anchorMax = Vector2.one;
            viewport.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            viewport.AddComponent<Image>().color = new Color(1, 1, 1, 0.1f);
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1); contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 1000);
            
            // Dùng GridLayoutGroup thay vì Vertical để xếp Tướng thành lưới nhiều cột
            GridLayoutGroup grid = content.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(300, 450); // Phù hợp kích thước thẻ bài
            grid.spacing = new Vector2(50, 50);
            grid.padding = new RectOffset(50, 50, 50, 50);
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;

            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
            scrollRect.content = contentRect;
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.horizontal = false;

            // Tìm Prefab HeroCard trong Project
            GameObject heroCardPrefab = null;
            string[] guids = AssetDatabase.FindAssets("t:Prefab HeroCard");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                heroCardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            if (heroCardPrefab == null)
            {
                Debug.LogWarning("Không tìm thấy Prefab HeroCard nào trong dự án. Bạn cần gán bằng tay vào script BarrackPanel nhé!");
            }

            // Nối dây Tự động vào Script BarrackPanel
            SerializedObject soBarrack = new SerializedObject(barrackScript);
            soBarrack.Update();
            soBarrack.FindProperty("closeButton").objectReferenceValue = closeBtn;
            soBarrack.FindProperty("heroListContainer").objectReferenceValue = content.transform;
            if (heroCardPrefab != null)
            {
                soBarrack.FindProperty("heroCardPrefab").objectReferenceValue = heroCardPrefab;
            }
            soBarrack.ApplyModifiedProperties();

            // Tắt Panel
            barrackPanel.SetActive(false);
            
            Debug.Log("<color=cyan>DOANH TRẠI ĐÃ ĐƯỢC CHẾ TẠO THÀNH CÔNG</color>");
        }

        // --- HỆ THỐNG RẠP CHIẾU PHIM CHIẾN ĐẤU (BATTLE VISUALIZER) ---
        private static void GenerateBattleSystem(Transform mainCanvas)
        {
            Transform existingPanel = mainCanvas.Find("Panel_Battle");
            if (existingPanel != null) DestroyImmediate(existingPanel.gameObject);

            GameObject battlePanel = CreatePanelBase("Panel_Battle", mainCanvas);
            UIPanel uiPan = battlePanel.AddComponent<UIPanel>();
            uiPan.PanelType = UIPanelType.Battle;
            
            // Xóa background đen nhánh mặc định của Base, dùng màu xám xịt làm nền Đấu Trường
            battlePanel.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 1f);

            LegendOfBlood.Combat.CombatVisualizerPanel visualizerScript = battlePanel.AddComponent<LegendOfBlood.Combat.CombatVisualizerPanel>();

            CreateText("BattleTitle", battlePanel.transform, new Vector2(0, -50), new Vector2(600, 100), "ĐẤU TRƯỜNG HUYỀN THOẠI", 50);

            // Container Phe Ta (Trái)
            GameObject allyContainer = new GameObject("AllyContainer");
            allyContainer.transform.SetParent(battlePanel.transform, false);
            RectTransform acRect = allyContainer.AddComponent<RectTransform>();
            acRect.anchorMin = new Vector2(0, 0.1f); acRect.anchorMax = new Vector2(0.5f, 0.9f);
            acRect.offsetMin = new Vector2(50, 0); acRect.offsetMax = new Vector2(-50, 0);
            GridLayoutGroup allyGrid = allyContainer.AddComponent<GridLayoutGroup>();
            allyGrid.cellSize = new Vector2(250, 350);
            allyGrid.spacing = new Vector2(20, 20);
            // Xếp từ Phải sang Trái để FrontLine đứng giữa màn hình
            allyGrid.startCorner = GridLayoutGroup.Corner.UpperRight; 
            allyGrid.startAxis = GridLayoutGroup.Axis.Vertical;
            allyGrid.childAlignment = TextAnchor.MiddleRight;

            // Container Phe Địch (Phải)
            GameObject enemyContainer = new GameObject("EnemyContainer");
            enemyContainer.transform.SetParent(battlePanel.transform, false);
            RectTransform ecRect = enemyContainer.AddComponent<RectTransform>();
            ecRect.anchorMin = new Vector2(0.5f, 0.1f); ecRect.anchorMax = new Vector2(1, 0.9f);
            ecRect.offsetMin = new Vector2(50, 0); ecRect.offsetMax = new Vector2(-50, 0);
            GridLayoutGroup enemyGrid = enemyContainer.AddComponent<GridLayoutGroup>();
            enemyGrid.cellSize = new Vector2(250, 350);
            enemyGrid.spacing = new Vector2(20, 20);
            // Xếp từ Trái sang Phải để FrontLine đứng giữa
            enemyGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            enemyGrid.startAxis = GridLayoutGroup.Axis.Vertical;
            enemyGrid.childAlignment = TextAnchor.MiddleLeft;

            // Nút điều khiển (Phía dưới)
            Button skipBtn = CreateButton("SkipButton", battlePanel.transform, new Vector2(-150, 50), new Vector2(250, 80), "BỎ QUA (SKIP)");
            skipBtn.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0); skipBtn.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0);
            
            Button speedBtn = CreateButton("SpeedButton", battlePanel.transform, new Vector2(-450, 50), new Vector2(250, 80), "TỐC ĐỘ: x1");
            speedBtn.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0); speedBtn.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0);
            TextMeshProUGUI speedText = speedBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

            // Màn hình Thắng/Thua (Sinh ra ẩn)
            GameObject victoryScreen = CreatePanelBase("VictoryScreen", battlePanel.transform);
            victoryScreen.GetComponent<Image>().color = new Color(0, 0.5f, 0, 0.9f); // Xanh lá
            CreateText("VictoryText", victoryScreen.transform, new Vector2(0, 100), new Vector2(800, 200), "CHIẾN THẮNG!", 100);
            Button closeVicBtn = CreateButton("CloseVictoryBtn", victoryScreen.transform, new Vector2(0, -100), new Vector2(300, 100), "Trở Về Làng");
            victoryScreen.SetActive(false);

            GameObject defeatScreen = CreatePanelBase("DefeatScreen", battlePanel.transform);
            defeatScreen.GetComponent<Image>().color = new Color(0.5f, 0, 0, 0.9f); // Đỏ 
            CreateText("DefeatText", defeatScreen.transform, new Vector2(0, 100), new Vector2(800, 200), "THẤT BẠI!", 100);
            Button closeDefBtn = CreateButton("CloseDefeatBtn", defeatScreen.transform, new Vector2(0, -100), new Vector2(300, 100), "Trở Về Làng");
            defeatScreen.SetActive(false);

            // Tự chế ra Prefab BattleUnitUI ngay trong Scene để gán, hoặc tìm Asset
            GameObject unitTemplate = new GameObject("BattleUnitTemplate");
            unitTemplate.SetActive(false);
            RectTransform utRect = unitTemplate.AddComponent<RectTransform>();
            utRect.sizeDelta = new Vector2(200, 300);
            
            Image avatarImg = new GameObject("Avatar").AddComponent<Image>();
            avatarImg.transform.SetParent(unitTemplate.transform, false);
            avatarImg.rectTransform.anchorMin = Vector2.zero; avatarImg.rectTransform.anchorMax = Vector2.one;
            avatarImg.rectTransform.sizeDelta = Vector2.zero;
            avatarImg.color = Color.gray; // Placeholder

            Slider hpSlider = new GameObject("HpSlider").AddComponent<Slider>();
            hpSlider.transform.SetParent(unitTemplate.transform, false);
            hpSlider.interactable = false;
            hpSlider.transition = Selectable.Transition.None;
            RectTransform hsRect = hpSlider.GetComponent<RectTransform>();
            hsRect.anchorMin = new Vector2(0, 1); hsRect.anchorMax = new Vector2(1, 1); // Cắm trên đầu
            hsRect.pivot = new Vector2(0.5f, 0); hsRect.anchoredPosition = new Vector2(0, 10);
            hsRect.sizeDelta = new Vector2(0, 30);
            // Tạo background đỏ + Fill xanh lá
            GameObject bgObj = new GameObject("Background"); bgObj.transform.SetParent(hpSlider.transform, false);
            Image bgImg = bgObj.AddComponent<Image>(); bgImg.color = Color.red;
            bgImg.rectTransform.anchorMin = Vector2.zero; bgImg.rectTransform.anchorMax = Vector2.one; bgImg.rectTransform.sizeDelta = Vector2.zero;
            GameObject fillArea = new GameObject("Fill Area"); fillArea.transform.SetParent(hpSlider.transform, false);
            RectTransform faRect = fillArea.AddComponent<RectTransform>(); faRect.anchorMin = Vector2.zero; faRect.anchorMax = Vector2.one; faRect.sizeDelta = Vector2.zero;
            GameObject fillObj = new GameObject("Fill"); fillObj.transform.SetParent(fillArea.transform, false);
            Image fillImg = fillObj.AddComponent<Image>(); fillImg.color = Color.green;
            fillImg.rectTransform.anchorMin = Vector2.zero; fillImg.rectTransform.anchorMax = Vector2.one; fillImg.rectTransform.sizeDelta = Vector2.zero;
            hpSlider.fillRect = fillImg.rectTransform;

            TextMeshProUGUI hpTxt = CreateText("HpText", hpSlider.transform, Vector2.zero, new Vector2(200, 30), "100/100", 20);
            hpTxt.color = Color.white; hpTxt.alignment = TextAlignmentOptions.Center;

            GameObject dmgCgObj = new GameObject("DamageTextContainer");
            dmgCgObj.transform.SetParent(unitTemplate.transform, false);
            RectTransform dmgRect = dmgCgObj.AddComponent<RectTransform>();
            CanvasGroup dmgCg = dmgCgObj.AddComponent<CanvasGroup>();
            dmgRect.anchorMin = new Vector2(0, 0.5f); dmgRect.anchorMax = new Vector2(1, 1.5f);
            
            TextMeshProUGUI dmgTxt = CreateText("DamageText", dmgCg.transform, Vector2.zero, new Vector2(300, 100), "-999", 40);
            dmgTxt.fontStyle = FontStyles.Bold; dmgTxt.alignment = TextAlignmentOptions.Center;

            BattleUnitUI unitScript = unitTemplate.AddComponent<BattleUnitUI>();
            unitScript.avatarImage = avatarImg;
            unitScript.hpSlider = hpSlider;
            unitScript.hpText = hpTxt;
            unitScript.damageTextCanvasGroup = dmgCg;
            unitScript.damageText = dmgTxt;

            // Móc dây vào CombatVisualizerPanel
            SerializedObject soVis = new SerializedObject(visualizerScript);
            soVis.Update();
            soVis.FindProperty("allyContainer").objectReferenceValue = allyContainer.transform;
            soVis.FindProperty("enemyContainer").objectReferenceValue = enemyContainer.transform;
            soVis.FindProperty("skipButton").objectReferenceValue = skipBtn;
            soVis.FindProperty("x2SpeedButton").objectReferenceValue = speedBtn;
            soVis.FindProperty("_speedText").objectReferenceValue = speedText;
            soVis.FindProperty("victoryScreen").objectReferenceValue = victoryScreen;
            soVis.FindProperty("defeatScreen").objectReferenceValue = defeatScreen;
            soVis.FindProperty("closeVictoryButton").objectReferenceValue = closeVicBtn;
            soVis.FindProperty("closeDefeatButton").objectReferenceValue = closeDefBtn;
            soVis.FindProperty("battleUnitPrefab").objectReferenceValue = unitTemplate;
            soVis.ApplyModifiedProperties();

            battlePanel.SetActive(false);
            
            Debug.Log("<color=cyan>ĐẤU TRƯỜNG COMBAT ĐÃ ĐƯỢC XÂY DỰNG - SN SÀNG MỞ CỬA!</color>");
        }
    }
}
#endif

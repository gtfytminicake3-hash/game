using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using LegendOfBlood;
using LegendOfBlood.UI;

public class AutoUIBuilder : EditorWindow
{
    // Tạo một menu mới trên thanh công cụ của Unity
    [MenuItem("Tools/Tạo Prefab Đợt 1")]
    public static void BuildPrefabs()
    {
        CreateReportItemPrefab();
        CreateSquadSelectionHeroCard();
        FixSquadSlotPrefab();
        
        // Làm mới Project window để hiển thị các file vừa tạo
        AssetDatabase.Refresh();
        Debug.Log("<color=green>Đã hoàn thành: Chạy tạo và sửa Prefab Đợt 1 thành công!</color>");
    }

    static void CreateReportItemPrefab()
    {
        // Bước 1: Tạo đối tượng Panel gốc
        GameObject panelGO = new GameObject("ReportItem_Prefab", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        
        // Bước 2: Gắn Script
        var reportScript = panelGO.AddComponent<ExpeditionReportItem>();

        // Bước 3: Tạo các đối tượng con
        GameObject poiTextGO = CreateTextPro("POINameText", panelGO.transform, "Tên Màn");
        GameObject resultTextGO = CreateTextPro("ResultText", panelGO.transform, "Thắng/Thua");
        GameObject claimBtnGO = CreateButtonWithText("ClaimBtn", panelGO.transform, "Nhận Thưởng");
        GameObject replayBtnGO = CreateButtonWithText("ReplayBtn", panelGO.transform, "Xem Lại");

        // Bước 4: Gán các UI Reference vào Script 
        // Lưu ý: Đảm bảo tên biến (poiNameText, resultText...) khớp với tên biến trong file ExpeditionReportItem.cs của bạn
        AssignPrivateField(reportScript, "poiNameText", poiTextGO.GetComponent<TextMeshProUGUI>());
        AssignPrivateField(reportScript, "outcomeText", resultTextGO.GetComponent<TextMeshProUGUI>());
        AssignPrivateField(reportScript, "claimButton", claimBtnGO.GetComponent<Button>());
        AssignPrivateField(reportScript, "replayButton", replayBtnGO.GetComponent<Button>());

        // Bước 5: Lưu thành Prefab và xóa trên Scene
        string path = "Assets/ReportItem_Prefab.prefab";
        PrefabUtility.SaveAsPrefabAsset(panelGO, path);
        DestroyImmediate(panelGO);
    }

    static void CreateSquadSelectionHeroCard()
    {
        // Bước 1: Tìm Prefab cũ
        string[] guids = AssetDatabase.FindAssets("HeroCard_Prefab t:Prefab");
        if (guids.Length == 0)
        {
            Debug.LogError("Không tìm thấy HeroCard_Prefab trong project!");
            return;
        }
        string oldPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        GameObject oldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(oldPath);

        // Duplicate bằng cách Instantiate
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(oldPrefab);
        instance.name = "HeroCard_SquadSelection_Prefab";

        // Bước 3: Thêm Script mới
        var newScript = instance.AddComponent<SquadSelectionHeroCard>();

        // Bước 4: Tạo nút SelectBtn to bự
        GameObject selectBtnGO = CreateButtonWithText("SelectBtn", instance.transform, "CHỌN VÀO ĐỘI");
        RectTransform btnRect = selectBtnGO.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(200, 60); // Làm cho nút to ra

        // Bước 5: Gán reference
        AssignPrivateField(newScript, "selectButton", selectBtnGO.GetComponent<Button>());

        // Bước 6: Lưu đè xuống Project làm bản sao và xóa trên Scene
        PrefabUtility.SaveAsPrefabAsset(instance, "Assets/HeroCard_SquadSelection_Prefab.prefab");
        DestroyImmediate(instance);
    }

    static void FixSquadSlotPrefab()
    {
        // Bước 1: Tìm mở Prefab SquadSlot_Prefab
        string[] guids = AssetDatabase.FindAssets("SquadSlot_Prefab t:Prefab");
        if (guids.Length == 0)
        {
            Debug.LogError("Không tìm thấy SquadSlot_Prefab để sửa!");
            return;
        }
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));

        // Mở script hiện có
        var slotScript = instance.GetComponent<SquadSlotCard>();
        if (slotScript == null)
        {
            Debug.LogWarning("Không tìm thấy Component SquadSlotCard trên SquadSlot_Prefab");
        }

        // Bước 3: Vẽ thêm nút Xóa (RemoveBtn)
        GameObject removeBtnGO = new GameObject("RemoveBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        removeBtnGO.transform.SetParent(instance.transform, false);
        removeBtnGO.GetComponent<Image>().color = Color.red; // Giả lập icon dấu X đỏ chà bá

        // Đặt góc phải
        RectTransform rect = removeBtnGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(40, 40);

        // Bước 4: Gán vào tham chiếu
        if (slotScript != null)
        {
            // Tùy vào tên biến thực tế trong script của bạn, hãy đổi 'slotButton' thành biến tương ứng.
            AssignPrivateField(slotScript, "slotButton", removeBtnGO.GetComponent<Button>());
        }

        // Lưu Prefab lại
        PrefabUtility.SaveAsPrefabAsset(instance, path);
        DestroyImmediate(instance);
    }

    // --- Các hàm tiện ích hỗ trợ tạo UI nhanh --- //

    static GameObject CreateTextPro(string name, Transform parent, string defaultText)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        go.GetComponent<TextMeshProUGUI>().text = defaultText;
        return go;
    }

    static GameObject CreateButtonWithText(string name, Transform parent, string text)
    {
        GameObject btnGO = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(parent, false);
        
        GameObject txtGO = CreateTextPro("Text (TMP)", btnGO.transform, text);
        txtGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        txtGO.GetComponent<TextMeshProUGUI>().color = Color.black;

        return btnGO;
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
            UnityEngine.Debug.LogError($"Field {fieldName} not found in {target.GetType()}");
        }
    }
}

public class AutoPanelBuilder : EditorWindow
{
    [MenuItem("Tools/Tạo Panel Đợt 2 (Inventory, Quest, Gacha, Class)")]
    public static void BuildPanels()
    {
        CreateInventoryPanel();
        CreateQuestPanel();
        CreateRecruitmentPanel();
        CreateProfessionSelectionPanel();

        // Làm mới Project window để nhận diện Prefab mới
        AssetDatabase.Refresh();
        Debug.Log("<color=cyan>Đã hoàn thành: Dựng 4 Panel và các Component đi kèm thành công!</color>");
    }

    static void CreateInventoryPanel()
    {
        GameObject panelGO = CreateUIObject("Panel_Inventory", null);
        var script = panelGO.AddComponent<InventoryPanel>();
        // LƯU Ý: Giả định biến enum Panel Type tên là panelType. Hãy sửa lại cho khớp Script của bạn.
        // AssignPrivateField(script, "panelType", PanelType.Inventory); 

        AssignPrivateField(script, "closeButton", CreateCloseButton(panelGO.transform));
        
        // Tab Group
        GameObject tabGroupObj = CreateUIObject("TabGroup", panelGO.transform);
        RectTransform tabRect = tabGroupObj.GetComponent<RectTransform>();
        tabRect.anchorMin = new Vector2(0, 1); tabRect.anchorMax = new Vector2(1, 1);
        tabRect.pivot = new Vector2(0.5f, 1); tabRect.sizeDelta = new Vector2(0, 100);
        var tabHlg = tabGroupObj.AddComponent<HorizontalLayoutGroup>();
        tabHlg.childControlWidth = true; tabHlg.childForceExpandWidth = true;
        
        AssignPrivateField(script, "itemTabButton", CreateButton("Btn_ItemTab", tabGroupObj.transform, "Items"));
        AssignPrivateField(script, "equipmentTabButton", CreateButton("Btn_EquipmentTab", tabGroupObj.transform, "Equipments"));

        // 2 GameObject rỗng stretch
        GameObject itemContainerGO = CreateEmptyStretch("Page_Items", panelGO.transform);
        AssignPrivateField(script, "itemContainerObj", itemContainerGO);
        GameObject equipmentContainerGO = CreateEmptyStretch("Page_Equipments", panelGO.transform);
        AssignPrivateField(script, "equipmentContainerObj", equipmentContainerGO);

        // Scroll View cho Items
        RectTransform tempItemContent;
        CreateScrollView("Scroll View", itemContainerGO.transform, out tempItemContent);
        AssignPrivateField(script, "itemContent", tempItemContent);
        
        // Thẻ bài Item
        GameObject itemCardGO = CreateUIObject("InventoryItemCard_Prefab", null);
        itemCardGO.AddComponent<InventoryItemCard>();
        AssignPrivateField(script, "itemCardPrefab", SaveAsPrefab(itemCardGO, "InventoryItemCard_Prefab"));

        // Scroll View cho Equipments
        RectTransform tempEquipContent;
        CreateScrollView("Scroll View", equipmentContainerGO.transform, out tempEquipContent);
        AssignPrivateField(script, "equipmentContent", tempEquipContent);

        // Thẻ bài Equipment
        GameObject equipCardGO = CreateUIObject("InventoryEquipmentCard_Prefab", null);
        equipCardGO.AddComponent<InventoryEquipmentCard>();
        AssignPrivateField(script, "equipmentCardPrefab", SaveAsPrefab(equipCardGO, "InventoryEquipmentCard_Prefab"));

        SaveAsPrefab(panelGO, "Panel_Inventory");
    }

    static void CreateQuestPanel()
    {
        GameObject panelGO = CreateUIObject("Panel_Quest", null);
        var script = panelGO.AddComponent<QuestPanel>();

        AssignPrivateField(script, "closeButton", CreateCloseButton(panelGO.transform));

        // Tab ngang
        GameObject tabGroupObj = CreateUIObject("TabGroup", panelGO.transform);
        RectTransform tabRect = tabGroupObj.GetComponent<RectTransform>();
        tabRect.anchorMin = new Vector2(0, 1); tabRect.anchorMax = new Vector2(1, 1);
        tabRect.pivot = new Vector2(0.5f, 1); tabRect.sizeDelta = new Vector2(0, 100);
        var tabHlg = tabGroupObj.AddComponent<HorizontalLayoutGroup>();
        tabHlg.childControlWidth = true; tabHlg.childForceExpandWidth = true;

        AssignPrivateField(script, "mainTabButton", CreateButton("Btn_Main", tabGroupObj.transform, "Main Quest"));
        AssignPrivateField(script, "dailyTabButton", CreateButton("Btn_Daily", tabGroupObj.transform, "Daily"));
        AssignPrivateField(script, "weeklyTabButton", CreateButton("Btn_Weekly", tabGroupObj.transform, "Weekly"));

        // Scrollview list ở dưới tab
        GameObject listGroup = CreateEmptyStretch("ListArea", panelGO.transform);
        listGroup.GetComponent<RectTransform>().offsetMax = new Vector2(0, -100); // Cách top 100
        
        RectTransform tempQuestListContainer;
        CreateScrollView("Scroll View", listGroup.transform, out tempQuestListContainer);
        AssignPrivateField(script, "questListContainer", tempQuestListContainer);

        // Prefab thẻ Quest
        GameObject questItemGO = CreateUIObject("QuestItem_Prefab", null);
        questItemGO.AddComponent<QuestItemUI>();
        CreateButton("Btn_Claim", questItemGO.transform, "Nhận thưởng"); // Button nhận thưởng ngang
        AssignPrivateField(script, "questItemPrefab", SaveAsPrefab(questItemGO, "QuestItem_Prefab"));

        SaveAsPrefab(panelGO, "Panel_Quest");
    }

    static void CreateRecruitmentPanel()
    {
        GameObject panelGO = CreateUIObject("Panel_Recruitment", null);
        var script = panelGO.AddComponent<RecruitmentPanel>();

        AssignPrivateField(script, "closeButton", CreateCloseButton(panelGO.transform));

        // Nhóm nút ngang ở giữa màn hình
        GameObject btnGroupObj = CreateUIObject("ButtonGroup", panelGO.transform);
        RectTransform btnRect = btnGroupObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f); btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.sizeDelta = new Vector2(600, 100); // Rộng đủ chứa 2 nút width 250
        var hlg = btnGroupObj.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 50; hlg.childAlignment = TextAnchor.MiddleCenter;

        AssignPrivateField(script, "recruitOneButton", CreateButton("Btn_Recruit1", btnGroupObj.transform, "Chiêu mộ x1"));
        AssignPrivateField(script, "recruitTenButton", CreateButton("Btn_Recruit10", btnGroupObj.transform, "Chiêu mộ x10"));

        SaveAsPrefab(panelGO, "Panel_Recruitment");
    }

    static void CreateProfessionSelectionPanel()
    {
        GameObject panelGO = CreateUIObject("Panel_ProfessionSelection", null);
        var script = panelGO.AddComponent<ProfessionSelectionPanel>();

        // Container cho các nút nghề
        GameObject profGroup = CreateEmptyStretch("ProfGroup", panelGO.transform);
        var vlg = profGroup.AddComponent<VerticalLayoutGroup>();
        vlg.childControlHeight = true; vlg.childForceExpandHeight = false; vlg.spacing = 20;
        vlg.padding = new RectOffset(50, 50, 200, 50);

        AssignPrivateField(script, "titleText", CreateText("TitleText", profGroup.transform, "Chọn nghề cho <Hero>"));
        AssignPrivateField(script, "warriorButton", CreateButton("Btn_Warrior", profGroup.transform, "Warrior"));
        AssignPrivateField(script, "archerButton", CreateButton("Btn_Archer", profGroup.transform, "Archer"));
        AssignPrivateField(script, "mageButton", CreateButton("Btn_Mage", profGroup.transform, "Mage"));
        AssignPrivateField(script, "closeButton", CreateCloseButton(panelGO.transform));

        SaveAsPrefab(panelGO, "Panel_ProfessionSelection");
    }

    // ================= CÁC HÀM TIỆN ÍCH DỰNG UI ================= //

    static Button CreateCloseButton(Transform parent)
    {
        Button btn = CreateButton("Btn_Close", parent, "X");
        RectTransform rect = btn.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-50, -50);
        rect.sizeDelta = new Vector2(100, 100);
        return btn;
    }

    static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        if (parent != null) go.transform.SetParent(parent, false);
        return go;
    }

    static GameObject CreateEmptyStretch(string name, Transform parent)
    {
        GameObject go = CreateUIObject(name, parent);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return go;
    }

    static TextMeshProUGUI CreateText(string name, Transform parent, string defaultText)
    {
        GameObject go = CreateUIObject(name, parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = defaultText;
        tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
    }

    static Button CreateButton(string name, Transform parent, string btnText)
    {
        GameObject go = CreateUIObject(name, parent);
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 80);
        go.AddComponent<Image>(); // Gắn Image cho nút
        Button btn = go.AddComponent<Button>();
        CreateText("Text (TMP)", go.transform, btnText).color = Color.black;
        return btn;
    }

    static void CreateScrollView(string name, Transform parent, out RectTransform contentRect)
    {
        // Khởi tạo cụm Scroll View cơ bản
        GameObject sv = CreateEmptyStretch(name, parent);
        ScrollRect scrollRect = sv.AddComponent<ScrollRect>();
        scrollRect.horizontal = false; // Xóa thanh cuộn ngang

        // Viewport
        GameObject viewport = CreateEmptyStretch("Viewport", sv.transform);
        var vpImg = viewport.AddComponent<Image>();
        vpImg.raycastTarget = false; // QUAN TRỌNG: Ngăn chặn Viewport nuốt mất thao tác bấm nút của các child bị đè
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        // Content
        GameObject content = CreateUIObject("Content", viewport.transform);
        contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = Vector2.one;
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 300);

        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.content = contentRect;
    }

    static GameObject SaveAsPrefab(GameObject go, string prefabName)
    {
        string path = $"Assets/{prefabName}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        DestroyImmediate(go);
        return prefab;
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
            UnityEngine.Debug.LogError($"Field {fieldName} not found in {target.GetType()}");
        }
    }
}

public class AutoPanelBuilderPart3 : EditorWindow
{
    [MenuItem("Tools/Tạo Panel Đợt 3 (Shop, Equip, Boss, Tower, Population)")]
    public static void BuildPanels()
    {
        // Lưu ý 2: Đảm bảo có thư mục Prefabs
        EnsurePrefabFolder();

        CreateArenaShopPanel();
        CreateEquipmentDetailPanel();
        CreateBossBattlePanel();
        CreateTowerPanel();
        CreatePopulationManagerPanel();

        // Làm mới Project window để nhận diện các thay đổi
        AssetDatabase.Refresh();
        Debug.Log("<color=green>Đã hoàn thành: Dựng 5 Panel Đợt 3 và lưu vào thư mục Assets/Prefabs thành công!</color>");
    }

    static void EnsurePrefabFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
    }

    static void CreateArenaShopPanel()
    {
        GameObject panelGO = CreateUIObject("Panel_ArenaShop", null);
        var script = panelGO.AddComponent<ArenaShopPanel>();

        AssignPrivateField(script, "closeButton", CreateCloseButtonPart3(panelGO.transform));
        
        // Scroll View chuẩn hóa
        RectTransform tempItemContainer;
        CreateVerticalScrollView("Scroll View", panelGO.transform, out tempItemContainer);
        AssignPrivateField(script, "itemContainer", tempItemContainer);

        // Prefab món hàng
        GameObject itemGO = CreateUIObject("ArenaShopItem_Prefab", null);
        itemGO.AddComponent<Image>(); // Hình ảnh
        CreateText("PriceText", itemGO.transform, "Giá tiền");
        itemGO.AddComponent<ArenaShopItem>();
        AssignPrivateField(script, "shopItemPrefab", SaveAsPrefab(itemGO, "ArenaShopItem_Prefab"));

        SaveAsPrefab(panelGO, "Panel_ArenaShop");
    }

    static void CreateEquipmentDetailPanel()
    {
        GameObject panelGO = CreateUIObject("Panel_EquipmentDetail", null);
        var script = panelGO.AddComponent<EquipmentDetailPanel>();

        // Khung thông tin
        GameObject infoFrame = CreateUIObject("InfoFrame", panelGO.transform);
        AssignPrivateField(script, "equipNameText", CreateText("EquipNameText", infoFrame.transform, "Tên Trang Bị"));
        AssignPrivateField(script, "equipLevelText", CreateText("EquipLevelText", infoFrame.transform, "Level: 1"));
        AssignPrivateField(script, "expProgressText", CreateText("ExpProgressText", infoFrame.transform, "EXP: 0/100"));
        AssignPrivateField(script, "statsText", CreateText("StatsText", infoFrame.transform, "ATK: +10"));

        // Các nút bấm
        GameObject btnGroup = CreateUIObject("ButtonGroup", panelGO.transform);
        RectTransform btnRect = btnGroup.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0, 0); btnRect.anchorMax = new Vector2(1, 0.15f);
        btnRect.offsetMin = Vector2.zero; btnRect.offsetMax = Vector2.zero;
        var hlg = btnGroup.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth = true; hlg.spacing = 30; hlg.padding = new RectOffset(50, 50, 50, 50);

        AssignPrivateField(script, "closeButton", CreateButton("Btn_Close", btnGroup.transform, "Đóng"));
        AssignPrivateField(script, "equipButton", CreateButton("Btn_Equip", btnGroup.transform, "Mặc vào"));
        AssignPrivateField(script, "quickUpgradeButton", CreateButton("Btn_QuickUpgrade", btnGroup.transform, "Nâng cấp nhanh"));

        SaveAsPrefab(panelGO, "Panel_EquipmentDetail");
    }

    static void CreateBossBattlePanel()
    {
        GameObject panelGO = CreateUIObject("Panel_BossBattle", null);
        var script = panelGO.AddComponent<BossBattlePanel>();

        // Cột TRÁI (Thông tin Boss)
        GameObject leftCol = CreateUIObject("LeftColumn_BossInfo", panelGO.transform);
        RectTransform leftRect = leftCol.GetComponent<RectTransform>();
        leftRect.anchorMin = new Vector2(0, 0.2f); leftRect.anchorMax = new Vector2(0.5f, 0.8f);
        leftRect.offsetMin = Vector2.zero; leftRect.offsetMax = Vector2.zero;
        leftCol.AddComponent<VerticalLayoutGroup>().childControlHeight = true;
        GameObject bossImgGO = CreateUIObject("BossImage", leftCol.transform);
        AssignPrivateField(script, "bossImage", bossImgGO.AddComponent<Image>());
        AssignPrivateField(script, "bossNameText", CreateText("BossNameText", leftCol.transform, "Tên Boss"));
        AssignPrivateField(script, "bossLevelText", CreateText("BossLevelText", leftCol.transform, "Lv. 99"));
        AssignPrivateField(script, "hpText", CreateText("HpText", leftCol.transform, "HP: 10000"));
        AssignPrivateField(script, "atkText", CreateText("AtkText", leftCol.transform, "ATK: 500"));
        AssignPrivateField(script, "defText", CreateText("DefText", leftCol.transform, "DEF: 200"));
        AssignPrivateField(script, "spdText", CreateText("SpdText", leftCol.transform, "SPD: 120"));

        // Cột PHẢI (Cơ chế & Kỹ năng)
        GameObject rightCol = CreateUIObject("RightColumn_Mechanics", panelGO.transform);
        RectTransform rightRect = rightCol.GetComponent<RectTransform>();
        rightRect.anchorMin = new Vector2(0.5f, 0.2f); rightRect.anchorMax = new Vector2(1, 0.8f);
        rightRect.offsetMin = Vector2.zero; rightRect.offsetMax = Vector2.zero;
        rightCol.AddComponent<VerticalLayoutGroup>().childControlHeight = true;
        AssignPrivateField(script, "normalSkillOutlineText", CreateText("NormalSkillOutlineText", rightCol.transform, "Đánh thường: ..."));
        AssignPrivateField(script, "aoeSkillOutlineText", CreateText("AoeSkillOutlineText", rightCol.transform, "Đánh AOE: ..."));
        AssignPrivateField(script, "mechanicNameText", CreateText("MechanicNameText", rightCol.transform, "Cơ chế: Bạo Kích"));
        AssignPrivateField(script, "mechanicDescText", CreateText("MechanicDescText", rightCol.transform, "Mô tả cơ chế..."));

        // Dưới cùng (Buttons)
        Button chalBtn = CreateButton("Btn_Challenge", panelGO.transform, "Khiêu Chiến");
        RectTransform chalRect = chalBtn.GetComponent<RectTransform>();
        chalRect.anchorMin = new Vector2(0.5f, 0); chalRect.anchorMax = new Vector2(0.5f, 0);
        chalRect.anchoredPosition = new Vector2(0, 100);
        AssignPrivateField(script, "challengeButton", chalBtn);
        
        AssignPrivateField(script, "closeButton", CreateCloseButtonPart3(panelGO.transform));

        SaveAsPrefab(panelGO, "Panel_BossBattle");
    }

    static void CreateTowerPanel()
    {
        GameObject panelGO = CreateUIObject("Panel_Tower", null);
        var script = panelGO.AddComponent<TowerPanel>();

        // Scroll View dọc
        RectTransform tempContentParent;
        CreateVerticalScrollView("Scroll View", panelGO.transform, out tempContentParent);
        AssignPrivateField(script, "contentParent", tempContentParent);

        // Prefab đại diện tầng tháp
        GameObject floorGO = CreateUIObject("FloorItem_Prefab", null);
        floorGO.AddComponent<Image>();
        CreateText("FloorNameText", floorGO.transform, "Tầng 1");
        AssignPrivateField(script, "floorItemPrefab", SaveAsPrefab(floorGO, "FloorItem_Prefab"));

        // Phần Footer
        GameObject footerGO = CreateUIObject("Footer", panelGO.transform);
        RectTransform footerRect = footerGO.GetComponent<RectTransform>();
        footerRect.anchorMin = new Vector2(0,0); footerRect.anchorMax = new Vector2(1, 0.2f);
        footerRect.offsetMin = Vector2.zero; footerRect.offsetMax = Vector2.zero;
        var fhGroup = footerGO.AddComponent<HorizontalLayoutGroup>();
        fhGroup.childControlWidth = true; fhGroup.spacing = 20; fhGroup.childAlignment = TextAnchor.MiddleCenter;

        AssignPrivateField(script, "currentFloorDetailText", CreateText("CurrentFloorDetailText", footerGO.transform, "Tầng hiện tại: 1"));
        AssignPrivateField(script, "difficultyText", CreateText("DifficultyText", footerGO.transform, "Độ khó: Thường"));
        AssignPrivateField(script, "monsterCountText", CreateText("MonsterCountText", footerGO.transform, "Quái: 10"));

        // Buttons
        AssignPrivateField(script, "enterButton", CreateButton("Btn_Enter", footerGO.transform, "Đi vào tháp"));
        AssignPrivateField(script, "closeButton", CreateButton("Btn_Close", footerGO.transform, "Đóng"));

        SaveAsPrefab(panelGO, "Panel_Tower");
    }

    static void CreatePopulationManagerPanel()
    {
        GameObject panelGO = CreateUIObject("Panel_PopulationManager", null);
        var script = panelGO.AddComponent<PopulationManagerPanel>();

        AssignPrivateField(script, "closeButton", CreateButton("Btn_Close", panelGO.transform, "Thoát"));
        AssignPrivateField(script, "populationCountText", CreateText("PopulationCountText", panelGO.transform, "Dân số: 34/50"));

        // Scroll View dọc
        RectTransform tempListContainer;
        CreateVerticalScrollView("Scroll View", panelGO.transform, out tempListContainer);
        AssignPrivateField(script, "listContainer", tempListContainer);

        // Prefab Thẻ Dân số
        GameObject cardGO = CreateUIObject("PopulationHeroCard_Prefab", null);
        cardGO.AddComponent<Image>(); // Ảnh hero
        CreateText("HeroNameText", cardGO.transform, "Tên Dân");
        CreateText("HeroLevelText", cardGO.transform, "Lv.1");
        CreateButton("Btn_Dismiss", cardGO.transform, "Sa Thải");
        cardGO.AddComponent<PopulationHeroCard>();
        
        AssignPrivateField(script, "heroCardPrefab", SaveAsPrefab(cardGO, "PopulationHeroCard_Prefab"));

        SaveAsPrefab(panelGO, "Panel_PopulationManager");
    }


    // ================= CÁC HÀM TIỆN ÍCH DỰNG UI ================= //

    static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        if (parent != null) go.transform.SetParent(parent, false);
        
        RectTransform rt = go.GetComponent<RectTransform>();
        if (name.StartsWith("Panel_")) {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            go.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Nền tối cho các Panel chính
        }
        return go;
    }

    static TextMeshProUGUI CreateText(string name, Transform parent, string defaultText)
    {
        GameObject go = CreateUIObject(name, parent);
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 100);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = defaultText;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white; // Chữ trắng trên nền tối
        return tmp;
    }

    static Button CreateButton(string name, Transform parent, string btnText)
    {
        GameObject go = CreateUIObject(name, parent);
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 100); // Nút to ra
        go.AddComponent<Image>().color = Color.gray; 
        Button btn = go.AddComponent<Button>();
        CreateText("Text (TMP)", go.transform, btnText);
        return btn;
    }

    // LƯU Ý 1 SỐNG CÒN: Hàm cấu hình ScrollView dọc chuẩn

    static Button CreateCloseButtonPart3(Transform parent)
    {
        Button btn = CreateButton("Btn_Close", parent, "X");
        RectTransform rect = btn.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-50, -50);
        rect.sizeDelta = new Vector2(100, 100);
        return btn;
    }
    static void CreateVerticalScrollView(string name, Transform parent, out RectTransform contentRect)
    {
        // 1. Tạo Scroll View gốc
        GameObject sv = CreateUIObject(name, parent);
        RectTransform svRect = sv.GetComponent<RectTransform>();
        svRect.anchorMin = Vector2.zero;
        svRect.anchorMax = Vector2.one;
        svRect.offsetMax = new Vector2(0, -150); // Cách top 150 để chừa view cho Tab/CloseBtn
        svRect.offsetMin = new Vector2(0, 50); // Cách đáy 50
        svRect.sizeDelta = Vector2.zero;
        ScrollRect scrollRect = sv.AddComponent<ScrollRect>();
        scrollRect.horizontal = false; // Tắt thanh cuộn ngang

        // 2. Tạo Viewport
        GameObject viewport = CreateUIObject("Viewport", sv.transform);
        RectTransform vpRect = viewport.GetComponent<RectTransform>();
        vpRect.anchorMin = Vector2.zero;
        vpRect.anchorMax = Vector2.one;
        vpRect.sizeDelta = Vector2.zero;
        viewport.AddComponent<Image>();
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        // 3. Tạo Content
        GameObject content = CreateUIObject("Content", viewport.transform);
        contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0); // Sẽ được tự động giãn bởi Fitter

        // ÁP DỤNG LƯU Ý SỐNG CÒN 1: Thêm Layout Group và Size Fitter
        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.childControlHeight = false; // Để item con tự giữ chiều cao
        vlg.childControlWidth = true;   // Dãn item theo chiều rộng khung
        vlg.childForceExpandHeight = false;
        vlg.spacing = 30; // Khoảng cách giữa các item
        vlg.padding = new RectOffset(20, 20, 20, 20);

        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Gắn Viewport & Content vào ScrollRect
        scrollRect.viewport = vpRect;
        scrollRect.content = contentRect;
    }

    // LƯU Ý 2 SỐNG CÒN: Lưu trực tiếp vào thư mục Assets/Prefabs
    static GameObject SaveAsPrefab(GameObject go, string prefabName)
    {
        string path = $"Assets/Prefabs/{prefabName}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        DestroyImmediate(go);
        return prefab;
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
            UnityEngine.Debug.LogError($"Field {fieldName} not found in {target.GetType()}");
        }
    }
}
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HospitalPanelGenerator : EditorWindow
{
    [MenuItem("UI Tools/Generate Hospital Panel")]
    public static void GenerateUI()
    {
        // 0. Auto-Cleanup Old UI
        var oldPanels = Object.FindObjectsByType<LegendOfBlood.HospitalPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in oldPanels) {
            Undo.DestroyObjectImmediate(p.gameObject);
        }

        GameObject canvasObj = new GameObject("Hospital_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // ScreenRoot
        GameObject screenRoot = CreateUIElement("ScreenRoot", canvasObj.transform);
        SetAnchor(screenRoot, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(screenRoot, 0, 0, 0, 280); // Chừa phần dưới 280px cho BottomNav chung
        if (System.Type.GetType("SafeArea") != null) {
            screenRoot.AddComponent(System.Type.GetType("SafeArea"));
        }

        // OuterFrame & BackgroundScene (Nằm ngoài SafeArea để full màn hình)
        GameObject outerFrame = CreateUIElement("OuterFrame", canvasObj.transform);
        outerFrame.transform.SetAsFirstSibling();
        SetAnchor(outerFrame, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(outerFrame, 0, 0, 0, 0);
        outerFrame.AddComponent<Image>().color = new Color(0.15f, 0.1f, 0.1f, 1f);

        GameObject backgroundScene = CreateUIElement("BackgroundScene", canvasObj.transform);
        backgroundScene.transform.SetSiblingIndex(1); // Ngay trên outerFrame
        SetAnchor(backgroundScene, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(backgroundScene, 10, 10, 10, 10);
        backgroundScene.AddComponent<Image>().color = new Color(0.12f, 0.14f, 0.16f, 1f);

        // TopBanner
        GameObject topBanner = CreateUIElement("TopBanner", screenRoot.transform);
        SetAnchor(topBanner, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        SetOffsets(topBanner, 29, 29, 70, -328); 
        topBanner.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        GameObject titleTextObj = AddTMPText(topBanner, "Healing Ward", 45);
        titleTextObj.name = "TitleText";
        SetAnchor(titleTextObj, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1)); 
        titleTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(79, -35); 
        titleTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 86);
        titleTextObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        // UpgradeButton
        GameObject upgradeBtn = CreateUIElement("UpgradeButton", topBanner.transform);
        SetAnchor(upgradeBtn, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));
        upgradeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-82, -7);
        upgradeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(262, 149);
        upgradeBtn.AddComponent<Image>().color = new Color(0.1f, 0.6f, 0.2f, 1f);
        Button upgradeBtnComp = upgradeBtn.AddComponent<Button>();
        AddTMPText(upgradeBtn, "UPGRADE", 24);

        // Custom Close Button 
        GameObject closeBtn = CreateUIElement("CloseButton", topBanner.transform);
        SetAnchor(closeBtn, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1)); 
        closeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-380, -35); 
        closeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);
        closeBtn.AddComponent<Image>().color = new Color(0.8f, 0.1f, 0.1f, 1f);
        Button closeBtnComp = closeBtn.AddComponent<Button>();
        AddTMPText(closeBtn, "X", 40);

        // SevereWardPanel
        GameObject severeWardPanel = CreateUIElement("SevereWardPanel", screenRoot.transform);
        SetAnchor(severeWardPanel, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        SetOffsets(severeWardPanel, 95, 101, 350, -889); 
        severeWardPanel.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 1f);

        GameObject severeTitle = CreateUIElement("SevereTitle", severeWardPanel.transform);
        SetAnchor(severeTitle, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        severeTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(1, -12);
        severeTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(482, 69);
        severeTitle.AddComponent<Image>().color = new Color(0.3f, 0.1f, 0.1f, 1f);
        GameObject severeTextObj = AddTMPText(severeTitle, "Severe Injury", 30);

        GameObject severeCardRow = CreateUIElement("SevereCardRow", severeWardPanel.transform);
        SetAnchor(severeCardRow, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        SetOffsets(severeCardRow, 7, 62, 166, -465); 
        
        HorizontalLayoutGroup hlgSevere = severeCardRow.AddComponent<HorizontalLayoutGroup>();
        hlgSevere.childAlignment = TextAnchor.UpperLeft;
        hlgSevere.spacing = 82;
        hlgSevere.padding = new RectOffset(0, 0, 0, 0); 
        hlgSevere.childControlWidth = true; hlgSevere.childControlHeight = true;
        hlgSevere.childForceExpandWidth = false; hlgSevere.childForceExpandHeight = false;

        string[] severeNames = {"Card_SirArthur", "Card_Clara", "Card_Gareth", "Card_Isolde"};
        for(int i=0; i<severeNames.Length; i++) {
            CreateCardPrefab(severeNames[i], severeCardRow.transform, 286);
        }

        GameObject severeArrowRight = CreateUIElement("ArrowRight", severeWardPanel.transform);
        SetAnchor(severeArrowRight, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
        severeArrowRight.GetComponent<RectTransform>().anchoredPosition = new Vector2(-22, 2); 
        severeArrowRight.GetComponent<RectTransform>().sizeDelta = new Vector2(44, 110);
        severeArrowRight.AddComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f, 1f);

        // DividerBeam
        GameObject dividerBeam = CreateUIElement("DividerBeam", screenRoot.transform);
        SetAnchor(dividerBeam, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        SetOffsets(dividerBeam, 0, 0, 925, -1001); 
        dividerBeam.AddComponent<Image>().color = new Color(0.25f, 0.2f, 0.1f, 1f);
        
        // LightWardPanel
        GameObject lightWardPanel = CreateUIElement("LightWardPanel", screenRoot.transform);
        SetAnchor(lightWardPanel, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        SetOffsets(lightWardPanel, 94, 101, 1007, -1592); 
        lightWardPanel.AddComponent<Image>().color = new Color(0.18f, 0.18f, 0.18f, 1f);

        GameObject lightTitle = CreateUIElement("LightTitle", lightWardPanel.transform);
        SetAnchor(lightTitle, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        lightTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(3, -24);
        lightTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(449, 66);
        lightTitle.AddComponent<Image>().color = new Color(0.1f, 0.3f, 0.1f, 1f);
        GameObject lightTextObj = AddTMPText(lightTitle, "Light Injury", 30);

        GameObject lightCardRow = CreateUIElement("LightCardRow", lightWardPanel.transform);
        SetAnchor(lightCardRow, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        SetOffsets(lightCardRow, 8, 62, 176, -472); 

        HorizontalLayoutGroup hlgLight = lightCardRow.AddComponent<HorizontalLayoutGroup>();
        hlgLight.childAlignment = TextAnchor.UpperLeft;
        hlgLight.spacing = 82;
        hlgLight.padding = new RectOffset(0, 0, 0, 0); 
        hlgLight.childControlWidth = true; hlgLight.childControlHeight = true;
        hlgLight.childForceExpandWidth = false; hlgLight.childForceExpandHeight = false;

        string[] lightNames = {"Card_Rurik", "Card_Anya", "Card_Finn", "Card_Bryn"};
        for(int i=0; i<lightNames.Length; i++) {
            CreateCardPrefab(lightNames[i], lightCardRow.transform, 283);
        }

        canvasObj.name = "Panel_Hospital";
        LegendOfBlood.HospitalPanel hospitalScript = canvasObj.AddComponent<LegendOfBlood.HospitalPanel>();
        hospitalScript.PanelType = LegendOfBlood.UIPanelType.Hospital;

        SerializedObject so = new SerializedObject(hospitalScript);
        so.Update();

        so.FindProperty("panelTitleText").objectReferenceValue = titleTextObj.GetComponent<TextMeshProUGUI>();
        so.FindProperty("severeInjuryLabelText").objectReferenceValue = severeTextObj.GetComponent<TextMeshProUGUI>();
        so.FindProperty("lightInjuryLabelText").objectReferenceValue = lightTextObj.GetComponent<TextMeshProUGUI>();
        so.FindProperty("severeInjuryListContainer").objectReferenceValue = severeCardRow.transform;
        so.FindProperty("lightInjuryListContainer").objectReferenceValue = lightCardRow.transform;
        so.FindProperty("upgradeBuildingButton").objectReferenceValue = upgradeBtnComp;
        so.FindProperty("associatedBuildingId").stringValue = "Hospital";
        so.FindProperty("closeButton").objectReferenceValue = closeBtnComp;

        GameObject cardPrefab = null;
        string[] guids = AssetDatabase.FindAssets("t:Prefab InjuredHeroCard");
        if (guids.Length == 0) guids = AssetDatabase.FindAssets("t:Prefab HeroWardCard");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
        if (cardPrefab != null) {
            so.FindProperty("injuredHeroCardPrefab").objectReferenceValue = cardPrefab;
        }

        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(canvasObj, "Generate Hospital Panel UI");
        Selection.activeGameObject = canvasObj;
        Debug.Log("Hospital Panel UI Generated Successfully!");
    }

    private static void CreateCardPrefab(string name, Transform parent, float height) {
        GameObject card = CreateUIElement(name, parent);
        LayoutElement le = card.AddComponent<LayoutElement>();
        le.preferredWidth = 157;
        le.preferredHeight = height;
        
        GameObject portrait = CreateUIElement("Portrait", card.transform);
        SetAnchor(portrait, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        portrait.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -8);
        portrait.GetComponent<RectTransform>().sizeDelta = new Vector2(131, 171);
        portrait.AddComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1f);

        GameObject frame = CreateUIElement("CardFrame", card.transform);
        SetAnchor(frame, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(frame, 0, 0, 0, 0);
        Image frameImg = frame.AddComponent<Image>();
        Sprite frameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Hospital/HospitalCardFrame.png");
        if (frameSprite != null) {
            frameImg.sprite = frameSprite;
            frameImg.color = Color.white;
            frameImg.preserveAspect = true;
        } else {
            frameImg.color = new Color(0.2f, 0.15f, 0.1f, 1f);
        }

        GameObject plusIcon = CreateUIElement("PlusIconTL", card.transform);
        SetAnchor(plusIcon, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
        plusIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0); 
        plusIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(35, 35);
        plusIcon.AddComponent<Image>().color = new Color(0.8f, 0.8f, 0.2f, 1f);

        GameObject namePlate = CreateUIElement("NamePlate", card.transform);
        SetAnchor(namePlate, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        namePlate.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 69);
        namePlate.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 34);
        namePlate.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 1f);
        AddTMPText(namePlate, name.Replace("Card_", ""), 16);

        GameObject hpArea = CreateUIElement("HPBarArea", card.transform);
        SetAnchor(hpArea, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        hpArea.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 43); // Slightly bumped up
        hpArea.GetComponent<RectTransform>().sizeDelta = new Vector2(130, 24); // Taller for the graphical bar

        GameObject hpBarBg = CreateUIElement("HPBarBG", hpArea.transform);
        SetAnchor(hpBarBg, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(hpBarBg, 0, 0, 0, 0);
        Image bgImg = hpBarBg.AddComponent<Image>();
        Sprite hpFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Hospital/HPFrame.png");
        if (hpFrameSprite != null) {
            bgImg.sprite = hpFrameSprite;
            bgImg.preserveAspect = true;
        } else {
            bgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        }

        GameObject hpBarFill = CreateUIElement("HPBarFill", hpArea.transform);
        SetAnchor(hpBarFill, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(hpBarFill, 0, 0, 0, 0); // Must exactly overlap BG
        Image fillImg = hpBarFill.AddComponent<Image>();
        Sprite hpFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Hospital/HPFill.png");
        if (hpFillSprite != null) {
            fillImg.sprite = hpFillSprite;
            fillImg.preserveAspect = true;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = 0; // Left
            fillImg.fillAmount = 0.5f; // Initialize to 50% for mockup
        } else {
            fillImg.color = new Color(0.8f, 0.2f, 0.2f, 1f);
            SetOffsets(hpBarFill, 2, 2, -2, -2);
        }

        GameObject healBtn = CreateUIElement("PriceButton", card.transform);
        SetAnchor(healBtn, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
        healBtn.GetComponent<RectTransform>().offsetMin = new Vector2(8, 6);
        healBtn.GetComponent<RectTransform>().offsetMax = new Vector2(-8, 40); 
        healBtn.AddComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f, 1f);
        AddTMPText(healBtn, "HEAL <color=#FFD700>50G</color>", 16);
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


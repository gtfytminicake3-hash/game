using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class HeroInfoLayoutGenerator
{
    private static Color bgColor = new Color(0.08f, 0.10f, 0.12f, 1f);
    private static Color borderColor = new Color(0.4f, 0.45f, 0.5f, 1f);
    private static Color slotBgColor = new Color(0.12f, 0.15f, 0.18f, 1f);
    private static Color textColor = new Color(0.85f, 0.85f, 0.85f, 1f);

    [MenuItem("Tools/Generate Hero Info Layout Perfected")]
    public static void GenerateLayoutPerfected()
    {
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        GameObject root = new GameObject("HeroInfo_Panel_Layout", typeof(RectTransform), typeof(Image));
        root.transform.SetParent(canvas.transform, false);
        RectTransform rootRT = root.GetComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.sizeDelta = Vector2.zero;
        root.GetComponent<Image>().color = bgColor;

        VerticalLayoutGroup mainVLG = root.AddComponent<VerticalLayoutGroup>();
        mainVLG.padding = new RectOffset(40, 40, 40, 40);
        mainVLG.spacing = 15;
        mainVLG.childControlHeight = false;
        mainVLG.childControlWidth = true;
        mainVLG.childForceExpandHeight = false;
        mainVLG.childForceExpandWidth = true;

        // 0. Top Ornament
        CreateTopOrnament(root.transform);

        // 1. Hero Name
        CreateText(root.transform, "Hero Name", 70, TextAlignmentOptions.Center, FontStyles.Normal, 80);

        // 2. Subheader
        GameObject subheader = CreateHorizontalGroup(root.transform, "Subheader", 0);
        subheader.AddComponent<LayoutElement>().preferredHeight = 40;
        CreateText(subheader.transform, "Level", 32, TextAlignmentOptions.Center);
        CreateText(subheader.transform, "Profession (Warrior)", 32, TextAlignmentOptions.Center);
        CreateText(subheader.transform, "Gender", 32, TextAlignmentOptions.Center);

        // 3. Equipment & Portrait area
        GameObject displayGroup = CreateHorizontalGroup(root.transform, "EquipmentAndPortrait", 20);
        displayGroup.AddComponent<LayoutElement>().preferredHeight = 450;

        GameObject leftEquip = CreateVerticalGroup(displayGroup.transform, "LeftEquip", 20);
        leftEquip.AddComponent<LayoutElement>().preferredWidth = 120;
        leftEquip.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
        CreateEquipmentSlot(leftEquip.transform, "Helm");
        CreateEquipmentSlot(leftEquip.transform, "Armor");
        CreateEquipmentSlot(leftEquip.transform, "Boots");

        GameObject portrait = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
        portrait.transform.SetParent(displayGroup.transform, false);
        portrait.GetComponent<Image>().color = new Color(0.2f, 0.25f, 0.3f, 1f);
        LayoutElement portraitLE = portrait.AddComponent<LayoutElement>();
        portraitLE.preferredWidth = 450;
        portraitLE.preferredHeight = 450;
        CreateText(portrait.transform, "[Portrait Image]", 36, TextAlignmentOptions.Center).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        GameObject rightEquip = CreateVerticalGroup(displayGroup.transform, "RightEquip", 20);
        rightEquip.AddComponent<LayoutElement>().preferredWidth = 120;
        rightEquip.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
        CreateEquipmentSlot(rightEquip.transform, "Ring 1");
        CreateEquipmentSlot(rightEquip.transform, "Weapon");
        CreateEquipmentSlot(rightEquip.transform, "Ring 2");

        // 4. Traits
        CreateDivider(root.transform, "Traits");
        GameObject traitsGroup = CreateHorizontalGroup(root.transform, "TraitsGroup", 15);
        traitsGroup.AddComponent<LayoutElement>().preferredHeight = 100;
        traitsGroup.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
        traitsGroup.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = false;
        for (int i = 0; i < 5; i++) CreateAbilitySlot(traitsGroup.transform, "Trait", 100);

        // 5. Skills
        CreateDivider(root.transform, "Skills");
        GameObject skillsGroup = CreateHorizontalGroup(root.transform, "SkillsGroup", 15);
        skillsGroup.AddComponent<LayoutElement>().preferredHeight = 90;
        skillsGroup.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
        skillsGroup.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = false;
        for (int i = 0; i < 6; i++) CreateAbilitySlot(skillsGroup.transform, "Skill", 90);

        // 6. Stats Box (Contains both Base & Advanced Stats)
        GameObject statsBox = new GameObject("StatsBox", typeof(RectTransform), typeof(Image));
        statsBox.transform.SetParent(root.transform, false);
        statsBox.GetComponent<Image>().color = new Color(0.11f, 0.13f, 0.16f, 1f);
        Outline statsBoxOutline = statsBox.AddComponent<Outline>();
        statsBoxOutline.effectColor = borderColor;
        statsBoxOutline.effectDistance = new Vector2(2, -2);
        
        VerticalLayoutGroup statsVLG = statsBox.AddComponent<VerticalLayoutGroup>();
        statsVLG.padding = new RectOffset(20, 20, 20, 20);
        statsVLG.spacing = 15;
        statsVLG.childControlHeight = true;
        statsVLG.childControlWidth = true;
        statsVLG.childForceExpandHeight = false;
        statsVLG.childForceExpandWidth = true;

        CreateDivider(statsBox.transform, "Base Stats", 28);
        
        GameObject baseStatsGrid = new GameObject("BaseStatsGrid", typeof(RectTransform));
        baseStatsGrid.transform.SetParent(statsBox.transform, false);
        GridLayoutGroup baseGrid = baseStatsGrid.AddComponent<GridLayoutGroup>();
        baseGrid.cellSize = new Vector2(300, 50);
        baseGrid.spacing = new Vector2(15, 15);
        baseGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        baseGrid.constraintCount = 3;
        LayoutElement baseGridLE = baseStatsGrid.AddComponent<LayoutElement>();
        baseGridLE.preferredHeight = 50 * 2 + 15; // 2 rows

        CreateStatEntry(baseStatsGrid.transform, "HP");
        CreateStatEntry(baseStatsGrid.transform, "ATK");
        CreateStatEntry(baseStatsGrid.transform, "DEF");
        CreateStatEntry(baseStatsGrid.transform, "UNIQUE ICON");
        CreateStatEntry(baseStatsGrid.transform, "SPD");
        CreateStatEntry(baseStatsGrid.transform, "Potential");

        CreateDivider(statsBox.transform, "Advanced Stats", 28);

        GameObject advStatsGrid = new GameObject("AdvStatsGrid", typeof(RectTransform));
        advStatsGrid.transform.SetParent(statsBox.transform, false);
        GridLayoutGroup advGrid = advStatsGrid.AddComponent<GridLayoutGroup>();
        advGrid.cellSize = new Vector2(300, 50);
        advGrid.spacing = new Vector2(15, 15);
        advGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        advGrid.constraintCount = 3;
        LayoutElement advGridLE = advStatsGrid.AddComponent<LayoutElement>();
        advGridLE.preferredHeight = 50; // 1 row

        CreateStatEntry(advStatsGrid.transform, "Evasion Rate");
        CreateStatEntry(advStatsGrid.transform, "Dmg Reduction");
        CreateStatEntry(advStatsGrid.transform, "Dmg Increase");

        // Spacer to push buttons to bottom
        GameObject spacer = new GameObject("Spacer", typeof(RectTransform));
        spacer.transform.SetParent(root.transform, false);
        spacer.AddComponent<LayoutElement>().flexibleHeight = 1;

        // 7. Bottom Buttons
        GameObject buttonGroup = CreateHorizontalGroup(root.transform, "ButtonGroup", 20);
        buttonGroup.AddComponent<LayoutElement>().preferredHeight = 80;
        
        CreateBottomButton(buttonGroup.transform, "+", "Stat\nAllocation", new Color(0.1f, 0.3f, 0.5f, 1f));
        CreateBottomButton(buttonGroup.transform, "↑", "Trait Upgrade", new Color(0.1f, 0.3f, 0.5f, 1f));
        CreateBottomButton(buttonGroup.transform, "P", "Use EXP Item", new Color(0.5f, 0.4f, 0.1f, 1f)); // P for potion

        Selection.activeGameObject = root;
        EditorGUIUtility.PingObject(root);
    }

    private static TextMeshProUGUI CreateText(Transform parent, string textStr, float fontSize, TextAlignmentOptions align, FontStyles fontStyle = FontStyles.Normal, float preferredHeight = -1)
    {
        GameObject textObj = new GameObject("Text_" + textStr.Replace("\n", ""), typeof(RectTransform));
        textObj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = textStr;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.fontStyle = fontStyle;
        tmp.color = textColor;
        tmp.enableWordWrapping = false;

        if (preferredHeight > 0)
        {
            LayoutElement le = textObj.AddComponent<LayoutElement>();
            le.preferredHeight = preferredHeight;
        }

        return tmp;
    }

    private static GameObject CreateHorizontalGroup(Transform parent, string name, float spacing)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        HorizontalLayoutGroup hlg = go.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = spacing;
        hlg.childControlHeight = true;
        hlg.childControlWidth = true;
        hlg.childForceExpandHeight = false;
        hlg.childForceExpandWidth = true;
        return go;
    }

    private static GameObject CreateVerticalGroup(Transform parent, string name, float spacing)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        VerticalLayoutGroup vlg = go.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = spacing;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        return go;
    }

    private static void CreateTopOrnament(Transform parent)
    {
        GameObject go = CreateHorizontalGroup(parent, "TopOrnament", 10);
        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 20;

        GameObject line1 = new GameObject("Line", typeof(RectTransform), typeof(Image));
        line1.transform.SetParent(go.transform, false);
        line1.GetComponent<Image>().color = borderColor;
        LayoutElement line1LE = line1.AddComponent<LayoutElement>();
        line1LE.preferredHeight = 2;
        line1LE.flexibleWidth = 1;

        GameObject diamond = new GameObject("Diamond", typeof(RectTransform), typeof(Image));
        diamond.transform.SetParent(go.transform, false);
        diamond.GetComponent<Image>().color = borderColor;
        diamond.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 45);
        LayoutElement diamondLE = diamond.AddComponent<LayoutElement>();
        diamondLE.preferredHeight = 10;
        diamondLE.preferredWidth = 10;

        GameObject line2 = new GameObject("Line", typeof(RectTransform), typeof(Image));
        line2.transform.SetParent(go.transform, false);
        line2.GetComponent<Image>().color = borderColor;
        LayoutElement line2LE = line2.AddComponent<LayoutElement>();
        line2LE.preferredHeight = 2;
        line2LE.flexibleWidth = 1;
    }

    private static void CreateDivider(Transform parent, string title, float fontSize = 36)
    {
        GameObject go = CreateHorizontalGroup(parent, "Divider_" + title, 15);
        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 50;

        // Line Left
        GameObject line1 = new GameObject("LineLeft", typeof(RectTransform), typeof(Image));
        line1.transform.SetParent(go.transform, false);
        line1.GetComponent<Image>().color = borderColor;
        LayoutElement line1LE = line1.AddComponent<LayoutElement>();
        line1LE.preferredHeight = 2;
        line1LE.flexibleWidth = 1;

        // Diamond Left
        GameObject diamond1 = new GameObject("Diamond", typeof(RectTransform), typeof(Image));
        diamond1.transform.SetParent(go.transform, false);
        diamond1.GetComponent<Image>().color = borderColor;
        diamond1.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 45);
        LayoutElement d1LE = diamond1.AddComponent<LayoutElement>();
        d1LE.preferredHeight = 8;
        d1LE.preferredWidth = 8;

        // Title
        TextMeshProUGUI titleText = CreateText(go.transform, title, fontSize, TextAlignmentOptions.Center);
        LayoutElement titleLE = titleText.gameObject.GetComponent<LayoutElement>();
        if (titleLE == null) titleLE = titleText.gameObject.AddComponent<LayoutElement>();
        titleLE.flexibleWidth = 0;
        ContentSizeFitter csf = titleText.gameObject.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Diamond Right
        GameObject diamond2 = new GameObject("Diamond", typeof(RectTransform), typeof(Image));
        diamond2.transform.SetParent(go.transform, false);
        diamond2.GetComponent<Image>().color = borderColor;
        diamond2.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 45);
        LayoutElement d2LE = diamond2.AddComponent<LayoutElement>();
        d2LE.preferredHeight = 8;
        d2LE.preferredWidth = 8;

        // Line Right
        GameObject line2 = new GameObject("LineRight", typeof(RectTransform), typeof(Image));
        line2.transform.SetParent(go.transform, false);
        line2.GetComponent<Image>().color = borderColor;
        LayoutElement line2LE = line2.AddComponent<LayoutElement>();
        line2LE.preferredHeight = 2;
        line2LE.flexibleWidth = 1;

        go.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
    }

    private static void CreateEquipmentSlot(Transform parent, string slotName)
    {
        GameObject bg = new GameObject("EquipSlot_" + slotName, typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(parent, false);
        bg.GetComponent<Image>().color = slotBgColor;
        Outline outline = bg.AddComponent<Outline>();
        outline.effectColor = borderColor;
        outline.effectDistance = new Vector2(1, -1);
        
        LayoutElement le = bg.AddComponent<LayoutElement>();
        le.preferredHeight = 120;
        le.preferredWidth = 120;
        le.flexibleWidth = 0;
        le.flexibleHeight = 0;

        GameObject innerFrame = new GameObject("InnerFrame", typeof(RectTransform), typeof(Image));
        innerFrame.transform.SetParent(bg.transform, false);
        RectTransform innerRT = innerFrame.GetComponent<RectTransform>();
        innerRT.anchorMin = new Vector2(0.05f, 0.05f);
        innerRT.anchorMax = new Vector2(0.95f, 0.95f);
        innerRT.offsetMin = Vector2.zero;
        innerRT.offsetMax = Vector2.zero;
        innerFrame.GetComponent<Image>().color = new Color(0,0,0,0);
        Outline innerOutline = innerFrame.AddComponent<Outline>();
        innerOutline.effectColor = new Color(0.3f, 0.3f, 0.3f, 1f);

        GameObject icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        icon.transform.SetParent(innerFrame.transform, false);
        RectTransform iconRT = icon.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.2f, 0.2f);
        iconRT.anchorMax = new Vector2(0.8f, 0.8f);
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;
        icon.GetComponent<Image>().color = new Color(0.6f, 0.6f, 0.6f, 0.5f); // placeholder
        CreateText(icon.transform, slotName, 20, TextAlignmentOptions.Center).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    private static void CreateAbilitySlot(Transform parent, string namePrefix, float size)
    {
        GameObject bg = new GameObject(namePrefix + "_Slot", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(parent, false);
        bg.GetComponent<Image>().color = slotBgColor;
        Outline outline = bg.AddComponent<Outline>();
        outline.effectColor = borderColor;
        
        LayoutElement le = bg.AddComponent<LayoutElement>();
        le.preferredWidth = size;
        le.preferredHeight = size;
        le.flexibleWidth = 0;
        le.flexibleHeight = 0;

        GameObject innerFrame = new GameObject("InnerFrame", typeof(RectTransform), typeof(Image));
        innerFrame.transform.SetParent(bg.transform, false);
        RectTransform innerRT = innerFrame.GetComponent<RectTransform>();
        innerRT.anchorMin = new Vector2(0.05f, 0.05f);
        innerRT.anchorMax = new Vector2(0.95f, 0.95f);
        innerRT.offsetMin = Vector2.zero;
        innerRT.offsetMax = Vector2.zero;
        innerFrame.GetComponent<Image>().color = new Color(0.2f, 0.25f, 0.2f, 1f); // subtle tint

        // Placeholder icon
        GameObject icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        icon.transform.SetParent(innerFrame.transform, false);
        RectTransform iconRT = icon.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.15f, 0.15f);
        iconRT.anchorMax = new Vector2(0.85f, 0.85f);
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;
        icon.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.6f, 0.8f);
    }

    private static void CreateStatEntry(Transform parent, string statName)
    {
        GameObject bg = new GameObject("Stat_" + statName, typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(parent, false);
        bg.GetComponent<Image>().color = new Color(0.09f, 0.11f, 0.14f, 1f);
        Outline outline = bg.AddComponent<Outline>();
        outline.effectColor = new Color(0.35f, 0.4f, 0.45f, 1f);

        GameObject hlgGO = CreateHorizontalGroup(bg.transform, "Layout", 15);
        hlgGO.GetComponent<HorizontalLayoutGroup>().padding = new RectOffset(15, 10, 5, 5);
        hlgGO.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;
        hlgGO.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = false;
        
        RectTransform hlgRT = hlgGO.GetComponent<RectTransform>();
        hlgRT.anchorMin = Vector2.zero;
        hlgRT.anchorMax = Vector2.one;
        hlgRT.offsetMin = Vector2.zero;
        hlgRT.offsetMax = Vector2.zero;

        // Vert divider simulation: Icon | Line | Text
        GameObject icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        icon.transform.SetParent(hlgGO.transform, false);
        icon.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.75f, 1f);
        LayoutElement iconLE = icon.AddComponent<LayoutElement>();
        iconLE.preferredWidth = 35;
        iconLE.preferredHeight = 35;
        iconLE.flexibleWidth = 0;

        GameObject vertLine = new GameObject("VertLine", typeof(RectTransform), typeof(Image));
        vertLine.transform.SetParent(hlgGO.transform, false);
        vertLine.GetComponent<Image>().color = borderColor;
        LayoutElement vlLE = vertLine.AddComponent<LayoutElement>();
        vlLE.preferredWidth = 2;
        vlLE.preferredHeight = 40;
        vlLE.flexibleWidth = 0;

        TextMeshProUGUI txt = CreateText(hlgGO.transform, statName, 28, TextAlignmentOptions.Left);
        LayoutElement txtLE = txt.gameObject.GetComponent<LayoutElement>();
        if (txtLE == null) txtLE = txt.gameObject.AddComponent<LayoutElement>();
        txtLE.flexibleWidth = 1;
    }
    private static void CreateBottomButton(Transform parent, string iconLabel, string textStr, Color bgColor)
    {
        GameObject btnGO = new GameObject("Btn_" + textStr.Replace("\n", ""), typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(parent, false);
        btnGO.GetComponent<Image>().color = bgColor;
        
        Outline outline = btnGO.AddComponent<Outline>();
        outline.effectColor = new Color(0.6f, 0.8f, 1f, 0.5f);
        outline.effectDistance = new Vector2(2, -2);

        LayoutElement le = btnGO.AddComponent<LayoutElement>();
        le.preferredHeight = 80;

        GameObject hlgGO = CreateHorizontalGroup(btnGO.transform, "Content", 15);
        hlgGO.GetComponent<HorizontalLayoutGroup>().padding = new RectOffset(20, 20, 10, 10);
        hlgGO.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
        hlgGO.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = false;

        RectTransform hlgRT = hlgGO.GetComponent<RectTransform>();
        hlgRT.anchorMin = Vector2.zero;
        hlgRT.anchorMax = Vector2.one;
        hlgRT.offsetMin = Vector2.zero;
        hlgRT.offsetMax = Vector2.zero;

        // Placeholder Icon for Button
        GameObject icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        icon.transform.SetParent(hlgGO.transform, false);
        icon.GetComponent<Image>().color = new Color(0.9f, 0.9f, 0.9f, 1f);
        LayoutElement iconLE = icon.AddComponent<LayoutElement>();
        iconLE.preferredWidth = 40;
        iconLE.preferredHeight = 40;
        CreateText(icon.transform, iconLabel, 30, TextAlignmentOptions.Center).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        TextMeshProUGUI tmp = CreateText(hlgGO.transform, textStr, 30, TextAlignmentOptions.Left);
        tmp.lineSpacing = -20f; // Tighter line spacing for 2-line text
    }
}

import re

file_path = r'e:\game\legendofblood\legacyofblood_unity_mobile_portraitframe\Assets\Scripts\Editor\buildpanelmissng.cs'

with open(file_path, 'r', encoding='utf-8') as f:
    text = f.read()

def inject(target_str, new_str):
    global text
    if target_str in text:
        text = text.replace(target_str, new_str)

# 1. Scale CreateButton default size
inject('''        GameObject go = CreateUIObject(name, parent);
        go.AddComponent<Image>(); 
        Button btn = go.AddComponent<Button>();''',
'''        GameObject go = CreateUIObject(name, parent);
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 100);
        go.AddComponent<Image>(); 
        Button btn = go.AddComponent<Button>();''')

# 2. Add Background Image to CreateUIObject if name has Panel
inject('''    static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        if (parent != null) go.transform.SetParent(parent, false);
        return go;
    }''',
'''    static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        if (parent != null) go.transform.SetParent(parent, false);
        
        RectTransform rt = go.GetComponent<RectTransform>();
        if (name.StartsWith("Panel_")) {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            go.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        }
        return go;
    }''')

# 3. CreateScrollView padding/margins from Top (Below Tabs)
inject('''    static void CreateScrollView(string name, Transform parent, out RectTransform contentRect)
    {
        // Kh?i t?o c?m Scroll View co b?n
        GameObject sv = CreateEmptyStretch(name, parent);''',
'''    static void CreateScrollView(string name, Transform parent, out RectTransform contentRect)
    {
        // Kh?i t?o c?m Scroll View co b?n
        GameObject sv = CreateEmptyStretch(name, parent);
        sv.GetComponent<RectTransform>().offsetMax = new Vector2(0, -150);''')

# 4. Vertical Layout Group Spacing 
inject('''        vlg.spacing = 10; // Kho?ng cách gi?a các item''',
       '''        vlg.spacing = 30; // Kho?ng cách gi?a các item
        vlg.padding = new RectOffset(20, 20, 20, 20);''')


# 5. Fix Info Frame in EquipmentDetailPanel
inject('''        // Khung thông tin
        GameObject infoFrame = CreateUIObject("InfoFrame", panelGO.transform);''',
'''        // Khung thông tin
        GameObject infoFrame = CreateUIObject("InfoFrame", panelGO.transform);
        RectTransform infoRect = infoFrame.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0.1f, 0.4f); infoRect.anchorMax = new Vector2(0.9f, 0.8f);
        infoRect.offsetMin = Vector2.zero; infoRect.offsetMax = Vector2.zero;
        var infoVlg = infoFrame.AddComponent<VerticalLayoutGroup>();
        infoVlg.childControlHeight = true; infoVlg.childForceExpandHeight = false; infoVlg.spacing = 20;''')

# 6. Fix EquipmentDetail Buttons to be horizontal bottom
inject('''        // Các nút b?m
        AssignPrivateField(script, "closeButton", CreateButton("Btn_Close", panelGO.transform, "Ðóng"));
        AssignPrivateField(script, "equipButton", CreateButton("Btn_Equip", panelGO.transform, "M?c vào"));
        AssignPrivateField(script, "quickUpgradeButton", CreateButton("Btn_QuickUpgrade", panelGO.transform, "Nâng c?p nhanh"));''',
'''        // Các nút b?m
        GameObject btnGroup = CreateUIObject("ButtonGroup", panelGO.transform);
        RectTransform btnRect = btnGroup.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0, 0); btnRect.anchorMax = new Vector2(1, 0.15f);
        btnRect.offsetMin = Vector2.zero; btnRect.offsetMax = Vector2.zero;
        var hlg = btnGroup.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth = true; hlg.spacing = 30; hlg.padding = new RectOffset(50, 50, 50, 50);

        AssignPrivateField(script, "closeButton", CreateButton("Btn_Close", btnGroup.transform, "Ðóng"));
        AssignPrivateField(script, "equipButton", CreateButton("Btn_Equip", btnGroup.transform, "M?c vào"));
        AssignPrivateField(script, "quickUpgradeButton", CreateButton("Btn_QuickUpgrade", btnGroup.transform, "Nâng c?p nhanh"));''')

# 7. Fix Top Padding on Vertical Layout View
inject('''    static void CreateVerticalScrollView(string name, Transform parent, out RectTransform contentRect)
    {
        // 1. T?o Scroll View g?c
        GameObject sv = CreateUIObject(name, parent);
        RectTransform svRect = sv.GetComponent<RectTransform>();
        svRect.anchorMin = Vector2.zero;
        svRect.anchorMax = Vector2.one;
        svRect.sizeDelta = Vector2.zero;''',
'''    static void CreateVerticalScrollView(string name, Transform parent, out RectTransform contentRect)
    {
        // 1. T?o Scroll View g?c
        GameObject sv = CreateUIObject(name, parent);
        RectTransform svRect = sv.GetComponent<RectTransform>();
        svRect.anchorMin = Vector2.zero;
        svRect.anchorMax = Vector2.one;
        svRect.offsetMax = new Vector2(0, -150); // Cách top 150 d? ch?a view
        svRect.sizeDelta = Vector2.zero;''')

# 8. Add top text padding to createText generic
inject('''    static TextMeshProUGUI CreateText(string name, Transform parent, string defaultText)
    {
        GameObject go = CreateUIObject(name, parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();''',
'''    static TextMeshProUGUI CreateText(string name, Transform parent, string defaultText)
    {
        GameObject go = CreateUIObject(name, parent);
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 100);
        var tmp = go.AddComponent<TextMeshProUGUI>();''')

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(text)

print("Applied 1080x1920 layout logic successfully.")

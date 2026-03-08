import re

file_path = r'e:\game\legendofblood\legacyofblood_unity_mobile_portraitframe\Assets\Scripts\Editor\buildpanelmissng.cs'

with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Make the Button creation scale better in the helper
repl_btn = '''
    static Button CreateButton(string name, Transform parent, string btnText)
    {
        GameObject go = CreateUIObject(name, parent);
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 80);
        go.AddComponent<Image>(); 
        Button btn = go.AddComponent<Button>();
        CreateText("Text (TMP)", go.transform, btnText).color = Color.black;
        return btn;
    }'''

content = content.replace('''    static Button CreateButton(string name, Transform parent, string btnText)
    {
        GameObject go = CreateUIObject(name, parent);
        go.AddComponent<Image>(); 
        Button btn = go.AddComponent<Button>();
        CreateText("Text (TMP)", go.transform, btnText);
        return btn;
    }''', repl_btn)

# Make Close Button Anchor Top Right across the board
repl_close = '''
    static Button CreateCloseButton(Transform parent)
    {
        Button btn = CreateButton("Btn_Close", parent, "X");
        RectTransform rect = btn.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-20, -20);
        rect.sizeDelta = new Vector2(80, 80);
        return btn;
    }'''

if 'CreateCloseButton(Transform' not in content:
    content = content.replace('    // ================= CÁC HÀM TI?N ÍCH D?NG UI ================= //', '    // ================= CÁC HÀM TI?N ÍCH D?NG UI ================= //\n' + repl_close)

# Add layout specifically for Inventory tabs
search_inv_tabs = '''AssignPrivateField(script, "itemTabButton", CreateButton("Btn_ItemTab", panelGO.transform, "Items"));
        AssignPrivateField(script, "equipmentTabButton", CreateButton("Btn_EquipmentTab", panelGO.transform, "Equipments"));'''

repl_inv_tabs = '''
        GameObject tabGroup = CreateUIObject("TabGroup", panelGO.transform);
        RectTransform tabRect = tabGroup.GetComponent<RectTransform>();
        tabRect.anchorMin = new Vector2(0, 1); tabRect.anchorMax = new Vector2(1, 1);
        tabRect.pivot = new Vector2(0.5f, 1); tabRect.sizeDelta = new Vector2(0, 100);
        var hlGroup = tabGroup.AddComponent<HorizontalLayoutGroup>();
        hlGroup.childControlWidth = true; hlGroup.childForceExpandWidth = true;
        
        AssignPrivateField(script, "itemTabButton", CreateButton("Btn_ItemTab", tabGroup.transform, "Items"));
        AssignPrivateField(script, "equipmentTabButton", CreateButton("Btn_EquipmentTab", tabGroup.transform, "Equipments"));'''

content = content.replace(search_inv_tabs, repl_inv_tabs)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)

print("Final adjustments done.")

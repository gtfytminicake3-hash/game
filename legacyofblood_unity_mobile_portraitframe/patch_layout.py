import re

file_path = r'e:\game\legendofblood\legacyofblood_unity_mobile_portraitframe\Assets\Scripts\Editor\buildpanelmissng.cs'

with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

# ADD LAYOUTING LOGIC TO buildpanelmissng.cs
import sys

# Replace CloseButton Anchor (Top Right)
repl_close = '''
        Button btn = CreateButton("Btn_Close", panelGO.transform, "X");
        RectTransform btnRect = btn.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 1);
        btnRect.anchorMax = new Vector2(1, 1);
        btnRect.pivot = new Vector2(1, 1);
        btnRect.anchoredPosition = new Vector2(-20, -20);
        btnRect.sizeDelta = new Vector2(50, 50);
        AssignPrivateField(script, "closeButton", btn);
'''
content = re.sub(r'AssignPrivateField\(script,\s*"closeButton",\s*CreateButton\("[^"]+",\s*panelGO\.transform,\s*"[^"]+"\)\);', repl_close, content)

# Replace 2 Object Containers (Item, Equipment Pages) -> Stretch below top bar
repl_container = '''
        GameObject itemContainerGO = CreateEmptyStretch("Page_Items", panelGO.transform);
        itemContainerGO.GetComponent<RectTransform>().offsetMax = new Vector2(0, -100); // Cách top 100px
        AssignPrivateField(script, "itemContainerObj", itemContainerGO);
        GameObject equipmentContainerGO = CreateEmptyStretch("Page_Equipments", panelGO.transform);
        equipmentContainerGO.GetComponent<RectTransform>().offsetMax = new Vector2(0, -100);
        AssignPrivateField(script, "equipmentContainerObj", equipmentContainerGO);
'''
content = content.replace('''        GameObject itemContainerGO = CreateEmptyStretch("Page_Items", panelGO.transform);
        AssignPrivateField(script, "itemContainerObj", itemContainerGO);
        GameObject equipmentContainerGO = CreateEmptyStretch("Page_Equipments", panelGO.transform);
        AssignPrivateField(script, "equipmentContainerObj", equipmentContainerGO);''', repl_container)


# Replace ScrollView Stretch with Offset (Below Title/Tabs)
repl_scroll_items = '''
        RectTransform tempItemContent;
        CreateScrollView("Scroll View", itemContainerGO.transform, out tempItemContent);
        tempItemContent.parent.parent.GetComponent<RectTransform>().offsetMax = new Vector2(0, -100);
        AssignPrivateField(script, "itemContent", tempItemContent);
'''
content = content.replace('''        RectTransform tempItemContent;
        CreateScrollView("Scroll View", itemContainerGO.transform, out tempItemContent);
        AssignPrivateField(script, "itemContent", tempItemContent);''', repl_scroll_items)

# Add Layouts using regex specifically targeting BossBattle Column Layout
content = content.replace('''
        // C?t TRÁI (Thông tin Boss)
        GameObject leftCol = CreateUIObject("LeftColumn_BossInfo", panelGO.transform);''', '''
        // C?t TRÁI (Thông tin Boss)
        GameObject leftCol = CreateUIObject("LeftColumn_BossInfo", panelGO.transform);
        RectTransform leftRect = leftCol.GetComponent<RectTransform>();
        leftRect.anchorMin = new Vector2(0, 0.2f); leftRect.anchorMax = new Vector2(0.5f, 0.8f); leftRect.offsetMin = Vector2.zero; leftRect.offsetMax = Vector2.zero;
        leftCol.AddComponent<VerticalLayoutGroup>().childControlHeight = false;
''')

content = content.replace('''
        // C?t PH?I (Co ch? & K? nang)
        GameObject rightCol = CreateUIObject("RightColumn_Mechanics", panelGO.transform);''', '''
        // C?t PH?I (Co ch? & K? nang)
        GameObject rightCol = CreateUIObject("RightColumn_Mechanics", panelGO.transform);
        RectTransform rightRect = rightCol.GetComponent<RectTransform>();
        rightRect.anchorMin = new Vector2(0.5f, 0.2f); rightRect.anchorMax = new Vector2(1, 0.8f); rightRect.offsetMin = Vector2.zero; rightRect.offsetMax = Vector2.zero;
        rightCol.AddComponent<VerticalLayoutGroup>().childControlHeight = false;
''')

# Bottom footer in Boss
content = content.replace('''AssignPrivateField(script, "challengeButton", CreateButton("Btn_Challenge", panelGO.transform, "Khiêu Chi?n"));''', '''
        Button challengeBtn = CreateButton("Btn_Challenge", panelGO.transform, "Khiêu Chi?n");
        RectTransform chalRect = challengeBtn.GetComponent<RectTransform>();
        chalRect.anchorMin = new Vector2(0.5f, 0); chalRect.anchorMax = new Vector2(0.5f, 0); chalRect.anchoredPosition = new Vector2(0, 50); chalRect.sizeDelta = new Vector2(250, 80);
        AssignPrivateField(script, "challengeButton", challengeBtn);
''')

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)

print("Patch Layout Done.")

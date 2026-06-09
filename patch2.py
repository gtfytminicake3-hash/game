import re

filepath = 'd:/legendofblood/legacyofblood_unity_mobile_portraitframe/Assets/Scripts/UI/CombatVisualizerPanel.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

old_prefab = r'        private GameObject CreateFallbackPrefab\(\).*?return unitTemplate;\s*\}'

new_prefab = '''        private GameObject CreateSimpleUnitPrefab()
        {
            GameObject unitTemplate = new GameObject("SimpleCombatUnitView");
            unitTemplate.SetActive(false);
            RectTransform utRect = unitTemplate.AddComponent<RectTransform>();
            utRect.sizeDelta = new Vector2(160, 220);
            
            UnityEngine.UI.Image avatarImg = new GameObject("Avatar").AddComponent<UnityEngine.UI.Image>();
            avatarImg.transform.SetParent(unitTemplate.transform, false);
            avatarImg.rectTransform.anchorMin = Vector2.zero; avatarImg.rectTransform.anchorMax = Vector2.one;
            avatarImg.rectTransform.sizeDelta = Vector2.zero;
            avatarImg.color = Color.white; 
            
            UnityEngine.UI.Image bg = unitTemplate.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0,0,0, 0.5f);

            GameObject bgObj = new GameObject("HpBarBackground"); 
            bgObj.transform.SetParent(unitTemplate.transform, false);
            UnityEngine.UI.Image bgImg = bgObj.AddComponent<UnityEngine.UI.Image>(); bgImg.color = new Color(0.2f, 0, 0, 1f);
            bgImg.rectTransform.anchorMin = new Vector2(0.5f, 1f); 
            bgImg.rectTransform.anchorMax = new Vector2(0.5f, 1f); 
            bgImg.rectTransform.pivot = new Vector2(0.5f, 1f);
            bgImg.rectTransform.sizeDelta = new Vector2(80, 10);
            bgImg.rectTransform.anchoredPosition = new Vector2(0, 15);
            
            GameObject fillObj = new GameObject("HpBarFill"); 
            fillObj.transform.SetParent(bgObj.transform, false);
            UnityEngine.UI.Image fillImg = fillObj.AddComponent<UnityEngine.UI.Image>(); 
            fillImg.color = Color.green;
            fillImg.type = UnityEngine.UI.Image.Type.Filled;
            fillImg.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            fillImg.fillAmount = 1f;
            fillImg.rectTransform.anchorMin = Vector2.zero; fillImg.rectTransform.anchorMax = Vector2.one; 
            fillImg.rectTransform.sizeDelta = Vector2.zero;

            GameObject dmgCgObj = new GameObject("DamageTextContainer");
            dmgCgObj.transform.SetParent(unitTemplate.transform, false);
            RectTransform dmgRect = dmgCgObj.AddComponent<RectTransform>();
            CanvasGroup dmgCg = dmgCgObj.AddComponent<CanvasGroup>();
            dmgRect.anchorMin = new Vector2(0, 0.5f); dmgRect.anchorMax = new Vector2(1, 1.5f);
            dmgRect.sizeDelta = Vector2.zero;
            
            TMPro.TextMeshProUGUI dmgTxt = new GameObject("DamageText").AddComponent<TMPro.TextMeshProUGUI>();
            dmgTxt.transform.SetParent(dmgCg.transform, false);
            dmgTxt.rectTransform.anchorMin = Vector2.zero; dmgTxt.rectTransform.anchorMax = Vector2.one;
            dmgTxt.rectTransform.sizeDelta = Vector2.zero;
            dmgTxt.text = ""; dmgTxt.fontSize = 45;
            dmgTxt.fontStyle = TMPro.FontStyles.Bold; dmgTxt.alignment = TMPro.TextAlignmentOptions.Center;

            BattleUnitUI unitScript = unitTemplate.AddComponent<BattleUnitUI>();
            unitScript.avatarImage = avatarImg;
            unitScript.hpFillImage = fillImg;
            unitScript.damageTextCanvasGroup = dmgCg;
            unitScript.damageText = dmgTxt;

            return unitTemplate;
        }'''
content = re.sub(old_prefab, new_prefab, content, flags=re.DOTALL)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Patched CreateSimpleUnitPrefab successfully.")

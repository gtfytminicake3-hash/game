using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class HeroCardCopier : MonoBehaviour
{
    [MenuItem("UI Tools/Create Beautiful Injured Card From Base")]
    public static void CreateBeautifulInjuredCard()
    {
        string sourcePath = "Assets/Prefabs/HeroCard_Prefab.prefab";
        string targetPath = "Assets/Prefabs/InjuredHeroCard_Beautiful.prefab";

        if (!System.IO.File.Exists(sourcePath)) {
            Debug.LogError("Source prefab not found at: " + sourcePath);
            return;
        }

        AssetDatabase.DeleteAsset(targetPath);
        if (!AssetDatabase.CopyAsset(sourcePath, targetPath)) {
            Debug.LogError("Failed to copy prefab!");
            return;
        }

        GameObject prefab = PrefabUtility.LoadPrefabContents(targetPath);

        // Add the component
        LegendOfBlood.InjuredHeroCard script = prefab.GetComponent<LegendOfBlood.InjuredHeroCard>();
        if (script == null) script = prefab.AddComponent<LegendOfBlood.InjuredHeroCard>();

        // We need: HPBarArea (graphic), TimerText, PriceButton

        // 1. HPBarArea
        Transform hpAreaOld = prefab.transform.Find("HPBarArea");
        if (hpAreaOld != null) Undo.DestroyObjectImmediate(hpAreaOld.gameObject);
        
        GameObject hpArea = new GameObject("HPBarArea");
        hpArea.transform.SetParent(prefab.transform, false);
        RectTransform areaRt = hpArea.AddComponent<RectTransform>();
        areaRt.anchorMin = new Vector2(0.5f, 0);
        areaRt.anchorMax = new Vector2(0.5f, 0);
        areaRt.pivot = new Vector2(0.5f, 0);
        areaRt.anchoredPosition = new Vector2(0, 43); 
        areaRt.sizeDelta = new Vector2(130, 24);

        GameObject hpBarBg = new GameObject("HPBarBG");
        hpBarBg.transform.SetParent(hpArea.transform, false);
        RectTransform bgRt = hpBarBg.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
        Image bgImg = hpBarBg.AddComponent<Image>();
        bgImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Hospital/HPFrame.png");
        bgImg.preserveAspect = true;

        GameObject hpBarFill = new GameObject("HPBarFill");
        hpBarFill.transform.SetParent(hpArea.transform, false);
        RectTransform fillRt = hpBarFill.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;
        Image fillImg = hpBarFill.AddComponent<Image>();
        fillImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Hospital/HPFill.png");
        fillImg.preserveAspect = true;
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillOrigin = 0;
        fillImg.fillAmount = 1.0f;

        // 2. Heal Button
        Transform healBtnOld = prefab.transform.Find("HealButton");
        if (healBtnOld != null) Undo.DestroyObjectImmediate(healBtnOld.gameObject);
        
        GameObject healBtn = new GameObject("HealButton");
        healBtn.transform.SetParent(prefab.transform, false);
        RectTransform healBtnRt = healBtn.AddComponent<RectTransform>();
        healBtnRt.anchorMin = new Vector2(0, 0); healBtnRt.anchorMax = new Vector2(1, 0);
        healBtnRt.pivot = new Vector2(0.5f, 0);
        healBtnRt.offsetMin = new Vector2(8, 6); healBtnRt.offsetMax = new Vector2(-8, 40); 
        Image healBtnImg = healBtn.AddComponent<Image>();
        healBtnImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Hospital/HealButton.png");
        if (healBtnImg.sprite == null) healBtnImg.color = new Color(0.2f, 0.8f, 0.2f, 1f);
        else healBtnImg.preserveAspect = true;
        
        Button btnComp = healBtn.AddComponent<Button>();

        GameObject healTextObj = new GameObject("Text");
        healTextObj.transform.SetParent(healBtn.transform, false);
        RectTransform healTextRt = healTextObj.AddComponent<RectTransform>();
        healTextRt.anchorMin = Vector2.zero; healTextRt.anchorMax = Vector2.one;
        healTextRt.offsetMin = Vector2.zero; healTextRt.offsetMax = new Vector2(-40, 0);
        var healTmp = healTextObj.AddComponent<TextMeshProUGUI>();
        healTmp.text = "HEAL <color=#FFD700>50G</color>";
        healTmp.alignment = TextAlignmentOptions.Center;
        healTmp.fontSize = 16;
        healTmp.fontStyle = FontStyles.Bold;
        healTmp.color = new Color(0.9f, 0.9f, 0.9f);

        // 3. Timer Text
        Transform timerOld = prefab.transform.Find("TimerText");
        if (timerOld != null) Undo.DestroyObjectImmediate(timerOld.gameObject);

        GameObject timeTextObj = new GameObject("TimerText");
        timeTextObj.transform.SetParent(prefab.transform, false);
        RectTransform timeRt = timeTextObj.AddComponent<RectTransform>();
        timeRt.anchorMin = new Vector2(0.5f, 0); timeRt.anchorMax = new Vector2(0.5f, 0);
        timeRt.pivot = new Vector2(0.5f, 0);
        timeRt.anchoredPosition = new Vector2(0, 75); // Place it below name plate normally
        timeRt.sizeDelta = new Vector2(140, 40);
        var timeTmp = timeTextObj.AddComponent<TextMeshProUGUI>();
        timeTmp.text = "00:00:00";
        timeTmp.alignment = TextAlignmentOptions.Center;
        timeTmp.fontSize = 14;
        timeTmp.color = Color.white;

        // Force disable click on the entire card because Injured card only responds to HealButton
        Button mainBtn = prefab.GetComponent<Button>();
        if (mainBtn != null) mainBtn.interactable = false; 

        // Set Serialized fields using SerializedObject to bypass private constraints
        SerializedObject so = new SerializedObject(script);
        so.Update();
        so.FindProperty("timerText").objectReferenceValue = timeTmp;
        so.FindProperty("costText").objectReferenceValue = healTmp; // Just in case costText is same as heal text
        so.FindProperty("healButton").objectReferenceValue = btnComp;
        // healAdButton left null
        so.ApplyModifiedProperties();

        // Remove any old mock components that look ugly
        // Such as generic HPBar (solid color) from HeroCard generator mockups
        Transform testOldHP = prefab.transform.Find("HPBar");
        if (testOldHP != null) Undo.DestroyObjectImmediate(testOldHP.gameObject);

        // Make sure background is HospitalCardFrame
        Transform cardFrame = prefab.transform.Find("CardFrame");
        if (cardFrame != null) {
             var img = cardFrame.GetComponent<Image>();
             var frameSp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Hospital/HospitalCardFrame.png");
             if (frameSp != null) { img.sprite = frameSp; img.preserveAspect = true; }
        }

        PrefabUtility.SaveAsPrefabAsset(prefab, targetPath);
        PrefabUtility.UnloadPrefabContents(prefab);
        Debug.Log("Created " + targetPath + " successfully!");

        // NOW assign it to HospitalPanel!
        GameObject newRef = AssetDatabase.LoadAssetAtPath<GameObject>(targetPath);
        
        // Scene panels
        int updateCount = 0;
        var panels = Object.FindObjectsByType<LegendOfBlood.HospitalPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in panels) {
            SerializedObject pso = new SerializedObject(p);
            pso.Update();
            pso.FindProperty("injuredHeroCardPrefab").objectReferenceValue = newRef;
            pso.ApplyModifiedProperties();
            EditorUtility.SetDirty(p.gameObject);
            updateCount++;
        }

        // Prefab panels
        string[] pGuids = AssetDatabase.FindAssets("t:Prefab Hospital");
        if (pGuids.Length == 0) pGuids = AssetDatabase.FindAssets("t:Prefab Panel_Hospital");
        foreach(string g in pGuids) {
            string pPath = AssetDatabase.GUIDToAssetPath(g);
            GameObject pPrefab = PrefabUtility.LoadPrefabContents(pPath);
            LegendOfBlood.HospitalPanel panelScript = pPrefab.GetComponentInChildren<LegendOfBlood.HospitalPanel>(true);
            if (panelScript != null) {
                SerializedObject pso = new SerializedObject(panelScript);
                pso.Update();
                pso.FindProperty("injuredHeroCardPrefab").objectReferenceValue = newRef;
                pso.ApplyModifiedProperties();
                PrefabUtility.SaveAsPrefabAsset(pPrefab, pPath);
                PrefabUtility.UnloadPrefabContents(pPrefab);
                updateCount++;
            } else {
                PrefabUtility.UnloadPrefabContents(pPrefab);
            }
        }
        Debug.Log("Assigned new perfect Prefab to " + updateCount + " panels!");
    }
}

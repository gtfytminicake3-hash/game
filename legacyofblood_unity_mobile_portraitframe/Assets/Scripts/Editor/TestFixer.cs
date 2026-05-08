using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class TestFixer : MonoBehaviour
{
    [MenuItem("UI Tools/Replace Old HP Bar In Cards")]
    public static void Fix()
    {
        string framePath = "Assets/Art/UI/Hospital/HPFrame.png";
        string fillPath = "Assets/Art/UI/Hospital/HPFill.png";
        
        Sprite frameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(framePath);
        Sprite fillSprite = AssetDatabase.LoadAssetAtPath<Sprite>(fillPath);

        if (frameSprite == null || fillSprite == null) {
            Debug.LogError("HP sprites not found!");
            return;
        }

        int count = 0;
        void ReplaceBar(Transform card) {
            Transform oldHpBar = card.Find("HPBar");
            Transform existingArea = card.Find("HPBarArea");
            
            if (oldHpBar != null) {
                Undo.DestroyObjectImmediate(oldHpBar.gameObject);
            }
            if (existingArea != null) {
                Undo.DestroyObjectImmediate(existingArea.gameObject);
            }

            // Create new HPBarArea
            GameObject hpArea = new GameObject("HPBarArea");
            hpArea.AddComponent<RectTransform>();
            hpArea.transform.SetParent(card, false);
            
            RectTransform areaRt = hpArea.GetComponent<RectTransform>();
            areaRt.anchorMin = new Vector2(0.5f, 0);
            areaRt.anchorMax = new Vector2(0.5f, 0);
            areaRt.pivot = new Vector2(0.5f, 0);
            areaRt.anchoredPosition = new Vector2(0, 43); // Bump slightly to fit better
            areaRt.sizeDelta = new Vector2(130, 24);

            // Create HPBarBG
            GameObject hpBarBg = new GameObject("HPBarBG");
            hpBarBg.AddComponent<RectTransform>();
            hpBarBg.transform.SetParent(hpArea.transform, false);
            RectTransform bgRt = hpBarBg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            Image bgImg = hpBarBg.AddComponent<Image>();
            bgImg.sprite = frameSprite;
            bgImg.preserveAspect = true;

            // Create HPBarFill
            GameObject hpBarFill = new GameObject("HPBarFill");
            hpBarFill.AddComponent<RectTransform>();
            hpBarFill.transform.SetParent(hpArea.transform, false);
            RectTransform fillRt = hpBarFill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;

            Image fillImg = hpBarFill.AddComponent<Image>();
            fillImg.sprite = fillSprite;
            fillImg.preserveAspect = true;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = 0; // Left
            fillImg.fillAmount = 0.5f;

            // Optional: ensuring it sits right before PriceButton in hierarchy
            Transform priceBtn = card.Find("PriceButton");
            if (priceBtn != null) {
                hpArea.transform.SetSiblingIndex(priceBtn.GetSiblingIndex());
            }

            count++;
        }

        var cards = Object.FindObjectsByType<LegendOfBlood.InjuredHeroCard>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in cards) { 
            ReplaceBar(c.transform); 
            EditorUtility.SetDirty(c.gameObject); 
        }

        var panels = Object.FindObjectsByType<LegendOfBlood.HospitalPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var panel in panels) {
            Transform severeRow = panel.transform.Find("ScrollView_Severe/Viewport/Content");
            if (severeRow != null) {
                foreach (Transform child in severeRow) ReplaceBar(child);
            }
            Transform lightRow = panel.transform.Find("ScrollView_Light/Viewport/Content");
            if (lightRow != null) {
                foreach (Transform child in lightRow) ReplaceBar(child);
            }
            EditorUtility.SetDirty(panel.gameObject);
        }

        string[] cardGuids = AssetDatabase.FindAssets("t:Prefab InjuredHeroCard");
        foreach(string g in cardGuids) {
            string path = AssetDatabase.GUIDToAssetPath(g);
            GameObject p = PrefabUtility.LoadPrefabContents(path);
            if(p != null) {
                ReplaceBar(p.transform);
                PrefabUtility.SaveAsPrefabAsset(p, path);
                PrefabUtility.UnloadPrefabContents(p);
            }
        }

        Debug.Log("Replaced HP bar on " + count + " active elements and all prefabs.");
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }

    [MenuItem("UI Tools/Populate Hospital Mockups")]
    public static void Populate() {
        var panels = Object.FindObjectsByType<LegendOfBlood.HospitalPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (panels.Length == 0) {
            Debug.LogError("No HospitalPanel found!");
            return;
        }
        var panel = panels[0];
        
        string[] guids = AssetDatabase.FindAssets("t:Prefab InjuredHeroCard");
        if (guids.Length == 0) return;
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[0]));
        
        Transform severeRow = panel.transform.Find("ScrollView_Severe/Viewport/Content");
        if (severeRow != null) {
            for(int i=severeRow.childCount-1; i>=0; i--) Undo.DestroyObjectImmediate(severeRow.GetChild(i).gameObject);
            for(int i=0; i<3; i++) {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, severeRow);
                Undo.RegisterCreatedObjectUndo(instance, "Mock");
                // Mock HP
                Transform hpFill = instance.transform.Find("HPBarArea/HPBarFill");
                if (hpFill != null) hpFill.GetComponent<Image>().fillAmount = 0.3f * (i+1);
                Transform namePlate = instance.transform.Find("NamePlate/Text");
                if (namePlate != null) namePlate.GetComponent<TMPro.TextMeshProUGUI>().text = "Hero " + (i+1);
            }
        }
        Transform lightRow = panel.transform.Find("ScrollView_Light/Viewport/Content");
        if (lightRow != null) {
            for(int i=lightRow.childCount-1; i>=0; i--) Undo.DestroyObjectImmediate(lightRow.GetChild(i).gameObject);
            for(int i=0; i<3; i++) {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, lightRow);
                Undo.RegisterCreatedObjectUndo(instance, "Mock");
                Transform hpFill = instance.transform.Find("HPBarArea/HPBarFill");
                if (hpFill != null) hpFill.GetComponent<Image>().fillAmount = 0.5f + 0.1f * i;
                Transform namePlate = instance.transform.Find("NamePlate/Text");
                if (namePlate != null) namePlate.GetComponent<TMPro.TextMeshProUGUI>().text = "Hero " + (i+4);
            }
        }
        EditorUtility.SetDirty(panel.gameObject);
        Debug.Log("Populated mockups from prefab!");
    }

    [MenuItem("UI Tools/Inject Real Injured Heroes (Play Mode)")]
    public static void InjectRealData() {
        if (!Application.isPlaying) {
            Debug.LogError("Must be in Play Mode to inject real data into DataManager!");
            return;
        }

        var dataManager = LegendOfBlood.DataManager.Instance;
        if (dataManager == null || dataManager.Player == null) {
            Debug.LogError("DataManager or Player is null!");
            return;
        }

        long currentTime = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        // Add 3 Severely Injured
        for (int i=0; i<3; i++) {
            var hd = new LegendOfBlood.HeroData(System.Guid.NewGuid().ToString(), "Trùm Băng Đảng " + i, LegendOfBlood.Gender.Male);
            hd.CalculateBaseStats();
            hd.currentHp = hd.GetFinalStats().hp * 0.2f * i;
            hd.isSeverelyInjured = true;
            hd.injuryEndTime = currentTime + 3600000;
            dataManager.Player.Heroes.Add(hd);
        }

        // Add 3 Lightly Injured
        for (int i=0; i<3; i++) {
            var hd = new LegendOfBlood.HeroData(System.Guid.NewGuid().ToString(), "Thiếu Nữ Giữ Xe " + i, LegendOfBlood.Gender.Female);
            hd.CalculateBaseStats();
            hd.currentHp = hd.GetFinalStats().hp * (0.5f + 0.1f * i);
            hd.isLightlyInjured = true;
            hd.lightInjuryEndTime = currentTime + 1800000;
            dataManager.Player.Heroes.Add(hd);
        }

        var panels = Object.FindObjectsByType<LegendOfBlood.HospitalPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (panels.Length > 0 && panels[0].gameObject.activeInHierarchy) {
            // Need to trigger RefreshLists(). Since it's private, we can disable/enable the panel
            panels[0].gameObject.SetActive(false);
            panels[0].gameObject.SetActive(true);
        }
        
        Debug.Log("Injected 6 injured heroes into real DataManager!");
    }
}

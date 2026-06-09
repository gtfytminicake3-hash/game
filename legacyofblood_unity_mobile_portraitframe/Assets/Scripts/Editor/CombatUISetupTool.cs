#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;
using LegendOfBlood.Combat;
using LegendOfBlood;

public class CombatUISetupTool
{
    private const string HERO_CARD_PATH = "Assets/Prefabs/HeroCard_Prefab.prefab";
    private const string COMBAT_CARD_PATH = "Assets/Prefabs/CombatHeroCard_Prefab.prefab";
    private const string PANEL_BATTLE_PATH = "Assets/Prefabs/Panel/Panel_Battle.prefab";
    private const string BACKGROUND_IMAGE_PATH = "Assets/Resources/Avatars/AnhHeroCard/background.jpeg";

    [MenuItem("Tools/Auto Setup Combat Replay UI")]
    public static void SetupCombatUI()
    {
        Debug.Log("[CombatUISetupTool] Starting Auto Setup Combat Replay UI...");

        // 1 & 2 & 4. Setup CombatHeroCard_Prefab
        GameObject combatCardPrefab = SetupCombatCardPrefab();

        // 6. Fix Sprite Import
        Sprite bgSprite = SetupBackgroundSprite();

        // 3. Setup CombatVisualizerPanel
        if (combatCardPrefab != null)
        {
            SetupCombatVisualizerPanel(combatCardPrefab, bgSprite);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CombatUISetupTool] Finished Auto Setup Combat Replay UI.");
    }

    private static GameObject SetupCombatCardPrefab()
    {
        GameObject heroCardSource = AssetDatabase.LoadAssetAtPath<GameObject>(HERO_CARD_PATH);
        if (heroCardSource == null)
        {
            Debug.LogError($"[CombatUISetupTool] Could not find source HeroCard_Prefab at {HERO_CARD_PATH}!");
            return null;
        }
        Debug.Log($"[CombatUISetupTool] Found source HeroCard_Prefab at {HERO_CARD_PATH}");

        GameObject combatCardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(COMBAT_CARD_PATH);
        if (combatCardPrefab == null)
        {
            Debug.Log($"[CombatUISetupTool] CombatHeroCard_Prefab not found. Duplicating from {HERO_CARD_PATH}...");
            AssetDatabase.CopyAsset(HERO_CARD_PATH, COMBAT_CARD_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            combatCardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(COMBAT_CARD_PATH);
            Debug.Log($"[CombatUISetupTool] Duplicated CombatHeroCard_Prefab at {COMBAT_CARD_PATH}");
        }
        else
        {
            Debug.Log($"[CombatUISetupTool] Found existing CombatHeroCard_Prefab at {COMBAT_CARD_PATH}");
        }

        // Open Prefab for editing
        GameObject prefabInstance = PrefabUtility.LoadPrefabContents(COMBAT_CARD_PATH);

        // Remove old HeroCard component if it exists
        var oldHeroCard = prefabInstance.GetComponent("HeroCard");
        if (oldHeroCard != null)
        {
            Object.DestroyImmediate(oldHeroCard, true);
            Debug.Log("[CombatUISetupTool] Removed original HeroCard component from CombatHeroCard_Prefab.");
        }

        // Ensure BattleUnitUI exists
        BattleUnitUI battleUnitUI = prefabInstance.GetComponent<BattleUnitUI>();
        if (battleUnitUI == null)
        {
            battleUnitUI = prefabInstance.AddComponent<BattleUnitUI>();
            Debug.Log("[CombatUISetupTool] Added BattleUnitUI component.");
        }

        // Hide Rarity Elements
        Transform[] allChildren = prefabInstance.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            string nameLower = child.name.ToLower();
            if (nameLower.Contains("rarity") || nameLower.Contains("glow") || nameLower.Contains("frame_color"))
            {
                child.gameObject.SetActive(false);
                Debug.Log($"[CombatUISetupTool] Hid rarity/glow visual: {child.name}");
            }
        }

        // Bind fields
        BindBattleUnitUIFields(prefabInstance, battleUnitUI);

        PrefabUtility.SaveAsPrefabAsset(prefabInstance, COMBAT_CARD_PATH);
        PrefabUtility.UnloadPrefabContents(prefabInstance);

        return AssetDatabase.LoadAssetAtPath<GameObject>(COMBAT_CARD_PATH);
    }

        private static void BindBattleUnitUIFields(GameObject root, BattleUnitUI ui)
    {
        // Try to bind avatar
        if (ui.avatarImage == null)
        {
            var images = root.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img.name.ToLower().Contains("avatar") || img.name.ToLower().Contains("icon"))
                {
                    ui.avatarImage = img;
                    Debug.Log($"[CombatUISetupTool] Bound avatarImage to {img.name}");
                    break;
                }
            }
        }

        // Ensure Combat UI elements exist
        if (ui.hpSlider == null)
        {
            GameObject sliderObj = new GameObject("HpSlider", typeof(RectTransform), typeof(Slider));
            sliderObj.transform.SetParent(root.transform, false);
            ui.hpSlider = sliderObj.GetComponent<Slider>();
            
            // basic visual for slider
            GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(sliderObj.transform, false);
            bgObj.GetComponent<Image>().color = Color.black;
            
            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObj.transform, false);
            
            GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillObj.transform.SetParent(fillArea.transform, false);
            fillObj.GetComponent<Image>().color = Color.green;
            
            ui.hpSlider.fillRect = fillObj.GetComponent<RectTransform>();
            
            RectTransform rt = sliderObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 0);
            rt.anchoredPosition = new Vector2(0, -10); rt.sizeDelta = new Vector2(0, 15);
            Debug.Log("[CombatUISetupTool] Created and bound hpSlider.");
        }

        if (ui.hpText == null)
        {
            GameObject hpTextObj = new GameObject("HpText", typeof(RectTransform), typeof(TextMeshProUGUI));
            hpTextObj.transform.SetParent(ui.hpSlider.transform, false);
            ui.hpText = hpTextObj.GetComponent<TextMeshProUGUI>();
            ui.hpText.fontSize = 12;
            ui.hpText.alignment = TextAlignmentOptions.Center;
            
            RectTransform rt = hpTextObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;
            Debug.Log("[CombatUISetupTool] Created and bound hpText.");
        }

        if (ui.damageTextCanvasGroup == null)
        {
            GameObject dmgGroupObj = new GameObject("DamageGroup", typeof(RectTransform), typeof(CanvasGroup));
            dmgGroupObj.transform.SetParent(root.transform, false);
            ui.damageTextCanvasGroup = dmgGroupObj.GetComponent<CanvasGroup>();
            ui.damageTextCanvasGroup.alpha = 0;
            
            RectTransform rt = dmgGroupObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f); rt.anchorMax = new Vector2(1, 1.5f);
            
            GameObject dmgTextObj = new GameObject("DamageText", typeof(RectTransform), typeof(TextMeshProUGUI));
            dmgTextObj.transform.SetParent(dmgGroupObj.transform, false);
            ui.damageText = dmgTextObj.GetComponent<TextMeshProUGUI>();
            ui.damageText.fontSize = 30;
            ui.damageText.color = Color.red;
            ui.damageText.fontStyle = FontStyles.Bold;
            ui.damageText.alignment = TextAlignmentOptions.Center;
            
            RectTransform txtRt = dmgTextObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one; txtRt.sizeDelta = Vector2.zero;
            
            Debug.Log("[CombatUISetupTool] Created and bound damageTextCanvasGroup and damageText.");
        }
    }

    private static Sprite SetupBackgroundSprite()
    {
        TextureImporter importer = AssetImporter.GetAtPath(BACKGROUND_IMAGE_PATH) as TextureImporter;
        if (importer != null)
        {
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
                Debug.Log($"[CombatUISetupTool] Fixed Sprite import settings for {BACKGROUND_IMAGE_PATH}");
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(BACKGROUND_IMAGE_PATH);
        }
        else
        {
            Debug.LogWarning($"[CombatUISetupTool] Background image not found at {BACKGROUND_IMAGE_PATH}");
            return null;
        }
    }

    private static void SetupCombatVisualizerPanel(GameObject battleUnitPrefab, Sprite bgSprite)
    {
        GameObject panelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PANEL_BATTLE_PATH);
        if (panelPrefab == null)
        {
            Debug.LogError($"[CombatUISetupTool] Could not find Panel_Battle at {PANEL_BATTLE_PATH}");
            return;
        }

        GameObject prefabInstance = PrefabUtility.LoadPrefabContents(PANEL_BATTLE_PATH);
        CombatVisualizerPanel visualizer = prefabInstance.GetComponent<CombatVisualizerPanel>();

        if (visualizer != null)
        {
            visualizer.battleUnitPrefab = battleUnitPrefab;
            Debug.Log($"[CombatUISetupTool] Assigned battleUnitPrefab to {visualizer.name}");
        }

        // Handle Background
        if (bgSprite != null)
        {
            Transform bgTransform = prefabInstance.transform.Find("Background");
            Image bgImage = null;

            if (bgTransform != null)
            {
                bgImage = bgTransform.GetComponent<Image>();
                if (bgImage == null) bgImage = bgTransform.gameObject.AddComponent<Image>();
            }
            else
            {
                GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
                bgObj.transform.SetParent(prefabInstance.transform, false);
                bgObj.transform.SetAsFirstSibling();
                
                RectTransform rt = bgObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                
                bgImage = bgObj.GetComponent<Image>();
            }

            bgImage.sprite = bgSprite;
            bgImage.color = Color.white; // reset color if it was black/gray
            Debug.Log($"[CombatUISetupTool] Assigned background sprite to Panel_Battle.");
        }

        PrefabUtility.SaveAsPrefabAsset(prefabInstance, PANEL_BATTLE_PATH);
        PrefabUtility.UnloadPrefabContents(prefabInstance);
    }
}
#endif



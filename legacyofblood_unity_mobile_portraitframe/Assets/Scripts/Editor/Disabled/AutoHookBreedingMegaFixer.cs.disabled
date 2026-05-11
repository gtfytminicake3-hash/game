using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using LegendOfBlood;
using System.Collections.Generic;

[InitializeOnLoad]
public static class AutoHookBreedingMegaFixer
{
    static AutoHookBreedingMegaFixer()
    {
        EditorApplication.delayCall += Hook;
    }
    
    static void Hook()
    {
        var menu = Object.FindAnyObjectByType<MenuPanel>(FindObjectsInactive.Include);
        if (menu != null)
        {
            var allTransforms = menu.GetComponentsInChildren<Transform>(true);
            List<Transform> breedings = new List<Transform>();
            foreach (var t in allTransforms)
            {
                if (t.name == "MenuItem_Breeding" && t.parent != null && t.parent.name.Contains("MenuItem_Breeding") == false)
                {
                    // Found a root MenuItem_Breeding (not a child of itself)
                    if (!breedings.Contains(t)) breedings.Add(t);
                }
            }

            if (breedings.Count >= 2)
            {
                // The first one is the original Settings button
                Transform settingObj = breedings[0];
                Transform breedingObj = breedings[1];

                // 1. Rename the first one back
                settingObj.name = "MenuItem_Settings";

                // 2. Change text back
                var settingTexts = settingObj.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                foreach (var st in settingTexts) 
                {
                    if (st.name == "Text" || st.name == "LabelText") st.text = "Setting";
                }

                // 3. Make sure breeding text is Breeding
                var breedingTexts = breedingObj.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                foreach (var bt in breedingTexts) 
                {
                    if (bt.name == "Text" || bt.name == "LabelText") bt.text = "Breeding";
                }

                // 4. Hook proper buttons
                var settingBtn = settingObj.GetComponent<Button>();
                var breedingBtn = breedingObj.GetComponent<Button>();

                var so = new SerializedObject(menu);
                if (settingBtn != null) 
                {
                    var p1 = so.FindProperty("settingButton");
                    if (p1 != null) p1.objectReferenceValue = settingBtn;
                }
                if (breedingBtn != null)
                {
                    var p2 = so.FindProperty("breedingButton");
                    if (p2 != null) p2.objectReferenceValue = breedingBtn;
                }
                
                so.ApplyModifiedProperties();
                Debug.Log("[MegaFixer] ALL FIXED! Settings is Settings, Breeding is Breeding.");
            }
            else
            {
                Debug.Log($"[MegaFixer] Found {breedings.Count} breeding objects. Cannot fix.");
            }
        }
    }
}

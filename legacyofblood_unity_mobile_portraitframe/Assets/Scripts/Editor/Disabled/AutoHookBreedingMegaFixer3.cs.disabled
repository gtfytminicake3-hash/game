using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using LegendOfBlood;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

[InitializeOnLoad]
public static class AutoHookBreedingMegaFixer3
{
    static AutoHookBreedingMegaFixer3()
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
                if (t.name.Contains("MenuItem_Breed") || t.name.Contains("MenuItem_Setting"))
                {
                    if (t.parent != null && !t.parent.name.Contains("MenuItem_"))
                    {
                        if (!breedings.Contains(t)) breedings.Add(t);
                    }
                }
            }

            Debug.Log($"[MegaFixer] Found {breedings.Count} buttons");

            if (breedings.Count >= 2)
            {
                breedings.Sort((a,b) => a.GetSiblingIndex().CompareTo(b.GetSiblingIndex()));

                Transform settingObj = breedings[0];
                Transform breedingObj = breedings[1];

                settingObj.name = "MenuItem_Settings";

                var settingTexts = settingObj.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                foreach (var st in settingTexts) 
                {
                    if (st.name == "Text" || st.name == "LabelText") st.text = "Setting";
                }

                breedingObj.name = "MenuItem_Breeding";

                var breedingTexts = breedingObj.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                foreach (var bt in breedingTexts) 
                {
                    if (bt.name == "Text" || bt.name == "LabelText") bt.text = "Breeding";
                }

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
                
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(menu);
                    if (menu.gameObject.scene.IsValid())
                    {
                        EditorSceneManager.MarkSceneDirty(menu.gameObject.scene);
                    }
                }

                Debug.Log("[MegaFixer] ALL FIXED! Settings is Settings, Breeding is Breeding.");
            }
        }
    }
}

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using LegendOfBlood;

[InitializeOnLoad]
public static class SetupBreedingFix
{
    static SetupBreedingFix()
    {
        EditorApplication.delayCall += FixNow;
    }

    public static void FixNow()
    {
        // 1. Hook MenuPanel
        var menuPanel = Object.FindAnyObjectByType<MenuPanel>(FindObjectsInactive.Include);
        if (menuPanel != null)
        {
            var breedingBtn = GameObject.Find("MenuItem_Breeding")?.GetComponent<Button>();
            if (breedingBtn != null)
            {
                var so = new SerializedObject(menuPanel);
                so.FindProperty("breedingButton").objectReferenceValue = breedingBtn;
                so.ApplyModifiedProperties();
                Debug.Log("[Fix] Hooked MenuItem_Breeding to MenuPanel");
            }
            else Debug.LogWarning("[Fix] MenuItem_Breeding not found!");
        }

        // 2. Setup EXTRACTED_Breeding_Panel
        var panels = Object.FindObjectsByType<BreedingUIController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in panels)
        {
            Debug.Log($"[Fix] Found BreedingUIController on {p.name}");
            // Auto map buttons if missing
            Button[] buttons = p.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                var txt = b.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (txt != null)
                {
                    if (txt.text.Contains("Lai Tạo", System.StringComparison.OrdinalIgnoreCase) || txt.text.Contains("Breed", System.StringComparison.OrdinalIgnoreCase))
                    {
                        var serializedObject = new SerializedObject(p);
                        serializedObject.FindProperty("breedButton").objectReferenceValue = b;
                        serializedObject.ApplyModifiedProperties();
                    }
                    if (txt.text == "<")
                    {
                        var serializedObject = new SerializedObject(p);
                        serializedObject.FindProperty("closeButton").objectReferenceValue = b;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }
            EditorUtility.SetDirty(p);
        }
        
        Debug.Log("[Fix] DONE!");
    }
}

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using LegendOfBlood;

[InitializeOnLoad]
public static class AutoHookBreedingFixer
{
    static AutoHookBreedingFixer()
    {
        EditorApplication.delayCall += Hook;
    }
    
    static void Hook()
    {
        var menu = Object.FindAnyObjectByType<MenuPanel>(FindObjectsInactive.Include);
        if (menu != null)
        {
            var transforms = menu.GetComponentsInChildren<Transform>(true);
            Button settingBtn = null;
            Button breedingBtn = null;

            foreach (var t in transforms)
            {
                if (t.name == "MenuItem_Settings") settingBtn = t.GetComponent<Button>();
                if (t.name == "MenuItem_Breeding") breedingBtn = t.GetComponent<Button>();
            }
            
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
            Debug.Log("[AutoHookFixer] Fixed Setting and Breeding button mappings!");
        }
    }
}

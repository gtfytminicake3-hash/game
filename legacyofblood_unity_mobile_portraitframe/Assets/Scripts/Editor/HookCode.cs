using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using LegendOfBlood;

public static class HookCode
{
    [MenuItem("Tools/HookBreedingButton")]
    public static void Hook()
    {
        var menu = Object.FindAnyObjectByType<MenuPanel>(FindObjectsInactive.Include);
        if (menu != null)
        {
            var btnGo = GameObject.Find("MenuItem_Breeding");
            if (btnGo == null)
            {
                var transforms = menu.GetComponentsInChildren<Transform>(true);
                foreach (var t in transforms)
                {
                    if (t.name == "MenuItem_Breeding")
                    {
                        btnGo = t.gameObject;
                        break;
                    }
                }
            }
            
            if (btnGo != null)
            {
                var btn = btnGo.GetComponent<Button>();
                var prop = new SerializedObject(menu).FindProperty("breedingButton");
                if (prop != null)
                {
                    prop.objectReferenceValue = btn;
                    prop.serializedObject.ApplyModifiedProperties();
                    Debug.Log("[HookCode] DONE Hooking Breeding Button into MenuPanel!");
                }
            }
        }
    }
}

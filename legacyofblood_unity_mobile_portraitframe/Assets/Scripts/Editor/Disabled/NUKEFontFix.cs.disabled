using UnityEngine;
using UnityEditor;
using TMPro;

[InitializeOnLoad]
public class NUKEFontFix
{
    static NUKEFontFix()
    {
        EditorApplication.delayCall += () => {
            TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
            if (defaultFont == null) return;

            string[] allGuids = AssetDatabase.FindAssets("t:Prefab");
            int countP = 0;
            foreach (var guid in allGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    bool changed = false;
                    TMP_Text[] texts = prefab.GetComponentsInChildren<TMP_Text>(true);
                    foreach (var t in texts)
                    {
                        if (t.font != defaultFont)
                        {
                            t.font = defaultFont;
                            t.fontSharedMaterial = defaultFont.material;
                            changed = true;
                        }
                    }
                    if (changed) 
                    {
                        PrefabUtility.SavePrefabAsset(prefab);
                        countP++;
                    }
                }
            }
            
            var allTexts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int countS = 0;
            foreach (var t in allTexts)
            {
                if (t.font != defaultFont)
                {
                    t.font = defaultFont;
                    t.fontSharedMaterial = defaultFont.material;
                    t.ForceMeshUpdate(true, true);
                    EditorUtility.SetDirty(t);
                    countS++;
                }
            }

            AssetDatabase.SaveAssets();
            if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.Repaint();
            
            Debug.Log($"<color=red><b>[NUKE FONT FIX]</b> B?T BU?C ÉP {countP} Prefabs và {countS} Scene Objects dùng Font '{defaultFont.name}'! KHÔNG CH?A B?T C? AI!</color>");
        };
    }
}

using UnityEngine;
using UnityEditor;
using TMPro;

[InitializeOnLoad]
public class HardRevertFontAuto
{
    static HardRevertFontAuto()
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
                        if (t.font != null && t.font.name.Contains("SystemArial"))
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
                if (t.font != null && t.font.name.Contains("SystemArial"))
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
            
            if (countP > 0 || countS > 0)
                Debug.Log($"<color=green><b>[ÐÃ DÙNG PHÁP B?O]</b> T? d?ng ép {countP} Prefabs và {countS} Scene Objects v? Font '{defaultFont.name}'!</color>");
        };
    }
}

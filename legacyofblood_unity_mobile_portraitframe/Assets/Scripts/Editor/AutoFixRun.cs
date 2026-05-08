using UnityEngine;
using UnityEditor;
using TMPro;

[InitializeOnLoad]
public class AutoFixRun
{
    static AutoFixRun()
    {
        EditorApplication.delayCall += () => {
            TMP_FontAsset fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/SystemArial-Permanent.asset");
            if (fallback != null && fallback.atlasPopulationMode == AtlasPopulationMode.Static) {
                fallback.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                fallback.ClearFontAssetData(true);
                EditorUtility.SetDirty(fallback);
                AssetDatabase.SaveAssets();
                Debug.Log("[URGENT FIX] SystemArial-Permanent is now DYNAMIC and CLEARED!");
            }
        };
    }
}

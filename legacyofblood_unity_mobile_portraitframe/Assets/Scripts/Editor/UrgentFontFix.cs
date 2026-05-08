using UnityEngine;
using UnityEditor;
using TMPro;

public class UrgentFontFix
{
    [MenuItem("Tools/Urgent Font Fix")]
    public static void Fix()
    {
        TMP_FontAsset fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/SystemArial-Permanent.asset");
        if (fallback != null) {
            fallback.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            fallback.ClearFontAssetData(true);
            EditorUtility.SetDirty(fallback);
            AssetDatabase.SaveAssets();
            Debug.Log("[URGENT FIX] SystemArial-Permanent is now DYNAMIC and CLEARED!");
        } else {
            Debug.Log("[URGENT FIX] SystemArial-Permanent NOT FOUND");
        }
    }
}

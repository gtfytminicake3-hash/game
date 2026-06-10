using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class StripCanvasScaler 
{
    [MenuItem("Tools/Fix All Panels Scale")]
    public static void FixPanels() 
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Panel", "Assets/Prefabs" });
        int count = 0;
        foreach (string guid in guids) 
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                var prefabRoot = editingScope.prefabContentsRoot;
                var scaler = prefabRoot.GetComponent<CanvasScaler>();
                if (scaler != null) 
                {
                    Object.DestroyImmediate(scaler, true);
                    count++;
                }
            }
        }
        Debug.Log($"Fixed {count} prefabs by removing CanvasScaler!");
    }
}

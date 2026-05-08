#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class PrefabChecker
{
    [MenuItem("Tools/Check Barrack Prefab")]
    public static void CheckPrefab()
    {
        LegendOfBlood.BarrackPanel panel = Object.FindAnyObjectByType<LegendOfBlood.BarrackPanel>(FindObjectsInactive.Include);
        if (panel == null)
        {
            Debug.LogError("No BarrackPanel found in the scene.");
            return;
        }

        var serializedObject = new SerializedObject(panel);
        var prefabProp = serializedObject.FindProperty("heroCardPrefab");
        if (prefabProp != null && prefabProp.objectReferenceValue != null)
        {
            GameObject prefab = prefabProp.objectReferenceValue as GameObject;
            string path = AssetDatabase.GetAssetPath(prefab);
            Debug.Log($"[CheckPrefab] BarrackPanel is using prefab at path: {path}");
        }
        else
        {
            Debug.LogError("[CheckPrefab] BarrackPanel does not have a heroCardPrefab assigned!");
        }
    }
}
#endif

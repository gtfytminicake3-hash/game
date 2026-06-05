using UnityEngine;
using UnityEditor;

public class InspectPrefab
{
    [MenuItem("Tools/Inspect POI_InfoPanel")]
    public static void Inspect()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/POI_InfoPanel.prefab");
        if (prefab != null)
        {
            Debug.Log($"Root: {prefab.name}, Active: {prefab.activeSelf}");
            foreach (Transform child in prefab.transform)
            {
                Debug.Log($"Child: {child.name}, Active: {child.gameObject.activeSelf}");
                foreach (Transform c2 in child)
                {
                    Debug.Log($"  SubChild: {c2.name}, Active: {c2.gameObject.activeSelf}");
                }
            }
        }
    }
}

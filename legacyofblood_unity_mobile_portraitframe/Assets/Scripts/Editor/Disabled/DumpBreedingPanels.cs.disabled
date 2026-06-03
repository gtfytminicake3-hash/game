using UnityEditor;
using UnityEngine;
using System.Linq;

[InitializeOnLoad]
public static class DumpBreedingPanels
{
    static DumpBreedingPanels()
    {
        EditorApplication.delayCall += Dump;
    }

    static void Dump()
    {
        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var go in allObjects)
        {
            if (go.name.Contains("Breeding", System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"[BreedingDump] FOUND: {go.name} at {GetPath(go)}, Active: {go.activeSelf}");
            }
        }
    }

    static string GetPath(GameObject go)
    {
        string path = go.name;
        Transform parent = go.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
}

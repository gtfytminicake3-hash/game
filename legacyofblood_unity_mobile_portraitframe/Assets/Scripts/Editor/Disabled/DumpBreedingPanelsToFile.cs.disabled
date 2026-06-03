using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;

[InitializeOnLoad]
public static class DumpBreedingPanelsToFile
{
    static DumpBreedingPanelsToFile()
    {
        EditorApplication.delayCall += Dump;
    }

    public static void Dump()
    {
        string pathStr = "";
        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var go in allObjects)
        {
            if (go.name.Contains("Breeding", System.StringComparison.OrdinalIgnoreCase))
            {
                pathStr += $"[BreedingDump] FOUND: {go.name} at {GetPath(go)}, Active: {go.activeSelf}, InstanceID: {go.GetInstanceID()}\n";
            }
        }
        File.WriteAllText("breeding_dump.txt", pathStr);
        Debug.Log("Dumped to breeding_dump.txt");
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

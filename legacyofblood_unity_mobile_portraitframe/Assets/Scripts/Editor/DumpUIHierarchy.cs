using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;

public class DumpUIHierarchy
{
    [MenuItem("Tools/Dump UI Hierarchy")]
    public static void Dump()
    {
        var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        StringBuilder sb = new StringBuilder();
        foreach (var root in rootObjects)
        {
            DumpTransform(root.transform, "", sb);
        }
        File.WriteAllText("ui_hierarchy_dump.txt", sb.ToString());
        Debug.Log("Dumped UI Hierarchy to ui_hierarchy_dump.txt");
    }

    private static void DumpTransform(Transform t, string indent, StringBuilder sb)
    {
        sb.AppendLine($"{indent}- {t.name} (Active: {t.gameObject.activeSelf})");
        foreach (Transform child in t)
        {
            DumpTransform(child, indent + "  ", sb);
        }
    }
}

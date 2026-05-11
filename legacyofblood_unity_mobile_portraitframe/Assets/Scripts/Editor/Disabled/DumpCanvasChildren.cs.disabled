using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.UI;

[InitializeOnLoad]
public static class DumpCanvasChildren
{
    static DumpCanvasChildren()
    {
        EditorApplication.delayCall += Dump;
    }

    public static void Dump()
    {
        string pathStr = "";
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            if (canvas.name == "Canvas" || canvas.name == "Main Canvas" || canvas.name == "Canvas_UI")
            {
                for (int i = 0; i < canvas.transform.childCount; i++)
                {
                    Transform child = canvas.transform.GetChild(i);
                    pathStr += $"[Canvas Child] {child.name} (Active: {child.gameObject.activeSelf})\n";
                }
            }
        }
        File.WriteAllText("canvas_dump.txt", pathStr);
        Debug.Log("Dumped to canvas_dump.txt");
    }
}

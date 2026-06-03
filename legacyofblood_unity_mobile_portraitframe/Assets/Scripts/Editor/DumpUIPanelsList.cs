using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using LegendOfBlood;

public class DumpUIPanelsList
{
    [MenuItem("LegendOfBlood/Fix/Dump UIPanels Info")]
    public static void DumpPanels()
    {
        UIPanel[] allPanels = Object.FindObjectsByType<UIPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Dictionary<UIPanelType, List<UIPanel>> panelGroups = new Dictionary<UIPanelType, List<UIPanel>>();

        foreach (var p in allPanels)
        {
            if (!panelGroups.ContainsKey(p.PanelType))
                panelGroups[p.PanelType] = new List<UIPanel>();
            panelGroups[p.PanelType].Add(p);
        }

        string result = $"--- UI PANEL DUMP ---\nTotal panels: {allPanels.Length}\n\n";
        foreach (var kvp in panelGroups)
        {
            if (kvp.Value.Count > 1)
            {
                result += $"DUPLICATES FOUND FOR {kvp.Key}: {kvp.Value.Count} instances\n";
                foreach (var p in kvp.Value)
                {
                    result += $"  - GameObject: {p.gameObject.name} | Path: {GetGameObjectPath(p.gameObject)}\n";
                }
            }
            else if (kvp.Value.Count == 1)
            {
                result += $"UNIQUE: {kvp.Key} -> {kvp.Value[0].gameObject.name}\n";
            }
        }

        File.WriteAllText(@"d:\game\legendofblood\legacyofblood_unity_mobile_portraitframe\ui_panels_dump_final.txt", result);
    }

    private static string GetGameObjectPath(GameObject obj)
    {
        string path = "/" + obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = "/" + obj.name + path;
        }
        return path;
    }
}

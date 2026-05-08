using UnityEngine;
using UnityEditor;
using LegendOfBlood;

public class DiagSwappedPanels
{
    [MenuItem("Tools/LegendOfBlood/Diag Swapped Panels")]
    public static void RunDiag()
    {
        var allPanels = Resources.FindObjectsOfTypeAll<UIPanel>();
        foreach(var panel in allPanels)
        {
            if (panel.gameObject.scene.name == null) continue; // Skip prefabs
            if (panel.PanelType == UIPanelType.Menu || panel.PanelType == UIPanelType.KingGodPass || panel.GetType().Name.Contains("MenuPanel") || panel.GetType().Name.Contains("KingGodPass"))
            {
                Debug.Log($"[Diag] GameObject: {panel.gameObject.name}, Script: {panel.GetType().Name}, PanelType Enum: {panel.PanelType}");
            }
        }

        var allNavs = Resources.FindObjectsOfTypeAll<UIPanelNavButton>();
        foreach(var nav in allNavs)
        {
            if (nav.gameObject.scene.name == null) continue;
            if (nav.targetPanel == UIPanelType.Menu || nav.targetPanel == UIPanelType.KingGodPass || nav.gameObject.name.Contains("KingGodPass") || nav.gameObject.name.Contains("Thiết Lập"))
            {
                Debug.Log($"[Diag] NavButton on GameObject: {nav.gameObject.name}, TargetPanel: {nav.targetPanel}");
            }
        }
    }
}

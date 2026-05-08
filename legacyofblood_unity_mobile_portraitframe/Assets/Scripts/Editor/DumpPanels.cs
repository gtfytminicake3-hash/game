using UnityEditor;
using UnityEngine;
using LegendOfBlood;

[InitializeOnLoad]
public class DumpPanels {
    static DumpPanels() { EditorApplication.delayCall += Run; }
    
    public static void Run() {
        UIPanel[] allPanels = Resources.FindObjectsOfTypeAll<UIPanel>();
        string s = "All Panels:\n";
        foreach (var p in allPanels) {
            s += p.gameObject.name + " type=" + p.PanelType + " active=" + p.gameObject.activeInHierarchy + "\n";
        }
        Debug.Log(s);
        EditorApplication.delayCall -= Run;
    }
}

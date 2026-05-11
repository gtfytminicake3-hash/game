using UnityEditor;
using UnityEngine;
using LegendOfBlood;

public class DumpArenaShopHierarchy {
    public static void Run() {
        ArenaShopPanel panel = Object.FindObjectOfType<ArenaShopPanel>(true);
        if (panel != null) {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            Dump(panel.transform, 0, sb);
            Debug.Log(sb.ToString());
        } else {
            Debug.Log("ArenaShopPanel not found in scene.");
        }
    }
    
    static void Dump(Transform t, int depth, System.Text.StringBuilder sb) {
        sb.AppendLine(new string('-', depth * 2) + t.name);
        foreach (Transform child in t) {
            Dump(child, depth + 1, sb);
        }
    }
}

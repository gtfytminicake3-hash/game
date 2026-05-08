using UnityEngine;
using UnityEditor;
using System.IO;

public class SyncDump {
    [MenuItem("Tools/Dump Hierarchy")]
    public static void Run() {
        var go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/HeroCard_Prefab.prefab");
        if(go == null) return;
        
        string result = "ROOT: " + go.name + "\n";
        var img = go.GetComponent<UnityEngine.UI.Image>();
        result += "ROOT has Image? " + (img != null) + "\n";
        
        for(int i=0; i<go.transform.childCount; i++) {
            Transform child = go.transform.GetChild(i);
            result += "- CHILD " + i + ": " + child.name + "\n";
            for(int j=0; j<child.childCount; j++) {
                result += "   - " + child.GetChild(j).name + "\n";
            }
        }
        File.WriteAllText("dump.txt", result);
        Debug.Log("Dumped to dump.txt");
    }
}

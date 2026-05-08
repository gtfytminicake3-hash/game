using UnityEngine;
using UnityEditor;

public class TestG
{
    public static void Run()
    {
        string[] guids = AssetDatabase.FindAssets("LiberationSans");
        foreach(var g in guids) {
            Debug.Log(AssetDatabase.GUIDToAssetPath(g));
        }
    }
}

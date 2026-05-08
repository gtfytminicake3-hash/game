using UnityEngine;
using UnityEditor;

public static class FindInactiveObject
{
    [MenuItem("Tools/Find Squad Selection ID")]
    public static void FindSquadID()
    {
        var allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (var t in allTransforms)
        {
            if (t.name == "EXTRACTED_SquadSelection_Panel" && t.root.name == "MainCanvas")
            {
                Debug.Log($"[FOUND_SQUAD] {t.name} ID: {t.gameObject.GetInstanceID()}");
            }
        }
    }
}

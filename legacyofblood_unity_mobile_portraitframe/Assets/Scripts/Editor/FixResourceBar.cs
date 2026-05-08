using UnityEngine;
using UnityEditor;
using TMPro;

public class FixResourceBar
{
    [MenuItem("Tools/Fix Resource Bar Mockups")]
    public static void Fix()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UIResourceBar_Prefab.prefab");
        if (prefab != null)
        {
            var uiBar = prefab.GetComponent<LegendOfBlood.UIResourceBar>();
            if (uiBar != null)
            {
                var texts = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in texts)
                {
                    t.text = "0"; // Replace mockup 9999999 with 0
                    if (t.transform.parent.name.Contains("Gold")) uiBar.GetType().GetField("goldText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(uiBar, t);
                    if (t.transform.parent.name.Contains("Wood")) uiBar.GetType().GetField("woodText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(uiBar, t);
                    if (t.transform.parent.name.Contains("Stone")) uiBar.GetType().GetField("stoneText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(uiBar, t);
                    if (t.transform.parent.name.Contains("Diamond") || t.transform.parent.name.Contains("Crystal")) uiBar.GetType().GetField("diamondText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(uiBar, t);
                }
                EditorUtility.SetDirty(prefab);
                PrefabUtility.SavePrefabAsset(prefab);
                Debug.Log("Fixed UIResourceBar_Prefab mockups!");
            }
        }
        
        // Fix in active scene as well
        var bars = Object.FindObjectsByType<LegendOfBlood.UIResourceBar>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var uiBar in bars)
        {
            var texts = uiBar.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in texts)
            {
                if (t.text == "9999" || t.text == "9999999" || t.text.Contains("99")) t.text = "0";
                
                if (t.transform.parent.name.Contains("Gold")) uiBar.GetType().GetField("goldText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(uiBar, t);
                if (t.transform.parent.name.Contains("Wood")) uiBar.GetType().GetField("woodText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(uiBar, t);
                if (t.transform.parent.name.Contains("Stone")) uiBar.GetType().GetField("stoneText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(uiBar, t);
                if (t.transform.parent.name.Contains("Diamond") || t.transform.parent.name.Contains("Crystal")) uiBar.GetType().GetField("diamondText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(uiBar, t);
            }
            EditorUtility.SetDirty(uiBar);
        }
        
        Debug.Log("Finished fixing resource bar mockups in scene.");
    }
}

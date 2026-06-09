using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class DebugRunnerTest
{
    [Test]
    public void TestHierarchy()
    {
        string path = "Assets/Prefabs/CombatHeroCard_Prefab.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError($"Prefab not found at {path}");
            return;
        }

        Debug.Log($"--- Hierarchy of {prefab.name} ---");
        PrintHierarchy(prefab.transform, 0);
    }

    private void PrintHierarchy(Transform t, int depth)
    {
        string indent = new string('-', depth * 2);
        string typeInfo = "Transform";
        string colorInfo = "";
        
        var img = t.GetComponent<Image>();
        if (img != null) {
            typeInfo = "Image";
            colorInfo = $", Color: {img.color}";
        }
        var txt = t.GetComponent<TextMeshProUGUI>();
        if (txt != null) {
            typeInfo = "TextMeshProUGUI";
            colorInfo = $", Text: {txt.text}";
        }
        var cg = t.GetComponent<CanvasGroup>();
        if (cg != null) {
            typeInfo += " + CanvasGroup (Alpha: " + cg.alpha + ")";
        }

        Debug.Log($"{indent} {t.name} [{typeInfo}] (Active: {t.gameObject.activeSelf}){colorInfo}");

        foreach (Transform child in t)
        {
            PrintHierarchy(child, depth + 1);
        }
    }
}

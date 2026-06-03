using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using LegendOfBlood;

public static class HeroCardReferenceAutoFixer
{
    [MenuItem("Tools/HeroCard/Auto Fix Missing References")]
    public static void Run()
    {
        int fixedCount = 0;
        int scannedCount = 0;

        var scene = SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var card in root.GetComponentsInChildren<HeroCard>(true))
            {
                scannedCount++;
                if (FixOne(card)) fixedCount++;
            }
        }

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (var guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            bool changed = false;
            foreach (var card in prefab.GetComponentsInChildren<HeroCard>(true))
            {
                scannedCount++;
                if (FixOne(card)) changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(prefab);
                fixedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"[HeroCardReferenceAutoFixer] Scanned={scannedCount}, Fixed={fixedCount}");
    }

    private static bool FixOne(HeroCard card)
    {
        var so = new SerializedObject(card);
        bool changed = false;

        changed |= AssignTMPIfMissing(so, card.transform, "combatPowerText", "combat", "cp");
        changed |= AssignImageIfMissing(so, card.transform, "professionIcon", "profession", "class");
        changed |= AssignButtonIfMissing(so, card.transform, "cardButton");

        if (changed)
        {
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(card);
        }

        return changed;
    }

    private static bool AssignTMPIfMissing(SerializedObject so, Transform root, string field, params string[] keys)
    {
        var p = so.FindProperty(field);
        if (p == null || p.objectReferenceValue != null) return false;
        foreach (var t in root.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            var n = t.name.ToLowerInvariant();
            foreach (var k in keys)
            {
                if (n.Contains(k)) { p.objectReferenceValue = t; return true; }
            }
        }
        return false;
    }

    private static bool AssignImageIfMissing(SerializedObject so, Transform root, string field, params string[] keys)
    {
        var p = so.FindProperty(field);
        if (p == null || p.objectReferenceValue != null) return false;
        foreach (var i in root.GetComponentsInChildren<Image>(true))
        {
            var n = i.name.ToLowerInvariant();
            foreach (var k in keys)
            {
                if (n.Contains(k)) { p.objectReferenceValue = i; return true; }
            }
        }
        return false;
    }

    private static bool AssignButtonIfMissing(SerializedObject so, Transform root, string field)
    {
        var p = so.FindProperty(field);
        if (p == null || p.objectReferenceValue != null) return false;
        var b = root.GetComponent<Button>() ?? root.GetComponentInChildren<Button>(true);
        if (b == null) return false;
        p.objectReferenceValue = b;
        return true;
    }
}

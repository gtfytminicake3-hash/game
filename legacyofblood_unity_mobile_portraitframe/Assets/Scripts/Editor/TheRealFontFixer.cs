using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

public class TheRealFontFixer
{
    [MenuItem("UI Tools/THE REAL FONT FIX")]
    public static void Run()
    {
        Font fontTTF = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/SystemArial_Pure.ttf");
        if (fontTTF == null) {
            Debug.LogError("C?N SYSTEM ARIAL TTF");
            return;
        }

        string assetPath = "Assets/Resources/SystemArial-Working.asset";
        TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
        if (existing != null) {
            AssetDatabase.DeleteAsset(assetPath);
        }

        // T?o c?c b?
        TMP_FontAsset newFont = TMP_FontAsset.CreateFontAsset(fontTTF, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic);
        
        List<uint> charList = new List<uint>();
        for (uint i = 32; i <= 126; i++) charList.Add(i);
        newFont.TryAddCharacters(charList.ToArray());
        
        newFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;

        // BÍ QUY?T LÀ ÐÂY: Luu t?t c? thành ph?n con!
        AssetDatabase.CreateAsset(newFont, assetPath);
        
        if (newFont.atlasTexture != null) {
            newFont.atlasTexture.name = "Atlas";
            AssetDatabase.AddObjectToAsset(newFont.atlasTexture, newFont);
        }
        if (newFont.material != null) {
            newFont.material.name = "Material";
            AssetDatabase.AddObjectToAsset(newFont.material, newFont);
        }

        EditorUtility.SetDirty(newFont);
        AssetDatabase.SaveAssets();

        // Bây gi? ép nó làm Default!
        TMP_Settings.defaultFontAsset = newFont;
        EditorUtility.SetDirty(TMP_Settings.instance);

        // Thay vào t?t c? Prefabs và Objects
        string[] allGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (var guid in allGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                bool changed = false;
                TMP_Text[] texts = prefab.GetComponentsInChildren<TMP_Text>(true);
                foreach (var t in texts)
                {
                    if (t.font != newFont)
                    {
                        t.font = newFont;
                        t.fontSharedMaterial = newFont.material;
                        changed = true;
                    }
                }
                if (changed) PrefabUtility.SavePrefabAsset(prefab);
            }
        }
        
        var allTexts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in allTexts)
        {
            if (t.font != newFont)
            {
                t.font = newFont;
                t.fontSharedMaterial = newFont.material;
                t.ForceMeshUpdate(true, true);
                EditorUtility.SetDirty(t);
            }
        }

        AssetDatabase.SaveAssets();
        if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.Repaint();
        
        Debug.Log($"<color=cyan><b>[THE REAL FIX]</b> Ðã t?o SystemArial-Working có d? Texture+Material Sub-assets và g?n làm Default! Có F, W, Z!</color>");
    }
}

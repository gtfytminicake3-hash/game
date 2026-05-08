using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

[InitializeOnLoad]
public class TheRealFontFixerAuto
{
    static TheRealFontFixerAuto()
    {
        EditorApplication.delayCall += () => {
            string doneFlag = "Assets/Resources/FontFixDone.txt";
            if (System.IO.File.Exists(doneFlag)) return;

            Font fontTTF = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/SystemArial_Pure.ttf");
            if (fontTTF == null) return;

            string assetPath = "Assets/Resources/SystemArial-Working2.asset";
            TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing != null) AssetDatabase.DeleteAsset(assetPath);

            TMP_FontAsset newFont = TMP_FontAsset.CreateFontAsset(fontTTF, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 4096, 4096, AtlasPopulationMode.Dynamic);
            
            List<uint> charList = new List<uint>();
            for (uint i = 32; i <= 126; i++) charList.Add(i);
            newFont.TryAddCharacters(charList.ToArray());
            
            newFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;

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

            TMP_Settings.defaultFontAsset = newFont;
            EditorUtility.SetDirty(TMP_Settings.instance);

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
            System.IO.File.WriteAllText(doneFlag, "DONE");
            if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.Repaint();
            
            Debug.Log($"<color=cyan><b>[THE REAL FIX]</b> Ðã t?o SystemArial-Working2 có d? Texture+Material Sub-assets và g?n làm Default! CÓ F, W, Z!</color>");
        };
    }
}

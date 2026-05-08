using UnityEngine;
using UnityEditor;

public class HardcoreHeroInfoFix {
    [MenuItem("Tools/Hardcore HeroInfo Fix")]
    public static void Run() {
        bool changed = false;
        
        // 1. Quét t?m t?t c? cc panel trong Scene v c?p nh?t ngy
        var panels = Object.FindObjectsByType<LegendOfBlood.HeroInfoPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach(var p in panels) {
            var cv = p.GetComponent<Canvas>();
            if(!cv) {
                cv = p.gameObject.AddComponent<Canvas>();
                p.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            if(!cv.overrideSorting || cv.sortingOrder < 100) {
                cv.overrideSorting = true;
                cv.sortingOrder = 100;
                EditorUtility.SetDirty(p);
                changed = true;
            }
        }
        
        if(changed && !Application.isPlaying) {
            var s = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(s);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(s);
        }

        // 2. Ch?nh m?nh tay vo file Prefab g?c d? sau d thnh vinh vi?n
        // T?m Prefab trn dia
        string[] guids = AssetDatabase.FindAssets("t:Prefab HeroInfo");
        foreach(string guid in guids) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(go != null && go.GetComponent<LegendOfBlood.HeroInfoPanel>() != null) {
                GameObject inst = PrefabUtility.InstantiatePrefab(go) as GameObject;
                var cv = inst.GetComponent<Canvas>();
                if(!cv) {
                    cv = inst.gameObject.AddComponent<Canvas>();
                    inst.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                }
                cv.overrideSorting = true;
                cv.sortingOrder = 100;
                
                PrefabUtility.SaveAsPrefabAsset(inst, path);
                Object.DestroyImmediate(inst);
                Debug.Log("[HARDCORE FIX] Patched Prefab at: " + path);
            }
        }

        Debug.Log("[HARDCORE FIX] Finished forcefully setting Canvas layering to 100.");
    }
}

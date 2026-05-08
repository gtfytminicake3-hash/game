using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using LegendOfBlood;

public class DeepSurg {
    [MenuItem("Tools/Deep Surg")]
    public static void Run() {
        bool changed = false;

        var allBtns = Object.FindObjectsByType<UnityEngine.UI.Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach(var btn in allBtns) {
            string n = btn.name.ToLower();
            if (n.Contains("menu") && !n.Contains("sub") && !n.Contains("item")) {
                var nav = btn.GetComponent<LegendOfBlood.UIPanelNavButton>();
                if(nav != null && nav.targetPanel != UIPanelType.Menu) {
                    nav.targetPanel = UIPanelType.Menu;
                    EditorUtility.SetDirty(nav);
                    changed = true;
                    Debug.Log($"Fixed Button {btn.name} to Menu Enum.");
                }
            }
        }

        var bp = Object.FindAnyObjectByType<LegendOfBlood.BarrackPanel>(FindObjectsInactive.Include);
        if(bp) {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/HeroCard_SquadSelection_Prefab.prefab");
            if (go) {
                var so = new SerializedObject(bp);
                so.FindProperty("heroCardPrefab").objectReferenceValue = go;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(bp);
                changed = true;
                Debug.Log("Fixed BarrackPanel Reference.");
            }
        }

        if(changed) {
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("DEEP SURG SAVED!");
        } else {
            Debug.Log("DEEP SURG NO CHANGES NEEDED.");
        }
    }
}

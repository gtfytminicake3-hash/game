using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using LegendOfBlood;
using LegendOfBlood.UI;

public class SurgicalFix {
    [MenuItem("Tools/Surgical Fix")]
    public static void Run() {
        bool changed = false;

        var allMenuPanels = Object.FindObjectsByType<MenuPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach(var mp in allMenuPanels) {
            var kgpass = mp.GetComponent<KingGodPassPanel>();
            if(kgpass != null) {
                Debug.Log("REMOVED rogue KingGodPassPanel from " + mp.gameObject.name);
                Object.DestroyImmediate(kgpass, true);
                changed = true;
            }
            if (mp.PanelType != UIPanelType.Menu) {
                mp.PanelType = UIPanelType.Menu;
                EditorUtility.SetDirty(mp);
                changed = true;
            }
        }

        var allBtns = Object.FindObjectsByType<UnityEngine.UI.Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach(var btn in allBtns) {
            string n = btn.name.ToLower();
            if (n.Contains("menu") && !n.Contains("sub") && !n.Contains("item")) {
                var nav = btn.GetComponent<UIPanelNavButton>();
                if(nav != null && nav.targetPanel == UIPanelType.KingGodPass) {
                    nav.targetPanel = UIPanelType.Menu;
                    Debug.Log("RESTORED " + btn.name + " target panel to Menu");
                    EditorUtility.SetDirty(nav);
                    changed = true;
                }
            }
        }

        if(changed) {
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Surgical Fix Applied & Scene Saved!");
        } else {
            Debug.Log("No surgical changes needed for Menu.");
        }
    }
}

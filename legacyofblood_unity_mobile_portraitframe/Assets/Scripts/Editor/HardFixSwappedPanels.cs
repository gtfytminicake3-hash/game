using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using LegendOfBlood;
using LegendOfBlood.UI;

public class HardFixSwappedPanels
{
    [MenuItem("Tools/LegendOfBlood/HardFix Swapped Panels")]
    public static void RunFix()
    {
        bool changed = false;

        // Fix MenuPanel Scripts
        var allMenuPanels = Resources.FindObjectsOfTypeAll<MenuPanel>();
        foreach(var mp in allMenuPanels)
        {
            if (mp.gameObject.scene.name == null) continue; // Skip prefabs if any
            if (mp.PanelType != UIPanelType.Menu)
            {
                Undo.RecordObject(mp, "Fix MenuPanel type");
                mp.PanelType = UIPanelType.Menu;
                Debug.Log($"[HardFix] Reset PanelType of GameObject '{mp.gameObject.name}' (MenuPanel script) to Menu");
                changed = true;
            }
        }

        // Fix KingGodPassPanel Scripts
        var allKgPanels = Resources.FindObjectsOfTypeAll<KingGodPassPanel>();
        foreach(var kp in allKgPanels)
        {
            if (kp.gameObject.scene.name == null) continue;
            if (kp.PanelType != UIPanelType.KingGodPass)
            {
                Undo.RecordObject(kp, "Fix KingGodPassPanel type");
                kp.PanelType = UIPanelType.KingGodPass;
                Debug.Log($"[HardFix] Reset PanelType of GameObject '{kp.gameObject.name}' (KingGodPassPanel script) to KingGodPass");
                changed = true;
            }
        }

        // Fix Nav Buttons
        var allNavs = Resources.FindObjectsOfTypeAll<UIPanelNavButton>();
        foreach(var nav in allNavs)
        {
            if (nav.gameObject.scene.name == null) continue;
            if (nav.gameObject.name.Contains("Thiết Lập") && nav.targetPanel != UIPanelType.Menu)
            {
                Undo.RecordObject(nav, "Fix Nav Button");
                nav.targetPanel = UIPanelType.Menu;
                Debug.Log($"[HardFix] Reset Nav Button '{nav.gameObject.name}' to point to Menu");
                changed = true;
            }
            if (nav.gameObject.name == "KingGodPass" && nav.targetPanel != UIPanelType.KingGodPass)
            {
                Undo.RecordObject(nav, "Fix Nav Button");
                nav.targetPanel = UIPanelType.KingGodPass;
                Debug.Log($"[HardFix] Reset Nav Button '{nav.gameObject.name}' to point to KingGodPass");
                changed = true;
            }
        }

        if (changed)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[HardFix] Done and marked scene dirty. Vui lòng Save Scene!");
        }
        else
        {
            Debug.Log("[HardFix] Everything is already correct.");
        }
    }
}

using UnityEditor;
using UnityEngine;

public class StretchPanelsFix
{
    [MenuItem("UI Tools/Fix Stretch On All Panels")]
    public static void Fix()
    {
        Canvas mainCanvas = Object.FindFirstObjectByType<Canvas>();
        if (mainCanvas == null) return;
        if (mainCanvas.name != "MainCanvas") {
            GameObject mc = GameObject.Find("MainCanvas");
            if (mc != null) mainCanvas = mc.GetComponent<Canvas>();
        }

        if (mainCanvas != null)
        {
            foreach (Transform panel in mainCanvas.transform)
            {
                RectTransform rt = panel.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                    EditorUtility.SetDirty(panel.gameObject);
                }

                // FIX HIERARCHY: Move Background out of SafeArea
                Transform safeArea = panel.Find("SafeArea");
                if (safeArea != null)
                {
                    Transform bg = safeArea.Find("Background");
                    if (bg != null)
                    {
                        bg.SetParent(panel, false);
                        bg.SetSiblingIndex(0); // Move to top so it renders behind everything
                        EditorUtility.SetDirty(panel.gameObject);
                    }
                    Transform outerFrame = safeArea.Find("OuterFrame");
                    if (outerFrame != null)
                    {
                        outerFrame.SetParent(panel, false);
                        outerFrame.SetSiblingIndex(1);
                        EditorUtility.SetDirty(panel.gameObject);
                    }
                    Transform bgScene = safeArea.Find("BackgroundScene");
                    if (bgScene != null)
                    {
                        bgScene.SetParent(panel, false);
                        bgScene.SetSiblingIndex(2);
                        EditorUtility.SetDirty(panel.gameObject);
                    }
                }
            }
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mainCanvas.gameObject.scene);
            Debug.Log("Fixed stretch and hierarchy on all panels inside MainCanvas!");
        }
    }
}

using UnityEngine;
using UnityEditor;

namespace LegendOfBlood.EditorMode
{
    public class ForcePrefabSetup
    {
        [MenuItem("LegendOfBlood/Map UI/FORCE FIX PREFAB UI")]
        public static void Fix()
        {
            string prefabPath = "Assets/Prefabs/POI_InfoPanel.prefab";
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError("Error loading " + prefabPath);
                return;
            }

            POI_InfoPanel poiPanel = prefabRoot.GetComponent<POI_InfoPanel>();
            if (poiPanel == null) return;

            // Delete old ones to start fresh
            Transform old1 = prefabRoot.transform.Find("DifficultySelectionPopup");
            if (old1 != null) Object.DestroyImmediate(old1.gameObject);
            Transform old2 = prefabRoot.transform.Find("NodeDetailPopup");
            if (old2 != null) Object.DestroyImmediate(old2.gameObject);

            // 1. Difficulty
            GameObject diffObj = new GameObject("DifficultySelectionPopup");
            diffObj.transform.SetParent(prefabRoot.transform, false);
            diffObj.transform.SetAsLastSibling();
            RectTransform diffRt = diffObj.AddComponent<RectTransform>();
            diffRt.anchorMin = Vector2.zero; diffRt.anchorMax = Vector2.one;
            diffRt.sizeDelta = Vector2.zero; diffRt.anchoredPosition = Vector2.zero;
            UnityEngine.UI.Image diffBg = diffObj.AddComponent<UnityEngine.UI.Image>();
            diffBg.color = new Color(0, 0, 0, 0.95f);
            
            // diff buttons setup... I will just copy the components properly
            // BUT, faster way: Instantiate from the one in Scene and set them as children
            POI_InfoPanel scenePnl = GameObject.FindFirstObjectByType<POI_InfoPanel>(FindObjectsInactive.Include);
            if (scenePnl != null)
            {
                Transform diffScene = scenePnl.transform.Find("DifficultySelectionPopup");
                Transform detailScene = scenePnl.transform.Find("NodeDetailPopup");
                if (diffScene != null) 
                {
                    GameObject clonedDiff = Object.Instantiate(diffScene.gameObject, prefabRoot.transform, false);
                    clonedDiff.name = "DifficultySelectionPopup";
                    poiPanel.GetType().GetField("difficultyPopup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .SetValue(poiPanel, clonedDiff.GetComponent<DifficultySelectionPopup>());
                }
                if (detailScene != null) 
                {
                    GameObject clonedDet = Object.Instantiate(detailScene.gameObject, prefabRoot.transform, false);
                    clonedDet.name = "NodeDetailPopup";
                    poiPanel.GetType().GetField("nodeDetailPopup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .SetValue(poiPanel, clonedDet.GetComponent<NodeDetailPopup>());
                }
            }
            
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            Debug.Log(">>> Đã bọc 2 Popup gắn vào đúng file Prefab gốc!");
        }
    }
}

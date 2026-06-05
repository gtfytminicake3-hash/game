using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SetupUITestEnvironment
{
    [MenuItem("Tools/Setup UI Test Environment")]
    public static void SetupUI()
    {
        // 1. Create Canvas
        GameObject canvasGO = new GameObject("Main Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920); // Portrait frame
        
        canvasGO.AddComponent<GraphicRaycaster>();
        Undo.RegisterCreatedObjectUndo(canvasGO, "Create Canvas");

        // 2. Create EventSystem
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(eventSystemGO, "Create EventSystem");
        }

        // 3. Find and destroy old dangling Panel_MainScreen from root if any
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var rootObj in rootObjects)
        {
            if (rootObj.name.Contains("Panel_MainScreen") && rootObj.GetComponent<Canvas>() == null)
            {
                Object.DestroyImmediate(rootObj);
            }
        }

        // 4. Instantiate Panel
        string prefabPath = "Assets/Prefabs/Panel/Panel_MainScreen.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null)
        {
            GameObject panelGO = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            panelGO.transform.SetParent(canvasGO.transform, false);
            
            // Ensure full stretch
            RectTransform rect = panelGO.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.localScale = Vector3.one;
            }
            Undo.RegisterCreatedObjectUndo(panelGO, "Create Panel");
            
            Selection.activeGameObject = panelGO;
        }
        else
        {
            Debug.LogError("Could not find Panel prefab at: " + prefabPath);
        }
    }
}

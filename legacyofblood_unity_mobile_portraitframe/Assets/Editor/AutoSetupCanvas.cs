using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

public class AutoSetupCanvas
{
    [MenuItem("Tools/Auto Setup Canvas And Prefab")]
    public static void ExecuteSetup()
    {
        // 1. Open GameClient scene
        string scenePath = "Assets/Scenes/GameClient.unity";
        if (File.Exists(scenePath))
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }
        else
        {
            // Try to find the scene
            string[] guids = AssetDatabase.FindAssets("GameClient t:Scene");
            if (guids.Length > 0)
            {
                scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }
            else
            {
                Debug.LogWarning("Could not find GameClient scene, executing in current scene.");
            }
        }

        // 2. Create MainCanvas
        GameObject canvasGO = GameObject.Find("MainCanvas");
        if (canvasGO != null)
        {
            Object.DestroyImmediate(canvasGO);
        }
        
        canvasGO = new GameObject("MainCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Ensure EventSystem
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
        }

        // Tự động tìm tất cả Prefab trong thư mục Assets/Prefabs/Panel/
        string[] panelGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Panel" });
        
        System.Collections.Generic.List<string> allPaths = new System.Collections.Generic.List<string>();
        foreach (string guid in panelGuids)
        {
            allPaths.Add(AssetDatabase.GUIDToAssetPath(guid));
        }

        // Add the explicitly requested extra panels that are directly in Prefabs folder
        allPaths.Add("Assets/Prefabs/POI_InfoPanel.prefab");
        allPaths.Add("Assets/Prefabs/TutorialPanel.prefab");

        foreach (string path in allPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.SetParent(canvasGO.transform, false);
                
                RectTransform rect = instance.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    rect.localScale = Vector3.one;
                }
                
                // Optional: Deactivate all panels except the main one to avoid chaos
                instance.SetActive(false);
            }
        }

        // 4. Save MainCanvas as Prefab
        string prefabSavePath = "Assets/Prefabs/MainCanvas.prefab";
        PrefabUtility.SaveAsPrefabAssetAndConnect(canvasGO, prefabSavePath, InteractionMode.AutomatedAction);
        
        Debug.Log("MainCanvas setup complete and saved as Prefab!");
    }
}

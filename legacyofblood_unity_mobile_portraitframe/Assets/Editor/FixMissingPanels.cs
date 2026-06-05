using UnityEngine;
using UnityEditor;

public class FixMissingPanels
{
    [MenuItem("Tools/Fix Missing Panels")]
    public static void ExecuteFix()
    {
        GameObject canvasGO = GameObject.Find("MainCanvas");
        if (canvasGO == null)
        {
            Debug.LogError("MainCanvas not found!");
            return;
        }

        string[] extraPrefabs = new string[] 
        {
            "Assets/Prefabs/POI_InfoPanel.prefab",
            "Assets/Prefabs/TutorialPanel.prefab"
        };

        foreach (string path in extraPrefabs)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                // Check if already exists
                bool exists = false;
                foreach (Transform child in canvasGO.transform)
                {
                    if (child.name.Contains(prefab.name))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
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
                    instance.SetActive(false);
                    Debug.Log($"Successfully added {prefab.name} to MainCanvas");
                }
            }
        }
        
        // Save prefab
        string prefabSavePath = "Assets/Prefabs/MainCanvas.prefab";
        PrefabUtility.SaveAsPrefabAssetAndConnect(canvasGO, prefabSavePath, InteractionMode.AutomatedAction);
        Debug.Log("MainCanvas updated and saved.");
    }
}

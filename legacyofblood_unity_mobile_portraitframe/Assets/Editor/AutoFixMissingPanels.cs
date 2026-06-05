using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class AutoFixMissingPanels
{
    static AutoFixMissingPanels()
    {
        EditorApplication.delayCall += RunFix;
    }

    private static void RunFix()
    {
        if (SessionState.GetBool("AutoFixMissingPanelsRun", false))
            return;
        
        SessionState.SetBool("AutoFixMissingPanelsRun", true);

        GameObject canvasGO = GameObject.Find("MainCanvas");
        if (canvasGO == null) return;

        string[] extraPrefabs = new string[] 
        {
            "Assets/Prefabs/POI_InfoPanel.prefab",
            "Assets/Prefabs/TutorialPanel.prefab"
        };

        bool changed = false;
        foreach (string path in extraPrefabs)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
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
                    instance.SetActive(false);
                    changed = true;
                    Debug.Log($"[AutoFix] Added {prefab.name} to MainCanvas");
                }
            }
        }
        
        if (changed)
        {
            PrefabUtility.SaveAsPrefabAssetAndConnect(canvasGO, "Assets/Prefabs/MainCanvas.prefab", InteractionMode.AutomatedAction);
            Debug.Log("[AutoFix] MainCanvas updated and saved.");
        }
    }
}

using UnityEditor;
using UnityEngine;

public class OpenHeroCardUI
{
    [MenuItem("Tools/UI Editor/1. Mở thẳng Hero Card Prefab")]
    public static void OpenHeroCard()
    {
        string path = "Assets/Prefabs/HeroCard_Prefab.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab != null)
        {
            AssetDatabase.OpenAsset(prefab);
            Debug.Log("Đã mở Prefab Hero Card!");
        }
        else
        {
            Debug.LogError("Không tìm thấy: " + path);
        }
    }

    [MenuItem("Tools/UI Editor/2. Mở thẳng Breeding Panel Prefab")]
    public static void OpenBreedingPanel()
    {
        string path = "Assets/Prefabs/Panel/Panel_Breeding.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab != null)
        {
            AssetDatabase.OpenAsset(prefab);
            Debug.Log("Đã mở Prefab Breeding Panel!");
        }
        else
        {
            Debug.LogError("Không tìm thấy: " + path);
        }
    }

    [MenuItem("Tools/UI Editor/3. ÉP MỞ (Kéo ra màn hình Scene)")]
    public static void InstantiateInScene()
    {
        // Tạo Canvas nếu chưa có
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Temp Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        }

        string path1 = "Assets/Prefabs/HeroCard_Prefab.prefab";
        GameObject prefab1 = AssetDatabase.LoadAssetAtPath<GameObject>(path1);
        if (prefab1 != null)
        {
            GameObject instance1 = (GameObject)PrefabUtility.InstantiatePrefab(prefab1, canvas.transform);
            instance1.transform.localPosition = Vector3.zero;
            Selection.activeGameObject = instance1;
            Debug.Log("Đã ép kéo Hero Card ra màn hình Scene để bạn sửa!");
        }
    }
}

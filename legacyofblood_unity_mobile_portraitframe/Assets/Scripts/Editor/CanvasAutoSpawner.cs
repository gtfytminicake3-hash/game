using UnityEngine;
using UnityEditor;

public class CanvasAutoSpawner : EditorWindow
{
    [MenuItem("Tools/Tự động kéo Panel vào Scene")]
    public static void AutoSpawnPanels()
    {
        // 1. Tìm hoặc tạo Canvas trong Scene
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // Tìm hoặc tạo EventSystem
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }
        }

        // Danh sách các Panel cần kéo vào
        string[] panelNames = new string[]
        {
            "Panel_Inventory",
            "Panel_Quest",
            "Panel_Recruitment",
            "Panel_ProfessionSelection",
            "Panel_ArenaShop",
            "Panel_EquipmentDetail",
            "Panel_BossBattle",
            "Panel_Tower",
            "Panel_PopulationManager",
            "Panel_Menu"
        };

        int count = 0;
        foreach (string panelName in panelNames)
        {
            // Tìm prefab bằng tên
            string[] guids = AssetDatabase.FindAssets(panelName + " t:Prefab");
            if (guids.Length == 0)
            {
                Debug.LogWarning($"<color=yellow>Không tìm thấy Prefab: {panelName}</color>");
                continue;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            // Sinh ra trong Canvas
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform);
            instance.name = panelName; // Xóa chữ "(Clone)" mặc định của Unity

            // Sắp xếp tự động kéo dãn (Stretch to Parent) cho thiết bị di động
            RectTransform rect = instance.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;   // Bottom Left
                rect.anchorMax = Vector2.one;    // Top Right
                rect.offsetMin = Vector2.zero;   // Left, Bottom padding = 0
                rect.offsetMax = Vector2.zero;   // Right, Top padding = 0
                rect.localScale = Vector3.one;
            }

            // Tắt Panel đi để tránh che mất màn hình chính (Chỉ chừa lại cái cần thiết khi dev mở lên bật)
            instance.SetActive(false);
            count++;
        }

        Debug.Log($"<color=green>Hoàn tất: Đã tự động kéo {count} Panel vào Canvas thành công!</color>");
    }
}

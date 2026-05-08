using UnityEngine;
using UnityEditor;

namespace LegendOfBlood.EditorMode
{
    public class ForceShowPopups
    {
        [MenuItem("LegendOfBlood/Map UI/Show Popups In Scene")]
        public static void ShowThem()
        {
            POI_InfoPanel poiPanel = GameObject.FindFirstObjectByType<POI_InfoPanel>(FindObjectsInactive.Include);
            if (poiPanel == null)
            {
                Debug.LogError("Không có POI_InfoPanel trong scene!");
                return;
            }

            // Mở rộng POI_InfoPanel trong Hierarchy để người dùng thấy các con của nó
            EditorGUIUtility.PingObject(poiPanel.gameObject);
            Selection.activeGameObject = poiPanel.gameObject;

            // Tìm và bật 2 pop up lên
            Transform diff = poiPanel.transform.Find("DifficultySelectionPopup");
            Transform detail = poiPanel.transform.Find("NodeDetailPopup");

            if (diff != null) 
            {
                diff.gameObject.SetActive(true);
                Debug.Log("Đã bật DifficultySelectionPopup!");
                Selection.activeGameObject = diff.gameObject; // Focus vào nó luôn
            }
            else 
            {
                Debug.LogWarning("Chưa có DifficultyPopup trong Scene POI_InfoPanel! Xin hãy chạy Auto Setup Popups lại 1 lần nữa trong Scene này.");
            }

            if (detail != null)
            {
                detail.gameObject.SetActive(true);
                Debug.Log("Đã bật NodeDetailPopup!");
            }
        }
    }
}

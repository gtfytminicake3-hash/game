using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using LegendOfBlood;
using TMPro;

public class BreedingSetupHelper : EditorWindow
{
    [MenuItem("Tools/SỬA LỖI GIAO DIỆN LAI TẠO NÀY (Bấm vào đây!)")]
    public static void ShowWindow()
    {
        BreedingSetupHelper window = GetWindow<BreedingSetupHelper>("Sửa Lỗi Lai Tạo");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }

    private GameObject theNewBreedingPanel;

    private void OnGUI()
    {
        GUILayout.Label("CÔNG CỤ TỰ ĐỘNG GẮN GIAO DIỆN", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("1. Kéo Giao Diện Lai Tạo bạn vừa thiết kế vào ô dưới đây:");
        theNewBreedingPanel = (GameObject)EditorGUILayout.ObjectField("Giao diện của bạn", theNewBreedingPanel, typeof(GameObject), true);
        
        GUILayout.Space(20);
        if (GUILayout.Button("2. TỰ ĐỘNG SỬA & GẮN NÚT VÀO MENU!", GUILayout.Height(50)))
        {
            DoTheMagicFix();
        }
    }

    private void DoTheMagicFix()
    {
        if (theNewBreedingPanel == null)
        {
            EditorUtility.DisplayDialog("Thiếu Panel", "Bạn chưa kéo Panel Lai Tạo vào kìa!", "OK");
            return;
        }

        // 1. Xoá mọi cái panel rác bị trùng (EXTRACTED)
        var oldPanels = Object.FindObjectsByType<BreedingUIController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in oldPanels)
        {
            if (p.gameObject != theNewBreedingPanel)
            {
                Debug.Log($"Xóa panel cũ/rác: {p.gameObject.name}");
                DestroyImmediate(p.gameObject);
            }
        }

        // 2. Gắn BreedingUIController nếu chưa có
        BreedingUIController controller = theNewBreedingPanel.GetComponent<BreedingUIController>();
        if (controller == null)
        {
            controller = theNewBreedingPanel.AddComponent<BreedingUIController>();
            Debug.Log($"Đã gắn BreedingUIController vào {theNewBreedingPanel.name}");
        }

        // 3. Tự động mò các nút
        Button[] buttons = theNewBreedingPanel.GetComponentsInChildren<Button>(true);
        foreach (Button b in buttons)
        {
            TextMeshProUGUI txt = b.GetComponentInChildren<TextMeshProUGUI>(true);
            if (txt != null)
            {
                if (txt.text.ToUpper().Contains("LAI")) controller.GetType().GetField("breedButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(controller, b);
                if (txt.text.Contains("<")) controller.GetType().GetField("closeButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(controller, b);
            }
        }

        EditorUtility.SetDirty(controller);

        // 4. Hook Menu Panel
        MenuPanel menu = Object.FindAnyObjectByType<MenuPanel>(FindObjectsInactive.Include);
        if (menu != null)
        {
            GameObject btnMenu = GameObject.Find("MenuItem_Breeding");
            if (btnMenu != null)
            {
                var so = new SerializedObject(menu);
                so.FindProperty("breedingButton").objectReferenceValue = btnMenu.GetComponent<Button>();
                so.ApplyModifiedProperties();
                Debug.Log("Gắn MenuItem_Breeding thành công vào MenuPanel!");
            }
        }

        EditorUtility.DisplayDialog("Thành Công", "Đã TỰ ĐỘNG dọn rác, gắn não cho giao diện của bạn, và nối đường dây điện tới Nút Menu! Hãy thoát ra, ấn Play và tận hưởng!", "Tuyệt Vời");
    }
}

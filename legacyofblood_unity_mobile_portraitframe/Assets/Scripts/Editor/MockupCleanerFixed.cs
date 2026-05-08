#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class MockupCleanerFixed
{
    [MenuItem("Tools/Clean Barrack Mockups")]
    public static void CleanMockups()
    {
        GameObject content = GameObject.Find("Panel_Barrack/SafeArea/Content");
        if (content == null)
            content = GameObject.Find("Panel_Barrack/SafeArea/Panel_DanhSachTuong/Viewport/Content"); // Try alternative path
            
        if (content != null)
        {
            int count = 0;
            for (int i = content.transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = content.transform.GetChild(i).gameObject;
                if (!child.name.StartsWith("SortManager_TopSpace")) // Keep the sort buttons if any
                {
                    Object.DestroyImmediate(child);
                    count++;
                }
            }
            Debug.Log($"Cleared {count} mockups from BarrackPanel content.");
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(content.scene);
        }
    }
}
#endif

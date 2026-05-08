#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;

public class BarrackButtonFixer 
{
    [MenuItem("UI Tools/Fix Barrack Buttons Text")]
    public static void FixButtons()
    {
        LegendOfBlood.BarrackPanel panel = Object.FindAnyObjectByType<LegendOfBlood.BarrackPanel>(FindObjectsInactive.Include);
        if (panel == null)
        {
            Debug.LogError("No BarrackPanel found in the scene.");
            return;
        }

        Transform sortBtn = panel.transform.Find("SafeArea/ActionButtonsRow/SortButton");
        if (sortBtn != null && sortBtn.GetComponentInChildren<TextMeshProUGUI>() == null)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(sortBtn, false);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "SORT: POWER";
            tmp.fontSize = 36;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
            RectTransform rt = textObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Debug.Log("Added Text to SortButton");
        }

        Transform filterBtn = panel.transform.Find("SafeArea/ActionButtonsRow/FilterButton");
        if (filterBtn != null && filterBtn.GetComponentInChildren<TextMeshProUGUI>() == null)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(filterBtn, false);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "FILTER: ALL";
            tmp.fontSize = 36;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
            RectTransform rt = textObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Debug.Log("Added Text to FilterButton");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }
}
#endif

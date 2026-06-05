using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using LegendOfBlood;
using TMPro;

public class AutoAddSpeedUpButton
{
    [MenuItem("Tools/Auto Fix Injured Hero Card")]
    public static void AddButton()
    {
        string path = "Assets/Prefabs/Panel/InjuredHeroCard_Final.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) 
        {
            Debug.LogError("Could not find InjuredHeroCard_Final.prefab");
            return;
        }

        InjuredHeroCard card = prefab.GetComponent<InjuredHeroCard>();
        if (card == null)
        {
            Debug.LogError("InjuredHeroCard script not found on prefab");
            return;
        }

        Transform existingButton = prefab.transform.Find("UseSpeedUpButton");
        if (existingButton != null)
        {
            Debug.Log("Button already exists!");
            return;
        }

        Transform healButton = prefab.transform.Find("HealButton");
        if (healButton == null)
        {
            // Fallback, try searching deeper or finding by component
            healButton = prefab.GetComponentInChildren<Button>(true)?.transform;
        }

        GameObject btnObj;
        if (healButton != null)
        {
            btnObj = Object.Instantiate(healButton.gameObject, prefab.transform);
            btnObj.name = "UseSpeedUpButton";
            
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y + 60f); // Dịch lên hoặc xuống tuỳ layout
        }
        else
        {
            btnObj = new GameObject("UseSpeedUpButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(prefab.transform, false);
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 30); 
            rt.sizeDelta = new Vector2(160, 40);
        }

        TextMeshProUGUI tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp == null)
        {
            GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;
            tmp = txtObj.GetComponent<TextMeshProUGUI>();
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 20;
        }
        
        tmp.text = "Dùng Đồng Hồ Cát";

        Button btn = btnObj.GetComponent<Button>();
        btn.onClick = new Button.ButtonClickedEvent(); // Reset events

        SerializedObject so = new SerializedObject(card);
        SerializedProperty sp = so.FindProperty("useSpeedUpButton");
        if (sp != null)
        {
            sp.objectReferenceValue = btn;
            so.ApplyModifiedProperties();
        }

        EditorUtility.SetDirty(prefab);
        AssetDatabase.SaveAssets();
        Debug.Log("Successfully added UseSpeedUpButton to prefab!");
    }
}

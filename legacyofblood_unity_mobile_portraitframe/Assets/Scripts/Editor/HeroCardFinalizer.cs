using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using LegendOfBlood;

public class HeroCardFinalizer : MonoBehaviour
{
    [MenuItem("UI Tools/2. Auto-Build Data Fields for Card")]
    public static void AutoBuild()
    {
        string path = "Assets/Prefabs/Panel/InjuredHeroCard_Final.prefab";
        GameObject prefab = PrefabUtility.LoadPrefabContents(path);
        if (prefab == null) {
            Debug.LogError("Could not find InjuredHeroCard_Final.prefab");
            return;
        }

        HeroCard heroCard = prefab.GetComponent<HeroCard>();
        InjuredHeroCard injCard = prefab.GetComponent<InjuredHeroCard>();

        // 1. Create a Full Card Button if missing
        if (heroCard != null) {
            SerializedObject so = new SerializedObject(heroCard);
            Button btn = prefab.GetComponent<Button>();
            if (btn == null) {
                btn = prefab.AddComponent<Button>();
                // Make a dummy graphic so button catches raycasts if needed, or rely on Image component
                Image bgImg = prefab.GetComponent<Image>();
                if (bgImg == null) {
                    bgImg = prefab.AddComponent<Image>();
                    bgImg.color = new Color(0,0,0,0); // Transparent but catches raycasts
                }
                btn.targetGraphic = bgImg;
            }
            so.FindProperty("cardButton").objectReferenceValue = btn;
            so.ApplyModifiedProperties();
        }

        // 2. Avatar Container
        Transform avatarObj = prefab.transform.Find("AvatarImage");
        if (avatarObj == null) {
            GameObject go = new GameObject("AvatarImage");
            go.transform.SetParent(prefab.transform, false);
            go.transform.SetSiblingIndex(1); // 1 = behind CardFrame ideally, 2 is frame
            avatarObj = go.transform;
            Image img = go.AddComponent<Image>();
            img.color = new Color(0.8f, 0.8f, 0.8f, 1f); 
            if (heroCard != null) {
                SerializedObject so = new SerializedObject(heroCard);
                so.FindProperty("avatarImage").objectReferenceValue = img;
                so.ApplyModifiedProperties();
            }
        }
        
        // Căn chỉnh ảnh sao cho vừa vặn chui lọt vào trong khung xám, ko trào ra ngoài
        RectTransform avaRt = avatarObj.GetComponent<RectTransform>();
        if (avaRt != null) {
            avaRt.anchorMin = new Vector2(0.12f, 0.35f); 
            avaRt.anchorMax = new Vector2(0.88f, 0.82f);
            avaRt.offsetMin = Vector2.zero;
            avaRt.offsetMax = Vector2.zero;
        }

        // 3. Name Text
        Transform nameObj = prefab.transform.Find("NameText");
        if (nameObj == null) {
            GameObject go = new GameObject("NameText");
            go.transform.SetParent(prefab.transform, false);
            nameObj = go.transform;
            
            var txt = go.AddComponent<TextMeshProUGUI>();
            txt.text = "Hero Name";
            txt.fontSize = 16;
            txt.alignment = TextAlignmentOptions.Top;
            txt.color = Color.white;
            txt.fontStyle = FontStyles.Bold;
            txt.outlineWidth = 0.2f;
            txt.outlineColor = new Color(0,0,0,255);

            if (heroCard != null) {
                SerializedObject so = new SerializedObject(heroCard);
                so.FindProperty("nameText").objectReferenceValue = txt;
                so.ApplyModifiedProperties();
            }
        }
        
        // Căn Tên thấp xuống một chút khỏi thanh gỗ trên cùng
        RectTransform nameRt = nameObj.GetComponent<RectTransform>();
        if (nameRt != null) {
            nameRt.anchorMin = new Vector2(0, 1);
            nameRt.anchorMax = new Vector2(1, 1);
            nameRt.pivot = new Vector2(0.5f, 1f);
            nameRt.anchoredPosition = new Vector2(0, -18);
            nameRt.sizeDelta = new Vector2(0, 30);
        }

        // 4. Level Text
        Transform lvlObj = prefab.transform.Find("LevelText");
        if (lvlObj == null) {
            GameObject go = new GameObject("LevelText");
            go.transform.SetParent(prefab.transform, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            // Place it near top left
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(25, -5); // right next to the yellow square
            rt.sizeDelta = new Vector2(50, 20);

            var txt = go.AddComponent<TextMeshProUGUI>();
            txt.text = "Lv.1";
            txt.fontSize = 13;
            txt.alignment = TextAlignmentOptions.Left;
            txt.color = new Color(1f, 0.9f, 0.4f, 1f); // Yellowish
            txt.fontStyle = FontStyles.Bold;
            txt.outlineWidth = 0.2f;
            txt.outlineColor = Color.black;

            if (heroCard != null) {
                SerializedObject so = new SerializedObject(heroCard);
                so.FindProperty("levelText").objectReferenceValue = txt;
                so.ApplyModifiedProperties();
            }
        }

        // 5. Timer Text (For InjuredHeroCard)
        Transform timerObj = prefab.transform.Find("TimerText");
        if (timerObj == null) {
            GameObject go = new GameObject("TimerText");
            go.transform.SetParent(prefab.transform, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            // Center of the avatar frame area
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(0, 40); // above hp bar
            rt.offsetMax = new Vector2(0, -40); // below name
            
            var txt = go.AddComponent<TextMeshProUGUI>();
            txt.text = "00:00:00";
            txt.fontSize = 18;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;
            txt.fontStyle = FontStyles.Bold;
            txt.outlineWidth = 0.2f;
            txt.outlineColor = Color.black;

            if (injCard != null) {
                SerializedObject so = new SerializedObject(injCard);
                so.FindProperty("timerText").objectReferenceValue = txt;
                so.ApplyModifiedProperties();
            }
        }

        // Delete any useless dummy "Hero" text if it happens to just have raw TextMeshPro attached to a general object
        var allTexts = prefab.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var t in allTexts) {
            if (t.gameObject.name != "NameText" && t.gameObject.name != "LevelText" && t.gameObject.name != "TimerText" && t.gameObject.name != "costText" && t.gameObject.name != "Text") {
                if (t.text == "Hero" || t.text == "hero") {
                    Undo.DestroyObjectImmediate(t.gameObject);
                }
            }
        }

        // Hard set the generic button components properly incase user missed them
        Transform healBtn = prefab.transform.Find("HealButton");
        if (healBtn != null && injCard != null) {
            SerializedObject soInfo = new SerializedObject(injCard);
            Button bComp = healBtn.GetComponent<Button>();
            if (bComp) soInfo.FindProperty("healButton").objectReferenceValue = bComp;
            
            var tComp = healBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (tComp) soInfo.FindProperty("costText").objectReferenceValue = tComp;
            
            soInfo.ApplyModifiedProperties();
        }

        PrefabUtility.SaveAsPrefabAsset(prefab, path);
        PrefabUtility.UnloadPrefabContents(prefab);
        
        Debug.Log("<color=cyan>FINISHED:</color> Auto-Built Avatar, Name, Level, and Timer elements and wired them perfectly!");
    }
}

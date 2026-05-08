using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using LegendOfBlood;

namespace LegendOfBlood.EditorMode
{
    public class MapPopupSetup
    {
        [MenuItem("LegendOfBlood/Map UI/Auto Setup Popups")]
        public static void AutoSetupPopups()
        {
            POI_InfoPanel poiPanel = GameObject.FindFirstObjectByType<POI_InfoPanel>(FindObjectsInactive.Include);
            if (poiPanel == null)
            {
                Debug.LogError("Không tìm thấy POI_InfoPanel trong Scene! Bạn có đang mở thư mục chứ không phải Scene chăng?");
                return;
            }



            // 1. Create DifficultySelectionPopup
            Transform diffTrans = poiPanel.transform.Find("DifficultySelectionPopup");
            GameObject diffObj = diffTrans != null ? diffTrans.gameObject : new GameObject("DifficultySelectionPopup");
            diffObj.transform.SetParent(poiPanel.transform, false);
            diffObj.transform.SetAsLastSibling();
            
            RectTransform diffRt = GetOrAdd<RectTransform>(diffObj);
            diffRt.anchorMin = Vector2.zero; diffRt.anchorMax = Vector2.one;
            diffRt.offsetMin = Vector2.zero; diffRt.offsetMax = Vector2.zero;
            
            Image diffBg = GetOrAdd<Image>(diffObj);
            diffBg.color = new Color(0, 0, 0, 0.95f);

            // Title
            TextMeshProUGUI diffTitle = CreateUI_Text(diffObj.transform, "Title", "CHỌN ĐỘ KHÓ\n<color=#D4AF37>MỎ</color>", 50, TextAlignmentOptions.Center, new Color(0.9f, 0.8f, 0.3f),
                new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f), new Vector2(800, 150), new Vector2(0, 0));

            // CP Text
            TextMeshProUGUI diffCP = CreateUI_Text(diffObj.transform, "CurrentCPText", "Lực chiến đội hình: <color=#00ff00>0</color>", 40, TextAlignmentOptions.Center, Color.white,
                new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(800, 100), new Vector2(0, 0));

            // Buttons
            Button btnNorm, btnHard, btnHell, btnNightmare;
            CreateUI_Button(diffObj.transform, "Btn_Normal", "Normal (Rec: 0 CP)", Color.green * 0.7f, new Vector2(0.5f, 0.60f), out btnNorm);
            CreateUI_Button(diffObj.transform, "Btn_Hard", "Hard (Rec: 5,000 CP)", new Color(0.8f, 0.5f, 0f) * 0.7f, new Vector2(0.5f, 0.45f), out btnHard);
            CreateUI_Button(diffObj.transform, "Btn_Hell", "Hell (Rec: 15,000 CP)", new Color(0.9f, 0.1f, 0.1f) * 0.7f, new Vector2(0.5f, 0.30f), out btnHell);
            CreateUI_Button(diffObj.transform, "Btn_Nightmare", "Nightmare (Rec: 35,000 CP)", new Color(0.5f, 0f, 0.5f) * 0.7f, new Vector2(0.5f, 0.15f), out btnNightmare);

            DifficultySelectionPopup diffScript = GetOrAdd<DifficultySelectionPopup>(diffObj);
            diffScript.titleText = diffTitle;
            diffScript.currentCPText = diffCP;
            diffScript.btnNormal = btnNorm;
            diffScript.btnHard = btnHard;
            diffScript.btnHell = btnHell;
            diffScript.btnNightmare = btnNightmare;
            diffObj.SetActive(false);

            // 2. Create NodeDetailPopup
            Transform nodeTrans = poiPanel.transform.Find("NodeDetailPopup");
            GameObject nodeObj = nodeTrans != null ? nodeTrans.gameObject : new GameObject("NodeDetailPopup");
            nodeObj.transform.SetParent(poiPanel.transform, false);
            nodeObj.transform.SetAsLastSibling();

            RectTransform nodeRt = GetOrAdd<RectTransform>(nodeObj);
            nodeRt.anchorMin = new Vector2(0.5f, 0.5f); nodeRt.anchorMax = new Vector2(0.5f, 0.5f);
            nodeRt.sizeDelta = new Vector2(750, 900);
            
            Image nodeBg = GetOrAdd<Image>(nodeObj);
            nodeBg.color = new Color(0.1f, 0.1f, 0.15f, 0.98f);

            // Title
            TextMeshProUGUI nodeTitle = CreateUI_Text(nodeObj.transform, "Title", "<size=45><color=#D4AF37><b>Tầng X - Type</b></color></size>", 40, TextAlignmentOptions.TopLeft, Color.white,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            nodeTitle.rectTransform.offsetMin = new Vector2(40, 700); nodeTitle.rectTransform.offsetMax = new Vector2(-40, -40);

            // Monsters
            TextMeshProUGUI monstersTxt = CreateUI_Text(nodeObj.transform, "Monsters", "Quái thú dự kiến:", 32, TextAlignmentOptions.TopLeft, Color.white,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            monstersTxt.rectTransform.offsetMin = new Vector2(40, 400); monstersTxt.rectTransform.offsetMax = new Vector2(-40, -200);

            // Loot
            TextMeshProUGUI lootTxt = CreateUI_Text(nodeObj.transform, "LootText", "Phần thưởng mong đợi:", 32, TextAlignmentOptions.TopLeft, Color.green,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            lootTxt.rectTransform.offsetMin = new Vector2(40, 150); lootTxt.rectTransform.offsetMax = new Vector2(-40, -500);

            // Close
            Button btnClose;
            CreateUI_Button(nodeObj.transform, "Btn_Close", "X", Color.red, new Vector2(1, 1), out btnClose, new Vector2(70, 70));
            btnClose.GetComponent<RectTransform>().anchoredPosition = new Vector2(-40, -40);
            
            // Action
            Button btnAction;
            CreateUI_Button(nodeObj.transform, "Btn_Action", "Bắt Đầu Khiêu Chiến", new Color(0.8f, 0.2f, 0.2f), new Vector2(0.5f, 0), out btnAction, new Vector2(450, 110));
            btnAction.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 80);
            TextMeshProUGUI actText = btnAction.transform.Find("Text").GetComponent<TextMeshProUGUI>();

            NodeDetailPopup nodeScript = GetOrAdd<NodeDetailPopup>(nodeObj);
            nodeScript.titleText = nodeTitle;
            nodeScript.monstersText = monstersTxt;
            nodeScript.lootText = lootTxt;
            nodeScript.btnClose = btnClose;
            nodeScript.btnAction = btnAction;
            nodeScript.btnActionText = actText;
            nodeObj.SetActive(false);

            // 3. Assign to POI_InfoPanel via SerializedObject
            SerializedObject so = new SerializedObject(poiPanel);
            so.Update();
            so.FindProperty("difficultyPopup").objectReferenceValue = diffScript;
            so.FindProperty("nodeDetailPopup").objectReferenceValue = nodeScript;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(poiPanel);
            Debug.Log("Giao diện chọn độ khó Popup đã được tạo ra thẳng TRONG MÀN HÌNH SCENE của ngài!");
        }

        private static T GetOrAdd<T>(GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            if (comp == null) comp = go.AddComponent<T>();
            return comp;
        }

        private static TextMeshProUGUI CreateUI_Text(Transform parent, string name, string text, float size, TextAlignmentOptions align, Color color, Vector2 aMin, Vector2 aMax, Vector2 sizeDelta, Vector2 pos)
        {
            Transform t = parent.Find(name);
            GameObject obj = t != null ? t.gameObject : new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = GetOrAdd<RectTransform>(obj);
            rt.anchorMin = aMin; rt.anchorMax = aMax;
            rt.sizeDelta = sizeDelta; rt.anchoredPosition = pos;

            TextMeshProUGUI tmp = GetOrAdd<TextMeshProUGUI>(obj);
            tmp.text = text; tmp.fontSize = size; tmp.alignment = align; tmp.color = color;
            return tmp;
        }

        private static void CreateUI_Button(Transform parent, string name, string text, Color bgColor, Vector2 anchor, out Button btn, Vector2? customSize = null)
        {
            Transform t = parent.Find(name);
            GameObject obj = t != null ? t.gameObject : new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = GetOrAdd<RectTransform>(obj);
            rt.anchorMin = anchor; rt.anchorMax = anchor;
            rt.sizeDelta = customSize ?? new Vector2(500, 120);
            
            Image img = GetOrAdd<Image>(obj);
            img.color = bgColor;
            btn = GetOrAdd<Button>(obj);

            TextMeshProUGUI tmp = CreateUI_Text(obj.transform, "Text", text, 36, TextAlignmentOptions.Center, Color.white, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            tmp.rectTransform.offsetMin = Vector2.zero; tmp.rectTransform.offsetMax = Vector2.zero;
        }
    }
}

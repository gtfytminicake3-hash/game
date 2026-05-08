using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FixSubTopBarLayoutWindow : EditorWindow {
    [MenuItem("UI Tools/Fix Overlap Fast")]
    public static void Run() {
        GameObject subRightMenu = GameObject.Find("SubRightMenu");
        GameObject pPass = GameObject.Find("KingGodPass");

        if (pPass != null) {
            Undo.RecordObject(pPass.GetComponent<RectTransform>(), "Fix Pass Ext");
            RectTransform passRt = pPass.GetComponent<RectTransform>();
            passRt.anchorMin = new Vector2(0, 1);
            passRt.anchorMax = new Vector2(0, 1);
            passRt.pivot = new Vector2(0, 1);
            passRt.anchoredPosition = new Vector2(30, -200); // Tch h?n ra kh?i gc v y xung
            passRt.sizeDelta = new Vector2(320, 120);
            
            // X?a l?i text d? b? r?c r?i
            foreach(Transform t in pPass.transform) {
                if (t.name == "TextValue" || t.name == "Icon") {
                    DestroyImmediate(t.gameObject);
                }
            }
        }

        if (subRightMenu != null) {
            Undo.RecordObject(subRightMenu.GetComponent<RectTransform>(), "Fix SubRightMenu Ext");
            RectTransform menuRt = subRightMenu.GetComponent<RectTransform>();
            menuRt.anchorMin = new Vector2(1, 1);
            menuRt.anchorMax = new Vector2(1, 1);
            menuRt.pivot = new Vector2(1, 1);
            menuRt.anchoredPosition = new Vector2(-30, -200); 
            
            // Giam kch thu?c cc nt bn trong d? khng b? qu to
            foreach(Transform child in subRightMenu.transform) {
                var le = child.GetComponent<LayoutElement>();
                if (le != null) {
                    Undo.RecordObject(le, "Fix layout element");
                    le.preferredWidth = 65;
                    le.preferredHeight = 65;
                }
            }
            
            var hlg = subRightMenu.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null) {
                Undo.RecordObject(hlg, "Fix hlg");
                hlg.spacing = 15;
            }
        }
        
        Debug.Log("Fixed. Disconnected KingGodPass and Menu visual overlap.");
    }
}

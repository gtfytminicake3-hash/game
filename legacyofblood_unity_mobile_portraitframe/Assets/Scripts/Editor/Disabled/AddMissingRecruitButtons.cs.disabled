using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LegendOfBlood.EditorScripts
{
    [InitializeOnLoad]
    public class AddMissingRecruitButtons
    {
        static AddMissingRecruitButtons()
        {
            EditorApplication.delayCall += DoBind;
        }

        private static void DoBind()
        {
            string prefabPath = "Assets/Prefabs/Panel/Panel_Recruitment.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) return;

            string assetPath = AssetDatabase.GetAssetPath(prefab);
            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
            {
                var prefabRoot = editingScope.prefabContentsRoot;
                var panelScript = prefabRoot.GetComponent<RecruitmentPanel>();
                if (panelScript == null) return;

                bool modified = false;

                // 1. Fix SummonCard_Single
                var singleCard = FindChild(prefabRoot, "SummonCard_Single");
                if (singleCard != null)
                {
                    var existingBtn = FindChild(singleCard, "ButtonBase");
                    if (existingBtn == null)
                    {
                        // Create button background
                        GameObject btnBase = CreateUIElement("ButtonBase", singleCard.transform);
                        SetAnchor(btnBase, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
                        btnBase.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0); // Position exactly below or similar
                        btnBase.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 50);
                        btnBase.AddComponent<Image>().color = new Color(0.8f, 0.2f, 0.1f, 1f);
                        var btnComp = btnBase.AddComponent<Button>();

                        // Find existing TitleText to move inside
                        var tTxt = FindChild(singleCard, "TitleText");
                        if (tTxt != null)
                        {
                            tTxt.transform.SetParent(btnBase.transform, false);
                            var rt = tTxt.GetComponent<RectTransform>();
                            rt.anchoredPosition = Vector2.zero;
                            rt.sizeDelta = new Vector2(180, 42);
                            var tmp = tTxt.GetComponent<TextMeshProUGUI>();
                            if (tmp != null) 
                            { 
                                tmp.text = "SUMMON x1"; 
                                tmp.fontSize = 22; 
                            }
                        }

                        // Rebind
                        var so = new SerializedObject(panelScript);
                        so.FindProperty("recruitOneButton").objectReferenceValue = btnComp;
                        so.ApplyModifiedProperties();
                        modified = true;
                    }
                }

                // 2. Fix FreeSummonCard_Right
                var freeCard = FindChild(prefabRoot, "FreeSummonCard_Right");
                if (freeCard != null)
                {
                    var existingBtn = FindChild(freeCard, "ButtonBase");
                    if (existingBtn == null)
                    {
                        // Create button background
                        GameObject btnBase = CreateUIElement("ButtonBase", freeCard.transform);
                        SetAnchor(btnBase, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
                        btnBase.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                        btnBase.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 50);
                        btnBase.AddComponent<Image>().color = new Color(0.8f, 0.2f, 0.1f, 1f);
                        var btnComp = btnBase.AddComponent<Button>();

                        // Find existing TitleText to move inside
                        var tTxt = FindChild(freeCard, "TitleText");
                        if (tTxt != null)
                        {
                            tTxt.transform.SetParent(btnBase.transform, false);
                            var rt = tTxt.GetComponent<RectTransform>();
                            rt.anchoredPosition = Vector2.zero;
                            rt.sizeDelta = new Vector2(200, 42);
                            var tmp = tTxt.GetComponent<TextMeshProUGUI>();
                            if (tmp != null) 
                            { 
                                tmp.text = "FREE SUMMON"; 
                                tmp.fontSize = 20; 
                            }
                        }

                        // Rebind
                        var so = new SerializedObject(panelScript);
                        so.FindProperty("recruitAdButton").objectReferenceValue = btnComp;
                        so.ApplyModifiedProperties();
                        modified = true;
                    }
                }

                if (modified)
                {
                    Debug.Log("[AddMissingRecruitButtons] SUCCESS: Added red buttons to Summon x1 and Free Summon cards!");
                }
            }
        }

        private static GameObject CreateUIElement(string name, Transform parent) 
        {
            GameObject go = new GameObject(name);
            go.AddComponent<RectTransform>();
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void SetAnchor(GameObject go, Vector2 min, Vector2 max, Vector2 pivot) 
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.pivot = pivot;
        }

        private static GameObject FindChild(GameObject parent, string name)
        {
            foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == name) return t.gameObject;
            }
            return null;
        }
    }
}

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace LegendOfBlood.EditorScripts
{
    [InitializeOnLoad]
    public class FixRecruitmentOverlay
    {
        static FixRecruitmentOverlay()
        {
            EditorApplication.delayCall += DoBind;
        }

        private static void DoBind()
        {
            // Fix in Prefab
            string prefabPath = "Assets/Prefabs/Panel/Panel_Recruitment.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(prefab);
                using (var editingScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
                {
                    FixPanel(editingScope.prefabContentsRoot, "Prefab");
                }
            }

            // Fix in active Scene
            var panelsInScene = GameObject.FindObjectsByType<RecruitmentPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var panel in panelsInScene)
            {
                FixPanel(panel.gameObject, "Scene");
            }
        }

        private static void FixPanel(GameObject root, string context)
        {
            var panelScript = root.GetComponent<RecruitmentPanel>();
            if (panelScript == null) return;

            // Check if ResultOverlay exists
            Transform overlay = FindChildRecursive(root.transform, "ResultOverlay");
            if (overlay == null) overlay = FindChildRecursive(root.transform, "Panel_RecruitmentResult");

            bool modified = false;

            if (overlay == null)
            {
                Debug.Log($"[FixRecruitmentOverlay] ResultOverlay missing in {context}. Generating new one...");
                
                var oldOverlays = root.GetComponentsInChildren<Transform>(true).Where(t => t.name == "ResultOverlay" || t.name == "Panel_RecruitmentResult").ToList();
                foreach(var o in oldOverlays) GameObject.DestroyImmediate(o.gameObject);

                GameObject resultOverlay = new GameObject("ResultOverlay");
                resultOverlay.transform.SetParent(root.transform, false);
                resultOverlay.transform.SetAsLastSibling(); // ensure it renders on top
                var rt = resultOverlay.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                var bg = resultOverlay.AddComponent<Image>();
                bg.color = new Color(0, 0, 0, 0.9f);

                // Title
                GameObject titleObj = new GameObject("ResultTitle");
                titleObj.transform.SetParent(resultOverlay.transform, false);
                var titleRt = titleObj.AddComponent<RectTransform>();
                titleRt.anchorMin = new Vector2(0.5f, 1);
                titleRt.anchorMax = new Vector2(0.5f, 1);
                titleRt.pivot = new Vector2(0.5f, 1);
                titleRt.anchoredPosition = new Vector2(0, -80);
                titleRt.sizeDelta = new Vector2(800, 80);
                var tmp = titleObj.AddComponent<TextMeshProUGUI>();
                tmp.text = "✨ CHIÊU MỘ THÀNH CÔNG ✨";
                tmp.fontSize = 36;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = new Color(1f, 0.84f, 0f);

                // Container
                GameObject containerObj = new GameObject("ResultCardContainer");
                containerObj.transform.SetParent(resultOverlay.transform, false);
                var cRt = containerObj.AddComponent<RectTransform>();
                cRt.anchorMin = new Vector2(0, 0.15f);
                cRt.anchorMax = new Vector2(1, 0.85f);
                cRt.offsetMin = new Vector2(40, 0);
                cRt.offsetMax = new Vector2(-40, 0);
                
                var hlg = containerObj.AddComponent<HorizontalLayoutGroup>();
                hlg.spacing = 20;
                hlg.childAlignment = TextAnchor.MiddleCenter;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;

                // Close Button
                GameObject closeBtn = new GameObject("CloseResultButton");
                closeBtn.transform.SetParent(resultOverlay.transform, false);
                var bRt = closeBtn.AddComponent<RectTransform>();
                bRt.anchorMin = new Vector2(0.5f, 0);
                bRt.anchorMax = new Vector2(0.5f, 0);
                bRt.anchoredPosition = new Vector2(0, 100);
                bRt.sizeDelta = new Vector2(300, 80);
                closeBtn.AddComponent<Image>().color = new Color(0.2f, 0.6f, 0.3f, 1f);
                closeBtn.AddComponent<Button>();

                var btnTextObj = new GameObject("Text");
                btnTextObj.transform.SetParent(closeBtn.transform, false);
                var tRt = btnTextObj.AddComponent<RectTransform>();
                tRt.anchorMin = Vector2.zero;
                tRt.anchorMax = Vector2.one;
                tRt.offsetMin = Vector2.zero;
                tRt.offsetMax = Vector2.zero;
                var bTmp = btnTextObj.AddComponent<TextMeshProUGUI>();
                bTmp.text = "XÁC NHẬN";
                bTmp.fontSize = 28;
                bTmp.alignment = TextAlignmentOptions.Center;
                bTmp.color = Color.white;

                resultOverlay.SetActive(false);
                overlay = resultOverlay.transform;
                modified = true;
            }

            var so = new SerializedObject(panelScript);
            if (modified || so.FindProperty("resultOverlay").objectReferenceValue == null)
            {
                so.FindProperty("resultOverlay").objectReferenceValue = overlay.gameObject;
                so.FindProperty("resultTitleText").objectReferenceValue = FindChildRecursive(overlay, "ResultTitle")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("resultCardContainer").objectReferenceValue = FindChildRecursive(overlay, "ResultCardContainer");
                so.FindProperty("resultCloseButton").objectReferenceValue = FindChildRecursive(overlay, "CloseResultButton")?.GetComponent<Button>();
                so.ApplyModifiedProperties();
                
                Debug.Log($"[FixRecruitmentOverlay] Fixed null references in {context}.");
            }
        }

        private static Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            foreach (Transform child in parent)
            {
                Transform res = FindChildRecursive(child, name);
                if (res != null) return res;
            }
            return null;
        }
    }
}

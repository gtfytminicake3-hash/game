using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using LegendOfBlood;
using LegendOfBlood.UI; // Assume RecruitmentPanel is in this namespace or similar

namespace LegendOfBlood.EditorScripts
{
    public class RecruitmentPanelAutoBinder : EditorWindow
    {
        [MenuItem("Legend of Blood/Bind Recruitment UI")]
        public static void ShowWindow()
        {
            var window = GetWindow<RecruitmentPanelAutoBinder>("Recruitment Binder");
            window.minSize = new Vector2(300, 150);
        }

        private void OnGUI()
        {
            GUILayout.Space(20);
            if (GUILayout.Button("Bind RecruitmentPanel Prefab", GUILayout.Height(50)))
            {
                BindPrefab();
            }
        }

        private void BindPrefab()
        {
            string prefabPath = "Assets/Prefabs/Panel/Panel_Recruitment.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogError($"[RecruitmentPanelAutoBinder] Could not find prefab at path {prefabPath}");
                return;
            }

            // Using PrefabUtility to edit prefab contents directly
            string assetPath = AssetDatabase.GetAssetPath(prefab);
            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
            {
                var prefabRoot = editingScope.prefabContentsRoot;
                var panelScript = prefabRoot.GetComponent<RecruitmentPanel>();

                if (panelScript == null)
                {
                    Debug.LogWarning("[RecruitmentPanelAutoBinder] RecruitmentPanel script not found on root. Adding it...");
                    panelScript = prefabRoot.AddComponent<RecruitmentPanel>();
                }

                // Bind Buttons
                panelScript.recruitAdButton = FindComponentInChild<Button>(prefabRoot, "FreeSummonCard_Right") ?? FindComponentInChild<Button>(prefabRoot, "Btn_RecruitAd");
                
                var recruit1Obj = FindChildByName(prefabRoot, "SummonCard_Single") ?? FindChildByName(prefabRoot, "Btn_Recruit1");
                if (recruit1Obj != null)
                {
                    var serializedObject = new SerializedObject(panelScript);
                    var property = serializedObject.FindProperty("recruitOneButton");
                    if (property != null)
                    {
                        property.objectReferenceValue = recruit1Obj.GetComponent<Button>();
                        serializedObject.ApplyModifiedProperties();
                    }
                }

                // SummonPortal_Center has ButtonBase as a child, or maybe itself is a button
                var tenBtnBaseObj = FindChildByName(prefabRoot, "ButtonBase", "SummonPortal_Center");
                var recruit10Obj = tenBtnBaseObj ?? FindChildByName(prefabRoot, "SummonPortal_Center") ?? FindChildByName(prefabRoot, "Btn_Recruit10");
                if (recruit10Obj != null)
                {
                    var serializedObject = new SerializedObject(panelScript);
                    var property = serializedObject.FindProperty("recruitTenButton");
                    if (property != null)
                    {
                        property.objectReferenceValue = recruit10Obj.GetComponent<Button>();
                        serializedObject.ApplyModifiedProperties();
                    }
                }
                
                var closeObj = FindChildByName(prefabRoot, "Btn_Close");
                if (closeObj != null)
                {
                    var serializedObject = new SerializedObject(panelScript);
                    var property = serializedObject.FindProperty("closeButton");
                    if (property != null)
                    {
                        property.objectReferenceValue = closeObj.GetComponent<Button>();
                        serializedObject.ApplyModifiedProperties();
                    }
                }

                // Bind Results Display / Overlay
                var resultCloseObj = FindChildByName(prefabRoot, "Btn_Close", "Result"); // attempt to find a close button within a Result container if it exists, otherwise generic close
                
                var resultPanelObj = FindChildByName(prefabRoot, "Panel_RecruitmentResult") ?? FindChildByName(prefabRoot, "ResultPanel") ?? FindChildByName(prefabRoot, "ResultOverlay");
                if (resultPanelObj != null)
                {
                    var serializedObject = new SerializedObject(panelScript);
                    serializedObject.FindProperty("resultOverlay").objectReferenceValue = resultPanelObj;
                    
                    var cardContainer = FindChildByName(resultPanelObj, "Container") ?? FindChildByName(resultPanelObj, "CardContainer") ?? FindChildByName(resultPanelObj, "Grid");
                    if (cardContainer != null) serializedObject.FindProperty("resultCardContainer").objectReferenceValue = cardContainer.transform;
                    
                    var resultCloseBtn = FindComponentInChild<Button>(resultPanelObj, "Btn_Close");
                    if (resultCloseBtn != null) serializedObject.FindProperty("resultCloseButton").objectReferenceValue = resultCloseBtn;
                    
                    var titleText = FindComponentInChild<TextMeshProUGUI>(resultPanelObj, "Text_Title") ?? FindComponentInChild<TextMeshProUGUI>(resultPanelObj, "Title");
                    if (titleText != null) serializedObject.FindProperty("resultTitleText").objectReferenceValue = titleText;

                    serializedObject.ApplyModifiedProperties();
                }

                Debug.Log($"[RecruitmentPanelAutoBinder] Successfully bound properties for {prefabRoot.name}.");
            }
        }

        private GameObject FindChildByName(GameObject parent, string childName, string restrictToParent = null)
        {
            Transform[] children = parent.GetComponentsInChildren<Transform>(true);
            foreach (var t in children)
            {
                if (t.name.Contains(childName))
                {
                    if (restrictToParent != null && !t.parent.name.Contains(restrictToParent))
                        continue;
                    return t.gameObject;
                }
            }
            return null;
        }

        private T FindComponentInChild<T>(GameObject parent, string childName) where T : Component
        {
            GameObject child = FindChildByName(parent, childName);
            if (child != null)
            {
                return child.GetComponent<T>();
            }
            return null;
        }
    }
}

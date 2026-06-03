using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LegendOfBlood.EditorScripts
{
    [InitializeOnLoad]
    public class FixSummonTenButton
    {
        static FixSummonTenButton()
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

                var portalCenter = FindChild(prefabRoot, "SummonPortal_Center");
                if (portalCenter != null)
                {
                    // Find ButtonBase inside SummonPortal_Center
                    var tenBtnBase = FindChild(portalCenter, "ButtonBase");
                    if (tenBtnBase != null)
                    {
                        var btn = tenBtnBase.GetComponent<Button>();
                        if (btn == null) btn = tenBtnBase.AddComponent<Button>();

                        var so = new SerializedObject(panelScript);
                        so.FindProperty("recruitTenButton").objectReferenceValue = btn;
                        so.ApplyModifiedProperties();
                        Debug.Log("[FixSummonTenButton] Fixed recruitTenButton to point to the actual red ButtonBase!");
                    }
                }
            }
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

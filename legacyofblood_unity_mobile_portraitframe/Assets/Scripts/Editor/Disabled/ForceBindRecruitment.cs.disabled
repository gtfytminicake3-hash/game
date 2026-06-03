using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LegendOfBlood.EditorScripts
{
    [InitializeOnLoad]
    public class ForceBindRecruitment
    {
        static ForceBindRecruitment()
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

                // Fix Button components if missing
                var singleCard = FindChild(prefabRoot, "SummonCard_Single");
                if (singleCard != null && singleCard.GetComponent<Button>() == null)
                    singleCard.AddComponent<Button>();

                var portalCenter = FindChild(prefabRoot, "SummonPortal_Center");
                if (portalCenter != null && portalCenter.GetComponent<Button>() == null)
                    portalCenter.AddComponent<Button>();

                var freeCard = FindChild(prefabRoot, "FreeSummonCard_Right");
                if (freeCard != null && freeCard.GetComponent<Button>() == null)
                    freeCard.AddComponent<Button>();

                // Assign references
                var so = new SerializedObject(panelScript);

                if (singleCard != null) 
                    so.FindProperty("recruitOneButton").objectReferenceValue = singleCard.GetComponent<Button>();
                
                if (portalCenter != null)
                    so.FindProperty("recruitTenButton").objectReferenceValue = portalCenter.GetComponent<Button>();

                if (freeCard != null)
                    so.FindProperty("recruitAdButton").objectReferenceValue = freeCard.GetComponent<Button>();

                so.ApplyModifiedProperties();
                Debug.Log("[ForceBindRecruitment] SUCCESS: Attached functions to SummonCard_Single, SummonPortal_Center, FreeSummonCard_Right!");
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

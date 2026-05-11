using UnityEditor;
using UnityEngine;
using LegendOfBlood;

public class FixCPBinding
{
    [InitializeOnLoadMethod]
    static void FixIt()
    {
        string[] prefabPaths = {
            "Assets/Prefabs/HeroCard_Prefab.prefab",
            "Assets/HeroCard_SquadSelection_Prefab.prefab"
        };

        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            HeroCard heroCard = prefab.GetComponent<HeroCard>();
            if (heroCard != null)
            {
                SerializedObject so = new SerializedObject(heroCard);
                so.Update();
                
                // Find CPText child
                Transform cpTextTrans = prefab.transform.Find("CombatPowerBadge/CPText");
                if (cpTextTrans == null) cpTextTrans = prefab.transform.Find("CPText"); // Fallback
                
                if (cpTextTrans != null)
                {
                    TMPro.TextMeshProUGUI cpText = cpTextTrans.GetComponent<TMPro.TextMeshProUGUI>();
                    if (cpText != null)
                    {
                        so.FindProperty("combatPowerText").objectReferenceValue = cpText;
                        so.ApplyModifiedProperties();
                        EditorUtility.SetDirty(prefab);
                        Debug.Log($"Fixed CP Text binding on {path}");
                    }
                }
            }
        }
        AssetDatabase.SaveAssets();
    }
}

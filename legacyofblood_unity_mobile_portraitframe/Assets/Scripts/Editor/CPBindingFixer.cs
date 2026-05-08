using UnityEditor;
using UnityEngine;
using TMPro;

[InitializeOnLoad]
public class CPBindingFixer
{
    static CPBindingFixer()
    {
        EditorApplication.delayCall += FixIt;
    }

    [MenuItem("Tools/Force Fix All HeroCard CPs")]
    public static void FixIt()
    {
        string[] prefabPaths = {
            "Assets/Prefabs/HeroCard_Prefab.prefab",
            "Assets/HeroCard_SquadSelection_Prefab.prefab"
        };

        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            LegendOfBlood.HeroCard heroCard = prefab.GetComponent<LegendOfBlood.HeroCard>();
            if (heroCard != null)
            {
                SerializedObject so = new SerializedObject(heroCard);
                so.Update();
                
                // Use GetComponentInChildren to reliably find any TextMeshProUGUI named "CPText" or "CombatPowerText"
                TextMeshProUGUI cpTextComponent = null;
                TextMeshProUGUI[] allTexts = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach(var t in allTexts)
                {
                    if (t.name.Contains("CPText") || t.name.Contains("CombatPower"))
                    {
                        cpTextComponent = t;
                        break;
                    }
                }
                
                if (cpTextComponent != null)
                {
                    so.FindProperty("combatPowerText").objectReferenceValue = cpTextComponent;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(prefab);
                    Debug.Log($"[Force Fix] Bound CP Text on {path} to {cpTextComponent.name}");
                }
                else
                {
                    Debug.LogWarning($"[Force Fix] Could not find any CP text component in {path}!");
                }
            }
        }
        AssetDatabase.SaveAssets();
    }
}

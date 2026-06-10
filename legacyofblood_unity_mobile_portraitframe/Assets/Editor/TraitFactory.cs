using UnityEngine;
using UnityEditor;
using System.IO;
using LegendOfBlood;

public static class TraitFactory
{
    private const string SAVE_PATH = "Assets/Resources/GameData/Traits/Samples";

    [MenuItem("Tools/Generate Sample Traits (P3)")]
    public static void GenerateSampleTraits()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/GameData")) AssetDatabase.CreateFolder("Assets/Resources", "GameData");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/GameData/Traits")) AssetDatabase.CreateFolder("Assets/Resources/GameData", "Traits");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/GameData/Traits/Samples")) AssetDatabase.CreateFolder("Assets/Resources/GameData/Traits", "Samples");

        int generatedCount = 0;

        string[] traitNames = {
            "Wolf's Vigor", "Bear's Endurance", "Eagle's Precision", "Viper's Strike",
            "Dragon's Blood", "Phoenix's Rebirth", "Turtle's Shell", "Panther's Grace",
            "Lion's Roar", "Fox's Cunning", "Owl's Wisdom", "Shark's Frenzy",
            "Rhino's Charge", "Spider's Web", "Bat's Echo", "Tiger's Fury"
        };

        for (int i = 0; i < 16; i++)
        {
            string traitId = $"SAMPLE_TRAIT_{i+1:00}";
            string path = $"{SAVE_PATH}/{traitId}.asset";

            if (AssetDatabase.LoadAssetAtPath<Trait>(path) != null)
            {
                Debug.Log($"[TraitFactory] Skipped {traitId} because it already exists.");
                continue;
            }

            Trait newTrait = ScriptableObject.CreateInstance<Trait>();
            newTrait.id = traitId;
            newTrait.traitName = traitNames[i];
            newTrait.description = $"A sample trait representing the power of {traitNames[i].Split('\'')[0]}.";
            newTrait.rank = (Trait.RarityRank)(i % 7); // Spread across ranks D to SSS
            newTrait.familyId = "BEAST_BLOOD";

            newTrait.effects = new System.Collections.Generic.List<TraitEffect>();

            if (i % 2 == 0)
            {
                newTrait.effects.Add(new TraitEffect { 
                    type = TraitEffectType.ADD_STAT, 
                    hp = 50 * (i + 1), 
                    atk = 10 * (i + 1) 
                });
            }
            else
            {
                newTrait.effects.Add(new TraitEffect { 
                    type = TraitEffectType.ON_HIT_REFLECT, 
                    effectDescription = "Reflects 10% damage back to attacker." 
                });
            }

            AssetDatabase.CreateAsset(newTrait, path);
            generatedCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[TraitFactory] Successfully generated {generatedCount} sample traits in {SAVE_PATH}.");
    }
}

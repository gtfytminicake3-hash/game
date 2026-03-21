#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using LegendOfBlood.GameConfigs;
using LegendOfBlood;

namespace LegendOfBlood.Editor
{
    public class GameConfigAutoSetup
    {
        [MenuItem("Tools/Legend Of Blood/Auto-Setup Default GameConfig", false, 1)]
        public static void AutoSetupGameConfig()
        {
            string[] guids = AssetDatabase.FindAssets("t:GameConfig");
            if (guids.Length == 0)
            {
                Debug.LogError("Cannot find GameConfig.asset in the project. Please create one first.");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>(path);

            if (config != null)
            {
                // Setup Rarity Configs
                config.RaritySettings = new List<RarityConfig>
                {
                    new RarityConfig { rank = Trait.RarityRank.S, dropChance = 0.5f, minPotential = 17, maxPotential = 21 },
                    new RarityConfig { rank = Trait.RarityRank.A, dropChance = 3.0f, minPotential = 13, maxPotential = 17 },
                    new RarityConfig { rank = Trait.RarityRank.B, dropChance = 6.5f, minPotential = 9, maxPotential = 13 },
                    new RarityConfig { rank = Trait.RarityRank.C, dropChance = 30.0f, minPotential = 5, maxPotential = 9 },
                    new RarityConfig { rank = Trait.RarityRank.D, dropChance = 60.0f, minPotential = 1, maxPotential = 5 }
                };

                // Setup Combat Configs
                config.CombatSettings = new CombatConfig
                {
                    archerBonusCritChance = 0.4f,
                    poisonDamageRatio = 0.2f,
                    slowSpeedReduction = -20f,
                    critUpBonus = 0.15f,
                    defDownRatio = -0.15f,
                    healOverTimeRatio = 0.5f
                };

                // Setup Tower Configs (Mock 20 Floors)
                config.TowerConfigs = new List<TowerFloorConfig>();
                for (int i = 1; i <= 20; i++)
                {
                    config.TowerConfigs.Add(new TowerFloorConfig
                    {
                        floorIndex = i,
                        monsterPool = new List<string> { "MONSTER_ID_01", "MONSTER_ID_02", "MONSTER_ID_03" },
                        customMonsterCount = 0 // Will fallback to 1 + floor/5 logic
                    });
                }

                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();

                Debug.Log("<color=green>GameConfig defaults have been successfully updated!</color> You can view them in the inspector.");
            }
        }
    }
}
#endif

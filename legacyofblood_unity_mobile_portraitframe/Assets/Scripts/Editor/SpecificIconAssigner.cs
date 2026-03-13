using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using LegendOfBlood;

public class SpecificIconAssigner
{
    [MenuItem("Tools/Assign Specific AI Icons")]
    public static void AssignIcons()
    {
        string sourceDir = @"C:\Users\admin\.gemini\antigravity\brain\82e93fb0-0e12-46bb-aece-1756bd385b3e\";
        string targetDir = "Assets/Resources/Icons/GenSpecific";
        
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        // We only care about the latest specific prefixes we generated
        string[] prefixes = new string[] {
            "sk_arc_fire_1", "sk_arc_fire_2", "sk_arc_ice_1", "sk_arc_nature_1",
            "sk_hea_aoe_1", "sk_hea_buff_1", "sk_hea_single_1",
            "sk_mag_fire_1", "sk_mag_ice_1",
            "sk_war_dmg_1", "sk_war_dmg_2", "sk_war_tank_1", "sk_war_tank_2",
            "tr_atk", "tr_def", "tr_hp", "tr_spd", "tr_all",
            "trait_d_08_fastgrower", "trait_s_04_twinoracle", "trait_ss_07_elitelineage", "trait_sss_04_geneselector"
        };

        // 1. Copy files
        foreach (var prefix in prefixes)
        {
            string[] files = Directory.GetFiles(sourceDir, prefix + "*.png");
            if (files.Length > 0)
            {
                string latestFile = files.OrderByDescending(f => new FileInfo(f).LastWriteTime).First();
                // Ensure proper naming for unity loading by taking just the prefix
                string destName = prefix.EndsWith("all") ? prefix + "_all.png" : prefix + ".png";
                if(prefix.StartsWith("tr_") && !prefix.EndsWith("_all")) destName = prefix + "_all.png"; // Map tr_atk to tr_atk_all.png

                // Workaround because the tool named trait icons with "_all" at the end like tr_atk_all
                string realPrefixLookup = prefix;
                if(prefix.StartsWith("tr_") && !prefix.Contains("_all")) realPrefixLookup = prefix + "_all";

                string[] specificFiles = Directory.GetFiles(sourceDir, realPrefixLookup + "*.png");
                if(specificFiles.Length > 0)
                {
                    latestFile = specificFiles.OrderByDescending(f => new FileInfo(f).LastWriteTime).First();
                    File.Copy(latestFile, Path.Combine(targetDir, realPrefixLookup + ".png"), true);
                }
            }
        }
        
        // Ensure generic fallback icons exist from previous run too
        string[] generalPrefixes = new string[] { "icon_skill_warrior", "icon_skill_mage", "icon_skill_healer", "icon_skill_archer" };
        foreach (var prefix in generalPrefixes)
        {
            string[] files = Directory.GetFiles(sourceDir, prefix + "*.png");
            if (files.Length > 0)
            {
                string latestFile = files.OrderByDescending(f => new FileInfo(f).LastWriteTime).First();
                File.Copy(latestFile, Path.Combine(targetDir, prefix + ".png"), true);
            }
        }

        AssetDatabase.Refresh();

        // 2. Set TextureImporter to Sprite
        string[] allDestFiles = Directory.GetFiles(targetDir, "*.png");
        foreach (var file in allDestFiles)
        {
            string assetPath = file.Replace(@"\", "/"); // Fix pathing
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }
        }

        AssetDatabase.Refresh();

        // 3. Assign to Skills
        string[] skillGuids = AssetDatabase.FindAssets("t:Skill");
        foreach (string guid in skillGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Skill skill = AssetDatabase.LoadAssetAtPath<Skill>(path);
            if (skill != null)
            {
                string pfx = "";
                string n = skill.name.ToLower();
                
                // Specific matching based on asset name
                if (n.Contains("arc_fire_1")) pfx = "sk_arc_fire_1";
                else if (n.Contains("arc_fire_2")) pfx = "sk_arc_fire_2";
                else if (n.Contains("arc_ice_1")) pfx = "sk_arc_ice_1";
                else if (n.Contains("arc_nature_1")) pfx = "sk_arc_nature_1";
                else if (n.Contains("hea_aoe")) pfx = "sk_hea_aoe_1";
                else if (n.Contains("hea_buff")) pfx = "sk_hea_buff_1";
                else if (n.Contains("hea_single")) pfx = "sk_hea_single_1";
                else if (n.Contains("mag_fire")) pfx = "sk_mag_fire_1";
                else if (n.Contains("mag_ice")) pfx = "sk_mag_ice_1";
                else if (n.Contains("war_dmg_1") || n.Contains("dungmanh")) pfx = "sk_war_dmg_1";
                else if (n.Contains("war_dmg_2")) pfx = "sk_war_dmg_2";
                else if (n.Contains("war_tank_1")) pfx = "sk_war_tank_1";
                else if (n.Contains("war_tank_2")) pfx = "sk_war_tank_2";
                
                // Fallbacks if no specific match
                else if (skill.requiredProfession == HeroClass.Mage) pfx = "icon_skill_mage";
                else if (skill.requiredProfession == HeroClass.Healer) pfx = "icon_skill_healer";
                else if (skill.requiredProfession == HeroClass.Archer) pfx = "icon_skill_archer";
                else pfx = "icon_skill_warrior";

                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(targetDir + "/" + pfx + ".png");
                if(sp != null)
                {
                    skill.icon = sp;
                    EditorUtility.SetDirty(skill);
                }
            }
        }

        // 4. Assign to Traits
        string[] traitGuids = AssetDatabase.FindAssets("t:Trait");
        foreach (string guid in traitGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Trait trait = AssetDatabase.LoadAssetAtPath<Trait>(path);
            if (trait != null)
            {
                string pfx = "tr_all_all"; 
                string nameLower = trait.name.ToLower();
                if (trait.familyId != null) nameLower += " " + trait.familyId.ToLower();

                if (trait.id == "D_08") pfx = "trait_d_08_fastgrower";
                else if (trait.id == "S_04") pfx = "trait_s_04_twinoracle";
                else if (trait.id == "SS_07") pfx = "trait_ss_07_elitelineage";
                else if (trait.id == "SSS_04") pfx = "trait_sss_04_geneselector";
                else if (nameLower.Contains("atk") || nameLower.Contains("damage")) pfx = "tr_atk_all";
                else if (nameLower.Contains("def") || nameLower.Contains("shield") || nameLower.Contains("armor")) pfx = "tr_def_all";
                else if (nameLower.Contains("hp") || nameLower.Contains("health") || nameLower.Contains("blood")) pfx = "tr_hp_all";
                else if (nameLower.Contains("spd") || nameLower.Contains("speed")) pfx = "tr_spd_all";
                
                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(targetDir + "/" + pfx + ".png");
                if(sp != null)
                {
                    trait.icon = sp;
                    EditorUtility.SetDirty(trait);
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Successfully generated and assigned UNIQUE AI icons to individual Skills and Traits!");
    }
}

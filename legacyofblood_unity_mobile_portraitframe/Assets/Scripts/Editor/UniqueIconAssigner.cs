using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using LegendOfBlood;

public class UniqueIconAssigner
{
    [MenuItem("Tools/Assign V3 Highly Unique AI Icons")]
    public static void AssignIcons()
    {
        string sourceDir = @"C:\Users\admin\.gemini\antigravity\brain\82e93fb0-0e12-46bb-aece-1756bd385b3e\";
        string targetDir = "Assets/Resources/Icons/GenUnique";
        
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        string[] prefixes = new string[] {
            "uniq_tr_atk_d", "uniq_tr_atk_c", "uniq_tr_atk_b", "uniq_tr_atk_a", "uniq_tr_atk_s", "uniq_tr_atk_ss",
            "uniq_tr_hp_d", "uniq_tr_hp_c", "uniq_tr_hp_b", "uniq_tr_hp_a", "uniq_tr_hp_s", "uniq_tr_hp_ss",
            "uniq_tr_def_d", "uniq_tr_def_c", "uniq_tr_def_b", "uniq_tr_def_a", "uniq_tr_def_s",
            "uniq_tr_spd_d", "uniq_tr_spd_c", "uniq_tr_spd_b", "uniq_tr_spd_a", "uniq_tr_spd_s",
            "uniq_trait_d_08_fastgrower", "uniq_tr_all_s", "uniq_tr_all_ss", "uniq_trait_s_04_twinoracle", "uniq_trait_ss_07_elitelineage", "uniq_trait_sss_04_geneselector",
            "uniq_sk_warrior_01", "uniq_sk_warrior_chemdungmanh", "uniq_sk_war_dmg_1", "uniq_sk_war_dmg_2", "uniq_sk_war_tank_1", "uniq_sk_war_tank_2",
            "uniq_sk_archer_01", "uniq_sk_arc_fire_1", "uniq_sk_arc_fire_2", "uniq_sk_arc_ice_1", "uniq_sk_arc_nature_1",
            "uniq_sk_mage_01", "uniq_sk_mag_ice_1", "uniq_sk_mag_fire_1",
            "uniq_sk_healer_01",
            "sk_hea_single_1", "sk_hea_aoe_1", "sk_hea_buff_1" // fallbacks for failed 3
        };

        // 1. Copy files
        foreach (var prefix in prefixes)
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
            string assetPath = file.Replace(@"\", "/");
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
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
                
                if (n.Contains("chemdungmanh")) pfx = "uniq_sk_warrior_chemdungmanh";
                else if (n.Contains("warrior_01")) pfx = "uniq_sk_warrior_01";
                else if (n.Contains("archer_01")) pfx = "uniq_sk_archer_01";
                else if (n.Contains("mage_01")) pfx = "uniq_sk_mage_01";
                else if (n.Contains("healer_01")) pfx = "uniq_sk_healer_01";
                else if (n.Contains("arc_fire_1")) pfx = "uniq_sk_arc_fire_1";
                else if (n.Contains("arc_fire_2")) pfx = "uniq_sk_arc_fire_2";
                else if (n.Contains("arc_ice_1")) pfx = "uniq_sk_arc_ice_1";
                else if (n.Contains("arc_nature_1")) pfx = "uniq_sk_arc_nature_1";
                else if (n.Contains("hea_aoe")) pfx = "sk_hea_aoe_1"; // fallback
                else if (n.Contains("hea_buff")) pfx = "sk_hea_buff_1"; // fallback
                else if (n.Contains("hea_single")) pfx = "sk_hea_single_1"; // fallback
                else if (n.Contains("mag_fire")) pfx = "uniq_sk_mag_fire_1";
                else if (n.Contains("mag_ice")) pfx = "uniq_sk_mag_ice_1";
                else if (n.Contains("war_dmg_1")) pfx = "uniq_sk_war_dmg_1";
                else if (n.Contains("war_dmg_2")) pfx = "uniq_sk_war_dmg_2";
                else if (n.Contains("war_tank_1")) pfx = "uniq_sk_war_tank_1";
                else if (n.Contains("war_tank_2")) pfx = "uniq_sk_war_tank_2";

                if (!string.IsNullOrEmpty(pfx))
                {
                    Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(targetDir + "/" + pfx + ".png");
                    if(sp != null)
                    {
                        skill.icon = sp;
                        EditorUtility.SetDirty(skill);
                    }
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
                string pfx = ""; 
                
                if (trait.id == "D_08") pfx = "uniq_trait_d_08_fastgrower";
                else if (trait.id == "S_04") pfx = "uniq_trait_s_04_twinoracle";
                else if (trait.id == "SS_07") pfx = "uniq_trait_ss_07_elitelineage";
                else if (trait.id == "SSS_04") pfx = "uniq_trait_sss_04_geneselector";
                else if (trait.name.Contains("TR_ALL_S") && !trait.name.Contains("SS")) pfx = "uniq_tr_all_s";
                else if (trait.name.Contains("TR_ALL_SS")) pfx = "uniq_tr_all_ss";
                else if (trait.name.Contains("ATK"))
                {
                    if (trait.name.EndsWith("D")) pfx = "uniq_tr_atk_d";
                    else if (trait.name.EndsWith("C")) pfx = "uniq_tr_atk_c";
                    else if (trait.name.EndsWith("B")) pfx = "uniq_tr_atk_b";
                    else if (trait.name.EndsWith("A")) pfx = "uniq_tr_atk_a";
                    else if (trait.name.EndsWith("_S")) pfx = "uniq_tr_atk_s";
                    else if (trait.name.EndsWith("_SS")) pfx = "uniq_tr_atk_ss";
                }
                else if (trait.name.Contains("HP"))
                {
                    if (trait.name.EndsWith("D")) pfx = "uniq_tr_hp_d";
                    else if (trait.name.EndsWith("C")) pfx = "uniq_tr_hp_c";
                    else if (trait.name.EndsWith("B")) pfx = "uniq_tr_hp_b";
                    else if (trait.name.EndsWith("A")) pfx = "uniq_tr_hp_a";
                    else if (trait.name.EndsWith("_S")) pfx = "uniq_tr_hp_s";
                    else if (trait.name.EndsWith("_SS")) pfx = "uniq_tr_hp_ss";
                }
                else if (trait.name.Contains("DEF"))
                {
                    if (trait.name.EndsWith("D")) pfx = "uniq_tr_def_d";
                    else if (trait.name.EndsWith("C")) pfx = "uniq_tr_def_c";
                    else if (trait.name.EndsWith("B")) pfx = "uniq_tr_def_b";
                    else if (trait.name.EndsWith("A")) pfx = "uniq_tr_def_a";
                    else if (trait.name.EndsWith("_S")) pfx = "uniq_tr_def_s";
                }
                else if (trait.name.Contains("SPD"))
                {
                    if (trait.name.EndsWith("D")) pfx = "uniq_tr_spd_d";
                    else if (trait.name.EndsWith("C")) pfx = "uniq_tr_spd_c";
                    else if (trait.name.EndsWith("B")) pfx = "uniq_tr_spd_b";
                    else if (trait.name.EndsWith("A")) pfx = "uniq_tr_spd_a";
                    else if (trait.name.EndsWith("_S")) pfx = "uniq_tr_spd_s";
                }
                
                if (!string.IsNullOrEmpty(pfx))
                {
                    Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(targetDir + "/" + pfx + ".png");
                    if(sp != null)
                    {
                        trait.icon = sp;
                        EditorUtility.SetDirty(trait);
                    }
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Successfully assigned V3 Highly Unique AI icons!");
    }
}

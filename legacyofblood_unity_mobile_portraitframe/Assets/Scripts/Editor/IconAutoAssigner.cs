using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using LegendOfBlood;

public class IconAutoAssigner
{
    [MenuItem("Tools/Auto Assign Generated Icons")]
    public static void AutoAssignIcons()
    {
        string sourceDir = @"C:\Users\admin\.gemini\antigravity\brain\82e93fb0-0e12-46bb-aece-1756bd385b3e\";
        string targetDir = "Assets/Resources/Icons/Gen";
        
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        string[] prefixes = new string[] {
            "icon_skill_warrior", "icon_skill_mage", "icon_skill_healer", "icon_skill_archer",
            "icon_trait_atk", "icon_trait_def", "icon_trait_hp", "icon_trait_spd", "icon_trait_all"
        };

        // 1. Copy files
        foreach (var prefix in prefixes)
        {
            string[] files = Directory.GetFiles(sourceDir, prefix + "*.png");
            if (files.Length > 0)
            {
                // Get the latest one if multiple exist
                string latestFile = files.OrderByDescending(f => new FileInfo(f).LastWriteTime).First();
                string destPath = Path.Combine(targetDir, prefix + ".png");
                File.Copy(latestFile, destPath, true);
            }
        }

        AssetDatabase.Refresh();

        // 2. Set TextureImporter to Sprite
        foreach (var prefix in prefixes)
        {
            string assetPath = targetDir + "/" + prefix + ".png";
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
                string pfx = "icon_skill_warrior";
                if (skill.requiredProfession == HeroClass.Mage) pfx = "icon_skill_mage";
                else if (skill.requiredProfession == HeroClass.Healer) pfx = "icon_skill_healer";
                else if (skill.requiredProfession == HeroClass.Archer) pfx = "icon_skill_archer";
                
                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(targetDir + "/" + pfx + ".png");
                skill.icon = sp;
                EditorUtility.SetDirty(skill);
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
                string pfx = "icon_trait_all"; // default
                string nameLower = trait.name.ToLower();
                if (trait.familyId != null) nameLower += " " + trait.familyId.ToLower();

                if (nameLower.Contains("atk") || nameLower.Contains("damage")) pfx = "icon_trait_atk";
                else if (nameLower.Contains("def") || nameLower.Contains("shield") || nameLower.Contains("armor")) pfx = "icon_trait_def";
                else if (nameLower.Contains("hp") || nameLower.Contains("health") || nameLower.Contains("blood")) pfx = "icon_trait_hp";
                else if (nameLower.Contains("spd") || nameLower.Contains("speed")) pfx = "icon_trait_spd";
                
                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(targetDir + "/" + pfx + ".png");
                trait.icon = sp;
                EditorUtility.SetDirty(trait);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Successfully assigned generic AI icons to all Skills and Traits!");
    }
}

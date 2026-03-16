using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using LegendOfBlood;

public class IconAutoAssigner
{
    [MenuItem("Tools/Auto Assign Skill Icons")]
    public static void AutoAssignIcons()
    {
        string skillIconDir = "Assets/Resources/Icons/skill";

        // 1. Ensure icons in the folder are set to Sprite
        string[] iconFiles = Directory.GetFiles(skillIconDir, "*.png");
        bool needsRefresh = false;
        foreach (string file in iconFiles)
        {
            string assetPath = file.Replace("\\", "/");
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
                needsRefresh = true;
            }
        }
        
        if (needsRefresh)
        {
            AssetDatabase.Refresh();
        }

        // 2. Assign to Skills
        string[] skillGuids = AssetDatabase.FindAssets("t:Skill");
        int assignedCount = 0;
        foreach (string guid in skillGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Skill skill = AssetDatabase.LoadAssetAtPath<Skill>(path);
            if (skill != null)
            {
                // Prefix based on profession
                string pfx = "";
                if (skill.requiredProfession == HeroClass.Warrior) pfx = "w";
                else if (skill.requiredProfession == HeroClass.Mage) pfx = "m";
                else if (skill.requiredProfession == HeroClass.Archer) pfx = "a";
                else if (skill.requiredProfession == HeroClass.Healer) pfx = "h";

                // Number based on skill asset name (e.g., SK_WAR_1)
                string name = skill.name;
                string order = "1";
                if (name.Contains("_1")) order = "1";
                else if (name.Contains("_2")) order = "2";
                else if (name.Contains("_3")) order = "3";
                else if (name.Contains("_4")) order = "4";
                else if (name.Contains("_5")) order = "5";

                if (!string.IsNullOrEmpty(pfx))
                {
                    string iconName = pfx + order;
                    string iconPath = skillIconDir + "/" + iconName + ".png";
                    
                    Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                    if (sp != null)
                    {
                        skill.icon = sp;
                        EditorUtility.SetDirty(skill);
                        assignedCount++;
                    }
                    else
                    {
                        Debug.LogWarning("Không tìm thấy icon cho skill " + skill.name + " tại: " + iconPath);
                    }
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Thành công! Đã tự động gắn " + assignedCount + " icon mới cho các kỹ năng!");
    }
}

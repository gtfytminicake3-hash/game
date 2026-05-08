#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class HeroCardImporter
{
    [MenuItem("Tools/Import Card Frame 2")]
    public static void ImportCardFrame2()
    {
        string path = "Assets/Resources/UI/HeroCardFrame2.png";
        AssetDatabase.ImportAsset(path);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            // Config 9-slice borders based on visual estimation of the frame
            importer.spriteBorder = new Vector4(120, 120, 120, 120); 
            importer.SaveAndReimport();
            Debug.Log("Successfully imported HeroCardFrame2 as Sprite");
        }
    }
}
#endif

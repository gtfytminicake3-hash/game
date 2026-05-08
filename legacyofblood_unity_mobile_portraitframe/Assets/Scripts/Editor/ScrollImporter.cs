#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ScrollImporter
{
    [MenuItem("Tools/Import Scroll")]
    public static void ImportScroll()
    {
        string path = "Assets/Resources/UI/ScrollNameBg.png";
        AssetDatabase.ImportAsset(path);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            // Config 9-slice borders based on visual estimation of the scroll
            importer.spriteBorder = new Vector4(60, 0, 60, 0); 
            importer.SaveAndReimport();
            Debug.Log("Successfully imported ScrollNameBg as Sprite");
        }
    }
}
#endif

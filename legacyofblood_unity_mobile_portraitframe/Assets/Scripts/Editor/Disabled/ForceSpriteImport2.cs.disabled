using UnityEditor;
using UnityEngine;
[InitializeOnLoad]
public class ForceSpriteImport2 {
    static ForceSpriteImport2() { EditorApplication.delayCall += Run; }
    public static void Run() {
        string[] paths = new string[] {
            "Assets/Resources/Icons/mainscreen/Stone.png",
            "Assets/Resources/Icons/mainscreen/Gems.png"
        };
        foreach (string path in paths) {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite) {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
                Debug.Log("Converted to Sprite (Stone/Gems): " + path);
            }
        }
    }
}

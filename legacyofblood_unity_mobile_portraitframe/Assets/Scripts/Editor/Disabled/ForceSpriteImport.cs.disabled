using UnityEditor;
using UnityEngine;
[InitializeOnLoad]
public class ForceSpriteImport {
    static ForceSpriteImport() { EditorApplication.delayCall += Run; }
    public static void Run() {
        string[] paths = new string[] {
            "Assets/Resources/Icons/Items/ITEM_FERTILITY_POTION.png",
            "Assets/Resources/Icons/Items/ITEM_EXP_BOOK_S.png",
            "Assets/Resources/Icons/Items/ITEM_SPEEDUP_1H.png",
            "Assets/Resources/Icons/Items/ITEM_SUMMON_SCROLL.png",
            "Assets/Resources/Icons/Equipments/Warrior_Helm_B.png",
            "Assets/Resources/Icons/mainscreen/Gold.png"
        };
        foreach (string path in paths) {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite) {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
                Debug.Log("Converted to Sprite: " + path);
            }
        }
    }
}

using UnityEngine;
using UnityEditor;

namespace LegendOfBlood.EditorMode
{
    public class OpenPrefabHelper
    {
        [MenuItem("LegendOfBlood/Map UI/DANG O DAU ROI")]
        public static void OpenThePrefab()
        {
            string path = "Assets/Prefabs/POI_InfoPanel.prefab";
            Object prefab = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (prefab != null)
            {
                AssetDatabase.OpenAsset(prefab);
                Debug.Log(">> Đã cưỡng chế mở Prefab POI_InfoPanel! Ngài hãy nhìn vào Hierarchy xem có 2 cục UI mới chưa.");
            }
            else
            {
                Debug.LogError("File Prefab không nằm ở " + path + ". Hãy tìm thủ công POI_InfoPanel trong ô search Project.");
            }
        }
    }
}

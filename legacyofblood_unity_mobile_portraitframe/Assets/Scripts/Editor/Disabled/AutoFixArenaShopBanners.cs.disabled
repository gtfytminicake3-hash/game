using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using LegendOfBlood;

[InitializeOnLoad]
public class AutoFixArenaShopBanners {
    static AutoFixArenaShopBanners() { EditorApplication.delayCall += Run; }
    
    public static void Run() {
        string prefabPath = "Assets/Prefabs/Panel/Panel_ArenaShop.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) {
            Debug.Log("ArenaShop prefab not found at " + prefabPath);
            return;
        }

        var panel = prefab.GetComponent<ArenaShopPanel>();
        if (panel == null) return;

        // Banners are usually in a container at the top. Let's find it.
        // We know they are 3 UI objects. Let's find all Images whose sprite name is "to_giay"
        Image[] images = prefab.GetComponentsInChildren<Image>(true);
        System.Collections.Generic.List<RectTransform> banners = new System.Collections.Generic.List<RectTransform>();
        
        foreach (var img in images) {
            if (img.sprite != null && img.sprite.name == "to_giay") {
                // Ignore the big ones in the shop items
                if (img.rectTransform.rect.width < 300) {
                    banners.Add(img.rectTransform);
                }
            }
        }
        
        // If we didn't find exactly 3, maybe we find them by looking at children of the top bar
        if (banners.Count < 3) {
            banners.Clear();
            // Loop through direct children of the panel to find a horizontal layout or something with 3 children
            foreach (Transform child in prefab.transform) {
                if (child.name.ToLower().Contains("header") || child.name.ToLower().Contains("top")) {
                    foreach (Transform grandChild in child) {
                        Image img = grandChild.GetComponent<Image>();
                        if (img != null) banners.Add(grandChild.GetComponent<RectTransform>());
                    }
                }
            }
        }

        // Ultimate fallback: Just find the 3 transforms based on their sibling relationship
        if (banners.Count < 3) {
            banners.Clear();
            foreach (Transform child in prefab.transform) {
                // Find a container that has exactly 3 image children with similar sizes
                int imgCount = 0;
                foreach(Transform grandChild in child) {
                    if (grandChild.GetComponent<Image>() != null) imgCount++;
                }
                if (imgCount == 3) {
                    foreach(Transform grandChild in child) {
                        banners.Add(grandChild.GetComponent<RectTransform>());
                    }
                    break;
                }
            }
        }

        if (banners.Count >= 3) {
            // Sort from left to right (X coordinate)
            banners.Sort((a, b) => a.localPosition.x.CompareTo(b.localPosition.x));
            
            SerializedObject serializedObject = new SerializedObject(panel);
            serializedObject.FindProperty("goldBanner").objectReferenceValue = banners[0];
            serializedObject.FindProperty("stoneBanner").objectReferenceValue = banners[1];
            serializedObject.FindProperty("gemBanner").objectReferenceValue = banners[2];
            serializedObject.ApplyModifiedProperties();
            
            EditorUtility.SetDirty(prefab);
            AssetDatabase.SaveAssets();
            Debug.Log("Successfully mapped " + banners.Count + " banners in ArenaShopPanel Prefab!");
            
            // Unregister to avoid running multiple times
            EditorApplication.delayCall -= Run;
        } else {
            Debug.Log("Could not find the 3 banners automatically. Found: " + banners.Count);
        }
    }
}

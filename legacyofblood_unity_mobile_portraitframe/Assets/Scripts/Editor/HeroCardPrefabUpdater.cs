#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class HeroCardPrefabUpdater
{
    [MenuItem("Tools/Update Hero Card Prefab Label")]
    public static void UpdatePrefab()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab HeroCard");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                Transform namePlate = prefab.transform.Find("NamePlate");
                if (namePlate != null)
                {
                    Image img = namePlate.GetComponent<Image>();
                    if (img != null)
                    {
                        Sprite spr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/ScrollNameBg.png");
                        if (spr != null)
                        {
                            img.sprite = spr;
                            img.type = Image.Type.Sliced;
                            img.color = Color.white;
                            EditorUtility.SetDirty(prefab);
                            AssetDatabase.SaveAssets();
                            Debug.Log("HeroCard Prefab successfully updated with scroll background!");
                        }
                    }
                }
            }
        }
    }

    [MenuItem("Tools/Fix Hero Card Class Icons")]
    public static void FixClassIcons()
    {
        string[] prefabPaths = {
            "Assets/Prefabs/HeroCard_Prefab.prefab",
            "Assets/HeroCard_SquadSelection_Prefab.prefab"
        };

        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            LegendOfBlood.HeroCard heroCard = prefab.GetComponent<LegendOfBlood.HeroCard>();
            if (heroCard != null)
            {
                // Assign the sprite fields
                SerializedObject so = new SerializedObject(heroCard);
                so.Update();

                Sprite sWarrior = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/barrack/icon_warrior.png");
                Sprite sArcher = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/barrack/icon_archer.png");
                Sprite sMage = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/barrack/icon_mage.png");
                Sprite sHealer = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/barrack/icon_healer.png");

                so.FindProperty("warriorIcon").objectReferenceValue = sWarrior;
                so.FindProperty("archerIcon").objectReferenceValue = sArcher;
                so.FindProperty("mageIcon").objectReferenceValue = sMage;
                so.FindProperty("healerIcon").objectReferenceValue = sHealer;

                // Adjust RectTransform of the bound professionIcon
                SerializedProperty propIcon = so.FindProperty("professionIcon");
                if (propIcon != null && propIcon.objectReferenceValue != null)
                {
                    Image profImage = propIcon.objectReferenceValue as Image;
                    if (profImage != null)
                    {
                        // CHỈ bật lại hiển thị (Sửa lỗi tàng hình Alpha=0), KHÔNG can thiệp vào vị trí nữa
                        profImage.color = new Color(1f, 1f, 1f, 1f);

                        // Tự động đẩy lên Top layer để không bị khung che (bạn vẫn có thể kéo tay trong Hierarchy)
                        RectTransform rt = profImage.rectTransform;
                        rt.SetAsLastSibling();

                        // Đã BỎ TOÀN BỘ CODE ép cứng kích thước và tọa độ ở đây.
                        // Bây giờ bạn có thể hoàn toàn mở Prefab lên và kéo thả, tinh chỉnh bằng tay (Manual) thoải mái!
                    }
                }

                // Căn chỉnh LevelText
                Transform levelObj = prefab.transform.Find("LevelText");
                if (levelObj != null)
                {
                    RectTransform lvlRt = levelObj.GetComponent<RectTransform>();
                    if (lvlRt != null)
                    {
                        lvlRt.SetAsLastSibling(); // Lớp trên cùng
                        
                        // Đã BỎ TOÀN BỘ CODE ép cứng kích thước và tọa độ ở đây.
                        // Bạn tha hồ mở Prefab Editor và kéo LevelText bằng tay!
                    }
                }

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(prefab);
                Debug.Log($"Restored visibility. You can now manually align layout for: {prefab.name}");
            }
        }
        AssetDatabase.SaveAssets();
    }
}
#endif

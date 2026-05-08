using UnityEngine;
using UnityEditor;
using LegendOfBlood;
using LegendOfBlood.GameConfigs;
using System.Collections.Generic;
using System.IO;

public class RealDataInjector
{
    [MenuItem("🔥 INJECT INVENTORY (BẤM VÀO ĐÂY) 🔥/💎 Bơm DATA THỰC TẾ (Real Data)", false, 2)]
    public static void CreateAndPopulateData()
    {
        string dirPath = "Assets/GameData/Items";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        string[] ids = { "item_mutation_potion", "item_wish_charm", "item_speed_hourglass" };
        string[] names = { "Thuốc Biến Dị", "Bùa Ước Nguyện", "Đồng Hồ Cát" };
        string[] descs = {
            "Một loại huyết thanh kì bí sủi bọt xanh. Có khả năng kích phát đột biến gen, cung cấp exp Khổng khồ cho các Hero.",
            "Tấm bùa rách nát cổ xưa, phát ra một thứ ánh sáng ma mị. Dùng để mở khóa chức năng Breeding triệu hồi chiến binh.",
            "Hạt cát bên trong chảy lướt qua thời không. Dùng để gia tốc trứng nở hoặc rút ngắn thời gian thám hiểm của Expedition."
        };
        ItemType[] types = { ItemType.Consumable, ItemType.BreedingMaterial, ItemType.SpeedUp };

        List<ItemData> newItems = new List<ItemData>();

        for (int i = 0; i < ids.Length; i++)
        {
            string assetPath = $"{dirPath}/{ids[i]}.asset";
            ItemData data = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<ItemData>();
                AssetDatabase.CreateAsset(data, assetPath);
            }

            data.id = ids[i];
            data.itemName = names[i];
            data.description = descs[i];
            data.type = types[i];
            
            Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/Items/{ids[i]}.png");
            if (icon != null) data.icon = icon;

            EditorUtility.SetDirty(data);
            newItems.Add(data);
        }

        GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/GameData/GameConfig.asset");
        if (config != null)
        {
            if (config.AllItems == null) config.AllItems = new List<ItemData>();
            foreach (var item in newItems)
            {
                if (!config.AllItems.Contains(item))
                {
                    config.AllItems.Add(item);
                }
            }
            EditorUtility.SetDirty(config);
            Debug.Log("<color=green>[RealDataInjector] Đã tự động tạo các ItemData và móc vào GameConfig!</color>");
        }
        else
        {
            Debug.LogError("[RealDataInjector] Không tìm thấy GameConfig tại Assets/GameData/GameConfig.asset");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

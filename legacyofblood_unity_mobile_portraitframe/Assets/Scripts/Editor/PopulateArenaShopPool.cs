using UnityEngine;
using UnityEditor;
using LegendOfBlood;
using LegendOfBlood.GameConfigs;

public class PopulateArenaShopPool
{
    [MenuItem("Tools/LegendOfBlood/Populate Arena Shop Pool")]
    public static void Run()
    {
        var db = Resources.Load<GameConfig>("GameConfig");
        if (db == null)
        {
            var guids = AssetDatabase.FindAssets("t:GameConfig");
            if (guids.Length > 0)
            {
                db = AssetDatabase.LoadAssetAtPath<GameConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
        }

        if (db == null)
        {
            Debug.LogError("Cannot find GameConfig!");
            return;
        }

        if (db.ArenaShopPool == null) db.ArenaShopPool = new System.Collections.Generic.List<ArenaShopPoolItem>();

        if (db.ArenaShopPool.Count > 0)
        {
            Debug.Log("[ArenaShop] Pool đã có dữ liệu, không ghi đè để tránh mất công sức cấu hình của bạn.");
            return;
        }

        // Tạo mảng dữ liệu mẫu dựa trên dailyGoods cũ và mở rộng
        db.ArenaShopPool.Add(new ArenaShopPoolItem { type = ShopGoodType.Item, refId = "ITEM_FERTILITY_POTION", displayName = "Thuốc Sinh Sản", priceMin = 4800, priceMax = 5200, amountMin = 1, amountMax = 2, weight = 100, isInfinite = false });
        db.ArenaShopPool.Add(new ArenaShopPoolItem { type = ShopGoodType.Item, refId = "ITEM_EXP_BOOK_S", displayName = "Sách Kinh Nghiệm", priceMin = 800, priceMax = 1200, amountMin = 3, amountMax = 10, weight = 200, isInfinite = false });
        db.ArenaShopPool.Add(new ArenaShopPoolItem { type = ShopGoodType.Item, refId = "ITEM_EXP_BOOK_M", displayName = "Sách Kinh Nghiệm Vừa", priceMin = 2500, priceMax = 3000, amountMin = 1, amountMax = 5, weight = 150, isInfinite = false });
        db.ArenaShopPool.Add(new ArenaShopPoolItem { type = ShopGoodType.Item, refId = "ITEM_EXP_BOOK_L", displayName = "Sách Kinh Nghiệm Lớn", priceMin = 8000, priceMax = 10000, amountMin = 1, amountMax = 2, weight = 50, isInfinite = false });
        
        db.ArenaShopPool.Add(new ArenaShopPoolItem { type = ShopGoodType.Item, refId = "ITEM_SPEEDUP_1H", displayName = "Tua Nhanh 1H", priceMin = 150, priceMax = 250, amountMin = 1, amountMax = 3, weight = 150, isInfinite = false });
        db.ArenaShopPool.Add(new ArenaShopPoolItem { type = ShopGoodType.Item, refId = "ITEM_SPEEDUP_4H", displayName = "Tua Nhanh 4H", priceMin = 500, priceMax = 800, amountMin = 1, amountMax = 2, weight = 80, isInfinite = false });
        
        db.ArenaShopPool.Add(new ArenaShopPoolItem { type = ShopGoodType.Item, refId = "ITEM_SUMMON_SCROLL", displayName = "Cuộn Triệu Hồi", priceMin = 3000, priceMax = 3000, amountMin = 1, amountMax = 1, weight = 50, isInfinite = false });
        db.ArenaShopPool.Add(new ArenaShopPoolItem { type = ShopGoodType.Item, refId = "ITEM_SUMMON_SCROLL_PREMIUM", displayName = "Triệu Hồi Cao Cấp", priceMin = 15000, priceMax = 15000, amountMin = 1, amountMax = 1, weight = 10, isInfinite = false });
        
        db.ArenaShopPool.Add(new ArenaShopPoolItem { type = ShopGoodType.Equipment, refId = "EQ_KNIGHT_HELM", displayName = "Mũ Hiệp Sĩ", priceMin = 1000, priceMax = 1500, equipTier = EquipmentTier.B, equipSlot = EquipmentSlot.Helm, equipRestriction = Profession.Warrior, weight = 100, isInfinite = false });
        db.ArenaShopPool.Add(new ArenaShopPoolItem { type = ShopGoodType.Equipment, refId = "EQ_ROGUE_DAGGER", displayName = "Dao Găm", priceMin = 1000, priceMax = 1500, equipTier = EquipmentTier.B, equipSlot = EquipmentSlot.Weapon, equipRestriction = Profession.Warrior, weight = 100, isInfinite = false });
        db.ArenaShopPool.Add(new ArenaShopPoolItem { type = ShopGoodType.Equipment, refId = "EQ_CLERIC_STAFF", displayName = "Trượng Hồi Máu", priceMin = 1000, priceMax = 1500, equipTier = EquipmentTier.B, equipSlot = EquipmentSlot.Weapon, equipRestriction = Profession.Healer, weight = 100, isInfinite = false });
        db.ArenaShopPool.Add(new ArenaShopPoolItem { type = ShopGoodType.Equipment, refId = "EQ_WARRIOR_ARMOR", displayName = "Giáp Thép", priceMin = 3000, priceMax = 4000, equipTier = EquipmentTier.A, equipSlot = EquipmentSlot.Armor, equipRestriction = Profession.Warrior, weight = 50, isInfinite = false });
        
        db.ArenaShopPool.Add(new ArenaShopPoolItem { type = ShopGoodType.Resource, refId = "GOLD", displayName = "Vàng", priceMin = 500, priceMax = 1000, amountMin = 5000, amountMax = 20000, weight = 300, isInfinite = true });
        db.ArenaShopPool.Add(new ArenaShopPoolItem { type = ShopGoodType.Resource, refId = "WOOD", displayName = "Gỗ", priceMin = 300, priceMax = 600, amountMin = 500, amountMax = 2000, weight = 200, isInfinite = false });
        db.ArenaShopPool.Add(new ArenaShopPoolItem { type = ShopGoodType.Resource, refId = "STONE", displayName = "Đá", priceMin = 300, priceMax = 600, amountMin = 500, amountMax = 2000, weight = 200, isInfinite = false });

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();

        Debug.Log($"[ArenaShop] Đã tự động tạo tổng cộng {db.ArenaShopPool.Count} mặt hàng mẫu vào mảng ArenaShopPool!");
    }
}

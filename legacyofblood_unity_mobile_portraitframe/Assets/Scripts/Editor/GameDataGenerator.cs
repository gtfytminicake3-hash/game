using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using LegendOfBlood;
using LegendOfBlood.GameConfigs;
using System.IO;

namespace LegendOfBlood.Editor
{
    public class GameDataGenerator
    {
        private static string dataPath = "Assets/Resources/GameData";

        [MenuItem("Tools/Legend Of Blood/Generate Master Data Assets", false, 2)]
        public static void GenerateMasterData()
        {
            if (!Directory.Exists(Application.dataPath + "/Resources/GameData"))
            {
                Directory.CreateDirectory(Application.dataPath + "/Resources/GameData");
                AssetDatabase.Refresh();
            }

            var traits = GenerateTraits();
            var skills = GenerateSkills();
            var items = GenerateItems();
            var buildings = GenerateBuildings();
            var quests = GenerateQuests();

            UpdateGameConfig(traits, skills, items, buildings, quests);
        }

        private static List<Trait> GenerateTraits()
        {
            List<Trait> list = new List<Trait>();

            Trait t1 = GetOrCreateAsset<Trait>($"{dataPath}/TR_ATK_D.asset");
            t1.id = "TR_ATK_D"; t1.traitName = "Sức Mạnh Tân Binh"; t1.description = "Tăng nhẹ sức tấn công.";
            t1.rank = Trait.RarityRank.D; t1.familyId = "ATK_UP"; t1.nextUpgradeTraitID = "TR_ATK_C";
            t1.effects = new List<TraitEffect> { new TraitEffect { type = TraitEffectType.ADD_STAT, atk = 10 } };
            list.Add(t1);

            Trait t2 = GetOrCreateAsset<Trait>($"{dataPath}/TR_ATK_C.asset");
            t2.id = "TR_ATK_C"; t2.traitName = "Sức Mạnh Chiến Binh"; t2.description = "Tăng lượng khá sức tấn công.";
            t2.rank = Trait.RarityRank.C; t2.familyId = "ATK_UP"; t2.nextUpgradeTraitID = "TR_ATK_B";
            t2.effects = new List<TraitEffect> { new TraitEffect { type = TraitEffectType.ADD_STAT, atk = 25 } };
            list.Add(t2);

            Trait t3 = GetOrCreateAsset<Trait>($"{dataPath}/TR_HP_C.asset");
            t3.id = "TR_HP_C"; t3.traitName = "Thể Lực Dồi Dào"; t3.description = "Tăng máu tối đa (%HP).";
            t3.rank = Trait.RarityRank.C; t3.familyId = "HP_UP";
            t3.effects = new List<TraitEffect> { new TraitEffect { type = TraitEffectType.MULTIPLY_STAT, hp = 15 } }; // 15%
            list.Add(t3);

            Trait t4 = GetOrCreateAsset<Trait>($"{dataPath}/TR_SPD_B.asset");
            t4.id = "TR_SPD_B"; t4.traitName = "Nhanh Như Gió"; t4.description = "Tăng tốc độ hành động.";
            t4.rank = Trait.RarityRank.B; t4.familyId = "SPD_UP";
            t4.effects = new List<TraitEffect> { new TraitEffect { type = TraitEffectType.ADD_STAT, spd = 20 } };
            list.Add(t4);

            Trait t5 = GetOrCreateAsset<Trait>($"{dataPath}/TR_ALL_S.asset");
            t5.id = "TR_ALL_S"; t5.traitName = "Hoàng Gia Chi Huyết"; t5.description = "Tăng toàn bộ chỉ số.";
            t5.rank = Trait.RarityRank.S; t5.familyId = "ALL_STAT";
            t5.effects = new List<TraitEffect> { 
                new TraitEffect { type = TraitEffectType.MULTIPLY_STAT, hp = 10, atk = 10, def = 10, spd = 10 } 
            };
            list.Add(t5);

            foreach(var t in list) EditorUtility.SetDirty(t);
            return list;
        }

        private static List<Skill> GenerateSkills()
        {
            List<Skill> list = new List<Skill>();

            Skill swar = GetOrCreateAsset<Skill>($"{dataPath}/SK_WARRIOR_01.asset");
            swar.id = "SK_WARRIOR_01"; swar.skillName = "Trảm Kích"; swar.description = "Chém kẻ thù hàng trước 150% sát thương.";
            swar.type = SkillType.Active; swar.requiredProfession = HeroClass.Warrior; swar.cooldown = 2;
            swar.targeting = TargetingType.SingleFrontEnemy; swar.powerRatio = 1.5f; swar.hitCount = 1;
            list.Add(swar);

            Skill sarch = GetOrCreateAsset<Skill>($"{dataPath}/SK_ARCHER_01.asset");
            sarch.id = "SK_ARCHER_01"; sarch.skillName = "Tiễn Độc"; sarch.description = "Bắn kẻ thù ngẫu nhiên, gây sát thương và nhiễm độc.";
            sarch.type = SkillType.Active; sarch.requiredProfession = HeroClass.Archer; sarch.cooldown = 3;
            sarch.targeting = TargetingType.RandomEnemy; sarch.powerRatio = 1.2f; sarch.hitCount = 1;
            sarch.appliedEffect = StatusEffectType.Poison; sarch.effectDuration = 3; sarch.effectChance = 0.8f;
            list.Add(sarch);

            Skill smage = GetOrCreateAsset<Skill>($"{dataPath}/SK_MAGE_01.asset");
            smage.id = "SK_MAGE_01"; smage.skillName = "Mưa Lửa"; smage.description = "Tấn công toàn bộ kẻ địch gây 100% sát thương M.ATK.";
            smage.type = SkillType.Active; smage.requiredProfession = HeroClass.Mage; smage.cooldown = 4;
            smage.targeting = TargetingType.AllEnemies; smage.powerRatio = 1.0f; smage.hitCount = 1;
            list.Add(smage);

            Skill sheal = GetOrCreateAsset<Skill>($"{dataPath}/SK_HEALER_01.asset");
            sheal.id = "SK_HEALER_01"; sheal.skillName = "Chữa Lành"; sheal.description = "Hồi máu cho đồng minh thấp máu nhất.";
            sheal.type = SkillType.Active; sheal.requiredProfession = HeroClass.Healer; sheal.cooldown = 2;
            sheal.targeting = TargetingType.LowestHpAlly; sheal.powerRatio = 2.0f; sheal.hitCount = 1;
            list.Add(sheal);

            foreach(var s in list) EditorUtility.SetDirty(s);
            return list;
        }

        private static List<ItemData> GenerateItems()
        {
            List<ItemData> list = new List<ItemData>();

            ItemData i1 = GetOrCreateAsset<ItemData>($"{dataPath}/ITEM_MUTATION_POTION.asset");
            i1.id = "ITEM_MUTATION_POTION"; i1.itemName = "Thuốc Đột Biến"; i1.description = "Tăng tỉ lệ đột biến Gen khi lai tạo Tướng.";
            i1.type = ItemType.Consumable; i1.isStackable = true; i1.sellPrice = 500;
            list.Add(i1);

            ItemData i2 = GetOrCreateAsset<ItemData>($"{dataPath}/ITEM_SPEEDUP_1H.asset");
            i2.id = "ITEM_SPEEDUP_1H"; i2.itemName = "Đồng Hồ Cát Trắng"; i2.description = "Giảm thời gian chờ đi 1 Giờ.";
            i2.type = ItemType.SpeedUp; i2.speedUpValueInSeconds = 3600; i2.isStackable = true; i2.sellPrice = 100;
            list.Add(i2);

            foreach(var it in list) EditorUtility.SetDirty(it);
            return list;
        }

        private static List<BuildingUpgradeData> GenerateBuildings()
        {
            List<BuildingUpgradeData> list = new List<BuildingUpgradeData>();
            string[] buildingTypes = { "TownHall", "Barracks", "Hospital", "BreedingPen" };

            foreach (var bType in buildingTypes)
            {
                BuildingUpgradeData data = GetOrCreateAsset<BuildingUpgradeData>($"{dataPath}/BLD_{bType}.asset");
                data.buildingId = bType;
                data.levels = new List<BuildingLevelData>
                {
                    new BuildingLevelData { level = 1, costs = new List<UpgradeCost>(), duration = 0 },
                    new BuildingLevelData { level = 2, costs = new List<UpgradeCost> { new UpgradeCost { resourceId = "Wood", amount = 150 } }, duration = 15 },
                    new BuildingLevelData { level = 3, costs = new List<UpgradeCost> { new UpgradeCost { resourceId = "Wood", amount = 300 }, new UpgradeCost { resourceId = "Stone", amount = 100 } }, duration = 60 }
                };
                list.Add(data);
                EditorUtility.SetDirty(data);
            }
            return list;
        }

        private static List<QuestData> GenerateQuests()
        {
            List<QuestData> list = new List<QuestData>();

            QuestData q1 = GetOrCreateAsset<QuestData>($"{dataPath}/Q_MAIN_01.asset");
            q1.questId = "Q_MAIN_01"; q1.category = QuestCategory.Main; q1.questName = "Xây Làng Dựng Nước";
            q1.description = "Nâng cấp nhà chính TownHall lên Cấp 2."; q1.type = QuestType.UPGRADE_BUILDING;
            q1.targetId = "TownHall"; q1.targetValue = 2;
            q1.rewards = new List<QuestReward> { new QuestReward { resourceId = "Gold", amount = 1000 } };
            list.Add(q1);

            QuestData q2 = GetOrCreateAsset<QuestData>($"{dataPath}/Q_DAILY_01.asset");
            q2.questId = "Q_DAILY_01"; q2.category = QuestCategory.Daily; q2.questName = "Nghệ Thuật Lai Tạo";
            q2.description = "Hoàn thành lai tạo 1 Tướng bất kỳ."; q2.type = QuestType.BREED_HERO;
            q2.targetValue = 1;
            q2.rewards = new List<QuestReward> { new QuestReward { resourceId = "Wood", amount = 200 } };
            list.Add(q2);

            foreach(var q in list) EditorUtility.SetDirty(q);
            return list;
        }

        private static void UpdateGameConfig(List<Trait> traits, List<Skill> skills, List<ItemData> items, List<BuildingUpgradeData> buildings, List<QuestData> quests)
        {
            AssetDatabase.SaveAssets();
            string[] guids = AssetDatabase.FindAssets("t:GameConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>(path);

                if (config != null)
                {
                    config.AllTraits = traits;
                    config.AllSkills = skills;
                    config.AllItems = items;
                    config.BuildingUpgradeDataList = buildings;
                    config.AllQuestData = quests;

                    // Mồi thêm một số Item Data nếu có
                    config.StartingSkills = new List<ProfessionStartingSkills>
                    {
                        new ProfessionStartingSkills { profession = Profession.Warrior, startingSkillIDs = new List<string> { "SK_WARRIOR_01" } },
                        new ProfessionStartingSkills { profession = Profession.Archer, startingSkillIDs = new List<string> { "SK_ARCHER_01" } },
                        new ProfessionStartingSkills { profession = Profession.Mage, startingSkillIDs = new List<string> { "SK_MAGE_01" } },
                        new ProfessionStartingSkills { profession = Profession.Healer, startingSkillIDs = new List<string> { "SK_HEALER_01" } }
                    };

                    EditorUtility.SetDirty(config);
                    AssetDatabase.SaveAssets();
                    Debug.Log("<color=cyan>[GameDataGenerator]</color> Đã nạp thành công bộ MASTER DATA Khổng Lồ vào GameConfig!");
                }
            }
            else
            {
                Debug.LogWarning("Không tìm thấy GameConfig.asset! Hãy tạo nó trước để Master Data có thể chèn vào.");
            }
        }

        private static T GetOrCreateAsset<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }
    }
}

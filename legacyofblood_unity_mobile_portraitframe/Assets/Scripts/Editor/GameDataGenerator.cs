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
            var expTable = GenerateExperienceTable();
            var evoTable = GenerateEvolutionTable();
            var poiConfig = GeneratePOIMonsterConfig();
            var rarityConfig = GenerateRaritySettings();
            var monsters = GenerateMonsters(); // NEW
            var bosses = GenerateBossData();
            var towerConfigs = GenerateTowerConfigs();

            UpdateGameConfig(traits, skills, items, buildings, quests, expTable, evoTable, poiConfig, rarityConfig, monsters, bosses, towerConfigs);
        }

        private static List<Trait> GenerateTraits()
        {
            List<Trait> list = new List<Trait>();

            // Hệ ATK
            list.Add(CreateTrait("TR_ATK_D", "trait_TR_ATK_D_name", "trait_TR_ATK_D_desc", Trait.RarityRank.D, "ATK_UP", "TR_ATK_C", TraitEffectType.ADD_STAT, atk: 5));
            list.Add(CreateTrait("TR_ATK_C", "trait_TR_ATK_C_name", "trait_TR_ATK_C_desc", Trait.RarityRank.C, "ATK_UP", "TR_ATK_B", TraitEffectType.ADD_STAT, atk: 15));
            list.Add(CreateTrait("TR_ATK_B", "trait_TR_ATK_B_name", "trait_TR_ATK_B_desc", Trait.RarityRank.B, "ATK_UP", "TR_ATK_A", TraitEffectType.ADD_STAT, atk: 35));
            list.Add(CreateTrait("TR_ATK_A", "trait_TR_ATK_A_name", "trait_TR_ATK_A_desc", Trait.RarityRank.A, "ATK_UP", "TR_ATK_S", TraitEffectType.ADD_STAT, atk: 100));
            list.Add(CreateTrait("TR_ATK_S", "trait_TR_ATK_S_name", "trait_TR_ATK_S_desc", Trait.RarityRank.S, "ATK_UP", "TR_ATK_SS", TraitEffectType.MULTIPLY_STAT, atk: 20));
            list.Add(CreateTrait("TR_ATK_SS", "trait_TR_ATK_SS_name", "trait_TR_ATK_SS_desc", Trait.RarityRank.SS, "ATK_UP", "", TraitEffectType.MULTIPLY_STAT, atk: 50));

            // Hệ HP
            list.Add(CreateTrait("TR_HP_D", "trait_TR_HP_D_name", "trait_TR_HP_D_desc", Trait.RarityRank.D, "HP_UP", "TR_HP_C", TraitEffectType.ADD_STAT, hp: 20));
            list.Add(CreateTrait("TR_HP_C", "trait_TR_HP_C_name", "trait_TR_HP_C_desc", Trait.RarityRank.C, "HP_UP", "TR_HP_B", TraitEffectType.ADD_STAT, hp: 80));
            list.Add(CreateTrait("TR_HP_B", "trait_TR_HP_B_name", "trait_TR_HP_B_desc", Trait.RarityRank.B, "HP_UP", "TR_HP_A", TraitEffectType.ADD_STAT, hp: 200));
            list.Add(CreateTrait("TR_HP_A", "trait_TR_HP_A_name", "trait_TR_HP_A_desc", Trait.RarityRank.A, "HP_UP", "TR_HP_S", TraitEffectType.ADD_STAT, hp: 500));
            list.Add(CreateTrait("TR_HP_S", "trait_TR_HP_S_name", "trait_TR_HP_S_desc", Trait.RarityRank.S, "HP_UP", "TR_HP_SS", TraitEffectType.MULTIPLY_STAT, hp: 20));
            list.Add(CreateTrait("TR_HP_SS", "trait_TR_HP_SS_name", "trait_TR_HP_SS_desc", Trait.RarityRank.SS, "HP_UP", "", TraitEffectType.MULTIPLY_STAT, hp: 50));

            // Hệ DEF
            list.Add(CreateTrait("TR_DEF_D", "trait_TR_DEF_D_name", "trait_TR_DEF_D_desc", Trait.RarityRank.D, "DEF_UP", "TR_DEF_C", TraitEffectType.ADD_STAT, def: 2));
            list.Add(CreateTrait("TR_DEF_C", "trait_TR_DEF_C_name", "trait_TR_DEF_C_desc", Trait.RarityRank.C, "DEF_UP", "TR_DEF_B", TraitEffectType.ADD_STAT, def: 10));
            list.Add(CreateTrait("TR_DEF_B", "trait_TR_DEF_B_name", "trait_TR_DEF_B_desc", Trait.RarityRank.B, "DEF_UP", "TR_DEF_A", TraitEffectType.ADD_STAT, def: 25));
            list.Add(CreateTrait("TR_DEF_A", "trait_TR_DEF_A_name", "trait_TR_DEF_A_desc", Trait.RarityRank.A, "DEF_UP", "TR_DEF_S", TraitEffectType.ADD_STAT, def: 60));
            list.Add(CreateTrait("TR_DEF_S", "trait_TR_DEF_S_name", "trait_TR_DEF_S_desc", Trait.RarityRank.S, "DEF_UP", "TR_DEF_SS", TraitEffectType.MULTIPLY_STAT, def: 20));

            // Hệ SPD
            list.Add(CreateTrait("TR_SPD_D", "trait_TR_SPD_D_name", "trait_TR_SPD_D_desc", Trait.RarityRank.D, "SPD_UP", "TR_SPD_C", TraitEffectType.ADD_STAT, spd: 2));
            list.Add(CreateTrait("TR_SPD_C", "trait_TR_SPD_C_name", "trait_TR_SPD_C_desc", Trait.RarityRank.C, "SPD_UP", "TR_SPD_B", TraitEffectType.ADD_STAT, spd: 5));
            list.Add(CreateTrait("TR_SPD_B", "trait_TR_SPD_B_name", "trait_TR_SPD_B_desc", Trait.RarityRank.B, "SPD_UP", "TR_SPD_A", TraitEffectType.ADD_STAT, spd: 12));
            list.Add(CreateTrait("TR_SPD_A", "trait_TR_SPD_A_name", "trait_TR_SPD_A_desc", Trait.RarityRank.A, "SPD_UP", "TR_SPD_S", TraitEffectType.ADD_STAT, spd: 30));
            list.Add(CreateTrait("TR_SPD_S", "trait_TR_SPD_S_name", "trait_TR_SPD_S_desc", Trait.RarityRank.S, "SPD_UP", "TR_SPD_SS", TraitEffectType.MULTIPLY_STAT, spd: 20));

            // Các Trait Đặc Biệt (S, SS, SSS)
            list.Add(CreateTrait("D_08", "trait_D_08_name", "trait_D_08_desc", Trait.RarityRank.D, "MATURE_SPEED", "", TraitEffectType.SPECIAL));
            list.Add(CreateSpecialTrait("TR_ALL_S", "trait_TR_ALL_S_name", "trait_TR_ALL_S_desc", Trait.RarityRank.S, "ALL_STAT", 10, 10, 10, 10));
            list.Add(CreateSpecialTrait("TR_ALL_SS", "trait_TR_ALL_SS_name", "trait_TR_ALL_SS_desc", Trait.RarityRank.SS, "ALL_STAT", 25, 25, 25, 25));
            
            // Các Trait ảnh hưởng Hệ Thống (Mã ID GDD_01: S_04, SS_07, SSS_04)
            Trait twins = CreateTrait("S_04", "trait_S_04_name", "trait_S_04_desc", Trait.RarityRank.S, "TWINS", "", TraitEffectType.SPECIAL);
            list.Add(twins);

            Trait elite = CreateTrait("SS_07", "trait_SS_07_name", "trait_SS_07_desc", Trait.RarityRank.SS, "ELITE", "", TraitEffectType.SPECIAL);
            list.Add(elite);

            Trait geneSel = CreateTrait("SSS_04", "trait_SSS_04_name", "trait_SSS_04_desc", Trait.RarityRank.SSS, "GENE_SELECTOR", "", TraitEffectType.SPECIAL);
            list.Add(geneSel);

            foreach (var t in list) EditorUtility.SetDirty(t);
            return list;
        }

        private static List<Skill> GenerateSkills()
        {
            List<Skill> list = new List<Skill>();

            // --- WARRIOR (Chiến binh) ---
            // Nhánh Sát Thương (Berserker)
            list.Add(CreateSkill("SK_WAR_DMG_1", "skill_SK_WAR_DMG_1_name", "skill_SK_WAR_DMG_1_desc", HeroClass.Warrior, TargetingType.SingleFrontEnemy, 1.5f, 2));
            list.Add(CreateSkill("SK_WAR_DMG_2", "skill_SK_WAR_DMG_2_name", "skill_SK_WAR_DMG_2_desc", HeroClass.Warrior, TargetingType.SingleFrontEnemy, 2.5f, 3));
            
            // Nhánh Khiên Thịt (Meat Shield)
            list.Add(CreateSkill("SK_WAR_TANK_1", "skill_SK_WAR_TANK_1_name", "skill_SK_WAR_TANK_1_desc", HeroClass.Warrior, TargetingType.AllEnemies, 0.8f, 3, StatusEffectType.DefDown, 1.0f));
            list.Add(CreateSkill("SK_WAR_TANK_2", "skill_SK_WAR_TANK_2_name", "skill_SK_WAR_TANK_2_desc", HeroClass.Warrior, TargetingType.Self, 0f, 4, StatusEffectType.Shield, 1.0f));

            // --- ARCHER (Cung Thủ) ---
            // Nhánh Hỏa (Burn)
            list.Add(CreateSkill("SK_ARC_FIRE_1", "skill_SK_ARC_FIRE_1_name", "skill_SK_ARC_FIRE_1_desc", HeroClass.Archer, TargetingType.RandomEnemy, 1.2f, 2, StatusEffectType.Poison, 0.8f));
            list.Add(CreateSkill("SK_ARC_FIRE_2", "skill_SK_ARC_FIRE_2_name", "skill_SK_ARC_FIRE_2_desc", HeroClass.Archer, TargetingType.RandomEnemy, 0.9f, 3, StatusEffectType.Poison, 0.5f, 3));

            // Nhánh Băng (Slow)
            list.Add(CreateSkill("SK_ARC_ICE_1", "skill_SK_ARC_ICE_1_name", "skill_SK_ARC_ICE_1_desc", HeroClass.Archer, TargetingType.SingleBackEnemy, 1.3f, 2, StatusEffectType.Slow, 1.0f));

            // Nhánh Cây (Poison)
            list.Add(CreateSkill("SK_ARC_NATURE_1", "skill_SK_ARC_NATURE_1_name", "skill_SK_ARC_NATURE_1_desc", HeroClass.Archer, TargetingType.RandomEnemy, 1.1f, 3, StatusEffectType.Poison, 1.0f));

            // --- MAGE (Pháp Sư) ---
            // Băng (AOE Slow)
            list.Add(CreateSkill("SK_MAG_ICE_1", "skill_SK_MAG_ICE_1_name", "skill_SK_MAG_ICE_1_desc", HeroClass.Mage, TargetingType.AllEnemies, 0.7f, 4, StatusEffectType.Slow, 0.6f));
            
            // Hỏa (AOE Burn)
            list.Add(CreateSkill("SK_MAG_FIRE_1", "skill_SK_MAG_FIRE_1_name", "skill_SK_MAG_FIRE_1_desc", HeroClass.Mage, TargetingType.AllEnemies, 1.0f, 5, StatusEffectType.Poison, 0.4f));

            // --- HEALER (Trị Liệu) ---
            // Hồi Đơn
            list.Add(CreateSkill("SK_HEA_SINGLE_1", "skill_SK_HEA_SINGLE_1_name", "skill_SK_HEA_SINGLE_1_desc", HeroClass.Healer, TargetingType.LowestHpAlly, 2.0f, 2));
            
            // Hồi AOE
            list.Add(CreateSkill("SK_HEA_AOE_1", "skill_SK_HEA_AOE_1_name", "skill_SK_HEA_AOE_1_desc", HeroClass.Healer, TargetingType.AllAllies, 0.8f, 4));
            
            // Giải Xấu/Buff
            list.Add(CreateSkill("SK_HEA_BUFF_1", "skill_SK_HEA_BUFF_1_name", "skill_SK_HEA_BUFF_1_desc", HeroClass.Healer, TargetingType.AllAllies, 0.5f, 3, StatusEffectType.CritUp, 1.0f));

            foreach (var s in list) EditorUtility.SetDirty(s);
            return list;
        }

        private static List<ItemData> GenerateItems()
        {
            List<ItemData> list = new List<ItemData>();

            // 1. Thuốc Tăng Giới hạn Sinh sản
            var it1 = CreateItem("IT_FERTILITY_POTION", "item_IT_FERTILITY_POTION_name", "item_IT_FERTILITY_POTION_desc", ItemType.Consumable, 1000);
            
            // 2. Thuốc Tăng Tốc độ
            var it2 = CreateItem("IT_SPEEDUP_1H", "item_IT_SPEEDUP_1H_name", "item_IT_SPEEDUP_1H_desc", ItemType.SpeedUp, 100, 3600);
            var it3 = CreateItem("IT_SPEEDUP_8H", "item_IT_SPEEDUP_8H_name", "item_IT_SPEEDUP_8H_desc", ItemType.SpeedUp, 500, 28800);
            
            // 3. Thuốc Đột Biến Gen
            var it4 = CreateItem("ITEM_MUTATION_POTION", "item_ITEM_MUTATION_POTION_name", "item_ITEM_MUTATION_POTION_desc", ItemType.BreedingMaterial, 2000);
            
            // 4. Thuốc Trưởng Thành Nhanh
            var it5 = CreateItem("IT_MATURATION_POTION", "item_IT_MATURATION_POTION_name", "item_IT_MATURATION_POTION_desc", ItemType.SpeedUp, 2000, 999999);

            // 5. Thẻ EXP
            var it6 = CreateItem("IT_EXP_BOOK_S", "item_IT_EXP_BOOK_S_name", "item_IT_EXP_BOOK_S_desc", ItemType.Consumable, 500);

            // 6. Bùa Ước Nguyện
            var it7 = CreateItem("IT_WISH_CHARM", "item_IT_WISH_CHARM_name", "item_IT_WISH_CHARM_desc", ItemType.BreedingMaterial, 3000);

            // 7. Vé Chiêu Mộ
            var it8 = CreateItem("IT_GACHA_TICKET", "item_IT_GACHA_TICKET_name", "item_IT_GACHA_TICKET_desc", ItemType.Consumable, 100);

            list.Add(it1); list.Add(it2); list.Add(it3); list.Add(it4); list.Add(it5); list.Add(it6); list.Add(it7); list.Add(it8);
            foreach (var it in list) EditorUtility.SetDirty(it);
            return list;
        }

        private static List<BuildingUpgradeData> GenerateBuildings()
        {
            List<BuildingUpgradeData> list = new List<BuildingUpgradeData>();
            string[] buildingTypes = { "Barracks", "Hospital", "BreedingPen" };

            foreach (var bType in buildingTypes)
            {
                BuildingUpgradeData data = GetOrCreateAsset<BuildingUpgradeData>($"{dataPath}/BLD_{bType}.asset");
                data.buildingId = bType;
                data.levels = new List<BuildingLevelData>();

                for (int level = 1; level <= 20; level++)
                {
                    // Công thức thời gian theo yêu cầu:
                    // Lv 5 ~ 1h (3600s), Lv 13 ~ 10h (36000s), Lv 20 ~ 1 Tuần (604800s)
                    long duration = 0;
                    if (level == 1) duration = 0;
                    else if (level == 2) duration = 60;
                    else if (level <= 5) duration = (long)Mathf.Lerp(60, 3600, (level - 2) / 3f);
                    else if (level <= 13) duration = (long)Mathf.Lerp(3600, 36000, (level - 5) / 8f);
                    else duration = (long)Mathf.Lerp(36000, 604800, (level - 13) / 7f);

                    int costGold = level * 100;
                    int costWood = level * 50;
                    int costStone = level * 25;

                    List<UpgradeCost> costs = new List<UpgradeCost>
                    {
                        new UpgradeCost { resourceId = "Gold", amount = costGold },
                        new UpgradeCost { resourceId = "Wood", amount = costWood },
                        new UpgradeCost { resourceId = "Stone", amount = costStone }
                    };

                    data.levels.Add(new BuildingLevelData
                    {
                        level = level,
                        duration = duration,
                        costs = (level == 1) ? new List<UpgradeCost>() : costs
                    });
                }

                list.Add(data);
                EditorUtility.SetDirty(data);
            }
            return list;
        }

        private static List<QuestData> GenerateQuests()
        {
            List<QuestData> list = new List<QuestData>();

            list.Add(CreateQuest("Q_T_01", "quest_Q_T_01_name", "quest_Q_T_01_desc", QuestCategory.Main, QuestType.CLEAR_POI, 1, new QuestReward { resourceId = "Gold", amount = 1000 }));
            list.Add(CreateQuest("Q_T_02", "quest_Q_T_02_name", "quest_Q_T_02_desc", QuestCategory.Main, QuestType.BREED_HERO, 1, new QuestReward { resourceId = "Wood", amount = 1000 }));
            list.Add(CreateQuest("Q_T_03", "quest_Q_T_03_name", "quest_Q_T_03_desc", QuestCategory.Main, QuestType.UPGRADE_BUILDING, 2, new QuestReward { resourceId = "Gold", amount = 500 }, "Barracks"));
            list.Add(CreateQuest("Q_T_04", "quest_Q_T_04_name", "quest_Q_T_04_desc", QuestCategory.Main, QuestType.UPGRADE_BUILDING, 2, new QuestReward { resourceId = "Stone", amount = 500 }, "Hospital"));

            // Daily Quests
            list.Add(CreateQuest("Q_D_01", "quest_Q_D_01_name", "quest_Q_D_01_desc", QuestCategory.Daily, QuestType.CLEAR_POI, 3, new QuestReward { resourceId = "Diamond", amount = 20 }));
            list.Add(CreateQuest("Q_D_02", "quest_Q_D_02_name", "quest_Q_D_02_desc", QuestCategory.Daily, QuestType.BREED_HERO, 1, new QuestReward { resourceId = "Diamond", amount = 20 }));
            
            // Weekly Quests
            list.Add(CreateQuest("Q_W_01", "quest_Q_W_01_name", "quest_Q_W_01_desc", QuestCategory.Weekly, QuestType.RECRUIT_HERO, 10, new QuestReward { resourceId = "Diamond", amount = 100 }));

            foreach (var q in list) EditorUtility.SetDirty(q);
            return list;
        }

        private static List<ExperienceData> GenerateExperienceTable()
        {
            List<ExperienceData> expList = new List<ExperienceData>();
            int baseExp = 100;
            for (int lvl = 1; lvl <= 100; lvl++)
            {
                expList.Add(new ExperienceData { level = lvl, experienceRequired = baseExp });
                // Tăng 10% exp mỗi level
                baseExp = Mathf.FloorToInt(baseExp * 1.1f);
            }
            return expList;
        }

        private static EvolutionTableData GenerateEvolutionTable()
        {
            EvolutionTableData evo = new EvolutionTableData();
            evo.rewards = new List<LegendOfBlood.GameConfigs.EvolutionRewardData>();
            evo.rewards.Add(new LegendOfBlood.GameConfigs.EvolutionRewardData { level = 30, description = "evo_reward_30" });
            evo.rewards.Add(new LegendOfBlood.GameConfigs.EvolutionRewardData { level = 50, description = "evo_reward_50" });
            evo.rewards.Add(new LegendOfBlood.GameConfigs.EvolutionRewardData { level = 70, description = "evo_reward_70" });
            evo.rewards.Add(new LegendOfBlood.GameConfigs.EvolutionRewardData { level = 100, description = "evo_reward_100" });
            return evo;
        }

        private static POIMonsterConfig GeneratePOIMonsterConfig()
        {
            POIMonsterConfig config = ScriptableObject.CreateInstance<POIMonsterConfig>();
            config.monsterGroups = new List<POIMonsterGroup>();

            // Easy
            config.monsterGroups.Add(new POIMonsterGroup
            {
                groupName = "poi_group_Mud_Monster_Camp",
                minDifficulty = 1,
                maxDifficulty = 3,
                monsterIDs = new List<string> { "Goblin", "Slime" }
            });

            // Medium
            config.monsterGroups.Add(new POIMonsterGroup
            {
                groupName = "poi_group_Orc_Barracks",
                minDifficulty = 4,
                maxDifficulty = 7,
                monsterIDs = new List<string> { "Orc Warrior", "Orc Shaman", "Troll" }
            });

            // Hard
            config.monsterGroups.Add(new POIMonsterGroup
            {
                groupName = "poi_group_Black_Dragon_Tomb",
                minDifficulty = 8,
                maxDifficulty = 10,
                monsterIDs = new List<string> { "Elder Dragon", "Dragon Whelp", "Fire Elemental", "Golem" }
            });

            AssetDatabase.CreateAsset(config, $"{dataPath}/POIMonsterConfig.asset");
            EditorUtility.SetDirty(config);
            return config;
        }

        private static List<RarityConfig> GenerateRaritySettings()
        {
            List<RarityConfig> list = new List<RarityConfig>
            {
                new RarityConfig { rank = Trait.RarityRank.D, dropChance = 40f, minPotential = 1, maxPotential = 5 },
                new RarityConfig { rank = Trait.RarityRank.C, dropChance = 30f, minPotential = 5, maxPotential = 9 },
                new RarityConfig { rank = Trait.RarityRank.B, dropChance = 15f, minPotential = 8, maxPotential = 12 },
                new RarityConfig { rank = Trait.RarityRank.A, dropChance = 10f, minPotential = 12, maxPotential = 16 },
                new RarityConfig { rank = Trait.RarityRank.S, dropChance = 3.5f, minPotential = 15, maxPotential = 18 },
                new RarityConfig { rank = Trait.RarityRank.SS, dropChance = 1.0f, minPotential = 18, maxPotential = 20 },
                new RarityConfig { rank = Trait.RarityRank.SSS, dropChance = 0.5f, minPotential = 19, maxPotential = 20 }
            };
            return list;
        }

        private static List<BossData> GenerateBossData()
        {
            List<BossData> list = new List<BossData>();

            // Vua Thỏ Khổng Lồ (Beginner Boss)
            list.Add(CreateBoss("BOSS_RABBIT_KING", "boss_BOSS_RABBIT_KING_name", 10, 5000, 200, 50, 150, "boss_BOSS_RABBIT_KING_atk1", 1.2f, "boss_BOSS_RABBIT_KING_atk2", 0.6f, "boss_BOSS_RABBIT_KING_mech_name", "boss_BOSS_RABBIT_KING_mech_desc", 50));
            
            // Gà Tây Phẫn Nộ (Mid Boss)
            list.Add(CreateBoss("BOSS_ANGRY_TURKEY", "boss_BOSS_ANGRY_TURKEY_name", 25, 25000, 800, 300, 250, "boss_BOSS_ANGRY_TURKEY_atk1", 1.5f, "boss_BOSS_ANGRY_TURKEY_atk2", 0.8f, "boss_BOSS_ANGRY_TURKEY_mech_name", "boss_BOSS_ANGRY_TURKEY_mech_desc", 30));
            
            // Sói Chóp Bu (Late Boss)
            list.Add(CreateBoss("BOSS_ALPHA_WOLF", "boss_BOSS_ALPHA_WOLF_name", 50, 120000, 3500, 1500, 600, "boss_BOSS_ALPHA_WOLF_atk1", 2.0f, "boss_BOSS_ALPHA_WOLF_atk2", 1.2f, "boss_BOSS_ALPHA_WOLF_mech_name", "boss_BOSS_ALPHA_WOLF_mech_desc", 5000));

            return list;
        }

        private static List<MonsterData> GenerateMonsters()
        {
            List<MonsterData> list = new List<MonsterData>();

            // The IDs here MUST MATCH the ones used in GeneratePOIMonsterConfig & Tower configs
            list.Add(CreateMonster("Slime", "monster_Slime_name", Profession.Warrior, 150f, 10f, 5f, 5f));
            list.Add(CreateMonster("Goblin", "monster_Goblin_name", Profession.Archer, 120f, 15f, 2f, 15f));
            
            list.Add(CreateMonster("Orc Warrior", "monster_Orc_Warrior_name", Profession.Warrior, 400f, 35f, 25f, 12f));
            list.Add(CreateMonster("Orc Shaman", "monster_Orc_Shaman_name", Profession.Mage, 250f, 50f, 10f, 10f));
            list.Add(CreateMonster("Troll", "monster_Troll_name", Profession.Warrior, 800f, 25f, 40f, 8f));

            list.Add(CreateMonster("Fire Elemental", "monster_Fire_Elemental_name", Profession.Mage, 600f, 80f, 30f, 20f));
            list.Add(CreateMonster("Golem", "monster_Golem_name", Profession.Warrior, 1500f, 40f, 100f, 5f));
            list.Add(CreateMonster("Dragon Whelp", "monster_Dragon_Whelp_name", Profession.Archer, 1000f, 100f, 50f, 30f));
            list.Add(CreateMonster("Elder Dragon", "monster_Elder_Dragon_name", Profession.Mage, 3000f, 200f, 150f, 25f));

            return list;
        }

        private static BossData CreateBoss(string id, string name, int level, int hp, int atk, int def, int spd, string normalAtkName, float normalAtkMult, string aoeAtkName, float aoeAtkMult, string mechName, string mechDesc, int mechValue)
        {
            BossData b = GetOrCreateAsset<BossData>($"{dataPath}/{id}.asset");
            b.id = id; b.bossName = name; b.level = level;
            b.baseHp = hp; b.baseAtk = atk; b.baseDef = def; b.baseSpd = spd; // Gán đúng Base thay vì tạo mới stats
            b.stats = new HeroStats { hp = hp, atk = atk, def = def, spd = spd, critChance = 0.05f, critDamage = 1.5f };
            b.skills = new BossSkills { normalAttackName = normalAtkName, normalAttackMultiplier = normalAtkMult, aoeAttackName = aoeAtkName, aoeAttackMultiplier = aoeAtkMult };
            b.mechanic = new BossMechanic { name = mechName, description = mechDesc, value = mechValue };
            EditorUtility.SetDirty(b);
            return b;
        }

        private static MonsterData CreateMonster(string id, string name, Profession prof, float hp, float atk, float def, float spd)
        {
            MonsterData m = GetOrCreateAsset<MonsterData>($"{dataPath}/MON_{id.Replace(" ", "_")}.asset");
            m.id = id;
            m.monsterName = name;
            m.profession = prof;
            m.baseHp = hp;
            m.baseAtk = atk;
            m.baseDef = def;
            m.baseSpd = spd;
            EditorUtility.SetDirty(m);
            return m;
        }

        private static List<TowerFloorConfig> GenerateTowerConfigs()
        {
            List<TowerFloorConfig> list = new List<TowerFloorConfig>();
            
            // We use some existing monster IDs to populate the tower.
            // Ideally this would match MonsterData generated elsewhere, but for now we use POI monsters
            var lowTierMonsters = new List<string> { "Goblin", "Slime" };
            var midTierMonsters = new List<string> { "Orc Warrior", "Orc Shaman", "Troll" };
            var highTierMonsters = new List<string> { "Elder Dragon", "Dragon Whelp", "Fire Elemental", "Golem" };

            for (int i = 1; i <= 20; i++)
            {
                var floorCfg = new TowerFloorConfig();
                floorCfg.floorIndex = i;
                
                if (i <= 5)
                    floorCfg.monsterPool = lowTierMonsters;
                else if (i <= 15)
                    floorCfg.monsterPool = midTierMonsters;
                else
                    floorCfg.monsterPool = highTierMonsters;

                // Tầng boss (mỗi 5 tầng), số lượng ít nhưng quái mạnh
                if (i % 5 == 0)
                {
                    floorCfg.customMonsterCount = 1;
                    // Add a boss if possible, or just keep it as 1 strong monster
                }
                else
                {
                    // Quái thường tăng dần theo tầng (công thức cũ là 1 + floor/5)
                    floorCfg.customMonsterCount = 0; // Let the dynamic logic in ExpeditionManager handle it
                }

                list.Add(floorCfg);
            }
            return list;
        }

        private static void UpdateGameConfig(List<Trait> traits, List<Skill> skills, List<ItemData> items, List<BuildingUpgradeData> buildings, List<QuestData> quests, List<ExperienceData> expTable, EvolutionTableData evoTable, POIMonsterConfig poiConfig, List<RarityConfig> rarityConfig, List<MonsterData> monsters, List<BossData> bosses, List<TowerFloorConfig> towerConfigs)
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
                    config.ExperienceTable = expTable;
                    config.EvolutionTable = evoTable;
                    config.POIMonsterConfig = poiConfig;
                    config.RaritySettings = rarityConfig;
                    config.AllMonsters = monsters; // NEW
                    config.AllBosses = bosses;
                    config.TowerConfigs = towerConfigs;

                    config.StartingSkills = new List<ProfessionStartingSkills>
                    {
                        new ProfessionStartingSkills { profession = Profession.Warrior, startingSkillIDs = new List<string> { "SK_WAR_DMG_1" } },
                        new ProfessionStartingSkills { profession = Profession.Archer, startingSkillIDs = new List<string> { "SK_ARC_FIRE_1" } },
                        new ProfessionStartingSkills { profession = Profession.Mage, startingSkillIDs = new List<string> { "SK_MAG_ICE_1" } },
                        new ProfessionStartingSkills { profession = Profession.Healer, startingSkillIDs = new List<string> { "SK_HEA_SINGLE_1" } }
                    };

                    EditorUtility.SetDirty(config);
                    AssetDatabase.SaveAssets();
                    Debug.Log("<color=cyan>[GameDataGenerator]</color> Đã nạp thành công bộ MASTER DATA Khổng Lồ vào GameConfig!");
                }
            }
        }

        // --- Helper Methods to Create Types ---
        private static Trait CreateTrait(string id, string name, string desc, Trait.RarityRank rank, string family, string nextUp, TraitEffectType tType, int hp=0, int atk=0, int def=0, int spd=0)
        {
            Trait t = GetOrCreateAsset<Trait>($"{dataPath}/{id}.asset");
            t.id = id; t.traitName = name; t.description = desc; t.rank = rank; t.familyId = family; t.nextUpgradeTraitID = nextUp;
            t.effects = new List<TraitEffect> { new TraitEffect { type = tType, hp = hp, atk = atk, def = def, spd = spd } };
            return t;
        }

        private static Trait CreateSpecialTrait(string id, string name, string desc, Trait.RarityRank rank, string family, int mhp, int matk, int mdef, int mspd)
        {
            Trait t = GetOrCreateAsset<Trait>($"{dataPath}/{id}.asset");
            t.id = id; t.traitName = name; t.description = desc; t.rank = rank; t.familyId = family; 
            t.effects = new List<TraitEffect> { new TraitEffect { type = TraitEffectType.MULTIPLY_STAT, hp = mhp, atk = matk, def = mdef, spd = mspd } };
            return t;
        }

        private static Skill CreateSkill(string id, string name, string desc, HeroClass hc, TargetingType tgt, float pRatio, int cd, StatusEffectType? eff = null, float effChance = 0f, int hitCount = 1)
        {
            Skill s = GetOrCreateAsset<Skill>($"{dataPath}/{id}.asset");
            s.id = id; s.skillName = name; s.description = desc; s.requiredProfession = hc; s.targeting = tgt; 
            s.powerRatio = pRatio; s.cooldown = cd; s.type = SkillType.Active; s.hitCount = hitCount;
            if (eff.HasValue) { s.appliedEffect = eff.Value; s.effectDuration = 3; s.effectChance = effChance; }
            return s;
        }

        private static ItemData CreateItem(string id, string name, string desc, ItemType t, int price, int speedVal = 0)
        {
            ItemData i = GetOrCreateAsset<ItemData>($"{dataPath}/{id}.asset");
            i.id = id; i.itemName = name; i.description = desc; i.type = t; i.sellPrice = price; i.isStackable = true;
            i.speedUpValueInSeconds = speedVal;
            return i;
        }

        private static QuestData CreateQuest(string id, string name, string desc, QuestCategory cat, QuestType qty, int tgVal, QuestReward rew, string tgtId = "")
        {
            QuestData q = GetOrCreateAsset<QuestData>($"{dataPath}/{id}.asset");
            q.questId = id; q.questName = name; q.description = desc; q.category = cat; q.type = qty; 
            q.targetValue = tgVal; q.targetId = tgtId; q.rewards = new List<QuestReward> { rew };
            return q;
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

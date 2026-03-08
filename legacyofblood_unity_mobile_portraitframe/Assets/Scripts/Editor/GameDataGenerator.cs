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
            var bosses = GenerateBossData();
            var towerConfigs = GenerateTowerConfigs();

            UpdateGameConfig(traits, skills, items, buildings, quests, expTable, evoTable, poiConfig, rarityConfig, bosses, towerConfigs);
        }

        private static List<Trait> GenerateTraits()
        {
            List<Trait> list = new List<Trait>();

            // Hệ ATK
            list.Add(CreateTrait("TR_ATK_D", "Sức Mạnh Tân Binh", "Tham gia quân ngũ. Lực Tay yếu +5 ATK.", Trait.RarityRank.D, "ATK_UP", "TR_ATK_C", TraitEffectType.ADD_STAT, atk: 5));
            list.Add(CreateTrait("TR_ATK_C", "Sức Mạnh Chiến Binh", "Rèn luyện qua năm tháng. +15 ATK.", Trait.RarityRank.C, "ATK_UP", "TR_ATK_B", TraitEffectType.ADD_STAT, atk: 15));
            list.Add(CreateTrait("TR_ATK_B", "Tay To Hơn Não", "Bắp tay to bự. +35 ATK.", Trait.RarityRank.B, "ATK_UP", "TR_ATK_A", TraitEffectType.ADD_STAT, atk: 35));
            list.Add(CreateTrait("TR_ATK_A", "Uy Lực Cự Thạch", "Đập vỡ đá chẻ núi. +100 ATK.", Trait.RarityRank.A, "ATK_UP", "TR_ATK_S", TraitEffectType.ADD_STAT, atk: 100));
            list.Add(CreateTrait("TR_ATK_S", "Sức Mạnh Cự Thần", "Gen đột biến Cự Thần. Tăng 20% ATK.", Trait.RarityRank.S, "ATK_UP", "TR_ATK_SS", TraitEffectType.MULTIPLY_STAT, atk: 20));
            list.Add(CreateTrait("TR_ATK_SS", "Quyền Vô Cực", "Tuyệt thế Vô Cực. Tăng 50% ATK.", Trait.RarityRank.SS, "ATK_UP", "", TraitEffectType.MULTIPLY_STAT, atk: 50));

            // Hệ HP
            list.Add(CreateTrait("TR_HP_D", "Thể Chất Bình Thường", "+20 HP tối đa.", Trait.RarityRank.D, "HP_UP", "TR_HP_C", TraitEffectType.ADD_STAT, hp: 20));
            list.Add(CreateTrait("TR_HP_C", "Thể Lực Dồi Dào", "+80 HP tối đa.", Trait.RarityRank.C, "HP_UP", "TR_HP_B", TraitEffectType.ADD_STAT, hp: 80));
            list.Add(CreateTrait("TR_HP_B", "Thân Thể Cường Tráng", "+200 HP tối đa.", Trait.RarityRank.B, "HP_UP", "TR_HP_A", TraitEffectType.ADD_STAT, hp: 200));
            list.Add(CreateTrait("TR_HP_A", "Máu Trâu Bò", "+500 HP tối đa.", Trait.RarityRank.A, "HP_UP", "TR_HP_S", TraitEffectType.ADD_STAT, hp: 500));
            list.Add(CreateTrait("TR_HP_S", "Sinh Mệnh Cây Thần", "Tăng 20% HP.", Trait.RarityRank.S, "HP_UP", "TR_HP_SS", TraitEffectType.MULTIPLY_STAT, hp: 20));
            list.Add(CreateTrait("TR_HP_SS", "Bất Tử Ma Cốt", "Tăng khủng khiếp 50% HP.", Trait.RarityRank.SS, "HP_UP", "", TraitEffectType.MULTIPLY_STAT, hp: 50));

            // Hệ DEF
            list.Add(CreateTrait("TR_DEF_D", "Da Dày Mỏng", "+2 DEF.", Trait.RarityRank.D, "DEF_UP", "TR_DEF_C", TraitEffectType.ADD_STAT, def: 2));
            list.Add(CreateTrait("TR_DEF_C", "Da Cỏ Đồng", "+10 DEF.", Trait.RarityRank.C, "DEF_UP", "TR_DEF_B", TraitEffectType.ADD_STAT, def: 10));
            list.Add(CreateTrait("TR_DEF_B", "Da Thạch Trùng", "+25 DEF.", Trait.RarityRank.B, "DEF_UP", "TR_DEF_A", TraitEffectType.ADD_STAT, def: 25));
            list.Add(CreateTrait("TR_DEF_A", "Da Thiết Giáp", "+60 DEF.", Trait.RarityRank.A, "DEF_UP", "TR_DEF_S", TraitEffectType.ADD_STAT, def: 60));
            list.Add(CreateTrait("TR_DEF_S", "Kim Cang Bất Hoại", "Tăng 20% DEF.", Trait.RarityRank.S, "DEF_UP", "TR_DEF_SS", TraitEffectType.MULTIPLY_STAT, def: 20));

            // Hệ SPD
            list.Add(CreateTrait("TR_SPD_D", "Bước Chân Vội Vã", "+2 SPD.", Trait.RarityRank.D, "SPD_UP", "TR_SPD_C", TraitEffectType.ADD_STAT, spd: 2));
            list.Add(CreateTrait("TR_SPD_C", "Bước Chân Lì Lợm", "+5 SPD.", Trait.RarityRank.C, "SPD_UP", "TR_SPD_B", TraitEffectType.ADD_STAT, spd: 5));
            list.Add(CreateTrait("TR_SPD_B", "Nhanh Như Gió", "+12 SPD.", Trait.RarityRank.B, "SPD_UP", "TR_SPD_A", TraitEffectType.ADD_STAT, spd: 12));
            list.Add(CreateTrait("TR_SPD_A", "Thân Pháp Quỷ Ảnh", "+30 SPD.", Trait.RarityRank.A, "SPD_UP", "TR_SPD_S", TraitEffectType.ADD_STAT, spd: 30));
            list.Add(CreateTrait("TR_SPD_S", "Tốc Biến", "Tăng 20% SPD.", Trait.RarityRank.S, "SPD_UP", "TR_SPD_SS", TraitEffectType.MULTIPLY_STAT, spd: 20));

            // Các Trait Đặc Biệt (S, SS, SSS)
            list.Add(CreateTrait("D_08", "Lớn Nhanh", "Giảm 20% thời gian trưởng thành.", Trait.RarityRank.D, "MATURE_SPEED", "", TraitEffectType.SPECIAL));
            list.Add(CreateSpecialTrait("TR_ALL_S", "Hoàng Gia Chi Huyết", "Là con cháu vương tôn. Tăng đồng đều 10% các chỉ số.", Trait.RarityRank.S, "ALL_STAT", 10, 10, 10, 10));
            list.Add(CreateSpecialTrait("TR_ALL_SS", "Long Tộc Hậu Duệ", "Huyết mạch loài Rồng sục sôi. Tăng đồng đều 25% các chỉ số.", Trait.RarityRank.SS, "ALL_STAT", 25, 25, 25, 25));
            
            // Các Trait ảnh hưởng Hệ Thống (Mã ID GDD_01: S_04, SS_07, SSS_04)
            Trait twins = CreateTrait("S_04", "Song Sinh Tiên Tri", "Tỉ lệ nhỏ (2%) sinh ra anh em sinh đôi khi lai tạo.", Trait.RarityRank.S, "TWINS", "", TraitEffectType.SPECIAL);
            list.Add(twins);

            Trait elite = CreateTrait("SS_07", "Dòng Dõi Tinh Anh", "Gen xuất chúng. Con cháu lai tạo có 10% cơ hội tăng 5% toàn bộ chỉ số cơ bản.", Trait.RarityRank.SS, "ELITE", "", TraitEffectType.SPECIAL);
            list.Add(elite);

            Trait geneSel = CreateTrait("SSS_04", "Kẻ Chọn Lọc Gene", "Thống trị chọn lọc. Có thể chủ động giữ lại 1 Trait chỉ định khi lai tạo.", Trait.RarityRank.SSS, "GENE_SELECTOR", "", TraitEffectType.SPECIAL);
            list.Add(geneSel);

            foreach (var t in list) EditorUtility.SetDirty(t);
            return list;
        }

        private static List<Skill> GenerateSkills()
        {
            List<Skill> list = new List<Skill>();

            // --- WARRIOR (Chiến binh) ---
            // Nhánh Sát Thương (Berserker)
            list.Add(CreateSkill("SK_WAR_DMG_1", "Trảm Kích", "Tấn công hàng trước cường lực.", HeroClass.Warrior, TargetingType.SingleFrontEnemy, 1.5f, 2));
            list.Add(CreateSkill("SK_WAR_DMG_2", "Cuồng Nộ Huyết", "Bổ đôi kẻ thù, sát thương trí mạng.", HeroClass.Warrior, TargetingType.SingleFrontEnemy, 2.5f, 3));
            
            // Nhánh Khiên Thịt (Meat Shield)
            list.Add(CreateSkill("SK_WAR_TANK_1", "Gồng Mình", "Tấn công diện rộng và tạo giảm thủ kẻ địch.", HeroClass.Warrior, TargetingType.AllEnemies, 0.8f, 3, StatusEffectType.DefDown, 1.0f));
            list.Add(CreateSkill("SK_WAR_TANK_2", "Khiên Ảo", "Tạo lớp khiên bảo vệ cho bản thân.", HeroClass.Warrior, TargetingType.Self, 0f, 4, StatusEffectType.Shield, 1.0f));

            // --- ARCHER (Cung Thủ) ---
            // Nhánh Hỏa (Burn)
            list.Add(CreateSkill("SK_ARC_FIRE_1", "Tiễn Hỏa", "Bắn mũi tên rực lửa, gây độc(Burn).", HeroClass.Archer, TargetingType.RandomEnemy, 1.2f, 2, StatusEffectType.Poison, 0.8f));
            list.Add(CreateSkill("SK_ARC_FIRE_2", "Mưa Hỏa Tiễn", "Bắn 3 mục tiêu ngẫu nhiên, tỉ lệ thiêu đốt cao.", HeroClass.Archer, TargetingType.RandomEnemy, 0.9f, 3, StatusEffectType.Poison, 0.5f, 3));

            // Nhánh Băng (Slow)
            list.Add(CreateSkill("SK_ARC_ICE_1", "Tiễn Băng", "Mũi tên lạnh giá làm chậm kẻ địch.", HeroClass.Archer, TargetingType.SingleBackEnemy, 1.3f, 2, StatusEffectType.Slow, 1.0f));

            // Nhánh Cây (Poison)
            list.Add(CreateSkill("SK_ARC_NATURE_1", "Mũi Tên Độc", "Bắn mũi tên tẩm độc gây sát thương theo thời gian.", HeroClass.Archer, TargetingType.RandomEnemy, 1.1f, 3, StatusEffectType.Poison, 1.0f));

            // --- MAGE (Pháp Sư) ---
            // Băng (AOE Slow)
            list.Add(CreateSkill("SK_MAG_ICE_1", "Bão Tuyết", "Triệu hồi bão tuyết đóng băng toàn bộ kẻ địch.", HeroClass.Mage, TargetingType.AllEnemies, 0.7f, 4, StatusEffectType.Slow, 0.6f));
            
            // Hỏa (AOE Burn)
            list.Add(CreateSkill("SK_MAG_FIRE_1", "Thiên Thạch", "Giáng quả cầu lửa khổng lồ, thiêu đốt địch.", HeroClass.Mage, TargetingType.AllEnemies, 1.0f, 5, StatusEffectType.Poison, 0.4f));

            // --- HEALER (Trị Liệu) ---
            // Hồi Đơn
            list.Add(CreateSkill("SK_HEA_SINGLE_1", "Hồi Phục", "Chữa trị vết thương cho đồng minh yếu nhất.", HeroClass.Healer, TargetingType.LowestHpAlly, 2.0f, 2));
            
            // Hồi AOE
            list.Add(CreateSkill("SK_HEA_AOE_1", "Thân Lạc Dược", "Làn gió mát hồi máu toàn đội Hình.", HeroClass.Healer, TargetingType.AllAllies, 0.8f, 4));
            
            // Giải Xấu/Buff
            list.Add(CreateSkill("SK_HEA_BUFF_1", "Thánh Quang", "Ban phước làm tăng trí mạng cho đồng đội.", HeroClass.Healer, TargetingType.AllAllies, 0.5f, 3, StatusEffectType.CritUp, 1.0f));

            foreach (var s in list) EditorUtility.SetDirty(s);
            return list;
        }

        private static List<ItemData> GenerateItems()
        {
            List<ItemData> list = new List<ItemData>();

            // 1. Thuốc Tăng Giới hạn Sinh sản
            var it1 = CreateItem("IT_FERTILITY_POTION", "Thuốc Mắn Đẻ", "Uống vào (+2) lượt sinh sản. Tối đa 20 lượt cho 1 Hero.", ItemType.Consumable, 1000);
            
            // 2. Thuốc Tăng Tốc độ
            var it2 = CreateItem("IT_SPEEDUP_1H", "Đồng Hồ 1H", "Giảm thời gian chờ đi 1 Tiếng.", ItemType.SpeedUp, 100, 3600);
            var it3 = CreateItem("IT_SPEEDUP_8H", "Đồng Hồ 8H", "Giảm thời gian chờ đi 8 Tiếng.", ItemType.SpeedUp, 500, 28800);
            
            // 3. Thuốc Đột Biến Gen
            var it4 = CreateItem("ITEM_MUTATION_POTION", "Thuốc Đột Biến SS", "Tăng mạnh tỉ lệ xuất hiện Trait hiếm khi tham gia Lai Tạo.", ItemType.BreedingMaterial, 2000);
            
            // 4. Thuốc Trưởng Thành Nhanh
            var it5 = CreateItem("IT_MATURATION_POTION", "Quả Trưởng Thành", "Giảm sốc thời gian chờ lớn lên của trẻ sơ sinh xuống 30 Phút.", ItemType.SpeedUp, 2000, 999999);

            // 5. Thẻ EXP
            var it6 = CreateItem("IT_EXP_BOOK_S", "Sách Giáo Khoa", "Cung cấp kinh nghiệm khi sử dụng. Rơi ngẫu nhiên.", ItemType.Consumable, 500);

            // 6. Bùa Ước Nguyện
            var it7 = CreateItem("IT_WISH_CHARM", "Bùa Ước Nguyện", "Tăng tỉ lệ sinh ra một nghề nghiệp mong muốn khi lai tạo.", ItemType.BreedingMaterial, 3000);

            list.Add(it1); list.Add(it2); list.Add(it3); list.Add(it4); list.Add(it5); list.Add(it6); list.Add(it7);
            foreach (var it in list) EditorUtility.SetDirty(it);
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

                    List<UpgradeCost> costs = new List<UpgradeCost>
                    {
                        new UpgradeCost { resourceId = "Gold", amount = costGold },
                        new UpgradeCost { resourceId = "Wood", amount = costWood }
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

            list.Add(CreateQuest("Q_T_01", "Chạm Trán Quái Vật", "Tiêu thụ Thể Lực khám phá POI 1 lần.", QuestCategory.Main, QuestType.CLEAR_POI, 1, new QuestReward { resourceId = "Gold", amount = 1000 }));
            list.Add(CreateQuest("Q_T_02", "Dòng Dõi Hoàng Gia", "Thực hiện Lai Tạo và có em bé đầu tay.", QuestCategory.Main, QuestType.BREED_HERO, 1, new QuestReward { resourceId = "Wood", amount = 1000 }));
            list.Add(CreateQuest("Q_T_03", "Cố Thủ Làng", "Nâng cấp Nhà Chính TownHall lên Cấp 2.", QuestCategory.Main, QuestType.UPGRADE_BUILDING, 2, new QuestReward { resourceId = "Gold", amount = 500 }, "TownHall"));
            list.Add(CreateQuest("Q_T_04", "Bệnh Viện Y Tế", "Nâng cấp Bệnh Viện lên Cấp 2.", QuestCategory.Main, QuestType.UPGRADE_BUILDING, 2, new QuestReward { resourceId = "Stone", amount = 500 }, "Hospital"));

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
            evo.rewards.Add(new LegendOfBlood.GameConfigs.EvolutionRewardData { level = 30, description = "Mở khóa Nhánh Nghề (Chọn 1 Trait hệ)" });
            evo.rewards.Add(new LegendOfBlood.GameConfigs.EvolutionRewardData { level = 50, description = "Nhận kỹ năng Active Level 2" });
            evo.rewards.Add(new LegendOfBlood.GameConfigs.EvolutionRewardData { level = 70, description = "Đột phá Bậc Hiếm Trait ngẫu nhiên" });
            evo.rewards.Add(new LegendOfBlood.GameConfigs.EvolutionRewardData { level = 100, description = "Hóa Thần! Chọn 1 Trait bậc S trở lên" });
            return evo;
        }

        private static POIMonsterConfig GeneratePOIMonsterConfig()
        {
            POIMonsterConfig config = ScriptableObject.CreateInstance<POIMonsterConfig>();
            config.monsterGroups = new List<POIMonsterGroup>();

            // Easy
            config.monsterGroups.Add(new POIMonsterGroup
            {
                groupName = "Bãi Quái Bùn",
                minDifficulty = 1,
                maxDifficulty = 3,
                monsterIDs = new List<string> { "Goblin", "Slime" }
            });

            // Medium
            config.monsterGroups.Add(new POIMonsterGroup
            {
                groupName = "Doanh Trại Orc",
                minDifficulty = 4,
                maxDifficulty = 7,
                monsterIDs = new List<string> { "Orc Warrior", "Orc Shaman", "Troll" }
            });

            // Hard
            config.monsterGroups.Add(new POIMonsterGroup
            {
                groupName = "Lăng Mộ Rồng Đen",
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
            list.Add(CreateBoss("BOSS_RABBIT_KING", "Vua Thỏ Khổng Lồ", 10, 5000, 200, 50, 150, "Cú Đá Sấm Sét", 1.2f, "Động Đất Mini", 0.6f, "Da Dày", "Giảm sát thương nhận vào nếu nhát chém dưới 50 ATK", 50));
            
            // Gà Tây Phẫn Nộ (Mid Boss)
            list.Add(CreateBoss("BOSS_ANGRY_TURKEY", "Gà Tây Phẫn Nộ", 25, 25000, 800, 300, 250, "Mổ Điên Cuồng", 1.5f, "Tiếng Gáy Đinh Tai", 0.8f, "Điên Cuồng", "Tăng mạnh tốc đánh khi máu dưới 30%", 30));
            
            // Sói Chóp Bu (Late Boss)
            list.Add(CreateBoss("BOSS_ALPHA_WOLF", "Sói Chóp Bu", 50, 120000, 3500, 1500, 600, "Cắn Xé", 2.0f, "Lang Quần Gọi bầy", 1.2f, "Tái Sinh Lỗi", "Hồi dòng máu lớn mỗi lượt", 5000));

            return list;
        }

        private static BossData CreateBoss(string id, string name, int level, int hp, int atk, int def, int spd, string normalAtkName, float normalAtkMult, string aoeAtkName, float aoeAtkMult, string mechName, string mechDesc, int mechValue)
        {
            BossData b = GetOrCreateAsset<BossData>($"{dataPath}/{id}.asset");
            b.id = id; b.bossName = name; b.level = level;
            b.stats = new HeroStats { hp = hp, atk = atk, def = def, spd = spd, critChance = 0.05f, critDamage = 1.5f };
            b.skills = new BossSkills { normalAttackName = normalAtkName, normalAttackMultiplier = normalAtkMult, aoeAttackName = aoeAtkName, aoeAttackMultiplier = aoeAtkMult };
            b.mechanic = new BossMechanic { name = mechName, description = mechDesc, value = mechValue };
            EditorUtility.SetDirty(b);
            return b;
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

        private static void UpdateGameConfig(List<Trait> traits, List<Skill> skills, List<ItemData> items, List<BuildingUpgradeData> buildings, List<QuestData> quests, List<ExperienceData> expTable, EvolutionTableData evoTable, POIMonsterConfig poiConfig, List<RarityConfig> rarityConfig, List<BossData> bosses, List<TowerFloorConfig> towerConfigs)
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

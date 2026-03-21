using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using LegendOfBlood.GameConfigs;

namespace LegendOfBlood.Editor
{
    public class GameDataExporterTool
    {
        [MenuItem("Tools/Legend Of Blood/Xuất Dữ Liệu Game Ra EXCEL (CSV)", false, 3)]
        public static void ExportDataToCSV()
        {
            string[] guids = AssetDatabase.FindAssets("t:GameConfig");
            if (guids == null || guids.Length == 0)
            {
                Debug.LogError($"[DataExporter] Không tìm thấy file GameConfig nào trong dự án! Đảm bảo bạn đã lưu GameConfig.asset.");
                return;
            }

            string configPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>(configPath);

            if (config == null)
            {
                Debug.LogError($"[DataExporter] Không thể load GameConfig tại đường dẫn: {configPath}");
                return;
            }

            string exportFolder = Application.dataPath + "/../GameData_Export";
            if (!Directory.Exists(exportFolder))
            {
                Directory.CreateDirectory(exportFolder);
            }

            ExportTraits(config.AllTraits, exportFolder);
            ExportSkills(config.AllSkills, exportFolder);
            ExportItems(config.AllItems, exportFolder);
            ExportBosses(config.AllBosses, exportFolder);
            ExportMonsters(config.AllMonsters, exportFolder); // NEW
            ExportBuildings(config.BuildingUpgradeDataList, exportFolder);
            ExportExperienceAndEvolution(config.ExperienceTable, config.EvolutionTable, exportFolder);
            ExportMonsterGroups(config.POIMonsterConfig, exportFolder);

            Debug.Log($"<color=cyan>[DataExporter] Đã xuất thành công toàn bộ Game Data ra thư mục: {exportFolder}</color>\nBạn có thể mở các file .csv bằng Excel hoặc Google Sheets.");

            
            // Tự động mở thư mục cho người dùng
            EditorUtility.RevealInFinder(exportFolder);
        }

        private static void SafeWriteCSV(string path, string content)
        {
            try
            {
                File.WriteAllText(path, "\uFEFF" + content, Encoding.UTF8);
            }
            catch (IOException e)
            {
                string fileName = Path.GetFileName(path);
                Debug.LogError($"[DataExporter] Lỗi khi ghi file {fileName}\nCó thể bạn đang mở file này trong Excel. Hãy đóng Excel và thử lại! Chi tiết: {e.Message}");
            }
        }

        private static void ExportTraits(List<Trait> traits, string folderPath)
        {
            if (traits == null || traits.Count == 0) return;

            StringBuilder sb = new StringBuilder();
            // Header (UTF-8 BOM is added on write for Excel Vietnamese support)
            sb.AppendLine("Trait ID,Tên Trait,Mô Tả,Độ Hiếm (Rarity),Phân Loại Nhóm,Chỉ Số Tác Động,ATK,HP,DEF,SPD");

            foreach (var t in traits)
            {
                string id = t.id ?? "";
                string name = $"\"{t.traitName}\""; // Quote to handle commas in text
                string desc = $"\"{t.description}\"";
                string rarity = t.rank.ToString();
                string group = t.familyId ?? "";
                string effect = (t.effects != null && t.effects.Count > 0) ? t.effects[0].type.ToString() : "";
                string t_atk = (t.effects != null && t.effects.Count > 0) ? t.effects[0].atk.ToString() : "0";
                string t_hp = (t.effects != null && t.effects.Count > 0) ? t.effects[0].hp.ToString() : "0";
                string t_def = (t.effects != null && t.effects.Count > 0) ? t.effects[0].def.ToString() : "0";
                string t_spd = (t.effects != null && t.effects.Count > 0) ? t.effects[0].spd.ToString() : "0";

                sb.AppendLine($"{id},{name},{desc},{rarity},{group},{effect},{t_atk},{t_hp},{t_def},{t_spd}");
            }

            SafeWriteCSV(Path.Combine(folderPath, "01_Traits_Config.csv"), sb.ToString());
        }

        private static void ExportSkills(List<Skill> skills, string folderPath)
        {
            if (skills == null || skills.Count == 0) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Skill ID,Tên Kỹ Năng,Mô Tả,Class Yêu Cầu,Loại Chọn Mục Tiêu,Hệ Số ATK/Heal (Power Ratio),Thời Gian Hồi (Cooldown),Hiệu Ứng Phụ,Tỉ Lệ Gây Hiệu Ứng,Số HIT");

            foreach (var s in skills)
            {
                string id = s.id ?? "";
                string name = $"\"{s.skillName}\"";
                string desc = $"\"{s.description}\"";
                string job = s.requiredProfession.ToString();
                string target = s.targeting.ToString();
                string power = s.powerRatio.ToString("F2");
                string cooldown = s.cooldown.ToString();
                string effect = s.appliedEffect.ToString();
                string effectCh = s.effectChance.ToString("F2");
                string hits = s.hitCount.ToString();

                sb.AppendLine($"{id},{name},{desc},{job},{target},{power},{cooldown},{effect},{effectCh},{hits}");
            }

            SafeWriteCSV(Path.Combine(folderPath, "02_Skills_Config.csv"), sb.ToString());
        }

        private static void ExportItems(List<ItemData> items, string folderPath)
        {
            if (items == null || items.Count == 0) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Mã Item (ID),Tên Vật Phẩm,Mô Tả,Chủng Loại,Giá (Vàng),Giảm Thời Gian (SpeedUp Seconds)");

            foreach (var i in items)
            {
                string id = i.id ?? "";
                string name = $"\"{i.itemName}\"";
                string desc = $"\"{i.description}\"";
                string type = i.type.ToString();
                string val = i.sellPrice.ToString();
                string speed = i.speedUpValueInSeconds.ToString();

                sb.AppendLine($"{id},{name},{desc},{type},{val},{speed}");
            }

            SafeWriteCSV(Path.Combine(folderPath, "03_Items_Config.csv"), sb.ToString());
        }

        private static void ExportBosses(List<BossData> bosses, string folderPath)
        {
            if (bosses == null || bosses.Count == 0) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Mã Boss,Tên Boss,Level,ATK,DEF,HP,SPD,Crit %,Crit DMG");

            foreach (var b in bosses)
            {
                string id = b.id ?? "";
                string name = $"\"{b.bossName}\"";
                
                string atk = b.stats != null ? b.stats.atk.ToString() : b.baseAtk.ToString();
                string def = b.stats != null ? b.stats.def.ToString() : b.baseDef.ToString();
                string hp = b.stats != null ? b.stats.hp.ToString() : b.baseHp.ToString();
                string spd = b.stats != null ? b.stats.spd.ToString() : b.baseSpd.ToString();

                string critC = b.stats != null ? b.stats.critChance.ToString("F2") : "0.05";
                string critD = b.stats != null ? b.stats.critDamage.ToString("F2") : "1.5";
                
                sb.AppendLine($"{id},{name},{b.level},{atk},{def},{hp},{spd},{critC},{critD}");
            }

            SafeWriteCSV(Path.Combine(folderPath, "04_Bosses_Config.csv"), sb.ToString());
        }

        private static void ExportBuildings(List<BuildingUpgradeData> buildings, string folderPath)
        {
            if (buildings == null || buildings.Count == 0) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Mã Tòa Nhà,Cấp Độ (Level),Thời Gian Nâng Cấp (Giây),Giá Vàng (Gold),Giá Gỗ (Wood)");

            foreach (var b in buildings)
            {
                if (b.levels == null) continue;
                foreach (var lvl in b.levels)
                {
                    string goldCost = "0";
                    string woodCost = "0";
                    if (lvl.costs != null)
                    {
                        var gold = lvl.costs.Find(c => c.resourceId == "Gold");
                        var wood = lvl.costs.Find(c => c.resourceId == "Wood");
                        if (gold != null) goldCost = gold.amount.ToString();
                        if (wood != null) woodCost = wood.amount.ToString();
                    }
                    sb.AppendLine($"{b.buildingId},{lvl.level},{lvl.duration},{goldCost},{woodCost}");
                }
            }

            SafeWriteCSV(Path.Combine(folderPath, "05_Buildings_Config.csv"), sb.ToString());
        }

        private static void ExportExperienceAndEvolution(List<ExperienceData> expTable, EvolutionTableData evoTable, string folderPath)
        {
            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine("--- BẢNG KINH NGHIỆM TĂNG CẤP HERO ---");
            sb.AppendLine("Cấp Độ (Level),Kinh Nghiệm Yêu Cầu (EXP)");
            if (expTable != null)
            {
                foreach (var exp in expTable)
                {
                    sb.AppendLine($"{exp.level},{exp.experienceRequired}");
                }
            }

            sb.AppendLine();
            sb.AppendLine("--- BẢNG TIẾN HÓA VÀ ĐỘT PHÁ CẢNH GIỚI ---");
            sb.AppendLine("Cấp Độ Yêu Cầu,Mô Tả Phần Thưởng");
            if (evoTable != null && evoTable.rewards != null)
            {
                foreach (var rew in evoTable.rewards)
                {
                    sb.AppendLine($"{rew.level},\"{rew.description}\"");
                }
            }

            SafeWriteCSV(Path.Combine(folderPath, "06_Hero_Progression_Config.csv"), sb.ToString());
        }

        private static void ExportMonsterGroups(POIMonsterConfig poiConfig, string folderPath)
        {
            if (poiConfig == null || poiConfig.monsterGroups == null || poiConfig.monsterGroups.Count == 0) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Tên Khu Vực Khám Phá,Độ Khó Tối Thiểu,Độ Khó Tối Đa,Danh Sách Quái Mặc Định");

            foreach (var group in poiConfig.monsterGroups)
            {
                string name = $"\"{group.groupName}\"";
                string monsters = group.monsterIDs != null ? string.Join(", ", group.monsterIDs) : "";
                sb.AppendLine($"{name},{group.minDifficulty},{group.maxDifficulty},\"{monsters}\"");
            }

            SafeWriteCSV(Path.Combine(folderPath, "07_POI_MonsterGroups_Config.csv"), sb.ToString());
        }

        private static void ExportMonsters(List<MonsterData> monsters, string folderPath)
        {
            if (monsters == null || monsters.Count == 0) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Mã Quái Vật (ID),Tên Nhận Diện,Nghề Nghiệp,Máu (Base HP),Tấn Công (Base ATK),Phòng Thủ (Base DEF),Chớp Nhoáng (Base SPD),Tỉ Lệ Chí Mạng (Crit Chance),Sát Thương Chí Mạng (Crit Dmg)");

            foreach (var m in monsters)
            {
                string id = m.id ?? "";
                string name = $"\"{m.monsterName}\"";
                string prof = m.profession.ToString();
                
                sb.AppendLine($"{id},{name},{prof},{m.baseHp},{m.baseAtk},{m.baseDef},{m.baseSpd},{m.critChance.ToString("F2")},{m.critDamage.ToString("F2")}");
            }

            SafeWriteCSV(Path.Combine(folderPath, "08_Monsters_BaseStats_Config.csv"), sb.ToString());
        }
    }
}

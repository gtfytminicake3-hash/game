using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using LegendOfBlood;

public class SkillDatabaseOverhaul : EditorWindow
{
    [MenuItem("Tools/Overhaul Skill Database (V2 Combos)")]
    public static void RunOverhaul()
    {
        string targetDir = "Assets/Resources/GameData";
        if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

        // 1. Delete old skills
        string[] oldSkills = Directory.GetFiles(targetDir, "SK_*.asset");
        foreach(var f in oldSkills) {
            AssetDatabase.DeleteAsset(f.Replace(@"\", "/"));
        }
        
        // 2. Define the new 20 skills
        var skillsData = new List<(string id, string name, string desc, HeroClass cls, TargetingType targeting, float power, int hitCount, int cd)>
        {
            // WARRIOR
            ("SK_WAR_1", "Chùy Phá Giáp", "Tác dụng gốc: Gây sát thương vật lý lên 1 mục tiêu.\nHiệu ứng Combo: Tự nhận 1 điểm [Khiên Ngự].", HeroClass.Warrior, TargetingType.SingleFrontEnemy, 1.2f, 1, 0),
            ("SK_WAR_2", "Tiếng Rống Chế Ngự", "Tác dụng gốc: Ép toàn bộ địch phải đánh Warrior trong 1 lượt (Taunt) và giảm 15% sát thương của địch.\nHiệu ứng Combo: Tự nhận 1 điểm [Khiên Ngự].", HeroClass.Warrior, TargetingType.AllEnemies, 0f, 1, 3),
            ("SK_WAR_3", "Thành Vách Sắt Đá", "Tác dụng gốc: Lập tức tạo một lớp Khiên Ảo bằng 15% Máu tối đa của Warrior, duy trì 2 lượt.\nHiệu ứng Combo: Tiêu hao toàn bộ [Khiên Ngự] đang có, mỗi điểm cộng thêm 10% giá trị Khiên Ảo.", HeroClass.Warrior, TargetingType.Self, 0f, 1, 4),
            ("SK_WAR_4", "Cú Nện Trấn Động", "Tác dụng gốc: Gây sát thương vật lý diện rộng (AoE) lên toàn bộ đội hình địch.\nHiệu ứng Combo: Nếu Warrior đang có Khiên Ảo, đòn này sẽ gây Choáng toàn bộ địch trong 1 lượt.", HeroClass.Warrior, TargetingType.AllEnemies, 0.8f, 1, 4),
            ("SK_WAR_5", "Nhất Kích Càn Khôn", "Tác dụng gốc: Gây sát thương vật lý cực lớn lên 1 mục tiêu.\nHiệu ứng Combo: Tiêu hao toàn bộ [Khiên Ngự], mỗi điểm tăng thêm 30% Sát thương Chí mạng cho đòn này.", HeroClass.Warrior, TargetingType.SingleFrontEnemy, 3.0f, 1, 5),

            // MAGE
            ("SK_MAG_1", "Phi Tiêu Tà Thuật", "Tác dụng gốc: Gây sát thương phép lên 1 mục tiêu.\nHiệu ứng Combo: Gắn 1 [Ấn Độc] (gây mất máu chuẩn mỗi lượt).", HeroClass.Mage, TargetingType.RandomEnemy, 1.0f, 1, 0),
            ("SK_MAG_2", "Vòng Tròn Suy Vong", "Tác dụng gốc: Gây sát thương phép AoE và giảm 20% Tốc độ của toàn bộ địch trong 2 lượt.\nHiệu ứng Combo: Gắn 1 [Ấn Độc] lên mọi mục tiêu trúng đòn.", HeroClass.Mage, TargetingType.AllEnemies, 0.7f, 1, 3),
            ("SK_MAG_3", "Xiềng Xích Băng Giá", "Tác dụng gốc: Khóa chặt 1 mục tiêu, khiến chúng mất lượt (Stun).\nHiệu ứng Combo: Nếu mục tiêu đang có [Ấn Độc], thời gian khóa tăng lên thành 2 lượt.", HeroClass.Mage, TargetingType.RandomEnemy, 0.5f, 1, 4),
            ("SK_MAG_4", "Thu Mạng", "Tác dụng gốc: Hút máu 1 mục tiêu, gây sát thương và hồi lại máu cho Mage.\nHiệu ứng Combo: Nếu mục tiêu có [Ấn Độc], lây lan ấn độc này sang 1 kẻ địch ngẫu nhiên bên cạnh.", HeroClass.Mage, TargetingType.LowestHpAlly, 1.5f, 1, 4),
            ("SK_MAG_5", "Đại Lễ Kích Nổ", "Tác dụng gốc: Gây sát thương phép diện rộng (AoE) cực mạnh lên toàn bộ địch.\nHiệu ứng Combo: Rút cạn toàn bộ [Ấn Độc] trên bàn cờ. Mỗi Ấn Độc bị rút nổ thêm sát thương Chuẩn tương đương 5% máu tối đa của nạn nhân.", HeroClass.Mage, TargetingType.AllEnemies, 2.5f, 1, 5),

            // ARCHER
            ("SK_ARC_1", "Mũi Tên Dò Xét", "Tác dụng gốc: Tấn công vật lý 1 mục tiêu.\nHiệu ứng Combo: Gắn trạng thái [Điểm Yếu] (giảm 15% tỷ lệ Né tránh trong 2 lượt).", HeroClass.Archer, TargetingType.SingleBackEnemy, 1.1f, 1, 0),
            ("SK_ARC_2", "Nhãn Lực Của Cú", "Tác dụng gốc: Tự buff cho bản thân tăng 30% Sát thương Vật lý trong 2 lượt.\nHiệu ứng Combo: Lượt đánh tiếp theo của Archer chắc chắn gây Chí mạng.", HeroClass.Archer, TargetingType.Self, 0f, 1, 3),
            ("SK_ARC_3", "Mưa Tên Xé Xuyển", "Tác dụng gốc: Bắn AoE lên toàn bộ đội hình địch.\nHiệu ứng Combo: Những kẻ địch đang có [Điểm Yếu] sẽ bị phá 30% Giáp Vật lý.", HeroClass.Archer, TargetingType.AllEnemies, 0.9f, 6, 4),
            ("SK_ARC_4", "Bước Lùi Chiến Thuật", "Tác dụng gốc: Lùi lại trên thanh tốc độ và ngay lập tức hồi máu nhẹ.\nHiệu ứng Combo: Xóa bỏ mọi hiệu ứng bất lợi (Debuff) đang có trên người Archer.", HeroClass.Archer, TargetingType.Self, 0.5f, 1, 4),
            ("SK_ARC_5", "Phát Bắn Đoạt Mệnh", "Tác dụng gốc: Bắn 1 phát cực mạnh vào 1 mục tiêu.\nHiệu ứng Combo: Tiêu hao [Điểm Yếu] trên mục tiêu để bỏ qua 100% Giáp. Nếu đòn này hạ gục kẻ địch, Archer lập tức được đánh thêm 1 lượt.", HeroClass.Archer, TargetingType.SingleBackEnemy, 3.5f, 1, 5),

            // HEALER
            ("SK_HEA_1", "Tia Sáng Nhỏ", "Tác dụng gốc: Hồi một lượng máu vừa phải cho 1 đồng minh.\nHiệu ứng Combo: Cấy 1 [Hạt Giống Sinh Mệnh] lên đồng minh đó.", HeroClass.Healer, TargetingType.LowestHpAlly, 1.2f, 1, 0),
            ("SK_HEA_2", "Lời Cầu Nguyện", "Tác dụng gốc: Tăng 20% Sức tấn công cho toàn đội trong 2 lượt.\nHiệu ứng Combo: Đồng minh nào đang có [Hạt Giống Sinh Mệnh] sẽ được tăng thêm 15% Tỷ lệ Chí mạng.", HeroClass.Healer, TargetingType.AllAllies, 0f, 1, 3),
            ("SK_HEA_3", "Liên Kết Huyết Mạch", "Tác dụng gốc: Chia sẻ sát thương giữa tự bản thân và 1 đồng minh trong 2 lượt.\nHiệu ứng Combo: Ngay lập tức cấy [Hạt Giống Sinh Mệnh] cho cả 2 người được liên kết.", HeroClass.Healer, TargetingType.LowestHpAlly, 0f, 1, 4),
            ("SK_HEA_4", "Hào Quang Thanh Trừng", "Tác dụng gốc: Lập tức xóa bỏ mọi hiệu ứng xấu (Debuff, Độc, Choáng) cho toàn bộ đồng minh.\nHiệu ứng Combo: Nếu đồng minh có [Hạt Giống], hạt giống lập tức nở ra, hồi thêm cho họ 10% máu tối đa.", HeroClass.Healer, TargetingType.AllAllies, 0f, 1, 4),
            ("SK_HEA_5", "Khai Hoa Nở Nhụy", "Tác dụng gốc: Hồi một lượng máu khổng lồ (AoE) cho toàn đội.\nHiệu ứng Combo: Tiêu hao toàn bộ [Hạt Giống Sinh Mệnh]. Mỗi hạt tạo ra một lớp Khiên Bất Tử, giúp chặn hoàn toàn 1 đòn đánh chí tử kế tiếp.", HeroClass.Healer, TargetingType.AllAllies, 2.5f, 1, 5)
        };

        // Reuse icons
        
        List<Skill> newSkillAssets = new List<Skill>();

        foreach(var data in skillsData)
        {
            Skill newSkill = ScriptableObject.CreateInstance<Skill>();
            newSkill.id = data.id;
            newSkill.skillName = data.id + "_name";
            newSkill.description = data.id + "_desc";
            newSkill.requiredProfession = data.cls;
            newSkill.targeting = data.targeting;
            newSkill.powerRatio = data.power;
            newSkill.hitCount = data.hitCount;
            newSkill.cooldown = data.cd;
            newSkill.type = SkillType.Active;

            string pfx = "";
            if(data.cls == HeroClass.Warrior) pfx = "w";
            if(data.cls == HeroClass.Mage) pfx = "m";
            if(data.cls == HeroClass.Archer) pfx = "a";
            if(data.cls == HeroClass.Healer) pfx = "h";

            string order = "1";
            if(data.id.Contains("_1")) order = "1";
            if(data.id.Contains("_2")) order = "2";
            if(data.id.Contains("_3")) order = "3";
            if(data.id.Contains("_4")) order = "4";
            if(data.id.Contains("_5")) order = "5";

            string iconPath = $"Assets/Resources/Icons/skill/{pfx}{order}.png";
            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            
            newSkill.icon = sp;

            string assetPath = $"{targetDir}/{newSkill.id}.asset";
            AssetDatabase.CreateAsset(newSkill, assetPath);
            newSkillAssets.Add(newSkill);
        }

        AssetDatabase.SaveAssets();

        // 3. Update the Main GameConfig so it registers these 20 skills
        string[] configGuids = AssetDatabase.FindAssets("t:GameConfig");
        if (configGuids.Length > 0)
        {
            string cfgPath = AssetDatabase.GUIDToAssetPath(configGuids[0]);
            LegendOfBlood.GameConfigs.GameConfig config = AssetDatabase.LoadAssetAtPath<LegendOfBlood.GameConfigs.GameConfig>(cfgPath);
            if (config != null)
            {
                config.AllSkills = newSkillAssets;
                EditorUtility.SetDirty(config);
            }
            else
            {
                Debug.LogWarning("Không thể Load GameConfig để cập nhật danh sách kỹ năng mới!");
            }
        }
        else
        {
            Debug.LogWarning("Không tìm thấy file GameConfig.asset trong dự án!");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Successfully generated 20 new Combo Skills and injected them into GameConfig!");
    }
}

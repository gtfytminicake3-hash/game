# -*- coding: utf-8 -*-
import os
import codecs

filepath = r"d:\game\legendofblood\legacyofblood_unity_mobile_portraitframe\Assets\Resources\Localization\vi.txt"
en_filepath = r"d:\game\legendofblood\legacyofblood_unity_mobile_portraitframe\Assets\Resources\Localization\en.txt"

def fix_file(path, is_vi):
    lines = []
    with codecs.open(path, 'r', 'utf-8', errors='replace') as f:
        lines = f.readlines()
        
    cutoff = -1
    for i, line in enumerate(lines):
        if line.startswith("SK_WAR_1_name="):
            cutoff = i
            break
            
    if cutoff != -1:
        good_lines = lines[:cutoff]
    else:
        good_lines = lines
        
    vi_text = """SK_WAR_1_name=Chùy Phá Giáp
SK_WAR_1_desc=Tác dụng gốc: Gây sát thương vật lý lên 1 mục tiêu.\\nHiệu ứng Combo: Tự nhận 1 điểm [Khiên Ngự].
SK_WAR_2_name=Tiếng Rống Chế Ngự
SK_WAR_2_desc=Tác dụng gốc: Ép toàn bộ địch phải đánh Warrior trong 1 lượt (Taunt) và giảm 15% sát thương của địch.\\nHiệu ứng Combo: Tự nhận 1 điểm [Khiên Ngự].
SK_WAR_3_name=Thành Vách Sắt Đá
SK_WAR_3_desc=Tác dụng gốc: Lập tức tạo một lớp Khiên Ảo bằng 15% Máu tối đa của Warrior, duy trì 2 lượt.\\nHiệu ứng Combo: Tiêu hao toàn bộ [Khiên Ngự] đang có, mỗi điểm cộng thêm 10% giá trị Khiên Ảo.
SK_WAR_4_name=Cú Nện Trấn Động
SK_WAR_4_desc=Tác dụng gốc: Gây sát thương vật lý diện rộng (AoE) lên toàn bộ đội hình địch.\\nHiệu ứng Combo: Nếu Warrior đang có Khiên Ảo (từ chiêu 3), đòn này sẽ gây Choáng toàn bộ địch trong 1 lượt.
SK_WAR_5_name=Nhất Kích Càn Khôn
SK_WAR_5_desc=Tác dụng gốc: Gây sát thương vật lý cực lớn lên 1 mục tiêu.\\nHiệu ứng Combo: Tiêu hao toàn bộ [Khiên Ngự], mỗi điểm tăng thêm 30% Sát thương Chí mạng cho đòn đánh này.

SK_MAG_1_name=Phi Tiêu Tà Thuật
SK_MAG_1_desc=Tác dụng gốc: Gây sát thương phép lên 1 mục tiêu.\\nHiệu ứng Combo: Gắn 1 [Ấn Độc] (gây mất máu chuẩn mỗi lượt).
SK_MAG_2_name=Vòng Tròn Suy Vong
SK_MAG_2_desc=Tác dụng gốc: Gây sát thương phép AoE và giảm 20% Tốc độ của toàn bộ địch trong 2 lượt.\\nHiệu ứng Combo: Gắn 1 [Ấn Độc] lên mọi mục tiêu trúng đòn.
SK_MAG_3_name=Xiềng Xích Băng Giá
SK_MAG_3_desc=Tác dụng gốc: Khóa chặt 1 mục tiêu, khiến chúng mất lượt (Stun).\\nHiệu ứng Combo: Nếu mục tiêu đang có [Ấn Độc], thời gian khóa tăng lên thành 2 lượt.
SK_MAG_4_name=Thu Mạng
SK_MAG_4_desc=Tác dụng gốc: Hút máu 1 mục tiêu, gây sát thương và hồi lại máu cho Mage.\\nHiệu ứng Combo: Nếu mục tiêu có [Ấn Độc], lây lan ấn độc này sang 1 kẻ địch ngẫu nhiên bên cạnh.
SK_MAG_5_name=Đại Lễ Kích Nổ
SK_MAG_5_desc=Tác dụng gốc: Gây sát thương phép diện rộng (AoE) cực mạnh lên toàn bộ địch.\\nHiệu ứng Combo: Rút cạn toàn bộ [Ấn Độc] trên bàn cờ. Mỗi Ấn Độc bị rút nổ thêm sát thương Chuẩn tương đương 5% máu tối đa của nạn nhân.

SK_ARC_1_name=Mũi Tên Dò Xét
SK_ARC_1_desc=Tác dụng gốc: Tấn công vật lý 1 mục tiêu.\\nHiệu ứng Combo: Gắn trạng thái [Điểm Yếu] (giảm 15% tỷ lệ Né tránh của mục tiêu trong 2 lượt).
SK_ARC_2_name=Nhãn Lực Của Cú
SK_ARC_2_desc=Tác dụng gốc: Tự buff cho bản thân tăng 30% Sát thương Vật lý trong 2 lượt.\\nHiệu ứng Combo: Lượt đánh tiếp theo của Archer chắc chắn gây Chí mạng.
SK_ARC_3_name=Mưa Tên Xé Xuyển
SK_ARC_3_desc=Tác dụng gốc: Bắn AoE lên toàn bộ đội hình địch.\\nHiệu ứng Combo: Những kẻ địch đang có [Điểm Yếu] sẽ bị phá 30% Giáp Vật lý.
SK_ARC_4_name=Bước Lùi Chiến Thuật
SK_ARC_4_desc=Tác dụng gốc: Lùi lại trên thanh tốc độ (nhường lượt cho đồng minh) và ngay lập tức giảm thời gian hồi 2 lượt cho tất cả kỹ năng.\\nHiệu ứng Combo: Xóa bỏ mọi hiệu ứng bất lợi (Debuff) đang có trên người Archer.
SK_ARC_5_name=Phát Bắn Đoạt Mệnh
SK_ARC_5_desc=Tác dụng gốc: Bắn 1 phát cực mạnh vào 1 mục tiêu.\\nHiệu ứng Combo: Tiêu hao [Điểm Yếu] trên mục tiêu để bỏ qua 100% Giáp. Nếu đòn này hạ gục kẻ địch, Archer lập tức được đánh thêm 1 lượt.

SK_HEA_1_name=Tia Sáng Nhỏ
SK_HEA_1_desc=Tác dụng gốc: Hồi một lượng máu vừa phải cho 1 đồng minh.\\nHiệu ứng Combo: Cấy 1 [Hạt Giống Sinh Mệnh] lên đồng minh đó.
SK_HEA_2_name=Lời Cầu Nguyện
SK_HEA_2_desc=Tác dụng gốc: Tăng 20% Sức tấn công cho toàn đội trong 2 lượt.\\nHiệu ứng Combo: Đồng minh nào đang có [Hạt Giống Sinh Mệnh] sẽ được tăng thêm 15% Tỷ lệ Chí mạng.
SK_HEA_3_name=Liên Kết Huyết Mạch
SK_HEA_3_desc=Tác dụng gốc: Chia sẻ sát thương giữa tự bản thân và 1 đồng minh trong 2 lượt.\\nHiệu ứng Combo: Ngay lập tức cấy [Hạt Giống Sinh Mệnh] cho cả 2 người được liên kết.
SK_HEA_4_name=Hào Quang Thanh Trừng
SK_HEA_4_desc=Tác dụng gốc: Lập tức xóa bỏ mọi hiệu ứng xấu (Debuff, Độc, Choáng) cho toàn bộ đồng minh.\\nHiệu ứng Combo: Nếu đồng minh có [Hạt Giống], hạt giống lập tức nở ra, hồi thêm cho họ 10% máu tối đa.
SK_HEA_5_name=Khai Hoa Nở Nhụy
SK_HEA_5_desc=Tác dụng gốc: Hồi một lượng máu khổng lồ (AoE) cho toàn đội.\\nHiệu ứng Combo: Tiêu hao toàn bộ [Hạt Giống Sinh Mệnh]. Mỗi hạt tạo ra một lớp Khiên Bất Tử, giúp chặn hoàn toàn 1 đòn đánh chí tử kế tiếp.
"""
    
    en_text = """SK_WAR_1_name=Armor Breaker Mace
SK_WAR_1_desc=Base Effect: Deals physical damage to 1 target.\\nCombo Effect: Gains 1 stack of [Aegis].
SK_WAR_2_name=Subduing Roar
SK_WAR_2_desc=Base Effect: Taunts all enemies, forcing them to attack the Warrior for 1 turn (Taunt) and reduces their damage by 15%.\\nCombo Effect: Gains 1 stack of [Aegis].
SK_WAR_3_name=Iron Wall
SK_WAR_3_desc=Base Effect: Instantly creates a Virtual Shield equal to 15% of the Warrior's Max HP, lasting 2 turns.\\nCombo Effect: Consumes all current [Aegis] stacks, each stack adds 10% to the Virtual Shield value.
SK_WAR_4_name=Trembling Smash
SK_WAR_4_desc=Base Effect: Deals Area of Effect (AoE) physical damage to the entire enemy team.\\nCombo Effect: If the Warrior has a Virtual Shield, this attack Stuns all enemies for 1 turn.
SK_WAR_5_name=Ultimate Strike
SK_WAR_5_desc=Base Effect: Deals massive physical damage to 1 target.\\nCombo Effect: Consumes all [Aegis] stacks, each stack increases Critical Damage for this attack by 30%.

SK_MAG_1_name=Demon Dart
SK_MAG_1_desc=Base Effect: Deals magic damage to 1 target.\\nCombo Effect: Applies 1 [Poison Mark] (deals true damage every turn).
SK_MAG_2_name=Circle of Decay
SK_MAG_2_desc=Base Effect: Deals AoE magic damage and reduces Speed of all enemies by 20% for 2 turns.\\nCombo Effect: Applies 1 [Poison Mark] to all targets hit.
SK_MAG_3_name=Frost Shackles
SK_MAG_3_desc=Base Effect: Chains 1 target, making them lose a turn (Stun).\\nCombo Effect: If the target has a [Poison Mark], the lock duration increases to 2 turns.
SK_MAG_4_name=Life Drain
SK_MAG_4_desc=Base Effect: Drains health from 1 target, dealing damage and healing the Mage.\\nCombo Effect: If the target has a [Poison Mark], spreads this poison mark to an adjacent random enemy.
SK_MAG_5_name=Grand Detonation
SK_MAG_5_desc=Base Effect: Deals extremely powerful AoE magic damage to all enemies.\\nCombo Effect: Drains all [Poison Marks] on the board. Each drained Mark detonates additional True Damage equal to 5% of the victim's max health.

SK_ARC_1_name=Scouting Arrow
SK_ARC_1_desc=Base Effect: Deals physical damage to 1 target.\\nCombo Effect: Applies [Weakness] status (reduces Target's Evasion rate by 15% for 2 turns).
SK_ARC_2_name=Owl's Vision
SK_ARC_2_desc=Base Effect: Self-buffs to increase Physical Damage by 30% for 2 turns.\\nCombo Effect: The Archer's next attack is guaranteed to be a Critical Hit.
SK_ARC_3_name=Piercing Arrow Rain
SK_ARC_3_desc=Base Effect: Fires AoE attacks at the entire enemy team.\\nCombo Effect: Enemies with [Weakness] will have 30% of their Physical Armor broken.
SK_ARC_4_name=Tactical Retreat
SK_ARC_4_desc=Base Effect: Steps back on the speed bar and reduces the cooldown of all skills by 2 turns.\\nCombo Effect: Removes all negative effects (Debuffs) currently on the Archer.
SK_ARC_5_name=Fatal Shot
SK_ARC_5_desc=Base Effect: Fires an extremely powerful shot at 1 target.\\nCombo Effect: Consumes [Weakness] on the target to ignore 100% Armor. If this attack kills the enemy, the Archer immediately gets an extra turn.

SK_HEA_1_name=Small Ray of Light
SK_HEA_1_desc=Base Effect: Restores a moderate amount of health to 1 ally.\\nCombo Effect: Plants 1 [Seed of Life] on that ally.
SK_HEA_2_name=Prayer
SK_HEA_2_desc=Base Effect: Increases Attack Power for the entire team by 20% for 2 turns.\\nCombo Effect: Any ally with a [Seed of Life] gets an additional 15% Critical Hit Rate.
SK_HEA_3_name=Bloodline Link
SK_HEA_3_desc=Base Effect: Shares damage between 2 allies for 2 turns (When 1 takes damage, the other bears 50%).\\nCombo Effect: Immediately plants a [Seed of Life] on both linked allies.
SK_HEA_4_name=Purifying Aura
SK_HEA_4_desc=Base Effect: Instantly removes all negative effects (Debuff, Poison, Stun) for all allies.\\nCombo Effect: If an ally has a [Seed], the seed instantly blooms, restoring an additional 10% of their max health.
SK_HEA_5_name=Blossom Bloom
SK_HEA_5_desc=Base Effect: Restores a massive amount of health (AoE) to the entire team.\\nCombo Effect: Consumes all [Seeds of Life]. Each consumed seed creates an Immortal Shield, completely blocking 1 fatal blow for the bearer.
"""
    
    text_to_append = vi_text if is_vi else en_text
    
    with codecs.open(path, 'w', 'utf-8-sig') as f:
        f.writelines(good_lines)
        if len(good_lines) > 0 and not good_lines[-1].endswith('\n'):
            f.write('\n')
        f.write('\n')
        f.write(text_to_append.replace('\\n', '\\n'))

fix_file(filepath, True)
fix_file(en_filepath, False)
print("Files fixed and appended using utf-8-sig.")

import os
import codecs

vi_path = r"d:\game\legendofblood\legacyofblood_unity_mobile_portraitframe\Assets\Resources\Localization\vi.txt"
en_path = r"d:\game\legendofblood\legacyofblood_unity_mobile_portraitframe\Assets\Resources\Localization\en.txt"

def clean_and_read(filepath):
    # 1. Read raw bytes
    with open(filepath, 'rb') as f:
        raw_bytes = f.read()

    # 2. Heuristic decoding
    text = ""
    
    # Check for UTF-16 LE (Powershell Out-File default)
    if b'\x00' in raw_bytes:
        # It's likely UTF-16 LE if alternating zero bytes are present
        try:
            # Clean pure NULs that might have been prepended
            text = raw_bytes.decode('utf-16le', errors='replace')
            text = text.replace('\x00', '') # Just in case some stray NULs
        except Exception:
            text = raw_bytes.decode('cp1258', errors='replace').replace('\x00', '')
    else:
        # Try UTF-8 first
        try:
            text = raw_bytes.decode('utf-8-sig') # also removes BOM
        except UnicodeDecodeError:
            # Fallback to Windows-1258 (Vietnamese ANSI)
            try:
                text = raw_bytes.decode('cp1258')
            except Exception:
                # Absolute fallback
                text = raw_bytes.decode('utf-8', errors='replace')
                
    # 3. Strip out the combo skills we appended previously to avoid duplication
    lines = text.split('\n')
    clean_lines = []
    for line in lines:
        line = line.strip('\r')
        if line.startswith("SK_WAR_1_name=") or line.startswith("combo_war_1="):
            break
        if line.strip() != "":
            clean_lines.append(line)
        else:
            # Keep empty lines before cutoff
            clean_lines.append(line)
            
    # Trim trailing empty lines
    while len(clean_lines) > 0 and clean_lines[-1].strip() == "":
        clean_lines.pop()
        
    return "\n".join(clean_lines)

# Text to append
vi_text = """
SK_WAR_1_name=Chùy Phá Giáp
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

combo_war_1=<color=#aaaaaa>{0} tự nhận 1 điểm [Khiên Ngự].</color>
combo_war_2=<color=#aaaaaa>{0} gầm lên khiêu khích toàn địch! Nhận 1 [Khiên Ngự].</color>
combo_war_3=<color=#ffff00>{0} tạo Khiên Ảo chặn {1} Sát Thương (Tiêu thụ {2} Khiên Ngự).</color>
combo_war_4=<color=#aaaaaa>Sức mạnh Khiên Ảo cộng hưởng gây Choáng toàn bộ kẻ địch!</color>
combo_mag_4=<color=#00ff00>{0} hút sinh lực về {1} HP.</color>
combo_mag_5=<color=#ff00ff>[Ấn Độc] nổ tung trên {0} gây {1} ST CHUẨN!</color>
combo_arc_2=<color=#00ffff>{0} ngưng thần, +30% ATK và chắc chắn chí mạng đòn sau!</color>
combo_arc_4=<color=#00ffff>{0} lùi chiến thuật, giảm 2 lượt Hồi chiêu toàn đội & Xóa Debuff!</color>
combo_arc_5=<color=#ffaa00>Phát Đạn Đoạt Mệnh hiệu quả, {0} được đánh thêm 1 lượt!</color>
combo_hea_2=<color=#00ff00>{0} chúc phúc, tăng Sức Tấn Công toàn đội.</color>
combo_hea_3=<color=#ff00ff>{0} tạo Liên Kết Huyết Mạch chia sẻ 50% ST với {1}!</color>
combo_hea_4=<color=#00ff00>[Hạt Giống] trên {0} bung nở hồi {1} Máu!</color>
combo_hea_5=<color=#ffff00>{0} nhận 1 lớp [Khiên Bất Tử] chặn chết 1 lần!</color>
combo_dmg_link=<color=#ff00ff>{0} gánh chịu {1} ST thay thế cho {2}!</color>
combo_immortal_pop=<color=#ffff00>[Khiên Bất Tử] đã vỡ, {0} thoát chết kỳ diệu với 1 HP!</color>
"""

en_text = """
SK_WAR_1_name=Armor Breaker Mace
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

combo_war_1=<color=#aaaaaa>{0} gains 1 stack of [Aegis].</color>
combo_war_2=<color=#aaaaaa>{0} roars to taunt all enemies! Gains 1 [Aegis].</color>
combo_war_3=<color=#ffff00>{0} creates Virtual Shield blocking {1} DMG (Consumes {2} Aegis).</color>
combo_war_4=<color=#aaaaaa>Virtual Shield resonance Stuns all enemies!</color>
combo_mag_4=<color=#00ff00>{0} drains life for {1} HP.</color>
combo_mag_5=<color=#ff00ff>[Poison Mark] detonates on {0} for {1} TRUE DMG!</color>
combo_arc_2=<color=#00ffff>{0} focuses, +30% ATK and next hit is a Critical Hit!</color>
combo_arc_4=<color=#00ffff>{0} falls back, reducing team cooldowns by 2 & Cleanses Debuffs!</color>
combo_arc_5=<color=#ffaa00>Fatal Shot triggered, {0} gains an extra turn!</color>
combo_hea_2=<color=#00ff00>{0} prays, increasing team Attack Power.</color>
combo_hea_3=<color=#ff00ff>{0} links bloodline, sharing 50% DMG with {1}!</color>
combo_hea_4=<color=#00ff00>[Seed] on {0} blooms, restoring {1} HP!</color>
combo_hea_5=<color=#ffff00>{0} gains 1 [Immortal Shield] to block 1 fatal blow!</color>
combo_dmg_link=<color=#ff00ff>{0} bears {1} DMG for {2}!</color>
combo_immortal_pop=<color=#ffff00>[Immortal Shield] shattered, {0} miraculously survives with 1 HP!</color>
"""

# Process VI
vi_base = clean_and_read(vi_path)
full_vi = vi_base + "\n\n" + vi_text.strip() + "\n"
# OVERWRITE as UTF-8 with BOM (utf-8-sig identifies standard text file across all windows editors + Unity flawlessly)
with open(vi_path, 'w', encoding='utf-8-sig') as f:
    f.write(full_vi)

# Process EN
en_base = clean_and_read(en_path)
full_en = en_base + "\n\n" + en_text.strip() + "\n"
with open(en_path, 'w', encoding='utf-8-sig') as f:
    f.write(full_en)

print("Files have been fully rebuilt from binary, cleaned of NUL characters, decoded automatically via ANSI/UTF16 heuristcs, and force-saved as 100% pure UTF-8-BOM.")

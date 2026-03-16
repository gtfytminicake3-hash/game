import os
import codecs

lang_dir = r"d:\game\legendofblood\legacyofblood_unity_mobile_portraitframe\Assets\Resources\Localization"
vi_path = os.path.join(lang_dir, "vi.txt")
en_path = os.path.join(lang_dir, "en.txt")

def read_lang(filepath):
    data = {}
    with open(filepath, 'r', encoding='utf-8-sig') as f:
        for line in f:
            line_s = line.strip()
            if line_s and not line_s.startswith('#'):
                parts = line_s.split('=', 1)
                if len(parts) >= 2:
                    data[parts[0].strip()] = parts[1].strip()
    return data

vi_data = read_lang(vi_path)
en_data = read_lang(en_path)

# New keys to add from prefabs
NEW_KEYS = {
    "btn_cancel": {"vi": "HỦY BỎ", "en": "CANCEL"},
    "panel_title_arena": {"vi": "ĐẤU TRƯỜNG HUYỀN THOẠI", "en": "LEGENDARY ARENA"},
    "btn_challenge": {"vi": "Thách Đấu", "en": "Challenge"},
    "panel_title_hospital": {"vi": "BỆNH VIỆN", "en": "HOSPITAL"},
    "panel_title_quest": {"vi": "NHIỆM VỤ", "en": "QUEST"},
    "panel_title_stat_alloc": {"vi": "CỘNG ĐIỂM", "en": "STAT ALLOCATION"},
    "btn_breed_action": {"vi": "Sinh con", "en": "Give Birth"},
    "title_select_hero": {"vi": "Chọn Hero", "en": "Select Hero"},
    "title_select_squad": {"vi": "Chọn Đội Hình", "en": "Select Squad"},
    "label_total_cp": {"vi": "Tổng Cp", "en": "Total CP"},
    "msg_hero_busy": {"vi": "Hero is busy right now", "en": "Hero is busy right now"},
    "label_advanced_stats": {"vi": "Advanced Stats", "en": "Advanced Stats"},
    "label_base_stats": {"vi": "Base Stats", "en": "Base Stats"},
    "label_dmg_increase": {"vi": "Dmg Increase", "en": "Dmg Increase"},
    "label_dmg_reduction": {"vi": "Dmg Reduction", "en": "Dmg Reduction"},
    "label_evasion": {"vi": "Evasion Rate", "en": "Evasion Rate"},
    "label_trait_upgrade": {"vi": "Trait Upgrade", "en": "Trait Upgrade"},
    "btn_use_exp_item": {"vi": "Use EXP Item", "en": "Use EXP Item"},
    "btn_free_gift": {"vi": "Free Gift", "en": "Free Gift"},
    "btn_skip_battle": {"vi": "BỎ QUA (SKIP)", "en": "SKIP"},
    "combat_victory": {"vi": "CHIẾN THẮNG!", "en": "VICTORY!"},
    "combat_defeat": {"vi": "THẤT BẠI!", "en": "DEFEAT!"},
    "btn_back_to_village": {"vi": "Trở Về Làng", "en": "Back to Village"},
    "combat_speed_format": {"vi": "TỐC ĐỘ: x1", "en": "SPEED: x{0}"},
    "sys_tap_to_start": {"vi": "NHẤN ĐỂ BẮT ĐẦU", "en": "TAP TO START"},
    "sys_loading_data": {"vi": "loading... just wait", "en": "Loading... please wait"},
    "btn_challenge_boss": {"vi": "Khiêu Chiến", "en": "Challenge"},
    "btn_equip_item": {"vi": "Mặc vào", "en": "Equip"},
    "btn_quick_upgrade": {"vi": "Nâng cấp nhanh", "en": "Quick Upgrade"},
    "tab_equipments": {"vi": "Equipments", "en": "Equipments"},
    "tab_items": {"vi": "Items", "en": "Items"},
    "btn_claim_x2_ad": {"vi": "Claim x2 (Ad)", "en": "Claim x2 (Ad)"},
    "panel_title_mailbox": {"vi": "HÒM THƯ (INBOX)", "en": "INBOX (MAIL)"},
    "label_result": {"vi": "Kết Quả", "en": "Result"},
    "btn_claim_all": {"vi": "Nhận Tất Cả Quà", "en": "Claim All Rewards"},
    "btn_replay_battle": {"vi": "Replay Battle", "en": "Replay Battle"},
    "btn_revive_retry": {"vi": "Revive Retry", "en": "Revive Retry"},
    "btn_close_return": {"vi": "Trở về Làng (Đóng)", "en": "Return to Village (Close)"},
    "label_mystic_chest": {"vi": "Mystic Chest", "en": "Mystic Chest"},
    "menu_recruit": {"vi": "CHIÊU MỘ", "en": "RECRUIT"},
    "menu_settings": {"vi": "CÀI ĐẶT", "en": "SETTINGS"},
    "menu_shop": {"vi": "CỬA HÀNG", "en": "SHOP"},
    "panel_title_menu": {"vi": "DANH MỤC / MENU", "en": "MENU"},
    "menu_mailbox": {"vi": "HÒM THƯ", "en": "MAILBOX"},
    "menu_inventory": {"vi": "TÚI ĐỒ", "en": "INVENTORY"},
    "title_select_profession": {"vi": "Chọn nghề cho <Hero>", "en": "Select Profession for {0}"},
    "prof_mage": {"vi": "Mage", "en": "Mage"},
    "prof_warrior": {"vi": "Warrior", "en": "Warrior"},
    "prof_archer": {"vi": "archer", "en": "Archer"},
    "prof_healer": {"vi": "healer", "en": "Healer"},
    "tab_daily": {"vi": "Daily", "en": "Daily"},
    "tab_main": {"vi": "Main Quest", "en": "Main Quest"},
    "tab_weekly": {"vi": "Weekly", "en": "Weekly"},
    "btn_ads_recruit": {"vi": "Ads Recruit", "en": "Ads Recruit"},
    "btn_recruit_x1": {"vi": "Chiêu mộ x1", "en": "Recruit x1"},
    "btn_recruit_x10": {"vi": "Chiêu mộ x10", "en": "Recruit x10"},
    "btn_skip_cooldown": {"vi": "Skip Cooldown", "en": "Skip Cooldown"},
    "btn_enter_tower": {"vi": "Đi vào tháp", "en": "Enter Tower"},
    "btn_dismiss_hero": {"vi": "Sa Thải", "en": "Dismiss"},
    "btn_claim_reward": {"vi": "Nhận thưởng", "en": "Claim Reward"},
    "btn_view_report": {"vi": "Xem Lại", "en": "View Report"},
    "btn_close_panel": {"vi": "ĐÓNG", "en": "CLOSE"},
    "btn_tutorial_skip": {"vi": "BỎ QUA", "en": "SKIP"},
    "tutorial_welcome": {"vi": "Chào mừng đến với Legend of Blood!", "en": "Welcome to Legend of Blood!"},
    "panel_title_tutorial": {"vi": "HƯỚNG DẪN TÂN THỦ", "en": "TUTORIAL"},
    "btn_tutorial_prev": {"vi": "LÙI LẠI", "en": "PREVIOUS"},
    "btn_tutorial_next": {"vi": "TIẾP THEO", "en": "NEXT"},
    "hospital_light_injury": {"vi": "Khu Vực Hồi Sức (Thương Nhẹ)", "en": "Recovery Room (Light Injury)"},
    "hospital_severe_injury": {"vi": "Khu Vực Hồi Sức (Thương Nặng)", "en": "Intensive Care Unit (Severe Injury)"},
}

with open(vi_path, 'a', encoding='utf-8-sig') as f_vi, open(en_path, 'a', encoding='utf-8-sig') as f_en:
    # We DO NOT sync from vi to en anymore because it caused bleeding of vietnamese strings
    # We only inject the newly discovered UI string keys
    
    f_vi.write("\n# --- Prefab Extracted UI Keys ---\n")
    f_en.write("\n# --- Prefab Extracted UI Keys ---\n")
    for k, text_dict in NEW_KEYS.items():
        v_vi = text_dict['vi']
        v_en = text_dict['en']
        if k not in vi_data:
            f_vi.write(f"{k}={v_vi}\n")
            vi_data[k] = v_vi
        if k not in en_data:
            f_en.write(f"{k}={v_en}\n")
            en_data[k] = v_en

print("Done injecting only the new UI Prefab keys!")

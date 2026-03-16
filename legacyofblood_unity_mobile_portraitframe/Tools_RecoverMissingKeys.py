import os

vi_path = r"d:\game\legendofblood\legacyofblood_unity_mobile_portraitframe\Assets\Resources\Localization\vi.txt"
en_path = r"d:\game\legendofblood\legacyofblood_unity_mobile_portraitframe\Assets\Resources\Localization\en.txt"

vi_translations = """
# --- Recovered Missing Keys ---
ad_limit_reached=Đã đạt giới hạn xem quảng cáo!
ad_reward_gacha_ticket=Xem QC nhận Vé Chiêu Mộ
breeding_change_father=Đổi Cha
breeding_change_mother=Đổi Mẹ
breeding_error_same_gender=Cha mẹ phải khác giới tính.
breeding_error_select_father_first=Vui lòng chọn cha trước.
breeding_error_select_parents_first=Vui lòng chọn đủ 2 bên cha mẹ.
breeding_failed=Lai tạo thất bại.
breeding_select_father=Chọn Cha
breeding_select_father_title=Chọn một người Cha
breeding_select_mother=Chọn Mẹ
breeding_select_mother_title=Chọn một người Mẹ
btn_breed=Lai Tạo
btn_challenge=Thách Đấu
btn_change_language=Ngôn Ngữ
btn_leaderboard=Bảng Xếp Hạng
btn_manage_population=Quản Lý Dân Số
btn_select_father=Chọn Cha
btn_select_mother=Chọn Mẹ
btn_shop=Cửa Hàng
btn_upgrade=Nâng Cấp
building_max_level=Đã Đạt Cấp Tối Đa
building_upgrade_info_format=Hiện tại: {0}\\nTiếp: {1}
building_upgrade_started_format=Bắt đầu nâng cấp {0}
building_upgrade_title_format=Nâng cấp {0}
cp_format_short=CP: {0}
datamanager_error_no_gameconfig=Thiếu GameConfig trong DataManager!
description_not_found=Không tìm thấy mô tả.
gender_format=Giới tính: {0}
gold_cost_format={0} Vàng
heal_light=Trị Liệu Nhẹ
heal_severe=Trị Liệu Đặc Biệt
healer_tower_soldier_name=Lính Gác Tháp
hero_dismissed_success=Đã sa thải {0}
hospital_hero_healed_format=Đã chữa trị cho {0}
inventory_error_no_playerdata=InventoryManager thiếu PlayerData!
inventory_error_not_enough_item=Không đủ '{0}'! Cần: {1}, Có: {2}
inventory_error_not_enough_resource=Không đủ {0}! Cần: {1}, Có: {2}
inventory_item_used_success=Dùng vật phẩm thành công!
inventory_not_enough_resource_notification=Không đủ {0}!
item_IT_EXP_BOOK_S_name=Sách EXP Cơ Bản
item_IT_FERTILITY_POTION_name=Thuốc Sinh Học
item_IT_SPEEDUP_1H_name=Tăng tốc 1 Giờ
level_format=Cấp {0}
level_format_short=Cấp {0}
mailbox_loot_claimed=Đã nhận: {0}
msg_max_expedition_reached=Giới hạn đội thám hiểm!
notification_building_already_upgrading=Đang trong tiến trình nâng cấp!
notification_building_max_level=Công trình đã đạt cấp tối đa!
notification_feature_locked=Tính năng đang khóa!
notification_not_enough_mutation_potion=Không đủ Thuốc Đột Biến!
notification_not_enough_resources=Không đủ tài nguyên!
notification_population_full=Quá giới hạn dân số!
poi_difficulty_format=Độ khó: {0}
poi_name_format={0} #{1}
poi_recommended_cp_format=CP Khuyến nghị: {0}
poi_tower_name_format=Tháp {0}
population_count_format=Dân số: {0}/{1}
profession_format=Nghề: {0}
profession_selection_title=Chọn Nghề cho {0}
ready=Sẵn Sàng
settings_title=Cài Đặt
stats_atk_format=ATK: {0}
stats_atk_format_detailed=ATK: {0} (+{1})
stats_def_format=DEF: {0}
stats_def_format_detailed=DEF: {0} (+{1})
stats_dmg_increase_format=Tăng ST: {0}%
stats_dmg_reduction_format=Giảm ST: {0}%
stats_evasion_format=Né Tránh: {0}%
stats_hp_format=HP: {0}
stats_hp_format_detailed=HP: {0} (+{1})
stats_potential_format=Tiềm Năng: {0}
stats_spd_format=SPD: {0}
stats_spd_format_detailed=SPD: {0} (+{1})
time_left_format=Còn lại {0}
total_cp_format=Tổng CP: {0}
tower_current_floor_format=Tầng: {0}
unknown=Ẩn số
worldmap_select_squad_title_format=Đội hình đến {0}
"""

en_translations = """
# --- Recovered Missing Keys ---
ad_limit_reached=Ad limit reached!
ad_reward_gacha_ticket=Watch AD for Recruit Ticket
breeding_change_father=Change Father
breeding_change_mother=Change Mother
breeding_error_same_gender=Parents must be of different genders.
breeding_error_select_father_first=Please select a father first.
breeding_error_select_parents_first=Please select both parents first.
breeding_failed=Breeding failed.
breeding_select_father=Select Father
breeding_select_father_title=Select a Father
breeding_select_mother=Select Mother
breeding_select_mother_title=Select a Mother
btn_breed=Breed
btn_challenge=Challenge
btn_change_language=Language
btn_leaderboard=Leaderboard
btn_manage_population=Manage Population
btn_select_father=Select Father
btn_select_mother=Select Mother
btn_shop=Shop
btn_upgrade=Upgrade
building_max_level=Max Level Reached
building_upgrade_info_format=Current: {0}\\nNext: {1}
building_upgrade_started_format=Started upgrading {0}
building_upgrade_title_format=Upgrade {0}
cp_format_short=CP: {0}
datamanager_error_no_gameconfig=GameConfig missing in DataManager!
description_not_found=Description not found.
gender_format=Gender: {0}
gold_cost_format={0} Gold
heal_light=Light Heal
heal_severe=Intensive Care
healer_tower_soldier_name=Tower Guard
hero_dismissed_success=Dismissed {0}
hospital_hero_healed_format=Healed {0}
inventory_error_no_playerdata=InventoryManager missing PlayerData!
inventory_error_not_enough_item=Not enough '{0}'! Need: {1}, Have: {2}
inventory_error_not_enough_resource=Not enough {0}! Need: {1}, Have: {2}
inventory_item_used_success=Item used!
inventory_not_enough_resource_notification=Not enough {0}!
item_IT_EXP_BOOK_S_name=Basic EXP Book
item_IT_FERTILITY_POTION_name=Fertility Potion
item_IT_SPEEDUP_1H_name=1H Speedup
level_format=Level {0}
level_format_short=Lv. {0}
mailbox_loot_claimed=Claimed: {0}
msg_max_expedition_reached=Expedition limit reached!
notification_building_already_upgrading=Building already upgrading!
notification_building_max_level=Building is at max level!
notification_feature_locked=Feature is locked!
notification_not_enough_mutation_potion=Not enough Mutation Potions!
notification_not_enough_resources=Not enough resources!
notification_population_full=Population limit reached!
poi_difficulty_format=Difficulty: {0}
poi_name_format={0} #{1}
poi_recommended_cp_format=Rec. CP: {0}
poi_tower_name_format=Tower of {0}
population_count_format=Population: {0}/{1}
profession_format=Class: {0}
profession_selection_title=Select Class for {0}
ready=Ready
settings_title=Settings
stats_atk_format=ATK: {0}
stats_atk_format_detailed=ATK: {0} (+{1})
stats_def_format=DEF: {0}
stats_def_format_detailed=DEF: {0} (+{1})
stats_dmg_increase_format=DMG Boost: {0}%
stats_dmg_reduction_format=DMG Resist: {0}%
stats_evasion_format=Evasion: {0}%
stats_hp_format=HP: {0}
stats_hp_format_detailed=HP: {0} (+{1})
stats_potential_format=Potential: {0}
stats_spd_format=SPD: {0}
stats_spd_format_detailed=SPD: {0} (+{1})
time_left_format={0} left
total_cp_format=Total CP: {0}
tower_current_floor_format=Floor: {0}
unknown=Unknown
worldmap_select_squad_title_format=Squad for {0}
combat_log_cleanse={0} cleanses negative effects on {1}.
"""

with open(vi_path, "a", encoding="utf-8-sig") as f:
    f.write("\n")
    f.write(vi_translations.strip())
    f.write("\n")

with open(en_path, "a", encoding="utf-8-sig") as f:
    f.write("\n")
    f.write(en_translations.strip())
    f.write("\n")

print("Đã thêm thành công tất cả 79 key bị mất và combat_log_cleanse vào cả 2 file.")

# Soi Plan & Plan Mới Chạy Được
*(Phân tích dựa trên code thật trong `legacyofblood_unity_mobile_portraitframe/`. Cập nhật: 2026-05-08)*

---

## Phần 1 — Content chính của game (xác định lại)

Vòng lặp cốt lõi của game là **Breeding chất lượng gen** (`potential`, `traitIDs`, `Profession`). Expedition + Mailbox + EXP là cơ chế support để tích lũy tài nguyên phục vụ Breeding.

1. **Content cốt lõi (must-have cho "playable basic"):**
   - Breeding (Panel_Breeding): Cha + mẹ → child.
   - Maturation: Child trưởng thành → chọn nghề ở level 10.
   - Expedition (EXTRACTED_WorldMap_Panel -> EXTRACTED_SquadSelection_Panel): Đưa hero đi đánh để lấy EXP/Gold.
   - Mailbox (Panel_Mailbox): Nhận thưởng.

2. **Content support:**
   - Population Manager (Panel_PopulationManager): Xem danh sách hero. Thay thế cho khái niệm Barrack cũ.

3. **Side content (post-MVP):**
   - EXTRACTED_Arena_Panel, Panel_BossBattle, Panel_Hospital, Panel_Tower.

---

## Phần 2 — Những chỗ bất hợp lý / cập nhật so với hierarchy mới

1. **Sai lệch Panel Name:**
   - Kế hoạch cũ nhắc tới các panel không tồn tại hoặc đã đổi tên. Hierarchy mới (GameClient) sử dụng: `Panel_PopulationManager` (cho Heroes), `EXTRACTED_WorldMap_Panel`, `EXTRACTED_SquadSelection_Panel`, `Panel_Breeding`, `Panel_Mailbox`. Kế hoạch mới PHẢI dùng chính xác các tên này.

2. **Bottom Nav MVP:**
   - Hierarchy hiện tại của `BottomNavIcons` chứa: `Tab_Shop`, `Tab_Barracks`, `Tab_Lobby`, `Tab_Hospital`, `Tab_Battlefield`.
   - Cần remap các tab này thành `Camp`, `Map`, `Heroes`, `Breed`, `Mail` và nối đúng panel.

3. **Sub-stage map procedural (POI_InfoPanel):**
   - Chọn Option A: Bỏ khỏi flow MVP. Flow sẽ là: World Map -> Squad Selection -> Start Expedition (chạy ngầm) -> Mailbox.

4. **Sửa lỗi data:**
   - Đổi fallback trong `ExpeditionManager.GetMonstersForTowerFloor` từ `"Orc"` thành `"Orc Warrior"`.

---

## Phần 3 — Plan Mới (chạy được, có verify cụ thể)

### Phase 0 — Hygiene Console (30 phút)
- Mở scene `GameClient.unity`, gom error vào `outputs/console_dump_initial.txt`.
- Di chuyển script spam (`Auto*.cs`, `Fix*.cs` không cần thiết) vào thư mục `Editor/Disabled/`.

### Phase 1 — Audit Scene Hierarchy (1 tiếng)
- File output: `outputs/bottomnav_audit.md` ghi nhận 5 tab trong `BottomNavIcons`.
- Map lại các nút `UIPanelNavButton`:
  - `Tab_Lobby` -> Mở `Panel_MainScreen` (Camp)
  - `Tab_Battlefield` -> Mở `EXTRACTED_WorldMap_Panel` (Map)
  - `Tab_Barracks` -> Mở `Panel_PopulationManager` (Heroes)
  - Đổi 1 tab trống -> Mở `Panel_Breeding` (Breed)
  - Đổi 1 tab trống -> Mở `Panel_Mailbox` (Mail)

### Phase 2 — Fix Bottom Nav & Panel Registration (30 phút)
- Tắt các panel rác. UIManager phải gọi đúng các Extract Panel và Panel chuẩn mới tìm thấy trong hierarchy.

### Phase 3 — Sửa flow WorldMap → Expedition (2 tiếng)
- Cập nhật click region trong `EXTRACTED_WorldMap_Panel`.
- Build POI presets (Capital Forest, Snowy Mountains, Magic Tree, Desert Ruins, Volcanic Lair).
- Mở `EXTRACTED_SquadSelection_Panel` -> Chọn đội -> Gọi `ExpeditionManager.StartExpedition()`.

### Phase 4 — Cập nhật UI Binding cho Squad Selection (1 tiếng)
- Do UI đã bị extract (`EXTRACTED_SquadSelection_Panel`), cần gán lại refs vào các slot `Slot_0..Slot_29` trong `AvailableListScrollView/Viewport/Content`.
- Đảm bảo hiển thị CP, Level, Name đúng của từng Hero.

### Phase 5 — Verify Mailbox claim → resource + EXP (30 phút)
- Đợi ExpeditionManager tick xong. Mở `Panel_Mailbox`.
- Claim report, verify log cộng resource và kinh nghiệm.

### Phase 6 — Verify Breeding → Child → Maturation (1 tiếng)
- Mở `Panel_Breeding`. Dùng `EXTRACTED_HeroPicker_Panel` chọn cha/mẹ.
- Sinh child, đợi mature, xuất hiện trong `Panel_PopulationManager`.

### Phase 7 — Save/Reload (30 phút)
- Đảm bảo `ExpeditionManager._activeExpeditions` được rebind đúng với `DataManager.Player.ActiveExpeditions` khi load save.

### Phase 8 — Debug tools (30 phút)
- Viết `DebugMenu.cs` có: Add Gold, Mature All Babies, Force Complete All Expeditions.

---

## Phần 4 — Definition of Done

- Quá trình chuyển từ Bootloader -> MainScreen mượt mà.
- 5 Tab Navigation chuẩn: Camp, Map, Heroes, Breed, Mail.
- Click Map -> Mở Squad Selection -> Chạy Expedition.
- Expedition xong báo về Mailbox -> Nhận thưởng.
- Breeding sinh child khác nhau (potential/trait gen).
- Load/Save không lỗi tham chiếu.

# Kế hoạch: Đưa Game Lên Mức Chơi Được Cơ Bản
*(Cập nhật: 2026-05-08 - Dựa trên cấu trúc GameClient mới nhất)*

---

## Mục Tiêu MVP

Game được coi là "chơi được cơ bản" khi người chơi có thể hoàn thành vòng lặp chính mà không cần tool ngoài Unity Editor:

- [ ] Mở game, Bootloader chạy, MainScreen/Camp hiện ra không có error đỏ.
- [ ] Xem và thao tác danh sách hero trong Panel_PopulationManager (Heroes).
- [ ] Mở World Map (EXTRACTED_WorldMap_Panel), chọn khu vực/POI, chọn đội hình (EXTRACTED_SquadSelection_Panel), bắt đầu expedition thật.
- [ ] Expedition kết thúc, report xuất hiện trong Mailbox (Panel_Mailbox).
- [ ] Claim Mailbox cộng đúng Gold/EXP/item/equipment/hero rescued nếu có.
- [ ] Hero nhận EXP, lên level, có `freeStatPoints` để phân phối.
- [ ] Lai tạo 1 nam + 1 nữ trưởng thành trong Breeding (Panel_Breeding), sinh child.
- [ ] Child trưởng thành có thể được chọn trong expedition.
- [ ] Save/reload không làm mất hero, reward đã claim, report chưa claim, expedition đang chạy.

Core loop phải chốt là:

```text
Camp -> World Map -> Expedition -> Mailbox -> Hero grows -> Breed -> Heroes management -> repeat
```

Content chính của game là breeding/bloodline + expedition progression. Các hệ như Arena, Shop, Hospital, BossBattle, KingGodPass là side content hoặc post-MVP, không được chặn mục tiêu "playable basic".

---

## Thực Trạng Đã Xác Minh

Các điểm dưới đây dựa trên kiểm tra code và hierarchy scene `GameClient` qua MCP:

- [x] Các panel đã bị đổi tên/cấu trúc: `Panel_PopulationManager` (thay cho Barracks), `EXTRACTED_WorldMap_Panel`, `EXTRACTED_SquadSelection_Panel`, `Panel_Breeding`.
- [x] `GameConfig.asset` hiện đã có `POIMonsterConfig`, `AllMonsters`, `AllBosses`, `TowerConfigs`.
- [x] Monster ID hiện dùng dạng `Goblin`, `Slime`, `Orc Warrior`, `Elder Dragon`.
- [x] `DataManager.GetMonsterByID()` đã có special case `INJURED_SOLDIER`.
- [x] `DataManager.CreateNewPlayerData()` đã seed Adam + Eva trưởng thành cho save mới.
- [x] `GameManager.Update()` đã tick `MaturationSystem.Tick()` và `ExpeditionManager.Tick()`.
- [ ] `WorldMapFixedController.OpenRegion()` hiện chỉ mở popup thông tin, chưa gọi `EXTRACTED_SquadSelection_Panel` hoặc `ExpeditionManager.StartExpedition()`.
- [ ] Bottom nav (`BottomBar/BottomNavIcons`) hiện chứa: `Tab_Shop`, `Tab_Barracks`, `Tab_Lobby`, `Tab_Hospital`, `Tab_Battlefield` -> Cần map lại cho MVP core loop.
- [ ] Scene còn nhiều panel duplicate hoặc `PanelType=None` cần fix.

---

## Bottom Nav MVP

Bottom nav hiện tại trong `Panel_MainScreen/SafeArea/BottomBar/BottomNavIcons` cần được chuẩn hóa lại thành 5 tab MVP phục vụ core loop:

- [ ] `Camp` -> `UIPanelType.MainScreen` (Mở `Panel_MainScreen`)
- [ ] `Map` -> `UIPanelType.WorldMap` (Mở `EXTRACTED_WorldMap_Panel`)
- [ ] `Heroes` -> `UIPanelType.Barrack` / `Population` (Mở `Panel_PopulationManager`)
- [ ] `Breed` -> `UIPanelType.Breeding` (Mở `Panel_Breeding`)
- [ ] `Mail` -> `UIPanelType.Mailbox` (Mở `Panel_Mailbox`)

Verify bottom nav:
- [ ] Mỗi tab có label đúng nghĩa với panel mở ra.
- [ ] Không có tab `targetPanel=None` trên bottom nav MVP.
- [ ] Tab Shop, Arena, Hospital đưa vào Menu hoặc MainScreen phụ (post-MVP).

---

## Phase 0: Import Và Console Sạch Đủ Để Làm Việc

Mục tiêu: Unity import ổn định trước khi sửa scene/UI.

Checklist:
- [ ] Kiểm tra console không còn dòng kiểu `Build asset version error` hoặc `timestamp mismatch`.
- [ ] Move các script fix tự động gây spam (font, tmp fixers) vào thư mục `Disabled/` nếu chúng tự chạy làm scene bị dirty ngoài ý muốn.

Verify:
- [ ] Unity mở scene `GameClient` thành công không có error đỏ block game loop.

---

## Phase 1: Chốt Data Không Đổi ID Bừa Bãi

Mục tiêu: giữ data hiện có, validate ID thay vì tạo lại data sai schema.

Checklist:
- [ ] Dùng ID hiện có trong `Assets/Resources/GameData`: `Goblin`, `Slime`, `Orc Warrior`, `Orc Shaman`, `Troll`, v.v.
- [ ] Kiểm tra `GameConfig.asset` đang reference đủ data.
- [ ] Sửa fallback pool trong `ExpeditionManager.GetMonstersForTowerFloor` từ `"Orc"` thành `"Orc Warrior"`.

---

## Phase 2: Fix Panel Registration Và Scene Hygiene

Mục tiêu: UIManager chỉ register đúng một panel cho mỗi `UIPanelType` MVP. Dựa vào hierarchy thực tế.

Checklist panel MVP bắt buộc:
- [ ] `Panel_Bootloader` -> `Bootloader`
- [ ] `Panel_MainScreen` -> `MainScreen`
- [ ] `EXTRACTED_WorldMap_Panel` -> `WorldMap`
- [ ] `EXTRACTED_SquadSelection_Panel` -> `SquadSelection`
- [ ] `Panel_Mailbox` -> `Mailbox`
- [ ] `Panel_PopulationManager` -> `Barrack` (Hoặc Population)
- [ ] `Panel_Breeding` -> `Breeding`
- [ ] `HeroInfo_Panel` -> `HeroInfo`

Verify:
- [ ] Play Mode: `UIManager.ShowPanel` gọi đúng panel tương ứng, không bị duplicate.

---

## Phase 3: Rebuild Bottom Nav Theo Core Loop

Mục tiêu: bottom nav phản ánh loop chính.

Checklist:
- [ ] Trong `Panel_MainScreen/SafeArea/BottomBar/BottomNavIcons`, sửa các nút thành 5 tab MVP.
- [ ] Cập nhật `BottomNavigationController.cs` để các nút gọi đúng `UIPanelType` mới.

Verify:
- [ ] Tap Camp -> MainScreen.
- [ ] Tap Map -> WorldMap.
- [ ] Tap Heroes -> PopulationManager.
- [ ] Tap Breed -> Breeding.
- [ ] Tap Mail -> Mailbox.

---

## Phase 4: Fix SquadSelection và Hero Picker UI Binding

Mục tiêu: UI chọn đội hình (đã bị extract) hoạt động đúng.

Checklist:
- [ ] Fix prefab / component mapping cho `EXTRACTED_SquadSelection_Panel`.
- [ ] Trong `EXTRACTED_SquadSelection_Panel`, map `SquadSlotsContainer` và các `Slot_0..Slot_29` trong `AvailableListScrollView/Viewport/Content`.
- [ ] Fix `EXTRACTED_HeroPicker_Panel` dùng cho Breeding.

Verify:
- [ ] Mở Heroes thấy Adam/Eva.
- [ ] Mở SquadSelection thấy danh sách hero trưởng thành.
- [ ] Chọn hero vào slot làm cập nhật tổng CP.
- [ ] Confirm button chỉ bật khi đủ hero.

---

## Phase 5: Nối WorldMapFixedController Vào Expedition Thật

Mục tiêu: click region trên map tạo expedition thật.

Checklist:
- [ ] Cập nhật logic click region trong `EXTRACTED_WorldMap_Panel`.
- [ ] Trong `OpenRegion(regionName)`, tạo `POIData` (preset: Capital Forest, Snowy Mountains, Magic Tree, Desert Ruins, Volcanic Lair).
- [ ] Mở `EXTRACTED_SquadSelection_Panel` -> Chọn đội -> `ExpeditionManager.StartExpedition()`.

Verify:
- [ ] Console log `Expedition ... started`.
- [ ] `Player.ActiveExpeditions.Count` tăng.

---

## Phase 6: Verify Expedition -> Mailbox -> Reward

Mục tiêu: expedition hoàn thành và claim được reward.

Checklist:
- [ ] `ExpeditionManager` chạy ngầm.
- [ ] Mở `Panel_Mailbox` sau khi expedition kết thúc, xuất hiện report.
- [ ] Claim report cộng Gold/EXP.

Verify:
- [ ] Mailbox instantiate report item không bị lỗi.
- [ ] EXP cộng vào surviving heroes, Gold cộng vào account.

---

## Phase 7: Verify Breeding -> Child -> Maturation

Mục tiêu: breeding loop hoạt động bằng data thật.

Checklist:
- [ ] Mở `Panel_Breeding`.
- [ ] Chọn father/mother (dùng `EXTRACTED_HeroPicker_Panel`).
- [ ] Breed thành công sinh child.
- [ ] Đợi `MaturationSystem.Tick()` hoặc dùng cheat để child trưởng thành.

Verify:
- [ ] Gold bị trừ. Child xuất hiện trong `Panel_PopulationManager`.

---

## Phase 8: Save/Reload Sanity

Mục tiêu: không mất tiến trình sau khi thoát game.

Checklist:
- [ ] Đảm bảo reload giữ nguyên `ActiveExpeditions` và `UnclaimedReports`.
- [ ] Khắc phục lỗi reference nếu `ExpeditionManager` không rebind sau khi load save mới.

---

## Phase 9: Debug Tools Tối Thiểu

Checklist:
- [ ] Cập nhật `DebugMenu.cs` để thêm Gold, Mature Babies, Complete Expeditions, Clear Save phục vụ test nhanh.

---

## Definition Of Done Cuối Cùng

- [ ] Bootloader -> MainScreen hiển thị ổn định.
- [ ] Bottom nav có đúng 5 tab: Camp, Map, Heroes, Breed, Mail hoạt động trơn tru.
- [ ] PopulationManager hiển thị hero.
- [ ] Map region click mở SquadSelection -> chạy Expedition thật.
- [ ] Expedition xong chuyển report qua Mailbox -> Claim thành công.
- [ ] Breeding tạo child, child trưởng thành đi đánh được.
- [ ] Save/reload không lỗi state.

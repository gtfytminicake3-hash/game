# GDD 05 — Hệ thống World Map & Expedition
*(Cập nhật lần cuối: 2026-05-07 — Đồng bộ hoàn toàn với code thực tế)*

---

## ⚠️ Chú Ý Kiến Trúc Quan Trọng

Hệ thống WorldMap hiện có **hai controller song song** trong codebase, đây là điểm cần làm rõ trước khi phát triển tiếp:

| Script | Kế thừa | Đăng ký UI | Trạng thái |
|---|---|---|---|
| `WorldMapFixedController.cs` | `UIPanel` | `UIPanelType.WorldMap` ✅ | **ACTIVE** — được UIManager nhận diện |
| `WorldMapController.cs` | `MonoBehaviour` | Không có | **LEGACY/PARALLEL** — có full logic POI nhưng không phải UIPanel |

`WorldMapFixedController` là panel được UIManager quản lý. Nó hiển thị map tĩnh với 5 nút khu vực (Snow, Forest, Tree, Desert, Volcano). `WorldMapController` chứa toàn bộ hệ thống POI động nhưng chỉ chạy nếu được gắn vào một GameObject trong scene.

**Việc cần làm:** Quyết định một trong hai — giữ WorldMapFixedController (static map) hoặc chuyển sang WorldMapController (dynamic POI map). Hiện tại hai script cùng tồn tại gây mơ hồ kiến trúc.

---

## 1. WorldMapFixedController (UIPanel đang hoạt động)

### 1.1. Chức năng

Đây là panel chính được UIManager nhận diện và điều hướng.

```
PanelType = UIPanelType.WorldMap
```

**Các nút khu vực tĩnh:**
- `btnRegionSnow` → "Snowy Mountains"
- `btnRegionForest` → "Capital Forest"
- `btnRegionTree` → "Magic Tree"
- `btnRegionDesert` → "Desert Ruins"
- `btnRegionVolcano` → "Volcanic Lair"

Mỗi nút khi click sẽ mở `POI_InfoPanel` với một `POIData` mock tạm thời — **chưa kết nối vào ExpeditionManager.**

### 1.2. Nút Quay Về

Nếu `btnLobby` không được gán trong Inspector, script tự tạo một nút "< CLOSE MAP" bằng runtime code. Nút này gọi `UIManager.HidePanel(UIPanelType.WorldMap)`.

### 1.3. Scroll / Zoom

Implement `IScrollHandler`, zoom mapContainer theo `eventData.scrollDelta.y`, clamp theo 1080×1920.

---

## 2. WorldMapController (Dynamic POI System — cần tích hợp)

### 2.1. Tổng quan

Script này chứa toàn bộ logic POI động và expedition visualization. Kế thừa `MonoBehaviour`, implement `IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler`.

### 2.2. Thông số Map

| Tham số | Giá trị mặc định | Ghi chú |
|---|---|---|
| `mapSize` | 2160 × 3840 | Portrait map, enforce minimum |
| `numberOfDungeons` | 40 | Enforce minimum |
| `numberOfRescues` | 20 | Enforce minimum |
| `minPoiDistance` | 350f | Khoảng cách tối thiểu giữa các POI |
| `minZoom` / `maxZoom` | 0.5 / 2.0 | |
| `zoomSpeed` | 0.1f | |

### 2.3. Khởi tạo Thế giới (`InitializeWorldMap`)

**Persistence:** Load POI từ `DataManager.Instance.Player.WorldPois`.

**Top-up logic:** Nếu số lượng POI thiếu, tự sinh thêm:
- Dungeon: top-up đến `numberOfDungeons = 40`
- RescueMission: top-up đến `numberOfRescues = 20`
- Boss: sinh 1 nếu chưa có
- TowerOfTrials: luôn đảm bảo đủ 4 tháp (1 cho mỗi nghề)

**Auto-fix duplicate towers:** Xóa tháp thừa, chỉ giữ tối đa 4, ưu tiên tháp có `currentFloor` cao nhất.

### 2.4. Phân bổ Vị trí POI (Elliptical Ring Distribution)

Đây là cơ chế phân bổ vị trí **quan trọng nhất** — khác hoàn toàn so với GDD cũ:

```
Bản đồ được chia thành 10 vòng đồng tâm (hình elip):
  Ring 1 = gần trung tâm làng nhất → độ khó 1
  Ring 10 = xa nhất → độ khó 10

Công thức vị trí:
  x = cos(angle) * radiusX
  y = sin(angle) * radiusY

Khoảng cách tối thiểu từ tâm (làng): 300px
```

POI khó hơn ở xa hơn. Làng nằm tại tọa độ (0, 0) trong không gian bản đồ.

### 2.5. Các Loại POI

| Loại | Prefab | Tính năng khi click |
|---|---|---|
| `Dungeon` | `dungeonPoiPrefab` | Mở POI_InfoPanel → SquadSelectionPanel → Expedition |
| `RescueMission` | `rescuePoiPrefab` | Giống Dungeon, loot là HeroData được giải cứu |
| `TowerOfTrials` | `towerPoiPrefab` | Mở TowerPanel, yêu cầu nghề cụ thể (`requiredProfession`) |
| `Boss` | `towerPoiPrefab` (fallback) | Mở BossBattlePanel |

**Boss POI:** Chọn ngẫu nhiên từ `DataManager.GameConfig.AllBosses`. Spawn 1 boss duy nhất trên map khi khởi tạo.

### 2.6. Profession Towers (Tháp Nghề Nghiệp)

Luôn có đúng 4 tháp: Warrior, Archer, Mage, Healer.

Mỗi tháp có:
- `requiredProfession` — chỉ hero nghề đó mới tham gia
- `currentFloor` — tiến độ hiện tại (1–20)
- `recoveryEndTime` — unix timestamp hết hồi phục (2 tiếng sau khi thất bại)
- Sprite được load từ `Resources/UI/Towers/{thap_mage|thap_healer|thap_acher|thap_warior}`

⚠️ Lưu ý lỗi typo trong code: `"thap_acher"` (thiếu 'r'), `"thap_warior"` (thiếu 'r'). Cần sửa hoặc đặt tên sprite đúng theo typo.

### 2.7. Luồng Tương tác POI (Click → Expedition)

```
1. Click vào POI button
2. [Dungeon/Rescue] → poiInfoPanel.Show()
                     → "Khám phá" → OpenSquadSelectionForPOI()
                     → SquadSelectionPanel (filter: hero isMature && !IsBusy())
                     → Confirm → ExpeditionManager.StartExpedition()
   [Tower] → UIManager.ShowPanel(UIPanelType.Tower)
            → TowerPanel.Show(poiData)
   [Boss]  → UIManager.ShowPanel(UIPanelType.BossBattle)
            → BossBattlePanel.Show(poiData)
```

### 2.8. Expedition Visualization (Xe Ngựa)

Khi expedition bắt đầu (`OnExpeditionStarted`):
- Instantiate `travelCartPrefab` vào `travelLayer` (được tự auto-reparent vào `mapContainer`)
- DOTween: cart di chuyển từ (0,0) → destination trong `travelDuration` giây
- Sau khi đến: `DOVirtual.DelayedCall(combatDuration)` → cart quay đầu và di chuyển về (0,0)
- Thêm DOTween pulsing outline màu vàng (alpha 1.0 ↔ 0.2, 0.6s yoyo loop)
- Hướng xe: `localScale.x = isMovingRight ? -1 : 1` (sprite gốc nhìn sang trái)

Khi expedition kết thúc (`OnExpeditionFinished`): Destroy cart.

---

## 3. ExpeditionManager (Fire-and-Forget System)

### ⚠️ Thay đổi kiến trúc quan trọng so với GDD cũ

GDD cũ mô tả một **state machine 4 trạng thái** (Traveling → Exploring → Returning → Finished). **Điều này không còn đúng.** Hệ thống hiện tại là **fire-and-forget**:

```
Khi StartExpedition() được gọi:
  1. Combat được tính NGAY LẬP TỨC (CombatSystem.Simulate())
  2. Kết quả lưu vào ActiveExpedition.preCalculatedReport
  3. completionTimestamp = now + travelTime*2 + combatTime
  4. ActiveExpedition được thêm vào PlayerData

Trong Tick() (gọi mỗi frame bởi GameManager):
  1. Duyệt qua tất cả ActiveExpeditions
  2. Nếu currentTime >= completionTimestamp:
     → Di chuyển report vào Player.UnclaimedReports (Mailbox)
     → Xử lý thương vong (HospitalSystem)
     → Xóa expedition khỏi danh sách
     → Phát sự kiện OnExpeditionFinished, OnNewReportReceived
```

**Không có trạng thái Exploring hay Returning.** Xe ngựa chỉ là hiệu ứng visual từ DOTween.

### 3.1. Các Static Events

| Event | Dữ liệu | Người nghe |
|---|---|---|
| `OnExpeditionStarted` | `ExpeditionDisplayData` | WorldMapController |
| `OnExpeditionFinished` | `string expeditionId` | WorldMapController |
| `OnNewReportReceived` | *(none)* | UIMainController (badge) |
| `OnTowerConquered` | `POIData` | WorldMapController |
| `OnPOICleared` | `POIData` | WorldMapController |

### 3.2. Giới hạn Số Đội Viễn Chinh

```
maxConcurrentExpeditions = 1 + (Barracks.level / 5)
```

Mặc định 1 đội. Mỗi 5 level Doanh Trại thêm 1 đội. Khi đạt giới hạn, hiển thị notification.

### 3.3. Tính Thời Gian

```
travelTimeMs = Vector2.Distance(village, destination) * 100
combatTimeMs = 2000 + (TotalTurns * 3000)  // 2s khám phá + 3s/turn
totalDuration = travelTime*2 + combatTime   // Đi + Về + Chiến đấu
```

### 3.4. Các Loại Expedition

| Loại | Hàm khởi động | Combat |
|---|---|---|
| Normal (Dungeon/Rescue) | `StartNormalExpedition()` | `CombatSystem.Simulate(heroes, monsterIDs, difficulty)` |
| Boss | `StartBossExpedition()` | `CombatSystem.SimulateBoss(heroes, bossId)` |
| Tower | `StartTowerChallenge()` | Simulate từng tầng liên tiếp (1→20), hero HP carry over |

### 3.5. Tower Challenge — Chi tiết

- Vòng lặp từ `towerPoi.currentFloor` đến tầng 20
- Mỗi tầng: `CombatSystem.Simulate()` với monster từ `TowerFloorConfig`
- Nếu thắng: cập nhật HP hero và tiếp tục tầng sau
- Nếu thua: lưu `currentFloor`, set `recoveryEndTime = now + 2hr`
- **Healer Tower:** Generate danh sách "INJURED_SOLDIER" thay vì monster thực

⚠️ "INJURED_SOLDIER" ID là placeholder — `DataManager.GetMonsterByID("INJURED_SOLDIER")` cần xử lý trường hợp này.

### 3.6. Hệ thống Phần thưởng

| Nguồn | Công thức |
|---|---|
| Dungeon Gold | `100 + (10 * difficulty)` |
| Dungeon Wood/Stone | `Random(0, 5 * difficulty)` |
| Dungeon EXP | `50 + (5 * difficulty)` (nếu thắng) |
| Rescue Hero | Level `Random(minLvl, maxLvl)`, Potential `Random(7d, 15d)` |
| Boss Gold | 2000 gold + 500 wood + 500 stone |
| Boss Items | 2× IT_EXP_BOOK_L + 3× IT_TICKET_RECRUIT |
| Boss Equipment | 1–2 trang bị ngẫu nhiên (floor equiv 30) |
| Tower Win | `1000 + floor*500` gold, IT_WISH_CHARM, equipment |
| Tower EXP | `floorsCleared * 100` |

### 3.7. Hospital Integration

Khi expedition kết thúc (trong `Tick()`):
- **Tử trận (PlayerCasualties):** `HospitalSystem.AdmitHero()` → thương nặng (8hr)
- **Sống sót nhưng mất HP:** `HospitalSystem.InflictLightInjury()` → thương nhẹ (5min)

### 3.8. POI Respawn

Sau khi Dungeon/Rescue bị clear:
1. `OnPOICleared` phát ra
2. `WorldMapController.HandlePOICleared()` xóa POI object và data
3. Gọi `GenerateAndRegisterNewPOI()` cùng loại, cùng độ khó

---

## 4. Monster Generation cho POI

Monster được sinh dựa trên `DataManager.GameConfig.POIMonsterConfig`:

```
POIMonsterConfig {
  monsterGroups: [
    { minDifficulty, maxDifficulty, monsterIDs: [...] }
  ]
}
```

Hệ thống tìm nhóm phù hợp với `difficultyLevel` của POI và chọn ngẫu nhiên 2–4 monster.

**Fallback:** Nếu không tìm thấy config phù hợp, lấy bất kỳ monster nào từ tất cả nhóm.

---

## 5. Navigation — Điều hướng Bản đồ

**Input (hiện tại hoạt động):**
- Pan: `OnDrag()` — `mapContainer.anchoredPosition += eventData.delta`
- Zoom: `OnScroll()` — scale mapContainer, clamp (0.5x–2.0x)

**Input bị comment:**
```csharp
// Input.GetMouseButton cũ đã bị comment vì conflict với New Input System
// → HandleInput() trong Update() hiện là hàm rỗng
```

Chức năng pan bằng chuột (editor testing) cần viết lại bằng `UnityEngine.InputSystem.Mouse.current`.

---

## 6. Danh sách Việc Cần Làm

| Mức độ | Vấn đề | Tác động |
|---|---|---|
| 🔴 Bắt buộc | Quyết định dùng WorldMapController hay WorldMapFixedController, xóa cái kia | Kiến trúc rõ ràng |
| 🔴 Bắt buộc | Fill POIMonsterConfig ScriptableObject với monster groups | Map hoạt động |
| 🔴 Bắt buộc | Fill TowerFloorConfig với 20 tầng | Tower hoạt động |
| 🔴 Bắt buộc | Sửa "INJURED_SOLDIER" placeholder trong DataManager | Healer Tower không crash |
| 🟡 Nên làm | Sửa typo sprite: `thap_acher` → `thap_archer`, `thap_warior` → `thap_warrior` | Tower có icon đúng |
| 🟡 Nên làm | Kết nối WorldMapFixedController region buttons vào ExpeditionManager thực sự | Chơi được qua map tĩnh |
| 🟢 Tùy chọn | Tích hợp WorldMapController (dynamic POI) làm chế độ chính | Map sống động hơn |

# GDD 09 — Các Hệ thống Mới (Chưa có trong GDD gốc)
*(Tạo ngày: 2026-05-07 — Các hệ thống được tìm thấy trong code nhưng chưa có tài liệu)*

---

## 1. Diamond Currency (Tiền Kim Cương)

**File:** `PlayerData.cs` — trường `int diamond`

Diamond là currency cao cấp thứ hai, khác với Gold. Hiện tại chỉ có data field, chưa có UI hiển thị hay nguồn thu riêng biệt. `InventoryManager` cần bổ sung `AddDiamond()` / `SpendDiamond()`.

**Trạng thái:** Data model có, chưa tích hợp gameplay.

---

## 2. Player Level & EXP System

**File:** `PlayerData.cs`, `InventoryManager.cs`

### 2.1. Cấu trúc

```csharp
PlayerData {
    int playerLevel = 1    // Cấp độ người chơi (không phải hero)
    int playerExp = 0      // EXP người chơi
}
```

### 2.2. Nguồn EXP

- Nhận report từ Mailbox: `AddPlayerExp(report.experienceGained * rMulti)`
- EXP người chơi tăng cùng lúc với EXP hero sống sót

### 2.3. Công thức Lên cấp

*(Chưa xác nhận — cần kiểm tra InventoryManager.AddPlayerExp() và ExperienceTable asset)*

---

## 3. King God Pass System

**Files:** `KingGodPassPanel.cs`, `PlayerData.cs`, `GameConfig.cs`

### 3.1. Cấu trúc Data

```csharp
PlayerData {
    int passLevel = 1
    int passExp = 0
    bool isPremiumPassUnlocked = false
    List<int> claimedFreePassLevels     // Các cấp đã nhận thưởng free track
    List<int> claimedPremiumPassLevels  // Các cấp đã nhận thưởng premium track
}
```

### 3.2. Config

`KingGodPassConfig` ScriptableObject — chứa danh sách `levels.Count` cấp với phần thưởng từng cấp (free + premium track).

**⚠️ ScriptableObject này chưa được điền dữ liệu — cần tạo content.**

### 3.3. UI Panel (`KingGodPassPanel.cs`)

- Đọc `GameConfig.KingGodPassConfig?.levels.Count`
- Lắng nghe `InventoryManager.OnPassExpChanged`
- Hiển thị 2 track: Free và Premium
- Premium track unlock bằng purchase (Diamonds hoặc IAP — chưa xác định)

### 3.4. Trạng thái

- UI Panel: ✅ Có script
- Config data: ❌ Rỗng (cần điền)
- Premium unlock payment flow: ❓ Chưa rõ

---

## 4. Ad Monetization System

**Files:** `AdManager.cs`, `AdRewardGateway.cs`, `PlayerData.cs`

### 4.1. SDK

Google Mobile Ads SDK. Test IDs hiện tại:
- Android: `ca-app-pub-3940256099942544/5224354917`
- iOS: `ca-app-pub-3940256099942544/1712485313`

**Cần thay bằng production IDs trước khi publish.**

### 4.2. RewardType Enum (10 loại)

| RewardType | Mô tả | Giới hạn/ngày |
|---|---|---|
| `DailySummon` | Chiêu mộ free mỗi ngày | `dailySummonAdsWatched` |
| `DoubleGold` | X2 phần thưởng Mailbox | `dailyDoubleGoldAdsWatched < 3` |
| `ReviveTeam` | Hồi sinh đội sau thua trận | `dailyCombatReviveAdsWatched < 2` |
| `MysticChest` | Mở rương bí ẩn | `dailyMysticChestAdsWatched` |
| `BuildingSpeedUp` | Tăng tốc xây dựng | `dailyBuildingSpeedUpsWatched` |
| `FreeHeal` | Hồi phục thương nhẹ miễn phí | `dailyFreeHealsWatched` |
| `ArenaTicket` | Nhận thêm vé đấu trường | `dailyArenaTicketAdsWatched` |
| `BreedingMutation` | Tăng tỉ lệ đột biến lai tạo | `dailyMutationAdsWatched` |
| `TowerCooldownSkip` | Bỏ qua cooldown tháp | `dailyTowerSkipAdsWatched` |
| `ShopFreebie` | Nhận hàng miễn phí ở shop | `dailyShopFreebieAdsWatched` |

### 4.3. Luồng hoạt động (AdRewardGateway)

```
UI button "Xem quảng cáo"
  → AdRewardGateway.Instance.RequestAd(RewardType, callback)
  → AdManager.ShowRewardedAd()
  → [User watches ad]
  → callback() được gọi
  → Tăng counter, cấp thưởng
```

### 4.4. Reset hàng ngày

Tất cả counter `dailyXxxAdsWatched` phải được reset khi qua ngày mới. Logic reset này cần kiểm tra — likely trong `QuestManager.CheckAndResetDaily()` hoặc `DataManager.Tick()`.

---

## 5. Arena Shop System

**Files:** `ArenaShopPanel.cs`, `PlayerData.cs`

### 5.1. Data

```csharp
PlayerData {
    long lastArenaShopRefreshTimestamp  // Timestamp lần refresh gần nhất
    List<ArenaShopGood> currentArenaShopGoods  // Hàng hóa hiện tại
}
```

### 5.2. ArenaShopGood

```csharp
class ArenaShopGood {
    ShopGoodType type       // Item, Equipment, Resource
    string goodId           // Item ID hoặc Equipment ID
    int price               // Giá bằng Arena Coins
    bool isPurchased        // Đã mua chưa
    bool isInfinite         // Không giới hạn số lượng mua
    EquipmentTier equipTier // Nếu type == Equipment
    EquipmentSlot equipSlot
    Profession equipRestriction
}
```

### 5.3. Refresh Logic

Shop refresh tự động sau một khoảng thời gian (cần xác nhận interval). Người chơi có thể refresh thủ công bằng Diamond (chưa implement).

### 5.4. Trạng thái

- UI Panel: ✅ Có script (ArenaShopPanel.cs)
- Data model: ✅ Hoàn chỉnh
- Refresh logic: ⚠️ Chưa rõ server-side hay client-side
- Content (hàng hóa thực tế): ❌ Chưa có dữ liệu cố định

---

## 6. Mailbox System (Hộp Thư)

**Files:** `MailboxPanel.cs`, `ExpeditionReport.cs` (trong PlayerData.cs)

### 6.1. Luồng

```
ExpeditionManager.Tick()
  → expedition hoàn thành
  → preCalculatedReport → Player.UnclaimedReports (thêm vào)

MailboxPanel.OnEnable()
  → RefreshUI() — hiển thị tất cả UnclaimedReports
  → Mỗi report có 4 nút: [Nhận] [X2] [Replay] [Hồi Sinh]
```

### 6.2. ExpeditionReport

```csharp
class ExpeditionReport {
    string poiId
    string poiName
    CombatResult combatResult  // Kết quả chiến đấu đã tính trước
    LootData loot              // Phần thưởng
    int experienceGained
}
```

### 6.3. Tính năng Đặc biệt

**X2 (Double Gold):** Nhân đôi Gold, Wood, Stone, Items, EXP khi claim. Không nhân đôi Equipment và Hero (giữ cân bằng).

**Hồi Sinh & Đánh Tiếp (Revive & Retry):**
- Hồi phục HP cho hero tử trận
- Simulate lại combat với enemy survivors còn lại
- Nối combat log
- Nếu thắng: tính lại loot

**Replay:** Mở CombatVisualizerPanel để xem lại trận đấu.

### 6.4. Welcome Mail

`PlayerData` constructor tự tạo 1 mail chào mừng: 10 vé gacha (`IT_GACHA_TICKET`).

---

## 7. Offline Progression System

**File:** `PlayerData.cs`

```csharp
PlayerData {
    long lastOfflineTimestamp  // Unix ms khi game tắt lần cuối
}
```

Logic offline progression chưa được implement hoàn chỉnh. Trường `lastOfflineTimestamp` được set khi khởi tạo PlayerData. Cần implement trong `GameManager.OnApplicationPause()` / `OnApplicationQuit()`.

---

## 8. Tower System (Formal)

**Files:** `TowerPanel.cs`, `ExpeditionManager.StartTowerChallenge()`, `GameConfig.TowerFloorConfig`

*(GDD_05 đề cập Tower nhưng chưa document riêng về cơ chế gameplay)*

### 8.1. Cấu trúc Tháp

- 4 tháp nghề nghiệp: Warrior, Archer, Mage, Healer
- Mỗi tháp có 20 tầng
- Cooldown 2 tiếng sau khi thua (reset về tầng 1)
- Chinh phục tầng 20 → Tháp biến mất, spawn tháp mới

### 8.2. Healer Tower — Challenge Đặc biệt

Thay vì chiến đấu, Healer Tower test kỹ năng hồi phục:
- "Enemies" là `INJURED_SOLDIER` — dummy đơn vị cần được heal
- CombatSystem phải xử lý ID đặc biệt này
- **⚠️ Chưa implement trong DataManager.GetMonsterByID()**

### 8.3. TowerFloorConfig

ScriptableObject cần điền: 20 entry, mỗi entry có `floorIndex`, `monsterPool[]`, `customMonsterCount`.

**⚠️ ScriptableObject chưa có dữ liệu — Tower không chạy được.**

### 8.4. Loot Tháp

```
Win:
  gold = 1000 + floor * 500
  items: IT_EXP_BOOK_S × (5 + floor), IT_WISH_CHARM × 1
  equipment: 1 món ngẫu nhiên (+ 1 bonus nếu floor >= 10 và 50% RNG)
EXP: floorsCleared × 100
```

### 8.5. TowerPanel.cs

- Hiển thị 20 tầng từ `DataManager.GameConfig.TowerConfigs`
- Có nút `skipCooldownAdButton` — xem ad để bỏ qua cooldown 2 tiếng
- Nhận `POIData _currentTowerData` để biết tháp nào đang xem

---

## 9. Player Progression Flags

Các trường trong `PlayerData` phục vụ monetization và progression mà không có trong GDD gốc:

```csharp
bool useDynamicUI           // Feature flag cho Dynamic UI mode
long lastDailyResetTimestamp    // Timestamp reset daily quest/ads
long lastWeeklyResetTimestamp   // Timestamp reset weekly quest/ads
long lastArenaShopRefreshTimestamp
long lastOfflineTimestamp
```

---

## 10. GDD TODO — Cần Document Thêm

Các phần chưa có GDD đầy đủ nhưng có code:

| Hệ thống | File Script | Ghi chú |
|---|---|---|
| CombatVisualizerPanel | `CombatVisualizerPanel.cs` | Replay trận đấu từ EventLog |
| BossBattlePanel | `BossBattlePanel.cs` | UI Boss fight |
| BuildingSystem nâng cấp | `BuildingSystem.cs` | Placeholder costs |
| InventoryPanel (Item tab) | `InventoryPanel.cs` | Bindings chưa xác nhận |
| InventoryPanel (Equipment tab) | `InventoryPanel.cs` | Bindings chưa xác nhận |
| POI_InfoPanel | `POI_InfoPanel.cs` | Hiển thị info POI trước khi đánh |
| SquadSelectionPanel | `SquadSelectionPanel.cs` | Chọn đội đi expedition |
| HeroPickerPanel | `HeroPickerPanel.cs` | Chọn hero trong popup |

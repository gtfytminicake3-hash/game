# GDD 07 — Hệ thống Trang bị (Equipment System)
*(Cập nhật lần cuối: 2026-05-07 — Đồng bộ với code EquipmentData.cs thực tế)*

---

## ⚠️ Thay đổi Lớn so với GDD Cũ

| Hạng mục | GDD Cũ | Code Thực Tế |
|---|---|---|
| Số slot trang bị | **2** (Weapon, Armor) | **6** (Weapon, Armor, Helm, Boots, Ring1, Ring2) |
| Số bậc hiếm (Tier) | **3** (Rarity 1-3) | **7** (D, C, B, A, S, SS, SSS) |
| Chỉ số phụ | 4 loại cơ bản | 9 loại bao gồm multiplier |
| Class restriction | Không có | Có (`classRestriction: Profession`) |
| Lock item | Không có | Có (`isLocked: bool`) |
| Icon path | Không xác định | `Resources/Icons/Equipments/{Profession}_{Slot}_{Tier}` |

---

## 1. Enum `EquipmentSlot`

```csharp
enum EquipmentSlot {
    Weapon,  // Vũ khí
    Armor,   // Giáp thân
    Helm,    // Mũ giáp
    Boots,   // Giày
    Ring1,   // Nhẫn 1
    Ring2    // Nhẫn 2
}
```

Mỗi hero có thể trang bị tối đa 6 món (`Dictionary<EquipmentSlot, EquipmentData>`).

---

## 2. Enum `EquipmentTier` (7 bậc)

| Tier | Tên | Màu gợi ý |
|---|---|---|
| D | Thường | Xám |
| C | Không phổ biến | Xanh lá |
| B | Hiếm | Xanh dương |
| A | Sử thi | Tím |
| S | Huyền thoại | Cam |
| SS | Thần thánh | Vàng |
| SSS | Vô thượng | Đỏ |

---

## 3. Cấu trúc `EquipmentData`

### 3.1. Định danh

| Trường | Kiểu | Mô tả |
|---|---|---|
| `id` | `string` | ID duy nhất (GUID khi sinh ngẫu nhiên) |
| `equipmentName` | `string` | Tên hiển thị |
| `slot` | `EquipmentSlot` | Slot được trang bị vào |
| `tier` | `EquipmentTier` | Bậc hiếm |
| `level` | `int` | Cấp độ hiện tại (1–100) |
| `experience` | `int` | EXP tích lũy để lên cấp |
| `classRestriction` | `Profession` | Nghề yêu cầu (`None` = mọi nghề đều dùng được) |
| `isLocked` | `bool` | Khóa — không cho phép dùng làm vật liệu cường hóa |

### 3.2. Chỉ số Cộng Thẳng (Flat Bonus)

| Trường | Mô tả |
|---|---|
| `hpBonus` | Cộng thẳng vào HP |
| `atkBonus` | Cộng thẳng vào ATK |
| `defBonus` | Cộng thẳng vào DEF |
| `spdBonus` | Cộng thẳng vào SPD |
| `evasionBonus` | Tỉ lệ né tránh (%) |
| `damageReductionBonus` | Giảm sát thương nhận (%) |
| `damageIncreaseBonus` | Tăng sát thương gây ra (%) |
| `critChanceBonus` | Cộng thêm tỉ lệ chí mạng (%) |
| `critDamageBonus` | Cộng thêm hệ số chí mạng (%) |

### 3.3. Chỉ số Nhân Hệ Số (Multiplier)

| Trường | Mô tả | Áp dụng |
|---|---|---|
| `hpMultiplier` | Nhân HP cuối (1.0 = không đổi) | Sau khi cộng flat |
| `atkMultiplier` | Nhân ATK cuối | Sau khi cộng flat |
| `defMultiplier` | Nhân DEF cuối | Sau khi cộng flat |
| `spdMultiplier` | Nhân SPD cuối | Sau khi cộng flat |

---

## 4. Sinh Trang bị Ngẫu nhiên (`EquipmentSystem.GenerateRandomEquipment`)

### 4.1. Đầu vào

```
GenerateRandomEquipment(int floorEquivalent)
```

`floorEquivalent` thường là độ khó của tháp hoặc boss (ví dụ: 30 cho Boss).

### 4.2. Công thức Level Rơi

```
dropLevel = Random(floorEquivalent - 5, floorEquivalent + 2)
dropLevel = Clamp(dropLevel, 1, 40)
```

Hiện tại soft cap ở level 40 từ drop.

### 4.3. Phân bổ Slot và Tier

*(Chưa document đầy đủ trong code — cần xác nhận implementation `EquipmentSystem.cs`)*

Dự kiến: Slot ngẫu nhiên trong 6 slot, Tier tính theo `floorEquivalent` hoặc ngẫu nhiên có trọng số.

---

## 5. Hệ thống Cường hóa (Nâng cấp Level)

### 5.1. EXP từ Vật liệu

```
GetExpYield(equipment) = 5 + (equipment.level * 2) + (equipment.experience / 2)
```

### 5.2. EXP Cần để Lên Cấp (Soft Cap tại 40)

```
Level 1–39: EXP cần = level * 10   (tuyến tính)
Level 40+ : EXP cần = 500 * (1.2 ^ (level - 40))   (hàm mũ)
```

### 5.3. Lưu ý

- `isLocked = true` → không thể dùng làm vật liệu
- Khi cường hóa qua ngưỡng EXP, tự động lên cấp và tính lại base stats

---

## 6. Icon và Hiển thị

**Path icon:** `Resources/Icons/Equipments/{Profession}_{Slot}_{Tier}`

Ví dụ:
- `Warrior_Weapon_S` — Vũ khí S-tier cho Chiến binh
- `None_Armor_A` — Giáp A-tier không giới hạn nghề
- `Mage_Ring1_SS` — Nhẫn 1 SS-tier cho Pháp sư

---

## 7. Tích hợp vào HeroData

```csharp
// Trang bị vào hero
hero.Equipments[EquipmentSlot.Weapon] = weaponData;

// GetFinalStats() tự động áp dụng:
// stats += Σ(equipment.flatBonuses)
// stats *= Π(equipment.multipliers)
```

---

## 8. Serialization

`Dictionary<EquipmentSlot, EquipmentData>` không serialize được native trong Unity. HeroData xử lý bằng 2 List song song:

```csharp
[SerializeField] List<EquipmentSlot> _equipSlotKeys
[SerializeField] List<EquipmentData> _equipSlotValues
```

Phương thức `Clone()` trong EquipmentData sao chép deep copy đầy đủ (cần cho combat simulation).

---

## 9. Trạng thái Triển khai

| Tính năng | Trạng thái |
|---|---|
| EquipmentData model (6 slot, 7 tier) | ✅ Hoàn chỉnh |
| GetFinalStats() áp dụng equipment | ✅ Hoàn chỉnh |
| GenerateRandomEquipment (drop) | ✅ Hoạt động |
| ExpeditionManager drop equipment | ✅ Hoạt động |
| InventoryPanel hiển thị equipment | ⚠️ Script có, bindings chưa xác nhận |
| UI trang bị vào hero slot | ❓ Cần kiểm tra |
| UI cường hóa (feed equipment) | ❓ Cần kiểm tra |

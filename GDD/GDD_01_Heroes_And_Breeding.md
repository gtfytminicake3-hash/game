# GDD 01 — Hệ thống Anh hùng, Di truyền & Tiến hóa
*(Cập nhật lần cuối: 2026-05-07 — Đồng bộ với code thực tế)*

Tài liệu này mô tả chi tiết về cấu trúc của một Anh hùng, cơ chế lai tạo và hệ thống tiến hóa, dựa trên mã nguồn C# hiện tại của dự án.

## 1. Cấu trúc Dữ liệu Anh hùng (`HeroData.cs`)

Mỗi anh hùng được định nghĩa bởi lớp `HeroData`, phản ánh một hệ thống tùy biến sâu sắc.

### 1.1. Lớp `HeroStats` (Chỉ số của Anh hùng)

| Tên Trường | Kiểu Dữ Liệu | Mô tả |
| --- | --- | --- |
| `hp` | `float` | Máu tối đa. |
| `atk` | `float` | Sức tấn công. |
| `def` | `float` | Sức phòng thủ. |
| `spd` | `float` | Tốc độ. |
| `critChance` | `float` | Tỉ lệ chí mạng (cơ bản là `0.05f` tức 5%). |
| `critDamage` | `float` | Sát thương chí mạng (cơ bản là `1.5f` tức 150%). |

### 1.2. Lớp `HeroData` (Dữ liệu chính)

| Tên Trường | Kiểu Dữ Liệu | Mô tả |
| --- | --- | --- |
| `id` | `string` | ID định danh duy nhất cho mỗi hero (GUID). |
| `heroName` | `string` | Tên của hero, lấy từ hệ thống localization. |
| `gender` | `Gender` | Giới tính (`Male` hoặc `Female`). |
| `avatarIndex` | `int` | Index của ảnh đại diện, quản lý bởi `AvatarManager`. |
| `level` | `int` | Cấp độ hiện tại, bắt đầu từ 1. |
| `experience` | `int` | Điểm kinh nghiệm của cấp hiện tại. |
| `potential` | `int` | **(QUAN TRỌNG)** Tiềm năng (1-20), chỉ số di truyền cốt lõi. |
| `baseStats` | `HeroStats` | Các chỉ số gốc của hero, được tính 1 lần lúc sinh ra dựa trên `potential`. **Không bao giờ thay đổi.** |
| `addedStats` | `HeroStats` | Các điểm chỉ số do người chơi cộng vào khi lên cấp. |
| `freeStatPoints` | `int` | Số điểm chỉ số đang chờ người chơi phân phối. |
| `evasionRate`, `damageReduction`, `damageIncrease` | `float` | Các chỉ số chiến đấu phụ. |
| `currentHp` | `float` | Lượng máu hiện tại. |
| `traitIDs` | `List<string>` | **QUAN TRỌNG:** Danh sách các ID của `Trait`, không phải đối tượng `Trait`. |
| `skillIDs` | `List<string>` | **QUAN TRỌNG:** Danh sách các ID của `Skill`, không phải đối tượng `Skill`. |
| `profession` | `Profession` | Nghề nghiệp (`None`, `Warrior`, `Archer`, `Mage`, `Healer`). |
| `isMature` | `bool` | `true` nếu hero đã trưởng thành. |
| `maturationEndTime` | `long` | Mốc thời gian (Unix timestamp) khi quá trình trưởng thành kết thúc. |
| `isLightlyInjured` | `bool` | `true` nếu bị thương nhẹ. |
| `lightInjuryEndTime` | `long` | Mốc thời gian khi hồi phục xong vết thương nhẹ. |
| `isSeverelyInjured` | `bool` | `true` nếu bị thương nặng. |
| `injuryEndTime` | `long` | Mốc thời gian khi hồi phục xong vết thương nặng. |

**⚠️ Các trường mới trong code — chưa có trong GDD cũ:**

| Tên Trường | Kiểu Dữ Liệu | Mô tả |
|---|---|---|
| `Equipments` | `Dictionary<EquipmentSlot, EquipmentData>` | 6 slot trang bị (Weapon, Armor, Helm, Boots, Ring1, Ring2). Serialized thủ công qua 2 List. |
| `breedingCount` | `int` | Số lần đã lai tạo (bắt đầu = 0). |
| `maxBreedingCount` | `int` | Giới hạn lai tạo tối đa (mặc định = 10). |
| `guaranteedProfession` | `Profession?` | Nghề nghiệp được đảm bảo khi con cái tiến hóa. Nullable. |
| `evasionRate` | `float` | Tỉ lệ né tránh. |
| `damageReduction` | `float` | Giảm sát thương nhận (%). |
| `damageIncrease` | `float` | Tăng sát thương gây ra (%). |

### 1.3. Các Hàm Tính Toán Quan Trọng

*   **`GetFinalStats()`**: Tính toán chỉ số cuối cùng để dùng trong chiến đấu.
    1.  Lấy `baseStats` cộng với `addedStats`.
    2.  Cộng tất cả các giá trị cộng thẳng từ hiệu ứng `ADD_STAT` của Trait.
    3.  Cộng dồn các giá trị phần trăm từ `MULTIPLY_STAT` vào các hệ số nhân riêng (ví dụ: `multiplyHp`, `multiplyAtk`).
    4.  **[MỚI — không có trong GDD cũ]** Cộng bonus từ Equipment được trang bị (`Equipments` dict): `hpBonus, atkBonus, defBonus, spdBonus` (cộng thẳng) + `hpMultiplier, atkMultiplier, defMultiplier, spdMultiplier` (nhân hệ số).
    5.  Nhân chỉ số đã cộng với hệ số nhân tổng hợp.
    6.  Kết quả cuối cùng được làm tròn xuống (`Mathf.FloorToInt`).

*   **`GetCombatPower()`**: Tính Sức mạnh Chiến đấu (CP).
    *   **Công thức:** `CP = floor(HP/10 + ATK*2 + DEF*3 + SPD*1.5)`.

*   **`AddExperience(int amount)`**: Xử lý việc nhận kinh nghiệm và lên cấp.
    *   Khi đủ kinh nghiệm, hero sẽ lên cấp.
    *   **Thay vì tự tăng chỉ số, hero nhận `freeStatPoints` bằng với `potential` của mình.**
    *   Phát ra sự kiện `OnHeroLeveledUp` để các hệ thống khác (như `EvolutionSystem`) lắng nghe và xử lý logic tiến hóa.

*   **`IsBusy()`**: Kiểm tra xem hero có đang trong một hoạt động (chưa trưởng thành, bị thương, đi thám hiểm) hay không.

## 2. Cấu trúc `Trait` và `Skill` (ScriptableObjects)

`Trait` và `Skill` không được lưu trực tiếp trong `HeroData` mà được quản lý dưới dạng `ScriptableObject` và được tham chiếu bằng ID.

### 2.1. `Trait.cs` (Cấu trúc mới)

Ngoài các trường cũ, `Trait` giờ có thêm các trường để hỗ trợ hệ thống tiến hóa và nâng cấp:
*   **`rank`**: `RarityRank` (D, C, B, A, S).
*   **`familyId`**: Một chuỗi định danh "họ" của Trait (ví dụ: "ATK_UP", "HP_ON_HIT").
*   **`nextUpgradeTraitID`**: ID của Trait cấp cao hơn trong cùng một `familyId`.

### 2.1. `Trait.cs`

*   **`id`**: ID duy nhất (`S_04`, `SS_07`,...).
*   **`traitName`**: Tên hiển thị.
*   **`description`**: Mô tả hiển thị.
*   **`effects`**: Một danh sách các `TraitEffect`. Mỗi `TraitEffect` có:
    *   **`type`**: `ADD_STAT`, `MULTIPLY_STAT`, `AURA`, `SPECIAL`.
    *   **`hp`, `atk`, `def`, `spd`**: Giá trị số nguyên cho hiệu ứng.
    *   **`effectDescription`**: Mô tả cho game designer.

### 2.2. `Skill.cs`

*   **`id`**: ID duy nhất (`SK_WARRIOR_01`,...).
*   **`skillName`**: Tên hiển thị.
*   **`description`**: Mô tả hiển thị.
*   **`type`**: `Active` hoặc `Passive`.
*   **`requiredProfession`**: Nghề nghiệp yêu cầu.
*   **`cooldown`, `targeting`, `powerRatio`, `hitCount`**: Các thông số cho hệ thống chiến đấu.
*   **`appliedEffect`, `effectDuration`, `effectChance`**: Các thông số cho hiệu ứng trạng thái.

## 3. Hệ thống Lai tạo (`BreedingSystem.cs`)

*(Cập nhật toàn bộ — code thực tế khác biệt lớn so với GDD cũ)*

### 3.1. BreedingOptions

```csharp
class BreedingOptions {
    bool UseMutationPotion  // +50% mutationChance khi lai
    string GuaranteedTraitID  // Trait được đảm bảo cho con
}
```

### 3.2. Chi phí Lai tạo (Dynamic Cost)

**GDD cũ ghi phí cố định — thực tế là phí động:**

```
Chi phí = baseBreedingCost + (tổng_breedingCount_2_cha_mẹ * costPerBreedingCount)
         = 500 + ((p1.breedingCount + p2.breedingCount) * 200)
```

Ví dụ: 2 hero mỗi người đã lai 2 lần → `500 + (4 * 200) = 1300 Gold`.

### 3.3. Điều kiện Lai tạo (`CanBreed`)

| Điều kiện | Chi tiết |
|---|---|
| Phải chọn đủ 2 hero | Cả hai phải khác null |
| Phải khác ID | Không thể tự lai với chính mình |
| Phải khác giới tính | 1 Nam + 1 Nữ |
| Phải trưởng thành | `isMature == true` |
| Chưa đạt giới hạn | `breedingCount < maxBreedingCount` (mặc định max = 10) |
| Đủ Gold | Kiểm tra theo chi phí động ở 3.2 |

### 3.4. Tính Potential cho Con

```
1. basePot = (parent1.potential + parent2.potential) / 2
   → Nếu potential bằng nhau: basePot = parent1.potential

2. Kiểm tra đột biến (mutationChance = 0.15f mặc định, +0.5f nếu dùng potion):
   - 5% của mutationChance  → basePot += Random(7, 10)  [Massive jump — có thể SSS]
   - 15% của mutationChance → basePot += Random(4, 7)   [Rare jump — có thể SS]
   - 80% còn lại            → basePot += Random(1, 4)   [Normal jump]

3. child.potential = Clamp(basePot, 1, 100)
```

### 3.5. Di truyền Nghề nghiệp (Profession Inheritance)

**Tính năng mới hoàn toàn — không có trong GDD cũ:**

```
Roll ngẫu nhiên [0.0, 1.0]:
  < 0.45 → con kế thừa nghề của parent1
  < 0.90 → con kế thừa nghề của parent2
  ≥ 0.90 → con có nghề ngẫu nhiên hoàn toàn (10% mutation)
```

Con được gọi `SetProfession()` → tự động assign starting skills theo nghề.

### 3.6. Di truyền Trait (`InheritTraits`)

```
HashSet<string> inherited = {}

// Từ parent1: mỗi trait có 50% cơ hội di truyền
// Từ parent2: mỗi trait có 50% cơ hội di truyền

// 20% cơ hội sinh trait đột biến ngẫu nhiên (từ DataManager.AllTraits)
```

Kết quả: HashSet → List (loại trùng tự động).

### 3.7. Đặt Tên Con (Name Generation)

**Tính năng mới — không có trong GDD cũ:**

```
n1 = parent1.heroName (hoặc "Hero" nếu ngắn < 3 ký tự)
n2 = parent2.heroName

part1 = n1[0 .. n1.Length/2]        // Nửa đầu tên cha/mẹ 1
part2 = n2[n2.Length/2 .. end]      // Nửa sau tên cha/mẹ 2

childName = Capitalize(part1 + part2)
```

Ví dụ: "Arthur" + "Merlin" → "ArMerlin" → "Arlin"

### 3.8. Giới hạn Lai tạo của Con

```
child.maxBreedingCount = Max(0, Min(p1.max, p2.max) - 1)
// Đảm bảo tối thiểu 3
if (child.maxBreedingCount < 3) child.maxBreedingCount = 3
```

Con luôn có giới hạn lai tạo thấp hơn cha mẹ để ngăn thế hệ vô hạn.

### 3.9. Trạng thái Sơ sinh

```
child.isMature = false
child.maturationEndTime = now + (maturationTimeMinutes * 60 * 1000)
// maturationTimeMinutes = 1.0f mặc định = 1 phút thực tế
```

## 4. Hệ thống Tiến hóa (`EvolutionSystem.cs`)

**⚠️ Chú ý: GDD cũ ghi EvolutionSystem là "lớp rỗng". Thực tế code đã IMPLEMENT ĐẦY ĐỦ.**

### 4.1. Cơ chế

EvolutionSystem subscribe vào `HeroData.OnHeroLeveledUp` (static event). Khi hero lên cấp, hệ thống đọc cấu hình từ `DataManager.GameConfig.EvolutionTable`.

### 4.2. Mốc Tiến hóa (đọc từ `EvolutionTable` ScriptableObject)

Mỗi entry trong EvolutionTable có: `requiredLevel`, `rewardType` (NewTrait / ProfessionSelect / TraitUpgrade / NewSkill).

**Các mốc GDD đề cập (cần điền vào ScriptableObject):**

| Cấp | Sự kiện | Chi tiết |
|---|---|---|
| 20, 40, 80 | Nhận Trait mới | `GrantNewTrait_TwoRolls()` |
| 60, 100 | Nâng cấp Trait | `OnProfessionSelectionRequested` event → UI xử lý |
| Tùy cấu hình | Kỹ năng mới | `GrantNewSkill()` |

### 4.3. Logic Cấp Trait Mới (`GrantNewTrait_TwoRolls`)

```
1. Roll "family" — chọn nhóm trait chưa có (familyId chưa tồn tại trong traitIDs)
2. Roll "rarity" — trong family đó, roll theo RarityConfig
   (RarityConfig được đọc từ DataManager.GameConfig.RaritySettings)
3. Thêm traitID vào hero.traitIDs
```

### 4.4. Dependency quan trọng

EvolutionSystem phụ thuộc vào:
- `DataManager.GameConfig.EvolutionTable` — phải có dữ liệu
- `DataManager.AllTraits` — phải được load
- `DataManager.GameConfig.RaritySettings` — phải có dữ liệu

**Nếu EvolutionTable rỗng → không có milestone → hệ thống im lặng, không báo lỗi.**

## 5. Hệ thống Tuyển mộ (`RecruitmentSystem.cs`)

*   **Cơ chế:** Cho phép người chơi nhận hero mới thông qua cơ chế "gacha".
*   **Logic:**
    1.  Quay tỉ lệ để xác định bậc hiếm (S, A, B, C, D).
    2.  Dựa vào bậc hiếm, xác định một khoảng `potential` (ví dụ: Bậc A -> POT từ 13-16).
    3.  Tạo hero mới với `potential` đó, tính `baseStats`, và gán 3 Trait ngẫu nhiên.

## 6. Hệ thống Dân số

*   **Giới hạn:** Số lượng hero tối đa được quyết định bởi cấp của nhà chính (`MainHall`).
*   **Công thức:** `10 + (cấp_nhà_chính * 2)`.
*   **Xử lý quá tải:** Khi nhận hero mới (từ lai tạo hoặc tuyển mộ) mà dân số đã đầy, một thông báo sẽ hiện ra. **(Chưa có panel `PopulationManagerPanel` để buộc người chơi sa thải hero cũ)**.

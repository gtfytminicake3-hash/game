# GDD - Hệ thống Anh hùng, Di truyền & Tiến hóa (Đã cập nhật)

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

### 1.3. Các Hàm Tính Toán Quan Trọng

*   **`GetFinalStats()`**: Tính toán chỉ số cuối cùng để dùng trong chiến đấu.
    1.  Lấy `baseStats` cộng với `addedStats`.
    2.  Cộng tất cả các giá trị cộng thẳng từ hiệu ứng `ADD_STAT` của Trait.
    3.  Cộng dồn các giá trị phần trăm từ `MULTIPLY_STAT` vào các hệ số nhân riêng (ví dụ: `multiplyHp`, `multiplyAtk`).
    4.  Nhân chỉ số đã cộng với hệ số nhân tương ứng.
    5.  Kết quả cuối cùng được làm tròn xuống (`Mathf.FloorToInt`).

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

Hệ thống này xử lý logic khi lai tạo hai hero.

*   **Đầu vào:** `HeroData father`, `HeroData mother`, và một đối tượng `BreedingOptions`.
*   **`BreedingOptions`**:
    *   `UseMutationPotion` (`bool`): Nếu `true`, tăng tỉ lệ đột biến lên 50%.
    *   `GuaranteedTraitID` (`string`): ID của Trait được đảm bảo di truyền (sử dụng bởi Trait `SSS_04`).

### 3.1. Công thức Di truyền Chỉ số (`CalculateInheritedStat`)

**Hệ thống cũ đã bị loại bỏ.** Chỉ số của con được tính như sau:
1.  **`potential`**: `(potential_cha + potential_mẹ) / 2`.
2.  **`baseStats`**: Được tính một lần duy nhất dựa trên `potential` mới của con theo công thức: `BaseStat = potential * Random.Range(8, 11)`.

### 3.2. Công thức Di truyền Trait (`InheritTraits`)

**Hệ thống cũ đã được thay thế.** Logic mới như sau:
1.  Sử dụng `HashSet` để đảm bảo 3 Trait cuối cùng là duy nhất.
2.  **Trait đảm bảo:** Nếu có `GuaranteedTraitID` trong `BreedingOptions`, thêm nó vào trước.
3.  **Trait từ Cha:** Lấy một Trait ngẫu nhiên từ danh sách Trait của cha (nếu nó chưa có trong `HashSet`).
4.  **Trait từ Mẹ:** Lấy một Trait ngẫu nhiên từ danh sách Trait của mẹ (nếu nó chưa có trong `HashSet`).
5.  **Trait ngẫu nhiên:** Nếu chưa đủ 3 Trait, lấy ngẫu nhiên từ toàn bộ danh sách Trait trong game (`DataManager.AllTraits`) cho đến khi đủ.

### 3.3. Các Trait Đặc biệt

*   **Song Sinh (`S_04`):** `2%` cơ hội sinh đôi (hàm `Breed` trả về danh sách 2 hero).
*   **Kẻ Chọn Lọc Gene (`SSS_04`):** Logic được xử lý ở UI, truyền `GuaranteedTraitID` vào `BreedingOptions`. **(Đã triển khai)**
*   **Dòng Dõi Tinh Anh (`SS_07`):** `10%` cơ hội cho con cái được nhân tất cả chỉ số cơ bản với `1.05f`.

## 4. Hệ thống Tiến hóa (`EvolutionSystem.cs`)

**Hệ thống này đã được đại tu hoàn toàn theo `update_planv2.md`.**

*   **Cơ chế:** Hệ thống lắng nghe sự kiện `HeroData.OnHeroLeveledUp`.
*   **Mốc nhận Trait mới (Cấp 20, 40, 80):** Hero sẽ được nhận một Trait mới thuộc một "họ" (family) mà hero chưa sở hữu.
*   **Mốc nâng cấp Trait (Cấp 60, 100):** Người chơi sẽ được phép chọn một trong các Trait hiện có để nâng cấp lên bậc hiếm cao hơn (dựa trên `nextUpgradeTraitID`). Logic này được xử lý bởi `TraitUpgradePanel.cs`.
*   **Lưu ý:** Logic chi tiết của `EvolutionSystem` cần được triển khai theo kế hoạch. Hiện tại, nó chỉ là một lớp rỗng.

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

# GDD - Hệ thống Anh hùng, Di truyền & Tiến hóa (Cập nhật theo Code)

Tài liệu này mô tả chi tiết về cấu trúc của một Anh hùng, cơ chế lai tạo và hệ thống tiến hóa, dựa trên mã nguồn C# hiện tại của dự án.

## 1. Cấu trúc Dữ liệu Anh hùng (`HeroData.cs`)

Mỗi anh hùng được định nghĩa bởi lớp `HeroData`. Dữ liệu được tổ chức như sau.

### 1.1. Lớp `HeroStats` (Chỉ số của Anh hùng)

| Tên Trường | Kiểu Dữ Liệu | Mô tả |
| --- | --- | --- |
| `hp` | `float` | Máu tối đa. |
| `atk` | `float` | Sức tấn công. |
| `def` | `float` | Sức phòng thủ. |
| `spd` | `float` | Tốc độ. |
| `critChance` | `float` | Tỉ lệ chí mạng (cơ bản là `0.00f` tức 0%). |
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
| `potential` | `int` | Tiềm năng, ảnh hưởng đến chỉ số nhận được khi lên cấp. |
| `baseStats` | `HeroStats` | Các chỉ số gốc của hero. |
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

*   **`GetFinalStats()`**: Tính toán chỉ số cuối cùng sau khi áp dụng hiệu ứng từ `Trait`.
    1.  Lấy `baseStats`.
    2.  Cộng tất cả các giá trị từ hiệu ứng `ADD_STAT`.
    3.  Cộng dồn các giá trị phần trăm từ `MULTIPLY_STAT` vào các hệ số nhân riêng (ví dụ: `multiplyHp`, `multiplyAtk`).
    4.  Nhân chỉ số đã cộng với hệ số nhân tương ứng.
    5.  Kết quả cuối cùng được làm tròn xuống (`Mathf.FloorToInt`).

*   **`GetCombatPower()`**: Tính Sức mạnh Chiến đấu (CP).
    *   **Công thức:** `CP = floor(HP/10 + ATK*2 + DEF*3 + SPD*1.5)`.

*   **`GainExp(int amount)`**: Xử lý việc nhận kinh nghiệm và lên cấp.
    *   Khi đủ kinh nghiệm, hero sẽ lên cấp và nhận điểm chỉ số dựa trên `potential`.
    *   Phát ra sự kiện `OnHeroLeveledUp` để các hệ thống khác (như `EvolutionSystem`) lắng nghe.

*   **`IsBusy()`**: Kiểm tra xem hero có đang trong một hoạt động (chưa trưởng thành, bị thương, đi thám hiểm) hay không.

## 2. Cấu trúc `Trait` và `Skill` (ScriptableObjects)

`Trait` và `Skill` không được lưu trực tiếp trong `HeroData` mà được quản lý dưới dạng `ScriptableObject` và được tham chiếu bằng ID.

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

Hàm này trả về một giá trị `float` đã được làm tròn xuống.
1.  Lấy `mutationChance` (mặc định 10%, hoặc 50% nếu dùng Potion).
2.  Quay số `roll` (0.0 đến 1.0).
3.  Kiểm tra các trường hợp theo thứ tự:
    *   `roll < mutationChance`: **Đột biến** (`Trung bình cộng Bố Mẹ * 1.1`).
    *   `roll < mutationChance + 0.3f`: Nhận chỉ số của **Bố**.
    *   `roll < mutationChance + 0.3f + 0.3f`: Nhận chỉ số của **Mẹ**.
    *   Còn lại: Nhận giá trị **ngẫu nhiên** trong khoảng [Bố, Mẹ].

### 3.2. Công thức Di truyền Trait (`InheritTraits`)

*   Sử dụng `HashSet` để đảm bảo không có Trait trùng lặp. Tối đa 3 Trait.
*   Nếu có `GuaranteedTraitID`, Trait đó sẽ được thêm vào đầu tiên.
*   Lặp 3 lần (hoặc cho đến khi đủ 3 Trait):
    1.  Quay số `roll`.
    2.  `roll < 0.30f`: Lấy 1 Trait ngẫu nhiên từ **Bố**.
    3.  `roll < 0.60f`: Lấy 1 Trait ngẫu nhiên từ **Mẹ**.
    4.  `roll < 0.70f`: Lấy 1 Trait ngẫu nhiên từ **toàn bộ danh sách Trait trong game**.
    5.  Còn lại: Không nhận được Trait trong lần lặp này.

### 3.3. Các Trait Đặc biệt

*   **Song Sinh (`S_04`):** `2%` cơ hội sinh đôi (hàm `Breed` trả về danh sách 2 hero).
*   **Kẻ Chọn Lọc Gene (`SSS_04`):** Logic được xử lý ở UI, truyền `GuaranteedTraitID` vào `BreedingOptions`.
*   **Dòng Dõi Tinh Anh (`SS_07`):** `10%` cơ hội cho con cái được nhân tất cả chỉ số cơ bản với `1.05f`.

## 4. Hệ thống Tiến hóa (`EvolutionSystem.cs`)

Hệ thống này hoàn toàn mới và chưa có trong GDD cũ.

*   **Cơ chế:** Hệ thống lắng nghe sự kiện `HeroData.OnHeroLeveledUp`.
*   **Điều kiện:** Khi một hero đạt các mốc cấp độ **30, 50, 70, 100**, hệ thống sẽ được kích hoạt.
*   **Phần thưởng:**
    *   Hệ thống sẽ tìm phần thưởng tương ứng với `Profession` và `level` của hero.
    *   Phần thưởng có thể là một `Skill` (ID bắt đầu bằng `SK_`) hoặc `Trait` (ID bắt đầu bằng `TR_`).
    *   ID của phần thưởng sẽ được thêm vào danh sách `skillIDs` hoặc `traitIDs` của hero.
    *   **Lưu ý:** Logic lấy phần thưởng hiện tại đang là giả lập (`GetPlaceholderRewardId`), cần được thay thế bằng cách đọc từ `GameConfig`.
 Thêm hệ thông tuyển người bên ngoài bằng cách gacha có garntie rank điểm tiềm năng random trait. 
 điểm tiềm năng dùng để gaachsa trong sinh đẻ, mỗi 20 cấp nhận trait mới trừ cấp 60 và 100 cho phép nâng trait 
 điểm tiềm năng và trait có 5 rank green, blue, tím, vàng, đỏ 60% 30 6,5 3 0,5
 làm bảng 20 trait
 nhân vật sinh ra chắc chắn có 3 trait, và nhận thêm 3 trait trong quá trình lên cấp
 nếu nhân vật đã có 4 trailt đầu là A thì chắc chắn trait tiếp theo nhận được là B trở xuống


 nhà nên để 20 cấp
 nhà sinh sản nâng cấp để tăng số lượng có thể sinh sản cùng lúc
nhà chính tăng cấp cuối cùng 200 dân số lv1 là 100 vượt quá phải chọn người để đuổi

nhiệm vụ của game . ban đầu giới thiệu cac nhà ban đầu , khóa màn hình, arena sẽ mở sau khi chơi đc 30p và nhà chính lên cấp 5 . sau đó nhiệm vụ sẽ là nvuj ngày , tuần , thành tựu ( thưởng ít. nhưng nhận nhiều lần. nhận nhỏ giọt)

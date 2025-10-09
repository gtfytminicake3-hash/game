# GDD - Hệ thống Anh hùng & Di truyền

Tài liệu này mô tả chi tiết về cấu trúc của một Anh hùng và cơ chế lai tạo.

## 1. Thuộc tính Anh hùng (`HeroData.ts`)

Mỗi anh hùng được đại diện bởi lớp `HeroData` và có các thuộc tính sau:

*   **Chỉ số Cơ bản (`baseStats`):** `hp`, `atk`, `def`, `spd`.
*   **Máu Hiện tại (`currentHp`):** Được khởi tạo bằng HP tối đa và giảm trong chiến đấu.
*   **Tiềm năng (`potential`):** Quyết định tốc độ tăng trưởng chỉ số khi lên cấp.
*   **Giới tính (`gender`):** `Male` hoặc `Female`, được quyết định ngẫu nhiên 50/50 khi sinh ra.
*   **Đặc tính (`traits`):** Một mảng các `Trait` bị động.
*   **Kỹ năng (`skills`):** Một mảng các `Skill` hoạt động.
*   **Nghề nghiệp (`profession`):** `Warrior`, `Archer`, `Mage`, `Healer`.
*   **Trạng thái:**
    *   `isMature`, `maturationEndTime`: Trạng thái trưởng thành.
    *   `isLightlyInjured`, `lightInjuryEndTime`: Trạng thái bị thương nhẹ.
    *   `isSeverelyInjured`, `injuryEndTime`: Trạng thái bị thương nặng.

## 2. Hệ thống Lai tạo (`BreedingSystem.ts`)

*   **Điều kiện:** Chỉ có thể lai tạo giữa hai hero khác giới tính. Giao diện người dùng (`BreedingUIController`) sẽ lọc danh sách để người chơi không thể chọn hai hero cùng giới tính.

*   **Công thức Di truyền Chỉ số:**
    *   `10%` cơ hội **Đột biến**: `Kết quả = Trung bình cộng của Bố Mẹ + 10% * Trung bình cộng`.
    *   `30%` cơ hội nhận chỉ số của **Bố**.
    *   `30%` cơ hội nhận chỉ số của **Mẹ**.
    *   `30%` cơ hội nhận giá trị **ngẫu nhiên** trong khoảng [Bố, Mẹ].

*   **Công thức Di truyền Trait:** (Một anh hùng có thể có tối đa 3 Trait)
    *   `30%` cơ hội nhận 1 Trait ngẫu nhiên từ **Bố**.
    *   `30%` cơ hội nhận 1 Trait ngẫu nhiên từ **Mẹ**.
    *   `10%` cơ hội nhận 1 Trait ngẫu nhiên từ **toàn bộ danh sách**.

*   **Các Trait Đặc biệt đã triển khai:**
    *   **Song Sinh (S_04):** `2%` cơ hội sinh đôi.
    *   **Kẻ Chọn Lọc Gene (SSS_04):** Cho phép chọn 1 Trait để chắc chắn di truyền (thông qua `BreedingOptions`).
    *   **Dòng Dõi Tinh Anh (SS_07):** `10%` cơ hội cho con +5% tất cả chỉ số cơ bản.

## 3. Hàm Tính toán Phụ (`HeroData.ts`)

*   **`getFinalStats()`:** Tính toán chỉ số cuối cùng của hero sau khi áp dụng tất cả các hiệu ứng `ADD_STAT` và `MULTIPLY_STAT` từ `Trait`.
    1.  Lấy `baseStats`.
    2.  Cộng tất cả các giá trị `ADD_STAT`.
    3.  Cộng dồn tất cả các giá trị `MULTIPLY_STAT` vào một hệ số nhân.
    4.  Nhân chỉ số đã cộng với hệ số nhân cuối cùng.

*   **`getCombatPower()`:** Tính toán một chỉ số Sức mạnh Chiến đấu (CP) đơn giản để hiển thị trên UI.
    *   **Công thức:** `CP = floor(HP/10 + ATK*2 + DEF*3 + SPD*1.5)`.
    *   **Hiển thị:** CP được hiển thị trên `HeroCard` và `HeroInfoPanel`.

---

### Lỗ hổng & Cơ hội Phát triển

*   **Đã giải quyết:** Giao diện lai tạo đã được nâng cấp với `HeroPickerPanel`, cho phép chọn hero từ danh sách được sắp xếp theo CP.
*   **Cần làm:** Tích hợp logic sử dụng các vật phẩm lai tạo (Thuốc Biến Dị, Bùa Ước nguyện) vào UI và hệ thống.
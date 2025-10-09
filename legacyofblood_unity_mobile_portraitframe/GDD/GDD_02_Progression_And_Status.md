# GDD - Hệ thống Tiến triển & Trạng thái

Tài liệu này mô tả các hệ thống quản lý vòng đời và trạng thái của một anh hùng sau khi được sinh ra.

## 1. Hệ thống Trưởng thành (`MaturationSystem.ts`)

*   **Mục đích:** Quản lý quá trình một hero sơ sinh trở thành một chiến binh sẵn sàng chiến đấu.
*   **Kích hoạt:** Được gọi bởi `BreedingSystem` sau khi một hero được tạo ra.
*   **Thời gian:** Cố định là **1 phút** (60,000 ms).
*   **Trait ảnh hưởng:** "Lớn Nhanh" (D_08) giảm 5% thời gian này.
*   **Quá trình Thức tỉnh:** Khi trưởng thành, hero sẽ:
    1.  Nhận ngẫu nhiên 1 trong 4 **Nghề nghiệp** (`Warrior`, `Archer`, `Mage`, `Healer`).
    2.  Nhận ngẫu nhiên 1 trong 3 **Kỹ năng** khởi đầu của nghề đó.

## 2. Hệ thống Lên cấp (`HeroData.ts`)

*   **Kích hoạt:** Phương thức `gainExp(amount)` được gọi.
*   **Công thức EXP Yêu cầu:** `EXP(N) = round((EXP(N-1) * 1.15) + (N * 10))` (N là cấp độ hiện tại).
*   **Công thức Tăng chỉ số khi Lên cấp:**
    *   `Điểm Phân phối = floor(Tiềm năng / 2)`
    *   `HP tăng thêm = floor(Điểm Phân phối * 1.5)`
    *   `ATK tăng thêm = Điểm Phân phối`
    *   `DEF tăng thêm = Điểm Phân phối`

## 3. Hệ thống Tiến hóa (`EvolutionSystem.ts`)

*   **Kích hoạt:** Lắng nghe sự kiện toàn cục `hero-leveled-up` do `HeroData` phát ra.
*   **Logic:** Khi hero đạt các mốc cấp độ, hệ thống sẽ trao phần thưởng tương ứng với nghề nghiệp từ file `EvolutionData.ts`.
    *   **Cấp 30:** Nhận kỹ năng thứ 2.
    *   **Cấp 50:** Nhận Trait/Skill bị động của nghề.
    *   **Cấp 70:** Nhận kỹ năng thứ 3.
    *   **Cấp 100:** Nhận Trait/Skill tối thượng.

## 4. Hệ thống Bệnh viện (`HospitalSystem.ts`)

*   **Bị thương nhẹ:**
    *   **Điều kiện:** Sống sót sau trận đấu nhưng `currentHp < maxHp`. Được kích hoạt bởi `CombatSystem`.
    *   **Hậu quả:** Không thể tham gia hoạt động trong **5 phút**.
    *   **Phục hồi:** Tự động hồi phục sau 5 phút hoặc trả phí Vàng để hồi phục ngay lập tức.
    *   **Chi phí:** `floor(CP / 50) + 10` Vàng.

*   **Bị thương nặng:**
    *   **Điều kiện:** `currentHp <= 0` trong các chế độ chơi có rủi ro (Boss, Giải cứu). Được kích hoạt bởi `CombatSystem`.
    *   **Hậu quả:** Không thể tham gia hoạt động trong **8 giờ**.
    *   **Phục hồi:** Phải trả phí Vàng để cứu thương. Nếu không, hero sẽ **biến mất vĩnh viễn** sau 8 giờ.
    *   **Chi phí:** `floor(CP / 10) + 50` Vàng.

## 5. Hệ thống Xây dựng (`BuildingSystem.ts`)

*   **Logic:** Quản lý việc xây dựng và nâng cấp các công trình.
*   **Trạng thái:** Một công trình có thể đang `isUnderConstruction` với một `constructionEndTime`.
*   **Hoàn thành:** Hệ thống kiểm tra định kỳ, khi `constructionEndTime` đã qua, `isUnderConstruction` được đặt thành `false` và `level` tăng lên 1.

---

### Lỗ hổng & Cơ hội Phát triển

*   **Đã giải quyết:** Hệ thống tài nguyên (`InventoryManager`) đã được triển khai và chi phí đã được áp dụng.
*   **Đã giải quyết:** Hệ thống thông báo (`UINotificationManager`) đã được triển khai để phản hồi các hành động (ví dụ: không đủ tài nguyên).
*   **Cần làm:** Tạo các biểu tượng thông báo (badge) trên các nút ở màn hình chính (ví dụ: Bệnh viện có hero đã hồi phục xong).
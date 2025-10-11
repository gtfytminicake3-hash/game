# Game Design Document (GDD) - Vương Triều Di Truyền

**Phiên bản:** Dựa trên mã nguồn hiện tại.

**Tài liệu này là trung tâm điều hướng, tổng hợp tất cả các cơ chế, công thức và hệ thống đã được triển khai. Vui lòng tham khảo các file chi tiết bên dưới.**

---

## 1. TỔNG QUAN & TẦM NHÌN

### 1.1. Tóm tắt

Vương Triều Di Truyền là một game chiến thuật, nơi người chơi xây dựng một đội quân hùng mạnh không phải bằng cách chiêu mộ, mà bằng cách lai tạo có chọn lọc qua nhiều thế hệ. Trọng tâm của game nằm ở chiến lược vĩ mô: quản lý di truyền, xây dựng đội hình và quản lý tài nguyên. Chiến đấu là tự động và là bài kiểm tra cho sự chuẩn bị của người chơi.

### 1.2. Triết lý Thiết kế Cốt lõi

*   **Di Truyền Vượt Trội Cấp Độ:** Sức mạnh thực sự của một anh hùng được quyết định bởi chất lượng gen (chỉ số gốc, Tiềm năng, Trait) tại thời điểm sinh ra.
*   **Sự Chuẩn bị là Chìa khóa:** Thành công được quyết định bởi sự chuẩn bị, không phải kỹ năng điều khiển.

---

## 2. CÁC HỆ THỐNG CHI TIẾT

*   [**Hệ thống Anh hùng & Di truyền**](./GDD_01_Heroes_And_Breeding.md)
*   [**Hệ thống Tiến triển & Trạng thái**](./GDD_02_Progression_And_Status.md)
*   [**Hệ thống Chiến đấu**](./GDD_03_Combat_System.md)
*   [**Hệ thống Giao diện Người dùng (UI)**](./GDD_04_UI_Systems.md)
*   [**Hệ thống Bản đồ Thế giới**](./GDD_05_WorldMap_System.md)
*   [**Hệ thống Túi đồ & Vật phẩm**](./GDD_06_Inventory_And_Items.md)

---

## 3. TỔNG KẾT TRẠNG THÁI DỰ ÁN

Phần này tổng hợp các tính năng đã hoàn thiện và các hạng mục cần ưu tiên phát triển tiếp theo, dựa trên việc rà soát toàn bộ mã nguồn.

### 3.1. Các Hệ thống đã Hoàn thiện & Hoạt động Ổn định

*   **Kiến trúc lõi:**
    *   Game có một luồng khởi động rõ ràng (`Bootloader` -> `CoreSystems` -> `MainScene`).
    *   Các Manager chính (`GameManager`, `DataManager`, `UIManager`, `InventoryManager`) hoạt động ổn định.
    *   Hệ thống lưu/tải dữ liệu người chơi (`PlayerData`) đã hoạt động.
    *   Hệ thống sự kiện (`EventManager`) và Localization (`LocalizationSystem`) đã được tích hợp sâu rộng.

*   **Vòng lặp Gameplay chính:**
    *   **Lai tạo:** Hệ thống lai tạo (`BreedingSystem`) hoạt động, di truyền chỉ số và trait đúng theo logic.
    *   **Trưởng thành & Lên cấp:** Hero có thể trưởng thành sau một thời gian chờ, nhận nghề nghiệp, và lên cấp thông qua `GainExp`.
    *   **Tiến hóa:** `EvolutionSystem` hoạt động, lắng nghe sự kiện lên cấp và trao phần thưởng tại các mốc 30, 50, 70, 100.
    *   **Thám hiểm (World Map):** Người chơi có thể cử đội đi thám hiểm. Vòng đời `Traveling` -> `Exploring` -> `Returning` -> `Finished` hoạt động chính xác.
    *   **Chiến đấu:** Hệ thống chiến đấu (`CombatSystem`) là một cỗ máy phức tạp và hoàn chỉnh, hỗ trợ đầy đủ kỹ năng, hiệu ứng trạng thái, cooldown, và AI chọn hành động cơ bản.
    *   **Hồi phục:** `HospitalSystem` quản lý chính xác trạng thái bị thương nhẹ/nặng, bao gồm cả cơ chế chết vĩnh viễn.

*   **Giao diện Người dùng (UI):**
    *   Kiến trúc `UIManager` vững chắc, có khả năng quản lý panel theo scene và có hệ thống `GoBack`.
    *   Các màn hình chính (`Breeding`, `Hospital`, `Arena`, `WorldMap`) và các panel đa dụng (`HeroPicker`, `SquadSelection`) đều đã hoạt động và được kết nối với logic hệ thống.
    *   Các thành phần UI như `UIResourceBar`, `UINotificationManager` hoạt động đúng như thiết kế.

### 3.2. Các Hạng mục cần Cải tiến hoặc Triển khai

Đây là danh sách các tính năng còn thiếu, chưa hoàn thiện, hoặc cần được kết nối với nhau để hoàn thiện vòng lặp game.

*   **Ưu tiên Cao - Hoàn thiện Vòng lặp Vật phẩm:**
    *   **Tích hợp Hiệu ứng Vật phẩm:** Logic của `InventoryManager` đã xong, nhưng hiệu ứng của vật phẩm thì chưa. Cần code logic sử dụng vật phẩm trong các hệ thống khác:
        *   **Tăng tốc:** Thêm chức năng dùng vật phẩm tăng tốc vào `MaturationSystem`, `HospitalSystem`, `BuildingSystem`.
        *   **Thuốc Biến Dị:** Thêm UI vào `BreedingUIController` để người chơi có thể chọn và sử dụng vật phẩm này.
    *   **Giao diện Túi đồ:** Xây dựng `InventoryPanel` để người chơi có thể xem và tương tác với các vật phẩm họ sở hữu.

*   **Ưu tiên Trung bình - Hoàn thiện Nội dung Game:**
    *   **Cấu hình Dữ liệu:** Rất nhiều hệ thống đang sử dụng dữ liệu giả lập (placeholder) trong code. Cần chuyển chúng ra các file cấu hình (`GameConfig` hoặc `ScriptableObject`):
        *   Phần thưởng của `EvolutionSystem`.
        *   Chi phí và thời gian nâng cấp của `BuildingSystem`.
        *   Danh sách quái vật của `GenerateMonstersForPOI`.
        *   Kỹ năng khởi đầu khi hero trưởng thành (`MaturationSystem`).
    *   **Traits Chiến đấu Đặc biệt:** Các Trait có hiệu ứng đặc biệt trong combat (Aura, Tái Sinh, Kẻ Săn Mồi...) đã có trong dữ liệu nhưng chưa được triển khai trong `CombatSystem`.
    *   **Logic Trận đấu Boss:** Thiết kế về trận đấu Boss (gộp đội hình) trong GDD cũ không khớp với `CombatSystem` hiện tại. Cần thiết kế lại hoặc triển khai một chế độ chơi mới dựa trên hệ thống chiến đấu hiện có.

*   **Ưu tiên Thấp - Cải thiện Trải nghiệm Người dùng (UX):**
    *   Thêm chức năng kéo-thả vào `SquadSelectionPanel`.
    *   Thêm các biểu tượng thông báo (badge) trên các nút điều hướng (ví dụ: Bệnh viện có hero đã hồi phục xong).
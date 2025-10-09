# GDD - Hệ thống Giao diện Người dùng (UI)

Tài liệu này mô tả kiến trúc và các thành phần UI đã được triển khai logic.

## 1. Kiến trúc (`UIManager.ts`)

*   Sử dụng một `UIManager` trung tâm để quản lý việc hiển thị và ẩn các panel.
*   Mỗi màn hình chính là một `UIPanel` được định danh bằng `UIPanelType`.
*   `UIManager` chứa một danh sách các Prefab/Node panel và hiển thị chúng dựa trên `panelType`.

## 2. Các Thành phần UI đã có Logic

*   **`UIMainController` (Màn hình chính/Doanh trại):**
    *   Hiển thị danh sách tất cả các hero bằng cách `instantiate` các `HeroCard_Prefab`.
    *   Lắng nghe sự kiện `hero-card-clicked` để mở `HeroInfoPanel`.
    *   Lắng nghe sự kiện toàn cục `hero-list-changed` để tự động làm mới danh sách khi có hero mới (ví dụ: sau khi lai tạo).
    *   Cung cấp các hàm điều hướng (`onBreedingClicked`, `onHospitalClicked`, `onArenaClicked`) để mở các panel khác.

*   **`HeroInfoPanel`:**
    *   Hiển thị đầy đủ thông tin của một hero được truyền vào, bao gồm chỉ số, giới tính, nghề nghiệp, traits và skills.
    *   Sử dụng `InfoItem_Prefab` để hiển thị các dòng Trait/Skill.

*   **`BreedingUIController`:**
    *   Sử dụng `HeroPickerPanel` để cho phép người chơi chọn Bố/Mẹ từ một danh sách trực quan.
    *   Danh sách chọn Mẹ sẽ tự động lọc các hero Nữ sau khi Bố đã được chọn.
    *   Gọi `BreedingSystem.breed()` và hiển thị kết quả.

*   **`HospitalPanel`:**
    *   Hiển thị 2 danh sách riêng biệt cho hero bị thương nặng và nhẹ.
    *   Sử dụng `InjuredHeroCard_Prefab` để hiển thị từng hero.
    *   Cho phép gọi hàm chữa trị trong `HospitalSystem` khi nhấn nút.

*   **`ArenaPanel`:**
    *   Tích hợp `SquadSelectionPanel`.
    *   Khi nhấn "Thách đấu", nó sẽ mở `SquadSelectionPanel` để người chơi chọn đội hình 5 người.
    *   Sau khi xác nhận, nó gọi `ArenaMode.startArenaMatch()` với đội hình đã chọn.

*   **`SquadSelectionPanel`:**
    *   Panel đa dụng để chọn đội hình với số lượng ô trống tùy chỉnh.
    *   Hỗ trợ cơ chế "nhấn để thêm" hero vào ô trống và "nhấn để loại bỏ".
    *   Tự động tính toán và hiển thị tổng CP của đội hình.

*   **`HeroPickerPanel`:**
    *   Panel đa dụng để chọn một hero từ danh sách.
    *   Tự động sắp xếp danh sách hero theo CP giảm dần.

*   **`UIResourceBar`:**
    *   Thanh hiển thị tài nguyên (Vàng, Gỗ, Đá) của người chơi.
    *   Lắng nghe sự kiện `resources-changed` để tự động cập nhật.

*   **`UINotificationManager`:**
    *   Hệ thống hiển thị các thông báo ngắn (toast) cho người chơi, ví dụ "Không đủ tài nguyên!".

---

### Lỗ hổng & Cơ hội Phát triển

*   **Chế độ chơi PvE:** Các màn hình cho `Dungeon` và `Rescue` vẫn còn trống, cần được tích hợp `SquadSelectionPanel` tương tự như `ArenaPanel`.
*   **Túi đồ:** Cần xây dựng `InventoryPanel` để người chơi xem và sử dụng các vật phẩm tiêu thụ.
*   **UX:** Cần thêm chức năng kéo-thả vào `SquadSelectionPanel` để có trải nghiệm tốt hơn.
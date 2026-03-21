# Vương Triều Di Truyền (Legacy of Blood)

![Game Screenshot](https://via.placeholder.com/800x450.png?text=Legacy+of+Blood+Gameplay)

**Vương Triều Di Truyền** là một dự án game chiến thuật trên di động được phát triển bằng Unity, nơi người chơi xây dựng một đội quân hùng mạnh không phải bằng cách chiêu mộ, mà bằng cách **lai tạo có chọn lọc** qua nhiều thế hệ.

Trọng tâm của game nằm ở chiến lược vĩ mô: quản lý di truyền, xây dựng đội hình và quản lý tài nguyên. Chiến đấu là tự động và là bài kiểm tra cho sự chuẩn bị của người chơi.

## 🚀 Triết lý Thiết kế

*   **Di Truyền Vượt Trội Cấp Độ:** Sức mạnh thực sự của một anh hùng được quyết định bởi chất lượng gen (chỉ số gốc, Tiềm năng, Trait) tại thời điểm sinh ra.
*   **Sự Chuẩn bị là Chìa khóa:** Thành công được quyết định bởi sự chuẩn bị chiến lược, không phải kỹ năng điều khiển trong trận đấu.

## ✨ Các Tính Năng Chính

*   **Hệ thống Di truyền (Breeding System):**
    *   Lai tạo hai anh hùng cha mẹ để tạo ra thế hệ con.
    *   Chỉ số, đặc tính (Traits) được di truyền theo các công thức phức tạp, bao gồm cả tỷ lệ đột biến.
*   **Hệ thống Tiến triển (Progression System):**
    *   Anh hùng lên cấp, tăng chỉ số dựa trên "Tiềm năng" (Potential).
    *   Tự động "Thức tỉnh" nghề nghiệp và học kỹ năng mới khi trưởng thành.
    *   "Tiến hóa" tại các mốc cấp độ 30, 50, 70, 100 để nhận thêm kỹ năng và đặc tính của nghề.
*   **Quản lý Anh hùng (Hero Management):**
    *   Chăm sóc các anh hùng bị thương (nhẹ và nặng) tại Bệnh viện (Hospital).
    *   Quản lý các trạng thái bận (đang trưởng thành, bị thương, đang thám hiểm).
*   **Bản đồ Thế giới & Thám hiểm (World Map & Expeditions):**
    *   Cử các đội quân đi làm nhiệm vụ tại các địa điểm (POI) trên bản đồ thế giới.
    *   Giao diện bản đồ tương tác với chức năng kéo (pan) và phóng to/thu nhỏ (zoom).
*   **Giao diện Người dùng (UI):**
    *   Hệ thống `UIManager` linh hoạt quản lý việc hiển thị các panel.
    *   Hỗ trợ panel tái sử dụng như `HeroPickerPanel` để chọn anh hùng cho nhiều tính năng.

## 🛠️ Hướng dẫn Cài đặt & Chạy

1.  Clone repository này về máy của bạn.
2.  Mở project bằng **Unity Hub**.
3.  Mở scene `Assets/Scenes/StartupScene.unity`.
4.  Nhấn **Play** trong Unity Editor. Scene khởi động sẽ tự động tải các hệ thống quản lý và chuyển sang `VillageScene`.

## 📂 Cấu trúc Thư mục

*   `Assets/Scripts/`: Chứa toàn bộ mã nguồn C# của game.
    *   `DataModels/`: Các lớp định nghĩa cấu trúc dữ liệu (ví dụ: `HeroData`).
    *   `Managers/`: Các hệ thống quản lý chính (Singleton) như `GameManager`, `UIManager`.
    *   `GameSystems/`: Các hệ thống logic gameplay như `BreedingSystem`, `EvolutionSystem`.
    *   `UI/`: Các script điều khiển panel và component giao diện.
*   `GDD/`: Chứa các tài liệu thiết kế game (Game Design Documents) chi tiết cho từng hệ thống.
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
*   [**Hệ thống Túi đồ & Vật phẩm**](./GDD_06_Inventory_And_Items.md)
*   [**Phụ lục Dữ liệu (Traits, Skills, Bosses, etc.)**](./GDD_05_Data_Appendices.md)

---

## 3. CÁC LỖ HỔNG & CƠ HỘI PHÁT TRIỂN TIẾP THEO

*(Xem chi tiết ở cuối mỗi tài liệu hệ thống con)*

**Cập nhật lớn gần đây:**
*   Đã triển khai **Hệ thống Tài nguyên** (`InventoryManager`) và tích hợp chi phí vào Bệnh viện, Xây dựng.
*   Đã triển khai **Hệ thống Thông báo** (`UINotificationManager`) để cung cấp phản hồi cho người chơi.
*   Đã cải thiện **Giao diện Lai tạo** bằng cách sử dụng một panel chọn hero chuyên dụng (`HeroPickerPanel`).
*   Đã thêm hiển thị **Combat Power (CP)** trên các thẻ hero và panel thông tin.
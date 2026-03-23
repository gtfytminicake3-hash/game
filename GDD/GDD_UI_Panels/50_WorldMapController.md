# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `WorldMapController.cs`
**Loại thành phần:** Root Game Scene / Màn Hình Bản Đồ Thế Giới (Kéo Thả 2D).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`WorldMapController` là siêu hệ thống quản lý Bản Đồ Thế Giới. Khác với những cái Panel bình tĩnh nằm im lìm, Màn hình Bản đồ có kích thước KHỔNG LỒ (2160x3840 pixels), cho phép người chơi dùng ngón tay Vuốt để trượt qua lại (Pan) và Véo để phóng to/thu nhỏ (Zoom). Trên bản đồ này sẽ rải rác hàng chục các Điểm Nhấn (POI - Point of Interest) và Xe ngựa chạy lon ton.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Nhiệm vụ của Designer ở tệp này cực kỳ khổng lồ, chia làm 3 mảng lớn:

### 2.1 Bản Đồ Nền (Map Canvas):
*   Thiết kế một tấm Cuộn Da Dê hoành tráng tột bậc hoặc một Tấm Bản Đồ Địa Hình thực tế với Kích thước tối thiểu `2160 x 3840` pixels (Có thể Seamless Tile để tiết kiệm RAM).
*   *Lưu ý Code Tham Chiếu:* Làng Mạc (Doanh Trại) của người chơi luôn bị Code ép nằm gọn ngay vị trí Tọa độ (0,0) (Chính Nghĩa / Trung Tâm của bản đồ). Địa hình càng xa tâm, Quái càng mạnh (Hệ thống rải POI theo hình vòng tròn đồng tâm). Nên vẽ Background vùng rìa bản đồ tối tăm, u ám (Núi lửa, Đầm lầy độc) so với vùng trung tâm nắng ấm.
*   **Nút Trở Về Làng (`Button backToVillageButton`):** Gắn cứng lơ lửng trên Bản đồ để bấm 1 phát thì Camera (Thực ra là `mapContainer`) trượt vèo về Tọa độ (0,0).

### 2.2 Các Trạm Dừng Chân (POI Prefabs):
Không cần vẽ Panel, mà vẽ cái CỤC ICON trên bản đồ. Mỗi cụm gộp chung 1 Box Collider làm Nút bấm.
*   **Hầm Ngục (`dungeonPoiPrefab`):** Icon Hang Động, Đầu Lâu. Kích thước khoảng 150x150.
*   **Giải Cứu (`rescuePoiPrefab`):** Icon Rạp Lều, Cũi Sắt, Lá Cờ.
*   **Tháp Thử Thách (`towerPoiPrefab`):** Icon Tòa Tháp. *Rất quan trọng:* Code có logic tự động đè Icon Sprite vào Tháp tùy theo Hệ phái yêu cầu. Designer BẮT BUỘC phải vẽ 4 tấm ảnh Tháp riêng biệt và đặt tên y xì đúc như thế này ở thư mục `Resources/UI/Towers`:
    *   `thap_mage` (Tháp cho Pháp Sư)
    *   `thap_healer` (Tháp cho Tu Sĩ)
    *   `thap_acher` (Tháp cho Cung Thủ)
    *   `thap_warior` (Tháp cho Chiến Binh)
*   **Icon Boss:** Code hiện tại chưa Export `bossPoiPrefab` mà đang "Xài Tạm" (Fallback) Hình của Tháp hoặc Hầm Ngục. Khuyến nghị vẽ Nặng Đô một cái Icon Rồng Trắng / Ác Quỷ để sẵn cho tương lai.

### 2.3 Xe Ngựa Viễn Chinh (`travelCartPrefab`):
Khi cử đội đi đánh, một chiếc Xe Ngựa sẽ trồi lên từ Tâm (0,0) và bò lết từ từ tới mục tiêu.
*   Graphic: Hình 1 cỗ Xe tải hành lý hoặc 1 Chibi Kỵ Sĩ cưỡi bò. 
*   **Quy Chuẩn Hướng Mặt:** Nét vẽ CHUẨN (Mặc định khi Import vào Unity) là Xe Ngựa Đang Ngó Mặt Sang TRÁI. Code sử dụng `Vector3(isMovingRight ? -1f : 1f)` để lật (Flip) trục X. Nếu vẽ sai góc, xe sẽ chạy kiểu lùi Moonwalk.
*   *Lưu ý Component:* Code tự động cài Cắm (Component) `Outline` viền ảnh chớp nháy màu Vàng bằng DOTween. Vui lòng cắt viền ảnh xe ngựa ở chế độ trong suốt (Alpha = 0) gọn gàng, đừng để lem nhem rác, kẻo Outline nhấp nháy ra cục gạch thì rất xấu.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   **Tương Tác Bấm Chạm:** UI/UX cần khéo léo. Trên điện thoại, ngón tay vừa có thể Vuốt lên Map để Di Chuyển, vừa có thể Nhấp Tĩnh vào Icon POI để vào trận. Việc khoanh vùng Box Collider cho Icon POI phải rất dứt khoát, không to quá làm người dùng lỡ tay ấn dính lúc đang muốn Kéo bản đồ.
*   Code rải Map tự do của Dev rất trâu bò (Mò mẫm 1000 lượt cố gắng không để Cục này dính Cục kia). Tuy nhiên vẫn có ngoại lệ Map tự Sinh Đè lên nhau nếu hết đất. Không cần cầu toàn 100%.

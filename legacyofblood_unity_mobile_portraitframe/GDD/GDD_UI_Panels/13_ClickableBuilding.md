# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `ClickableBuilding.cs`
**Loại thành phần:** Tương tác Môi trường / Nút Khối (Object Collider/Image).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`ClickableBuilding` là một script gắn thẳng lên các toà nhà (Ảnh 2D) dán trên cảnh nền của MainScreen/Làng. Mục tiêu của nó là biến một cái nhà vô tri (Ví dụ: Trại lính Barracks) thành một Nút Bấm Khổng Lồ chứa cực kỳ nhiều chức năng ẩn: Click nhẹ thì vào sảnh chờ giao diện nhà, Nhấn Giữ (1 giây) thì gọi Thợ xây tới đập búa nâng cấp, và nhà nào đang bị phong toả nâng cấp thì nó vác hẳn một cái đồng hồ đếm ngược che lên nóc nhà.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Cần có sự tinh tế khi cắt ghép hình ảnh các toà nhà:

### 2.1 Cắt Hình Dáng (Hitbox Của Căn Nhà):
*   Toà nhà xuất ra file `.png` bắt buộc phải là loại ảnh tách nền trong suốt tươm tất.
*   Trọng yếu: Trong Unit Editor, file ảnh của toà nhà phải được Tick chọn `Read/Write Enabled` trong tag Advanced (Của Texture Importer settings) và bật cờ `Mesh Type = Full Rect`. Script này có dòng code `_image.alphaHitTestMinimumThreshold = 0.1f;` giúp người chơi CLICK TRÚNG MÁI NHÀ thì nhận, còn click vào phần trong suốt (Rìa ảnh) thì click xuyên qua không bị vướng tay. Kỹ thuật cắt ảnh của Designer quyết định độ nhạy của nút.

### 2.2 Hiệu Ứng Bóp To Thu Nhỏ (Breathing & Pressed):
*   Khi người chơi chạm tay vào nhà (`OnPointerDown`), nhà sẽ lún xuống (`pressedScale = 0.95`, rút lại 5% kích thước). Do đó ảnh nhà cắt chừa viền biên cho kỹ để co giãn không bị cắt mất đuôi.
*   Khi thừa tiền nâng cấp cấp độ (`_canUpgrade = true`), toà nhà sẽ "thở" phập phồng (Phóng to lên `1.05f` rồi thụt xuống).

### 2.3 Khung Đồng Hồ Đếm Ngược Nóc Nhà (Tự Động Tạo Bằng Code):
*   Nếu nhà đang được nâng cấp, Script sẽ tự chèn (Instantiate) một bóng đen hình chữ nhật (100x30) đè nổi lên NÓC NHÀ (Cách mỏ neo trên cùng của ảnh 10 Pixel). Lên trong khung có chèn Font số màu trắng của đoạn text đếm ngược thời gian.
*   **Designer:** Đừng vẽ quá sát mép nóc nhà vào trần của màn hình điện thoại (Phải chừa một rãnh biên trên cùng của màn hình chính đủ nhét cái đồng hồ đen mờ này kẻo nó văng lỡ cỡ ngoài màn hình).

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Do cơ chế Nhấn Giữ (Long Press 1 giây) sẽ bật popup, game không cần vẽ thêm Menu Icon hình cái búa râu ria lượn lờ cản tấm nhìn nữa. Toà nhà chính là cái nút ẩn (Tối giản mạch UI Căn Cứ Làng). Khung nhà còn link đến thông tin công trình để check vàng, gỗ, đá.

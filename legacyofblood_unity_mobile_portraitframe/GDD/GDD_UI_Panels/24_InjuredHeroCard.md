# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `InjuredHeroCard.cs`
**Loại thành phần:** Item Prefab (Biến thể của The Bài Nhân Vật).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`InjuredHeroCard` là một tập lệnh "Ký sinh" (Nó bắt buộc Unity phải đính kèm script `HeroCard` gốc vào chung 1 Object). Thẻ bài này chỉ xuất hiện độc quyền bên trong Bệnh Xá (`HospitalPanel`). Mục đích của nó là biến thẻ Tướng thông thường thành Hồ Sơ Bệnh Án: Có báo giá tiền viện phí, có đồng hồ đếm ngược chờ tự khỏi bệnh.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Không cần phải phác thảo lại từ đầu một thẻ bài mới! Designer chỉ cần nhân bản (Duplicate) thiết kế của `HeroCard` (Đã làm ở file 18), sau đó "đập đi xây lại" phần viền dưới/viền ngoài của nó để đắp thêm các chi chét Y Tế vào:

### 2.1 Lớp Phủ Trạng Thái Bệnh Bật (Overlay):
*   Tại `HeroCard` gốc có một layer màng đen gọi là `busyIndicator` (Đã được yêu cầu vẽ ở tài liệu 18). Designer hãy đổi icon của layer đó trong trường hợp này thành Dấu Thập Đỏ, Thuốc Băng Bó, hoặc một màng sương mờ màu Nhạt/Cam. Mục đích để ám chỉ thằng cha trong ảnh đang ốm.

### 2.2 Khu Vực Thanh Toán & Thời Gian (Medical Footer):
Thêm một Layout (Ngang/Dọc) đè lên góc dưới của thẻ bài hoặc lơ lửng ngay dưới chân dung:
*   **Đồng Hồ Đếm Ngược (`TextMeshProUGUI timerText`):** Font chữ điện tử. Ghi "01:14:00" biểu thị thời gian xuất viện. Khi hết giờ, code tự động điền chữ "Sẵn Sàng" (`ready`).
*   **Giá Viện Phí (`TextMeshProUGUI costText`):** Số lượng Vàng cần xì ra để đút lót y tá cho xuất viện sớm. Code tính tay (Khoảng chục tới Vài trăm vàng tuỳ Level Tướng).
*   **Nút Chữa Nhanh (Trả Tiền) (`Button healButton`):** Bấm trừ vàng (`costText`) ngay lập tức. Cần kèm Icon Gold. 
*   **Nút Coi Ad Biển Thủ Tiền Viện Phí (`Button healAdButton`):** Bấm xem quảng cáo để chữa Free. Chỉ hiện ra đối với bệnh Ngặt Ngoè (Ca Nặng).

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   **Hành vi Click Bị Chặn:** Script này không định nghĩa hàm `button.onClick` để chặn phóng to Card như cái bảng Picker. Nó tận dụng nút có sẵn. Tuy nhiên, người thao tác chủ yếu sẽ bấm vào Khối Footer (2 nút Chữa Nhanh / Xem Ad). Designer cần chia Hitbox cho khéo léo để người chơi không lỡ tay bấm nhầm nút Vàng sang nút Coi Ad.

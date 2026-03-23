# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `UIResourceBar.cs`
**Loại thành phần:** HUD Component / Thanh Ngang Hiển Thị Tài Nguyên.

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`UIResourceBar` không phải là một Bảng Panel che khuất tầm nhìn, nó là một HUD (Heads-Up Display) neo cứng trên Đỉnh Màn Hình (Top Bar). Trách nhiệm duy nhất của nó là báo cáo Tình trạng Túi Tiền hiện tại của người chơi: Vàng, Gỗ, Đá, Kim Cương.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế một Dải Băng Ngang (Horizontal Bar) bám chặt vào trần nhà. Vì game màn hình dọc khá chật chội, thanh này cần thiết kế tối giản, thon gọn, không che mất các Button bên dưới.

### 2.1 Cấu Trúc Khối Vàng Bạc (4 Cụm Chỉ Số):
Thanh ngang này chia làm 4 hộc nhỏ bằng nhau xếp dàn ngang (Hoặc 2 hàng x 2 cột nếu thanh quá hẹp). Mỗi hộc cần có:
*   **Icon Tài Nguyên (Static Image):** Hình đồng tiền Vàng, Cục Gỗ, Tảng Đá, Viên Kim Cương lấp lánh.
*   **Text Hiển Thị Số Lượng:** 
    *   Vàng (`TextMeshProUGUI goldText`)
    *   Gỗ (`TextMeshProUGUI woodText`)
    *   Đá (`TextMeshProUGUI stoneText`)
    *   Kim Cương (`TextMeshProUGUI diamondText`)
*   *Gợi ý Layout:* Có thể vẽ phía sau mỗi Text một cái Box chứa màu nền xám mờ mờ bo tròn (Pill shape) để số tiền luôn nổi bật trên bất kỳ nền địa hình nào của Game.

### 2.2 Nút Nạp Tiền Nhanh (Tuỳ chọn mở rộng):
*   Mặc dù Code hiện tại chưa Export biến Nút Bấm (`Button addDiamondBtn`), nhưng chuẩn mực UI game Mobile là bên cạnh cục Kim Cương thường có 1 **Dấu Cộng Cửa Hàng (+)**. Designer nên vẽ sẵn Dấu Cộng này bên cạnh Kim Cương hoặc Vàng để chừa đường lui cho tương lai nhét Cửa Hàng IAP vào.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Thanh này chạy Ngầm và cực kỳ nhạy (`OnResourceChanged`). Bất cứ khi nào Player tiêu 1 đồng hay nhặt 1 viên đá mồ côi, Text sẽ nhảy số ngay lập tức.
*   Graphic Designer cần chọn Text Font dạng số (Monospace Numbers) để khi số Vàng nhảy từ 1000 lên 1001, nguyên cái thanh UI không bị giật nhấp nhô thò thụt (Do chiều rộng các số 1 và 0 khác nhau ở font thường).

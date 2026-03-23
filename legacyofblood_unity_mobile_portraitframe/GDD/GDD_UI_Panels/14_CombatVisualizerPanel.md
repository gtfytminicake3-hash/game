# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `CombatVisualizerPanel.cs`
**Loại thành phần:** Full Screen Panel / Màn Hình Diễn Hoạt Trận Đánh (Auto-Chess style).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`CombatVisualizerPanel` là Đấu Trường 2D của game. Khác với các game action, game này xử lý tính toán kết quả trận đánh cực nhanh ở đằng sau (Backend/Core), sau đó ném một quyển Nhật Ký (Event Log) cho Panel này dàn dựng lại vở kịch đấm nhau để người chơi xem giải trí. 
Panel này cần một thiết kế Sân Khấu rộng rãi, đủ chỗ cho 2 đạo quân xông vào nhau.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Màn hình này bận rộn nhất game, chia làm 3 mảng thiết kế cốt lõi:

### 2.1 Sân Khấu Bàn Cờ (Bản Đồ Chiến Đấu):
Đây là vùng trung tâm màn hình, có thể vẽ Background nền đất/đấu trường tuỳ hoàn cảnh. Yêu cầu tạo 2 GridLayoutGroup (hoặc Transform Neo) đối xứng nhau:
*   **Vùng Quân Ta (`Transform allyContainer`):** Nửa bên trái hoặc nửa dưới màn hình. Chia làm 9 ô (Lưới 3x3) để code nhét `BattleUnitUI` vào.
*   **Vùng Quân Địch (`Transform enemyContainer`):** Nửa bên phải hoặc nửa trên màn hình. Cũng đối xứng 9 ô lưới.
*   *Lưu ý khoảng cách:* Giữa 2 vùng phải có khoảng trống (No man's land) để các đơn vị có diễn hoạt lao lên tấn công.

### 2.2 Bộ Nút Điều Khiển Thời Gian (Góc Màn Hình):
*   **Nút Tua Nhanh (`Button x2SpeedButton`):** Dạng nút kẹp Text (`TextMeshProUGUI _speedText`). Bấm vào nó xoay vòng chữ "x1" -> "x2" -> "x4". Designer vứt nó ở góc trên cùng bên phải.
*   **Nút Bỏ Qua (`Button skipButton`):** Bấm phát bỏ qua diễn hoạt, hiện luôn kết quả cục diện. Có chữ `skipButtonText` ("Bỏ Qua / Skip").

### 2.3 Màn Hình Kết Quả Trận Đánh (PopUp):
Game cần 2 cái Bảng thông báo bự chà bá, mặc định ẨN (Inactive). Chỉ lúc nào diễn viên đánh xong mới rớt xuống giữa màn hình:
*   **Bảng Thắng Trận (`GameObject victoryScreen`):** 
    *   Màu chủ đạo: Vàng cường điệu/Xanh la quang vinh.
    *   Tiêu đề (`victoryTitleText`): Chữ "Chiến Thắng".
    *   Gắn cái nút `closeVictoryButton` (Ký hiệu "Hoàn Tất" hoặc X) để bấm thoát ra ngoài.
*   **Bảng Thua Trận (`GameObject defeatScreen`):**
    *   Màu chủ đạo: Xám tro/Đỏ bầm u ám.
    *   Tiêu đề (`defeatTitleText`): Chữ "Thất Bại".
    *   Gắn cái nút `closeDefeatButton`.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Panel này gọi và nhét `BattleUnitUI` (đã thiết kế ở file 06) vào các lưới định sẵn. 
*   **Logic Tua Nhanh (x2/x4):** Khi bấm nút Tua, các biến delay logic đánh nhau sẽ bị chia nhỏ đi.
*   **Logic Bỏ Qua (Skip):** Game không rảnh đi tính toán lại. Nó lấy luôn Mảng Data Tử Trận (Casualties) từ kết quả biết trước ra, thằng nào có tên trong sổ tử thì bị chém máu về 0 (Hiệu ứng xám xịt/Mất tích) rồi bật luôn Bảng Chiến Thắng/Thất bại lên mặt. Mọi thứ dọn dẹp rất nhanh.

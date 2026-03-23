# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `BattleUnitUI.cs`
**Loại thành phần:** Khối Prefab (UI Element) / Đại diện cho 1 Nhân vật trong trận Combat.

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`BattleUnitUI` là một thẻ con cờ đại diện cho nhân vật (Ta và Địch) xuất hiện trên khung hình của màn hình chiến đấu (Combat Visualizer Panel). Nó làm nhiệm vụ sinh động hóa trận đánh bằng cách thể hiện Avatar, Thanh Máu (HP Bar), hiệu ứng trúng đòn (Giật đỏ màn hình), lướt tới tấn công, và đặc biệt là nảy số sát thương (Floating Text).

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế Prefab thẻ tướng chiến đấu này cần làm theo phong cách Minimalist (rút gọn) để nhét vừa 10 unit (5 vs 5) trên màn hình mà không bị rối. Nhất thiết phải có 3 bộ phận:

### 2.1 Cấu Trúc Khối Chân Dung (Avatar Body):
*   **Avatar Image (`Image avatarImage`):** Hình vuông hoặc tròn. Bắt buộc để trắng (White) không tint màu ở trạng thái gốc. Code sẽ tự chuyển nó sang ảnh xám xịt (`Color.gray`) khi Unit này chết (HP = 0) hoặc nháy đỏ (`Color.red`) trong 0.1 giây mỗi khi trúng đòn.

### 2.2 Thanh Trạng Thái (Health Bar):
*   Nằm ngay dưới hoặc ngay trên đỉnh đầu Avatar.
*   **Thanh Máu (`Slider hpSlider`):** Cần một khung viền (Background) và ruột (Fill) màu đỏ hoặc xanh lá.
*   **Chữ Text Máu (`TextMeshProUGUI hpText`):** Đè lên Slider hoặc nằm cạnh. Định dạng: "Hiện Tại / Tối Đa" (Vd: "150/200"). Chữ cần bé và dễ đọc.

### 2.3 Layer Nảy Số Sát Thương (Floating Damage Text):
*   Đây là một `CanvasGroup` đi liền chứa 1 cái Text (`dmgText`).
*   Khung này nằm vô hình (Alpha = 0) ở trên đỉnh đầu Avatar.
*   **Logic Tương Tác:** Khi nhận sát thương, Text sẽ hiện rõ -> Nảy vọt lên trên 50 pixel -> Mờ dần biến hình (Fade Out Alpha -> 0) trong nửa giây.
*   **Size Chữ & Màu Sắc:** 
    *   Sát thương bình thường: Text Đỏ (`Color.red`), Size 30.
    *   Bạo kích (Critical): Text Vàng (`Color.yellow`), Size 40, thêm hậu tố " C.HIT!".
    *   Hồi máu (Heal): Text Xanh Lá (`Color.green`), Size 30, dấu cộng.
    *   Designer cần set Font TextMeshPro có nét viền (Outline) mỏng hoặc Đổ bóng (Drop Shadow) đen để chống lóa trên mọi phông nền bối cảnh.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Để hoạt ảnh đâm lướt (`PlayAttackAnim`) hoạt động đúng, Prefab này không được sử dụng Component `LayoutElement` nào khóa cứng vị trí tuyệt đối. Script sẽ trực tiếp thay đổi Transform (X,Y) kéo ảnh giật tới trước (cách mục tiêu 0.5 đơn vị) và giật lùi về trong 0.35s.

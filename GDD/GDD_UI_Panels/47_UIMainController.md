# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `UIMainController.cs`
**Loại thành phần:** Root Game Object / Controller Trôi Nổi Trên Màn Hình Chờ (Doanh Trại).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
Tuy mang chữ "UI", `UIMainController` không đại diện cho riêng biệt một Bảng Panel nào cả. Nó là kẻ "Gác Cổng" bám trên HUD (Heads-Up Display) chung của toàn khu vực Làng Mạc / Doanh Trại. Nhiệm vụ chính của nó là theo dõi thời gian và ném ra các "Mồi Nhử" thả thính người chơi bấm vào.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Xương sống của File này là điều khiển các Floating Icon (Biểu tượng trôi nổi) gắn mép màn hình Làng.

### 2.1 Cụm Nút Quảng Cáo HUD:
Cần thiết kế 2 Nút bấm (Nút loại nhỏ, Bo Tròn, mang tính chất Icon gọi mời hơn là Text dài dòng):
*   **Icon Gọi Vé Tân Binh (`Button watchAdButton`):** Biểu tượng một Cuộn Giấy / Vé cuộn tròn lấp lánh có dán chữ "Ads" hoặc Icon Dấu Play góc nhỏ. Nút này được ghim chết góc Màn hình.
*   **Icon Rương Bí Ẩn Đột Xuất (`Button mysticChestAdButton`):** Biểu tượng một cái Rương Gỗ đang nẩy tưng tưng hoặc vầng sương mù bí ẩn có Logo Play bên trên. Nút này chỉ thình lình xuất hiện, không hề cố định.

### 2.2 Các Liên Kết Cứng (Popups):
File này chứa sợi dây chuyền kéo thẳng vào `ProfessionSelectionPanel` (Đã phân tích ở tệp 34). Designer không cần thiết kế gì thêm ở mục này, chỉ cần đảm bảo Prefab Chọn Nghề được load thành công vào Scene Mẹ.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   **Hiệu ứng Bất Ngờ (Surprise Effect):** Nút Mở Rương Bí Ẩn (`mysticChestAdButton`) hoạt động dựa trên bộ đếm giờ (Timer) cực gắt: **3600 giây (1 tiếng) ló mặt một lần** (Và tối đa 3 lần/ngày).
    *   Do đó, Graphic Designer KHÔNG THỂ quăng cho Coder 1 cái hình tĩnh. Yêu cầu tạo một Animation rung lắc nhẹ (Jiggle) hoặc lấp lánh đính kèm cái Nút Rương Bí Ẩn này. Mục đích là để khi nó Vô Ưu vô thức Mọc Lên giữa màn hình, nó sẽ kéo tròng mắt của Coder nhìn vào nó và ấn kiếm Vàng/Sách kinh nghiệm ngay tắp lự.

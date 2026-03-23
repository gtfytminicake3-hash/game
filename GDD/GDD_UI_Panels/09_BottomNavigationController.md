# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `BottomNavigationController.cs`
**Loại thành phần:** Khối Giao Diện Thường Trực / Thanh Điều Hướng Dưới Cùng (Bottom Navigation Bar).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`BottomNavigationController` không đại diện cho một bảng (Panel) có thể đóng/mở đơn thuần, mà nó chạy ngầm quản lý cụm phím điều hướng (Navigation Bar) luôn luôn hiển thị ở mép dưới màn hình (hoặc mép ngang nếu là giao diện PC/Landscape). Chức năng chính là giúp người chơi nhảy vọt qua lại giữa các màn hình, và kích hoạt các **chấm đỏ thông báo (Red Badges)** để gây sự chú ý.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế một thanh ngang (hoặc dock) chứa ít nhất 8-9 nút bấm liền kề. Không gian phải đủ phân bổ.

### 2.1 Mảng Nút Bấm Điều Hướng (Navigation Buttons):
Đội design cần vẽ một bộ Icon nhất quán (Cùng style) cho các chức năng sau:
*   Màn hình chính (Căn cứ Làng).
*   Doanh Trại (Barrack).
*   Đấu Trường (Arena).
*   Thế giới / Ra Khơi (World Map).
*   Túi Đồ (Inventory).
*   Menu Phụ (Cài Đặt).
*   Hòm Thư (Mailbox).

### 2.2 Các Nút Có Đi Kèm Chấm Đỏ (Notification Badges) - QUAN TRỌNG:
Có 2 nút đặc biệt trên thanh Navigation phải được thiết kế dạng khối kép (Nút bấm + Object Chấm đỏ/Lấp lánh đè lên góc phải trên của nút). Object chấm đỏ phải tách rời để code có thể Bật/Tắt (Set_Active).
*   **Icon Bệnh Xá (Hospital):** 
    *   *Yêu cầu:* Kèm một chấm đỏ `hospitalBadge`. 
    *   *Logic code:* Thanh Nav sẽ quét tịnh tiến 60 fps/s. Nếu quét thấy kho máu của đội hình có biến (Dù chỉ 1 tướng rớt xuống trạng thái Nhẹ hoặc Nặng), chấm đỏ này sẽ rực sáng để hối thúc player vào bệnh viện.
*   **Icon Phòng Ấp / Nhân Giống (Breeding):** 
    *   *Yêu cầu:* Kèm một chấm đỏ `breedingBadge`.
    *   *Logic code:* Quét liên tục dữ liệu tướng trẻ em. Một khi thanh tiến trình trưởng thành chạm mốc đếm ngược = 0, chấm đỏ sáng lên, réo gọi người chơi vào ấn "Nhận/Thành Đinh".

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   **Không code hàm OnClick trong script:** Script này cung cấp sẵn danh sách hàng loạt hàm `OpenXXXPanel()`. Designer/Dev lúc làm Prefab chỉ việc kéo Script vào thanh Nav, rồi gán UnityEvent OnClick() thủ công trên Inspector trỏ tới hàm tương ứng.
*   **Layer Hiển thị:** Thanh Navigation này phải đè trên mọi UIPanel nền tảng (Z-index/Sorting Order cao hơn), nhưng phải nằm dưới mọi loại Popup/Hộp thoại cảnh báo toàn màn hình che mất nó.

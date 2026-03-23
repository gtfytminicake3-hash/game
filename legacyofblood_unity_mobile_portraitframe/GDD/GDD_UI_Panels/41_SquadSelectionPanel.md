# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `SquadSelectionPanel.cs`
**Loại thành phần:** Full Screen Panel / Màn Hình Lên Đội Hình.

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`SquadSelectionPanel` là màn hình sống còn xuất hiện trước mỗi trận đánh lớn (Khám phá Bản đồ, Leo Tháp...). Có nhiệm vụ điều động những tinh anh trong Làng gộp thành 1 tổ đội (Squad) từ 1 đến N người.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế Layout kinh điển của game màn hình dọc: Nửa trên là Trực thăng đón rước (Đội Hình). Nửa dưới là Trại tị nạn (Danh sách Tướng có sẵn).

### 2.1 Mảng Header & Footer:
*   **Tiêu Điểm (`TextMeshProUGUI titleText`):** Đỉnh màn hình. Vd: "Tuyển Lựa Tổ Đội Viễn Chinh".
*   **Nút Thoát (`Button closeButton`):** Bấm X hoặc Mũi Tên Trở Về.
*   **Tổng Lực Chiến (`TextMeshProUGUI totalCpText`):** Đặt ở vế dưới (Phía trên nút Xác Nhận). Vd: "Tổng Lực Chiến Đội Hình: 99.999".
*   **Nút Bấm Xác Nhận Xuất Quân (`Button confirmButton`):** Nút to, hoành tráng nhất màn hình, nằm dưới cùng. *Lưu Ý: Nút này sẽ bị xám mờ (`interactable = false`) cho đến khi người chơi chọn ĐỦ số lượng tướng theo yêu cầu trận đánh (Ví dụ ô trên có 3 lổ trống, phải nhét đủ 3 người nút mới sáng lên xanh).*

### 2.2 Sân Khấu Ô Đội Hình (Nửa Trên Màn Hình):
*   **Bệ Đứng Đội Hình (`Transform squadSlotsContainer`):** Một vùng ngang (Horizontal Layout) kéo vắt ngang giữa màn. 
*   **Tạo Hình Các Ô (Prefab `squadSlotPrefab`):** Code sẽ tự động nhét những cái Bệ Đứng (Sẽ được viết chi tiết ở File 42 `SquadSlotCard.cs`) vào khu vực này. Graphic Designer chỉ cần vẽ 1 cái bệ hoặc 1 cái khung tròn làm Nền Tảng (Background) cho Bệ.

### 2.3 Doanh Trại Tướng Chờ Nhập Ngũ (Nửa Dưới Màn Hình):
*   **Hồ Bơi Danh Sách (`Transform availableListContainer`):** Kéo 1 cái Scroll View chiếm 40% diện tích bên dưới. Bên trong Content gắn Grid Layout Group kéo các Thẻ Tướng đứng thành 3 cột hoặc 4 cột.
*   **Thẻ Tướng (`GameObject heroCardPrefab`):** Nhúng bộ Graphic xào lại từ tập lệnh `SquadSelectionHeroCard` (Tài liệu 40) vào đây. Thẻ bài có cái ngàm chữ "Chọn" ở dưới đít.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Panel này yêu cầu Drag & Drop (Kéo Thả) logic nâng cao. Người chơi có thể kéo Tướng từ Ô số 1 thả sang Ô số 3 trên Đội hình để Swap (Đổi vị trí) cho ra chiến thuật đẹp. UI Builder cần cài đặt Raycast Target chuẩn để không bị thọt sự kiện vuốt chạm.
*   Nhân sự xuất hiện ở dưới Hồ Bơi là danh sách bị LẶC CỰC KỲ KHẮT KHE:
    *   Trẻ em (Dưới 18) không có cửa lên mặt báo.
    *   Tướng Không đúng Yêu Cầu (Ví dụ Ụ tháp chỉ cho Phép thuật mà dám vác Chiến Binh) cũng bị giấu đi.
    *   Kẻ đã trèo lên Bệ Đứng ở trên thì sẽ Tàng Hình khỏi Hồ Bơi ở dưới.

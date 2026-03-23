# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `MailboxPanel.cs`
**Loại thành phần:** Full Screen Panel / Màn Hình Hòm Thư - Chiến Báo.

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`MailboxPanel` là hòm thư cá nhân của người chơi, nơi đổ về mọi kết quả phiễu lưu, viễn chinh, hoặc quà tặng hệ thống. Khác với hòm thư đọc báo tĩnh lặng, đây là nơi đầy mùi Cắn Xé vì nó chứa hàng tá thông số Thắng/Thua và chiến lợi phẩm chờ nhận từ các đạo quân đi farm offline.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế một màn hình quản lý Danh Sách (List) rất đơn giản, tương tự Bệnh viện hoặc Danh sách Tướng. 

### 2.1 Khu Vực Định Hướng (Header ngang/dọc tuỳ ý):
*   **Tiêu Điểm (`TextMeshProUGUI panelTitleText`):** Đặt ở vị trí trung tâm hoặc góc trái. (Vd: "Thư Viện Chiến Báo").
*   **Nút Đóng (`Button closeButton`):** Chỉ cần nút X.
*   **Nút Vơ Vét (`Button claimAllButton`):** Yêu cầu vẽ một Nút bự, lấp lánh (Vàng/Cam) ghi là "Nhận Tất Cả". Cực kì quan trọng vì người chơi lười sẽ bấm nút này thay vì dò từng thư. Nó có biến text con là `claimAllButtonText`.

### 2.2 Phân Khu Danh Sách (Body):
*   **Thùng Chứa Bản Tin (`Transform reportItemsContainer`):** Một Scroll View chiếm đến 80% diện tích màn hình. Component Content của nó nên được gắn một `VerticalLayoutGroup` để các bức thư xếp chồng lên nhau thành một cột dọc.
*   **Thiết Kế Thư Báo (`GameObject reportItemPrefab`):** Không cần vẽ mới ở đây! Nó chính là hệ thống Prefab `ExpeditionReportItem` nằm chình ình ở Tệp tài liệu số 17. Code sẽ làm nhiệm vụ Clone và nhét Tệp số 17 vào đây.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Panel này là màn hình "Nóng". Có nghĩa là người chơi chạm vào Thư -> Popup Bảng đấm nhau 2D nhảy bổ ra -> Hết đấm nhau -> Thu về Bảng thư.
*   Dev đã chặn đứng lỗi vô dụng: Khi Danh sách Không có lá thư nào rớt trong giỏ, Nút "Nhận Tất Cả" sẽ tự động chuyển sang màu Xám mờ (`interactable = false`). Nên Design cần vẽ nút này có 2 state Normal và Disabled cho đàng hoàng.

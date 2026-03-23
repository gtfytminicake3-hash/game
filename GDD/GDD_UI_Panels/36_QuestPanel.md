# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `QuestPanel.cs`
**Loại thành phần:** Full Screen Panel / Màn Hình Sổ Nhiệm Vụ.

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`QuestPanel` là quyển sổ cái ghi ghép tiến trình chơi của game thủ. Nó chứa đủ mọi loại nhiệm vụ từ Cốt truyện chính (Main) cho tới việc vặt Hằng ngày (Daily) và Hằng Tuần (Weekly).

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Màn hình này bám rất sát cấu trúc của Hòm Thư (`MailboxPanel`) nhưng thay vì nút "Nhận Tất Cả", nó sử dụng không gian đó để dàn trải các Tab Phân Loại.

### 2.1 Khu Vực Tiêu Điểm & Tab (Header):
*   **Tiêu Điểm (`TextMeshProUGUI panelTitleText`):** Đặt to ở trên cùng. Vd: "Danh Sách Nhiệm Vụ".
*   **Nút Đóng (`Button closeButton`):** Bấm X.
*   **Hệ Thống Tab Phân Loại Lưới (Tab Navigation):** Yêu cầu vẽ 3 Nút liên kết với nhau (Radio Buttons / Trượt ngang) đặt ngay dưới Tiêu điểm:
    *   **Tab Chính (`Button mainTabButton`):** Gắn biến chữ `mainTabText` ("Cốt Truyện").
    *   **Tab Hàng Ngày (`Button dailyTabButton`):** Gắn biến chữ `dailyTabText` ("Hàng Ngày").
    *   **Tab Hàng Tuần (`Button weeklyTabButton`):** Gắn biến chữ `weeklyTabText` ("Hàng Tuần").
    *   *Gợi ý Layout:* Có thể thiết kế 3 tab này nằm xen kẽ dọc bên mép Trái màn hình giống game thẻ tướng rảnh tay, hoặc xếp ngang truyền thống trên đỉnh.

### 2.2 Khu Vực Cuộn Băng Thưởng (Main Body):
*   **Danh Sách Nhiệm Vụ (`Transform questListContainer`):** Một Scroll View chiếm đến 80% diện tích màn hình. Component Content của nó nên được gắn một `VerticalLayoutGroup` để các Nhiệm vụ xếp lợp lên nhau.
*   **Thẻ Nhiệm Vụ (`GameObject questItemPrefab`):** Code sẽ tự động lôi thiết kế Băng Rôn Nhiệm Vụ đã phân tích ở `QuestItemUI` (Tệp Số 35) thả vào danh sách này.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Panel này cài đặt Lọc Hiển Thị (Filter). Nó tàn nhẫn cắt bỏ toàn bộ những nhiệm vụ nào ở trạng thái "Đã Nhận Thưởng" (`Claimed`). Do đó Danh sách sẽ luôn thụt ngắn lại sau mỗi cú click nhận quà, giữ cho màn hình luôn sạch sẽ tập trung vào mục tiêu đang chạy.
*   Do đặc thù các thẻ `QuestItemUI` (Prefab) khá dài và nhiều chữ, khoảng cách (`Spacing`) của `VerticalLayoutGroup` trong container này nên để rộng rãi thoải mái (Ví dụ: Yết hầu 10-20px) để chống rối mắt.

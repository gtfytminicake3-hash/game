# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `InventoryPanel.cs`
**Loại thành phần:** Full Screen Panel / Màn Hình Túi Đồ Chính.

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`InventoryPanel` là kho chứa tài sản khổng lồ của người chơi. Nó quản lý mọi thứ thu thập được từ thế giới game, chia làm 2 ngăn rõ rệt: Tạp Hoá (Items/Nguyên liệu) và Trang Bị (Equipments/Vũ khí, Giáp...).

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế cấu trúc Panel lớn với hệ thống Tab Navigation định hướng.

### 2.1 Khu Vực Điều Hướng (Tabs & Header):
*   **Tiêu đề:** "Hành Trang" hoặc "Túi Đồ".
*   **Nút Chuyển Tab:** Cần vẽ 2 nút kề nhau ở trên đỉnh hoặc dọc bên mép sườn.
    *   Nút 1: Vật Phẩm (`Button itemTabButton`). Gắn Text/Icon Cái rương.
    *   Nút 2: Trang Bị (`Button equipmentTabButton`). Gắn Text/Icon Thanh Kiếm & Cái Khiên.
    *   *Lưu ý Code:* Code gán cứng màu sắc: Khi đang ở tab nào thì Nút đó đổi màu Trắng (`Color.white`), Tab kia đổi màu Xám (`Color.gray`). Designer nếu muốn xài đồ hoạ đổi màu này thì dùng Sprite nguyên bản nền trắng tinh, gợn nét chìm.
*   **Nút Đóng (`Button closeButton`).**

### 2.2 Trang Trang Bị (Equipment Page - Đơn giản):
Khu vực này tốn đa số diện tích màn hình:
*   **Lưới Khay Đồ (`Transform equipmentGridParent`):** Là một Component `Scroll Rect` trượt lên/xuống. Designer nhét 1 `GridLayoutGroup` vào Content Box rỗng của nó.
*   **Cắm Prefab:** Toàn bộ Thẻ bài Trang bị (`InventoryEquipmentCard` - Ở file 25) sẽ được Code auto nhét vào cái Lưới này. Bảng này không có Popup phụ đính kèm (Vì nếu bấm thẻ, Hệ thống gọi `EquipmentDetailPanel` đè lên nguyên con game rồi).

### 2.3 Trang Tạp Hoá (Item Page - Phức Tạp Hơn Nhẹ):
Trang này bắt buộc phải chia màn hình ra làm 2 vế (Ví dụ: Trái 70% / Phải 30%).
*   **Phân Khu Lưới Tạp Hoá (Trái - `Transform itemGridParent`):** Tương tự Trang bị, đây là chỗ đổ rác. Nnhét Prefab Component tên là `ItemSlot` (Sẽ đọc ở tệp sau) vào. Chú ý tệp `ItemSlot` là loại Thẻ Vuông Rất Nhỏ, không phải thẻ chữ nhật như `InventoryItemCard`.
*   **Phân Khu Đọc Chi Tiết (Phải - `GameObject detailView`):** Một miếng gỗ hoặc miếng da nằm dán cố định ở bên phải màn hình. Chỉ hiện lên khi Click vào 1 cục rác trong Bảng Trái. Box này chứa:
    *   `detailIcon` (Ảnh phóng to).
    *   `detailNameText` (Tên).
    *   `detailCountText` (Số lượng, Vd: "Số lượng: 99").
    *   `detailDescText` (Đoạn mô tả công dụng).
    *   `useButton` (Nút xài nhanh, chỉ hiện khi vật đó ăn được).

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   **Phân loại Card:** Ở file 26 ta có `InventoryItemCard`, nhưng ở file này (27) tác giả script lại xài `ItemSlot` (file 28) thay thế cho Grid của Tab Tạp hoá. Khả năng cao File 26 chỉ là phương án sơ cua hồi xưa (hoặc dùng nhúng ở Hòm thư). Cần phải setup cái `ItemSlot` kỹ ở tài liệu kế tiếp.
*   Hãy dùng Masking cẩn thận cho 2 cái Page (GameObject) vì Code tắt mở bạo lực bằng `SetActive(true/false)`, dễ gây lỗi tràn UI nếu không neo Anchor đoàng hoàng.

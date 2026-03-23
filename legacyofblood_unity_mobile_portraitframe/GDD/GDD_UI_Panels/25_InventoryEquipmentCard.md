# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `InventoryEquipmentCard.cs`
**Loại thành phần:** Item Prefab (Thành phần List dạng thẻ trong Túi Đồ).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`InventoryEquipmentCard` là một thẻ bài nhỏ đại diện cho 1 món vũ khí/áo giáp/trăng sức nằm la liệt trong Túi Đồ (Inventory) của người chơi. Khác với `HeroEquipmentSlot` (Chỉ là 1 cái icon bé xíu dán trên người Tướng), thẻ bài này yêu cầu phải hiển thị vừa đủ các thuộc tính quan trọng để người chơi lướt qua xem nhanh được món đó cùi hay xịn liền mà không cần bấm lôi Popup ra đọc.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế một thẻ bài chữ nhật nằm ngang (Horizontal Ticket) hoặc ô vuông lớn (Large Slot) có khoảng trống để nhét văn bản. Dưới đây là cấu trúc ưu tiên cho dạng thẻ ngang (Khuyên dùng trong Scroll View Túi Đồ dọc).

### 2.1 Cụm Avata Món Đồ (Bên Trái):
*   **Vector Viền Phẩm Chất (`Image rarityBorder`):** Ô vuông viền. Phải vẽ bằng Trắng đen (Grayscale) để Unity tự nhuộm màu theo Tier D -> SSS (Trắng/Lục/Lam/Tím/Vàng/Cam/Đỏ).
*   **Ảnh Món Đồ (`Image iconImage`):** Đặt lọt lòng bên trong khung viền.
*   **Cấp Độ Chỉ Gốc (`TextMeshProUGUI levelText`):** Có thể thiết kế thành một Badge (Huy hiệu) cực nhỏ lề mép góc dưới/trái của Icon. Hiển thị "Lv.1".

### 2.2 Cụm Đọc Thông Số Nhanh (Bên Phải):
Toàn bộ phần Text bên cạnh này cần font chữ mảnh, sắc nét.
*   **Tên Trang Bị (`TextMeshProUGUI nameText`):** Cỡ chữ lớn nhất trong khối Text. (Vd: Nhẫn Ma Pháp).
*   **Dòng Chỉ Số Cốt Lõi (`TextMeshProUGUI mainStatText`):** Chỉ số chính. (Vd: "ATK: +200"). Màu text tuỳ chọn (Đỏ/Cam/Trắng).
*   **Dòng Phụ Lục (`TextMeshProUGUI subStatsText`):** Box text chứa 2 dòng chữ mô tả tính năng phụ. Code tự động chèn ký hiệu xuống dòng `\n`. Design cần chừa đủ chiều cao Line-height. Khuyến nghị màu Xanh lá (`#00FF00`) cho chỉ số xịn.

### 2.3 Khu Vực Thao Tác:
*   **Vùng Nhấn Mở Rộng (`Button clickButton`):** Toàn bộ thẻ bài này phải được bọc trong một cái Component Nút. Người chơi chỉ cần chọc ngón tay vào bất kỳ đâu trên thẻ bài này, nó sẽ ném Data về hệ thống để bung cái bảng `EquipmentDetailPanel` (Ở file 15) bự chà bá ra giữa màn hình.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   **Tối Ưu Hoá Giao Diện:** Túi đồ của người chơi thường sẽ chứa tới hàng trăm món đồ loại này. Prefab này cần tối giản số lượng game object. Gom chung Text nếu có thể (Ví dụ `mainStatText` và `subStatsText` ráng gom chung 1 component Text nếu được, nhưng ở đây Dev đang chia làm 2 biến để dễ format màu).
*   Không nên gắn bất kỳ Animation nhấp nháy/Aura nào vào Thẻ này trong Túi Đồ trừ phi món đó là Cấp SSS cực hiếm (Tính năng mở rộng sau).

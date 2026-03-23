# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `InventoryItemCard.cs`
**Loại thành phần:** Item Prefab (Thành phần List dạng thẻ trong Tab Tạp Hóa - Túi Đồ).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`InventoryItemCard` là một thẻ bài nhỏ đại diện cho các vật phẩm Chức năng / Tạp hoá trong game (Không phải là Trang Bị). Ví dụ: Sách Kinh Nghiệm, Thẻ Càn Quét, Mảnh Tướng, Thuốc Nhuộm Đột Biến... Thẻ này được sinh ra và xếp lớp vào bên trong Tab "Vật Phẩm" của màn hình Hành Trang chính.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế một thẻ bài chữ nhật nằm ngang (Horizontal Ticket) tương đương kích thước với `InventoryEquipmentCard` để giữ tính đồng bộ khi cùng lướt trong 1 UI chuẩn của túi đồ.

### 2.1 Cụm Avata Món Đồ (Bên Trái):
*   **Ảnh Món Đồ (`Image itemIcon`):** Đặt trong khung vuông hoặc tròn nhỏ mép trái thẻ. Không cần vẽ viền Rarity phức tạp như Trang bị, chỉ cần viền gỗ/kim loại trơn là được.

### 2.2 Cụm Đọc Văn Bản (Giữa):
*   **Tên Vật Phẩm (`TextMeshProUGUI itemNameText`):** Đặt to ở dòng 1. (Vd: Sách Tiềm Năng Nhỏ).
*   **Số Lượng (`TextMeshProUGUI itemCountText`):** Đặt lơ lửng ở cạnh ảnh hoặc dòng 1 tùy Layout. Yêu cầu có chữ "x" nhét vào sẵn (Vd: x 999).
*   **Mô Tả Vật Phẩm (`TextMeshProUGUI itemDescriptionText`):** Dòng nhỏ bên dưới Tên vật phẩm. Cần hỗ trợ box rớt nhiều chữ hoặc Text Tràn (Ellipsis) để cắt bớt "Đọc tiếp...". (Vd: Cung cấp 100 EXP cho Tướng).

### 2.3 Cụm Nút Bấm Xử Lý (Bên Phải Tận Cùng):
*   **Nút Sử Dụng (`Button useButton`):** Yêu cầu thiết kế rõ ràng là một Nút, không phải ẩn như thẻ Trang Bị. Kích thước vừa ngón tay. Có chữ "Sử Dụng" hoặc Icon Mũi Tên Hướng Lên / Tia Chớp. 

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   **Nút Sử Dụng Bị Chặn:** Theo logic code, nếu `type` của vật phẩm không phải loại Bỏ Vào Mồm Nhai Được (`Consumable`), ví dụ như Nguyên Liệu Nâng Cấp Tòa Nhà hay Mảnh Quái Vật, thì nút Sử Dụng này sẽ tự động mờ đi (Disabled/Uninteractable). Designer cần làm 2 trạng thái chìm nổi/sáng xám cho Nút này một cách công phu.
*   **Lướt Không Lag:** Tương tự đồ đạc, khu này chứa hàng tấc icon/mảnh rác... Tránh xa Animation!

# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `ArenaShopItem.cs`
**Loại thành phần:** Khối Prefab (UI Element tái sử dụng) / Thẻ vật phẩm trong Shop.

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`ArenaShopItem` KHÔNG phải là một bảng Panel độc lập, mà là thiết kế của một "thẻ hàng hóa" (Item Card) mọc ra nhiều nhân bản trong danh sách cuộn thuộc Cửa Hàng Đấu Trường (`ArenaShopPanel`). Định nghĩa cấu hình thẻ này để UI designer xuất prefab nhỏ.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế một thẻ hàng hóa chữ nhật dọc (hoặc ngang) vừa vặn nằm trong ScrollView. Bắt buộc chứa các yếu tố sau:

### 2.1 Các Thành Phần Giữ Chỗ (Placeholder):
*   **Icon Vật Phẩm (Item Image):** Một khung trống hình vuông ở chính giữa thẻ để code nhét hình ảnh Cúp, Rương mảng, Sách EXP vào.
*   **Tên Món Hàng (`TextMeshProUGUI itemNameText`):** Text hiển thị chữ (Ví dụ: "Mảnh Tướng Ngẫu Nhiên"). Cần chọn font hiển thị rõ ở size nhỏ.
*   **Dòng Giá Tiền (`TextMeshProUGUI priceText`):** Text hiển thị số lượng tiền yêu cầu (Ví dụ: "1000 Khuyển" hoặc "1000 Huy Chương").

### 2.2 Các Nút Bấm Tương Tác:
*   **Nút Chốt Đơn (`Button buyButton`):** Đặt ở dưới cùng thẻ. Trên nút nên khắc chữ "Mua" (Buy) hoặc Icon giỏ hàng. 

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   **Data Binding:** Code sẽ tự động gọi hàm `Setup("Tên Đồ", Giá_Số_Nguyên)` để gán thẳng chữ vào `itemNameText` và `priceText`. Không được Hardcode vĩnh viễn chữ trong Prefab. Thêm vào đó, đơn vị tiền (" Khuyển") hiện đang gắn tạm trong code, cần chừa khoảng trắng hợp lý để chữ không bị tràn khung nếu giá tiền lên tới hàng ngàn. 
*   **Sự Kiện Click Mua:** Khi click, nút Buy sẽ chạy logic trừ tiền và thêm vào Inventory (Hiện tại đang chỉ In Log rỗng). Designer có thể làm thêm state nhấp nháy/pop cho nút bớt thô cạnh.

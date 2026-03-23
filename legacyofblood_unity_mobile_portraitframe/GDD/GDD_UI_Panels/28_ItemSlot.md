# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `ItemSlot.cs`
**Loại thành phần:** Item Prefab (Ô Vuông Tạp Hoá - Nằm trong Lưới Hành Trang).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
Trái ngược với vẻ bề thế toàn chữ là chữ của `InventoryItemCard` (File 26), `ItemSlot` là một phiên bản rút gọn siêu cấp tối giản mang phong cách Minecraft/Nhập vai cổ điển. Nó chỉ là một ô vuông chứa duy nhất biểu tượng của món đồ và số lượng đang sở hữu. Khi người chơi nhấn vào nó, Bảng Chi Tiết (Chiếm nửa màn hình bên phải của `InventoryPanel`) mới hiện ra đọc cho người chơi nghe thông số.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế một Box vuông nhỏ vức (Kích thước tầm 100x100px hoặc 120x120px là đẹp).

### 2.1 Cấu Trúc Box:
*   **Vector Nền/Viền:** Vẽ một viền ô vuông. Khuyến nghị vẽ viền dạng Gỗ / Sắt / Túi Da. Mảng này làm nền tĩnh. Khu vực này không xài RarityBorder (Trắng/Xanh/Tím) như trang bị, nên cứ vẽ tĩnh 1 màu cố định là được.
*   **Trái Tim Đồ Vật (`Image iconImage`):** Lớp Hình ảnh Avatar của món đồ. Nằm gọn lọt thỏm trong lòng Viền. Cần phải scale nó bé lại 1 tí ở mép để khi người chơi bấm nút không tạo cảm giác hình bị chèn mép.

### 2.2 Đọc Thông Số:
*   **Con Số Lưu Lượng (`TextMeshProUGUI amountText`):** Đặt ở góc phải bên dưới của Box. Format hiển thị số trần: "99" (Không có chữ x phía trước để tiết kiệm diện tích). 
    *   *Lưu ý Code:* Nếu món đồ chỉ có số lượng là 1, Code sẽ xoá con số này đi để làm trống hình khối. Nếu món số lượng trên 1000, Code có thể sinh ra các tiền tố K/M. Designer nên chọn Font có Outline Đen dày để chống hòa màu với Hình Món Đồ đằng sau.

### 2.3 Phân Khu Thao Tác:
*   **Lớp Phủ Button (`Button clickButton`):** Bọc toàn bộ Box thành Nút. Mọi thao tác bấm lướt, kéo thả đều nằm trong tầm phản hồi của Nút này.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   **Mối Quan Hệ Với File 26:** Hai File 26 (`InventoryItemCard`) và 28 (`ItemSlot`) có vẻ đang được Dev làm test trên 2 kiểu hiển thị màn hình riêng rẽ (Hoặc do thừa mứa). Trong phiên bản Túi Đồ hiện tại của Code `InventoryPanel.cs` tệp số 27, Dev đang chọn xài cái `ItemSlot` chèn vào Lưới Tạp Hoá (Ngăn túi bên trái màn hình chia đôi).
*   Prefabs này sẽ bị sinh ra chục cái hàng loạt, nên Graphic không nên để Shadow/Blur đổ bóng quá nặng để giảm Draw Call.

# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `UIPanel.cs`
**Loại thành phần:** LỚP CHA TỔNG (Base Class) - Không ám chỉ một Prefab cụ thể nào.

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`UIPanel.cs` là bản thiết kế Gốc, Đạo Yết Kiêu của mọi màn hình Menu trong game. Những File như `InventoryPanel`, `SettingsPanel` đều là con cháu kế thừa từ nó. Nó chịu trách nhiệm khai báo Danh tính (Loại Panel gì) và cung cấp một chức năng chung chạ nhất: Tự động chạy Hình Nền Động.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Do đây là Class Ẩn (Abstract / Base Object), không có giao diện cụ thể nào cần vẽ RIÊNG cho file này. Tuy nhiên, nó đưa ra một LUẬT CHƠI MỚI về BACKGROUND mà mọi Designer cần tuân thủ khi thiết kế BẤT KỲ bảng Panel nào.

### 2.1 Luật Thiết Kế Khung Nền (Background Base):
*   Khi thiết kế các Bảng Full Screen (Màn hình chi tiết kích thước lớn), đừng dùng một tấm ảnh tĩnh đuôi `.png` nhàm chán.
*   Hãy thiết kế các Frame Ảnh Động (Ví dụ: Thác nước chảy đằng sau bảng Inventory, Lửa đuốc phập phùng đằng sau bảng Cài Đặt). 
*   Lớp vỏ của Bảng Panel nào cũng có quyền bật tính năng `useDynamicBackground = true`. Nếu được bật, Code sẽ tự động lôi Lớp Nghệ Thuật `SpriteSequencePlayer` (Đã phân tích ở tệp 39) ra đắp vào.
*   **Hạn chế đính liền (Embedded):** Đừng vẽ nội dung Nút bấm, Chữ nghĩa DÍNH MỘT CỤC vào chung tấm ảnh Background. Hãy tách Nền thành layer riêng biệt hoàn toàn. Coder sẽ ốp Nền vào gốc (`GetComponent<Graphic>()`) và cho nó chạy Ảnh Động cật lực. Còn Nút bấm, chữ nghĩa sẽ là lớp rải đè lên trên.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   **Chế độ Máy Yếu (Save Battery):** Không phải lúc nào Background động cũng được phát. Tại hàm `Start()`, game sẽ kiểm tra xem người chơi có đang bật `DataManager.Instance.Player.useDynamicUI` hay không (Tính năng được điều khiển ở bàng Settings). Nếu người chơi tắt đi để đỡ hao pin, ảnh nền của Panel sẽ đứng Tĩnh 1 chỗ. Do đó, Frame ảnh tĩnh số 1 đem làm Base phải đẹp nhất có thể.

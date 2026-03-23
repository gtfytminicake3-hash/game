# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `HeroPickerPanel.cs`
**Loại thành phần:** Popup Overlay / Bảng Chọn Tướng Đa Dụng (Dùng chung).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`HeroPickerPanel` là một cái List (Danh sách trượt) bật lên đè che các màn hình khác khi trò chơi cần người chơi thực hiện "Quyết Định Sự Lựa Chọn". Ví dụ nổi bật nhất là khi ở `BreedingUIController`, khi bấm "Chọn Mẹ" -> Popup này sẽ nhảy ra liệt kê toàn bộ Hero Giới tính Nữ đang rảnh rỗi ở nhà.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế một Panel trượt (Scroll View) đơn giản, giống hệt cấu trúc của BarrackPanel nhưng mang tính chất Overlay (Tạm thời):

### 2.1 Khu Vực Định Danh Dính Phía Trên (Header):
*   **Tiêu Điểm (`TextMeshProUGUI titleText`):** Tiêu đề này là Text động (Code sẽ bơm thẳng chữ "Chọn Cha" hoặc "Chọn Leader" vào tuỳ ngữ cảnh gọi hàm).
*   **Nút Đóng (`Button closeButton`):** Chỉ cần nút X ở góc phải trên. Panel này không cần nút Back. Bấm X là huỷ thao tác chọn lọc.

### 2.2 Khu Vực Hiển Thị Mạng Lưới (Scroll View / Body):
*   **Container Tướng (`Transform listContainer`):** Một Content RectTransform nằm trong Scroll Rect. Nên set nó là loại dải lưới `GridLayoutGroup` chia cột đều đặn (2 hoặc 3 cột tùy màn hình dọc).
*   **Thiết Kế Thẻ Con (`GameObject heroCardPrefab`):** Nhúng trực tiếp thiết kế `HeroCard` (File 18_HeroCard) vào đây. KHÔNG VẼ THÊM GÌ MỚI. Khung viền thẻ bài sẽ lấp đầy Grid này.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Panel này là màn hình Dùng Chung (Reusable Component). Nên mọi góc cạnh thiết kế phải mang tính chất Tối Giản/Trung Tính, không nên gắn tên hay Background đặc thù của 1 tính năng nào hết.
*   **Xếp hạng tự động:** Khi nhảy popup ra, Code có một dòng can thiệp tự động là xếp thẻ Tướng Gấu nhất (Combat Power to nhất) trồi lên trên cùng theo Layout. 
*   **Cơ chế huỷ diệt:** Ngay khi Click vào 1 Thẻ, Panel này sẽ Đóng sập tự động (Self-Destruct), nên Designer bè mảng LayoutGroup cẩn trọng để lúc Instantiate không bị bung UI 1 giây đầu tiên làm xấu hình.

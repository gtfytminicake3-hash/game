# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `PopulationHeroCard.cs`
**Loại thành phần:** Item Prefab (Thẻ Nhân Vật Dạng List Ngang).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`PopulationHeroCard` là một "Biến thể Mở Rộng" của thẻ Tướng. Nó chỉ xuất hiện trong Bảng Quản Lý Dân Số (`PopulationManagerPanel`). Do tính chất của Bảng Quản Lý là duyệt qua hàng trăm con tướng để tìm ra đứa phế vật nhất đem đi... đuổi cổ khỏi làng (Dismiss), thẻ này được ưu tiên thiết kế dưới dạng Thanh Ngang (Horizontal Bar / List Item) để tối ưu diện tích Scroll tột cùng, thay vì dạng Thẻ Bài dọc (Portrait Card) rườm rà.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
 Thiết kế một thẻ chữ nhật NGANG. Hãy tưởng tượng nó giống như một dòng trong Bảng Tra Cứu (Excel/Table).

### 2.1 Mảng Trái (Hình Ảnh Định Danh):
*   **Chân Dung (`Image avatarImage`):** Cắt thành hình Tròn nhỏ hoặc Khung Vuông nhỏ gọn đặt ở ngoài cùng bên trái.
*   **Trích Ngang:** Ngay cạnh Avatar là nguyên một cụm 3 con Text chồng xếp/kề nhau:
    *   Tên (`TextMeshProUGUI heroNameText`). Vd: Arthur Đệ Nhất.
    *   Cấp Độ (`TextMeshProUGUI levelText`). Vd: Lv.10.
    *   Nghề (`TextMeshProUGUI professionText`). Vd: Chiến Binh.

### 2.2 Mảng Phải (Chỉ Số Tối Quan Trọng & Búa Sa Thải):
*   **Lực Chiến (`TextMeshProUGUI cpText`):** Text to nằm dịch sang bên phải. Màu nhấn/đậm để người chơi so sánh nhanh thằng nào yếu sinh lý nhất làng.
*   **Nút Đuổi Cổ (`Button dismissButton`):** Đặt ở tận cùng bên Phải. Nút này mang ý nghĩa Tiêu Cực (Destructive Action). Bắt buộc phải ốp màu **Đỏ / Cam** báo động. Icon có thể là Cái Cửa, Bàn Chân Đá đít, hoặc Thùng Rác.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Thẻ này không có Button chìm phủ toàn thể. Người chơi click vào bất cứ chỗ nào trên thanh ngang này ngoại trừ Nút Đỏ thì thẻ bài cũng Câm Điếc (Không tung ra Bảng Chi Tiết Tướng). Code chỉ đoái hoài đến duy nhất `dismissButton`. Cho nên Design cần vẽ cái nút đó thật rõ ràng và cách ly để chống "Bấm nhầm khóc hận".

# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `SquadSelectionHeroCard.cs`
**Loại thành phần:** Item Prefab (Biến thể Thẻ Nhỏ dán dưới Grid Đội Hình).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`SquadSelectionHeroCard` là một Kịch Bản Ký Sinh (Tương tự `InjuredHeroCard` ở File 24). Nó đòi hỏi Unity phải ráp chung nó với bộ xương Prefab gốc `HeroCard` (File 18). Nhiệm vụ của nó là Biến Thẻ Tướng Bình Thường thành Thẻ "Tuyển Quân": Khi người chơi bấm vào thẻ này, thay vì mở Bảng Thông Số 3D dài dòng, Hệ thống sẽ Xách cổ con tướng đó nhét thẳng lên Trận Hình (Squad).

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Không cần phải phác thảo lại từ đầu một thẻ bài mới! Designer chỉ cần nhân bản (Duplicate) thiết kế của Thẻ Tướng Cơ Bản `HeroCard` (Được làm ở tài liệu 18), sau đó "chế cháo" lại vùng viền dưới để gắn Nút Chọn.

### 2.1 Thành Phần Sống Ký Sinh (Lớp Phủ Nút Bấm):
*   **HeroCard Gốc:** Giữ nguyên mọi thứ từ Avatar, Viền Rarity, Dải Băng Tên, Text Cấp độ. Đừng đổi cấu trúc gốc của chúng vì Code File 18 vẫn cần tìm đúng tên biến Text để gán dữ liệu.
*   **Khoét Không Gian Trống:** Kéo dãn khung nền Thẻ bài gốc (Background) dài xuống dưới thêm cỡ 30px-40px.
*   **Nút Chọn Tướng (`Button selectButton`):** Tại không gian mới kéo dãn ra, chèn một Nút Bấm lọt lòng vào đó.
    *   Màu sắc: Xanh Lá (Khuyến nghị) mang ý nghĩa Add/Thêm Vào.
    *   Text: "Chọn" hoặc Icon Móc Câu / Dấu Cộng.
    *   Kích thước: Phải đủ bự để ngón tay cái bấm không trượt, vì thao tác Chọn Quân diễn ra liên tục, nếu Box Collider nhỏ quá người chơi sẽ quạu.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   **Chặn Tương Tác Xung Đột:** Tệp `HeroCard` gốc file 18 có 1 trigger là khi người chơi rờ tay lên MẶT thẻ, thẻ sẽ gọi Màn Hình Chi Tiết Tướng sập xuống. Còn tệp Ký Sinh 40 này thì chỉ phản hồi khi chạm vào Nút "CHỌN" phía dưới đáy thẻ.
*   Designer cần chia Hitbox Cực Thận Trọng. Đảm bảo khu vực `selectButton` KHÔNng bị chèn lấp bởi Graphic Raycast của Tấm Hình Nền thẻ bài gốc.
*   Thẻ này sẽ được lặp lại nhét thành 1 dải ngang hoặc dọc dưới chân màn hình Bố cục Đội Hình (`SquadSelectionPanel`). Tránh Add Effect tỏa sáng phức tạp.

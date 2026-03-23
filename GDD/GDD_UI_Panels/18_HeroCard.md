# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `HeroCard.cs`
**Loại thành phần:** Item Prefab (Thẻ Bài Nhân Vật - Có thể tái sử dụng ở nhiều Panel khác nhau).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`HeroCard` là một trong những thành phần UI quan trọng nhất và xuất hiện dày đặc nhất trong game (Tại Doanh trại, Màn hình xếp đội hình, Màn hình Nhân giống...). Nó đóng vai trò là "Căn cước công dân" thu nhỏ của mỗi Tướng, cho phép người chơi lướt qua để đánh giá thực lực và click vào để xem chi tiết.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Cần vẽ một thẻ bài dạng dọc (Portrait Card), viền ngoài bo góc hoặc sắc cạnh tùy Art Style (Dark Fantasy ưu tiên sắc cạnh/Gothic).

### 2.1 Lớp Nền và Chân Dung (Base & Avatar):
*   **Ảnh Chân Dung (`Image avatarImage`):** Đây là mảng hiển thị lớn nhất chiến 60-70% diện tích thẻ. Cắt Mask gọn gàng trong khung.
*   **Trạng Thái Bận Rộn (`GameObject busyIndicator`):** Yêu cầu vẽ một khung màng đen mờ (Dimmer) đè lên cả thẻ bài, ở giữa thả một Icon ổ khoá / mỏ neo / chữ "Đang Hành Quân" / "Đang Nghỉ Ngơi". Đoạn code này chỉ BẬT cái Overlay này lên khi con tướng đang kẹt trong một tiến trình (Khám bệnh, Đi farm quái, Xếp hình...). Khi có lớp này, người chơi mặc dù vẫn click xem thông tin được nhưng không thể nhét con tướng này vào đội hình khác.

### 2.2 Khu Vực Đọc Nhanh Thông Số (Info Bar):
*   **Tên Tướng (`TextMeshProUGUI nameText`):** Đặt ở góc dưới/nửa dưới của thẻ.
*   **Cấp Độ (`TextMeshProUGUI levelText`):** Đặt góc trái/phải trên. Format hiển thị "Lv. 9".
*   **Lực Chiến (`TextMeshProUGUI combatPowerText`):** Con số quan trọng nhất. Cần có Icon thanh Kiếm / Nắm Đấm đặt cạnh. Bỏ màu nhấn nổi bật (Vàng hoặc Cam). Format: "CP: 4500".

### 2.3 Các Biểu Tượng Ký Hiệu (Icons):
Khu vực này có thể xếp thành một dải dọc hoặc ngang dọc theo viền thẻ bài:
*   **Giới tính (`Image genderIcon`):** Code cần nạp 2 viên Graphic: `maleIcon` (Ký hiệu sao Hoả) và `femaleIcon` (Ký hiệu sao Kim).
*   **Hệ / Nghề Nghiệp (`Image professionIcon`):** Icon đại diện cho chức nghiệp (Chiến binh, Đấu Sĩ, Pháp sư...). Chỗ này cần slot rỗng để code quăng hình vào.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   **Tính Đa Dụng:** Prefab này sẽ bị LayoutGroup (Của các Panel mẹ như BarrackPanel, SquadSelectionPanel) co kéo/Scale liên tục cho vừa kích thước lưới. Nên Designer tuyệt đối phải dùng hệ thống Anchor linh hoạt (Tỉ lệ %), KHÔNG được fix cứng giá trị scale bằng Pixel.
*   **Nút Bấm Gốc:** Bản thân lớp viền ngoài cùng của thẻ bài chính là một cái `Button` bự (`cardButton`). Khi bấm, nó phóng to cái thẻ thành Panel phân tích chi tiết: `UIManager.ShowPanel(UIPanelType.HeroInfo)`.

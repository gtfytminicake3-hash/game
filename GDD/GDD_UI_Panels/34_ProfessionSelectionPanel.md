# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `ProfessionSelectionPanel.cs`
**Loại thành phần:** Popup Mờ / Bảng Lựa Chọn Chức Nghiệp.

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`ProfessionSelectionPanel` là một bục vinh quang. Nó chỉ xuất hiện ở một khoảnh khắc duy nhất: Ngày một Em Bé trưởng thành (Tròn 18 tuổi). Nó sẽ Popup đè lên màn hình, yêu cầu người chơi định hướng nghề nghiệp cho Tướng mới nở: Chiến Binh (Búa/Kiếm), Cung Thủ (Cung/Nỏ), hay Pháp Sư (Gậy phép). Việc lựa chọn này là Vĩnh Viễn.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế một Bảng Popup hình chữ nhật Nằm Ngang cỡ lớn hiện ra giữa màn hình.

### 2.1 Tiêu Đề Cáo Thị:
*   **Dòng Thư Báo (`TextMeshProUGUI titleText`):** Đặt ở giữa mép trên. Format: "Hãy chọn đường đi cho [Tên Hero]". Mảng này có thể vẽ background mây mù/nhật ký lãng mạn hoài cổ.
*   **Nút Thoát (`Button closeButton`):** Chỉ là nút X bé để người chơi có thể tạm tắt đi lo việc khác.

### 2.2 Bục Lựa Chọn (Tâm Điểm Layout):
Yêu cầu vẽ **3 Cái Thẻ (Card/Poster)** dài sọc đứng kề nhau rải đều màn hình theo chiều ngang (Sử dụng Horizontal Layout Group):
*   **Thẻ Chiến Binh (`Button warriorButton`):** Minh hoạ nhân vật cầm Kiếm/Khiên hoặc Búa dũng mãnh. Màu sắc chủ đạo: Đỏ / Cam / Thép tĩnh.
*   **Thẻ Cung Thủ (`Button archerButton`):** Minh hoạ nhân vật cầm Cung lẩn khuất. Màu sắc chủ đạo: Xanh Lá / Nâu Da.
*   **Thẻ Pháp Sư (`Button mageButton`):** Minh hoạ nhân vật cầm quyền trượng bay lơ lửng sấm sét/lửa. Màu sắc chủ đạo: Tím / Xanh Biển Bí Ẩn.
*   *Lưu ý Click:* Cả 3 thẻ này chính là 3 Nút Bấm vĩ đại. Bấm vào đâu là hệ thống "Chốt Đơn" vào thẳng nghề đó mà không có Box Xác Nhận lần 2.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   **Phép Màu Chuyển Dạng:** Khoảnh khắc nhấn vào nút Nghề Nghiệp, Tướng (Em bé da trơn phàm trần) sẽ mọc ra quần áo xịn sò và 1 Chiêu Thức ngẫu nhiên bẩm sinh theo môn phái. Popup này tự huỷ diệt và Màn Hình Ở Dưới (HeroInfoPanel) sẽ nháy sáng Flash nạp lại hình ảnh Cực Ngầu của Tướng. Do đó 3 tấm Card mô tả ở trên Design vẽ càng ngầu thì cảm giác chuyển biến nghề càng phê.

# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `TutorialPanel.cs`
**Loại thành phần:** Nhóm Overlay Panel / Cuốn Sổ Hướng Dẫn Tân Thủ (Tutorial).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`TutorialPanel` là màn hình hỗ trợ người chơi mới làm quen với game. Cách hoạt động hiện tại của nó giống như một cuốn Sổ Tay lật từng trang, hiển thị nội dung text tĩnh để người chơi đọc luật chơi hoặc các mẹo vặt. 

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế một Bảng Vuông/Chữ nhật kích thước trung bình hoặc lớn nằm giữa màn hình. Phong cách như một cuốn sách cổ, cuộn giấy da, hoặc một Bảng Thông Báo trang trọng.

### 2.1 Tiêu Điểm / Khung Sườn:
*   **Tiêu đề Bảng (`TextMeshProUGUI panelTitleText`):** Mép trên cùng. (Vd: "Hướng Dẫn Tân Thủ").
*   **Nút Thoát Nhanh (`Button closeButton`):** Bấm X hoặc Mũi tên Trở về để Tắt ngang bảng hướng dẫn ở bất kỳ trang nào.

### 2.2 Nội Dung Chính (Main Content):
*   **Khu Vực Chữ Nghĩa (`TextMeshProUGUI tutorialText`):** Một ô Text rất to nằm chễm chệ ở giữa bảng để chứa nội dung hướng dẫn. 
*   *Lưu ý mở rộng cho Designer:* Mặc dù Code hiện tại chưa có phần gắn Hình Ảnh (`Image`) Minh Họa, nhưng kinh nghiệm thiết kế UI Tutorial là LUÔN chừa một khoảng trống (Box) to đùng phía trên hoặc bên cạnh Khối Chữ để sau này Coder đắp ảnh minh họa (Vd: Hình minh hoạ cách kéo thả lính) vào.

### 2.3 Điều Hướng (Navigation Footer):
Khu vực dưới đáy bảng cần có 2 Nút Bấm chia làm 2 cực Trái Phải rõ ràng:
*   **Nút Quay Lại (`Button prevButton`):** 
    *   Vị trí: Góc Vế Trái.
    *   Icon: Mũi tên chỉ Trái (<-).
    *   Text Gắn kèm (`TextMeshProUGUI prevButtonText`): "Quay Lại", "Trang Trước".
    *   *Tính năng:* Nút này sẽ Vô Hình (Tàng hình hoàn toàn) khi người chơi đang đứng ở Trang số 1.
*   **Nút Đi Tiếp (`Button nextButton`):**
    *   Vị trí: Góc Vế Phải.
    *   Icon: Mũi tên chỉ Phải (->).
    *   Text Gắn kèm (`TextMeshProUGUI nextButtonText`): "Tiếp Theo".
    *   *Tính năng Biến Đổi:* Khi người chơi lật đến Trang Cuối Cùng, chữ trên màn hình sẽ đổi thành "Đóng" hoặc "Hoàn Thành", và bấm vào sẽ tắt luôn bảng. Hãy đổ màu Nút này chói lóa (Xanh Lục/Vàng) để hút mắt người dùng, khuyến khích họ ráng bấm cho hết Tutorial.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Panel này có tính chất Đọc-Hiểu Tĩnh. Số lượng trang đang mặc định Cứng trong Code là 4 Bước. Graphic Designer không cần thiết kế Animation lật trang 3D phức tạp, chỉ cần Layout gọn gàng rành mạch là đủ. Giữ phông chữ to và độ tương phản cao để đảm bảo người chơi có thể dễ dàng đọc được hướng dẫn.

# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `EquipmentDetailPanel.cs`
**Loại thành phần:** Popup Overlay / Thẻ Chi Tiết Trang Bị.

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`EquipmentDetailPanel` hoạt động như một thẻ bài (Card) phóng to, nổi lên giữa màn hình khi người chơi nhấn vào bất kỳ món đồ nào trong Túi Đồ (Inventory) hoặc trên người Tướng. Popup này liệt kê toàn bộ lai lịch hiển hách của món đồ (Cấp độ, Điểm kinh nghiệm, Chỉ số gốc, Chỉ số dòng ẩn), đồng thời cho phép Khoá/Mở khoá vòng quay may rủi hoặc cường hoá nó.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Cần vẽ một Popup hình chữ nhật đứng (Vertical Card) nằm đè lên một lớp màn đen mờ (Dimmed Background).

### 2.1 Khu Vực Định Danh (Đỉnh Thẻ):
*   **Tên Trang Bị (`TextMeshProUGUI equipNameText`):** Text to rõ. Script sẽ tự đính kèm dòng chữ `<color=red>[Đã Khóa]</color>` lên trước tên nếu món đồ này đang bị khoá bảo vệ. Do đó cần chừa khoảng trống bên trái đủ rộng.
*   **Cấp Độ (`TextMeshProUGUI equipLevelText`):** Hiển thị "Lv. 15". Nếu max cấp thì hiển thị "Tối Đa".

### 2.2 Khu Vực Thanh Tiến Trình (Thân Trên):
*   **Chữ Kinh Nghiệm (`TextMeshProUGUI expProgressText`):** Báo cáo số EXP để lên đời (Vd: "EXP: 50 / 200"). Khuyến nghị Designer nên vẽ thêm một thanh Slider (ProgressBar) rỗng ở dưới dòng chữ này để đội Code có thể tự bơm fillAmount vào cho trực quan hơn (mặc dù script hiện tại mới chỉ gán Text).

### 2.3 Khu Vực Đọc Chỉ Số (Thân Giữa):
*   **Khung Lọc Chỉ Số (`TextMeshProUGUI statsText`):** Đây là một Box chữ khá lớn, gánh tải vác nhiều dòng văn bản được nhồi nhét từ Code (Xuống dòng bằng `\n`):
    *   Dòng 1: Chỉ số cốt lõi (Màu thường/Trắng). Ví dụ: `ATK +500` hoặc `ST. Chí Mạng +20.5%`.
    *   Dòng 2: Nội Tiết 1 (`bonusStat1Description`) (Màu xanh lá `#00FF00`).
    *   Dòng 3: Nội Tiết 2 (`bonusStat2Description`) (Màu xanh lá `#00FF00`).
    *   *Yêu cầu Design:* Font chữ ở đây phải sắc nét, hỗ trợ Rich-Text. Viền quanh Box này có thể trang trí hoa văn Ma Thuật/Phù Thuỷ.

### 2.4 Dàn Nút Bấm Xử Lý (Dưới Cùng/Footer):
Khu vực này có thể xếp thành hàng ngang (Grid) hoặc Dọc tuỳ ý, chứa 4 nút:
*   **Nút "Mặc Bị" (`Button equipButton`):** Bấm để đeo lên người.
*   **Nút Cường Hoá Nhanh (`Button quickUpgradeButton`):** Nút to/vàng sáng. Bấm gọi búa rèn nâng cấp đồ.
*   **Nút Khoá Đồ (`Button lockButton`):** Yêu cầu có Icon Chìa Khoá / Ổ Khoá đóng mở. Giúp người chơi cài chốt an toàn để không vô tình phi tang món đồ xịn đi làm phôi tế thần. Trạng thái chữ con của nó sẽ đảo chiều giữa "Mở Khóa" và "Khóa Đồ".
*   **Nút Đóng (`Button closeButton`):** Nút X hoặc "Quay Lại".

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Panel này không có hàm đóng vĩnh viễn, nó chỉ dùng `gameObject.SetActive(false)` để lẩn x trốn chui nhủi đằng sau scene, chờ đợi lượt gọi lên tiếp theo. Cần tối giản khối lượng Draw Call.

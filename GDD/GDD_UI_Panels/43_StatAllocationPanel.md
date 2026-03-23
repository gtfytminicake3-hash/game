# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `StatAllocationPanel.cs`
**Loại thành phần:** Popup Mờ / Bảng Tăng Điểm Tiềm Năng (Stat Allocation).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`StatAllocationPanel` là một Popup nhỏ gọi ra từ Màn Hình Chi Tiết Tướng (`HeroInfoPanel`). Khi Tướng lên cấp, họ nhận được Vài "Điểm tự do" (Free Points). Người chơi sẽ mở bảng này lên, cân nhắc đổ Điểm vào Máu (HP), Sức Đánh (ATK), Phòng Thủ (DEF) hay Tốc Độ (SPD) tùy theo gu thẩm mỹ. Bảng này mang tính sống còn trong việc "Build" Tướng rác thành Thần Đồng.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế một Bảng Vuông/Chữ Nhật đứng, lơ lửng giữa màn hình. Phong cách RGP Kinh Điển (Giống bảng nâng điểm của Diablo hay MU Online).

### 2.1 Header & Điểm Dư:
*   **Tiêu Đề Bảng (`TextMeshProUGUI panelTitleText`):** Đỉnh màn hình. (Vd: "Phân Bổ Tiềm Năng").
*   **Quỹ Điểm Hiện Có (`TextMeshProUGUI freePointsText`):** Đặt to oành ngay dưới Tiêu Đề. (Vd: "Điểm dư: 5"). Yêu cầu có Icon Chấm Sáng/Ngôi Sao lấp lánh bên cạnh.
*   **Nút Thoát (`Button closeButton`):** Bấm X.

### 2.2 Khu Vực Cày Cuốc Chỉ Số (4 Hàng):
Vẽ 4 hàng ngang xếp chồng lên nhau, mỗi hàng đại diện cho 1 loại chỉ số: Sinh Mệnh (HP), Sát Thương (ATK), Phòng Thủ (DEF), Tốc Độ (SPD). Trong MỖI hàng cần có:
*   **Tên Chỉ Số (Stative Text):** Chữ "HP", "ATK"... Không cần map code, vẽ cứng trên màn hình. Kèm Icon Trái Tim, Thanh Kiếm, Cái Khiên, Đôi Giày.
*   **Con Số Nhảy Múa (`TextMeshProUGUI hpValueText`, `atkValueText`,...):** Nằm giữa. Format: "Base (+Added)". Vd: `1500 (+250)`. Code sẽ tự động đẩy thông số vào các biến mang đuôi `ValueText` này.
*   **Nút Tăng (`Button hpPlusBtn`, `atkPlusBtn`,...):** Nút hình Dấu Cộng (+). Màu Xanh Lá/Vàng tươi sáng. 
*   **Nút Giảm (`Button hpMinusBtn`, `atkMinusBtn`,...):** Nút hình Dấu Trừ (-). Màu Đỏ/Xám. Nút này dùng để "Hối Hận" (Trừ lại điểm vừa cộng nhầm).

### 2.3 Phút Chốt Đơn (Footer):
*   **Nút Xác Nhận (`Button confirmButton`):** Nút cực to nằm bẹp dưới đít. Có chữ `confirmButtonText` (Vd: "Lưu Cấu Hình").

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   **Bộ Tìm Kiếm Tự Động (Auto-Wire):** Trong tập lệnh `StatAllocationPanel.cs`, Coder đã viết 1 thuật toán quét Tên Vật Thể (`AutoWire`). Nghĩa là nếu Designer quăng các nút vào mà mệt quá quên kéo thả vào Cột Script bên Inspector cũng không sao. **Chỉ cần đặt đúng tên Node**.
    *   Nút Tăng hãy đặt tên có chữ: `plus` hoặc `add`. (Vd: `HP_PlusBtn`).
    *   Nút Giảm hãy đặt tên có chữ: `sub` hoặc `minus`. (Vd: `HP_MinusBtn`).
    *   Text thông số hãy đặt tên có chữ: `val`. (Vd: `HP_ValText`).
*   Graphic Nút: Đừng vẽ nút (+) và (-) bằng hình ảnh bitmap có sẵn Text. Hãy vẽ Component Nút (Button) 9-Slice trơn, rồi đính 1 lớp TextMeshPro (Chữ +) lên trên để đảm bảo sắc nét trên màn hình Retia.

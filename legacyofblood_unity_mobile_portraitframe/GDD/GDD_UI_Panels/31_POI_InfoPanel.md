# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `POI_InfoPanel.cs`
**Loại thành phần:** Popup Overlay / Bảng Thông Tin Cứ Điểm (World Map).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`POI_InfoPanel` là cái Popup hiện ra khi người chơi đang đi dạo ngoài Bản Đồ Thế Giới (World Map) và bấm tay vào một Cứ Điểm rình rập nào đó (Ví dụ: Trại Goblin, Mỏ Vàng, hoặc Tháp Thử Thách). Mục đích của nó là tung tin tình báo (Chiến lực khuyến nghị, Danh sách quái...) trước khi người chơi quyết định ném quân vào chỗ chết.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế một Bảng Nhỏ (Popup) trồi lên từ Dưới cùng màn hình (Bottom Sheet) hoặc nổi lên giữa Màn hình.

### 2.1 Khu Vực Định Danh Cứ Điểm (Header):
*   **Tên Địa Danh (`TextMeshProUGUI poiNameText`):** Text rất to rõ ràng ("Rừng Rậm Elven", "Hang Ổ Rồng").
*   **Nút Tắt (`Button closeButton`):** Bấm X hoặc chữ "Trở về".

### 2.2 Khu Vực Phân Luồng Thông Tin (Trọng Tâm):
Dev đã chia cái Box này thành 2 phiên bản chồng lên nhau (Tự động bật/tắt theo ID của Cứ điểm). Designer có thể xếp chúng nằm đè lên nhau trong Editor (Cùng 1 toạ độ) rồi Dev sẽ nhét chúng vào 2 Group ẩn hiện.
*   **Phiên Bản Bãi Farm Thường (Normal POI):**
    *   **Cấp Độ Khó (`TextMeshProUGUI difficultyText`):** Ví dụ "Độ khó: 5".
    *   **Lực Chiến Khuyến Nghị (`TextMeshProUGUI recommendedCpText`):** Con số để dọa người chơi (Vd: "Yêu Cầu: 50.000 CP").
*   **Phiên Bản Tháp Thử Thách (Tower Of Trials):**
    *   Cần gói vào 1 cái Object chung tên là `Tower Info Container`.
    *   **Tầng Hiện Tại (`TextMeshProUGUI currentFloorText`):** Ví dụ "Đang leo tháp tầng: 35".
    *   **Thời Gian Chờ (`TextMeshProUGUI recoveryTimeText`):** Dạng đồng hồ bấm giờ ("Cooldown: 01:25:10"). Tháp này đánh thua phải chờ hồi máu, lúc chờ thì chữ màu Trắng/Đỏ. Hết thời gian chờ chữ này biến thành Text Xanh "Ready".

### 2.3 Nút Quyết Định (Footer):
*   **Nút Viễn Chinh (`Button exploreButton`):** Nút bự nhất bảng. Có gắn `exploreButtonText` (Chữ "Khám Phá" hoặc "Điều Quân"). 
    *   *Lưu ý cho Designer:* Khi Tháp Thử Thách đang bị dính Cooldown (Chờ hồi phục), nút này sẽ bị dập Tối màu (Disabled). Hãy lên màu đẹp cho Disabled State.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Panel này chứa 1 tính năng "Trị Tàng Hình": Đôi lúc trên Bản đồ, do lỗi khởi tạo Canvas, bảng này sẽ bị rớt ra ngoài Không gian. Dev đã viết tính năng tự kéo nó về gắn lên `mainCanvas` để chống lỗi tàng hình UI.
*   Thiết kế linh hoạt: Nếu rảnh, Designer có thể gắn thêm một dải lưới ảnh nhỏ bên trái Bảng này để Dev nhét Hình ảnh của Cứ Điểm vào cho sinh động (Hiện tại Script đang thuần chữ).

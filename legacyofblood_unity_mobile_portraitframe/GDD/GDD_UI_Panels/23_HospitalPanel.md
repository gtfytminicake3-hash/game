# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `HospitalPanel.cs`
**Loại thành phần:** Full Screen Panel / Màn Hình Quản Lý Bệnh Xá.

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`HospitalPanel` quản lý công trình Bệnh Viện (Chạm từ Bản đồ chính). Nơi đây tiếp nhận tất cả những Tướng bị trọng thương hoặc nứt mẻ sau khi đi Viễn Chinh hoặc Đấu Trường về. Tướng nằm viện sẽ rơi vào trạng thái "Busy" không thể làm gì khác cho đến khi trị khỏi. Panel này chia người bệnh thành 2 khoa: Thương Khí Chữa Nhanh (Nhẹ) và Thương Tật Trầm Trọng (Nặng).

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế một màn hình phân chia rõ 2 khu vực dọc (Trên/Dưới) hoặc ngang (Trái/Phải).

### 2.1 Khu Vực Tiêu Đề & Nút Hệ Thống (Header):
*   **Tiêu đề Bảng (`TextMeshProUGUI panelTitleText`):** Đặt trên cùng. (Ví dụ: "Bệnh Xá Y Liệu").
*   **Nút Đóng (`Button closeButton`):** Góc trên phải.
*   **Nút Nâng Cấp Công Trình (`Button upgradeBuildingButton`):** Góc trên trái hoặc đặt cạnh Tiêu đề. Nút này bấm sẽ chuyển panel sang Bảng Nâng cấp để ép Level toà nhà tăng tốc độ hồi phục toàn cục.

### 2.2 Khu Vực Điều Trị Trọng Thương (Severe Injury Ward):
Dành cho những Tướng nằm liệt giường. Tướng ở khu này đếm ngược tốn thời gian tính bằng Phút/Giờ. Cần trả vàng để chữa tức thì hoặc phó rưới cho tự nhiên (chờ lâu).
*   **Tiêu đề phân khu (`TextMeshProUGUI severeInjuryLabelText`):** Text nhãn ghi "Ca Trọng Thương". Có thể tô màu Đỏ/Cam.
*   **Lưới Danh Sách Bệnh Nhân (`Transform severeInjuryListContainer`):** Một Scroll View lồng Layout Group (Grid hoặc Horizontal tùy thiết kế thẻ Prefab). Code sẽ thả thẻ Bệnh Nhân vào đây.

### 2.3 Khu Vực Điều Trị Tiểu Phẫu (Light Injury Ward):
Dành cho những Tướng bị sứt sẹo nhẹ. Tướng ở đây đếm ngược nhanh hơn (Khoảng vài phút). Mọi cơ chế y hệt mảng trên.
*   **Tiêu đề phân khu (`TextMeshProUGUI lightInjuryLabelText`):** Text nhãn ghi "Ca Thương Nhẹ". Tô màu Vàng/Xanh lá.
*   **Lưới Danh Sách Cơ Sở (`Transform lightInjuryListContainer`):** Cũng là một vùng Scroll View riêng biệt. Không được Merge chung với khu Trọng thương trên mặt Code (Inspector phải tách bạch 2 lưới).

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Panel này KHÔNG DÙNG cái `heroCardPrefab` gốc (Ở file 18), mà dùng một biến thể bọc đường băng bó thuốc men chắp vá gọi là `injuredHeroCardPrefab` (Sẽ mô tả ở doc sau). 
*   **Trải nghiệm Tĩnh/Động:** Trong code có xử lý quét event liên tục `HospitalSystem.OnHeroHealed`. Nghĩa là nếu bệnh nhân thứ tự 3 trong danh sách vừa hồi phục xong (do hết giờ), thì không cần bấm nút load lại trang, cái thẻ Thứ 3 sẽ bốc hơi ngay trước mặt người chơi và Layout thu màn vào lấp chỗ trống ngay lập tức. Layout Group của Unity sẽ tự lo vụ này.

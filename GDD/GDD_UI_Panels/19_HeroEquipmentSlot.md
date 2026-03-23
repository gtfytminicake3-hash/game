# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `HeroEquipmentSlot.cs`
**Loại thành phần:** Item Prefab (Ô Vuông Trang Bị - Lắp ráp vào Bảng Thông Tin Tướng).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`HeroEquipmentSlot` đại diện cho MỘT ô trang bị (Slot) duy nhất trên người Hero (Nhân vật có 6 slot chẵn: Vũ khí, Áo giáp, Mũ bảo hiểm, Giày cạp, Nhẫn 1, Nhẫn 2). Bản thân nó là một ô hình vuông tĩnh, nhưng sẽ tự động biến đổi giao diện qua lại giữa 2 dạng: Rỗng (Chưa mặc gì) và Đầy (Đã đeo đồ).

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế một ô vuông nhỏ vuông vức. Kích thước tương đương icon kỹ năng (Khoảng 100x100px tới 120x120px là đẹp).

### 2.1 Trạng Thái Bụng Đói (Rỗng - `GameObject emptyStateContent`):
Trạng thái này hiện lên khi Hero đang cởi truồng ở vị trí đó.
*   Yêu cầu vẽ một khung viền kim loại tối màu. Ở giữa là **Bóng Nước (Silhouette / Watermark)** hình dạng mờ mờ của loại trang bị mặc định (Vd: Hình thanh kiếm chéo, Hình cái nón rơm). Mức độ trong suốt (Alpha) khoảng 30%-50%.

### 2.2 Trạng Thái No Nê (Đã Mặc - `GameObject filledStateContent`):
Trạng thái này hiện lên đè bẹp trạng thái Rỗng khi Hero đã mặc đồ vào.
*   **Vector Khung Mầu Phẩm Chất (`Image rarityBorder`):** Ô vuông viền ngoài cùng. Không được vẽ chết Line Art. Phải là một file vector/sprite đơn nhân trắng (Grayscale) để Code có thể nhuộm màu viền tự động (Trắng: D, Lục: C, Lam: B, Tím: A, Vàng: S, Cam: SS, Đỏ: SSS).
*   **Trái Tim Trang Bị (`Image equipmentIcon`):** Lớp Hình ảnh Avatar của món đồ. Cắt lọt thỏm vào trong Khung viền. Trừ tỷ lệ hở 2-3px mép viền.
*   **Chỉ Số Cường Hoá (`TextMeshProUGUI levelText`):** Đặt ở góc phải/trái dưới của Box. Font số nhỏ, Format "+15" (Có dấu cộng đằng trước). Có outline đen.

### 2.3 Cấu Trúc Khối Nút Bấm:
Tất cả những UI/Image ở trên phải nằm lọt trong lòng mẹ mìn `Button slotButton`. Nút này trải dài full diện tích cái Slot.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Script này **KÉP** 2 Container: `emptyStateContent` và `filledStateContent`. Dev lúc gắp thả vào Editor nhớ nhét UI tĩnh vào Empty và UI Động vào Filled để code SetActive(true/false) đảo nhau cho chuẩn.
*   Luồng sự kiện: Mặc đồ chạy rồi, nếu người dùng click vào cái ô này, sẽ xảy ra 2 trường hợp:
    1. Lỗ trống: Kích hoạt Event bắn tin đi đòi mở kho chứa đồ ra để lượm nhặt.
    2. Úp sọt đồ: Nở bung thẻ báo cáo chi tiết chỉ số món đồ (Gọi Popup `EquipmentDetailPanel`).

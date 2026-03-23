# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `SettingsPanel.cs`
**Loại thành phần:** Popup Overlay / Màn Hình Cài Đặt.

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`SettingsPanel` là bảng tùy chỉnh thông số game cơ bản: Âm lượng, Ngôn ngữ, và Cấu hình đồ hoạ (Chống giật lag UI). Thường được gọi ra từ MenuPanel góc màn hình.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế một Bảng Vuông/Chữ nhật dựng đứng (Popup) vừa phải, nằm phủ giữa màn hình.

### 2.1 Tiêu Điểm / Khung Viền:
*   **Tiêu đề Cài Đặt (`TextMeshProUGUI titleText`):** Đặt ở giữa mép trên. (Vd: "Cài Đặt Hệ Thống").
*   **Nút Thoát (`Button closeButton`):** Bấm X.

### 2.2 Các Khối Tùy Chỉnh (Settings Blocks):
Nên xếp thành từng hàng dọc (Rows) đi xuống, mỗi hàng là một tính năng:
*   **Hàng 1 - Điều Chỉnh Âm Thanh:**
    *   Cần vẽ một Thanh Trượt (`Slider volumeSlider`). Thanh này có 1 cái Núm (Knob) kéo qua kéo lại. Kế bên có thể vẽ cái loa (Icon).
*   **Hàng 2 - Đổi Ngôn Ngữ:**
    *   Cần vẽ Nút bự (`Button changeLanguageButton`). Text bên trong nút: "Đổi Ngôn Ngữ" hoặc "Change Language". Mọi logic dịch thuật Tiếng Anh / Tiếng Việt đổi chéo cho nhau Code đã lo hết thảy.
*   **Hàng 3 - Đồ Hoạ / Hiệu Ứng Giao Diện:**
    *   Cần vẽ một Nút Gạt Cười/Khóc (Switch) hoặc Dấu Tích Vuông (`Toggle dynamicUIToggle`). Dùng để tắt bớt mấy cái múa may của Card/Nút để máy yếu đỡ giật lag.
    *   Bên cạnh Toggle cần gắn chữ (`TextMeshProUGUI dynamicUIText`): "Bật Hiệu Ứng Nổi" hoặc "Enable Dynamic UI".

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Panel này cài đặt rất ít hàm, không có gì phức tạp. Graphic Designer chỉ cần tập trung vẽ cái Nút Trượt `Slider` sao cho mượt (Có thanh Background xám và thanh Fill màu xanh). Kích thước Slider cần đủ bự để ngón tay miết vào không bị trượt.
*   Font chữ trong bảng này ưu tiên dễ đọc, viền mỏng.

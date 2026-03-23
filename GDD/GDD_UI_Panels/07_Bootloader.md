# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `Bootloader.cs`
**Loại thành phần:** Full Screen Menu / Màn Hình Khởi Động Game (Loading Screen).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`Bootloader` là màn hình đầu tiên người chơi nhìn thấy khi bật app. Nó có trách nhiệm giấu đi quá trình tải tài nguyên, nạp data lưu trữ, đồng thời cung cấp một màn hình chuẩn bị (Loading Bar) mang đậm phong cách Dark Fantasy (Chất lỏng máu).

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế khung sườn cho màn hình Loading gồm:

### 2.1 Màn Hình Chờ (Tap To Start):
*   Tính năng này có thể được gộp làm một hoặc chia phase. Hiện tại Code dùng `tapToStartGroup` chứa màn hình tĩnh chờ người dùng chạm tay vào màn hình rồi mới nhảy sang Loading.
*   **Text "Chạm Để Bắt Đầu" (`TextMeshProUGUI tapToStartText`):** Đặt ở nửa dưới màn hình, có hiệu ứng Fade nhấp nháy (Breathing).

### 2.2 Màn Hình Tải Game (Loading Screen - Được Code tự động vẽ ra):
⚠️ **QUAN TRỌNG:** Script này hiện tại đang KHÔNG dùng Prefab kéo thả từ Inspector (chỉ lấy mỏ neo `loadingScreenGroup`), mà nó tự động TẠO RA các component Hình ảnh thông qua hàm `SetupDynamicProgressBar`. Do đó Designer bắt buộc phải xuất đúng tên file và bỏ vào thư mục `Resources/UI/`:
*   **Ảnh Nền (Backdrop):** 
    *   Tĩnh: `Resources/UI/BootloaderBackdrop.png`.
    *   Động (Sequence): `Resources/UI/BootloaderBackdrop_Frames` (Một chuỗi file để chạy ảnh GIF).
*   **Khung Sườn Thanh Loading (Chassis):** Xuất file `Resources/UI/LoadingBarChassis.png`. Đây là cái viền bọc ngoài của thanh đếm. Khuyến nghị vẽ viền mạ vàng gai góc hoặc viền xương rồng.
*   **Lõi Thanh Loading (Fill Blood):** Xuất file `Resources/UI/liquid_loadingbar_rmbg.png`. Vẽ bằng dải màu máu đỏ sẫm. Code sẽ trượt (fillAmount) cái ruột này từ trái qua phải.
*   **Khối Tracking Giọt Máu %:** Xuất file `Resources/UI/blood_percent_rmbg.png`. Là một cục tròn/giọt máu trượt trên đầu mép phải của thanh Fill Blood. Cục này dùng làm Background để in con số "99%".

### 2.3 Phụ đề Trạng Thái (Loading Text):
*   Có một `TextMeshProUGUI loadingText` nằm dưới thanh máu gánh vác việc báo cáo tiến độ bằng các câu thoại mượt mà ("Awakening ancient bloodlines...", "Summoning legends..."). Chữ phải có Outline đen để nổi bần bật.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Quá trình Loading chia làm 4 chặng ảo (0->20%, ->50%, ->80%, ->100%). Hoàn thành 100%, tự xoá sạch thanh Loading và gọi mở `MainScreen`.
*   Vì Code tự `AddComponent<Image>()` ở runtime, nên Designer không cần ráp Layout phức tạp cho cái Loading Bar này trong scene, chỉ cần vứt đúng hình 2D vào đường dẫn thư mục `Resources/UI/` là Bootloader tự lắp ráp và tự chạy.

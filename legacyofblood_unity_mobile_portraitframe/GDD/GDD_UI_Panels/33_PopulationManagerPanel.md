# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `PopulationManagerPanel.cs`
**Loại thành phần:** Full Screen Panel / Bảng Quản Lý Nhập Cư.

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`PopulationManagerPanel` là màn hình Kế Hoạch Hóa Gia Đình của game. Truy cập thông qua Trại Lính (Barrack). Khi nhà màng chật ních (Ví dụ sức chứa 50/50), người chơi buộc phải vào đây để chọn ra những con Tướng phế vật nhất đuổi ra khỏi làng, dành Slot cho những con Tướng mới xịn hơn. Mục tiêu của màn hình này là Liệt kê thật nhanh và Xoá thật lẹ.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế một màn hình quản lý Danh Sách dọc truyền thống, Layout giống y hệt màn hình Hòm Thư hoặc Bệnh Xá nhưng xài thẻ dạng Ngang.

### 2.1 Khu Vực Tiêu Điểm (Top Header):
*   **Chỉ Tiêu Dân Số (`TextMeshProUGUI populationCountText`):** Một Box Text nằm chễm chệ trên cùng. Format "Dân Số: 45 / 50". Đội Design có thể vẽ thêm một thanh Slider Bar (ProgressBar) rỗng nằm dưới chữ này cho đẹp, tuy bộ Code hiện tại chưa hỗ trợ fill thanh đó nhưng tương lai dễ thêm.
*   **Nút Tắt Bảng (`Button closeButton`):** Chỉ cần nút X.

### 2.2 Khu Vực Trục Xuất (Main Body):
*   **Lưới Danh Sách (`Transform listContainer`):** Một Scroll View chiếm 90% diện tích màn hình. Component Content của nó nên được gắn một `VerticalLayoutGroup` để các Thẻ xếp hàng đều đặn từ trên xuống.
*   **Prefab Thẻ Dân So (`GameObject heroCardPrefab`):** Nhúng trực tiếp bản Prefab thẻ NGANG đã phân tích trong File 32 (`PopulationHeroCard`) vào đây. KHÔNG nhúng thẻ DỌC `HeroCard`.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Do đặc thù màn hình này là "Trục Xuất", khi Click nút Sa Thải, The Bài đó sẽ bay màu (Bốc hơi tức khắc khỏi Lưới Danh Sách), sau đó Scroll View tự chớp Layout dồn hàng lên lấp đầy khoảng trống ngay lập tức. Graphic Designer lưu ý KHÔNG gắn những cái Effect/Particle nặng nề vào Card Prefab kéo hệ thống bị nghẽn (Stuttering) trong khung cảnh này. 
*   Tuỳ biến thêm: Nếu Design thích sự an toàn, có thể thiết kế một cái Popup Nhỏ xác nhận "Bạn có chắc chắn đuổi con này không?". Tuy nhiên, trong luồng code hiện tại, Dev đang chém đứt đuôi nòng nọc (Bấm là xoá luôn không hỏi). Mọi tinh chỉnh cần thống nhất giữa 2 bên.

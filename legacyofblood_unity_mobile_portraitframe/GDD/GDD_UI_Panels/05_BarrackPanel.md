# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `BarrackPanel.cs`
**Loại thành phần:** Full Screen Panel / UIPanel chứa tính năng Doanh Trại cốt lõi.

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`BarrackPanel` là kho tướng, nơi tải và xếp hạng toàn bộ các hero mà người chơi đang sở hữu. Script này có nhiệm vụ tạo ra vô số thẻ nhân vật (`HeroCard`) và tự động nhét vào một khung cuộn (ScrollView). Danh sách này luôn được sắp xếp theo tổng Sức mạnh (Combat Power) từ cao xuống thấp.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Panel này cần thiết kế một giao diện khung rộng, tập trung tối đa không gian cho danh sách.

### 2.1 Khu Vực Header (Tiêu Đề & Nút Công Cụ):
*   **Tiêu đề (`TextMeshProUGUI panelTitleText`):** Chữ lớn ở đỉnh màn hình (Text key: `panel_title_barrack`).
*   **Nút Quản Lý Dân Số (`Button populationManagerButton`):** Dùng icon người/group. Khi bấm sẽ chuyển sang `PopulationManagerPanel`. Dùng text `btn_manage_population`.
*   **Nút Nâng Cấp Nhà Bạt (`Button upgradeBuildingButton`):** Liên kết với công trình `TownHall`. Có icon búa nâng cấp. Dùng text `btn_upgrade`.
*   **Nút Đóng (`Button closeButton`):** Cần thiết kế mũi tên Back ở góc hoặc dấu X ở góc đỉnh tay phải.

### 2.2 Khu Vực Hiển Thị Danh Sách (Danh sách Tướng Lưới Trọng Tâm):
*   **Vùng Phức Hợp Chứa Thẻ (`Transform heroListContainer`):** 
    *   Thiết kế một màn hình Grid Layout dạng cuộn dọc (Vertical ScrollRect). 
    *   Chia cột sao cho hiển thị được 4 đến 5 thẻ theo chiều ngang (Nếu là màn dọc thì 3 đến 4 thẻ dọc). Không gian này sẽ bị script gọi Prefab `HeroCard` ném vào lấp đầy.
    *   *Lưu ý UI:* Cần Mask kỹ càng để khi cuộn thẻ tướng không lọt ra ngoài phạm vi viền chữ nhật.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Panel này phản ứng real-time. Nghĩa là cứ mỗi khi `DataManager` có sự kiện thay đổi tướng (`OnHeroListChanged`), hàm `RefreshHeroList()` lập tức xóa sạch các thẻ tướng cũ trên màn hình và clone ra dải thẻ tướng mới cập nhật Combat Power.
*   Thiết kế Prefab `HeroCard` (Sẽ mô tả ở 1 file Spec riêng biệt) phải khớp kích thước với Grid Layout Group của `heroListContainer` nếu không lúc Spawn ra các thẻ sẽ bị méo.

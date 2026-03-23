# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `HeroInfoPanel.cs`
**Loại thành phần:** Full Screen Panel / Màn Hình Thông Tin Chi Tiết Tướng.

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`HeroInfoPanel` là cái rốn vũ trụ của sự phát triển nhân vật. Đây là màn hình phức tạp nhất, nơi người chơi đọc mọi thông số gen, tiểu sử, mặc đồ, xem bộ chiêu thức và quyết định cộng điểm tiềm năng cho Tướng. Panel này yêu cầu không gian rộng lớn, thường cướp toàn bộ màn hình điện thoại (Full screen Popup).

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế Panel này giống như một trang hồ sơ nhân vật RPG cổ điển. Khuyến nghị chia màn hình thành 2 cột (Trái/Phải) hoặc 3 mảng (Trên/Giữa/Dưới).

### 2.1 Cột Trái (Chiêm ngưỡng & Định danh):
*   **Ảnh Chân Dung Lớn (`Image heroAvatarImage`):** Hero Avatar phóng to, sắc nét.
*   **Trích Ngang:** Bao gồm 4 Text Box bám quanh Avatar. 
    *   Tên Tướng (`heroNameText`)
    *   Cấp Độ (`levelText`)
    *   Giới tính (`genderText`)
    *   Nghề/Hệ (`professionText`)
*   **Bộ 6 Khe Cắm Đồ (`HeroEquipmentSlot[] equipmentSlots`):** Yêu cầu vẽ 6 cái bục / Lưới 2x3 ở bên dưới hoặc hai bên mép Avatar để nhét Prefab Slot vào.

### 2.2 Cột Phải - Mảng 1 (Sổ Cầm Tay Trạng Thái Thể Chất):
*   Thiết kế một dải Menu hoặc Khung Gỗ Liệt kê 8 dòng chỉ số chi chít:
    *   `hpText` (Máu), `atkText` (Tấn công), `defText` (Phòng thủ), `spdText` (Tốc độ chạy).
    *   `evasionText` (Né tránh %), `dmgReductionText` (Miễn thương %), `dmgIncreaseText` (Khuếch đại sát thương %).
    *   `potentialText` (Ngưỡng cấp Tiềm năng Tối đa của bộ gen).
*   *Lưu ý Code:* Code sẽ đổ dữ liệu theo format "Gốc + Màu Xanh (Chỉ số đồ cộng thêm)". Nên Font chữ chọn loại rõ ràng.

### 2.3 Cột Phải - Mảng 2 (Kỹ Năng & Định Mệnh):
Khu vực này gồm 2 cái Scroll View hoặc Layout Group nằm dọc (Kéo dài xuống dưới):
*   **Khu Kỹ Năng (`Transform skillsContainer`):** Một lưới chứa các cục Kỹ Năng (Code tự nhét ảnh icon và Text mô tả chiêu vào). Cần thiết kế 1 item rỗng làm mẫu (`infoItemPrefab`) gồm 1 Icon ảnh nhỏ và 2 dòng Text (Tên chiêu, Mô tả chiêu).
*   **Khu Nội Tại/Vận Mệnh (`Transform traitsContainer`):** Giống hệt Khu kỹ năng. Nơi show các loại Gen Đột biến bẩm sinh.

### 2.4 Thanh Công Cụ Đáy Màn Hình (Action Toolbar):
Dàn nút lấp lánh (Có thể thiết kế tắt/mờ đi thay vì ẩn mất nếu muốn đẹp Layout):
*   **Nút Cắn Sách EXP (`Button useExpItemButton`):** Bấm xài sách EXP trong kho lên người (Bị ẩn nếu Max Level hoặc chưa đến tuổi trưởng thành).
*   **Nút Cộng Điểm (`Button statAllocationButton`):** Nút này bật sáng trưng khi Tướng lên cấp và có Điểm Tiềm Năng dư thừa (`freeStatPoints > 0`). Bấm để gọi bảng nâng điểm đổ xí ngầu.
*   **Nút Thức Tỉnh/Đột Phá (`Button traitUpgradeButton`):** Nút cực nguy hiểm, chỉ bật sáng chóe khi Hero chạm mốc LV 60 hoặc LV 100.
*   **Nút Thoát (`Button closeButton`).**

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   **Tránh Cụt Nguồn:** Script có viết tính năng Auto-wire tự móc nút nếu Designer quên kéo nút vào Inspector. Nhưng nên gán bằng tay đàng hoàng để tránh lỗi. Nút Cộng điểm thường nhét chung ở khu vực Chỉ Số. Nút Đột phá nhét ở khu vực Nút Kỹ năng/Traits. Tùy layout.

# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `BreedingUIController.cs`
**Loại thành phần:** Full Screen Panel / Giao Diện Gắn Kết Phòng Ấp (Nhân Giống).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`BreedingUIController` là một trong những tính năng lõi của game, quản lý nhà Nhân giống (BreedingPen). Panel này hoạt động rẽ nhánh qua cơ chế State Machine nội bộ, chia màn hình làm 2 rạp hiển thị: Thể thức **Chọn Lọc (Selection Phase)** và Thể thức **Nhận Hàng (Result Phase)**. 

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế Panel lớn tắt bật 2 khu Container riêng rẽ.

### 2.1 Khu Vực Header & Nút Cố Định:
*   **Tiêu đề Panel (`TextMeshProUGUI panelTitleText`):** Đỉnh màn hình.
*   **Nút Đóng (`Button closeButton`):** Chỉ xuất hiện ở Phase Selection. Ở Phase Result, nút này bị ẩn đi để ép người chơi bấm Xác nhận.
*   **Nút Nâng Cấp Công Trình (`Button upgradeBuildingButton`):** Tùy chọn, đi rẽ nhánh sang Quản lý nhà "BreedingPen".

### 2.2 Trạng Thái 1: Khu Vực Chọn Lọc Sinh Sản (`GameObject selectionArea`):
Màn hình này chia làm 2 ô to tượng trưng cho 2 buồng nuôi cấy.
*   **Slot Vị trí Tướng Cha (`GameObject fatherSlot`):**
    *   `Button selectFatherButton`: Nút bự "Chọn Cha" đính kèm khung viền rỗng.
    *   `HeroCard fatherCard`: Khẩu độ dành cho Prefab HeroCard. Khi người chơi chọn xong, card này đè lên nút ở trên.
*   **Slot Vị trí Tướng Mẹ (`GameObject motherSlot`):** 
    *   Giống hệt Cha. Có `selectMotherButton` và `motherCard`.
*   **Bộ Bệ Phóng & Kích Thích Gen (Action Bar dưới đáy):**
    *   **Toggle Thuốc Đột Biến (`Toggle useMutationPotionToggle`):** Dạng Checkbox vuông. Gắn cạnh icon bình thuốc đỏ. Tác dụng: Dùng bật/tắt quyền xài 1 Item `IT_MUTATION_POTION` giúp sinh con đột biến.
    *   **Nút Quảng Cáo Đột Biến Mẽo (`Button mutationAdButton`):** Nút tùy chọn cho phép xem quảng cáo để đột biến Free (Max 2 lần/ngày). Nhỏ bằng 1/3 nút Breed gốc.
    *   **Nút Tiến Hành Lai Tạo (`Button breedButton`):** Nút Siêu to khổng lồ. Mặc định tô màu Xám (Ngủ đông). Căn code sẽ bắt sáng Lên khi nhét đủ cả Cha và Mẹ vào Slot. Bấm là tốn tiền / Chạy logic.

### 2.3 Trạng Thái 2: Khu Vực Xem Kết Quả (`GameObject resultArea`):
Màn hình này bừng sáng sau khi bấm Breed thành công (Selection Area sẽ bị tắt rụp chừa chỗ cho cái này).
*   **Toả Sáng Trung Tâm:** 
    *   `HeroCard newHeroCard_Result`: Chỗ đứng của Tướng Baby vừa chào đời. Cần vẽ hào quang / tia nắng tỏa ra phía sau lưng lá bài này cho lung linh.
*   **Bảng Dữ Liệu Bẩm Sinh:** Một khung Box chứa 5 dòng text thông số gen quy định sức mạnh của Baby:
    *   `hpText_Result`, `atkText_Result`, `defText_Result`, `spdText_Result` (Tứ Đại Chỉ Số Cơ Bản).
    *   `potentialText_Result` (Ngưỡng tiềm năng giới hạn).
*   **Nút Xác Nhận Thoát (`Button confirmResultButton`):** Nằm dưới cùng. Bấm chữ "Xác Nhận" để giấu luôn Panel về lại Màn hình chính.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Panel này không chứa danh sách tướng. Thay vào đó, nó **mượn** `HeroPickerPanel` để làm một Sub-Popup. Cụ thể: Click "Chọn Cha" -> Chạy popup `HeroPickerPanel` lôi cổ toàn bộ tướng giới tính Nam rảnh rỗi ra -> Chọn -> Đóng Popup -> Update lên Slot. Giai đoạn thiết kế cần chú ý điểm luân chuyển này kẻo bị cấn layer Z.

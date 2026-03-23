# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `QuestItemUI.cs`
**Loại thành phần:** Item Prefab (Thanh chữ nhật Ngang - Chứa Nhiệm Vụ).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`QuestItemUI` là một bảng tóm tắt nhiệm vụ. Nó sẽ được sinh sản vô tính để dải đầy vào trong Bảng Danh Sách Nhiệm Vụ (`QuestPanel`). Nhìn chung, nó hao hao giống một tấm Bằng Khen mỏng nằm ngang.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế một dải băng (Banner) nằm ngang vắt ngang giao diện. Chia làm 2 vế Trái (Nội dung) và Phải (Nút bấm):

### 2.1 Mảng Trái (Nội Dung & Tiến Độ):
*   **Tiêu đề Nhiệm Vụ (`TextMeshProUGUI titleText`):** Text to chà bá. (Vd: "Săn Bắt Đầu Mùa").
*   **Chi Tiết Chỉ Tiêu (`TextMeshProUGUI targetDescriptionText`):** Text nhỏ hơn nằm dưới Tiêu đề. (Vd: "Tiêu diệt 10 con Lợn Dại").
*   **Thanh Tiến Độ (`TextMeshProUGUI progressText`):** Đặt cạnh (Hoặc vắt đè lên) dòng Mô Tả. Format: "5/10". *Khuyến khích:* Designer vẽ thêm một cái Slider/ProgressBar kề dưới khối chữ này để tương lai Coder có thể ốp thanh chạy màu xanh lá cây vào cho trực quan.

### 2.2 Mảng Phải (Quà Cáp & Thao Tác):
Khối này nằm ép vào lề bên phải của tấm thẻ:
*   **Cục Quà Kèm Nhẹ:**
    *   **Ảnh Quà (`Image rewardIcon`):** Icon nhỏ (Tiền, Gems, Mảnh).
    *   **Số lượng (`TextMeshProUGUI rewardAmountText`):** Đặt dưới đuôi hoặc bên cạnh ảnh Quà.
*   **Khu Vực "Nút Chọn Hóa Kiếp":** Nơi này có 2 Component chồng xác lên nhau (Chỉ 1 thằng được hiện ra ở 1 thời điểm):
    *   **Trạng Thái 1 - Nút Chịu Đòi Quà (`Button claimButton`):** Yêu cầu vẽ Nút có chữ "Nhận" hoặc "Hoàn Thành". Nút này bọc màu Vàng Cam lấp lánh (Có thể vẽ 2 trạng thái Chìm/Uninteractable lúc chưa làm xong, và Nổi/Pulse lúc có thể bấm Nhận).
    *   **Trạng Thái 2 - Con Dấu Xác Nhận (`GameObject completedIndicator`):** Yêu cầu vẽ một con Dấu Mộc đỏ chữ "Đã Nhận", hoặc một Icon Checkmark (Dấu chữ V) bọc viền Thép. Bộ phận này CHUYÊN dùng để thế mạng, đè lên chỗ của cái nút Nhận sau khi người chơi đã lấy quà.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Panel này là tĩnh, không có Action Bấm Phóng To. Toàn bộ khu vực vế trái của nó (Chữ nghĩa) là đồ vứt đi không Click được. Component duy nhất phản hồi tương tác là cái Nút Trạng Thái 1 `claimButton`.
*   Tối ưu Draw Call: Đừng táng glow effect đè nén vào đây. Lưới chứa nhiệm vụ sẽ rất dài và cuộn rất chóng mặt. Mọi Asset xin dùng Vector đồ họa phẳng.

# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `TraitUpgradePanel.cs`
**Loại thành phần:** Popup Mờ / Bảng Thăng Cấp Thể Chất.

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`TraitUpgradePanel` là một Popup chuyên sâu được gọi ra từ màn hình Chi Tiết Tướng. Nó phục vụ cho tính năng "Tẩy Tủy / Nâng Cấp Thể Chất". Mọi con Tướng khi sinh ra đều mang theo vài Nội Tại (Traits) ngẫu nhiên (Ví dụ: Trâu Bò Cấp 1). Màn hình này liệt kê đống Nội tại đó ra, đi kèm một nút Nâng Cấp để biến Cấp 1 thành Cấp 2 nếu thỏa điều kiện bí mật.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế một Bảng Vuông/Chữ nhật dựng đứng, lơ lửng giữa màn hình tương tự như Bảng Phân Bổ Tiềm Năng (`StatAllocationPanel`).

### 2.1 Tiêu Điểm / Khung Viền:
*   **Tiêu đề Bảng (`TextMeshProUGUI panelTitleText`):** Đặt ở giữa đỉnh bảng. (Vd: "Đột Phá Thể Chất").
*   **Nút Thoát (`Button closeButton`):** Bấm X. Điểm mù của Dev: Nếu bạn quên kéo thả vào Cột Script, hãy cứ đặt tên Nút X này có chữ `close` hoặc `back`, thuật toán Auto-Wire của Code sẽ tự thắt dây an toàn cho bạn.

### 2.2 Khu Vực Trưng Bày Nội Tại (Main Body):
*   **Vùng Cuộn (`Transform itemsContainer`):** Một miếng Scroll View chiếm không gian chính. Đặt tên GameObject là "Content" hoặc "Container" để thuật toán Auto-Wire tự tìm thấy.
*   **Thẻ Nội Tại (`GameObject traitUpgradeItemPrefab`):** ĐÂY LÀ PHẦN TRỌNG TÂM. Graphic Designer cần thiết kế 1 Thẻ Nằm Ngang (Panel nhỏ) để List nhét vào. Cấu trúc lọt lòng của Thẻ này BẮT BUỘC gồm:
    *   **Text Cấp Độ & Tên Trait:** Một cục `TextMeshProUGUI` to rõ. (Vd: "[Cấp 1] Da Sâu Róm").
    *   **Text Mô Tả Lợi Ích:** Một cục `TextMeshProUGUI` nhỏ hơn ở dưới. (Vd: "Tăng 5% máu"). *Hoặc gộp 2 cái Text này làm 1 nếu bạn lười, Code sẽ tự nội suy ép chữ rớt dòng `<size=80%>`.*
    *   **Nút Phá Thuế (`Button upgradeBtn`):** Phải đặt tên GameObject của Nút này có cụm từ tắt `upg` (Ví dụ: `btn_upg`, `upgrade_button`). Nằm sát mép phải của thanh Thẻ. 

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   **Trạng Thái Về Hưu (MAXED):** Nút Nâng Cấp (`upgradeBtn`) sẽ Bốc Hơi (`SetActive(false)`) hoặc Chết Lặng (`interactable = false`) kèm biểu ngữ "MAXED" khi Nội tại đó đã lên đỉnh. Lời khuyên thiết kế: Hãy làm nút này bốc màu Đỏ rực khi nâng được, và tắt ngúm màu Trắng Đen khi đã Maxed.
*   Bảng này không có nút Xác nhận tổng, người chơi bấm nâng 1 trait bất kỳ là Bảng Tự Đóng Sập lại ngay lập tức (Xài chiêu 1 hit chốt đơn).

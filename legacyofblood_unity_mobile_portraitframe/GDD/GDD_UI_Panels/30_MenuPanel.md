# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `MenuPanel.cs`
**Loại thành phần:** Overlay Popup / Menu Mở Rộng.

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`MenuPanel` là một bảng điều khiển mở rộng (dạng Hamburger Menu hoặc Floating Menu) xuất hiện khi người chơi bấm vào một nút "Menu/Tính năng" ở ngoài màn hình chính. Nó đóng vai trò là Trạm Trung Chuyển (Hub) xả ra một đống lối tắt để bay thẳng vào các hệ thống tính năng khác của game mà giao diện màn hình ngoài không có đủ chỗ để bày.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế này không nên phóng to full màn hình. Thay vào đó, hãy làm một Bảng Phụ (Drawer) trượt từ mép Màn hình phải sang, hoặc rơi từ mép Bàn Cờ Đáy (Bottom Anchor) lên trên. Layout cực kỳ đơn giản:

### 2.1 Mảng Nút Điều Hướng Cốt Lõi:
Yêu cầu thiết kế 6 cái Nút (Buttons) to rõ, xếp thành lưới 2x3 hoặc 1 dọc 6 hàng. Mỗi nút phải có Icon minh họa và Text rõ nghĩa:
*   **Kho Đồ (`Button inventoryButton`):** Icon Balô/Rương Vàng.
*   **Nhiệm Vụ (`Button questButton`):** Icon Cuộn Sách/Bảng Gỗ/Dấu Chấm Hỏi.
*   **Hòm Thư (`Button mailboxButton`):** Icon Bức Thư/Cánh Chim.
*   **Cửa Hàng (`Button shopButton`):** Icon Tiền Vàng/Sạp Hàng/Cái Cân.
*   **Quán Trọ Rút Tướng (`Button recruitmentButton`):** Icon Ly Bia/Cốc Rượu/Bài Tarot (Gacha).
*   **Cài Đặt (`Button settingButton`):** Icon Bánh Răng/Cờ Lê.

### 2.2 Nút Đóng Thoát:
*   **Nút Đóng (`Button closeButton`):** Nút X nằm ngoài viền khối Menu để người dùng tắt bảng phụ này đi. HOẶC Designer có thể cài đặt cho nguyên cái Layout mờ màu đen (Dimmer) phía sau rập khuôn thành cái nút Close này, cứ bấm ra ngoài vùng Menu là Tự tắt.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   **Hành vi Cướp Màn Hình:** Panel này có đặc tính `GameManager.Instance.UIManager.ShowPanel(type, true);` - Nghĩa là ngay khi người chơi bấm vào 1 trong 6 nút lối tắt, Bảng Menu Phụ này tự động "tự sát" (Biến Nhanh / Ẩn Nhanh) để nhường lại sân khấu trọn vẹn cho màn hình vừa được gọi. Tránh hiệu ứng chuyển cảnh lòe loẹt ở Bảng này.

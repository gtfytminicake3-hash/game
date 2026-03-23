# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `ArenaShopPanel.cs`
**Loại thành phần:** Panel / Pop-up Giao diện Cửa hàng con của Arena.

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`ArenaShopPanel` là cửa sổ cho phép người chơi sử dụng điểm tích luỹ từ Đấu Trường (Arena Coins) để đổi lấy các vật phẩm sinh tồn, sách kinh nghiệm, thẻ tăng tốc. Nó hoạt động dưới dạng một danh sách hàng hoá cập nhật mỗi ngày.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Màn hình này có cấu trúc giống một Shop cơ bản, cần các khu vực sau:

### 2.1 Khu Vực Header (Tiêu Đề):
*   Tên Panel: "Cửa Hàng Đấu Trường".
*   Hiển thị **Số Dư Hiện Tại**: Chỗ để text hiển thị quỹ "Xu Đấu Trường / Khuyển" mà người chơi đang sở hữu để họ biết đường mua sắm (Dù trong script hiện chưa map biến này, designer cứ vẽ chừa chỗ).
*   **Nút Đóng (`Button closeButton`):** Dấu X quen thuộc ở góc trên phải.

### 2.2 Khu Vực Quầy Hàng (Scroll View):
*   **Danh Sách Mặt Hàng (`Transform itemContainer`):** Một vùng chữ nhật rộng gắn tính năng kéo trượt dọc (Vertical Scroll Rect) và Grid Layout. 
*   Vùng này sẽ dùng để code Spawn ra các Item Prefab (Đã thiết kế ở phần `ArenaShopItem`). Thiết lập cho container này giãn kích cỡ tự động phù hợp với số lượng item.

### 2.3 Khu Vực Nút Tương Tác Đặc Biệt:
*   **Nút Xem Quảng Cáo Nhận Xu Miễn Phí (`Button adFreebieButton`):** 
    *   Vị trí: Nên đặt tách biệt ở góc dưới cùng màn hình dọc hoặc bo góc nổi bật bên cạnh số dư tiền.
    *   Thiết kế: Nút có hộp quà/icon xu Arena + Icon Video.
    *   Logic hoạt động: Bấm vào xem Ads sẽ được thưởng thẳng Xu. Giới hạn 3 lượt/ngày. Khi hết 3 lượt, nút sẽ biến mất (`SetActive(false)`).

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Mỗi lần mở panel (OnEnable), script sẽ tự động gọi hàm `RefreshShop()` để dọn dẹp hàng cũ và bày hàng mới ra. Hiện tại code đang Hardcode 3 vật phẩm (Bình thụ thai, Sách Exp S, Thẻ tăng tốc). Designer cần đảm bảo kích thước Grid Layout Cell Size rộng đủ lớn để nhét 3 thẻ Prefab vào mà không trễ viền xô lệch.

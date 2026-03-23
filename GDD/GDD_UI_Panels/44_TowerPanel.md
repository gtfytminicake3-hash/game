# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `TowerPanel.cs`
**Loại thành phần:** Full Screen Panel / Màn Hình Leo Tháp Cấu Trúc.

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`TowerPanel` là một Màn Hình Gameplay đồ sộ, mở ra khi người chơi bấm vào Tháp Thử Thách trên Bản đồ thế giới. Nó vẽ ra một cấu trúc thẳng đứng từ Tầng 1 đến Tầng 20 để người chơi thấy rõ tiến độ cày cuốc.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế Màn Hình chia làm 2 khúc: Nửa trên là Trục Tháp khổng lồ cuộn dọc, Nửa dưới là Bảng Điều Khiển (Footer).

### 2.1 Trục Tháp Leo Trèo (Scroll Area):
*   **Vùng Cuộn Tháp (`RectTransform contentParent`):** Một Scroll View lấp đầy 70% màn hình phía trên. Do Tháp có 20 tầng, Code sẽ tự sinh từ Tầng 20 xếp đè trên cùng, rải đều xuống Tầng 1 dưới đáy. Designer cần thiết lập Content Box cuộn từ DƯỚI lên TRÊN.
*   **Thẻ Tầng Tháp (`GameObject floorItemPrefab`):** Design một thẻ hình chữ nhật Nằm ngang dài dẹt, hoặc một Bậc Thang đá. Text trên thẻ chỉ việc ghi "Tầng [X]". 
    *   *Lưu Ý Cho Graphic:* Prefab này dùng một `Image` gốc màu Trắng Tinh. Code sẽ có dính màn "Phục Dựng Màu Sắc Tự Động" (`Color.Lerp`, Đổi màu RGB): Tầng Xám (Đi Qua), Tầng Trắng (Sắp Tới), Tầng Vàng (Đang Đứng). Designer đừng nêm nếm gradient loè loẹt vào Lớp nềm kẻo Code tô màu bị ra màu đục bùn. Cứ mỗi tầng chia 5 (Tầng Boss 5,10,15,20), lớp nền sẽ tự động bị ám Đỏ máu.

### 2.2 Khu Vực Trạm Kiểm Soát (Footer Details):
Một khối bảng neo cứng ở Đáy Màn hình, không bị cuộn. Bao gồm:
*   **Trích Ngang Thông Tin:**
    *   `TextMeshProUGUI currentFloorDetailText`: Ghi "Tầng Hiện Tại: 10".
    *   `TextMeshProUGUI monsterCountText`: Ghi "Só Lượng Quái: 3".
*   **Cảnh Báo Kẹt Đạn (`TextMeshProUGUI difficultyText`):** Biến Text này là Con dao hai lưỡi. Lúc bình thường thì nó ghi chữ "Sẵn Sàng". Nhưng lúc người chơi bị quái tháp đánh cho Vỡ Mồm thì Tháp sẽ khóa cửa (Ví dụ 1 tiếng). Phải nhét vào biến này dòng chữ nhấp nháy: "Đang Bị Phạt Chờ: 00:59:59". Hãy chừa khoảng trống vừa đủ để đếm số.
*   **Hệ Thống Phá Lệnh Cấm (Nút Bấm):**
    *   **Nút Vào Viễn Chinh (`Button enterButton`):** Gắn Text "Vào Trận". (Khi đang bị phạt chờ, nút này sẽ bị Xám `interactable = false`).
    *   **Nút Xem Quảng Cáo (`Button skipCooldownAdButton`):** Thiết kế nút này chồng đè cướp vị trí của Nút Vào Trận, hay nhét chéo kế bên tùy ý. Nó mang Text "Miễn Phí Hủy Chờ" hoặc Icon Xem Ad. Code bảo kê chỉ hiện nút này khi Player đang bị phạt giờ (Cooldown) và chưa xài hết 3 lượt quảng cáo xá tội mỗi ngày.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Panel này là Trạm Cửa Ngõ. Khi người chơi nhấn Vào Trận thành công, nó sẽ cắn Đuôi nối thẳng gọi Màn Hình Bày Quân (`SquadSelectionPanel` - File 41) sập xuống để xin 5 slot Tướng xuất chiến. Cần cài đặt đóng/mở mượt mà.
*   **Bố Cục Rỗng Toác:** Vì các Bậc Thang cứ xếp dọc lên mây, Phần Nền (Background) phía sau ScrollView rất trống trải, Designer nên ốp cảnh Trời Xanh / Mây Đen chạy cuộn nhẹ ở đằng sau Lưới Tầng để tạo cảm giác leo núi.

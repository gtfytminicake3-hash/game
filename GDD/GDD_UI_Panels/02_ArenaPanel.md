# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `ArenaPanel.cs`
**Loại thành phần:** Full Screen Panel / UIPanel chứa tính năng Đấu trường.

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`ArenaPanel` là cửa sổ trung tâm của hệ thống Đấu Trường (PvP). Nơi đây người chơi có thể xem thứ hạng của bản thân, điểm xếp hạng, số lượt đánh giá, đi chợ xếp hạng, hoặc nâng cấp công trình Đấu Trường ảo trong làng. Đây là xương sống trước khi bước vào trận chọn đội hình (Squad Selection).

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Panel này cần vẽ một layout toàn màn hình (hoặc bảng lớn trung tâm), chia làm các khu vực sau:

### 2.1 Khu Vực Sidebar Thể Hiện Trạng Thái Bản Thân (Bên Trái / Giữa màn hình):
Nơi hiển thị profile Đấu Trường của người chơi:
*   **Rank Icon (`Image rankIconImage`):** Hình ảnh minh hoạ danh hiệu (Ví dụ: Huy hiệu Đồng, Bạc, Vàng). Cần có bộ icon Rank từ thấp đến cao.
*   **Tên Bậc Rank (`Text rankNameText`):** Text hiển thị chữ (Ví dụ: Đấu sĩ Vàng III). (Sử dụng Localization key `arena_rank`).
*   **Điểm Hiện Tại (`Text rankPointsText`):** Chuỗi chữ hiển thị (Ví dụ: "Điểm: 1500").
*   **Số Lượt Đánh (`Text ticketsText`):** Hiển thị số vé Arena còn dư (Ví dụ: "Lượt Đấu: 3/5").

### 2.2 Khu Vực Các Nút Bấm Tương Tác Quan Trọng:
*   **Nút Thách Đấu (`Button challengeButton`):** Bắt buộc phải vẽ to, nổi bật. Trạng thái: Có thể xám lại nếu số hero không đủ. Bấm vào nút này sẽ chuyển tiếp sang màn hình `SquadSelectionPanel`. Dùng text `btn_challenge`.
*   **Nút Thêm Lượt Bằng Quảng Cáo (`Button addTicketAdButton`):** Nút nhỏ, thường đặt kế bên Dòng Text "Số Lượt Đánh". Design yêu cầu icon biểu tượng mũi tên/video nhỏ. Nút này sẽ **ẩn đi** mặc định và chỉ hiện lên khi vé về `0` MÀ người chơi chưa xài hết 3 lần xem quảng cáo/ngày.
*   **Nút Bảng Xếp Hạng (`Button leaderboardButton`):** Nút chuyển qua màn hình vinh danh. Thiết kế icon vương miện/cúp điểm. Dùng text `btn_leaderboard`.
*   **Nút Cửa Hàng Đấu Trường (`Button shopButton`):** Nút chuyển qua Shop đổi điểm vinh dự lấy đồ. Thiết kế Icon cửa tiệm. Dùng text `btn_shop`.
*   **Nút Nâng Cấp Công Trình (`Button upgradeBuildingButton`):** Tùy chỉnh (có icon cái búa). Vì game có hệ thống nâng cấp nhà nên nút này sẽ đá người chơi sang bảng Building Upgrade của nhà "Arena".
*   **Nút Đóng Panel (`Button closeButton`):** Dấu `X` kinh điển ở góc trên bên phải.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW)
1. Khi nhấn Thách Đấu: Nếu người chơi không có đủ vé -> Bắn thông báo. Nếu tài khoản không có đủ 5 Hero ở trạng thái "Rảnh rỗi" -> Bắn thông báo lỗi bắt cất tướng đi. Nếu đủ -> Sang màn chọn Đội hình.
2. Xử lý Sau trận đánh: Đánh xong script sẽ thả xuống một cái HUD Notification (Chữ "Chiến Thắng" hoặc "Thất Bại" rớt trên màn hình - Design cần lưu ý có Notification UI).

**Tóm lại:** Designer phải chuẩn bị khung HUD, bộ Icon Rank, Nút to Action (Challenge), và Nút phụ trợ (Ad, Leaderboard, Shop, Upgrade).

# GDD - Các Hệ thống Khác (Quy mô nhỏ)

Tài liệu này tổng hợp các hệ thống và quản lý (Manager) có quy mô nhỏ hơn nhưng đóng vai trò quan trọng trong vòng lặp của game. Dựa trên code hiện tại (`ArenaSystem.cs`, `QuestManager.cs`, `LocalizationSystem.cs`, `AvatarManager.cs`).

---

## 1. Hệ thống Đấu trường (`ArenaSystem.cs`)

Quản lý chế độ chơi Đấu trường (PvP giả lập trực tuyến bằng Bot).
*   **Vé tham dự (Tickets):** Cấp lại lên mức tối đa (5 vé) vào đầu mỗi ngày tính theo giờ hệ thống (UTC).
*   **Tìm đối thủ:** Hiện tại đang sử dụng Bot mô phỏng. Bot được sinh ra ngẫu nhiên với cấp độ và chỉ số thay đổi (`difficultyMultiplier`) tỉ lệ thuận với số Điểm Đấu trường (`arenaPoints`) của người chơi (Tỉ lệ: `1 + points/200`). Cấp độ bot = `(points/50) + 1`.
*   **Kết quả Trận đấu:**
    *   *Thắng:* Nhận khoảng 25 điểm (có điều chỉnh theo chênh lệch) + 30 `ArenaCoin`.
    *   *Thua:* Bị trừ khoảng 20 điểm + 10 `ArenaCoin`.
    *   (Trừ ít nhất 5 điểm).

---

## 2. Hệ thống Nhiệm vụ (`QuestManager.cs`)

Quản lý danh sách nhiệm vụ Hàng ngày, Hàng tuần và Tân thủ.
*   **Khởi tạo:** Kích hoạt sẵn danh sách nhiệm vụ mặc định (`Q_T_01`, `Q_T_02`,... v.v) ở trạng thái `Active`.
*   **Trạng thái Nhiệm vụ (`QuestState`):** 
    1. NotStarted 
    2. Active 
    3. Completed (Có thể nhận thưởng)
    4. Claimed (Đã nhận xong)
*   **Theo dõi Sự kiện (Tracking):** Hiện tại đang triển khai mẫu cho chức năng nghe ngóng `OnBuildingUpgradeCompleted` từ `BuildingSystem`. Nó kiểm tra nhiệm vụ loại `UPGRADE_BUILDING` và đánh dấu `Completed` nếu đúng điều kiện (Level).
*   **Reset Nhiệm vụ:** 
    *   Nhiệm vụ Hàng ngày (`Daily`) sẽ reset tiến độ và trạng thái dựa theo ngày mới (Local Unix Timestamp).
    *   Nhiệm vụ Hàng tuần (`Weekly`) reset tại ngày đầu tuần.
*   **Trả thưởng:** Khi gọi `ClaimReward()`, hệ thống tìm `QuestData`, phân tích chuỗi phần thưởng và thêm tài nguyên trực tiếp qua `InventoryManager`. Tính năng Quest theo chuỗi (chain) cũng sẽ kích hoạt `nextQuestInChain`.

---

## 3. Hệ thống Đa Ngôn ngữ (`LocalizationSystem.cs`)

*   **Tính chất:** Hệ thống tĩnh (Static), không cần tồn tại dưới dạng GameObject.
*   **Hoạt động:** Tải file cấu hình text từ thư mục `Resources/Localization/` dựa trên mã ngôn ngữ (`en` hoặc `vi`).
*   **Phân tích Data:** Đọc file text dạng `Key=Value` và lưu vào `Dictionary`. 
*   **Sự kiện:** Kích hoạt `OnLanguageChanged` toàn cục để tất cả các thành phần UI đang mở có thể tự cập nhật lại text (chưa triển khai rộng).

---

## 4. Quản lý Ảnh Đại diện (`AvatarManager.cs`)

*   **Tính chất:** Singleton lưu trữ pool (kho) ảnh avatar từ Resources. Cung cấp API để gán ảnh đúng cho Hero.
*   **Tính năng chính:**
    *   Tách biệt mảng avatar Nam (`maleAvatars`) và Nữ (`femaleAvatars`).
    *   `GetRandomAvatarIndexByClass`: Lọc qua tên của Sprite xem có chứa keyword Nghề nghiệp (ví dụ: "Warrior") hay không. Nếu có, chọn ngẫu nhiên trong khoảng đó, tạo cảm giác nghề nào thì mặc "đồ" đó. Trả về Index để lưu vào `HeroData`.
    *   Có chế độ fallback an toàn nếu không tìm thấy file thay vì gây lỗi.

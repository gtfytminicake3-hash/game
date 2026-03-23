# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `BossBattlePanel.cs`
**Loại thành phần:** Panel / Pop-up Giao diện Thông tin Thách đấu Trùm (Boss).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`BossBattlePanel` là cửa sổ phân tích tình báo hiện lên khi người chơi bấm vào một Cứ điểm (POI) chứa Trùm Thế Giới hoặc Boss Hầm Ngục trên Bản Đồ (World Map). Màn hình này phô bày toàn bộ chỉ số sát thủ của Boss để người chơi liệu sức mà xếp đội hình.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Panel này không cần cuộn (No ScrollView). Nó là một bảng thông tin tĩnh chia làm 3 khúc rõ rệt trên dưới:

### 2.1 Khúc Header (Đỉnh Bảng):
*   **Tên Boss (`TextMeshProUGUI bossNameText`):** Text to nhất, nằm giữa. (Ví dụ: "HẮC LONG SỨ").
*   **Level Boss (`TextMeshProUGUI bossLevelText`):** Nằm cạnh hoặc dưới tên (Ví dụ: "Lv. 99").

### 2.2 Khúc Thân (Body - Có thể chia làm 2 cột Trái/Phải):
**Cột Trái (Chỉ số & Hình Thể):**
*   **Ảnh Trùm (`Image bossImage`):** Một khung viền gai góc lồng Spine/Avatar quái vật. Đủ to để doạ nạt.
*   **Bảng Tứ Trụ Chỉ Số:** 4 dòng Text nhỏ đặt kề nhau hoặc xếp lưới:
    *   Máu gốc (`hpText`): Hiện "HP: 50000"
    *   Sức tấn công (`atkText`): Hiện "ATK: 500"
    *   Sức phòng thủ (`defText`): Hiện "DEF: 300"
    *   Tốc chạy (`spdText`): Hiện "SPD: 120"
    
**Cột Phải (Bảng Kỹ Năng / Mechanics):**
*   **Bộ Kỹ Năng Thường & AOE:** Khu vực chứa 2 cụm chữ:
    *   `normalSkillOutlineText`: Ghi tên đòn đánh tay và % sát thương.
    *   `aoeSkillOutlineText`: Ghi tên đòn kỹ năng diện rộng và % sát thương.
*   **Khung Cơ Chế Đặc Thù (Mechanic Frame):** Một khung đóng hộp (Panel Box nhỏ) nhấn mạnh nội tại của Boss. Trong hộp có chữ Tiêu Đề (`mechanicNameText` - Vd: "Thánh Giáp Hắc Ám") và Phần mô tả (`mechanicDescText` - Vd: "Giảm 50% sát thương nhận vào từ hệ Lửa..."). Khoảng trống nội dung cần dồi dào vì text có thể dài.

### 2.3 Khúc Footer (Đáy Bảng):
*   **Nút Đóng Kèo (`Button closeButton`):** Góc viền Panel hoặc nút 'Bỏ qua' phía dưới.
*   **Nút Xuất Trận (`Button challengeButton`):** To, nổi bật ở đáy, màu Đỏ/Cam đe doạ. Khi ấn vào, nó sẽ dẫn qua vòng lặp chọn tổ đội.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Panel này yêu cầu đặc biệt so với các kèo đánh quái thường: Code cho phép người chơi gọi tối đa lên tới **15 Heroes** (chia làm 3 Squads / Lớp cắt) để hội đồng con Boss. Nên luồng đi là: Click `Challenge` -> Mở `SquadSelectionPanel` với giới hạn tướng là 15 chứ không phải 5 -> Click Start -> Sang Battle Simulator.

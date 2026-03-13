# GDD - Hệ thống Tiến triển & Trạng thái (Cập nhật theo Code)

Tài liệu này mô tả các hệ thống quản lý vòng đời, trạng thái, và sự tiến triển của anh hùng và người chơi, dựa trên mã nguồn C# hiện tại.

## 1. Hệ thống Trưởng thành (`MaturationSystem.cs`)

*   **Mục đích:** Quản lý quá trình một hero sơ sinh "thức tỉnh" để có thể sử dụng.
*   **Cơ chế Kích hoạt:**
    1.  `BreedingSystem` sau khi tạo ra hero sẽ đặt một mốc thời gian `maturationEndTime` (hiện tại là 1 phút sau khi sinh).
    2.  `MaturationSystem.Tick()` (được gọi liên tục bởi `GameManager`) sẽ kiểm tra và tìm các hero đã vượt qua mốc thời gian này.
    1.  Cờ `isMature` được đặt thành `true`.
    2.  Hero **GIỮ NGUYÊN** nghề nghiệp là `None` (Chưa có nghề).
    3.  Khi Hero đạt **Level 10**, hệ thống `EvolutionSystem` sẽ kích hoạt giao diện mở bảng chọn nghề cho người chơi tự quyết định.
    4.  **Tuyển mộ (Gacha)**: Hero sinh ra từ Gacha sẽ luôn mặc định là **Level 10** và được gán ngẫu nhiên 1 trong 4 Nghề nghiệp có sẵn (không qua bước chọn nghề).
    5.  **Kỹ năng khởi đầu**: Khi chính thức nhận nghề (qua chọn ở Level 10 hoặc quay Gacha), Hero sẽ được gán ngẫu nhiên một kỹ năng khởi đầu của nghề đó.
*   **LƯU Ý:** Trait "Lớn Nhanh" (D_08) được đề cập trong GDD cũ **chưa được triển khai** trong code.

## 2. Hệ thống Lên cấp & Tiến hóa

### 2.1. Lên cấp (`HeroData.cs`)

*   **Kích hoạt:** Phương thức `GainExp(int amount)` được gọi khi hero nhận kinh nghiệm.
*   **Bảng Kinh nghiệm:**
    *   **QUAN TRỌNG:** Game **KHÔNG** sử dụng công thức toán học để tính EXP cần thiết. Thay vào đó, game sử dụng một bảng kinh nghiệm được định nghĩa sẵn trong `Assets/GameData/ExperienceTable.asset`.
    *   `DataManager` sẽ tải và cung cấp dữ liệu từ file này.
*   **Công thức Tăng chỉ số khi Lên cấp (Đã xác thực - Chính xác):**
    *   `Điểm Phân phối = floor(Tiềm năng / 2)`
    *   `HP tăng thêm = floor(Điểm Phân phối * 1.5)`
    *   `ATK tăng thêm = Điểm Phân phối`
    *   `DEF tăng thêm = Điểm Phân phối`

### 2.2. Tiến hóa (`EvolutionSystem.cs`)

*   **Kích hoạt:** Hệ thống lắng nghe sự kiện `HeroData.OnHeroLeveledUp`.
*   **Logic (Chọn Nghề & Thưởng Mốc):** 
    *   **Level 10:** Kích hoạt sự kiện hiển thị bảng Chọn nghề (`ProfessionSelectionPanel`) nếu Hero chưa có nghề (`Profession.None`). Sau khi chọn xong, nhận kỹ năng cơ bản tiên quyết của hệ.
    *   **Level 30, 50, 70, 100:** Hệ thống sẽ trao phần thưởng (Skill hoặc Trait mới). 
    *   **LƯU Ý:** Logic phần thưởng hiện tại đang là **giả lập (placeholder)** trong `EvolutionSystem.cs` và cần được chuyển sang `GameConfig` để quản lý tập trung.

## 3. Hệ thống Trạng thái & Hồi phục (`HospitalSystem.cs`)

Đây là hệ thống quản lý các trạng thái bất lợi của hero.

*   **Bị thương nhẹ:**
    *   **Điều kiện:** Một hệ thống khác (ví dụ: `CombatSystem`) gọi `InflictLightInjury(hero)`.
    *   **Hậu quả:** Hero `isLightlyInjured` và không thể hoạt động trong **5 phút**.
    *   **Phục hồi:**
        1.  Tự động hồi phục khi hết giờ (xử lý trong `HospitalSystem.Tick()`).
        2.  Trả phí Vàng để hồi phục ngay lập tức (`HealLightInjuryInstantly`).
    *   **Chi phí:** `floor(CP / 50) + 10` Vàng.

*   **Bị thương nặng:**
    *   **Điều kiện:** Một hệ thống khác gọi `AdmitForSevereInjury(hero)`.
    *   **Hậu quả:** Hero `isSeverelyInjured` và sẽ **biến mất vĩnh viễn** sau **8 giờ** nếu không được cứu.
    *   **Phục hồi:** **BẮT BUỘC** phải trả phí Vàng để cứu thương (`HealSevereInjury`). Không có tự động hồi phục.
    *   **Chi phí:** `floor(CP / 10) + 50` Vàng.
    *   Nếu hết 8 giờ, `HospitalSystem.Tick()` sẽ xóa hero khỏi `DataManager`.

## 4. Hệ thống Xây dựng (`BuildingSystem.cs`)

*   **Mục đích:** Quản lý việc xây và nâng cấp các công trình.
*   **Cơ chế:**
    1.  Người chơi nhấn nút nâng cấp, UI gọi `StartUpgrade(buildingId)`.
    2.  Hệ thống kiểm tra và trừ tài nguyên (Vàng, Gỗ,...) cần thiết.
    3.  Công trình được đặt trạng thái `isUnderConstruction = true` và một mốc `constructionEndTime` được thiết lập.
    4.  `BuildingSystem.Tick()` sẽ kiểm tra và gọi `CompleteConstruction()` khi tới giờ.
    5.  `CompleteConstruction()` đặt `isUnderConstruction = false` và tăng `level` của công trình lên 1.
*   **LƯU Ý:** Chi phí và thời gian nâng cấp hiện đang là **giả lập (placeholder)** trong code.

## 5. Dữ liệu Tiến trình Người chơi (`PlayerData.cs`)

Đây là file lưu trữ chính cho toàn bộ tiến trình của người chơi, bao gồm:
*   `resources`: Vàng, Gỗ, Đá.
*   `items`: Kho vật phẩm.
*   `BuildingLevels`: Cấp độ của tất cả các công trình.
*   `ActiveExpeditions`: Các đoàn thám hiểm đang hoạt động.
*   `WorldPois`: Trạng thái các địa điểm trên bản đồ thế giới.


nâng cấp nhà cần tính toán làm sao để sau 1 tiếng chơi liên tục nhà lên được cấp 5 10 tiếng liên tục thì là cấp 13 1 tuần liên tục thì full cấp

những vật phầm có thể có trong game/ mỗi 1 hero có giới hạn sinh sản là 10 lần
tăng giới hạn sinh sản +2 tối đa 20 /nvaatj
tăng tốc nói chung
tăng tỉ lệ đột biến ss ( tăng tỉ lệ ra trait tốt hơn )
tăng tốc ss , trưởng thành trong 30 p
thẻ kinh nghiệm rơi khi đánh quái, mua trong shop

Tháp thử thách cho từng nghề , để thu tài nguyên của từng nghề. để healer là 1 nghề 
khiên thịt, khiên ảo , berseker. cung băng, hỏa, cây. băng, hỏa ,heal

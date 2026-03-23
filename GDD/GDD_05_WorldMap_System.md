# GDD - Hệ thống Bản đồ Thế giới (Cập nhật theo Code)

Tài liệu này mô tả chi tiết về các cơ chế và chức năng của màn hình World Map, dựa trên mã nguồn C# hiện tại.

## 1. Kiến trúc Tổng quan

Hệ thống được chia thành hai thành phần chính:

*   **`WorldMapController.cs` (View & Input):** Chịu trách nhiệm hiển thị bản đồ, POI, xe ngựa, và xử lý toàn bộ tương tác của người chơi (pan, zoom, click).
*   **`ExpeditionManager.cs` (Logic & State):** Một hệ thống chạy nền, quản lý trạng thái và vòng đời của tất cả các chuyến thám hiểm đang diễn ra.

## 2. Khởi tạo và Tương tác Bản đồ

### 2.1. Khởi tạo Thế giới (`InitializeWorldMap`)

*   **Tính bền vững (Persistence):** Khi bản đồ được mở, hệ thống sẽ ưu tiên tải danh sách POI đã có từ `DataManager.Instance.Player.WorldPois`.
*   **Tạo mới:** Nếu không có dữ liệu, hệ thống sẽ tạo ra một thế giới mới bằng cách gọi `GenerateAndRegisterNewPOI` nhiều lần.
*   **Logic Tạo POI (`GenerateAndRegisterNewPOI`):
    *   **Chống chồng chéo:** Vị trí của POI mới được chọn ngẫu nhiên và được kiểm tra bằng hàm `IsPositionValid` để đảm bảo nó cách các POI khác một khoảng `minPoiDistance`.
    *   **Tạo quái vật:** Sau khi có vị trí, hàm `GenerateMonstersForPOI` được gọi để tạo một danh sách ID quái vật cho POI đó, dựa trên độ khó.
    *   Dữ liệu POI mới được lưu vào `PlayerData`.

### 2.2. Tương tác Bản đồ (Pan & Zoom)

*   **Pan (Kéo):** Người chơi dùng 1 ngón tay để kéo bản đồ. Vị trí được giới hạn trong một khung (`ClampMapPosition`) để không bị kéo ra ngoài.
*   **Zoom (Phóng to):** Người chơi dùng 2 ngón tay để phóng to/thu nhỏ. Tỷ lệ được giới hạn giữa `minZoom` và `maxZoom`.

## 3. Luồng Tương tác với POI

Luồng xử lý khi người chơi muốn khám phá một địa điểm đã được cải tiến so với GDD cũ.

1.  **Click vào POI:** Người chơi nhấn vào một `GameObject` POI trên bản đồ.
2.  **Hiển thị Panel Thông tin:** `WorldMapController` gọi hàm `poiInfoPanel.Show()`. Panel này hiển thị tên, độ khó, và CP đề nghị của POI.
3.  **Nhấn nút "Khám phá":** Người chơi nhấn nút trên `POI_InfoPanel`.
4.  **Mở Panel Chọn Đội:** `WorldMapController` gọi `squadSelectionPanel.Show()` để người chơi chọn đội hình.
5.  **Xác nhận Đội hình:** Người chơi chọn đủ số hero và nhấn "Xác nhận".
6.  **Bắt đầu Chuyến đi:** `SquadSelectionPanel` gọi một hàm callback, hàm này sẽ ra lệnh cho `ExpeditionManager.StartExpedition()` với đội hình đã chọn và POI làm đích đến.

## 4. Vòng đời Chuyến thám hiểm (`ExpeditionManager`)

`ExpeditionManager.Tick()` là một máy trạng thái (state machine) chạy liên tục để cập nhật các chuyến đi.

| Trạng thái | Mô tả Hoạt động |
| --- | --- |
| **`Traveling`** | Xe ngựa di chuyển từ làng đến POI. Thời gian được tính dựa trên khoảng cách. Khi đến nơi, chuyển sang `Exploring`. |
| **`Exploring`** | **(Mới)** Trạng thái khi đội đã đến POI. **`CombatSystem.Simulate()` được gọi ngay lập tức** để mô phỏng trận đấu. Kết quả được lưu lại. Trạng thái này có một thời gian chờ ngắn (hiện tại là 2 giây) để giả lập việc khám phá, sau đó chuyển sang `Returning`. |
| **`Returning`** | Xe ngựa di chuyển từ POI trở về làng. `OnExpeditionReturning` được phát ra để `WorldMapController` hiển thị lại xe ngựa. |
| **`Finished`** | Khi xe ngựa về đến làng, chuyến đi kết thúc. `FinalizeExpedition()` được gọi để xử lý kết quả (phần thưởng/thương vong) và `OnExpeditionFinished` được phát ra. |

## 5. Tái tạo POI (Respawn)

*   Khi một chuyến đi kết thúc thành công (`FinalizeExpedition`), `ExpeditionManager` sẽ phát ra sự kiện `OnPOICleared`.
*   `WorldMapController` lắng nghe sự kiện này, tìm và xóa `GameObject` của POI đã hoàn thành trên bản đồ, đồng thời xóa dữ liệu của nó khỏi `PlayerData`.
*   Ngay sau đó, `GenerateAndRegisterNewPOI` được gọi để tạo ra một POI mới cùng loại, đảm bảo thế giới luôn có nội dung để người chơi khám phá.

## 6. Hiển thị Trực quan (`WorldMapController`)

`WorldMapController` lắng nghe các sự kiện từ `ExpeditionManager` để cập nhật UI:

*   **`OnExpeditionStarted`**: Tạo một `travelCartPrefab` và dùng DOTween di chuyển nó từ làng đến POI, sau đó ẩn đi.
*   **`OnExpeditionReturning`**: Hiện lại xe ngựa và di chuyển nó từ POI về làng.
*   **`OnExpeditionFinished`**: Xóa `GameObject` xe ngựa khỏi bản đồ.
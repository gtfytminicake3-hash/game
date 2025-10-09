# GDD - Hệ thống Bản đồ Thế giới (World Map)

Tài liệu này mô tả chi tiết về các cơ chế và chức năng đã được triển khai trong `WorldMapController.ts`.

## 1. Mục tiêu

Cung cấp một giao diện bản đồ thế giới tương tác, nơi người chơi có thể khám phá, xem các địa điểm quan trọng (Points of Interest - POI), và cử các đội anh hùng đi làm nhiệm vụ (Expedition).

## 2. Cấu trúc Node chính

*   **`MapContainer`**: Node cha chứa toàn bộ các yếu tố của bản đồ (POI, nền, v.v.). Đây là node được di chuyển và thay đổi tỷ lệ khi người chơi pan/zoom.
*   **`GeneratedPOIsContainer`**: Node con của `MapContainer`, chứa tất cả các POI được tạo ra ngẫu nhiên.
*   **`TravelLayer`**: Một layer riêng biệt để hiển thị các đối tượng di chuyển (xe ngựa) của các chuyến thám hiểm, đảm bảo chúng luôn hiển thị đúng trên bản đồ.

## 3. Chức năng đã triển khai

### 3.1. Tạo POI ngẫu nhiên

*   **Logic:** Hệ thống tự động tạo và đặt một số lượng `Dungeon` và `Rescue` POI lên bản đồ khi màn hình World Map được mở.
*   **Vị trí:** Các POI được đặt ở các tọa độ ngẫu nhiên trong phạm vi `mapSize` đã định nghĩa.
*   **Tương tác:** Mỗi POI được gán dữ liệu (`POIData`) và một sự kiện `TOUCH_END` để xử lý khi người chơi nhấn vào.
*   **Hạn chế:** Logic hiện tại chưa kiểm tra để đảm bảo các POI không bị tạo chồng chéo lên nhau.

### 3.2. Tương tác Bản đồ (Pan & Zoom)

*   **Pan (Kéo bản đồ):** Người chơi có thể dùng một ngón tay để kéo và di chuyển bản đồ.
*   **Zoom (Phóng to/Thu nhỏ):** Người chơi có thể dùng hai ngón tay (chụm/xòe) để thay đổi tỷ lệ của bản đồ.
*   **Giới hạn:**
    *   Tỷ lệ zoom được giới hạn trong khoảng `minZoom` và `maxZoom`.
    *   Vị trí của bản đồ được giới hạn (`clampMapPosition`) để người chơi không thể kéo bản đồ ra khỏi khung nhìn.

### 3.3. Tương tác POI

*   Khi người chơi nhấn vào một POI, hàm `onPOIClicked` được gọi.
*   Hàm này sẽ kích hoạt `SquadSelectionPanel` (Panel chọn đội hình), cho phép người chơi chọn các hero để bắt đầu một chuyến thám hiểm đến POI đó.
*   Sau khi xác nhận đội hình, `ExpeditionManager.instance.startExpedition` được gọi để chính thức bắt đầu nhiệm vụ.

### 3.4. Hiển thị Chuyến đi (Expedition Visualization)

`WorldMapController` lắng nghe các sự kiện từ `ExpeditionManager` để hiển thị trực quan trạng thái của các chuyến đi:

*   **`EXPEDITION_STARTED`**:
    *   Tạo một `travelCartPrefab` (xe ngựa) tại vị trí làng (`villagePosition`).
    *   Sử dụng `tween` để di chuyển xe ngựa từ làng đến vị trí của POI.
    *   Khi đến nơi, xe ngựa sẽ bị ẩn đi.
*   **`EXPEDITION_RETURNING`**:
    *   Hiển thị lại xe ngựa tại vị trí POI.
    *   Sử dụng `tween` để di chuyển xe ngựa từ POI trở về làng.
*   **`EXPEDITION_FINISHED`**:
    *   Khi xe ngựa về đến làng và chuyến đi kết thúc, xe ngựa tương ứng sẽ bị xóa khỏi bản đồ.
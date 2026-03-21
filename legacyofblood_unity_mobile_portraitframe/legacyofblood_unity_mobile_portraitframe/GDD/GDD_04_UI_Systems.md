# GDD - Hệ thống Giao diện Người dùng (UI) (Cập nhật theo Code)

Tài liệu này mô tả kiến trúc, luồng hoạt động và các thành phần chính của hệ thống UI, dựa trên mã nguồn C# hiện tại.

## 1. Kiến trúc Cốt lõi

Hệ thống UI được xây dựng theo mô hình Event-Driven và được quản lý bởi một `UIManager` trung tâm.

### 1.1. `UIManager.cs`

Đây là bộ não của hệ thống UI, chịu trách nhiệm quản lý vòng đời của các panel.

*   **Tự động Đăng ký Panel:** Khi một scene mới được tải (`OnSceneLoaded`), `UIManager` sẽ tự động tìm tất cả các `GameObject` có gắn script `UIPanel` và đăng ký chúng vào một `Dictionary`. Điều này giúp hệ thống linh hoạt, không cần gán prefab thủ công.
*   **Quản lý Lịch sử:** `UIManager` sử dụng một `Stack` để lưu lại lịch sử các panel đã được mở. Chức năng `GoBack()` sẽ lấy panel từ đỉnh stack ra để quay lại màn hình trước đó.
*   **API:** Cung cấp các hàm `ShowPanel(type, hideCurrent)`, `HidePanel(type)`, và `GoBack()`.

### 1.2. `UIPanel.cs`

Một script rất đơn giản, chỉ chứa một biến `public UIPanelType PanelType`. Nó đóng vai trò là một "thẻ đánh dấu" để `UIManager` có thể nhận diện và đăng ký các panel.

### 1.3. Hệ thống Sự kiện (`EventManager` & `GameEvents`)

UI hoạt động chủ yếu dựa trên sự kiện, thay vì các lời gọi hàm trực tiếp. Điều này giúp các thành phần độc lập với nhau.

*   **Ví dụ:** Khi một `HeroCard` được nhấn, nó không trực tiếp gọi `HeroInfoPanel`. Thay vào đó, nó phát ra sự kiện `GameEvents.OnHeroCardClicked` với dữ liệu của hero. `HeroInfoPanel` (đã được kích hoạt bởi `UIManager`) sẽ lắng nghe sự kiện này và tự điền dữ liệu.
*   Các sự kiện quan trọng khác: `OnResourceChanged`, `OnHeroListChanged`, `OnPlayerDataLoaded`.

## 2. Luồng Hoạt động & Điều hướng

1.  **`Bootloader.cs` (Entry Point):** Khi game bắt đầu, `Bootloader` sẽ được chạy. Nó có nhiệm vụ khởi tạo `CoreSystems` (chứa các Manager) và tải `MainScene` một cách bất đồng bộ.
2.  **`MainScene`:** Chứa các công trình của làng và các `UIPanel` chính.
3.  **`BottomNavigationController.cs` (Điều hướng chính):** Đây là thanh điều hướng dưới cùng của màn hình. Các nút trên thanh này gọi trực tiếp các hàm public trong script này (ví dụ: `OpenHospitalPanel()`, `OpenBreedingPanel()`), và các hàm này sẽ ra lệnh cho `UIManager` hiển thị panel tương ứng.

## 3. Các Panel Đa dụng (Smart Panels)

Đây là các panel được thiết kế để có thể tái sử dụng ở nhiều nơi.

*   **`HeroPickerPanel.cs`:**
    *   **Chức năng:** Một popup để chọn **một** hero từ một danh sách cho trước.
    *   **Cách hoạt động:** Được gọi bởi một controller khác (ví dụ: `BreedingUIController`) thông qua hàm `Show(title, heroList)`. Khi người chơi chọn một hero, panel này sẽ phát ra sự kiện tĩnh `public static event Action<HeroData> OnHeroPicked`. Controller đã gọi nó sẽ lắng nghe sự kiện này để nhận kết quả.

*   **`SquadSelectionPanel.cs`:**
    *   **Chức năng:** Một popup để chọn một đội hình với số lượng hero tùy chỉnh.
    *   **Cách hoạt động:** Được gọi bởi một controller khác (ví dụ: `ArenaPanel`) thông qua hàm `Show(title, heroList, squadSize, callback)`. Tham số cuối cùng `callback` là một `Action<List<string>>`. Khi người chơi xác nhận đội hình, panel sẽ gọi hàm callback này và truyền vào danh sách ID của các hero đã chọn.

## 4. Tổng quan các Panel Chức năng

| Tên Panel | Script Điều khiển | Mô tả & Hoạt động |
| --- | --- | --- |
| **Màn hình chính** | `UIMainController.cs` | Hiển thị danh sách tất cả hero, sắp xếp theo CP. Lắng nghe `OnHeroListChanged` để tự làm mới. |
| **Thông tin Hero** | `HeroInfoPanel.cs` | Hiện đè lên màn hình chính. Lắng nghe `OnHeroCardClicked` để nhận dữ liệu và hiển thị chi tiết chỉ số, traits, skills. |
| **Lai tạo** | `BreedingUIController.cs` | Có 2 trạng thái: Chọn lựa và Kết quả. Sử dụng `HeroPickerPanel` để chọn Bố/Mẹ. Gọi `BreedingSystem` và hiển thị hero con. |
| **Bệnh viện** | `HospitalPanel.cs` | Hiển thị 2 danh sách hero bị thương nặng và nhẹ. Sử dụng `InjuredHeroCard` để hiển thị timer và nút chữa trị. Gọi `HospitalSystem`. |
| **Đấu trường** | `ArenaPanel.cs` | Sử dụng `SquadSelectionPanel` để người chơi chọn đội hình 5 người. Sau đó gọi `CombatSystem.Simulate()` để bắt đầu trận đấu. |

## 5. Các Thành phần UI cơ bản

*   **`HeroCard.cs`:** Hiển thị thông tin tóm tắt của một hero. Khi được click, nó yêu cầu `UIManager` mở `HeroInfoPanel` và sau đó phát sự kiện `OnHeroCardClicked`.
*   **`InjuredHeroCard.cs`:** Kế thừa `HeroCard`, thêm vào một đồng hồ đếm ngược và nút "Chữa trị".
*   **`HeroPickerCard.cs`:** Một script phụ trợ được thêm vào `HeroCard` khi nó nằm trong `HeroPickerPanel` để ghi đè hành vi click.
*   **`SquadSlotCard.cs`:** Đại diện cho một ô trong đội hình đang chọn, có thể ở trạng thái trống hoặc đã có hero.
*   **`UIResourceBar.cs`:** Thanh tài nguyên Vàng, Gỗ, Đá. Lắng nghe sự kiện `OnResourceChanged` để tự cập nhật.

## 6. Hệ thống Thông báo Toàn cục

*   **`UINotificationManager.cs`:** Quản lý việc hiển thị các thông báo ngắn (toast). Nó sử dụng một hàng đợi (`Queue`) để đảm bảo các thông báo được hiển thị lần lượt, không bị đè lên nhau.
# Tài Liệu Mô Tả UI và Luồng Hoạt Động

Đây là tài liệu mô tả cấu trúc giao diện người dùng (UI), các thành phần cần tạo, và luồng tương tác chính của game để bạn dễ hình dung khi lắp ráp trong Unity Editor.

## 1. Các Scene Cần Dựng

Dựa trên cấu trúc file, game có 2 scene chính:

1.  **StartupScene (Scene Khởi Động):**
    *   **Mục đích:** Có thể là một scene đơn giản để tải các hệ thống lõi của game.
    *   **Đối tượng cần có:** Một GameObject duy nhất, ví dụ `GameManager`, chứa tất cả các script quản lý hệ thống (trong thư mục `Core`, `Systems`, `GameModes`). GameObject này phải được đánh dấu là `DontDestroyOnLoad` để tồn tại khi chuyển scene.
    *   Scene này sẽ tự động chuyển sang `VillageScene` sau khi tải xong.

2.  **VillageScene (Scene Làng Chính):**
    *   **Mục đích:** Đây là scene chính của game, nơi người chơi tương tác với hầu hết các tính năng.
    *   **Đối tượng cần có:**
        *   Một `Canvas` chính để chứa toàn bộ UI.
        *   Một `UIManager` object để quản lý các panel.
        *   Một `EventSystem` của Unity.
        *   Tất cả các UI Panel được liệt kê ở mục 3.

## 2. Các Prefab Giao Diện Cần Tạo

Đây là những thành phần UI có thể tái sử dụng. Bạn nên tạo chúng trước và lưu thành Prefab.

*   `HeroCard.prefab`
    *   **Mô tả:** Một thẻ (card) hiển thị thông tin tóm tắt của một hero (tên, level, avatar, CP, chỉ số cơ bản).
    *   **Cấu trúc:** Là một `Button` chứa các `TextMeshPro - Text` và một `Image`.
    *   **Script:** `HeroCard.cs`.

*   `InjuredHeroCard.prefab`
    *   **Mô tả:** Tương tự `HeroCard` nhưng dùng trong bệnh viện. Hiển thị thời gian hồi phục và có nút "Heal".
    *   **Cấu trúc:** Giống `HeroCard` nhưng có thêm `TextMeshPro - Text` cho `countdownLabel` và một `Button` cho `healButton`.
    *   **Script:** `InjuredHeroCard.cs`.

*   `HeroCardSlot.prefab`
    *   **Mô tả:** Một ô trống trong giao diện chọn đội hình. Có thể trống hoặc chứa một `HeroCard`.
    *   **Cấu trúc:** Là một `Button` có một `GameObject` con để hiển thị trạng thái trống và một `GameObject` con khác để chứa `HeroCard`.
    *   **Script:** `HeroCardSlot.cs`.

*   `TraitItem.prefab` / `SkillItem.prefab`
    *   **Mô tả:** Một dòng text đơn giản để hiển thị thông tin của một Trait hoặc Skill trong danh sách.
    *   **Cấu trúc:** Một `GameObject` có component `TextMeshPro - Text`.

*   `TravelCart.prefab`
    *   **Mô tả:** Một icon (ví dụ: xe ngựa) để biểu diễn một đoàn viễn chinh đang di chuyển trên bản đồ thế giới.
    *   **Cấu trúc:** Một `Image` đơn giản.

## 3. Các Panel và Modal Chính

Đây là các cửa sổ giao diện chính trong `VillageScene`.

*   **HeroListPanel (Panel Chính / Doanh Trại):**
    *   **Chức năng:** Màn hình chính của game. Hiển thị danh sách các hero người chơi đang sở hữu. Chứa các nút điều hướng chính đến các tính năng khác.
    *   **Script:** `HeroListPanel.cs`.

*   **WorldMapPanel:**
    *   **Chức năng:** Hiển thị bản đồ thế giới, cho phép người chơi pan/zoom, xem các điểm POI và cử đội hình đi viễn chinh.
    *   **Script:** `WorldMapController.cs`.

*   **BreedingPanel:**
    *   **Chức năng:** Giao diện lai tạo, cho phép chọn 2 hero cha mẹ để tạo ra hero mới.
    *   **Script:** `BreedingPanel.cs`.

*   **HospitalPanel:**
    *   **Chức năng:** Hiển thị danh sách các hero bị thương (nặng và nhẹ) và cho phép chữa trị cho họ.
    *   **Script:** `HospitalPanel.cs`.

*   **ArenaPanel:**
    *   **Chức năng:** Giao diện đấu trường, cho phép người chơi thách đấu với các đội khác.
    *   **Script:** `ArenaPanel.cs`.

*   **HeroInfoPanel (Modal):**
    *   **Chức năng:** Một cửa sổ pop-up hiển thị thông tin chi tiết của một hero được chọn.
    *   **Script:** `HeroInfoPanel.cs`.

*   **SquadSelectionPanel (Modal):**
    *   **Chức năng:** Một cửa sổ pop-up để người chơi chọn một đội hình (squad) từ danh sách hero có sẵn.
    *   **Script:** `SquadSelectionPanel.cs`.

*   **HeroPickerPanel (Modal):**
    *   **Chức năng:** Một cửa sổ pop-up đơn giản hơn, chỉ để chọn một hero duy nhất từ danh sách.
    *   **Script:** `HeroPickerPanel.cs`.

## 4. Luồng Tương Tác Chính

Đây là mô tả luồng đi của người dùng trong game.

1.  **Bắt đầu game:**
    *   Game bắt đầu ở `VillageScene`, `UIManager` sẽ hiển thị `HeroListPanel` (màn hình chính).

2.  **Tại Màn Hình Chính (`HeroListPanel`):**
    *   Người chơi thấy danh sách các hero của mình.
    *   **Click vào một `HeroCard`:** `HeroInfoPanel` sẽ hiện ra (dạng modal) để hiển thị thông tin chi tiết của hero đó.
    *   **Click nút "World Map":** `HeroListPanel` bị ẩn, `WorldMapPanel` hiện ra.
    *   **Click nút "Breeding":** `HeroListPanel` bị ẩn, `BreedingPanel` hiện ra.
    *   (Tương tự cho các nút `Hospital`, `Arena`...)

3.  **Luồng Viễn Chinh (World Map):**
    *   Người chơi đang ở `WorldMapPanel`.
    *   Họ kéo hoặc zoom bản đồ để tìm một điểm POI (ví dụ: một cái hang động).
    *   **Click vào một POI:** `SquadSelectionPanel` hiện ra (dạng modal).
    *   Trong `SquadSelectionPanel`, người chơi click vào các `HeroCard` ở danh sách bên dưới để thêm vào các `HeroCardSlot` ở trên.
    *   **Click nút "Confirm":** `SquadSelectionPanel` đóng lại. Dữ liệu đội hình được gửi đi. Một `TravelCart.prefab` xuất hiện tại làng và bắt đầu di chuyển đến POI trên bản đồ.

4.  **Luồng Lai Tạo (Breeding):**
    *   Người chơi đang ở `BreedingPanel`.
    *   **Click nút "Select Parent A":** `HeroPickerPanel` hiện ra.
    *   Người chơi chọn một hero. `HeroPickerPanel` đóng lại. `parentACard` trong `BreedingPanel` được cập nhật.
    *   Lặp lại cho "Select Parent B".
    *   **Click nút "Breed":** Hệ thống lai tạo chạy, một hero con được tạo ra và hiển thị ở khu vực `childCard`.

5.  **Luồng Bệnh Viện (Hospital):**
    *   Người chơi mở `HospitalPanel`.
    *   Panel hiển thị 2 danh sách: hero bị thương nặng và hero bị thương nhẹ, sử dụng `InjuredHeroCard.prefab`.
    *   **Click nút "Heal" trên một `InjuredHeroCard`:** Hero tương ứng được chữa trị, trả phí, và biến mất khỏi danh sách.
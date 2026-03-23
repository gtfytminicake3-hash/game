# Hướng Dẫn Cài Đặt Project Unity - Legend of Blood

Đây là tài liệu hướng dẫn các bước thao tác thủ công trong Unity Editor để kết nối các script đã được dịch và tạo thành một game có thể chạy được.

## Phần 1: Cài Đặt Dữ Liệu Game (ScriptableObjects)

Đây là bước quan trọng nhất để game có dữ liệu để chạy. Bạn cần tạo các asset chứa dữ liệu cho Skills, Traits, Bosses,...

1.  **Mở Project Unity:** Mở thư mục `legacyofblood_unity_mobile_portraitframe` bằng Unity Hub.
2.  **Tạo Thư Mục Dữ Liệu:** Trong cửa sổ Project, tạo một thư mục mới, ví dụ: `Assets/GameData`.
3.  **Tạo Asset Dữ Liệu:** Trong thư mục `GameData`, lần lượt tạo các asset sau bằng cách:
    *   Click chuột phải -> `Create` -> `Legend of Blood` -> (Chọn loại asset).
    *   **Experience Table:** Tạo 1 asset `ExperienceTable`. Chọn asset này, và trong cửa sổ Inspector, nhấn nút **"Generate EXP Table"** để tự động điền dữ liệu.
    *   **Evolution Data:** Tạo 1 asset `EvolutionData`. Dựa vào file `EvolutionData.ts` gốc, điền dữ liệu cho từng `Profession` (Warrior, Archer,...) và các mốc level (30, 50, 70, 100) cùng với ID của Skill/Trait tương ứng.
    *   **Boss Data:** Với **mỗi** con boss trong file `AllBosses.ts`, tạo một asset `Boss Data` mới. Điền đầy đủ thông tin `id`, `name`, `cp`, `stats`, `skills`, `mechanic` cho từng con boss.
    *   **Skill Data:** Với **mỗi** skill trong file `AllSkills.ts`, tạo một asset `Skill Data` mới và điền thông tin.
    *   **Trait Data:** Với **mỗi** trait trong file `AllTraits.ts`, tạo một asset `Trait Data` mới và điền thông tin.

## Phần 2: Cài Đặt GameManager

`GameManager` là đối tượng trung tâm điều khiển toàn bộ game.

1.  **Tạo GameManager Object:** Trong một Scene mới (ví dụ: `MainScene`), tạo một GameObject trống và đặt tên là `GameManager`.
2.  **Gắn (Attach) Scripts:** Kéo tất cả các script từ các thư mục sau vào `GameManager` object:
    *   `Assets/Scripts/Core` (Tất cả các file)
    *   `Assets/Scripts/Systems` (Tất cả các file)
    *   `Assets/Scripts/GameModes` (Tất cả các file)
3.  **Liên Kết Dữ Liệu cho DataManager:**
    *   Chọn `GameManager` object.
    *   Trong Inspector, tìm component `DataManager`.
    *   Kéo asset `ExperienceTable` bạn đã tạo vào ô `Exp Table DB`.
    *   Kéo asset `EvolutionData` vào ô `Evolution DB`.
    *   Khóa Inspector (biểu tượng ổ khóa ở góc trên bên phải).
    *   Vào thư mục `GameData`, chọn tất cả các asset `Boss Data`, `Skill Data`, `Trait Data` và kéo chúng vào các ô tương ứng (`Boss DB`, `Skill DB`, `Trait DB`) trong `DataManager`.

## Phần 3: Xây Dựng UI (Giao Diện Người Dùng)

Bạn cần phải dựng lại giao diện từ đầu. Dưới đây là hướng dẫn cho các thành phần chính.

### 3.1. Tạo Prefab cho các Component Tái Sử Dụng

*   **HeroCard Prefab:**
    1.  Tạo một UI > Button. Thêm `CanvasGroup` và script `HeroCard`.
    2.  Bên trong Button, tạo các `TextMeshPro - Text` cho `nameLabel`, `levelLabel`, `statsLabel`, `cpLabel` và một `UI > Image` cho `avatarSprite`.
    3.  Kéo các component Text/Image này vào các ô tương ứng trên script `HeroCard` trong Inspector.
    4.  Lưu lại thành một Prefab, ví dụ: `HeroCard.prefab`.
*   **InjuredHeroCard Prefab:** Tương tự `HeroCard`, nhưng gắn script `InjuredHeroCard` và có thêm `countdownLabel` và một `Button` cho `healButton`.
*   **HeroCardSlot Prefab:** Tạo một UI > Button, gắn script `HeroCardSlot`. Bên trong có một `GameObject` trống (`emptyNode`) và một `GameObject` khác (`heroCardContainer`) để chứa `HeroCard` khi được thêm vào.

### 3.2. Dựng Các Panel Chính

1.  **Tạo Canvas:** Tạo một `UI > Canvas` để chứa tất cả các panel.
2.  **Tạo UIManager Object:** Tạo một GameObject trống tên là `UIManager`, gắn script `UIManager` vào.
3.  **Dựng HeroListPanel (MainMenu):**
    *   Tạo một `UI > Panel`, đặt tên là `HeroListPanel`, gắn script `HeroListPanel`.
    *   Bên trong, tạo một `UI > Scroll View`. Kéo `Content` của Scroll View vào ô `Content Node` của script `HeroListPanel`.
    *   Kéo `HeroCard.prefab` vào ô `Hero Card Prefab`.
    *   Tạo các button (`WorldMap`, `Breeding`, `Hospital`,...) và kết nối sự kiện `OnClick()` của chúng tới các hàm tương ứng trong script `HeroListPanel` (ví dụ: `OnWorldMapClicked()`).
4.  **Dựng Các Panel Khác:** Lặp lại quy trình tương tự cho các panel khác (`BreedingPanel`, `HospitalPanel`, `WorldMapController`, `SquadSelectionPanel`,...):
    *   Tạo Panel, gắn script tương ứng.
    *   Tạo các thành phần con (Button, Text, ScrollView,...).
    *   Kéo các thành phần con và các prefab cần thiết vào các ô `[SerializeField]` trên script trong Inspector.
5.  **Hoàn Thiện UIManager:**
    *   Chọn `UIManager` object.
    *   Trong Inspector, kéo tất cả các Panel object bạn vừa tạo vào danh sách `Panels`.

## Phần 4: Chạy Thử và Gỡ Lỗi (Debugging)

Sau khi đã liên kết tất cả, hãy nhấn nút Play.

*   **Lỗi "NullReferenceException":** Đây là lỗi phổ biến nhất. Nó có nghĩa là bạn đã quên kéo một đối tượng nào đó vào một ô `[SerializeField]` trong Inspector. Hãy đọc kỹ thông báo lỗi trong Console để biết script nào và dòng nào bị lỗi để kiểm tra lại.
*   **UI không hoạt động:** Kiểm tra xem các Button đã được kết nối `OnClick()` đúng với hàm trong script chưa.
*   **Dữ liệu không hiển thị:** Kiểm tra xem các asset trong thư mục `GameData` đã được điền đủ thông tin và đã được kéo vào `DataManager` chưa.

Quá trình này đòi hỏi sự tỉ mỉ, nhưng sau khi hoàn thành, bạn sẽ có một nền tảng vững chắc cho game trên Unity.
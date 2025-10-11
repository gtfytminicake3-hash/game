# GDD - Hệ thống Túi đồ & Vật phẩm (Cập nhật theo Code)

Tài liệu này mô tả kiến trúc của hệ thống túi đồ và trạng thái triển khai của các vật phẩm so với thiết kế ban đầu.

## 1. Kiến trúc Hệ thống (`InventoryManager.cs`)

**Trạng thái:** ĐÃ HOÀN THÀNH VÀ TRIỂN KHAI ĐẦY ĐỦ.

*   **Mục đích:** Là một Manager Singleton, chịu trách nhiệm quản lý tất cả tài nguyên và vật phẩm của người chơi.
*   **Dữ liệu Quản lý:**
    *   **Tài nguyên (`ResourceType` enum):** `Gold`, `Wood`, `Stone`. Được lưu trong đối tượng `PlayerResources` của `PlayerData`.
    *   **Vật phẩm (`Items`):** Một `Dictionary<string, int>` trong `PlayerData`, lưu trữ `ItemID` và số lượng.
*   **Phương thức Chính (Đã triển khai):**
    *   `AddResource(type, amount)`
    *   `SpendResource(type, amount)` (có kiểm tra và trả về `bool`)
    *   `AddItem(itemId, amount)`
    *   `UseItem(itemId, amount)` (có kiểm tra và trả về `bool`)
    *   `GetResourceAmount(type)`
    *   `GetItemCount(itemId)`
*   **Sự kiện (Đã triển khai):**
    *   `public static event Action<ResourceType, int> OnResourceChanged;`
    *   `public static event Action<string, int> OnItemChanged;`

## 2. Cấu trúc Dữ liệu Vật phẩm (`ItemData.cs`)

**Trạng thái:** ĐÃ HOÀN THÀNH VÀ TRIỂN KHAI ĐẦY ĐỦ.

*   Mỗi loại vật phẩm trong game được định nghĩa là một `ScriptableObject` kế thừa từ `ItemData`.
*   **Các thuộc tính chính:**
    *   `id`: ID duy nhất của vật phẩm (ví dụ: `ITEM_MUTATION_POTION`).
    *   `itemName`, `description`, `icon`: Thông tin hiển thị trên UI.
    *   `type` (`ItemType` enum): Phân loại vật phẩm (`Consumable`, `SpeedUp`, `BreedingMaterial`).
    *   `isStackable`, `maxStackSize`: Quy định về việc cộng dồn.
    *   `speedUpValueInSeconds`: Dành riêng cho vật phẩm tăng tốc, chứa số giây được giảm.

## 3. Tình trạng Triển khai Chức năng

Phần này làm rõ những gì đã hoạt động và những gì vẫn còn trên giấy tờ.

### 3.1. Các Chức năng ĐÃ HOÀN THÀNH

*   **Quản lý Tài nguyên:** Các hệ thống khác đã có thể cộng/trừ tài nguyên. Ví dụ:
    *   `HospitalSystem` gọi `SpendResource` để trả phí chữa trị.
    *   `BuildingSystem` gọi `SpendResource` để trả phí nâng cấp (logic giả lập).
*   **Lưu trữ Vật phẩm:** `InventoryManager` có thể thêm, bớt, đếm số lượng vật phẩm trong `PlayerData` một cách chính xác.
*   **Hiển thị Tài nguyên:** `UIResourceBar.cs` đã được triển khai, lắng nghe sự kiện `OnResourceChanged` và cập nhật số lượng Vàng, Gỗ, Đá trên UI một cách chính xác.

### 3.2. Các Chức năng ĐƯỢC THIẾT KẾ nhưng CHƯA TRIỂN KHAI

Đây là các tính năng đã được mô tả trong GDD nhưng **chưa có code logic** trong các hệ thống liên quan.

*   **HIỆU ỨNG VẬT PHẨM - TĂNG TỐC:**
    *   **Thiết kế:** Người chơi có thể dùng vật phẩm để giảm thời gian chờ trong các hệ thống `Maturation`, `Hospital`, `Building`.
    *   **Thực trạng:** **CHƯA TRIỂN KHAI.** Không có code nào trong các hệ thống trên gọi `InventoryManager.UseItem()` hoặc xử lý logic giảm thời gian.

*   **HIỆU ỨNG VẬT PHẨM - THUỐC BIẾN DỊ:**
    *   **Thiết kế:** Người chơi dùng trong màn hình Lai tạo để tăng tỉ lệ đột biến.
    *   **Thực trạng:** **TRIỂN KHAI MỘT NỬA.**
        *   **Đã có:** `BreedingSystem` có `BreedingOptions` với cờ `UseMutationPotion` để thay đổi tỉ lệ đột biến.
        *   **Còn thiếu:** `BreedingUIController` **không có** bất kỳ UI hay logic nào cho phép người chơi chọn và áp dụng vật phẩm này.

*   **HIỆU ỨNG VẬT PHẨM - BÙA ƯỚC NGUYỆN:**
    *   **Thiết kế:** Dùng khi lai tạo để tăng tỉ lệ con cái ra một nghề nghiệp nhất định.
    *   **Thực trạng:** **CHƯA TRIỂN KHAI.** Không có logic nào liên quan đến vật phẩm này trong `BreedingSystem` hay `MaturationSystem`.

*   **GIAO DIỆN - TÚI ĐỒ (`InventoryPanel`):**
    *   **Thiết kế:** Một màn hình để người chơi xem và quản lý các vật phẩm tiêu thụ.
    *   **Thực trạng:** **CHƯA TRIỂN KHAI.** Chưa có file `InventoryPanel.cs` hay Prefab tương ứng.

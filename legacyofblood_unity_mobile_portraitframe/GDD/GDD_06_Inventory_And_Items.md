# GDD - Hệ thống Túi đồ & Vật phẩm

Tài liệu này mô tả kiến trúc của Túi đồ người chơi và các loại vật phẩm mà nó có thể chứa.

## 1. Kiến trúc Hệ thống (InventoryManager.ts)

**Mục đích:** Là một hệ thống trung tâm, thường trú (Singleton, tương tự DataManager), chịu trách nhiệm quản lý tất cả các tài nguyên và vật phẩm mà người chơi sở hữu.

**Dữ liệu Quản lý:**
*   **Tài nguyên (Currencies):** Một đối tượng lưu trữ số lượng các loại tài nguyên chính. Ví dụ: `{ gold: 1000, wood: 500, stone: 300 }`.
*   **Vật phẩm (Items):** Một danh sách hoặc đối tượng lưu trữ các vật phẩm tiêu thụ. Ví dụ: `{ "ITEM_SPEEDUP_1H": 5, "ITEM_MUTATION_POTION": 2 }`.

**Phương thức Chính:**
*   `addResource(type, amount)`: Cộng tài nguyên.
*   `spendResource(type, amount)`: Trừ tài nguyên (kiểm tra xem có đủ không).
*   `addItem(itemId, amount)`: Thêm vật phẩm vào túi đồ.
*   `useItem(itemId, amount)`: Sử dụng (tiêu thụ) vật phẩm.
*   `getItemCount(itemId)`: Kiểm tra số lượng vật phẩm đang có.

**Sự kiện (Events):** InventoryManager sẽ phát ra các sự kiện toàn cục để các hệ thống UI khác lắng nghe:
*   `resource-changed(type, newValue)`: Khi một loại tài nguyên thay đổi.
*   `item-changed(itemId, newCount)`: Khi số lượng một vật phẩm thay đổi.

## 2. Các Loại Vật phẩm & Tích hợp

Dưới đây là thiết kế chi tiết cho các vật phẩm cốt lõi mà chúng ta đã thảo luận, và cách chúng tương tác với các hệ thống hiện có.

### 2.1. Tài nguyên (Currencies)

*   **Vàng:**
    *   **Nguồn kiếm:** Thám hiểm Dungeon, phần thưởng Arena.
    *   **Sử dụng:** Chữa trị trong Bệnh viện (HospitalSystem), Nâng cấp công trình (BuildingSystem).
*   **Gỗ, Đá:**
    *   **Nguồn kiếm:** Thám hiểm Dungeon.
    *   **Sử dụng:** Nâng cấp công trình (BuildingSystem).

### 2.2. Vật phẩm Tăng tốc (Speed-ups)

*   **ID:** `ITEM_SPEEDUP_1MIN`, `ITEM_SPEEDUP_1H`, v.v.
*   **Mô tả:** Các vật phẩm dùng để giảm hoặc hoàn thành ngay lập tức thời gian chờ.
*   **Tích hợp:**
    *   **Trưởng thành (`MaturationSystem`):** Giao diện của hero đang trưởng thành sẽ có một nút "Tăng tốc". Khi nhấn, nó sẽ kiểm tra `InventoryManager` xem người chơi có vật phẩm tăng tốc không. Nếu có, nó sẽ gọi `InventoryManager.useItem()` và `MaturationSystem.reduceTime()`.
    *   **Bệnh viện (`HospitalSystem`):** Tương tự, nút "Tăng tốc" trên thẻ hero bị thương.
    *   **Xây dựng (`BuildingSystem`):** Nút "Tăng tốc" trên công trình đang xây.

### 2.3. Vật phẩm Lai tạo (Breeding Items)

*   **ID:** `ITEM_MUTATION_POTION`
*   **Tên:** Thuốc Biến Dị.
*   **Mô tả:** "Sử dụng trong quá trình lai tạo để tăng mạnh tỉ lệ xảy ra Đột biến Chỉ số."
*   **Tích hợp (`BreedingSystem` & `BreedingUIController`):**
    *   Trong màn hình Lai tạo (`BreedingUIController`), thêm một ô trống "Vật phẩm Hỗ trợ".
    *   Khi người chơi nhấn vào ô này, một panel sẽ hiện ra, hiển thị các vật phẩm lai tạo họ có trong `InventoryManager`.
    *   Nếu người chơi chọn "Thuốc Biến Dị", `BreedingUIController` sẽ truyền một option đặc biệt vào hàm `BreedingSystem.breed(heroA, heroB, { useMutationPotion: true })`.
    *   Trong `BreedingSystem`, logic di truyền chỉ số sẽ kiểm tra option này và tạm thời tăng tỉ lệ Đột biến từ 10% lên, ví dụ, 50% cho lần lai tạo đó.

*   **ID:** `ITEM_CLASS_CHARM_WARRIOR`, `ITEM_CLASS_CHARM_ARCHER`, v.v.
*   **Tên:** Bùa Ước nguyện (Chiến sĩ), Bùa Ước nguyện (Cung thủ).
*   **Mô tả:** "Sử dụng trong quá trình lai tạo. Đứa con sinh ra sẽ có tỉ lệ thức tỉnh thành nghề [Tên nghề] cao hơn."
*   **Tích hợp (`BreedingSystem` & `MaturationSystem`):**
    *   Tương tự như Thuốc Biến Dị, người chơi chọn Bùa trong màn hình Lai tạo.
    *   `BreedingSystem` sẽ lưu lại `classCharmUsed` vào `HeroData` của đứa con.
    *   Khi `MaturationSystem` thực hiện quá trình Thức tỉnh, nó sẽ kiểm tra thuộc tính `classCharmUsed` này và điều chỉnh tỉ lệ ra nghề tương ứng.

## 3. Giao diện Người dùng (GDD_04_UI_Systems.md)

Cần thêm:
*   **`UIResourceBar`:** Một thanh hiển thị Vàng, Gỗ, Đá ở cạnh trên màn hình chính (`UIMainController`). Thanh này sẽ lắng nghe sự kiện `resource-changed` từ `InventoryManager` để tự cập nhật.
*   **`InventoryPanel`:** Một màn hình Túi đồ hoàn chỉnh, có thể truy cập từ màn hình chính. Panel này sẽ hiển thị tất cả các vật phẩm tiêu thụ mà người chơi có, cho phép họ xem mô tả và số lượng.
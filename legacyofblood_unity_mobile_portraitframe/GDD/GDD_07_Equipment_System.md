# GDD - Hệ thống Trang bị (Equipment System)

Tài liệu này mô tả chi tiết hệ thống trang bị được triển khai trong `EquipmentSystem.cs`.

## 1. Cấu trúc Trang bị (`EquipmentData`)

Trang bị trong game có các đặc tính sau:
*   **Loại (Slot):** Vũ khí (`Weapon`) hoặc Giáp (`Armor`).
*   **Cấp độ (Level):** Từ 1 đến giới hạn tối đa `MAX_LEVEL` (100).
*   **Độ hiếm (Rarity):** Từ 1 đến 3. Ảnh hưởng trực tiếp đến sức mạnh cơ bản.
*   **Chỉ số Cơ bản (Base Stats):** Tăng trưởng theo Level.
    *   *Vũ khí:* Cung cấp `ATK`.
    *   *Giáp:* Cung cấp `HP` và `DEF`.
*   **Chỉ số Thêm (Bonus Stats):** Mỗi trang bị có 2 dòng chỉ số ngẫu nhiên.

## 2. Sinh Trang bị Ngẫu nhiên (`GenerateRandomEquipment`)

Trang bị được sinh ra như một phần thưởng rơi ra từ tháp trong lúc thám hiểm.
*   **Cấp độ rơi (Drop Level):** Dựa vào độ khó của tầng tháp (`floorDifficulty`). Nằm trong khoảng `[floorDifficulty - 5]` đến `[floorDifficulty + 2]` (giới hạn min 1, max 40).
*   **Tỉ lệ Loại:** 50% Vũ khí ("Vũ Khí Cổ Đại"), 50% Giáp ("Giáp Cổ Đại").
*   **Công thức Chỉ số Cơ bản:**
    *   Hệ số Level (`levelMultiplier`) = `1 + (level * 0.1 * rarity)`.
    *   `ATK` vũ khí = `10 * levelMultiplier`.
    *   `HP` giáp = `50 * levelMultiplier`.
    *   `DEF` giáp = `5 * levelMultiplier`.

## 3. Chỉ số Thêm (Bonus Stats)

Khi sinh trang bị, hệ thống tự động quay ngẫu nhiên 2 dòng chỉ số phụ (có thể trùng loại):
1.  **+% ATK:** `5% - 15%`
2.  **+% HP:** `5% - 15%`
3.  **Tỉ lệ Chí Mạng:** `2% - 8%`
4.  **Tốc độ (Cộng thẳng):** `2 - 10`

## 4. Hệ thống Cường hóa (Nâng cấp Level)

Trang bị có thể được cường hóa bằng cách vứt các trang bị khác làm "vật liệu" (food) để tăng Cấp độ. Cấp độ tối đa là 100.

### 4.1. Kinh nghiệm từ Vật liệu (`GetExpYield`)

Số EXP thu được khi "ăn" một trang bị vật liệu:
*   `Cơ bản (5) + (Level vật liệu * 2) + (EXP hiện có của vật liệu / 2)`

### 4.2. Đường cong Kinh nghiệm Lên cấp (`GetExpRequiredForLevel`)

Hệ thống sử dụng cơ chế _Soft Cap_ (Giới hạn mềm) ở cấp 40 để làm chậm tiến độ ở giai đoạn lategame:
*   **Từ Cấp 1 đến 39:** Tăng trưởng tuyến tính. EXP cần = `Level * 10`. (Ví dụ: Level 10 cần 100 EXP).
*   **Từ Cấp 40 trở đi:** Tăng trưởng hàm mũ siêu khó. EXP cần = `500 * (1.2 ^ Số cấp vượt 40)`.

Khi cường hóa vượt đủ EXP cần thiết, trang bị sẽ thăng cấp, tự động tính lại `Base Stats` dựa trên Level mới.

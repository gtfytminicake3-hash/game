# GDD - Hệ thống Chiến đấu

Tài liệu này mô tả chi tiết về các cơ chế chiến đấu đã được triển khai trong `CombatSystem.ts`.

## 1. Trận đấu Thường (Normal Battle)

*   **Đối tượng:** Sử dụng lớp `Combatant` để gói `HeroData` và các chỉ số chiến đấu.
*   **Lượt đi:** Các `Combatant` được sắp xếp theo chỉ số `SPD` giảm dần để quyết định thứ tự hành động trong mỗi lượt.
*   **Mục tiêu:** Tấn công một mục tiêu ngẫu nhiên còn sống của phe địch.
*   **Công thức Sát thương Cơ bản:**
    *   `Sàn Sát thương = floor(ATK_TấnCông * 0.1)`
    *   `Sát thương = max(Sàn Sát thương, ATK_TấnCông - DEF_MụcTiêu)`

### 1.1. Các Trait Đặc biệt trong Chiến đấu

*   **AURA (Áp dụng đầu trận):**
    *   `Lãnh Đạo (A_05)`: Tăng 5% ATK và DEF cho các đồng đội cùng hàng.
    *   `Hộ Vệ Hoàng Gia (SS_08)`: Tạo giáp bằng 15% HP tối đa cho toàn đội (hiện chỉ ghi log).
*   **SPECIAL (Kích hoạt theo sự kiện):**
    *   `Tái Sinh (S_01)`: Khi chết, có 50% cơ hội hồi sinh với 25% HP.
    *   `Kẻ Săn Mồi (SS_06)`: Sau khi hạ gục một kẻ địch, tăng 20% ATK.

## 2. Trận đấu Boss (Boss Battle)

*   **Đối tượng:**
    *   Người chơi: 3 `Squad` (Tiên phong, Cốt lõi, Hậu cần). Mỗi `Squad` là một thực thể duy nhất với tổng chỉ số của các hero thành viên.
    *   Boss: 1 `BossCombatant`.
*   **Lượt đi của Boss:**
    *   **Lượt lẻ:** Tấn công đơn mục tiêu vào Biệt đội Tiên phong.
    *   **Lượt chẵn:** Tấn công diện rộng (AOE) lên tất cả các Biệt đội.
*   **Công thức Sát thương Boss:**
    *   `Sát thương = (ATK_Boss * Hệ_số_kỹ_năng) - DEF_Biệt_đội`
*   **Cơ chế Boss Đặc biệt đã triển khai:**
    *   **Da Dày (BOSS_1):** Miễn nhiễm sát thương từ Biệt đội có tổng ATK < 500.

## 3. Hậu quả sau Trận đấu

*   **Bị thương nhẹ:** Các hero phe người chơi còn sống nhưng mất máu sẽ bị `inflictLightInjury`.
*   **Bị thương nặng:** Các hero phe người chơi có HP <= 0 sẽ được `admitHero` vào bệnh viện.

---

### Lỗ hổng & Cơ hội Phát triển

*   Hệ thống kỹ năng chưa được tích hợp vào vòng lặp chiến đấu. Các hero hiện chỉ có thể tấn công cơ bản.
*   Nhiều hiệu ứng Trait (ví dụ: `Xuyên Giáp`, `Vết Thương Sâu`, `Bình Thản`) đã có trong dữ liệu nhưng chưa có logic xử lý trong `CombatSystem`.
*   Logic tạo giáp của `Hộ Vệ Hoàng Gia` mới chỉ ghi log, chưa có hiệu ứng thực tế.
*   Chưa có hệ thống tài nguyên (mana/năng lượng) cho việc sử dụng kỹ năng.
# GDD - Hệ thống Chiến đấu (Cập nhật theo Code)

**CẢNH BÁO:** Tài liệu GDD cũ đã hoàn toàn lỗi thời. Đây là mô tả chính xác về hệ thống chiến đấu theo lượt được triển khai trong `CombatSystem.cs`.

## 1. Các Khái niệm Cốt lõi

*   **`Combatant`**: Một lớp "wrapper" bao quanh `HeroData` cho mục đích chiến đấu. Nó chứa các chỉ số chiến đấu thực tế (đã áp dụng hiệu ứng), HP hiện tại, các kỹ năng được chọn, cooldown, hiệu ứng trạng thái đang có, và vị trí trong đội hình.

*   **`CombatResult`**: Một lớp dữ liệu chứa toàn bộ kết quả của trận đấu, bao gồm: phe thắng, phe thua, danh sách hero sống sót/hy sinh của mỗi bên, và một nhật ký chi tiết toàn bộ diễn biến trận đấu (`CombatLog`).

*   **Đội hình (`RowPosition`)**: Các hero được tự động xếp vào 3 hàng: `Front` (Warrior), `Back` (Healer), và `Middle` (các nghề còn lại). Vị trí này ảnh hưởng đến việc chọn mục tiêu.

*   **Hành động (`CombatAction`)**: Đại diện cho hành động của một `Combatant` trong lượt, có thể là dùng `Skill` hoặc tấn công cơ bản (`IsBasicAttack`).

## 2. Luồng Diễn biến Trận đấu (`RunSimulation`)

Trận đấu là một vòng lặp theo lượt, tối đa 50 lượt để tránh kéo dài vô tận.

1.  **Khởi tạo:**
    *   Tạo ra các đối tượng `Combatant` cho cả hai đội.
    *   Mỗi `Combatant` được gán kỹ năng ngẫu nhiên từ danh sách skill của nghề đó (`AssignSkills`). Số lượng: 1 skill nếu cấp < 40, 2 skill nếu cấp >= 40.
    *   Sắp xếp đội hình (trước, giữa, sau).

2.  **Bắt đầu Vòng lặp (Turn Loop):**
    *   **Quyết định Thứ tự Lượt:** Tất cả các `Combatant` còn sống của cả hai phe được sắp xếp theo chỉ số `SPD` giảm dần.
    *   **Lần lượt Hành động:** Từng `Combatant` trong danh sách thứ tự sẽ thực hiện hành động của mình.

3.  **Trong Lượt của một `Combatant`:**
    *   **Hiệu ứng Đầu lượt:** Kích hoạt các hiệu ứng như Độc (gây sát thương) hoặc Hồi máu theo thời gian (`ProcessStartOfTurnEffects`).
    *   **Giảm Cooldown:** Giảm 1 lượt cho tất cả các kỹ năng đang trong thời gian hồi (`TickCooldowns`).
    *   **Quyết định Hành động (`DecideAction`):** Đây là "AI" của hero:
        *   Nếu là `Healer` và có đồng đội dưới 60% HP, ưu tiên dùng kỹ năng hồi máu.
        *   Nếu có kỹ năng khả dụng (cooldown = 0), chọn ngẫu nhiên một trong số đó để sử dụng.
        *   Nếu không, thực hiện đòn tấn công cơ bản.
    *   **Thực thi Hành động (`ExecuteAction`):**
        *   Tìm mục tiêu dựa trên `TargetingType` của kỹ năng (xem mục 4).
        *   Tính toán sát thương/hồi máu và áp dụng lên mục tiêu.
        *   Áp dụng hiệu ứng trạng thái nếu có.
        *   Đặt lại cooldown cho kỹ năng vừa sử dụng.

4.  **Kết thúc:** Vòng lặp dừng khi một trong hai đội không còn ai sống sót, hoặc sau 50 lượt. `CombatResult` được tạo ra và trả về.

## 3. Các Công thức Tính toán

*   **Công thức Sát thương (`PerformAttack`):**
    *   `Sát thương cơ bản = ATK_TấnCông * PowerRatio_KỹNăng` (PowerRatio = 1.0 cho đòn đánh thường).
    *   `Sát thương cuối = floor(max(1, Sát thương cơ bản - DEF_MụcTiêu))`.

*   **Chí mạng (Critical Hit):**
    *   Một cơ chế **đã được triển khai**. Tỉ lệ chí mạng được tính từ chỉ số của hero cộng với hiệu ứng.
    *   Khi chí mạng, `Sát thương cuối` được nhân với chỉ số `critDamage` (mặc định 150%).

*   **Công thức Hồi máu (`PerformHeal`):**
    *   `Lượng hồi máu = floor(ATK_Healer * PowerRatio_KỹNăng)`.

## 4. Kỹ năng & Hiệu ứng Trạng thái

*   **Hệ thống Kỹ năng:** **ĐÃ HOÀN THIỆN VÀ TÍCH HỢP ĐẦY ĐỦ.**

*   **Chọn Mục tiêu (`GetTargets`):** Logic chọn mục tiêu rất đa dạng, dựa trên `TargetingType` của `Skill.cs` và đội hình. Ví dụ:
    *   `SingleFrontEnemy`: Tấn công một kẻ địch ngẫu nhiên ở hàng trước. Nếu hàng trước trống, chuyển sang hàng giữa, rồi đến hàng sau.
    *   `LowestHpAlly`: Chọn đồng đội có % HP thấp nhất.
    *   `AllEnemies`: Tấn công toàn bộ kẻ địch còn sống.
    *   Và nhiều loại khác...

*   **Hiệu ứng Trạng thái:** **ĐÃ HOÀN THIỆN.**
    *   Khi một kỹ năng có `appliedEffect` được sử dụng, nó sẽ áp dụng `ActiveStatusEffect` lên mục tiêu.
    *   Các hiệu ứng đã có logic xử lý:
        *   `Poison`: Gây sát thương theo % ATK của người gây ra vào đầu lượt của nạn nhân.
        *   `Slow`: Trừ thẳng vào chỉ số `SPD`.
        *   `CritUp`: Cộng thẳng vào tỉ lệ `critChance`.
        *   `DefDown`: Trừ theo % `DEF` của nạn nhân.
        *   `HealOverTime`: Hồi máu vào đầu lượt của người được hưởng.

## 5. Tình trạng Triển khai

*   **ĐÃ TRIỂN KHAI:**
    *   Hệ thống chiến đấu theo lượt hoàn chỉnh.
    *   Hệ thống kỹ năng, cooldown, và AI chọn hành động.
    *   Hệ thống hiệu ứng trạng thái đầy đủ.
    *   Cơ chế đội hình Trước-Giữa-Sau.
    *   Cơ chế tính sát thương, hồi máu, và chí mạng.

*   **CHƯA TRIỂN KHAI (So với GDD cũ):**
    *   **Logic Trận đấu Boss:** Cơ chế gộp các `Squad` thành một thực thể duy nhất là **KHÔNG TỒN TẠI**. Hệ thống chiến đấu hiện tại xử lý Boss như một `Combatant` bình thường.
    *   **Traits Kích hoạt trong Combat:** Các Trait có hiệu ứng đặc biệt trong chiến đấu (Aura, Tái Sinh, Kẻ Săn Mồi,...) **CHƯA** có logic xử lý trong `CombatSystem.cs`.

## 6. Hậu quả sau Trận đấu

`CombatSystem` không trực tiếp gây ra trạng thái bị thương. Nó chỉ cập nhật `currentHp` trong `HeroData` và trả về `CombatResult`. Hệ thống gọi `CombatSystem` (ví dụ: `ExpeditionManager`) sẽ có trách nhiệm đọc `CombatResult` và gọi `HospitalSystem` để xử lý các hero bị thương hoặc hy sinh.



Trait tăng cấp tối đa
chọn nhanh mạnh nhất hoặc yếu nhất để đi thám hiểm, đánh nhau ( nút + logic)
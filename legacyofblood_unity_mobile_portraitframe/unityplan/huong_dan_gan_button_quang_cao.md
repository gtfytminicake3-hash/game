# Hướng Dẫn Kéo Thả Nút Quảng Cáo Trực Tiếp Trong Unity Editor

Chào bạn, để toàn bộ hệ thống quảng cáo chúng ta vừa Code có thể hoạt động được và hiển thị trong Game, bạn cần thực hiện các thao tác kéo-thả (Assign) thủ công trên giao diện phần mềm Unity Editor. 

Dưới đây là Danh sách tất cả 10 vị trí quảng cáo và cách gắn Nút (Button) cho từng Panel.

---

## Nguyên tắc chung để tạo Nút (Button):
1. Mở Prefab của Panel tương ứng (thường nằm trong thư mục `Assets/Prefabs/UI/`).
2. Nhấn chuột phải vào vùng bạn muốn đặt nút -> [UI](file:///d:/game/legacyofblood_unity_mobile_portraitframe/legacyofblood_unity_mobile_portraitframe/Assets/Scripts/UI/BreedingUIController.cs#231-265) -> `Button - TextMeshPro` (hoặc Button thường tuỳ game của bạn).
3. Đổi Text của nút thành "Xem Quảng Cáo", "Free", "Bonus" hoặc chèn một Icon có chữ "Ad".
4. Chọn gốc của Panel (GameObject chứa Script chính).
5. Kéo cái Nút bạn vừa tạo vào thẳng cái "lỗ" (Field) tương ứng có chữ `Ad Button` trên thanh Inspector của Script.

---

## Chi Tiết 10 Vị Trí Cần Gắn Nút

### 1. Nút X2 Thưởng Thư Chiến Thắng (Mailbox)
- **Mở Prefab:** [ExpeditionReportItem](file:///d:/game/legacyofblood_unity_mobile_portraitframe/legacyofblood_unity_mobile_portraitframe/Assets/Scripts/UI/ExpeditionReportItem.cs#9-163) (Đây là cục phần thưởng nhỏ xíu hiển thị từng dòng trong hòm thư, không phải cả cái MailboxPanel to đâu nhé).
- **Tạo nút:** Tạo 1 nút "X2 Vàng (Ad)" kế bên nút "Nhận" (Claim).
- **Script chứa:** [ExpeditionReportItem](file:///d:/game/legacyofblood_unity_mobile_portraitframe/legacyofblood_unity_mobile_portraitframe/Assets/Scripts/UI/ExpeditionReportItem.cs#9-163)
- **Tên ô cần kéo vào:** `Claim Ad Button`

### 2. Nút Bỏ Qua Chờ Xây Nhà (Building Upgrade)
- **Mở Prefab:** [BuildingUpgradePanel](file:///d:/game/legacyofblood_unity_mobile_portraitframe/legacyofblood_unity_mobile_portraitframe/Assets/Scripts/UI/BuildingUpgradePanel.cs#7-181)
- **Tạo nút:** Tạo 1 nút "Hoàn thành ngay (Ad)" kế bên nút "Nâng cấp".
- **Script chứa:** [BuildingUpgradePanel](file:///d:/game/legacyofblood_unity_mobile_portraitframe/legacyofblood_unity_mobile_portraitframe/Assets/Scripts/UI/BuildingUpgradePanel.cs#7-181)
- **Tên ô cần kéo vào:** `Speed Up Ad Button`

### 3. Nút Chữa Thương Bệnh Viện (Hospital)
- **Mở Prefab:** [InjuredHeroCard](file:///d:/game/legacyofblood_unity_mobile_portraitframe/legacyofblood_unity_mobile_portraitframe/Assets/Scripts/UI/InjuredHeroCard.cs#8-124) (Giống hòm thư, đây là thẻ Tướng bị thương nằm bên trong HospitalPanel).
- **Tạo nút:** Tạo 1 nút "Chữa Miễn Phí (Ad)" kế bên nút tốn vàng.
- **Script chứa:** [InjuredHeroCard](file:///d:/game/legacyofblood_unity_mobile_portraitframe/legacyofblood_unity_mobile_portraitframe/Assets/Scripts/UI/InjuredHeroCard.cs#8-124)
- **Tên ô cần kéo vào:** `Heal Ad Button`

### 4. Nút Quay Tướng Miễn Phí Hàng Ngày (Recruitment)
- **Mở Prefab:** [RecruitmentPanel](file:///d:/game/legacyofblood_unity_mobile_portraitframe/legacyofblood_unity_mobile_portraitframe/Assets/Scripts/UI/RecruitmentPanel.cs#7-139)
- **Tạo nút:** Tạo 1 nút "Quảng Cáo" bên dưới hoặc bên cạnh nút "Triệu Hồi x1".
- **Script chứa:** [RecruitmentPanel](file:///d:/game/legacyofblood_unity_mobile_portraitframe/legacyofblood_unity_mobile_portraitframe/Assets/Scripts/UI/RecruitmentPanel.cs#7-139)
- **Tên ô cần kéo vào:** `Recruit Ad Button`

### 5. Nút Thêm Vé Đấu Trường (Arena)
- **Mở Prefab:** [ArenaPanel](file:///d:/game/legacyofblood_unity_mobile_portraitframe/legacyofblood_unity_mobile_portraitframe/Assets/Scripts/UI/ArenaPanel.cs#9-196)
- **Tạo nút:** Tạo 1 Dấu Của Dấu Cộng `[+]` hoặc nút bấm kế bên biểu tượng Vé Đấu Trường (Tickets).
- **Script chứa:** [ArenaPanel](file:///d:/game/legacyofblood_unity_mobile_portraitframe/legacyofblood_unity_mobile_portraitframe/Assets/Scripts/UI/ArenaPanel.cs#9-196)
- **Tên ô cần kéo vào:** `Add Ticket Ad Button`

### 6. Nút Lai Tạo Đột Biến (Breeding)
- **Mở Prefab:** [BreedingPanel](file:///d:/game/legacyofblood_unity_mobile_portraitframe/legacyofblood_unity_mobile_portraitframe/Assets/Scripts/UI/BreedingUIController.cs#151-158) (hoặc GameObject chứa [BreedingUIController](file:///d:/game/legacyofblood_unity_mobile_portraitframe/legacyofblood_unity_mobile_portraitframe/Assets/Scripts/UI/BreedingUIController.cs#12-425))
- **Tạo nút:** Tạo 1 nút có biểu tượng "Bình Thuốc Kèm Chữ Ad". Đặt gần nút "Lai Tạo" hoặc gần Toggle chọn bình thuốc.
- **Script chứa:** [BreedingUIController](file:///d:/game/legacyofblood_unity_mobile_portraitframe/legacyofblood_unity_mobile_portraitframe/Assets/Scripts/UI/BreedingUIController.cs#12-425)
- **Tên ô cần kéo vào:** `Mutation Ad Button`

### 7. Nút Tua Nhanh Thời Gian Phạt Tháp (Tower)
- **Mở Prefab:** [TowerPanel](file:///d:/game/legacyofblood_unity_mobile_portraitframe/legacyofblood_unity_mobile_portraitframe/Assets/Scripts/UI/TowerPanel.cs#9-159)
- **Tạo nút:** Tạo 1 nút "Chiến Tiếp Bằng Ad" đè lên hoặc nằm cạnh nút Enter/Challenge. (Nút này sẽ tự hiện khi đang đếm lùi thời gian phạt).
- **Script chứa:** [TowerPanel](file:///d:/game/legacyofblood_unity_mobile_portraitframe/legacyofblood_unity_mobile_portraitframe/Assets/Scripts/UI/TowerPanel.cs#9-159)
- **Tên ô cần kéo vào:** `Skip Cooldown Ad Button`

### 8. Nút Quà Cửa Hàng (Shop Freebie)
- **Mở Prefab:** [ArenaShopPanel](file:///d:/game/legacyofblood_unity_mobile_portraitframe/legacyofblood_unity_mobile_portraitframe/Assets/Scripts/UI/ArenaShopPanel.cs#6-77)
- **Tạo nút:** Tạo 1 nút "Freebie" hoặc "Nhận Xu Free (Ad)" đặt phía trên cùng bảng Shop.
- **Script chứa:** [ArenaShopPanel](file:///d:/game/legacyofblood_unity_mobile_portraitframe/legacyofblood_unity_mobile_portraitframe/Assets/Scripts/UI/ArenaShopPanel.cs#6-77)
- **Tên ô cần kéo vào:** `Ad Freebie Button`

### 9. Nút Phục Thù Báo Cáo Thất Bại (Mailbox Revive)
- **Mở Prefab:** [ExpeditionReportItem](file:///d:/game/legacyofblood_unity_mobile_portraitframe/legacyofblood_unity_mobile_portraitframe/Assets/Scripts/UI/ExpeditionReportItem.cs#9-163) (nằm trong UI/Mailbox hoặc tương tự)
- **Tạo nút:** Tạo 1 nút "Hồi Sinh & Đánh Tiếp (Ad)". Chỉ khi Thư báo Cáo là Thất Bại thì nút này mới Tự Động Hiện Lên.
- **Script chứa:** [ExpeditionReportItem](file:///d:/game/legacyofblood_unity_mobile_portraitframe/legacyofblood_unity_mobile_portraitframe/Assets/Scripts/UI/ExpeditionReportItem.cs#9-163)
- **Tên ô cần kéo vào:** `Revive Retry Button`

### 10. Nút Rương Bí Ẩn (Mystic Chest)
- **Mở Scene / Prefab:** [UIMainController](file:///d:/game/legacyofblood_unity_mobile_portraitframe/legacyofblood_unity_mobile_portraitframe/Assets/Scripts/UI/UIMainController.cs#11-221)
- **Tạo nút:** Tạo 1 nút hình cái hòm bạc hoặc rương vàng đang lơ lửng. Nên gắn Animation rung lắc cho nó. Nút này nên đặt góc trên cùng bên trái.
- **Script chứa:** [UIMainController](file:///d:/game/legacyofblood_unity_mobile_portraitframe/legacyofblood_unity_mobile_portraitframe/Assets/Scripts/UI/UIMainController.cs#11-221)
- **Tên ô cần kéo vào:** `Mystic Chest Ad Button`

---
**Lưu ý Cuối Cùng:** 
- Bạn không cần viết Code hành vi cho Nút.
- Code kiểm tra thời gian cho `Mystic Chest` đã được lập trình sẵn và tự chạy ngầm, nút này sẽ tự ẩn hiện dựa trên Cooldown (hiện đang thiết lập 1 giờ). Bạn có thể đổi Icon sao cho thật đặc sắc nhé.

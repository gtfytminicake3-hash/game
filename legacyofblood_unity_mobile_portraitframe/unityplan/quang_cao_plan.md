# Kế hoạch Tích hợp Quảng cáo (Ad Monetization Strategy)

Dựa trên cấu trúc game hiện tại của bạn (Game Màn hình dọc - Portrait, có hệ thống Thẻ Tướng - Hero Card, Squad Selection), đây là một chiến lược rất phù hợp cho dòng game RPG Thẻ tướng / Chiến thuật để tối ưu hoá doanh thu mà không gây bức xúc cho người chơi.

## 1. Xác định Vị trí Quảng cáo (Ad Placements) & Phần thưởng (Rewards)

Trong dòng game thẻ tướng/RPG, việc lạm dụng quảng cáo tự động (Interstitial Ads - ép người chơi xem) sẽ khiến người chơi gỡ game ngay lập tức. Thay vào đó, chúng ta sẽ tập trung 100% vào **Quảng cáo có thưởng (Rewarded Video Ads)** theo mô hình "Tự nguyện đôi bên cùng có lợi".

Dưới đây là các vị trí đặt quảng cáo "Vàng" mang lại doanh thu cao nhất:

### Vị trí 1: Cửa hàng (Shop / Black Market)
- **Cơ chế:** Thêm một mục "Quà Tặng Hàng Ngày" (Daily Freebies) trong Cửa hàng.
- **Phần thưởng:** Nhận thẻ Triệu hồi cơ bản (Basic Summon Scroll) hoặc Vàng/Kim Cương x10, x20.
- **Tần suất:** Giới hạn 3-5 lần xem mỗi ngày.
- **Mục đích:** Kéo người chơi mở Cửa hàng mỗi ngày (tăng tỷ lệ họ nhìn thấy các gói Nạp tiền IAP thật).

### Vị trí 2: Sau Trận Đấu (Post-Match / Victory Screen)
- **Cơ chế:** Nút "X2 Phần Thưởng" (Double Rewards) ở màn hình Victory.
- **Phần thưởng:** Gấp đôi hoặc gấp rưỡi số lượng Vàng / Exp / Thẻ Tướng nhận được từ ván đấu vừa xong.
- **Tần suất:** Xuất hiện ở tất cả các trận PvE hoặc các trận đánh Boss, farm tài nguyên.
- **Mục đích:** Đánh đúng vào tâm lý "muốn cày cuốc nhanh", người chơi sẵn sàng cúng 30s cuộc đời để đỡ phải đánh lại ván đó.

### Vị trí 3: Hồi Sinh Khẩn Cấp (Revive / Continue)
- **Cơ chế:** Khi đội hình/tướng của người chơi chết trong một ải khó, thay vì Game Over, cho họ 1 cơ hội xem quảng cáo để hồi sinh nhóm với 50% hoặc 100% HP.
- **Phần thưởng:** Hồi sinh toàn đội + một chút Buff (ví dụ: Full Năng lượng/Mana).
- **Tần suất:** Giới hạn 1 lần cho mỗi ải.

### Vị trí 4: Hòm Thư Đi Lạc / Rương Bí Ẩn (Mystic Chest trong sảnh)
- **Cơ chế:** Lâu lâu trên Màn hình chính (Main Screen) xuất hiện một Rương báu vật có cánh bay qua hoặc một NPC thương buôn. Nhấn vào sẽ hỏi có muốn xem quảng cáo để mở không.
- **Phần thưởng:** Tài nguyên nâng cấp Tướng, Mảnh ghép Tướng ngẫu nhiên hiếm.
- **Tần suất:** 2-3 tiếng xuất hiện 1 lần.

---

## 2. Cách Gắn Quảng Cáo Vào Game (Implementation)

Để gắn quảng cáo vào luồng trò chơi mà không làm lag game hoặc bị bug, hãy tuân theo quy trình tiêu chuẩn sau. (Chi tiết code bạn đã có ở file `RewardedAdManager` trong cuộc hội thoại trước).

### Bước 1: Gắn UI đúng chuẩn
1. Thiết kế Nút (Button) có biểu tượng cuộn phim nhỏ hoặc chữ `Ad` nhỏ trên góc nút để người chơi nhận thức rõ đây là nút Xem Quảng Cáo chứ không phải nút miễn phí hoàn toàn.
2. Nút X2 Thưởng (trong màn Victory) phải được làm nổi bật (nhiều màu sắc, chớp nháy nhẹ), trong khi nút "Bỏ Qua" (Skip/Claim x1) làm màu chìm hoặc xám đi một chút.

### Bước 2: Viết một Manager Trung Gian (AdRewardGateway.cs)
Không nên gắn thẳng hàm `ShowRewardedAd` của AdManager vào mọi Nút UI. Bạn nên phân loại phần thưởng trước. Ví dụ:

```csharp
public enum RewardType
{
    DoubleGold,
    DailySummon,
    ReviveTeam,
    MysticChest
}

// Bất cứ UI nào muốn gọi Quảng cáo thì truyền Enum loại phần thưởng vào
public void RequestAd(RewardType type)
{
    currentRewardType = type;
    AdManager.Instance.ShowRewardedAd(OnAdFinishCallback);
}

// Callback khi xem xong
public void OnAdFinishCallback(bool success)
{
    if (success) 
    {
        switch(currentRewardType)
        {
            case RewardType.DoubleGold:
                // Tính x2 vàng và update UI
                break;
            case RewardType.DailySummon:
                // Mở rương thẻ quay gacha
                break;
            // vv...
        }
    }
}
```

### Bước 3: Quản lý Luồng Thời Gian (TimeScale)
- **Cực kỳ lọt hố rớt não:** Khi video quảng cáo bật lên, Game của bạn VẪN ĐANG CHẠY ngầm trừ khi bạn dừng nó lại.
- **Cách khắc phục:** 
  - Ngay trước khi gọi `AdManager.Show()`, thiết lập `Time.timeScale = 0f;` để tạm ngưng vật lý/animation. Tắt tiếng nhạc nền game (AudioListener.pause = true).
  - Khi người chơi xem xong (hoặc đóng) quảng cáo, bật lại `Time.timeScale = 1f;` và bật lại tiếng nhạc (`AudioListener.pause = false`).

### Bước 4: Chống Click Liên Tục (Spam Click)
- Khi người dùng bấm nút Xem, ngay lập tức tắt `button.interactable = false;` để tránh họ bấm đúp 2 lần khiến quảng cáo chập cheng hoặc nhận x2 thưởng 2 lần do lỗi logic.
- Sử dụng UI xoay xoay (Loading Spinner) nếu quảng cáo mất >1 giây để tải từ mạng xuống.

---

## 3. Lộ Trình Triển Khai (Roadmap)

- **Giai đoạn 1 (Tuần 1):** Tích hợp Google AdMob dùng *Mã Test*. Viết `AdManager` cơ bản và gắn thử vào 1 nút duy nhất trên MainScreen (ví dụ: Nút nhận 100 Vàng) để test logic trả thưởng.
- **Giai đoạn 2 (Tuần 2):** Thiết kế lại UI màn hình Kết thúc trận đánh (Victory) và Cửa hàng. Gắn luồng X2 phần thưởng và theo dõi TimeScale có bị lỗi gì không.
- **Giai đoạn 3 (Tuần 3):** Lưu trữ số lần đã xem vào `PlayerPrefs` hoặc Server (để giới hạn người chơi xem tối đa 5 lần mỗi ngày, tránh lạm phát tiền tệ). Có thể làm bảng đếm ngược (Countdown) cho mốc quảng cáo tiếp theo.
- **Giai đoạn 4 (Phát hành):** Thay mã Test bằng App ID thật của AdMob. Build APK gửi lên Google Play và chờ tiền chảy về túi!

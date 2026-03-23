# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `ExpeditionReportItem.cs`
**Loại thành phần:** Item Prefab (Thành phần List dạng thẻ ngang / Thư báo).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`ExpeditionReportItem` không phải là một Panel đứng độc lập. Nó là một cái thẻ báo cáo (Giống như 1 email trong Hòm Thư) được nhồi liên tục vào danh sách trượt (ScrollView) của Hòm Thư (Mailbox) hoặc Nhật ký chiến đấu. Thẻ này thông báo kết quả của một đạo quân mà người chơi đã cử đi đánh quái ở World Map.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế một thẻ bài nằm ngang (Horizontal Bar/Card) nhỏ gọn. Mặc dù nhỏ nhưng trên này gắn tới 4 cái nút, nên Designer cần cân nhắc bố cục cực kỳ cẩn thận kẻo người chơi bấm nhầm.

### 2.1 Khu Vực Đọc Kết Quả (Bên Trái/Trên):
*   **Tên Địa Điểm (`TextMeshProUGUI poiNameText`):** Text bự làm tiêu đề (Vd: "Rừng Sương Mù").
*   **Trạng Thái (`TextMeshProUGUI outcomeText`):** Thắng hoặc Thua. Cần Text hoặc Icon (Vd: Cúp vàng / Khuyển cốt).
*   **Mớ Phần Thưởng (`TextMeshProUGUI rewardsText`):** Một đoạn Text rải dài chứa chiến lợi phẩm (Vd: "100 Kinh nghiệm, 50 Vàng, 2 Trang bị..."). 
    *   *Giao Việc:* Khúc này rủi ro về độ dài text rất cao. Cần vẽ khu vực này đủ rộng hoặc Text hỗ trợ tự động xuống dòng/cắt đuôi `Trăng khuyết...`

### 2.2 Dàn Nút Thao Tác (Bên Phải/Dưới):
Khu vực này có 4 nút, nhưng KHÔNG BAO GIỜ hiện ra cùng 1 lúc 4 cái. Logic chia làm 2 phe Thắng/Thua:
*   **Khi Thắng Trận (Victory Mode):**
    *   Nút "Nhận Thưởng" (`Button claimButton`).
    *   Nút "Xem Ad x2" (`Button claimX2Button`): Kèm icon quảng cáo, dụ người chơi ấn vào để nhân đôi đồ rơi rớt.
    *   Nút "Xem Lại" (`Button replayButton`): Icon con mắt/nút Play.
*   **Khi Thua Trận (Defeat Mode):**
    *   Nút "Xoá Thu" (`Button claimButton` được script tận dụng đổi tên mờ đi).
    *   Nút "Hồi Sinh & Lại Tử" (`Button reviveRetryButton`): Yêu cầu vẽ Icon hình Trái Tim / Chữ Thập. Kêu gọi người chơi bỏ tiền/coi Ad để cứu team sống lại đánh vòng 2 ngay tại chỗ.
    *   Nút "Xem Lại" (`Button replayButton`).

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Do là Prefab Instantiated tĩnh, UI này phải tiết kiệm poly. Không gắn Particle hay Animation nhấp nháy vào thẻ này nếu không scroll danh sách sẽ bị lag tụt Quần.
*   **Layer Layout:** Khuyên dùng `HorizontalLayoutGroup` chia thẻ thành 2 nửa: Left (Text) = 70%, Right (Buttons set) = 30%. Các nút bên trong nhét vào 1 cái `VerticalLayoutGroup` hoặc lưới nhỏ.

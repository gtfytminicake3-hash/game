# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `SquadSlotCard.cs`
**Loại thành phần:** Item Prefab (Ô Trống Chứa Tướng Trên Đội Hình).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`SquadSlotCard` là những cái Bục Đứng / Ghế Trống nằm ở nửa trên của màn hình Tuyển Quân (`SquadSelectionPanel`). Đây là nơi người chơi kéo thả các Thẻ Tướng từ bên dưới lên để lấp đầy đội hình. Tương tác của nó giống như chơi cờ: Có thể nhấc 1 con Lốt này thả đè lên Lốt kia để Hoán đổi (Swap) vị trí.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế một Box vuông/chữ nhật dọc bằng đúng kích cỡ của Thẻ Tướng Cơ Bản. Box này có 2 phiên bản hiển thị đè nhau:

### 2.1 Trạng Thái Rỗng (Chưa Xếp Tướng):
*   **Viền Khung Nền Trống (`Image placeholderBackground`):** Vẽ một cái rập khuôn (Silhouette) đứt nét mờ mờ, hoặc một cái bục đá rỗng. Box này cực kì quan trọng vì nó có nhiệm vụ chống sập Layout. Nếu không có Nền này hứng Layout, lúc người chơi nhấc khung có ruột đi chỗ khác, Layout sẽ bị teo tóp lại.
*   **Dấu Báo Hiệu (`GameObject emptyIconObject`):** Đặt ngay giữa Khung Nền Trống. Thường là Icon Dấu Cộng (+), Ý bảo "Bấm / Kéo thả vào đây đi".

### 2.2 Trạng Thái Có Người (Đã Điền Tướng):
*   **Chân Dung Tướng (`Image heroAvatarImage`):** Đặt chèn lên trên Hình nền. Kích thước tương đương cái Lõi của Thẻ Tướng.
*   **Tên Tướng Mờ (`TextMeshProUGUI heroNameText`):** Đặt dưới chân dung.
*   *Lưu ý Phân Mảng:* Toàn bộ khối Đồ Họa Của Trạng Thái Có Người phải được nhóm lại. Khi rút quân, Code sẽ Phựt tắt cụm này đi và Bật lại cụm Dấu Báo Hiệu (Trạng thái Rỗng) nhanh như chớp.

### 2.3 Khu Vực Thao Tác (Button / Drag Drop):
*   **Lớp Phủ Button (`Button slotButton`):** Bọc kín toàn thân thẻ bài. Ngay vị trí người chơi ấn vào (Click/Tap), Tướng sẽ lập tức bị đá văng khỏi Đội Hình, rơi tòm xuống Hồ Danh sách phía dưới. Nút này được Code bắt luôn cả sự kiện Vuốt (Drag).

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Panel này là màn trình diễn "Ma Thuật Kéo Thả" phức tạp nhất game:
    *   Khi người chơi miết ngón tay nhấc thẻ bài lên, Thẻ Bài sẽ lập tức mờ đi 40% (Độ trong suốt Alpha = 0.6) và lơ lửng đi theo ngòi tay xuyên qua màn hình (`blocksRaycasts = false`).
    *   Do đó, Graphic vẽ ra cái Thẻ này (Avata, Viền...) tuyệt đối không được dùng Layout tự động bóp méo hình dạng (Aspect Ratio dỏm), mà phải khóa Fixed Size đàng hoàng. Bằng không lúc Nhấc Lên Bỏ Xuống Thẻ bài sẽ bị dẹp lép hoặc vỡ nát.

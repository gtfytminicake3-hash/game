# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `SpriteSequencePlayer.cs`
**Loại thành phần:** Utility Component / Trình Phát Ảnh Động Giao Diện.

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`SpriteSequencePlayer` KHÔNG PHẢI LÀ MỘT BẢNG PANEL HAY NÚT BẤM. Nó là một Nhạc Trưởng rập khuôn hình ảnh (Animator). Trong Unity, đôi khi xài Hệ thống Animator nguyên bản để làm một cái Icon lấp lánh thì quá cồng kềnh. Tập lệnh này cho phép gắn thẳng vào một tấm ảnh `Image` bình thường, ném cho nó 1 cái Tên Thư Mục, và nó sẽ tự lật từng bức ảnh lên ghép thành chuỗi ảnh động (Giống lật sách Flipbook).

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Không có yêu cầu vẽ Frame UI cụ thể cho cái này, mà đây là Yêu Cầu Xuất File Đầu Ra (Asset Export) khi làm Animation UI cho Coder.

### 2.1 Chuẩn Giao Tiếp Resource (Assets):
*   Khi thiết kế các hiệu ứng Động cho UI (VD: Lửa cháy trên kiếm, Viền khung nhấp nháy 7 sắc cầu vồng, Hiệu ứng thu thập rớt vàng cộp cộp)... Designer hãy xuất chúng ra thành **Chuỗi Ảnh Tĩnh (Image Sequence)** thay vì File GIF hay Video.
*   **Chuẩn Đặt Tên Bắt Buộc:** Phải lót số đuôi chạy tăng dần. Ví dụ: `fx_fire_01.png`, `fx_fire_02.png`, ... `fx_fire_30.png`. Trình Code sử dụng lệnh `.OrderBy(s => s.name)` nên nếu đặt tên lôm côm không theo thứ tự AlphaB, nó sẽ phát hình nhảy cóc hư hết animation.

### 2.2 Tối Ưu Dung Lượng (FPS):
*   Tính năng này chạy Coroutine `yield return new WaitForSeconds(1f / frameRate)`. FPS mặc định của dev đang set là **12 Frame/Giây**. Designer có thể tuỳ biến con số này khi ráp vào Editor, nhưng khuyên dùng từ 12-24 FPS để tiết kiệm File Assets Game. Đừng xuất chuỗi 60 tấm ảnh / giây cho một vòng lặp sẽ khiến App phình to cả GB vì nó load thẳng vào RAM từ `Resources`.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   **Tích hợp:** Mọi thứ chạy cục bộ. Script này sẽ "nhuộm" linh hồn lên thành phần `Image` hoặc `RawImage` gốc của Designer. Nên cái GameObject đó cứ để màu Trắng (`#FFFFFF`) để màu xuất ra được chân thực nhất.
*   **Sử dụng ở đâu:** Có thể dùng cho Lửa Đuốc trên màn hình Menu, Hiệu ứng Sấm Sét bên trên nút Bấm "Vào Trận Nhanh", hoặc Quầng Sáng xoay vòng sau lưng Thẻ Tướng Cấp SSS.

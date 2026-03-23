# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `HeroPickerCard.cs`
**Loại thành phần:** Script Phụ Trợ (Modifier / Behavior Override).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`HeroPickerCard` là một tệp lệnh đặc biệt. Nó **KHÔNG ĐÒI HỎI MỘT THIẾT KẾ UI MỚI**.
Nó hoạt động như một loại ký sinh trùng, chỉ được đính kèm vào Prefab `HeroCard` (Đã thiết kế ở file 18_HeroCard) mỗi khi trò chơi mở bảng chọn tướng (Ví dụ: Mở bảng chọn Cha/Mẹ để nhân giống, bảng chọn người tung vào Đấu Trường...).

Mục đích duy nhất của đoạn lệnh này là cướp cò (Override) biến cố Nhấn Nút (Click Event) của thẻ `HeroCard` gốc.

## 2. PHÂN TÍCH HÀNH VI (CHO DEV & DESIGN)
*   **Hành vi gốc của HeroCard:** Khi người chơi bấm vào thẻ bài, thẻ bài sẽ phòng to lên thành Bảng chi tiết Hệ Gen (`HeroInfoPanel`).
*   **Hành vi bị cướp bởi HeroPickerCard:** Khi thẻ `HeroCard` được sinh ra bên trong một danh sách lựa chọn chọn lọc (`HeroPickerPanel`), Component này lập tức xoá bỏ hành vi cũ đi. Trải nghiệm của người chơi lúc này là: Bấm vào thẻ -> Thẻ không phóng to -> Bảng danh sách đóng rụp lại -> Trả bức ảnh Tướng đó về cho Ô trống (Slot Cha/Mẹ/Đội hình) đang đòi hỏi trước đó.

## 3. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
*   **Không cần làm gì cả cho tệp này.** Cứ giữ nguyên thiết kế xịn sò của Prefab `HeroCard`. Mọi thứ đã được Dev xử lý ngầm ở Backend.

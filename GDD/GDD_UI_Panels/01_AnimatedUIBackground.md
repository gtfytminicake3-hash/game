# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `AnimatedUIBackground.cs`
**Loại thành phần:** Visual Effect / UI Extension (Không phải là một Panel trọn vẹn mà là một script gắn kèm).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`AnimatedUIBackground` là một công cụ giúp tạo ra các hình nền động dạng 2D (chạy frame-by-frame animation, giống ảnh GIF) ngay trên UI Canvas của Unity.
Script này thường được gắn vào các màn hình cần sự sống động ở background, chẳng hạn như **Màn hình Tuyển dụng (Recruitment)**, màn hình chờ, hoặc cảnh quay gacha.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Để script này hoạt động được, đội ngũ Thiết kế Mỹ thuật cần cung cấp:

*   **Sequence Frames (Chuỗi ảnh động):**
    *   Cần xuất ra một chuỗi các ảnh nén rời (PNG hoặc JPG), cắt theo từng frame (ví dụ 24 khung hình/giây).
    *   **Quy tắc đặt tên:** Bắt buộc đặt tên theo số thứ tự liên tiếp có padding (Ví dụ: `bg_anim_001.png`, `bg_anim_002.png`, `bg_anim_003.png`...). Việc này rất quan trọng để script phân loại đúng logic thời gian.
    *   **Đường dẫn:** Các file này phải được đóng gói và ném vào thư mục quy định trong Unity (Mặc định trong code đang là: `Resources/UI/RecruitmentBG`).

## 3. CÁC THÔNG SỐ CẤU HÌNH (CHO EDITOR/DEV)
Panel/Hình nền nào gắn script này sẽ có các tùy chỉnh sau trên Inspector:
*   `resourceFolderPath` (string): Đường dẫn thư mục chứa chuỗi ảnh (VD: "UI/RecruitmentBG").
*   `fps` (float): Số lượng khung hình chạy trên một giây (Khuyến nghị: 24).
*   `loop` (bool): Có lặp lại vòng lặp ảnh cảnh hay không (Ví dụ: Gió thổi lặp đi lặp lại hay chỉ lướt qua 1 lần rồi đứng yên).

## 4. CÁC NÚT BẤM (BUTTONS) VÀ ELEMENT TƯƠNG TÁC
*   **Không có nút bấm trực tiếp:** Script này thuần túy là hiển thị Background (kế thừa yêu cầu 1 component `Image`). Nó sẽ chạy ngầm ngay khi màn hình (mẹ) được gọi lên (`Awake/Update`). Nên không cần vẽ nút bấm nào dành riêng cho logic file này.

# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `RecruitmentPanel.cs`
**Loại thành phần:** Full Screen Panel / Quán Trọ Tuyển Quân (Gacha).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`RecruitmentPanel` đóng vai trò là Lõi Gacha của game (Hệ thống quay lô tướng). Nơi người chơi đốt Kim cương hoặc Vé Triệu Hồi để cầu may đổi đời. Giao diện này cần mang tính Epic (Thần thoại), hào nhoáng để kích thích nạp tiền.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Màn hình này KHÔNG CẦN CHĂM CHÚT BACKROUND. Dev đã gắn một Script `AnimatedUIBackground` tự động phát chuỗi ảnh động (Sequence) từ thư mục `Resources/UI/RecruitmentBG` làm nền phía sau. Do đó, team UI chỉ cần vẽ Cụm Nút Điều Khiển là đủ.

### 2.1 Mảng Nút Thoát:
*   **Nút Thoát (`Button closeButton`):** Chỉ cần nút X hoặc Mũi Tên Trở Về nằm khiêm tốn ở mép trên cùng.

### 2.2 Cụm Điều Khiển Triệu Hồi (Đặt ở dưới đáy màn hình):
Đây là xương sống của mọi game Gacha. Vẽ 3 cái Nút Siêu To Khổng Lồ xếp dàn ngang hoặc khối hình tam giác:
*   **Nút Triệu Hồi x1 (`Button recruitOneButton`):** 
    *   Icon: Quyển Sách / Bia Rượu / Vé.
    *   Text Gắn Liền: "Triệu Hồi 1 Lần".
    *   Tem Giá: "1 Vé Triệu Hồi" Hoặc "100 Kim Cương".
*   **Nút Triệu Hồi x10 (`Button recruitTenButton`):** 
    *   Icon: Quần sáng to hơn, xịn hơn.
    *   Text Gắn Liền: "Triệu Hồi 10 Lần".
    *   Tem Giá: "10 Vé Triệu Hồi" Hoặc "900 Kim Cương". (Có thể thêm cái biển chữ "Sale 10%" nhấp nháy).
*   **Nút Triệu Hồi Quảng Cáo (`Button recruitAdButton`):**
    *   Icon: Logo Play Video Quảng Cáo.
    *   Text Gắn Liền: "Miễn Phí 1 Lần".
    *   *Cách thức hoạt động:* Nút này mỗi ngày chỉ hiện thị lên một lần duy nhất. Cho nên khi thiết kế có thể đặt nó nhét kẹp giữa 2 nút kia hoặc để nổi nhấp nháy đè lên nút x1. Khi user coi xong, Code sẽ giấu biến cái Nút này (`SetActive(false)`) chứ không phải bôi xám đi.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   **Thiếu Vắng Cảnh Kết Quả:** Kịch bản code hiện tại bóp cò rút Tướng thành công sẽ chỉ sinh ra 1 con Notification (Toast nổ chữ "Thu phục Arthur thành công!"). Phần múa mây (Gacha Result UI) đang nằm lửng lơ ở dòng chú thích `// TODO: Show hero results UI`. Designer có quyền đề xuất 1 cái Popup Trưng Bày Ẩn để tương lai Dev gỡ TODO này ra ném Thẻ Hero vào.

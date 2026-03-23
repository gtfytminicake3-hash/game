# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `BuildingUpgradePanel.cs`
**Loại thành phần:** Panel / Pop-up Giao diện Nâng Cấp Công Trình.

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`BuildingUpgradePanel` đóng vai trò là "Nhà thầu xây dựng". Khi người chơi click vào cái Búa (Nâng cấp) ở trong các nhà Barrack, Hospital... Panel này sẽ hiện lên báo giá. Khác với các panel thông thường, nó chứa cả Tình trạng Đang Xây (Under Construction) có đếm ngược thời gian.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế một Modal/Popup kích thước nhỏ hoặc vừa (không cần full màn hình). Yêu cầu phân mảng rõ ràng giữa Nội dung Thông tin và Giá Tiền:

### 2.1 Khu Vực Định Danh Công Trình (Top & Middle):
*   **Tiêu đề (`TextMeshProUGUI titleText`):** Chỉ đích danh nhà nào (Ví dụ: "Nâng Cấp Nhà Chữa Bệnh").
*   **Chỉ Số Cấp Độ (`TextMeshProUGUI infoText`):** Chữ nhỏ, nằm dưới tiêu đề. Format: "Level X -> Level Y".
*   **Chữ Text Lợi Ích (`TextMeshProUGUI benefitText`):** Một đoạn Text quan trọng. Kêu gọi người chơi nâng cấp. Nghĩa là nâng cấp thì tôi được gì? (Ví dụ hiên ra chữ: `Sức chứa Dân Số: 50 -> 55`).

### 2.2 Khu Vực Báo Giá Nguyên Liệu (Material List):
*   **Hộp chi phí (`TextMeshProUGUI costText`):** Vùng Textbox cần rộng, vì nó phải chứa 3 dòng hiển thị giá (Vàng, Gỗ, Đá). 
*   *Gợi ý Layout:* Designer có thể vẽ thay textbox bằng một dãy UI Grid 3 slot, tuy nhiên hiện tại source code đang cộng dồn chuỗi string (vd: `Vàng: 100 \n Gỗ: 50 \n Đá: 10`), nên 1 hộp Textbox bự cho `costText` là bắt buộc phải có cho script móc data vào.

### 2.3 Khu Vực Điều Khiển Tiến Độ & Xây Dựng (Footer):
*   **Nút Đập Búa Nâng Cấp (`Button upgradeButton`):** Đặt to ở đáy. Chữ "Nâng Cấp". Có 2 trạng thái bật (Sáng) khi thừa Mộc/Đá, và Khoá xám (Disable) khi nghèo/đói nguyên liệu.
*   **Đồng Hồ Đếm Ngược (`TextMeshProUGUI upgradeTimerText`):** Hiển thị dạng giờ (02:15:30). Mặc định ẨN ĐI. Khi nhà "Đang xây" thì sẽ BẬT lên ở giữa màn thay vì báo giá nguyên liệu.
*   **Nút Xem Ad Rút Ngắn Giờ (`Button speedUpAdButton`):** Chỉ bật ra khi nhà đang bị xây/vướng cooldown. Design yêu cầu gắn Icon Quảng Cáo Video (Ad) + Icon mộc bản Thời Gian. Script khi bấm sẽ skip ngay 20 phút xây (1,200,000 milisecond).

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   Panel này là màn hình "2 mặt" dùng chung 1 khung hình. 
    1. Khi **chưa xây**: Sẽ hiển thị Giá Tiền + Nút Upgrade to bàng hoàng. (Nút Ad và Đồng hồ biến mất).
    2. Khi **đã bấm mũi khoan xây dựng xong**: Nút Upgrade biến mất nhường đường cho Nút XEM AD Tua Giời Gian ngoi lên, CostText có thể che đi nhét cái Đồng hồ đếm ngược vào thế chỗ. Layout phải linh hoạt để chữ/nút ẩn hiện không bị vấp/lệch hình học. Cần chú pivot layout tĩnh.

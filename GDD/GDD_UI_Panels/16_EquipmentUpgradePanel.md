# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `EquipmentUpgradePanel.cs`
**Loại thành phần:** Popup phụ (Sub-Popup) / Hộp Thoại Nâng Cấp Tự Động.

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`EquipmentUpgradePanel` là một hộp thoại nhỏ xuất hiện dồn dập khi người chơi bấm nút "Cường Hóa Nhanh" từ `EquipmentDetailPanel`. Khác với các game truyền thống bắt người chơi gắp từng món đồ rác bỏ vào khung cường hóa, trò chơi này sử dụng cơ chế **Auto-Cannibalize (Tự động cắn rác)**. Người chơi tick chọn những cấp độ Phẩm Chất (Tier) tụi rác mà họ muốn hiến tế, bấm Xác nhận, và thuật toán sẽ càn quét túi đồ để nuốt sạch chúng.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Thiết kế một hộp thoại (Dialog Box) vừa vặn, nằm chính giữa màn hình.

### 2.1 Khu Vực Tiêu Đề & Nút Chọn Bộ Lọc (Filter Toggles):
*   **Tiêu đề:** "Nâng Cấp Nhanh" hoặc "Chọn Phôi Tế Thần".
*   **Lưới Checkbox Phẩm Chất (5 Toggles):**
    *   Yêu cầu vẽ 5 Option Box (hoặc nút bấm dạng Check/Uncheck) đại diện cho 5 Tier: D, C, B, A, S. Mặc định vào game Code sẽ tick On cái ô Tier D.
    *   Gắn biến `Toggle toggleTierD`, `toggleTierC`, `toggleTierB`, `toggleTierA`, `toggleTierS` vào. Có thể trang trí màu viền dựa theo luật màu phẩm chất: Trắng (D), Lục (C), Lam (B), Tím (A), Cam (S).

### 2.2 Khu Vực Cảnh Báo (Tùy chọn cho Design):
*   Nên có một đoạn Note Text tĩnh (Không cần code nhúng) cảnh báo: "Lưu ý: Thao tác này sẽ tự động phân rã TOÀN BỘ trang bị Lv.1 (Chưa khoá) trong túi có cùng Phẩm chất đã chọn." để người chơi không bị sốc khi bay sạch túi đồ.

### 2.3 Khúc Dưới / Nút Xác Nhận:
*   **Nút Xác Nhận Cắn Rác (`Button confirmUpgradeButton`):** Bấm vào là hệ thống thực thi.
*   **Nút Hủy / Đóng (`Button closeButton`):** Bấm để quay về.

## 3. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   **An toàn Tế Thần:** Để đảm bảo người chơi không khóc thét, Logic Code của panel này đang khống chế rất chặt: Nó chỉ nuốt những món đồ thỏa mãn cả 3 điều kiện: 
    1. Cùng Tier với ô Checkbox đã chọn.
    2. Đang ở **Level 1** (Nghĩa là chưa từng được mang đi upgrade bao giờ).
    3. Trạng thái `isLocked` phải là **False** (Không dập ổ khoá).
*   Popup này có tính chất trồng chéo (Overlay chồng Overlay), nên Z-Order của nó phải cao hơn `EquipmentDetailPanel`.

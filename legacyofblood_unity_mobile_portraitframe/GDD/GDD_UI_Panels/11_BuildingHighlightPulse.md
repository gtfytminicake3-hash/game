# TÀI LIỆU THIẾT KẾ UI (UI SPECIFICATION)
**Tệp Script:** `BuildingHighlightPulse.cs`
**Loại thành phần:** Hiệu ứng Hình Ảnh (Visual Effect Extension / UI Decorator).

---

## 1. MÔ TẢ CHUNG VÀ MỤC ĐÍCH
`BuildingHighlightPulse` KHÔNG phải là một Panel hay Màn hình. Nó là một đoạn mã (Script) cỡ nhỏ chuyên dùng để gắn đè lên các nút bấm (Button) hoặc hình ảnh công trình (Image) trên Bản Đồ Ngôi Làng nhằm mục đích **gây chú ý**.
Tác dụng của nó là tự động cộng thêm một đường viền (Outline) phát sáng nhấp nháy như đèn neon (Breathing effect) dọc theo hình thù của chủ thể.

## 2. YÊU CẦU CHO ĐỘI DESIGNER (2D/UI)
Do script này tận dụng Component `Outline` có sẵn của Unity Engine, nên Designer **không cần vẽ hình ảnh animation nhấp nháy thủ công** rườm rà. Chỉ cần chú ý hai điểm:
*   **Sprite của Vật thể mẹ (Base Image):** Phải được cắt viền rỗng tươm tất (Transparent PNG/Alpha cắt gọt sắc nét). Component Outline của Unity sẽ dò theo mép cực hạn của Alpha Channel để viền nét. Nếu chừa lại rác/hoặc viền mờ trong file ảnh PNG thì viền nhấp nháy cũng sẽ hiện ra mờ nhoè hoặc hình vuông rất xấu xí.
*   **Không Vẽ Khóa Khung (Margins):** Vì viền sáng sẽ lấn ra bên ngoài mép ảnh dăm pixel (Code đang set `effectDistance = (4, -4)`), không nên tống sát cạnh ảnh vào sát mép RectTransform kẻo nó cắt đuôi mất cái viền sáng đèn.

## 3. CÁC THÔNG SỐ CẤU HÌNH (CHO EDITOR/DEV)
Người ráp UI trên Unity có thể điều chỉnh các thanh trượt sau trên Inspector từng vật thể:
*   `pulseSpeed` (Tốc độ mạch): Vòng nhấp nháy trên giây. Chỉnh cao thì chớp như còi cảnh sát, chỉnh thấp bằng `1` thì thở nhịp nhàng.
*   `minAlpha` & `maxAlpha`: Giới hạn mờ đi và rực sáng của viền sáng. 
*   `pulseColor`: Màu sắc đèn báo (Mặc định: Vàng). Có thể đổi sang Đỏ nếu là báo lỗi công trình, Xanh lá nếu hoàn thành nhiệm vụ...

## 4. LƯU Ý LUỒNG HOẠT ĐỘNG (FLOW DÀNH CHO DEV/DESIGN)
*   **Bảo Toàn Hiệu Năng:** Chớp tắt liên tục dùng hàm Sin ở Update() được xếp vào mảng (Dynamic UI). Vì game có hệ thống tinh chỉnh settings cá nhân để chống giật máy, nên Code sẽ dò quyền `useDynamicUI`. Nếu máy yếu, viền sáng này bị triệt tiêu chế độ chớp nháy và chỉ sáng viền tĩnh chơ vơ (Solid).

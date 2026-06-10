# 🛠️ Hướng Dẫn Setup Dự Án Trên Máy Mới (Legend of Blood)

Khi clone dự án Unity từ GitHub sang một máy tính mới, do các thư mục cache (Library, Logs, Temp, obj) đã bị ignore để giảm dung lượng tải, Unity sẽ cần phải re-import và cấu hình lại. Việc thiếu đồng nhất hoặc lỗi hiển thị (đặc biệt là lỗi font chữ, UI hồng, mất reference) thường xuyên xảy ra ở lần mở đầu tiên.

Hãy làm theo trình tự các bước dưới đây để đảm bảo dự án chạy hoàn hảo 100% giống máy gốc.

---

## Bước 1: Cài đặt đúng phiên bản Unity
- Dự án này bắt buộc phải chạy trên **Unity 6000.3.10f1**.
- Mở Unity Hub -> Installs -> Install Editor -> Chọn đúng bản 6000.3.10f1 (Nhớ tích chọn module **Android Build Support** và **iOS Build Support** vì đây là game mobile).

## Bước 2: Mở dự án lần đầu tiên
- Trong Unity Hub, chọn **Add project from disk** và trỏ tới thư mục vừa clone.
- Mở dự án lên. **Lưu ý:** Lần mở đầu tiên sẽ mất khá nhiều thời gian (từ 5 - 15 phút) vì Unity phải tải lại toàn bộ Package và rebuild lại thư mục Library. Hãy kiên nhẫn và không tắt ngang.

## Bước 3: Đổi Platform sang Mobile (RẤT QUAN TRỌNG)
Game được thiết kế dọc cho Mobile. Nếu máy mới đang để mặc định là PC/Mac/Linux Standalone, UI sẽ bị sai tỉ lệ và một số plugin (như GoogleMobileAds) có thể báo lỗi.
1. Vào **File > Build Profiles** (hoặc **Build Settings** tùy giao diện Unity 6).
2. Chọn platform là **Android** (hoặc **iOS**).
3. Nhấn **Switch Platform** và đợi Unity re-import các assets về chuẩn nén của Mobile.

## Bước 4: Sửa lỗi hiển thị UI / TextMeshPro (Lỗi phổ biến nhất)
Nếu bạn thấy chữ bị lỗi (hiển thị cục vuông), UI màu hồng, hoặc Console báo lỗi liên quan đến TMP_FontAsset, m_AtlasTextures, hãy làm ngay:
1. Trên thanh menu, chọn **Window > TextMeshPro > Import TMP Essential Resources**.
2. Một bảng hiện ra, nhấn **Import All**.
3. Nếu vẫn còn lỗi font trong các Prefab, hãy vào cửa sổ Project, tìm file font chữ game đang dùng (trong thư mục Fonts/Resources), click chuột phải vào file font đó và chọn **Reimport**.

## Bước 5: Reload lại Scene gốc
Vì lúc mở lên có thể TMP chưa có sẵn, Scene đang mở có thể bị "gãy" reference.
1. Đừng lưu Scene hiện tại (Don't Save).
2. Vào cửa sổ Project, mở **Assets/Scenes/GameClient.unity** (hoặc Scene chính của game).
3. Chỉnh cửa sổ **Game View** sang tỉ lệ màn hình dọc di động (VD: 1080 x 1920 Portrait hoặc Phone 9:16). UI sẽ hiển thị khớp và không bị vỡ.

## Bước 6: Khôi phục Addressables / Asset Bundles (Nếu có)
1. Vào **Window > Asset Management > Addressables > Groups**.
2. Chọn **Build > Clear Build State** (để xóa rác từ máy cũ nếu lỡ commit).
3. Chọn **Build > New Build > Default Build Script**. Việc này sẽ bundle lại các asset cần thiết cho runtime.

## Bước 7: Xử lý lỗi thư viện / Package báo đỏ
Nếu màn hình Console (Ctrl + Shift + C) báo lỗi đỏ liên quan đến Code (Assembly, Packages):
1. Đảm bảo bạn đang kết nối mạng ổn định.
2. Cửa sổ Project -> Nhấn chuột phải vào thư mục Packages -> **Resolve Packages**.
3. Hoặc mở **Window > Package Manager**, đổi bộ lọc góc trái trên cùng sang In Project, chờ nó load xong để chắc chắn không có package nào bị fail.

## Bước 8: Dọn dẹp lỗi ảo (Dummy Errors)
1. Bấm **Clear** trong cửa sổ Console để xem lỗi có thực sự tồn tại hay chỉ là lỗi cache lúc mới mở.
2. Nếu vẫn còn lỗi Script không chịu nhận, click chuột phải vào thư mục Assets -> chọn **Reimport All**. (Sẽ mất thêm chút thời gian nhưng giải quyết được 99% lỗi metadata).

---
✅ **Hoàn tất:** Sau khi làm xong 8 bước này, bấm nút **Play (▶)**, dự án sẽ chạy trơn tru với UI chuẩn xác như trên máy dev ban đầu!

Kế hoạch Nâng cấp Toàn diện: Bản đồ Thế giới (World Map)
Phiên bản: 1.0
Ngày: 11/10/2025
1. Tầm nhìn & Mục tiêu Tổng thể
Bản nâng cấp này nhằm mục đích chuyển đổi hệ thống World Map từ một tính năng thụ động thành một trung tâm chiến lược, năng động và hấp dẫn.
Về Trải nghiệm Người dùng (UX): Cung cấp một luồng trải nghiệm mượt mà, không gián đoạn, cho phép người chơi quản lý kết quả của nhiều chuyến thám hiểm một cách hiệu quả thông qua hệ thống Hộp thư.
Về Chiều sâu Gameplay: Giới thiệu Tháp Thử Thách, một thử thách PvE đỉnh cao, độc nhất, khuyến khích người chơi thử nghiệm các chiến thuật mới và hợp tác giữa các đội hình.
Về Phần thưởng & Động lực: Tạo ra những mục tiêu rõ ràng và phần thưởng xứng đáng, thúc đẩy người chơi khám phá bản đồ và đầu tư vào việc xây dựng nhiều đội quân mạnh mẽ.
Phần A: Hệ thống Hộp thư Báo cáo Thám hiểm
A.1. Mục tiêu Thiết kế
Loại bỏ hoàn toàn tình trạng "địa ngục popup" khi nhiều đoàn thám hiểm trở về cùng lúc.
Cho phép người chơi nhận phần thưởng và xử lý hậu quả trận chiến một cách chủ động, vào thời điểm họ mong muốn.
Đảm bảo không có phần thưởng hoặc kết quả nào bị mất, kể cả khi người chơi thoát game trước khi nhận.
A.2. Quy định & Luật chơi
Lưu trữ Tự động: Khi một chuyến thám hiểm (bất kể loại nào) kết thúc, kết quả của nó sẽ không được xử lý ngay lập tức. Thay vào đó, một "Báo cáo Thám hiểm" chi tiết sẽ được tự động tạo và lưu vào Hộp thư của người chơi.
Thông báo Badge: Một biểu tượng thông báo (badge) sẽ xuất hiện trên nút "Hộp thư" ở giao diện chính để báo cho người chơi biết có báo cáo mới chưa đọc.
Nhận thưởng Thủ công: Người chơi phải vào Hộp thư và nhấn nút "Nhận" trên từng báo cáo hoặc "Nhận tất cả" để tài nguyên, vật phẩm, kinh nghiệm được cộng vào tài khoản và các hero bị thương được gửi đến Bệnh viện.
Tính bền vững: Các báo cáo chưa nhận sẽ được lưu vĩnh viễn trong dữ liệu của người chơi (PlayerData) cho đến khi được nhận.
A.3. Luồng Xử lý Kỹ thuật
Tạo Cấu trúc Dữ liệu ExpeditionReport:
Tạo một lớp C# mới ExpeditionReport có [System.Serializable].
Lớp này chứa tất cả dữ liệu cần thiết: CombatResult (nếu có), LootData, kinh nghiệm nhận được, ID của POI, tên POI, v.v.
Sửa đổi PlayerData.cs:
Thêm một trường dữ liệu mới: public List<ExpeditionReport> UnclaimedReports;
Viết lại ExpeditionManager.FinalizeExpedition():
Hàm này sẽ bị thay đổi hoàn toàn. Chức năng chính của nó bây giờ là:
Tạo một đối tượng ExpeditionReport mới.
Điền đầy đủ thông tin kết quả của chuyến đi vào đối tượng đó.
Thêm đối tượng này vào danh sách DataManager.Instance.Player.UnclaimedReports.
Phát ra sự kiện toàn cục, ví dụ GameEvents.OnNewReportReceived, để UI có thể hiển thị badge.
Triển khai Giao diện Hộp thư (MailboxPanel.cs):
Khi mở, panel sẽ đọc danh sách UnclaimedReports từ PlayerData để hiển thị.
Khi người chơi nhấn "Nhận", panel này sẽ chịu trách nhiệm gọi các hệ thống tương ứng để xử lý dữ liệu từ báo cáo:
Gọi InventoryManager để cộng tài nguyên/vật phẩm.
Gọi hero.GainExp() cho từng hero.
Gọi HospitalSystem cho các hero bị thương/hy sinh.
Sau khi xử lý xong, xóa báo cáo đó khỏi danh sách UnclaimedReports và cập nhật lại giao diện.
Phần B: POI Đặc biệt - Tháp Thử Thách
B.1. Mục tiêu Thiết kế
Tạo ra một thử thách PvE endgame độc nhất, đòi hỏi sự chuẩn bị chiến lược ở cấp độ cao.
Giới thiệu cơ chế "Tiếp sức", nơi nhiều đội có thể hợp tác để chinh phục một mục tiêu chung.
Cung cấp một nguồn phần thưởng hiếm và giá trị, không thể kiếm được từ các hoạt động thông thường.
B.2. Quy định & Luật chơi
Tính Độc nhất: Chỉ có duy nhất một Tháp Thử Thách tồn tại trên bản đồ tại một thời điểm.
Cấu trúc: Tháp có 20 tầng cố định.
Luật Chiến đấu Liên tục: Đội tham chiến sẽ chiến đấu qua các tầng liên tiếp. HP và trạng thái của hero không được hồi lại giữa các tầng.
Luật Thất bại & Lưu Tiến độ:
Khi một đội bị đánh bại (ví dụ ở Tầng 15), chuyến thám hiểm của họ kết thúc.
POI Tháp sẽ vào trạng thái "Hồi phục" trong 2 tiếng.
Tiến độ của Tháp được lưu lại (vẫn đang ở Tầng 15).
Luật Tiếp sức: Trong thời gian Tháp hồi phục, người chơi có thể gửi một đội khác đến. Đội này sẽ bắt đầu chiến đấu từ tầng mà đội trước đã thất bại (Tầng 15).
Luật Reset: Nếu hết 2 tiếng hồi phục mà không có đội nào khác đến, tiến độ của Tháp sẽ bị reset về Tầng 1.
Luật Phần thưởng: Phần thưởng chỉ được trao một lần duy nhất khi người chơi chinh phục thành công Tầng 20. Không có phần thưởng cho các tầng trung gian.
Luật Tái tạo: Ngay khi bị chinh phục, POI Tháp sẽ biến mất và một Tháp mới (đã reset) sẽ xuất hiện ở một vị trí ngẫu nhiên khác.
B.3. Luồng Xử lý Kỹ thuật
Mở rộng Cấu trúc Dữ liệu PoiData:
Thêm một enum PoiType và giá trị TowerOfTrials.
Thêm các trường dữ liệu chỉ dành cho Tháp: public int currentFloor và public long recoveryEndTime. Trạng thái của Tháp được lưu trực tiếp trên chính POI đó trong PlayerData.WorldPois.
Sửa đổi WorldMapController:
Đảm bảo logic tạo POI chỉ tạo ra một Tháp duy nhất trên bản đồ.
Lắng nghe sự kiện OnTowerConquered để xóa POI cũ và gọi hàm tạo một Tháp mới.
Sửa đổi ExpeditionManager:
Trong máy trạng thái Tick(), tại trạng thái Exploring, thêm một nhánh logic:
if (poi.Type == PoiType.TowerOfTrials): Gọi hàm xử lý Tháp chuyên dụng ProcessTowerChallenge().
else: Chạy logic chiến đấu tiêu chuẩn.
Hàm ProcessTowerChallenge() sẽ thực hiện:
Kiểm tra recoveryEndTime để quyết định có reset currentFloor về 1 hay không.
Bắt đầu một vòng lặp chiến đấu liên tục từ currentFloor.
Nếu thua, cập nhật recoveryEndTime và currentFloor trên đối tượng poi, sau đó kết thúc chuyến đi.
Nếu thắng toàn bộ 20 tầng, gán gói phần thưởng lớn cho chuyến đi, phát sự kiện OnTowerConquered, sau đó kết thúc chuyến đi.
Cập nhật POI_InfoPanel.cs:
Panel thông tin cần được nâng cấp để đọc và hiển thị các dữ liệu đặc biệt của Tháp: tiến độ tầng hiện tại và đồng hồ đếm ngược thời gian hồi phục (nếu có).
Kế hoạch Triển khai: Hệ thống Thám hiểm Offline & Tính toán Tức thì
1. Tầm nhìn & Nguyên tắc Thiết kế
Nguyên tắc "Fire-and-Forget" (Bắn và Quên): Ngay khi người chơi xác nhận đội hình và bắt đầu chuyến đi, hành động của họ được coi là đã hoàn tất. Game sẽ ngay lập tức tính toán toàn bộ kết quả của chuyến đi đó. Người chơi không cần phải online để quá trình diễn ra.
Thời gian Chờ là Cơ chế Gameplay, không phải Thời gian Xử lý: Thời gian di chuyển của xe ngựa trên bản đồ chỉ đơn thuần là một bộ đếm ngược hiển thị cho người chơi biết khi nào họ có thể nhận kết quả. Nó không phải là lúc game đang thực sự xử lý logic.
Đảm bảo Tính Toàn vẹn Dữ liệu: Hệ thống phải đảm bảo rằng dù người chơi online hay offline, kết quả của chuyến thám hiểm vẫn được tính toán chính xác và được lưu lại an toàn để chờ người chơi quay lại nhận.
2. Luồng Xử lý Mới (Cập nhật)
Đây là luồng xử lý từ góc nhìn của hệ thống, từ lúc người chơi nhấn nút "Xác nhận" đến khi kết quả nằm trong Hộp thư.
Hành động của Người chơi: Người chơi nhấn nút "Xác nhận" trên SquadSelectionPanel để bắt đầu một chuyến thám hiểm đến một POI (thường hoặc Tháp).
(THAY ĐỔI CỐT LÕI) Tính toán Tức thì:
Ngay tại thời điểm đó, ExpeditionManager được gọi.
ExpeditionManager ngay lập tức thực hiện toàn bộ chuỗi logic mô phỏng cho chuyến đi đó:
a. Tính toán Thời gian: Xác định tổng thời gian của chuyến đi (travel_time_to_poi + exploring_time + travel_time_back_to_village).
b. Mô phỏng Kết quả:
Gọi CombatSystem.Simulate() (hoặc logic ProcessTowerChallenge) để xác định thắng/thua, thương vong.
Tính toán phần thưởng (LootData), kinh nghiệm nhận được.
c. Tạo Báo cáo Hoàn chỉnh: Tạo một đối tượng ExpeditionReport chứa tất cả kết quả tính toán ở bước (b).
Lưu trữ Trạng thái Chờ:
ExpeditionManager không lưu báo cáo vào Hộp thư ngay.
Thay vào đó, nó tạo một đối tượng ActiveExpedition mới. Đối tượng này sẽ chứa:
ID của chuyến đi.
ID của các hero tham gia.
Toàn bộ đối tượng ExpeditionReport đã được tính toán trước.
Một mốc thời gian kết thúc: completionTimestamp = currentTime + total_expedition_time.
Lưu đối tượng ActiveExpedition này vào danh sách PlayerData.ActiveExpeditions.
Lưu Game & Bắt đầu Hiển thị:
Hệ thống ngay lập tức lưu lại PlayerData.
ExpeditionManager phát ra sự kiện OnExpeditionStarted như cũ.
WorldMapController lắng nghe sự kiện này và tạo ra travelCartPrefab để di chuyển trên bản đồ, mô phỏng chuyến đi cho người chơi xem.
Khi Người chơi Online:
Khi game khởi động hoặc khi người chơi online, ExpeditionManager.Tick() (hoặc một hàm CheckCompletedExpeditions() được gọi một lần khi khởi động) sẽ chạy.
Nó sẽ duyệt qua danh sách PlayerData.ActiveExpeditions.
Với mỗi chuyến đi, nó kiểm tra: if (currentTime >= expedition.completionTimestamp).
Nếu điều kiện đúng:
Chuyến đi đã hoàn thành.
Lấy ExpeditionReport đã được tính sẵn từ trong ActiveExpedition.
Chuyển báo cáo này vào PlayerData.UnclaimedReports (Hộp thư).
Xóa chuyến đi khỏi danh sách ActiveExpeditions.
Phát ra sự kiện OnExpeditionFinished để WorldMapController xóa GameObject xe ngựa và OnNewReportReceived để Hộp thư hiển thị badge.
Lưu lại PlayerData.
3. Tác động và Thay đổi Kỹ thuật
3.1. File cần sửa đổi: ExpeditionManager.cs
Hàm StartExpedition(): Đây sẽ là nơi thực hiện toàn bộ logic nặng.
Sẽ gọi CombatSystem.Simulate() ngay bên trong nó.
Sẽ tính toán toàn bộ thời gian.
Sẽ tạo ra ExpeditionReport đầy đủ.
Sẽ tạo và lưu ActiveExpedition vào PlayerData.
Hàm Tick(): Logic của nó sẽ được đơn giản hóa rất nhiều.
Nó không còn là một máy trạng thái phức tạp (Traveling, Exploring...).
Chức năng chính của nó chỉ là kiểm tra completionTimestamp của các chuyến đi đang hoạt động và chuyển báo cáo vào Hộp thư khi đến giờ.
3.2. Cấu trúc Dữ liệu ActiveExpedition
Cần định nghĩa hoặc sửa đổi lớp này để nó có thể chứa kết quả đã được tính toán trước.
code
C#
[System.Serializable]
public class ActiveExpedition
{
    public string expeditionId;
    public List<string> heroIds;
    public string poiId;
    public long completionTimestamp;

    // Trường quan trọng nhất: Kết quả đã được tính toán và đang chờ
    public ExpeditionReport preCalculatedReport;
}
3.3. File cần sửa đổi: WorldMapController.cs
Logic hiển thị xe ngựa không cần thay đổi nhiều. Nó vẫn có thể lắng nghe OnExpeditionStarted và OnExpeditionFinished. Tuy nhiên, các trạng thái trung gian như OnExpeditionReturning có thể không còn cần thiết nữa, hoặc chỉ là một sự kiện được phát ra dựa trên tính toán thời gian để lật chiều di chuyển của xe ngựa.
4. Lợi ích của Cách tiếp cận này
Chống Gian lận (Anti-Cheat): Vì kết quả được tính ngay lập tức dựa trên trạng thái của người chơi tại thời điểm bắt đầu, người chơi không thể thay đổi đội hình hay trang bị giữa chừng để ảnh hưởng đến kết quả.
Độ tin cậy Cao: Kể cả khi game bị crash hoặc người chơi mất kết nối, chuyến đi vẫn được lưu an toàn. Lần đăng nhập tiếp theo, hệ thống sẽ kiểm tra và trao kết quả một cách chính xác.
Tối ưu Hiệu năng: Các tính toán nặng (chiến đấu) chỉ xảy ra một lần duy nhất tại một thời điểm người chơi chủ động yêu cầu. Các lần kiểm tra sau đó chỉ là so sánh thời gian, rất nhẹ nhàng.
Trải nghiệm Người dùng Mượt mà: Người chơi có thể tự tin đóng game và biết rằng các hoạt động của họ vẫn đang tiến triển, tạo ra động lực để quay lại kiểm tra kết quả.


quái vật di chuyển thay vì dungeon, độ khó dựa vào khoảng cách, càng xa càng khó
khoảng cách spawn 400px nhiều lên

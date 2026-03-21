Dưới đây là một kế hoạch chi tiết để xây dựng toàn bộ hệ thống này, bám sát vào kiến trúc hiện có của dự án.
1. Tầm nhìn & Mục tiêu
Tầm nhìn: Biến Đấu trường thành một chế độ chơi cạnh tranh theo mùa (seasonal), nơi người chơi nỗ lực leo hạng để khẳng định sức mạnh đội hình và nhận những phần thưởng giá trị.
Mục tiêu:
Tạo ra một hệ thống Xếp hạng (Rank) rõ ràng, có tính tiến triển.
Cung cấp động lực tham gia liên tục thông qua phần thưởng sau mỗi trận đấu.
Tạo ra mục tiêu dài hạn bằng phần thưởng tuần dựa trên thứ hạng đạt được.
Quản lý lượt tham gia thông qua hệ thống vé hàng ngày.
2. Cơ chế Chi tiết
2.1. Bậc Xếp hạng (Ranks) và Điểm (Points)
Hệ thống sẽ dựa trên Điểm Xếp hạng (ĐXH). Người chơi sẽ được xếp vào các bậc hạng dựa trên số điểm họ có.
Các Bậc Xếp hạng:
Đồng (0 - 999 ĐXH)
Bạc (1000 - 1999 ĐXH)
Vàng (2000 - 2999 ĐXH)
Bạch Kim (3000 - 3999 ĐXH)
Kim Cương (4000 - 4999 ĐXH)
Thách Đấu (Top 200 người chơi cao điểm nhất)
Cơ chế Tính điểm:
Thắng: Nhận được +X điểm.
Thua: Bị trừ -Y điểm (có thể không trừ điểm ở hạng Đồng).
Logic Nâng cao: Số điểm nhận/mất sẽ phụ thuộc vào chênh lệch ĐXH giữa hai người chơi. Thắng người có hạng cao hơn sẽ được nhiều điểm hơn, và thua người có hạng thấp hơn sẽ bị trừ nhiều điểm hơn.
2.2. Vé Đấu trường (Arena Tickets)
Cơ chế:
Cần 1 vé để tham gia một trận Đấu trường.
Mỗi ngày vào một thời điểm cố định (ví dụ: 00:00 UTC), hệ thống sẽ bổ sung vé cho người chơi để đạt lại mốc 5 vé.
Ví dụ: Nếu người chơi còn 2 vé, hệ thống sẽ cấp thêm 3 vé. Nếu người chơi còn 5 vé, họ sẽ không nhận được gì.
Nguồn vé khác: Có thể mua thêm vé bằng tiền tệ cao cấp hoặc nhận từ các sự kiện đặc biệt.
2.3. Hệ thống Phần thưởng (Rewards)
Đây là trái tim của hệ thống, được chia làm hai loại như bạn đề xuất.
a) Thưởng Nóng (Sau mỗi trận):
Mục đích: Tạo cảm giác thỏa mãn tức thì, đảm bảo mỗi trận đấu đều có giá trị.
Loại phần thưởng:
"Huy hiệu Đấu trường" (Arena Coins): Một loại tiền tệ mới, chỉ có thể kiếm được từ Đấu trường.
Kinh nghiệm cho các hero tham chiến.
Logic:
Thắng: Nhận 30 Huy hiệu + 100% EXP.
Thua: Nhận 10 Huy hiệu + 50% EXP.
b) Thưởng Tuần (Dựa trên Xếp hạng):
Mục đích: Đặt ra mục tiêu lớn, khuyến khích người chơi nỗ lực leo hạng cao nhất có thể.
Thời gian: Chốt hạng và phát thưởng vào một ngày cố định trong tuần (ví dụ: 23:59 Chủ Nhật hàng tuần). Sau đó có thể reset một phần điểm để bắt đầu mùa mới.
Loại phần thưởng (ví dụ):
Bậc Xếp hạng	Kim Cương (Premium Currency)	Huy hiệu Đấu trường	Vật phẩm đặc biệt
Đồng	50	500	-
Bạc	100	1000	-
Vàng	200	2000	1x Thuốc Biến Dị
Bạch Kim	350	3500	3x Thuốc Biến Dị
Kim Cương	500	5000	1x Bùa Ước Nguyện
Thách Đấu	1000	10000	1x Bùa Ước Nguyện + Title
2.4. Cửa hàng Đấu trường (Arena Shop)
Để làm cho "Huy hiệu Đấu trường" có giá trị, cần có một cửa hàng riêng nơi người chơi có thể dùng chúng để đổi lấy những vật phẩm độc quyền không thể mua bằng Vàng, ví dụ:
Trang bị hiếm.
Vật phẩm tiến hóa.
Hero độc quyền của Đấu trường.
3. Kế hoạch Triển khai Kỹ thuật
Bước 1: Cập nhật Cấu trúc Dữ liệu
File cần sửa đổi: PlayerData.cs
Thêm các trường mới để lưu trữ trạng thái Đấu trường của người chơi:
code
C#
public int arenaPoints; // Điểm xếp hạng hiện tại
public int arenaTickets; // Số vé hiện có
public long lastTicketRefreshTimestamp; // Thời điểm cuối cùng được nhận vé
public int arenaCoins; // Số Huy hiệu Đấu trường
File cần tạo mới: ArenaRankData.cs
Loại: ScriptableObject.
Nội dung: Định nghĩa một bậc xếp hạng, bao gồm: rankName (Vàng), minPoints, maxPoints, rankIconSprite.
Mục đích: Tạo một danh sách các file asset (Dong.asset, Bac.asset,...) để ArenaPanel đọc và hiển thị thông tin hạng của người chơi.
Bước 2: Xây dựng Logic Hệ thống
File cần tạo mới: ArenaSystem.cs (Manager Logic)
Loại: Singleton Manager, tương tự HospitalSystem.
Nhiệm vụ:
CheckDailyTicketRefresh(): Kiểm tra lastTicketRefreshTimestamp và bổ sung vé nếu cần. Hàm này nên được gọi bởi GameManager mỗi khi game khởi động.
FindOpponent(int playerPoints): Logic tìm đối thủ. Ban đầu, có thể giả lập bằng cách tạo ra một đội quân quái vật có sức mạnh tương đương với người chơi. Về sau, có thể lấy dữ liệu đội hình của một người chơi khác có điểm số tương tự.
ProcessMatchResult(CombatResult result, int playerPoints, int opponentPoints):
Trừ 1 vé của người chơi.
Tính toán điểm ĐXH nhận/mất dựa trên kết quả và chênh lệch điểm.
Cập nhật playerData.arenaPoints.
Cộng "Thưởng Nóng" (Huy hiệu, EXP) vào cho người chơi.
DistributeWeeklyRewards(): Logic phát thưởng tuần (sẽ được gọi bởi một hệ thống quản lý thời gian).
Bước 3: Nâng cấp Giao diện
File cần sửa đổi: ArenaPanel.cs
Thêm các thành phần UI mới:
Text để hiển thị: Tên hạng, Điểm Xếp hạng, số Vé.
Image để hiển thị icon của bậc hạng.
Button "Tìm trận", "Bảng Xếp hạng", "Cửa hàng".
Cập nhật Logic:
Khi mở panel, đọc dữ liệu từ PlayerData để hiển thị thông tin.
Nút "Tìm trận" sẽ gọi ArenaSystem.Instance.FindOpponent(), sau đó mở SquadSelectionPanel.
Callback từ SquadSelectionPanel sẽ bắt đầu trận đấu.
Sau trận đấu, thay vì quay lại ArenaPanel ngay lập tức, sẽ hiển thị một Popup Kết quả Trận đấu (có thể tái sử dụng hoặc tạo mới) để thông báo: Thắng/Thua, Số điểm thay đổi, Phần thưởng nóng nhận được.
File cần tạo mới: ArenaShopPanel.cs
Giao diện cửa hàng, nơi người chơi tiêu arenaCoins.
Danh mục Vật phẩm trong Cửa hàng Đấu trường
Loại 1: Vật phẩm Tăng cường Vĩnh viễn (Hấp dẫn nhất)
Đây là những vật phẩm người chơi sẽ khao khát nhất vì chúng mang lại sức mạnh lâu dài. Chúng nên rất đắt và có giới hạn mua.
Thuốc Tăng trưởng Chỉ số (Stat Growth Potions):
Mô tả: Vật phẩm tiêu thụ, sử dụng lên một hero cụ thể để cộng vĩnh viễn vào chỉ số GỐC (baseStats) của hero đó. Điều này cực kỳ mạnh vì nó sẽ được nhân lên bởi các hiệu ứng MULTIPLY_STAT từ Trait.
Các loại:
Thuốc Sức mạnh: +5 ATK gốc.
Thuốc Dẻo dai: +15 HP gốc.
Thuốc Bền bỉ: +5 DEF gốc.
Thuốc Nhanh nhẹn: +2 SPD gốc.
Giới hạn: Mỗi hero chỉ có thể sử dụng tối đa 5 lọ thuốc mỗi loại trong đời. (Để tránh một hero trở nên quá bá đạo).
Giá: Rất đắt (ví dụ: 5,000 Huy hiệu Đấu trường/lọ).
Sách Khai phá Tiềm năng (Potential Unlock Tome):
Mô tả: Sử dụng lên một hero để tăng vĩnh viễn chỉ số potential của hero đó thêm một lượng nhỏ.
Các loại:
Sách Sơ cấp: +1 potential.
Sách Trung cấp: +3 potential.
Giới hạn: Giới hạn mua 1 Sách mỗi tuần.
Giá: Cực kỳ đắt (ví dụ: 10,000 Huy hiệu cho Sách Sơ cấp).
Vé Tái tạo Trait (Trait Reroll Ticket):
Mô tả: Một vật phẩm cực hiếm, cho phép người chơi chọn một Trait trên một hero và quay ngẫu nhiên ra một Trait khác. Không đảm bảo sẽ ra Trait xịn hơn.
Giới hạn: Chỉ có thể mua 1 vé mỗi mùa giải Đấu trường.
Giá: Siêu đắt, là phần thưởng cuối cùng cho những người chơi chuyên cần.
Loại 2: Vật phẩm Tiêu thụ Hữu ích (Giá trị trung bình)
Đây là các vật phẩm mà người chơi sẽ thường xuyên mua để đẩy nhanh tiến độ.
Vật phẩm Tăng tốc (Speed-Up Items):
Mô tả: Đúng như bạn đề xuất, đây là các vật phẩm giúp giảm thời gian chờ.
Các loại:
Đồng hồ Cát (1 giờ): Giảm 1 giờ cho bất kỳ quá trình chờ nào (xây dựng, trưởng thành, hồi phục).
Đồng hồ Cát (8 giờ): Giảm 8 giờ.
Giới hạn: Giới hạn mua hàng ngày (ví dụ: 5 cái 1 giờ, 1 cái 8 giờ mỗi ngày).
Giá: Vừa phải.
Thuốc Biến Dị (Mutation Potion):
Mô tả: Vật phẩm đã có trong thiết kế. Tăng tỉ lệ đột biến khi lai tạo.
Giới hạn: Không giới hạn. Đây là cách tốt để người chơi tiêu thụ Huy hiệu còn thừa.
Giá: Rẻ.
Bùa Ước Nguyện Nghề nghiệp (Class Wish Charm):
Mô tả: Vật phẩm đã có trong thiết kế. Tăng tỉ lệ con cái sinh ra có một nghề nghiệp nhất định.
Các loại: Bùa Chiến binh, Bùa Cung thủ,...
Giới hạn: Không giới hạn.
Giá: Rẻ.
Hero Độc quyền của Đấu trường:
Mô tả: Một hoặc hai hero đặc biệt với bộ kỹ năng độc đáo, chỉ có thể mua được bằng cách tích lũy một lượng lớn Huy hiệu Đấu trường. Đây sẽ là động lực cày cuốc lớn nhất.
Ví dụ: "Đấu sĩ Vô địch" - một hero hệ Warrior với các kỹ năng chuyên về phản đòn và chống chịu.
Giới hạn: Mua 1 lần duy nhất.
Giá: Cần vài tuần hoặc cả tháng cày cuốc mới đủ mua.
Tổng kết Bảng giá (Ví dụ)
Vật phẩm	Giá (Huy hiệu Đấu trường)	Giới hạn Mua
Thuốc Sức mạnh (+5 ATK)	5,000	5 lần/hero
Sách Khai phá (+1 Potential)	10,000	1 lần/tuần
Vé Tái tạo Trait	50,000	1 lần/mùa
Đồng hồ Cát (1 giờ)	200	5 lần/ngày
Thuốc Biến Dị	500	Không giới hạn
Hero "Đấu sĩ Vô địch"	25,000	1 lần duy nhất
đội hình do bot tạo ra theo rank và dựa trên base CP

# Kế hoạch Nâng cấp Toàn diện: Lối chơi Chiến lược & Tùy biến Anh hùng

**Tài liệu này là bản kế hoạch chi tiết và duy nhất, chứa tất cả thông tin cần thiết để thực hiện. Người thực hiện không cần tham chiếu đến bất kỳ file GDD nào khác.**

---

## Giai đoạn 1: Đại tu Hệ thống Anh hùng & Chỉ số

*Mục tiêu: Thay đổi nền tảng cốt lõi của anh hùng, chuyển từ hệ thống chỉ số di truyền phức tạp sang một mô hình tập trung vào "Potential" (POT) và sự tùy biến của người chơi.*

### **Nhiệm vụ 1.1: Tái cấu trúc `HeroData.cs` và Chỉ số**

*   **Mục tiêu:** Thay đổi cấu trúc dữ liệu của anh hùng để phản ánh hệ thống POT mới.
*   **Thông tin tham khảo:**
    *   **File cần sửa:** `HeroData.cs`
    *   **Mô hình chỉ số mới:**
        *   `potential` (POT): Chỉ số di truyền duy nhất (int, 1-20).
        *   `baseStats`: Các chỉ số (HP, ATK, DEF, SPD) được tính 1 lần lúc sinh ra. **Không bao giờ thay đổi.**
        *   `addedStats`: Các điểm chỉ số do người chơi cộng vào khi lên cấp.
        *   `freeStatPoints`: Số điểm chờ được phân phối.
*   **Các bước thực hiện:**
    1.  Mở file `HeroData.cs`.
    2.  **Xóa** các trường `baseStats` cũ nếu chúng được thiết kế để thay đổi khi lên cấp.
    3.  **Thêm** các trường sau:
        ```csharp
        // Chỉ số di truyền cốt lõi
        public int potential; 

        // Chỉ số gốc, tính 1 lần lúc sinh ra
        public HeroStats baseStats; 

        // Điểm cộng từ người chơi
        public HeroStats addedStats; 

        // Điểm chờ phân phối
        public int freeStatPoints;

        // Các chỉ số chiến đấu mới
        public float evasionRate;       // % Né tránh
        public float damageReduction;   // % Giảm sát thương
        public float damageIncrease;    // % Tăng sát thương
        ```
    4.  Định nghĩa struct `HeroStats` nếu chưa có, đảm bảo nó chứa `hp`, `atk`, `def`, `spd`.
    5.  Tạo một hàm mới `GetFinalStats()` sẽ được dùng trong `CombatSystem`:
        ```csharp
        public HeroStats GetFinalStats()
        {
            HeroStats finalStats = new HeroStats();
            finalStats.hp = baseStats.hp + addedStats.hp; // + bonus từ Trait
            finalStats.atk = baseStats.atk + addedStats.atk; // + bonus từ Trait
            finalStats.def = baseStats.def + addedStats.def; // + bonus từ Trait
            finalStats.spd = baseStats.spd + addedStats.spd; // + bonus từ Trait
            // Lưu ý: Logic cộng bonus từ Trait sẽ được thêm sau.
            return finalStats;
        }
        ```

### **Nhiệm vụ 1.2: Cập nhật Logic Tạo Anh hùng**

*   **Mục tiêu:** Sửa đổi hệ thống tạo anh hùng (`BreedingSystem`, `RecruitmentSystem`) để tính toán `baseStats` dựa trên POT.
*   **Thông tin tham khảo:**
    *   **File cần sửa:** `BreedingSystem.cs`, `RecruitmentSystem.cs` (cần tạo mới).
    *   **Công thức:** `BaseStat = POT * Random.Range(8, 11)` (sử dụng 11 vì `Random.Range` cho int là độc quyền ở max).
*   **Các bước thực hiện:**
    1.  Trong `BreedingSystem.cs` (và sau này là `RecruitmentSystem.cs`), sau khi `potential` của hero con được xác định, hãy gọi một hàm mới `CalculateBaseStats(HeroData newHero)`.
    2.  Triển khai hàm `CalculateBaseStats`:
        ```csharp
        private void CalculateBaseStats(HeroData newHero)
        {
            newHero.baseStats = new HeroStats();
            newHero.baseStats.hp = newHero.potential * Random.Range(8, 11);
            newHero.baseStats.atk = newHero.potential * Random.Range(8, 11);
            newHero.baseStats.def = newHero.potential * Random.Range(8, 11);
            newHero.baseStats.spd = newHero.potential * Random.Range(8, 11);

            // Khởi tạo các giá trị khác
            newHero.addedStats = new HeroStats(); // Bắt đầu bằng 0
            newHero.freeStatPoints = 0;
            newHero.evasionRate = 0f;
            newHero.damageReduction = 0f;
            newHero.damageIncrease = 0f;
        }
        ```

### **Nhiệm vụ 1.3: Triển khai Hệ thống Phân phối Điểm khi Lên cấp**

*   **Mục tiêu:** Thay thế việc tự động tăng chỉ số bằng việc cho người chơi `freeStatPoints` để tự phân phối.
*   **Thông tin tham khảo:**
    *   **File cần sửa:** `HeroData.cs` (hàm `GainExp`), `HeroInfoPanel.cs` (UI).
    *   **Logic:** Khi lên cấp, `freeStatPoints += potential`.
*   **Các bước thực hiện:**
    1.  Mở `HeroData.cs` và tìm đến hàm `GainExp(int amount)`.
    2.  Bên trong vòng lặp `while (experience >= requiredExp)`, sau khi `level++`, **xóa bỏ hoàn toàn logic cộng chỉ số cũ** và thay bằng:
        ```csharp
        this.freeStatPoints += this.potential;
        ```
    3.  **Tạo UI Panel mới `StatAllocationPanel.cs`:**
        *   Panel này được mở từ `HeroInfoPanel`.
        *   Hiển thị `freeStatPoints` và 4 dòng cho HP, ATK, DEF, SPD. Mỗi dòng có tên chỉ số, giá trị hiện tại (`base + added`), và các nút `+` / `-`.
        *   Nhấn `+` sẽ tăng `addedStat` tương ứng và giảm `freeStatPoints`.
        *   Thêm nút "Xác nhận" để lưu lại thay đổi vào `HeroData`.
    4.  Trong `HeroInfoPanel.cs`, thêm một nút "Phân phối điểm" chỉ hiển thị khi `hero.freeStatPoints > 0`. Nút này sẽ mở `StatAllocationPanel`.

---

## Giai đoạn 2: Đại tu Hệ thống Trait

*Mục tiêu: Xây dựng một hệ thống Trait có chiều sâu, minh bạch, loại bỏ may rủi tiêu cực và tạo ra sự tiến triển có ý nghĩa.*

### **Nhiệm vụ 2.1: Tái cấu trúc Dữ liệu `Trait.cs`**

*   **Mục tiêu:** Mở rộng `ScriptableObject` của Trait để hỗ trợ các cơ chế mới.
*   **Thông tin tham khảo:**
    *   **File cần sửa:** `Trait.cs` (hoặc file định nghĩa ScriptableObject của Trait).
*   **Các bước thực hiện:**
    1.  Mở file `Trait.cs`.
    2.  **Thêm** các trường sau:
        ```csharp
        public enum RarityRank { D, C, B, A, S }

        [Header("Trait Evolution")]
        public RarityRank rank;
        public string familyId; // Ví dụ: "ATK_UP", "HP_ON_HIT"
        public string nextUpgradeTraitID; // ID của Trait kế tiếp trong cùng family
        ```

### **Nhiệm vụ 2.2: Triển khai Logic Nhận Trait Mới (Cấp 20, 40, 80)**

*   **Mục tiêu:** Anh hùng nhận Trait mới thuộc một "gia đình" (family) mà họ chưa có, đảm bảo không trùng lặp chức năng.
*   **Thông tin tham khảo:**
    *   **File cần sửa:** `EvolutionSystem.cs`.
    *   **Cơ chế "2 Lượt Quay":**
        1.  **Lượt 1 (Nội dung):** Chọn một `familyId` mà hero chưa sở hữu.
        2.  **Lượt 2 (Chất lượng):** Quay tỉ lệ để ra bậc hiếm `RarityRank`.
*   **Các bước thực hiện:**
    1.  Tạo một lớp quản lý dữ liệu `TraitDatabase.cs` (nếu chưa có). Lớp này phải có khả năng:
        *   Tải tất cả `Trait.cs` từ `Resources`.
        *   Cung cấp hàm `GetTraitByFamilyAndRank(string familyId, RarityRank rank)`.
        *   Cung cấp hàm `GetAllFamilyIDs()`.
    2.  Mở `EvolutionSystem.cs` và sửa logic `OnHeroLeveledUp`.
    3.  Tại các mốc cấp 20, 40, 80, gọi hàm mới `GrantNewTrait_TwoRolls(HeroData hero)`.
    4.  Triển khai `GrantNewTrait_TwoRolls`:
        ```csharp
        private void GrantNewTrait_TwoRolls(HeroData hero)
        {
            // 1. Lọc ra các family hero chưa có
            var allFamilies = TraitDatabase.GetAllFamilyIDs();
            var ownedFamilies = hero.traitIDs.Select(id => TraitDatabase.GetTraitByID(id).familyId).ToHashSet();
            var unownedFamilyPool = allFamilies.Where(f => !ownedFamilies.Contains(f)).ToList();

            if (unownedFamilyPool.Count == 0) return; // Không còn family nào để nhận

            // 2. Lượt quay 1: Chọn nội dung
            string chosenFamily = unownedFamilyPool[Random.Range(0, unownedFamilyPool.Count)];

            // 3. Lượt quay 2: Chọn chất lượng
            RarityRank chosenRank = RollForRarity(); // Hàm quay tỉ lệ D-S

            // 4. Kết hợp và trao thưởng
            Trait newTrait = TraitDatabase.GetTraitByFamilyAndRank(chosenFamily, chosenRank);
            if (newTrait != null)
            {
                hero.traitIDs.Add(newTrait.id);
            }
        }
        ```

### **Nhiệm vụ 2.3: Triển khai Logic Nâng cấp Trait (Cấp 60, 100)**

*   **Mục tiêu:** Cho phép người chơi chọn và nâng cấp một Trait hiện có lên bậc hiếm cao hơn.
*   **Thông tin tham khảo:**
    *   **File cần sửa:** `EvolutionSystem.cs`, `HeroInfoPanel.cs`.
*   **Các bước thực hiện:**
    1.  Trong `EvolutionSystem.cs`, tại mốc cấp 60 và 100, không làm gì cả. Thay vào đó, `HeroInfoPanel` sẽ tự xử lý.
    2.  **Tạo UI Panel mới `TraitUpgradePanel.cs`**.
    3.  Trong `HeroInfoPanel.cs`:
        *   Thêm một nút "Nâng cấp Trait" chỉ hiển thị khi `(hero.level == 60 || hero.level == 100)` và việc nâng cấp chưa được thực hiện.
        *   Nút này sẽ mở `TraitUpgradePanel`, truyền vào `hero.traitIDs`.
    4.  Trong `TraitUpgradePanel.cs`:
        *   Hiển thị danh sách các Trait của hero.
        *   Đối với mỗi Trait, kiểm tra `nextUpgradeTraitID`. Nếu nó hợp lệ và không rỗng, nút "Nâng cấp" sẽ hiện ra.
        *   Khi người chơi chọn nâng cấp một Trait, tìm ID của Trait đó trong `hero.traitIDs` và thay thế nó bằng `nextUpgradeTraitID`.
        *   Lưu lại thay đổi và đóng panel.

### **Nhiệm vụ 2.4: Đại tu Di truyền Trait trong `BreedingSystem`**

*   **Mục tiêu:** Triển khai cơ chế di truyền 3 Trait (1 từ Cha, 1 từ Mẹ, 1 ngẫu nhiên) với luật "an toàn".
*   **Thông tin tham khảo:**
    *   **File cần sửa:** `BreedingSystem.cs`.
    *   **Luật chơi:**
        *   **Trait từ Cha/Mẹ:** Quay ra bậc hiếm mục tiêu. Nếu cha/mẹ có Trait ở bậc đó -> nhận ngẫu nhiên. Nếu không -> nhận Trait có bậc hiếm thấp nhất của cha/mẹ.
        *   **Trait ngẫu nhiên:** Quay ra bậc hiếm và lấy một Trait từ toàn bộ game.
        *   **Chống trùng lặp:** Cả 3 Trait cuối cùng phải là duy nhất.
*   **Các bước thực hiện:**
    1.  Trong `BreedingSystem.cs`, thay thế logic `InheritTraits` cũ bằng hàm mới.
    2.  Sử dụng mã giả chi tiết đã được cung cấp trong file kế hoạch gốc để triển khai 3 hàm: `InheritTraits(father, mother)`, `GetTraitFromParent(parent)`, và `RollForRarity()`. Logic này đảm bảo con nhận 1 Trait từ cha, 1 từ mẹ (không trùng), và 1 Trait ngẫu nhiên (không trùng với 2 cái trước).

---

## Giai đoạn 3: Hoàn thiện Vòng lặp Gameplay & Hướng dẫn

*Mục tiêu: Xây dựng các hệ thống phụ trợ để tạo ra một vòng lặp chơi game bền vững và dẫn dắt người chơi một cách hiệu quả.*

### **Nhiệm vụ 3.1: Triển khai Hệ thống Tuyển mộ (Gacha)**

*   **Mục tiêu:** Tạo một cách để người chơi có được anh hùng mới ngoài việc lai tạo.
*   **Thông tin tham khảo:**
    *   **File cần tạo:** `RecruitmentSystem.cs`.
    *   **Tỉ lệ POT:** D (60%), C (30%), B (6.5%), A (3%), S (0.5%).
*   **Các bước thực hiện:**
    1.  Tạo `RecruitmentSystem.cs`.
    2.  Tạo hàm `PerformRecruitment(int amount)`.
    3.  Bên trong hàm, lặp `amount` lần:
        *   Quay số để xác định bậc hiếm của POT.
        *   Dựa vào bậc hiếm, random một giá trị POT trong khoảng tương ứng (ví dụ: B -> 9-12).
        *   Gọi hàm `CalculateBaseStats()` đã tạo ở Giai đoạn 1.
        *   Thực hiện logic di truyền Trait (có thể tái sử dụng logic từ `BreedingSystem` nhưng chỉ cần 3 Trait ngẫu nhiên theo tỉ lệ).
        *   Thêm hero mới vào `DataManager`.
    4.  Tạo `RecruitmentPanel.cs` để người chơi có thể thực hiện việc tuyển mộ.

### **Nhiệm vụ 3.2: Nâng cấp Hệ thống Công trình & Dân số**

*   **Mục tiêu:** Hoàn thiện logic nâng cấp nhà và giới hạn dân số.
*   **Thông tin tham khảo:**
    *   **File cần sửa:** `BuildingSystem.cs`.
    *   **Cân bằng:** 1h -> lv5, 10h -> lv13, 1 tuần -> lv20.
*   **Các bước thực hiện:**
    1.  **Tạo `BuildingUpgradeData.cs` (ScriptableObject):** Chứa một danh sách hoặc dictionary, map `buildingId` và `level` tới `cost` và `duration`.
    2.  Trong `BuildingSystem.cs`, khi nâng cấp, đọc chi phí và thời gian từ `BuildingUpgradeData` thay vì hard-code.
    3.  **Logic Dân số:**
        *   Trong `GameManager` hoặc một nơi tương tự, khi nhận hero mới (từ lai tạo/tuyển mộ), kiểm tra `DataManager.Instance.Player.Heroes.Count >= maxPopulation`.
        *   `maxPopulation` được tính dựa trên cấp độ của Nhà chính.
        *   Nếu quá tải, mở một `PopulationManagerPanel.cs` mới, buộc người chơi phải chọn một hero để "sa thải" trước khi hero mới được thêm vào.

### **Nhiệm vụ 3.3: Xây dựng Hệ thống Nhiệm vụ**

*   **Mục tiêu:** Dẫn dắt người chơi mới và tạo mục tiêu ngắn hạn/dài hạn.
*   **Thông tin tham khảo:**
    *   **File cần tạo:** `QuestManager.cs`, `QuestData.cs` (ScriptableObject).
*   **Các bước thực hiện:**
    1.  Tạo `QuestData.cs` để định nghĩa một nhiệm vụ: ID, mô tả, loại (ví dụ: `UPGRADE_BUILDING`, `BREED_HERO`), mục tiêu, phần thưởng.
    2.  Tạo `QuestManager.cs`:
        *   Quản lý danh sách nhiệm vụ của người chơi.
        *   Lắng nghe các sự kiện game (ví dụ: `EventManager.OnBuildingUpgraded`, `EventManager.OnHeroBorn`).
        *   Khi một sự kiện xảy ra, kiểm tra xem nó có khớp với điều kiện của nhiệm vụ nào không và cập nhật tiến độ.
    3.  Tạo `QuestPanel.cs` để hiển thị danh sách nhiệm vụ và cho người chơi nhận thưởng.
    4.  **Logic Khóa Tính năng:**
        *   Trong `UIManager.cs`, thêm các biến bool `isArenaUnlocked`, `isTowerUnlocked`.
        *   Trong hàm `Update()`, kiểm tra các điều kiện (ví dụ: `Time.timeSinceLevelLoad > 1800 && DataManager.Instance.GetBuildingLevel("MainHall") >= 5`).
        *   Nếu điều kiện được đáp ứng, đặt cờ thành `true` và kích hoạt nút tương ứng trên UI.
Kế hoạch Hành động: Hoàn thiện Bản Nâng cấp Gameplay
Mục tiêu: Triển khai tất cả các hạng mục được đánh dấu "Chưa triển khai" hoặc "Chưa hoàn thành" trong báo cáo.
Phần 1: Hoàn thiện Hệ thống Anh hùng & Chỉ số
Nhiệm vụ: Triển khai logic và giao diện cho phép người chơi thiết lập tỉ lệ và tự động phân phối freeStatPoints.
Các bước thực hiện:
HeroData.cs: Thêm một trường Dictionary<StatType, float> autoAssignRatios; để lưu tỉ lệ (ví dụ: StatType là enum HP, ATK, DEF, SPD).
StatAllocationPanel.cs:
Tạo một popup mới AutoAssignPanel được mở từ nút "Tự động cộng".
AutoAssignPanel chứa 4 slider, tổng giá trị của chúng luôn là 100. Khi người chơi thay đổi một slider, các slider khác tự điều chỉnh.
Khi nhấn "Xác nhận", lưu các giá trị tỉ lệ vào hero.autoAssignRatios.
HeroData.AddExperience():
Sau khi freeStatPoints được cộng thêm, kiểm tra xem autoAssignRatios có được thiết lập hay không.
Nếu có, hãy lặp qua Dictionary tỉ lệ, tính toán số điểm cần cộng cho mỗi chỉ số dựa trên freeStatPoints mới, cộng chúng vào addedStats, và reset freeStatPoints về 0 (hoặc số dư nếu có phép chia lẻ).
Nhiệm vụ: Cập nhật UI để hiển thị 3 chỉ số Né, Giảm ST, Tăng ST.
Các bước thực hiện:
HeroInfoPanel.cs:
Thêm 3 [SerializeField] private TextMeshProUGUI evasionText, damageReductionText, damageIncreaseText;.
Trong hàm UpdateUI(HeroData hero), thêm code để cập nhật giá trị cho 3 Text này từ hero.evasionRate, hero.damageReduction, hero.damageIncrease. Giá trị cần được nhân với 100 và thêm ký tự %.
Unity Editor: Kéo các đối tượng TextMeshProUGUI tương ứng từ Prefab HeroInfoPanel vào các trường vừa tạo trong script.
Phần 2: Đại tu Hệ thống Trait
Nhiệm vụ: Viết lại hoàn toàn logic di truyền trong BreedingSystem.cs để tuân theo kế hoạch update_planv2.md.
Các bước thực hiện:
BreedingSystem.cs:
Xóa logic di truyền cũ.
Triển khai hàm InheritTraits(father, mother) mới, tuân thủ chính xác 3 quy tắc:
Quy tắc 1: Thừa kế từ Cha (có luật "an toàn").
Quy tắc 2: Thừa kế từ Mẹ (có luật "an toàn" và chống trùng lặp với Cha).
Quy tắc 3: Trait ngẫu nhiên (chống trùng lặp với cả Cha và Mẹ).
Tạo các hàm phụ GetTraitFromParent(parent) và RollForRarity() để làm cho code sạch sẽ hơn (tham khảo kế hoạch chi tiết đã thảo luận).
Nhiệm vụ: Viết lại EvolutionSystem.cs để triển khai logic "2 lượt quay".
Các bước thực hiện:
EvolutionSystem.cs:
File này không còn là "lớp rỗng". Nó phải lắng nghe sự kiện HeroData.OnHeroLeveledUp.
Trong hàm xử lý sự kiện, kiểm tra if (hero.level == 20 || hero.level == 40 || hero.level == 80).
Nếu đúng, gọi hàm GrantNewTrait_TwoRolls(hero).
Triển khai hàm này theo đúng logic: Lọc Gia đình Trait chưa có -> Quay lần 1 chọn Gia đình -> Quay lần 2 chọn Độ hiếm -> Kết hợp và trao Trait.
Phần 3: Đại tu Hệ thống Nghề nghiệp & Lai tạo
Nhiệm vụ: Thay đổi thời điểm và cách thức nhận nghề.
Các bước thực hiện:
MaturationSystem.cs: Xóa hoàn toàn dòng code gán nghề nghiệp ngẫu nhiên khi trưởng thành.
EvolutionSystem.cs:
Trong hàm xử lý sự kiện OnHeroLeveledUp, thêm một điều kiện if (hero.level == 20).
Nếu đúng, mở một UI Panel mới ProfessionSelectionPanel.
ProfessionSelectionPanel.cs (Mới): Hiển thị các lựa chọn nghề. Khi người chơi xác nhận, panel này sẽ gọi lại và cập nhật trường hero.profession.Nhiệm vụ: Triển khai logic và UI cho 3 Tháp Thử thách.
Các bước thực hiện:
WorldMapController.cs:
Trong hàm InitializeWorldMap(), thêm logic để tạo ra 3 POI Tháp cố định (Warrior, Archer, Mage) nếu chúng chưa tồn tại trong PlayerData. Các POI này không được tạo ngẫu nhiên.
POI_InfoPanel.cs: Khi hiển thị thông tin cho một POI loại "Tháp nghề", cần hiển thị rõ nghề yêu cầu.
SquadSelectionPanel.cs: Khi được gọi cho một "Tháp nghề", panel cần lọc danh sách anh hùng, chỉ cho phép chọn những người thuộc đúng nghề yêu cầu.
ExpeditionManager.cs: Logic chiến đấu vẫn như cũ, nhưng phần thưởng (LootData) phải bao gồm các loại tài nguyên nâng cấp nghề đặc biệt tương ứng.
Nhiệm vụ: Thêm cơ chế hồi chiêu 2 phút cho mỗi anh hùng sau khi lai tạo.
Các bước thực hiện:
HeroData.cs: Thêm một trường public long breedingCooldownEndTimestamp;.
BreedingSystem.cs:
Trong hàm Breed(), sau khi tạo ra con, hãy cập nhật trường breedingCooldownEndTimestamp cho cả cha và mẹ thành currentTime + 120 seconds.
BreedingUIController.cs (hoặc nơi chọn hero):
Khi hiển thị danh sách hero để chọn, lọc ra những hero có currentTime < breedingCooldownEndTimestamp. Những hero này sẽ bị làm mờ hoặc có một icon đồng hồ trên đó.
Nhiệm vụ: Thêm giới hạn 10 lần sinh sản cho mỗi hero và hiển thị trên UI.
Các bước thực hiện:
HeroData.cs: Thêm public int breedingCount; và public int maxBreedingLimit = 10;.
BreedingSystem.cs: Trong hàm Breed(), tăng breedingCount cho cả cha và mẹ lên 1.
BreedingUIController.cs: Lọc ra những hero đã có breedingCount >= maxBreedingLimit.
HeroInfoPanel.cs: Thêm một TextMeshProUGUI mới để hiển thị "Số lần sinh sản: [breedingCount] / [maxBreedingLimit]".
Phần 4: Hoàn thiện Hệ thống Công trình & Nhiệm vụ
Nhiệm vụ: Cung cấp dữ liệu cân bằng cụ thể cho coder.
Dữ liệu Cân bằng: Dưới đây là bảng dữ liệu gợi ý cho Nhà chính. Coder cần nhập dữ liệu này vào một ScriptableObject hoặc file cấu hình. "Thời gian xây dựng" chính là thời gian chờ nâng cấp.
Cấp	Tài nguyên Cần	Thời gian Xây dựng (giây)	Ghi chú Tiến độ
1->2	100 Vàng, 50 Gỗ	30	
2->3	200 Vàng, 100 Gỗ	90	
3->4	400 Vàng, 200 Gỗ	300 (5 phút)	
4->5	800 Vàng, 400 Gỗ	900 (15 phút)	Tổng thời gian ~1 giờ để đạt Cấp 5
5->6	1,500 Vàng, 800 Gỗ	1,800 (30 phút)	
...	...	...	(Tiếp tục tăng theo cấp số nhân)
12->13	50,000 Vàng, 25,000 Gỗ	10,800 (3 giờ)	Tổng thời gian ~10 giờ để đạt Cấp 13
...	...	...	(Tiếp tục tăng mạnh)
19->20	1,000,000 Vàng, 500,000 Gỗ	86,400 (24 giờ)	Tổng thời gian ~1 tuần để đạt Cấp 20
(Đây là bảng ví dụ, coder cần một bảng đầy đủ từ 1-20)			
Nhiệm vụ: Triển khai logic tăng số lượng cặp đôi lai tạo cùng lúc.
Các bước thực hiện:
PlayerData.cs: Thêm một trường public int maxConcurrentBreedingSlots;.
BuildingSystem.cs: Trong hàm OnUpgradeComplete(), kiểm tra if (building.id == "BreedingHall"). Nếu đúng, cập nhật PlayerData.maxConcurrentBreedingSlots dựa trên cấp độ mới của Nhà Sinh sản.
BreedingUIController.cs: Trước khi cho phép bắt đầu lai tạo, kiểm tra xem số lượng cặp đang lai tạo có nhỏ hơn PlayerData.maxConcurrentBreedingSlots hay không.
Nhiệm vụ: Hoàn thiện giao diện PopulationManagerPanel.
Các bước thực hiện:
PopulationManagerPanel.cs:
Trong hàm Show(), cần nhận vào một Action<HeroData> callback (hành động sa thải).
Đọc danh sách DataManager.Instance.AllHeroes và tạo ra các HeroCard trong một ScrollView.
Mỗi HeroCard phải có thêm một nút "Sa thải". Khi nhấn, một popup xác nhận sẽ hiện ra.
Nếu người chơi xác nhận sa thải, gọi hàm DataManager.RemoveHero(hero), đóng panel, và thực thi callback.
Nhiệm vụ: Phân loại và hiển thị các loại nhiệm vụ khác nhau.
Các bước thực hiện:
QuestData.cs: Cần có một enum QuestCategory { Daily, Weekly, Achievement }.
QuestManager.cs:
Thay vì một danh sách chung, cần có 3 danh sách riêng: activeDailyQuests, activeWeeklyQuests, trackedAchievements.
Thêm logic để reset activeDailyQuests mỗi ngày và activeWeeklyQuests mỗi tuần.
QuestPanel.cs:
Giao diện cần được thiết kế lại để có 3 tab (Ngày, Tuần, Thành tựu).
Mỗi tab sẽ đọc dữ liệu từ danh sách tương ứng trong QuestManager và hiển thị ra.

Check-list Kiểm tra Triển khai Kế hoạch Nâng cấp Game
Mục đích: Xác minh rằng tất cả các hệ thống gameplay cốt lõi đã được cập nhật theo đúng các yêu cầu thiết kế mới.
Phần 1: Hệ thống Anh hùng & Chỉ số (Đại tu POT)
#	Yêu cầu Kiểm tra	Kết quả Mong đợi (Coder phải giải thích & trình diễn được)
1.1	Hãy cho tôi xem một anh hùng Cấp 1. Giải thích cách các chỉ số HP/ATK/DEF/SPD của nó được tính toán từ chỉ số POT.	Coder chỉ vào chỉ số POT của anh hùng và giải thích công thức Chỉ số cơ bản = POT * (một số ngẫu nhiên từ 8 đến 10) đã được áp dụng.
1.2	Hãy dùng một anh hùng bất kỳ và cho nó lên 1 cấp. Điều gì xảy ra với các chỉ số và điểm tiềm năng?	Coder cho thấy các chỉ số cơ bản không thay đổi. Thay vào đó, một kho "Điểm Tự do" (Free Points) được cộng thêm một lượng bằng chính chỉ số POT của anh hùng đó.
1.3	Hãy cho tôi xem giao diện phân phối "Điểm Tự do".	Coder mở một panel, nơi có thể cộng điểm vào các chỉ số HP/ATK/DEF/SPD. Giao diện hiển thị rõ số điểm còn lại.
1.4	Trình diễn tính năng "Tự động cộng điểm".	Coder mở một popup/giao diện, thiết lập tỉ lệ (ví dụ: 50% ATK, 50% HP), sau đó cho anh hùng lên cấp. "Điểm Tự do" mới nhận được sẽ tự động được phân phối theo đúng tỉ lệ đã cài đặt.
1.5	Cho tôi xem 3 chỉ số mới (Né, Giảm ST, Tăng ST) nằm ở đâu trên thông tin nhân vật.	Coder chỉ ra 3 chỉ số mới này trên HeroInfoPanel, với giá trị ban đầu là 0% (trừ khi có Trait).
Phần 2: Hệ thống Trait (Đại tu Toàn diện)
#	Yêu cầu Kiểm tra	Kết quả Mong đợi (Coder phải giải thích & trình diễn được)
2.1	Hãy cho tôi xem quá trình lai tạo và giải thích 3 Trait khởi đầu của đứa con được xác định như thế nào.	Coder giải thích rõ quy tắc: 1 Trait thừa kế từ Cha, 1 từ Mẹ (có luật "an toàn" nếu quay ra độ hiếm thấp), và 1 Trait hoàn toàn ngẫu nhiên.
2.2	Hãy dùng một anh hùng cấp 19 và cho nó lên cấp 20. Quá trình nhận Trait mới (Trait thứ 4) diễn ra như thế nào?	Coder giải thích về quy trình "2 lượt quay": quay lần 1 để chọn ra một "Gia đình Trait" mà anh hùng chưa có, sau đó quay lần 2 để xác định "Độ hiếm" của Trait đó.
2.3	Hãy dùng một anh hùng cấp 59 và cho nó lên cấp 60. Điều gì xảy ra?	Anh hùng không nhận Trait mới. Thay vào đó, một giao diện hiện ra cho phép người chơi chọn NÂNG CẤP một trong các Trait hiện có lên bậc hiếm cao hơn (ví dụ từ C lên B).
2.4	Thử nâng cấp một Trait đã ở bậc A (Vàng).	Giao diện phải cho thấy Trait bậc A không thể được nâng cấp nữa.
2.5	Cho tôi xem một ví dụ về một "Gia đình Trait" trong cơ sở dữ liệu (ví dụ: gia đình Tăng ATK).	Coder cho xem 5 Trait riêng biệt (D, C, B, A, S) nhưng đều có cùng một familyId, và cho thấy giá trị hiệu ứng tăng dần qua từng bậc.
Phần 3: Hệ thống Nghề nghiệp & Lai tạo
#	Yêu cầu Kiểm tra	Kết quả Mong đợi (Coder phải giải thích & trình diễn được)
3.1	Hãy cho tôi xem một anh hùng Cấp 19. Nghề của nó là gì?	Anh hùng phải ở trạng thái "Chưa có nghề".
3.2	Cho anh hùng đó lên Cấp 20. Điều gì xảy ra?	Một giao diện hiện ra, cho phép người chơi CHỦ ĐỘNG CHỌN một trong các nghề cơ bản (Warrior, Archer, Mage) cho anh hùng.
3.3	Hãy trình diễn Tháp Thử thách theo nghề.	Coder chỉ vào các POI Tháp cố định trên bản đồ. Khi vào giao diện chọn đội, chỉ các anh hùng thuộc đúng nghề mới được phép chọn. Phần thưởng phải là một loại tài nguyên đặc biệt.
3.4	Hãy cho một anh hùng đi lai tạo. Sau đó thử cho nó đi lai tạo ngay lập tức.	Hệ thống phải báo lỗi hoặc hiển thị đồng hồ đếm ngược 2 phút cooldown cá nhân của anh hùng đó.
3.5	Cho tôi xem giới hạn sinh sản của một anh hùng.	Coder chỉ ra một con số (ví dụ: "Số lần sinh sản: 3/10") trên HeroInfoPanel.
Phần 4: Hệ thống Công trình & Nhiệm vụ
#	Yêu cầu Kiểm tra	Kết quả Mong đợi (Coder phải giải thích & trình diễn được)
4.1	Hãy cho tôi xem cấp tối đa của Nhà chính.	Coder cho thấy giới hạn nâng cấp là Cấp 20.
4.2	Nâng cấp Nhà Sinh sản có tác dụng gì?	Coder giải thích và cho thấy ở các mốc cấp độ nhất định, số lượng cặp đôi có thể lai tạo cùng lúc tăng lên.
4.3	Tạo một tình huống dân số bị đầy và thử nhận một anh hùng mới.	Game phải hiện ra một màn hình quản lý, buộc người chơi phải sa thải (dismiss) một anh hùng cũ để giải phóng chỗ trống trước khi có thể nhận người mới.
4.4	Với một tài khoản mới chơi, hãy cho tôi xem trạng thái của nút Đấu trường (Arena).	Nút Đấu trường phải bị khóa hoặc không thể tương tác.
4.5	Làm thế nào để mở khóa Đấu trường?	Coder giải thích điều kiện: chơi 30 phút VÀ Nhà chính phải đạt Cấp 5.
4.6	Cho tôi xem giao diện Nhiệm vụ Ngày, Tuần và Thành tựu.	Coder mở các tab tương ứng, cho thấy các nhiệm vụ khác nhau với các mục tiêu và phần thưởng rõ ràng.
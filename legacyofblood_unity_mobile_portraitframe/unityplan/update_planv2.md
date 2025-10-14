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

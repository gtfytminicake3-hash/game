namespace LegendOfBlood
{
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq; // Cần thiết cho việc truy vấn danh sách

    /// <summary>
    /// Lớp tùy chọn để truyền các vật phẩm hoặc hiệu ứng đặc biệt vào quá trình lai tạo.
    /// </summary>
    public class BreedingOptions
    {
        public bool UseMutationPotion { get; set; } = false;
        public string GuaranteedTraitID { get; set; } = null;
        // Có thể thêm trong tương lai: public Profession CharmUsed { get; set; } = Profession.None;
    }

    /// <summary>
    /// Chứa toàn bộ logic và công thức cho việc lai tạo các hero.
    /// Đây là một lớp logic thuần túy, không kế thừa từ MonoBehaviour.
    /// </summary>
    public class BreedingSystem
    {
        // Các hằng số ID của Trait đặc biệt để tránh lỗi chính tả
        private const string TRAIT_TWINS = "S_04";
        private const string TRAIT_GENE_SELECTOR = "SSS_04";
        private const string TRAIT_ELITE_BLOODLINE = "SS_07";

        private const int MALE_NAME_COUNT = 27;
        private const int FEMALE_NAME_COUNT = 27;

        /// <summary>
        /// Hàm chính để thực hiện lai tạo giữa hai hero.
        /// </summary>
        /// <param name="father">Hero cha</param>
        /// <param name="mother">Hero mẹ</param>
        /// <param name="options">Các tùy chọn bổ sung như vật phẩm hỗ trợ</param>
        /// <returns>Một danh sách các hero con (thường là 1, có thể là 2 nếu có trait Song Sinh)</returns>
        public List<HeroData> Breed(HeroData father, HeroData mother, BreedingOptions options = null)
        {
            if (father.gender == mother.gender)
            {
                Debug.LogError(global::LocalizationSystem.GetText("breeding_error_same_gender"));
                return new List<HeroData>(); // Trả về danh sách rỗng nếu thất bại
            }

            // Xử lý tùy chọn mặc định nếu không được cung cấp
            options ??= new BreedingOptions();

            int numberOfOffspring = 1;
            // Kiểm tra Trait Song Sinh (S_04)
            if (father.traitIDs.Contains(TRAIT_TWINS) || mother.traitIDs.Contains(TRAIT_TWINS))
            {
                if (Random.value < 0.02f) // 2% cơ hội sinh đôi
                {
                    numberOfOffspring = 2;
                }
            }

            List<HeroData> offspringList = new List<HeroData>();
            for (int i = 0; i < numberOfOffspring; i++)
            {
                HeroData offspring = CreateSingleOffspring(father, mother, options);
                offspringList.Add(offspring);
            }

            return offspringList;
        }

        /// <summary>
        /// Tạo ra một hero con duy nhất.
        /// </summary>
        private HeroData CreateSingleOffspring(HeroData father, HeroData mother, BreedingOptions options)
        {
            // Xác định giới tính và tên trước
            Gender offspringGender = (Random.value < 0.5f) ? Gender.Male : Gender.Female;
            string offspringName = GetRandomName(offspringGender);

            var offspring = new HeroData
            {
                id = System.Guid.NewGuid().ToString(),
                heroName = offspringName,
                gender = offspringGender,
                level = 1,
                // Di truyền Tiềm năng (ví dụ: lấy trung bình)
                potential = (father.potential + mother.potential) / 2
            };

            // 1. Di truyền chỉ số
            offspring.baseStats = InheritStats(father.baseStats, mother.baseStats, options);
            offspring.currentHp = offspring.baseStats.hp;

            // 2. Di truyền Trait
            offspring.traitIDs = InheritTraits(father.traitIDs, mother.traitIDs, options);

            // 3. Áp dụng các hiệu ứng Trait đặc biệt sau khi di truyền
            // Trait Dòng Dõi Tinh Anh (SS_07)
            if (father.traitIDs.Contains(TRAIT_ELITE_BLOODLINE) || mother.traitIDs.Contains(TRAIT_ELITE_BLOODLINE))
            {
                if (Random.value < 0.10f) // 10% cơ hội +5% chỉ số cơ bản
                {
                    offspring.baseStats.hp = Mathf.FloorToInt(offspring.baseStats.hp * 1.05f);
                    offspring.baseStats.atk = Mathf.FloorToInt(offspring.baseStats.atk * 1.05f);
                    offspring.baseStats.def = Mathf.FloorToInt(offspring.baseStats.def * 1.05f);
                    offspring.baseStats.spd = Mathf.FloorToInt(offspring.baseStats.spd * 1.05f);
                }
            }
            
            // 4. Thiết lập trạng thái chưa trưởng thành (theo GDD_02)
            offspring.isMature = false;
            long maturationDuration = 60000; // 1 phút
            // TODO: Xử lý trait "Lớn Nhanh" (D_08) ở đây để giảm thời gian
            offspring.maturationEndTime = new System.DateTimeOffset(System.DateTime.UtcNow).ToUnixTimeMilliseconds() + maturationDuration;

            return offspring;
        }

        /// <summary>
        /// Xử lý logic di truyền chỉ số dựa trên công thức GDD.
        /// </summary>
        private HeroStats InheritStats(HeroStats fatherStats, HeroStats motherStats, BreedingOptions options)
        {
            // Tăng tỉ lệ đột biến nếu dùng thuốc
            float mutationChance = options.UseMutationPotion ? 0.5f : 0.1f; // 50% nếu dùng thuốc, 10% mặc định

            return new HeroStats
            {
                hp = CalculateInheritedStat(fatherStats.hp, motherStats.hp, mutationChance),
                atk = CalculateInheritedStat(fatherStats.atk, motherStats.atk, mutationChance),
                def = CalculateInheritedStat(fatherStats.def, motherStats.def, mutationChance),
                spd = CalculateInheritedStat(fatherStats.spd, motherStats.spd, mutationChance)
            };
        }

        /// <summary>
        /// Tính toán giá trị của một chỉ số duy nhất dựa trên công thức GDD.
        /// </summary>
        private int CalculateInheritedStat(int fatherStat, int motherStat, float mutationChance)
        {
            float roll = Random.value; // Số ngẫu nhiên từ 0.0 đến 1.0
            float average = (fatherStat + motherStat) / 2.0f;

            // Các ngưỡng xác suất
            float fatherChance = mutationChance + 0.3f;
            float motherChance = fatherChance + 0.3f;

            if (roll < mutationChance) // Cơ hội Đột biến
            {
                return Mathf.FloorToInt(average + (0.1f * average));
            }
            if (roll < fatherChance) // Cơ hội nhận chỉ số của Bố
            {
                return fatherStat;
            }
            if (roll < motherChance) // Cơ hội nhận chỉ số của Mẹ
            {
                return motherStat;
            }
            // Còn lại: Ngẫu nhiên trong khoảng [Bố, Mẹ]
            return Mathf.FloorToInt(Random.Range(Mathf.Min(fatherStat, motherStat), Mathf.Max(fatherStat, motherStat) + 1));
        }

        /// <summary>
        /// Xử lý logic di truyền Trait.
        /// </summary>
        private List<string> InheritTraits(List<string> fatherTraits, List<string> motherTraits, BreedingOptions options)
        {
            var childTraits = new HashSet<string>(); // Dùng HashSet để tránh trùng lặp

            // Xử lý trait được đảm bảo từ "Kẻ Chọn Lọc Gene" (SSS_04)
            if (!string.IsNullOrEmpty(options.GuaranteedTraitID))
            {
                childTraits.Add(options.GuaranteedTraitID);
            }

            // Lặp để có cơ hội nhận trait, tối đa 3 trait
            for (int i = 0; i < 3 && childTraits.Count < 3; i++)
            {
                float roll = Random.value;

                if (roll < 0.30f) // 30% cơ hội nhận trait từ Bố
                {
                    if (fatherTraits.Count > 0)
                    {
                        string randomTrait = fatherTraits[Random.Range(0, fatherTraits.Count)];
                        childTraits.Add(randomTrait); // HashSet tự xử lý trùng lặp
                    }
                }
                else if (roll < 0.60f) // 30% cơ hội nhận trait từ Mẹ
                {
                    if (motherTraits.Count > 0)
                    {
                        string randomTrait = motherTraits[Random.Range(0, motherTraits.Count)];
                        childTraits.Add(randomTrait);
                    }
                }
                else if (roll < 0.70f) // 10% cơ hội nhận 1 trait ngẫu nhiên từ toàn bộ danh sách
                {
                    var allTraitIds = DataManager.Instance.AllTraits.Keys.ToList();
                    if (allTraitIds.Count > 0)
                    {
                        string randomTrait = allTraitIds[Random.Range(0, allTraitIds.Count)];
                        childTraits.Add(randomTrait);
                    }
                }
            }

            return childTraits.ToList();
        }

        /// <summary>
        /// Lấy một tên ngẫu nhiên dựa trên giới tính.
        /// </summary>
        private string GetRandomName(Gender gender)
        {
            string nameKey;
            if (gender == Gender.Male)
            {
                int nameIndex = Random.Range(1, MALE_NAME_COUNT + 1);
                nameKey = $"male_name_{nameIndex}";
            }
            else
            {
                int nameIndex = Random.Range(1, FEMALE_NAME_COUNT + 1);
                nameKey = $"female_name_{nameIndex}";
            }
            return LocalizationSystem.GetText(nameKey);
        }
    }
}
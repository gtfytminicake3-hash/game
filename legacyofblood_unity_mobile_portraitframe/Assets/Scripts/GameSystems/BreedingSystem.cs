namespace LegendOfBlood
{
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;

    public class BreedingOptions
    {
        public bool UseMutationPotion { get; set; } = false;
        public string GuaranteedTraitID { get; set; } = null;
    }

    public class BreedingSystem
    {
        private const string TRAIT_TWINS = "S_04";
        private const string TRAIT_GENE_SELECTOR = "SSS_04";
        private const string TRAIT_ELITE_BLOODLINE = "SS_07";

        private const int MALE_NAME_COUNT = 27;
        private const int FEMALE_NAME_COUNT = 27;
        
        public List<HeroData> Breed(HeroData father, HeroData mother, BreedingOptions options = null)
        {
            if (father.gender == mother.gender)
            {
                Debug.LogError(global::LocalizationSystem.GetText("breeding_error_same_gender"));
                return new List<HeroData>();
            }

            options ??= new BreedingOptions();

            int numberOfOffspring = 1;
            if (father.traitIDs.Contains(TRAIT_TWINS) || mother.traitIDs.Contains(TRAIT_TWINS))
            {
                if (Random.value < 0.02f)
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
        
        private HeroData CreateSingleOffspring(HeroData father, HeroData mother, BreedingOptions options)
        {
            Gender offspringGender = (Random.value < 0.5f) ? Gender.Male : Gender.Female;
            string offspringName = GetRandomName(offspringGender);

            // Sử dụng constructor để đảm bảo các giá trị cơ bản được khởi tạo đúng
            var offspring = new HeroData(System.Guid.NewGuid().ToString(), offspringName, offspringGender)
            {
                level = 1,
                potential = (father.potential + mother.potential) / 2
            };

            offspring.baseStats = InheritStats(father.baseStats, mother.baseStats, options);
            offspring.traitIDs = InheritTraits(father.traitIDs, mother.traitIDs, options);
            
            if (father.traitIDs.Contains(TRAIT_ELITE_BLOODLINE) || mother.traitIDs.Contains(TRAIT_ELITE_BLOODLINE))
            {
                if (Random.value < 0.10f)
                {
                    // SỬA LỖI: Gán trực tiếp giá trị float, không cần FloorToInt
                    offspring.baseStats.hp *= 1.05f;
                    offspring.baseStats.atk *= 1.05f;
                    offspring.baseStats.def *= 1.05f;
                    offspring.baseStats.spd *= 1.05f;
                }
            }
            
            // Đảm bảo máu hiện tại bằng máu tối đa sau khi đã tính toán tất cả
            offspring.currentHp = offspring.baseStats.hp;

            offspring.isMature = false;
            long maturationDuration = 60000;
            offspring.maturationEndTime = new System.DateTimeOffset(System.DateTime.UtcNow).ToUnixTimeMilliseconds() + maturationDuration;

            return offspring;
        }

        private HeroStats InheritStats(HeroStats fatherStats, HeroStats motherStats, BreedingOptions options)
        {
            float mutationChance = options.UseMutationPotion ? 0.5f : 0.1f;

            return new HeroStats
            {
                // Các hàm này bây giờ nhận và trả về float, không còn lỗi
                hp = CalculateInheritedStat(fatherStats.hp, motherStats.hp, mutationChance),
                atk = CalculateInheritedStat(fatherStats.atk, motherStats.atk, mutationChance),
                def = CalculateInheritedStat(fatherStats.def, motherStats.def, mutationChance),
                spd = CalculateInheritedStat(fatherStats.spd, motherStats.spd, mutationChance)
            };
        }

        /// <summary>
        /// SỬA LỖI: Thay đổi chữ ký của hàm để nhận và trả về float.
        /// </summary>
        private float CalculateInheritedStat(float fatherStat, float motherStat, float mutationChance)
        {
            float roll = Random.value;
            float average = (fatherStat + motherStat) / 2.0f;
            
            float fatherChance = mutationChance + 0.3f;
            float motherChance = fatherChance + 0.3f;

            float result;
            if (roll < mutationChance) // Đột biến
            {
                result = average + (0.1f * average);
            }
            else if (roll < fatherChance) // Nhận chỉ số của Bố
            {
                result = fatherStat;
            }
            else if (roll < motherChance) // Nhận chỉ số của Mẹ
            {
                result = motherStat;
            }
            else // Ngẫu nhiên trong khoảng
            {
                result = Random.Range(Mathf.Min(fatherStat, motherStat), Mathf.Max(fatherStat, motherStat));
            }
            
            // Làm tròn kết quả cuối cùng để có số đẹp, nhưng vẫn giữ kiểu float
            return Mathf.Floor(result);
        }

        private List<string> InheritTraits(List<string> fatherTraits, List<string> motherTraits, BreedingOptions options)
        {
            var childTraits = new HashSet<string>();

            if (!string.IsNullOrEmpty(options.GuaranteedTraitID))
            {
                childTraits.Add(options.GuaranteedTraitID);
            }

            for (int i = 0; i < 3 && childTraits.Count < 3; i++)
            {
                float roll = Random.value;

                if (roll < 0.30f && fatherTraits.Any())
                {
                    childTraits.Add(fatherTraits[Random.Range(0, fatherTraits.Count)]);
                }
                else if (roll < 0.60f && motherTraits.Any())
                {
                    childTraits.Add(motherTraits[Random.Range(0, motherTraits.Count)]);
                }
                else if (roll < 0.70f)
                {
                    var allTraitIds = DataManager.Instance.AllTraits.Keys.ToList();
                    if (allTraitIds.Any())
                    {
                        childTraits.Add(allTraitIds[Random.Range(0, allTraitIds.Count)]);
                    }
                }
            }

            return childTraits.ToList();
        }
        
        private string GetRandomName(Gender gender)
        {
            string nameKey;
            if (gender == Gender.Male)
            {
                nameKey = $"male_name_{Random.Range(1, MALE_NAME_COUNT + 1)}";
            }
            else
            {
                nameKey = $"female_name_{Random.Range(1, FEMALE_NAME_COUNT + 1)}";
            }
            return LocalizationSystem.GetText(nameKey);
        }
    }
}
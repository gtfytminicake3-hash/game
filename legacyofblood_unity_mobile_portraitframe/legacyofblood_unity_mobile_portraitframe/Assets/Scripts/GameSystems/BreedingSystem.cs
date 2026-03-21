namespace LegendOfBlood
{
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;

    public class BreedingOptions
    {
        public bool UseMutationPotion { get; set; } = false;
        public string GuaranteedTraitID { get; set; } = null;
        public LegendOfBlood.Profession? GuaranteedProfession { get; set; } = null;
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

            if (father.breedingCount >= father.maxBreedingCount || mother.breedingCount >= mother.maxBreedingCount)
            {
                Debug.LogWarning("Một trong hai Hero đã hết lượt sinh sản!");
                return new List<HeroData>(); // Hoặc trả về null tùy logic UI
            }

            father.breedingCount++;
            mother.breedingCount++;

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

            var offspring = new HeroData(System.Guid.NewGuid().ToString(), offspringName, offspringGender)
            {
                level = 1,
                potential = (father.potential + mother.potential) / 2
            };

            CalculateBaseStats(offspring);
            offspring.traitIDs = InheritTraits(father.traitIDs, mother.traitIDs, options);
            
            // Pass the guaranteed profession from the Wish Amulet to the newborn
            if (options.GuaranteedProfession.HasValue)
            {
                offspring.guaranteedProfession = options.GuaranteedProfession.Value;
            }
            
            if (father.traitIDs.Contains(TRAIT_ELITE_BLOODLINE) || mother.traitIDs.Contains(TRAIT_ELITE_BLOODLINE))
            {
                if (Random.value < 0.10f)
                {
                    offspring.baseStats.hp *= 1.05f;
                    offspring.baseStats.atk *= 1.05f;
                    offspring.baseStats.def *= 1.05f;
                    offspring.baseStats.spd *= 1.05f;
                }
            }
            
            offspring.currentHp = offspring.GetFinalStats().hp;

            offspring.isMature = false;
            long maturationDuration = 60000;
            
            // Xử lý Trait: Lớn Nhanh (Fast Grower - D_08)
            if (offspring.traitIDs.Contains("D_08"))
            {
                maturationDuration = (long)(maturationDuration * 0.7f); // Giảm 30% thời gian trưởng thành
                Debug.Log($"Hero {offspring.heroName} có trait Lớn Nhanh (D_08), giảm thời gian trưởng thành còn {maturationDuration}ms.");
            }

            offspring.maturationEndTime = new System.DateTimeOffset(System.DateTime.UtcNow).ToUnixTimeMilliseconds() + maturationDuration;

            return offspring;
        }

        private void CalculateBaseStats(HeroData newHero)
        {
            if (newHero.baseStats == null) newHero.baseStats = new HeroStats();
            newHero.baseStats.hp = (int)(newHero.potential * Random.Range(8f, 11f));
            newHero.baseStats.atk = (int)(newHero.potential * Random.Range(8f, 11f));
            newHero.baseStats.def = (int)(newHero.potential * Random.Range(8f, 11f));
            newHero.baseStats.spd = (int)(newHero.potential * Random.Range(8f, 11f));

            if (newHero.addedStats == null) newHero.addedStats = new HeroStats();
            newHero.freeStatPoints = 0;
        }

        private List<string> InheritTraits(List<string> fatherTraits, List<string> motherTraits, BreedingOptions options)
        {
            var inheritedTraits = new HashSet<string>();

            // 1. Guaranteed Trait (if any)
            if (!string.IsNullOrEmpty(options.GuaranteedTraitID))
            {
                inheritedTraits.Add(options.GuaranteedTraitID);
            }

            // 2. Get trait from father
            var traitFromFather = GetTraitFromParent(fatherTraits, inheritedTraits);
            if (traitFromFather != null)
            {
                inheritedTraits.Add(traitFromFather);
            }

            // 3. Get trait from mother
            var traitFromMother = GetTraitFromParent(motherTraits, inheritedTraits);
            if (traitFromMother != null)
            {
                inheritedTraits.Add(traitFromMother);
            }

            // 4. Get random traits until we have 3, respecting uniqueness
            // SỬA LỖI: Lấy Trait từ DataManager và lọc những trait chưa có để tránh vòng lặp vô hạn
            var availableTraits = DataManager.Instance.AllTraits.Values
                                    .Where(t => t != null && !inheritedTraits.Contains(t.id))
                                    .ToList(); 
                                    
            // 3.5 Use Mutation Potion Logic
            if (options.UseMutationPotion)
            {
                var highRankTraits = availableTraits.Where(t => t.rank == Trait.RarityRank.A || t.rank == Trait.RarityRank.S || t.rank == Trait.RarityRank.SS || t.rank == Trait.RarityRank.SSS).ToList();
                if (highRankTraits.Count > 0)
                {
                    var randomHighTrait = highRankTraits[Random.Range(0, highRankTraits.Count)];
                    inheritedTraits.Add(randomHighTrait.id);
                    availableTraits.Remove(randomHighTrait);
                    Debug.Log("Mutation Potion triggered! Guaranteed a high-rank trait.");
                }
            }

            while (inheritedTraits.Count < 3 && availableTraits.Count > 0)
            {
                var randomTrait = availableTraits[Random.Range(0, availableTraits.Count)];
                inheritedTraits.Add(randomTrait.id);
                availableTraits.Remove(randomTrait);
            }

            return inheritedTraits.ToList();
        }

        private string GetTraitFromParent(List<string> parentTraits, HashSet<string> existingTraits)
        {
            if (parentTraits == null || parentTraits.Count == 0)
            {
                return null;
            }

            var validTraits = parentTraits.Where(t => !existingTraits.Contains(t)).ToList();
            if (validTraits.Count == 0)
            {
                return null;
            }

            return validTraits[Random.Range(0, validTraits.Count)];
        }

        // The NameGenerator class now handles name generation.
        private string GetRandomName(Gender gender)
        {
            return NameGenerator.GetRandomName(gender);
        }
    }
}
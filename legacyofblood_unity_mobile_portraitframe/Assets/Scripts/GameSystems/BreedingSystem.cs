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

            var offspring = new HeroData(System.Guid.NewGuid().ToString(), offspringName, offspringGender)
            {
                level = 1,
                potential = (father.potential + mother.potential) / 2
            };

            CalculateBaseStats(offspring);
            offspring.traitIDs = InheritTraits(father.traitIDs, mother.traitIDs, options);
            
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
            offspring.maturationEndTime = new System.DateTimeOffset(System.DateTime.UtcNow).ToUnixTimeMilliseconds() + maturationDuration;

            return offspring;
        }

        private void CalculateBaseStats(HeroData newHero)
        {
            newHero.baseStats = new HeroStats();
            newHero.baseStats.hp = newHero.potential * Random.Range(8, 11);
            newHero.baseStats.atk = newHero.potential * Random.Range(8, 11);
            newHero.baseStats.def = newHero.potential * Random.Range(8, 11);
            newHero.baseStats.spd = newHero.potential * Random.Range(8, 11);

            newHero.addedStats = new HeroStats();
            newHero.freeStatPoints = 0;
            newHero.evasionRate = 0f;
            newHero.damageReduction = 0f;
            newHero.damageIncrease = 0f;
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
            // SỬA LỖI: Lấy Trait từ DataManager, không dùng TraitDatabase
            var allTraits = DataManager.Instance.AllTraits.Values.ToList(); 
            while (inheritedTraits.Count < 3 && allTraits.Any())
            {
                var randomTrait = allTraits[Random.Range(0, allTraits.Count)];
                if (randomTrait != null && !inheritedTraits.Contains(randomTrait.id))
                {
                    inheritedTraits.Add(randomTrait.id);
                }
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

        private string GetRandomName(Gender gender)
        {
            if (gender == Gender.Male)
            {
                return $"MaleName_{Random.Range(1, MALE_NAME_COUNT + 1)}";
            }
            else
            {
                return $"FemaleName_{Random.Range(1, FEMALE_NAME_COUNT + 1)}";
            }
        }
    }
}
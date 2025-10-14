namespace LegendOfBlood
{
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;

    public class RecruitmentSystem
    {
        public List<HeroData> PerformRecruitment(int amount = 1)
        {
            var newHeroes = new List<HeroData>();
            for (int i = 0; i < amount; i++)
            {
                if (DataManager.Instance.IsPopulationFull())
                {
                    Debug.LogWarning("Population is full! Stopping recruitment.");
                    GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("notification_population_full"));
                    break; // Dừng tuyển mộ nếu dân số đã đầy
                }
                // 1. Roll for Rarity
                Trait.RarityRank rarity = RollForRarity();

                // 2. Determine POT based on Rarity
                int pot = GetPotentialFromRarity(rarity);

                // 3. Create Hero
                Gender gender = (Random.value < 0.5f) ? Gender.Male : Gender.Female;
                string name = GetRandomName(gender);
                var newHero = new HeroData(System.Guid.NewGuid().ToString(), name, gender)
                {
                    level = 1,
                    potential = pot
                };

                // 4. Calculate Base Stats
                CalculateBaseStats(newHero);

                // 5. Assign Traits
                newHero.traitIDs = AssignRandomTraits();
                
                newHero.currentHp = newHero.GetFinalStats().hp;

                // 6. Add to Player's Hero List
                DataManager.Instance.AddHero(newHero); // Sẽ được xử lý bởi UI sau này, tạm thời vẫn thêm trực tiếp
                newHeroes.Add(newHero);
                Debug.Log($"<color=green>Recruited!</color> A new {rarity}-rank hero named {name} with POT {pot} has joined.");
            }
            return newHeroes;
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

        private List<string> AssignRandomTraits()
        {
            var childTraits = new HashSet<string>();
            // SỬA LỖI: Lấy Trait từ DataManager, không dùng TraitDatabase
            var allTraits = DataManager.Instance.AllTraits.Values.ToList();

            if (!allTraits.Any())
            {
                Debug.LogError("DataManager.AllTraits is empty! Cannot assign random traits.");
                return new List<string>();
            }

            while (childTraits.Count < 3)
            {
                var randomTrait = allTraits[Random.Range(0, allTraits.Count)];
                if (!childTraits.Contains(randomTrait.id))
                {
                    childTraits.Add(randomTrait.id);
                }
            }
            return childTraits.ToList();
        }

        private Trait.RarityRank RollForRarity()
        {
            float roll = Random.Range(0f, 100f);
            if (roll < 0.5f) return Trait.RarityRank.S;     // 0.5%
            if (roll < 3.5f) return Trait.RarityRank.A;     // 3%
            if (roll < 10f) return Trait.RarityRank.B;      // 6.5%
            if (roll < 40f) return Trait.RarityRank.C;      // 30%
            return Trait.RarityRank.D;                      // 60%
        }

        private int GetPotentialFromRarity(Trait.RarityRank rarity)
        {
            return rarity switch
            {
                Trait.RarityRank.S => Random.Range(17, 21),
                Trait.RarityRank.A => Random.Range(13, 17),
                Trait.RarityRank.B => Random.Range(9, 13),
                Trait.RarityRank.C => Random.Range(5, 9),
                _ => Random.Range(1, 5),
            };
        }

        private string GetRandomName(Gender gender)
        {
            string nameKey;
            if (gender == Gender.Male)
            {
                nameKey = $"male_name_{Random.Range(1, 28)}";
            }
            else
            {
                nameKey = $"female_name_{Random.Range(1, 28)}";
            }
            return LocalizationSystem.GetText(nameKey);
        }
    }
}

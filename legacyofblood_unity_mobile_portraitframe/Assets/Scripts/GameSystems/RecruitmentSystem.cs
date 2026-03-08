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
            newHero.CalculateBaseStats();
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

            var availableTraits = allTraits.ToList(); // Copy to manipulate
            while (childTraits.Count < 3 && availableTraits.Count > 0)
            {
                var randomTrait = availableTraits[Random.Range(0, availableTraits.Count)];
                childTraits.Add(randomTrait.id);
                availableTraits.Remove(randomTrait);
            }
            return childTraits.ToList();
        }

        private Trait.RarityRank RollForRarity()
        {
            var raritySettings = DataManager.Instance?.GameConfig?.RaritySettings;
            if (raritySettings != null && raritySettings.Count > 0)
            {
                float roll = Random.Range(0f, 100f);
                float cumulative = 0f;
                // Sắp xếp tăng dần theo drop chance để chắc chắn việc check là hợp lý nhất, hoặc duyệt theo thứ tự khai báo.
                foreach (var setting in raritySettings.OrderBy(r => r.dropChance))
                {
                    cumulative += setting.dropChance;
                    if (roll <= cumulative) return setting.rank;
                }
                // Fallback nếu tổng < 100
                return raritySettings.Last().rank;
            }

            // Fallback nếu chưa config
            float simpleRoll = Random.Range(0f, 100f);
            if (simpleRoll < 0.5f) return Trait.RarityRank.S;     // 0.5%
            if (simpleRoll < 3.5f) return Trait.RarityRank.A;     // 3%
            if (simpleRoll < 10f) return Trait.RarityRank.B;      // 6.5%
            if (simpleRoll < 40f) return Trait.RarityRank.C;      // 30%
            return Trait.RarityRank.D;                      // 60%
        }

        private int GetPotentialFromRarity(Trait.RarityRank rarity)
        {
            var raritySettings = DataManager.Instance?.GameConfig?.RaritySettings;
            if (raritySettings != null && raritySettings.Count > 0)
            {
                var setting = raritySettings.FirstOrDefault(r => r.rank == rarity);
                if (setting != null)
                {
                    // Lấy random trong khoảng min, max config
                    return Random.Range(setting.minPotential, setting.maxPotential + 1);
                }
            }

            // Fallback
            return rarity switch
            {
                Trait.RarityRank.S => Random.Range(17, 21),
                Trait.RarityRank.A => Random.Range(13, 17),
                Trait.RarityRank.B => Random.Range(9, 13),
                Trait.RarityRank.C => Random.Range(5, 9),
                _ => Random.Range(1, 5),
            };
        }

        // The NameGenerator class now handles name generation.
        private string GetRandomName(Gender gender)
        {
            return NameGenerator.GetRandomName(gender);
        }
    }
}

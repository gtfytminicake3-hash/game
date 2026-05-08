namespace LegendOfBlood
{
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;

    public class RecruitmentSystem
    {
        // Pity counter: tăng mỗi lần KHÔNG ra rank B trở lên
        private int _pityCounter = 0;
        private int _sPityCounter = 0; // Pity riêng cho S-Rank
        private const int PITY_THRESHOLD_A = 50;  // Sau 50 lần quay sẽ guaranteed A
        private const int PITY_THRESHOLD_B = 20;  // Sau 20 lần quay sẽ guaranteed B
        private const int PITY_THRESHOLD_S = 9;   // Cứ 10 lần quay sẽ guaranteed S (lần thứ 10)

        // Tổng số lần đã quay (tracking cho stat/achievement)
        private int _totalRecruitments = 0;

        public int PityCounter => _pityCounter;
        public int TotalRecruitments => _totalRecruitments;

        public List<HeroData> PerformRecruitment(int amount = 1)
        {
            var newHeroes = new List<HeroData>();
            for (int i = 0; i < amount; i++)
            {
                if (DataManager.Instance.IsPopulationFull())
                {
                    Debug.LogWarning("Population is full! Stopping recruitment.");
                    break;
                }

                // 1. Roll for Rarity (with Pity)
                Trait.RarityRank rarity = RollForRarityWithPity();
                _totalRecruitments++;

                // 2. Determine POT based on Rarity
                int pot = GetPotentialFromRarity(rarity);

                // 3. Create Hero
                Gender gender = (Random.value < 0.5f) ? Gender.Male : Gender.Female;
                string name = GetRandomName(gender);

                var newHero = new HeroData(System.Guid.NewGuid().ToString(), name, gender)
                {
                    level = 10,
                    potential = pot,
                    isMature = true
                };

                // 4. Assign random Profession
                var professions = System.Enum.GetValues(typeof(Profession)).Cast<Profession>().ToList();
                professions.Remove(Profession.None);
                newHero.SetProfession(professions[Random.Range(0, professions.Count)]);

                // 5. Assign starting skill
                var startingSkills = DataManager.Instance.GetStartingSkills(newHero.profession);
                if (startingSkills != null && startingSkills.Count > 0)
                {
                    string randomSkillID = startingSkills[Random.Range(0, startingSkills.Count)];
                    if (!newHero.skillIDs.Contains(randomSkillID))
                    {
                        newHero.skillIDs.Add(randomSkillID);
                    }
                }

                // 6. Calculate Base Stats
                CalculateBaseStats(newHero);

                // Give free stat points for level 1 to 10 (9 levels worth of potential)
                newHero.freeStatPoints = newHero.potential * 9;

                // 7. Assign Traits (quality scales with rarity)
                newHero.traitIDs = AssignRandomTraits(rarity);

                newHero.currentHp = newHero.GetFinalStats().hp;

                // 8. Add to Player's Hero List
                DataManager.Instance.AddHero(newHero);
                newHeroes.Add(newHero);

                Debug.Log($"<color=green>Recruited!</color> [{rarity}] {name} | POT {pot} | {newHero.profession} | S-Pity: {_sPityCounter}");
            }
            return newHeroes;
        }

        private void CalculateBaseStats(HeroData newHero)
        {
            newHero.CalculateBaseStats();
        }

        #region Rarity with Pity

        private Trait.RarityRank RollForRarityWithPity()
        {
            // S-Rank Pity Guarantee
            if (_sPityCounter >= PITY_THRESHOLD_S)
            {
                _sPityCounter = 0;
                _pityCounter = 0;
                Debug.Log("<color=yellow>[PITY] Guaranteed S-rank!</color>");
                return Trait.RarityRank.S;
            }
            if (_pityCounter >= PITY_THRESHOLD_A)
            {
                _pityCounter = 0;
                _sPityCounter++;
                Debug.Log("<color=yellow>[PITY] Guaranteed A-rank!</color>");
                return Trait.RarityRank.A;
            }
            if (_pityCounter >= PITY_THRESHOLD_B)
            {
                // Soft pity: tăng tỉ lệ B+ lên
                float bonusChance = (_pityCounter - PITY_THRESHOLD_B) * 2f; // +2% mỗi lần
                if (Random.Range(0f, 100f) < bonusChance)
                {
                    _pityCounter = 0;
                    _sPityCounter++;
                    Debug.Log("<color=cyan>[SOFT PITY] B-rank triggered!</color>");
                    return Trait.RarityRank.B;
                }
            }

            Trait.RarityRank result = RollForRarity();

            // Update pity counters
            if (result == Trait.RarityRank.S)
            {
                _sPityCounter = 0;
                _pityCounter = 0;
            }
            else
            {
                _sPityCounter++;
                if (result >= Trait.RarityRank.B)
                {
                    _pityCounter = 0; // Reset khi ra B trở lên
                }
                else
                {
                    _pityCounter++;
                }
            }

            return result;
        }

        private Trait.RarityRank RollForRarity()
        {
            var raritySettings = DataManager.Instance?.GameConfig?.RaritySettings;
            if (raritySettings != null && raritySettings.Count > 0)
            {
                // Chỉ lấy các rank từ S trở xuống (không cho phép summon ra SS và SSS)
                var validSettings = raritySettings.Where(r => r.rank <= Trait.RarityRank.S).OrderBy(r => r.dropChance).ToList();
                
                // Tính lại tổng tỷ lệ (do đã loại bỏ SS, SSS)
                float totalChance = validSettings.Sum(r => r.dropChance);
                float roll = Random.Range(0f, totalChance);
                
                float cumulative = 0f;
                foreach (var setting in validSettings)
                {
                    cumulative += setting.dropChance;
                    if (roll <= cumulative) return setting.rank;
                }
                return Trait.RarityRank.D;
            }

            // Fallback
            float simpleRoll = Random.Range(0f, 100f);
            if (simpleRoll < 2.0f) return Trait.RarityRank.S;     // 2%
            if (simpleRoll < 10.0f) return Trait.RarityRank.A;    // 8% (2 + 8 = 10)
            if (simpleRoll < 30.0f) return Trait.RarityRank.B;    // 20% (10 + 20 = 30)
            if (simpleRoll < 70.0f) return Trait.RarityRank.C;    // 40% (30 + 40 = 70)
            return Trait.RarityRank.D;                            // 30%
        }

        #endregion

        #region Potential & Traits

        private int GetPotentialFromRarity(Trait.RarityRank rarity)
        {
            var raritySettings = DataManager.Instance?.GameConfig?.RaritySettings;
            if (raritySettings != null && raritySettings.Count > 0)
            {
                var setting = raritySettings.FirstOrDefault(r => r.rank == rarity);
                if (setting != null)
                {
                    return Random.Range(setting.minPotential, setting.maxPotential + 1);
                }
            }

            return rarity switch
            {
                Trait.RarityRank.S => Random.Range(17, 21),
                Trait.RarityRank.A => Random.Range(13, 17),
                Trait.RarityRank.B => Random.Range(9, 13),
                Trait.RarityRank.C => Random.Range(5, 9),
                _ => Random.Range(1, 5),
            };
        }

        private List<string> AssignRandomTraits(Trait.RarityRank heroRarity = Trait.RarityRank.D)
        {
            var childTraits = new HashSet<string>();
            var allTraits = DataManager.Instance.AllTraits.Values.ToList();

            if (!allTraits.Any())
            {
                Debug.LogError("DataManager.AllTraits is empty! Cannot assign random traits.");
                return new List<string>();
            }

            // Số lượng trait dựa trên rarity
            int traitCount = heroRarity switch
            {
                Trait.RarityRank.S => 4,
                Trait.RarityRank.A => 3,
                _ => 3
            };

            // Ưu tiên trait rank cao cho hero rank cao
            List<Trait> pool;
            if (heroRarity >= Trait.RarityRank.A)
            {
                // Hero rank A+ có cơ hội nhận trait rank cao
                pool = allTraits
                    .OrderByDescending(t => t.rank)
                    .ThenBy(_ => Random.value)
                    .ToList();
            }
            else
            {
                pool = allTraits.OrderBy(_ => Random.value).ToList();
            }

            foreach (var trait in pool)
            {
                if (childTraits.Count >= traitCount) break;
                childTraits.Add(trait.id);
            }

            return childTraits.ToList();
        }

        #endregion

        private string GetRandomName(Gender gender)
        {
            return NameGenerator.GetRandomName(gender);
        }
    }
}

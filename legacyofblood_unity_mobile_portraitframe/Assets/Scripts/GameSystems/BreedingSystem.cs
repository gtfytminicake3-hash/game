using System;
using System.Collections.Generic;
using UnityEngine;
using LegendOfBlood;

namespace LegendOfBlood
{
    public class BreedingOptions 
    {
        public bool UseMutationPotion { get; set; } = false;
        public string GuaranteedTraitID { get; set; } = null;
    }

    public class BreedingSystem
    {
        [Header("Breeding Configuration")]
        [Tooltip("Cost base in Gold for breeding")]
        public int baseBreedingCost = 500;
        
        [Tooltip("Extra gold cost per existing breeding count of parents")]
        public int costPerBreedingCount = 200;

        [Tooltip("Time in minutes for a newborn hero to mature")]
        public float maturationTimeMinutes = 1f;

        [Tooltip("Chance to mutate potential to a higher rarity (0.0 to 1.0)")]
        public float mutationChance = 0.15f;

        public event Action<HeroData> OnHeroBred;

        public bool CanBreed(HeroData parent1, HeroData parent2, out string errorMessage)
        {
            if (parent1 == null || parent2 == null)
            {
                errorMessage = "Chưa chọn đủ 2 Anh Hùng để lai tạo.";
                return false;
            }

            if (parent1.id == parent2.id)
            {
                errorMessage = "Không thể lai tạo cùng một Anh Hùng.";
                return false;
            }

            if (parent1.gender == parent2.gender)
            {
                errorMessage = "Cần 1 Nam và 1 Nữ để tiến hành lai tạo!";
                return false;
            }

            // --- BẮT BUỘC: Kiểm tra Trưởng thành (isMature) ---
            if (!parent1.isMature || !parent2.isMature)
            {
                errorMessage = "Cả 2 Anh Hùng phải trưởng thành mới có thể lai tạo.";
                return false;
            }

            // --- BẮT BUỘC: Kiểm tra Giới hạn Lai tạo ---
            if (parent1.breedingCount >= parent1.maxBreedingCount)
            {
                errorMessage = $"{parent1.heroName} đã đạt giới hạn lai tạo tối đa ({parent1.maxBreedingCount}).";
                return false;
            }
            if (parent2.breedingCount >= parent2.maxBreedingCount)
            {
                errorMessage = $"{parent2.heroName} đã đạt giới hạn lai tạo tối đa ({parent2.maxBreedingCount}).";
                return false;
            }

            int cost = CalculateBreedingCost(parent1, parent2);
            if (GameManager.Instance != null && GameManager.Instance.InventoryManager != null)
            {
                if (GameManager.Instance.InventoryManager.GetResourceAmount(ResourceType.Gold) < cost)
                {
                    errorMessage = $"Không đủ Vàng! Cần {cost} Vàng.";
                    return false;
                }
            }

            errorMessage = "";
            return true;
        }

        public int CalculateBreedingCost(HeroData parent1, HeroData parent2)
        {
            int extraCounts = (parent1 != null ? parent1.breedingCount : 0) + (parent2 != null ? parent2.breedingCount : 0);
            return baseBreedingCost + (extraCounts * costPerBreedingCount);
        }

        public List<HeroData> Breed(HeroData parent1, HeroData parent2, BreedingOptions options = null)
        {
            float cacheMutation = mutationChance;
            if (options != null && options.UseMutationPotion)
            {
                mutationChance += 0.5f; // Bonus 50% mutation chance with potion
            }

            string error;
            if (!CanBreed(parent1, parent2, out error))
            {
                Debug.LogWarning($"[Breeding] Breeding failed: {error}");
                ToastNotificationManager.Show(error, 2f);
                mutationChance = cacheMutation;
                return new List<HeroData>();
            }

            // Deduct cost
            int cost = CalculateBreedingCost(parent1, parent2);
            GameManager.Instance.InventoryManager.SpendResource(ResourceType.Gold, cost);

            // Increment breeding counts
            parent1.breedingCount++;
            parent2.breedingCount++;

            // Create baby
            HeroData child = GenerateOffspring(parent1, parent2);
            
            // Note: Don't add to inventory here! BreedingUIController does it for us via DataManager.Instance.AddHero()!
            // Wait, Inventory vs DataManager? Usually DataManager.AddHero adds to AllHeroes, and then Inventory fetches it.
            // I will not manually insert it into Inventory, as BreedingUIController explicitly says:
            // "foreach (var offspring in offspringList) { DataManager.Instance.AddHero(offspring); }"
            
            OnHeroBred?.Invoke(child);
            GameManager.Instance.SaveGame();
            
            mutationChance = cacheMutation;
            return new List<HeroData> { child };
        }

        private HeroData GenerateOffspring(HeroData parent1, HeroData parent2)
        {
            // 1. Gender Random
            Gender childGender = UnityEngine.Random.value > 0.5f ? Gender.Male : Gender.Female;
            HeroData child = new HeroData(Guid.NewGuid().ToString(), GenerateChildName(parent1, parent2), childGender);

            // 2. Potential (Rarity)
            int basePot;
            if (parent1.potential == parent2.potential)
            {
                basePot = parent1.potential;
            }
            else
            {
                basePot = (parent1.potential + parent2.potential) / 2;
            }

            if (UnityEngine.Random.value < mutationChance)
            {
                float jumpRoll = UnityEngine.Random.value;
                if (jumpRoll < 0.05f) // 5% of mutation chance -> Massive jump (can reach SSS)
                {
                    basePot += UnityEngine.Random.Range(7, 10);
                }
                else if (jumpRoll < 0.2f) // 15% of mutation chance -> Rare jump (can reach SS)
                {
                    basePot += UnityEngine.Random.Range(4, 7);
                }
                else
                {
                    basePot += UnityEngine.Random.Range(1, 4); // +1 to +3 potential
                }
            }
            child.potential = Mathf.Clamp(basePot, 1, 100);

            // 3. Profession Inheritance
            float profRoll = UnityEngine.Random.value;
            Profession childProf = Profession.None;
            if (profRoll < 0.45f) childProf = parent1.profession;
            else if (profRoll < 0.90f) childProf = parent2.profession;
            else 
            {
                // 10% chance to mutate into a random profession
                Array profs = Enum.GetValues(typeof(Profession));
                childProf = (Profession)profs.GetValue(UnityEngine.Random.Range(1, profs.Length)); // Skip 0 (None)
            }
            child.SetProfession(childProf);

            // 4. Traits Inheritance
            child.traitIDs = InheritTraits(parent1, parent2);

            // 5. Maturation (Children are born as babies!)
            child.isMature = false;
            child.maturationEndTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + (long)(maturationTimeMinutes * 60 * 1000);
            
            // Generate stats based on new Potential
            child.CalculateBaseStats();

            // Set a slightly lower breeding capacity to prevent infinite generation
            int p1Max = parent1.maxBreedingCount;
            int p2Max = parent2.maxBreedingCount;
            child.maxBreedingCount = Mathf.Max(0, Mathf.Min(p1Max, p2Max) - 1);
            if (child.maxBreedingCount < 3) child.maxBreedingCount = 3; // Guaranteed minimum 3

            return child;
        }

        private List<string> InheritTraits(HeroData p1, HeroData p2)
        {
            var inherited = new HashSet<string>();
            
            // Mix traits from both parents
            if (p1.traitIDs != null)
            {
                foreach (var t in p1.traitIDs)
                {
                    if (UnityEngine.Random.value < 0.5f) inherited.Add(t);
                }
            }
            
            if (p2.traitIDs != null)
            {
                foreach (var t in p2.traitIDs)
                {
                    if (UnityEngine.Random.value < 0.5f) inherited.Add(t);
                }
            }

            // Small chance for a new completely random trait (mutation)
            if (UnityEngine.Random.value < 0.2f && DataManager.Instance != null && DataManager.Instance.AllTraits != null)
            {
                var keys = new List<string>(DataManager.Instance.AllTraits.Keys);
                if (keys.Count > 0)
                {
                    string randomTrait = keys[UnityEngine.Random.Range(0, keys.Count)];
                    inherited.Add(randomTrait);
                }
            }

            return new List<string>(inherited);
        }

        private string GenerateChildName(HeroData p1, HeroData p2)
        {
            // Simple name combination logic (e.g. "Jon" + "Anna" -> "Jonna" or "Anon")
            string n1 = p1.heroName != null && p1.heroName.Length > 2 ? p1.heroName : "Hero";
            string n2 = p2.heroName != null && p2.heroName.Length > 2 ? p2.heroName : "Hero";

            // Grab first half of Parent 1, second half of Parent 2
            int half1 = Mathf.Max(1, n1.Length / 2);
            int half2 = Mathf.Max(1, n2.Length / 2);

            string part1 = n1.Substring(0, half1);
            string part2 = n2.Substring(n2.Length - half2);

            string newName = part1 + part2;
            newName = newName.ToLower();
            newName = char.ToUpper(newName[0]) + newName.Substring(1);

            return newName;
        }
    }
}
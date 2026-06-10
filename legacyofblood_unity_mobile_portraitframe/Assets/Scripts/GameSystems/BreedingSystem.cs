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
            else
            {
#if !UNITY_EDITOR
                errorMessage = "Không tìm thấy InventoryManager! Tạm thời không thể lai tạo.";
                return false;
#endif
            }

            // Recipe Check
            var recipe = GetHighestPriorityRecipe(parent1, parent2);
            if (recipe != null && !string.IsNullOrEmpty(recipe.requireMaterialId) && recipe.requireMaterialAmount > 0)
            {
                if (GameManager.Instance != null && GameManager.Instance.InventoryManager != null)
                {
                    if (GameManager.Instance.InventoryManager.GetItemCount(recipe.requireMaterialId) < recipe.requireMaterialAmount)
                    {
                        errorMessage = $"Không đủ vật liệu lai tạo! Cần {recipe.requireMaterialAmount} {recipe.requireMaterialId}.";
                        return false;
                    }
                }
                else
                {
#if !UNITY_EDITOR
                    errorMessage = "Không tìm thấy InventoryManager để kiểm tra vật liệu lai tạo.";
                    return false;
#endif
                }
            }

            errorMessage = "";
            return true;
        }

        public GameConfigs.BreedingRecipe GetHighestPriorityRecipe(HeroData p1, HeroData p2)
        {
            if (DataManager.Instance == null) return null;
            var config = DataManager.Instance.GetBreedingConfig();
            if (config == null || config.recipes == null || config.recipes.Count == 0) return null;

            GameConfigs.BreedingRecipe bestRecipe = null;
            foreach (var recipe in config.recipes)
            {
                bool matchP1 = string.IsNullOrEmpty(recipe.requireFatherTraitId) || (p1.traitIDs != null && p1.traitIDs.Contains(recipe.requireFatherTraitId));
                bool matchP2 = string.IsNullOrEmpty(recipe.requireMotherTraitId) || (p2.traitIDs != null && p2.traitIDs.Contains(recipe.requireMotherTraitId));
                
                if (matchP1 && matchP2)
                {
                    if (bestRecipe == null || recipe.priority > bestRecipe.priority)
                    {
                        bestRecipe = recipe;
                    }
                }
            }
            return bestRecipe;
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
            if (GameManager.Instance != null && GameManager.Instance.InventoryManager != null)
            {
                GameManager.Instance.InventoryManager.SpendResource(ResourceType.Gold, cost);
            }

            // Recipe logic
            var recipe = GetHighestPriorityRecipe(parent1, parent2);
            if (recipe != null)
            {
                if (!string.IsNullOrEmpty(recipe.requireMaterialId) && recipe.requireMaterialAmount > 0)
                {
                    if (GameManager.Instance != null && GameManager.Instance.InventoryManager != null)
                    {
                        GameManager.Instance.InventoryManager.ConsumeItem(recipe.requireMaterialId, recipe.requireMaterialAmount);
                    }
                }
                if (options == null) options = new BreedingOptions();
                options.GuaranteedTraitID = recipe.resultTraitId;
            }

            // Increment breeding counts
            parent1.breedingCount++;
            parent2.breedingCount++;

            // Create baby
            HeroData child = GenerateOffspring(parent1, parent2, options);
            
            // Note: Don't add to inventory here! BreedingUIController does it for us via DataManager.Instance.AddHero()!
            // Wait, Inventory vs DataManager? Usually DataManager.AddHero adds to AllHeroes, and then Inventory fetches it.
            // I will not manually insert it into Inventory, as BreedingUIController explicitly says:
            // "foreach (var offspring in offspringList) { DataManager.Instance.AddHero(offspring); }"
            
            OnHeroBred?.Invoke(child);
            GameManager.Instance.SaveGame();
            
            mutationChance = cacheMutation;
            return new List<HeroData> { child };
        }

        private HeroData GenerateOffspring(HeroData parent1, HeroData parent2, BreedingOptions options = null)
        {
            // 1. Gender Random
            Gender childGender = UnityEngine.Random.value > 0.5f ? Gender.Male : Gender.Female;
            HeroData child = new HeroData(Guid.NewGuid().ToString(), GenerateChildName(parent1, parent2), childGender);

            // --- LINEAGE ---
            child.fatherId = parent1.gender == Gender.Male ? parent1.id : parent2.id;
            child.motherId = parent1.gender == Gender.Female ? parent1.id : parent2.id;
            child.generation = Mathf.Max(parent1.generation, parent2.generation) + 1;

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
            
            // 4.5. Guaranteed Trait from Recipe
            if (options != null && !string.IsNullOrEmpty(options.GuaranteedTraitID))
            {
                if (child.traitIDs == null) child.traitIDs = new List<string>();
                if (!child.traitIDs.Contains(options.GuaranteedTraitID))
                {
                    child.traitIDs.Add(options.GuaranteedTraitID);
                }
            }

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

            bool hasData = DataManager.Instance != null && DataManager.Instance.AllTraits != null;
            GameConfigs.BreedingConfig config = DataManager.Instance != null ? DataManager.Instance.GetBreedingConfig() : null;

            if (config == null || !hasData)
            {
                // --- OLD FALLBACK (if config or trait data is missing) ---
                if (p1.traitIDs != null)
                {
                    foreach (var t in p1.traitIDs)
                        if (!string.IsNullOrEmpty(t) && UnityEngine.Random.value < 0.5f) inherited.Add(t);
                }
                
                if (p2.traitIDs != null)
                {
                    foreach (var t in p2.traitIDs)
                        if (!string.IsNullOrEmpty(t) && UnityEngine.Random.value < 0.5f) inherited.Add(t);
                }

                if (UnityEngine.Random.value < 0.2f && hasData)
                {
                    var keys = new List<string>(DataManager.Instance.AllTraits.Keys);
                    if (keys.Count > 0) inherited.Add(keys[UnityEngine.Random.Range(0, keys.Count)]);
                }
            }
            else
            {
                // --- NEW WEIGHTED ALGORITHM ---
                int penLevel = 1; // Fallback
                if (DataManager.Instance.AllBuildings != null)
                {
                    var pen = DataManager.Instance.AllBuildings.Find(b => b.id == "BreedingPen");
                    if (pen != null) penLevel = pen.level;
                }

                int maxTraits = config.maxInheritedTraits;
                int fatherWeight = config.inheritanceWeight.fatherWeight;
                int motherWeight = config.inheritanceWeight.motherWeight;
                int mutationWeight = config.inheritanceWeight.mutationWeight;
                int totalWeight = fatherWeight + motherWeight + mutationWeight;

                float actualMutationChance = config.mutationConfig.baseMutationChance + (penLevel * config.mutationConfig.mutationChancePerLevel);

                var traitKeys = new List<string>(DataManager.Instance.AllTraits.Keys);

                for (int i = 0; i < maxTraits; i++)
                {
                    // 1. Direct Mutation chance (from building/base config)
                    if (UnityEngine.Random.value < actualMutationChance && traitKeys.Count > 0)
                    {
                        inherited.Add(traitKeys[UnityEngine.Random.Range(0, traitKeys.Count)]);
                        continue;
                    }

                    // 2. Weighted selection
                    if (totalWeight <= 0) continue;
                    int roll = UnityEngine.Random.Range(0, totalWeight);

                    if (roll < fatherWeight)
                    {
                        if (p1.traitIDs != null && p1.traitIDs.Count > 0)
                        {
                            string t = p1.traitIDs[UnityEngine.Random.Range(0, p1.traitIDs.Count)];
                            if (!string.IsNullOrEmpty(t)) inherited.Add(t);
                        }
                    }
                    else if (roll < fatherWeight + motherWeight)
                    {
                        if (p2.traitIDs != null && p2.traitIDs.Count > 0)
                        {
                            string t = p2.traitIDs[UnityEngine.Random.Range(0, p2.traitIDs.Count)];
                            if (!string.IsNullOrEmpty(t)) inherited.Add(t);
                        }
                    }
                    else
                    {
                        if (traitKeys.Count > 0) inherited.Add(traitKeys[UnityEngine.Random.Range(0, traitKeys.Count)]);
                    }
                }
            }

            // Validate and clean up
            var finalList = new List<string>();
            foreach (var t in inherited)
            {
                if (string.IsNullOrEmpty(t)) continue;
                
                if (hasData && !DataManager.Instance.AllTraits.ContainsKey(t))
                {
                    Debug.LogWarning($"[Breeding] TraitID '{t}' không tồn tại. Skipping an toàn.");
                    continue;
                }
                finalList.Add(t);
            }

            return finalList;
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
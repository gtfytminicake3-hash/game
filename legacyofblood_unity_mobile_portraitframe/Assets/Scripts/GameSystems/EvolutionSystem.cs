
namespace LegendOfBlood
{
    using System.Linq;
    using UnityEngine;

    public class EvolutionSystem
    {
        public EvolutionSystem()
        {
            HeroData.OnHeroLeveledUp += HandleHeroLeveledUp;
            Debug.Log("EvolutionSystem Initialized and Subscribed to Level Up event.");
        }

        // Destructor to unsubscribe, good practice
        ~EvolutionSystem()
        {
            HeroData.OnHeroLeveledUp -= HandleHeroLeveledUp;
        }

        private void HandleHeroLeveledUp(HeroData hero)
        {
            if (DataManager.Instance == null || DataManager.Instance.GameConfig == null) return;
            var evolutionTable = DataManager.Instance.GameConfig.EvolutionTable;
            if (evolutionTable == null || evolutionTable.rewards == null) return;

            var reward = evolutionTable.rewards.FirstOrDefault(r => r.level == hero.level);
            if (reward != null)
            {
                if (reward.allowProfessionSelection && hero.profession == Profession.None)
                {
                    EventManager.TriggerEvent(GameEvents.OnProfessionSelectionRequested, hero);
                }
                
                if (reward.giveRandomTrait)
                {
                    GrantNewTrait_TwoRolls(hero);
                }
                
                if (reward.giveRandomSkill)
                {
                    GrantNewSkill(hero);
                }
            }
        }

        private void GrantNewSkill(HeroData hero)
        {
            if (DataManager.Instance == null) return;

            var availableSkills = DataManager.Instance.AllSkills.Values
                .Where(s => (int)s.requiredProfession == (int)hero.profession && !hero.skillIDs.Contains(s.id))
                .ToList();

            if (availableSkills.Count > 0)
            {
                var newSkill = availableSkills[UnityEngine.Random.Range(0, availableSkills.Count)];
                hero.skillIDs.Add(newSkill.id);
                Debug.Log($"<color=cyan>Evolution!</color> {hero.heroName} learned a new skill: [{newSkill.skillName}] at level {hero.level}.");
            }
        }

        private void GrantNewTrait_TwoRolls(HeroData hero)
        {
            // REFACTOR: Changed to use the central DataManager singleton instead of the obsolete TraitDatabase
            if (DataManager.Instance == null)
            {
                Debug.LogError("DataManager not found! Cannot grant new trait.");
                return;
            }

            // REFACTOR: Get all trait families directly from DataManager's collection
            var allFamilies = DataManager.Instance.AllTraits.Values
                                .Select(t => t.familyId)
                                .Where(f => !string.IsNullOrEmpty(f))
                                .Distinct()
                                .ToList();

            // REFACTOR: Get owned families using the correct DataManager method
            var ownedFamilies = hero.traitIDs
                                .Select(id => DataManager.Instance.GetTraitByID(id)?.familyId)
                                .Where(f => f != null)
                                .ToHashSet();

            var unownedFamilyPool = allFamilies.Where(f => !ownedFamilies.Contains(f)).ToList();

            if (unownedFamilyPool.Count == 0)
            {
                Debug.LogWarning($"{hero.heroName} has no new trait families to learn. All families acquired.");
                return;
            }

            // Roll 1: Choose a content family the hero doesn't have
            string chosenFamily = unownedFamilyPool[UnityEngine.Random.Range(0, unownedFamilyPool.Count)];
            
            // Roll 2: Choose a quality rank
            Trait.RarityRank chosenRank = RollForRarity();

            // REFACTOR: Find the resulting trait from DataManager's collection
            Trait newTrait = DataManager.Instance.AllTraits.Values
                                .FirstOrDefault(t => t.familyId == chosenFamily && t.rank == chosenRank);

            if (newTrait != null)
            {
                hero.traitIDs.Add(newTrait.id);
                Debug.Log($"<color=cyan>Evolution!</color> {hero.heroName} learned a new trait: [{newTrait.traitName}] (Rank: {newTrait.rank}) at level {hero.level}.");
            }
            else
            {
                // This can happen if a family exists but doesn't have a trait for the rolled rarity (e.g., no 'S' rank trait)
                Debug.LogWarning($"Could not find a trait for family '{chosenFamily}' with rank '{chosenRank}'. Trying to find any other rank in the same family as a fallback.");
                
                // Fallback: try to give any trait from the chosen family
                Trait fallbackTrait = DataManager.Instance.AllTraits.Values
                                        .Where(t => t.familyId == chosenFamily)
                                        .OrderBy(t => t.rank) // Get the lowest rank as a fallback
                                        .FirstOrDefault();
                if(fallbackTrait != null)
                {
                    hero.traitIDs.Add(fallbackTrait.id);
                    Debug.Log($"<color=yellow>Fallback Success!</color> {hero.heroName} learned a fallback trait: [{fallbackTrait.traitName}] (Rank: {fallbackTrait.rank}) at level {hero.level}.");
                }
                else
                {
                     Debug.LogError($"CRITICAL: Could not find any trait for family '{chosenFamily}' after fallback attempt.");
                }
            }
        }

        private Trait.RarityRank RollForRarity()
        {
            var raritySettings = DataManager.Instance?.GameConfig?.RaritySettings;
            if (raritySettings != null && raritySettings.Count > 0)
            {
                float roll = UnityEngine.Random.Range(0f, 100f);
                float cumulative = 0f;
                foreach (var setting in raritySettings.OrderBy(r => r.dropChance))
                {
                    cumulative += setting.dropChance;
                    if (roll <= cumulative) return setting.rank;
                }
                return raritySettings.Last().rank;
            }

            // Fallback
            float simpleRoll = UnityEngine.Random.Range(0f, 100f);
            if (simpleRoll < 0.5f) return Trait.RarityRank.S;    // 0.5%
            if (simpleRoll < 3.5f) return Trait.RarityRank.A;    // 3%
            if (simpleRoll < 10f) return Trait.RarityRank.B;     // 6.5%
            if (simpleRoll < 40f) return Trait.RarityRank.C;     // 30%
            return Trait.RarityRank.D;                        // 60%
        }
    }
}

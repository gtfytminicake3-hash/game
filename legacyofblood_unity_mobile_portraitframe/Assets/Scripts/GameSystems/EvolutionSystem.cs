namespace LegendOfBlood
{
    using System.Linq;
    using UnityEngine;

    public class EvolutionSystem
    {
        public EvolutionSystem()
        {
            HeroData.OnHeroLeveledUp += HandleHeroLeveledUp;
            Debug.Log("EvolutionSystem Initialized.");
        }

        ~EvolutionSystem()
        {
            HeroData.OnHeroLeveledUp -= HandleHeroLeveledUp;
        }

        private void HandleHeroLeveledUp(HeroData hero)
        {
            switch (hero.level)
            {
                case 20:
                case 40:
                case 80:
                    GrantNewTrait_TwoRolls(hero);
                    break;
                // Note: Levels 60 and 100 are for upgrades, handled by UI (TraitUpgradePanel)
            }
        }

        private void GrantNewTrait_TwoRolls(HeroData hero)
        {
            if (TraitDatabase.Instance == null) 
            {
                Debug.LogError("TraitDatabase not found!");
                return;
            }

            var allFamilies = TraitDatabase.Instance.GetAllFamilyIDs();
            var ownedFamilies = hero.traitIDs.Select(id => TraitDatabase.Instance.GetTraitByID(id)?.familyId).ToHashSet();
            var unownedFamilyPool = allFamilies.Where(f => !string.IsNullOrEmpty(f) && !ownedFamilies.Contains(f)).ToList();

            if (unownedFamilyPool.Count == 0)
            {
                Debug.LogWarning($"{hero.heroName} has no new trait families to learn.");
                return;
            }

            string chosenFamily = unownedFamilyPool[Random.Range(0, unownedFamilyPool.Count)];
            Trait.RarityRank chosenRank = RollForRarity();

            Trait newTrait = TraitDatabase.Instance.GetTraitByFamilyAndRank(chosenFamily, chosenRank);
            if (newTrait != null)
            {
                hero.traitIDs.Add(newTrait.id);
                Debug.Log($"<color=cyan>Evolution!</color> {hero.heroName} learned a new trait: [{newTrait.traitName}] at level {hero.level}.");
            }
            else
            {
                Debug.LogWarning($"Could not find a trait for family '{chosenFamily}' with rank '{chosenRank}'.");
            }
        }

        private Trait.RarityRank RollForRarity()
        {
            float roll = Random.Range(0f, 100f);
            if (roll < 0.5f) return Trait.RarityRank.S;
            if (roll < 3.5f) return Trait.RarityRank.A;
            if (roll < 10f) return Trait.RarityRank.B;
            if (roll < 40f) return Trait.RarityRank.C;
            return Trait.RarityRank.D;
        }
    }
}
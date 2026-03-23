namespace LegendOfBlood
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class TraitDatabase : MonoBehaviour
    {
        public static TraitDatabase Instance { get; private set; }

        private List<Trait> _allTraits;
        private Dictionary<string, Dictionary<Trait.RarityRank, Trait>> _traitsByFamilyAndRank;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                Initialize();
            }
        }

        private void Initialize()
        {
            _allTraits = Resources.LoadAll<Trait>("Traits").ToList();
            _traitsByFamilyAndRank = new Dictionary<string, Dictionary<Trait.RarityRank, Trait>>();

            foreach (var trait in _allTraits)
            {
                if (string.IsNullOrEmpty(trait.familyId)) continue;

                if (!_traitsByFamilyAndRank.ContainsKey(trait.familyId))
                {
                    _traitsByFamilyAndRank[trait.familyId] = new Dictionary<Trait.RarityRank, Trait>();
                }
                _traitsByFamilyAndRank[trait.familyId][trait.rank] = trait;
            }
        }

        public Trait GetTraitByID(string id)
        {
            return _allTraits.FirstOrDefault(t => t.id == id);
        }

        public List<Trait> GetAllTraits()
        {
            return _allTraits;
        }

        public List<string> GetAllFamilyIDs()
        {
            return _traitsByFamilyAndRank.Keys.ToList();
        }

        public Trait GetTraitByFamilyAndRank(string familyId, Trait.RarityRank rank)
        {
            if (_traitsByFamilyAndRank.TryGetValue(familyId, out var family) && family.TryGetValue(rank, out var trait))
            {
                return trait;
            }
            return null;
        }
    }
}

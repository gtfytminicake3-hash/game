namespace LegendOfBlood
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using System.Collections.Generic;

    public class TraitUpgradePanel : MonoBehaviour
    {
        [SerializeField] private GameObject traitUpgradeItemPrefab;
        [SerializeField] private Transform container;
        [SerializeField] private Button closeButton;

        private HeroData _currentHero;
        private List<GameObject> _instantiatedItems = new List<GameObject>();

        private void Awake()
        {
            closeButton.onClick.AddListener(ClosePanel);
        }

        public void Show(HeroData hero)
        {
            _currentHero = hero;
            gameObject.SetActive(true);
            PopulateTraits();
        }

        private void PopulateTraits()
        {
            ClearItems();
            foreach (var traitId in _currentHero.traitIDs)
            {
                var traitData = TraitDatabase.Instance.GetTraitByID(traitId);
                if (traitData == null) continue;

                var itemGO = Instantiate(traitUpgradeItemPrefab, container);
                var texts = itemGO.GetComponentsInChildren<TextMeshProUGUI>();
                var button = itemGO.GetComponentInChildren<Button>();

                texts[0].text = traitData.traitName;
                texts[1].text = traitData.description;

                bool canUpgrade = !string.IsNullOrEmpty(traitData.nextUpgradeTraitID) && traitData.rank < Trait.RarityRank.A;
                button.interactable = canUpgrade;

                if (canUpgrade)
                {
                    button.onClick.AddListener(() => OnUpgradeClicked(traitId, traitData.nextUpgradeTraitID));
                }
                else
                {
                    texts[2].text = "Maxed";
                }
                _instantiatedItems.Add(itemGO);
            }
        }

        private void OnUpgradeClicked(string oldTraitId, string newTraitId)
        {
            int index = _currentHero.traitIDs.IndexOf(oldTraitId);
            if (index != -1)
            {
                _currentHero.traitIDs[index] = newTraitId;
            }
            ClosePanel();
        }

        private void ClearItems()
        {
            foreach (var item in _instantiatedItems)
            {
                Destroy(item);
            }
            _instantiatedItems.Clear();
        }

        private void ClosePanel()
        {
            gameObject.SetActive(false);
        }
    }
}

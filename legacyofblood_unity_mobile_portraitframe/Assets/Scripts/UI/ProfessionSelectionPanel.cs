using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace LegendOfBlood
{
    public class ProfessionSelectionPanel : UIPanel
    {
        [Header("UI Elements")]
        [SerializeField] private Button warriorButton;
        [SerializeField] private Button archerButton;
        [SerializeField] private Button mageButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button closeButton;

        private HeroData _targetHero;
        private System.Action _onComplete;

        public void Show(HeroData hero, System.Action onComplete)
        {
            _targetHero = hero;
            _onComplete = onComplete;
            
            if(titleText != null)
                titleText.text = string.Format(LocalizationSystem.GetText("profession_selection_title"), hero.heroName);
            
            this.gameObject.SetActive(true);
        }

        private void Start()
        {
            warriorButton.onClick.AddListener(() => SelectProfession(Profession.Warrior));
            archerButton.onClick.AddListener(() => SelectProfession(Profession.Archer));
            mageButton.onClick.AddListener(() => SelectProfession(Profession.Mage));
        }

        private void SelectProfession(Profession profession)
        {
            if (_targetHero == null) return;

            _targetHero.profession = profession;
            Debug.Log($"Hero {_targetHero.heroName} selected profession: {profession}");
            
            // Save data
            DataManager.Instance.SavePlayerData();

            // Callback
            _onComplete?.Invoke();
            
            this.gameObject.SetActive(false);
        }
    }
}

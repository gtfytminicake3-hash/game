namespace LegendOfBlood
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class PopulationHeroCard : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI heroNameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI professionText;
        [SerializeField] private TextMeshProUGUI cpText;
        [SerializeField] private Image avatarImage;
        [SerializeField] private Button dismissButton;

        private HeroData _heroData;

        public void Setup(HeroData heroData, System.Action<HeroData> onDismiss)
        {
            _heroData = heroData;

            if (heroNameText != null) heroNameText.text = heroData.heroName;
            if (levelText != null) levelText.text = $"Lv {heroData.level}";
            if (professionText != null) professionText.text = heroData.profession.ToString();
            
            // Note: GetCombatPower() calculation assumes base + traits, check if it's available and public
            int cp = heroData.GetCombatPower();
            if (cpText != null) cpText.text = $"CP: {cp}";

            if (avatarImage != null) 
            {
                avatarImage.sprite = heroData.GetAvatarSprite();
                avatarImage.color = Color.white; // Ensure it's not tinted gray
            }

            if (dismissButton != null)
            {
                dismissButton.onClick.RemoveAllListeners();
                dismissButton.onClick.AddListener(() => onDismiss?.Invoke(_heroData));
            }
        }
    }
}

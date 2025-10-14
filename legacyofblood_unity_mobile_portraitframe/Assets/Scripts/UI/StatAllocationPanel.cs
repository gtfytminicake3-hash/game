namespace LegendOfBlood
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class StatAllocationPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI freePointsText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button closeButton;

        // Add references for HP, ATK, DEF, SPD rows

        private HeroData currentHero;
        private HeroStats tempAddedStats;

        private void Awake()
        {
            confirmButton.onClick.AddListener(OnConfirm);
            closeButton.onClick.AddListener(ClosePanel);
        }

        public void Show(HeroData hero)
        {
            currentHero = hero;
            tempAddedStats = new HeroStats
            {
                hp = hero.addedStats.hp,
                atk = hero.addedStats.atk,
                def = hero.addedStats.def,
                spd = hero.addedStats.spd
            };
            
            UpdateUI();
            gameObject.SetActive(true);
        }

        private void UpdateUI()
        {
            freePointsText.text = $"Points: {currentHero.freeStatPoints}";
            // Update text for each stat row
        }

        public void AddPoint(string stat)
        {
            if (currentHero.freeStatPoints > 0)
            {
                currentHero.freeStatPoints--;
                switch (stat.ToLower())
                {
                    case "hp": tempAddedStats.hp++; break;
                    case "atk": tempAddedStats.atk++; break;
                    case "def": tempAddedStats.def++; break;
                    case "spd": tempAddedStats.spd++; break;
                }
                UpdateUI();
            }
        }

        public void RemovePoint(string stat)
        {
            // Logic to remove points and give them back to freeStatPoints
            UpdateUI();
        }

        private void OnConfirm()
        {
            currentHero.addedStats = tempAddedStats;
            // Maybe save the game here
            ClosePanel();
        }

        private void ClosePanel()
        {
            gameObject.SetActive(false);
        }
    }
}

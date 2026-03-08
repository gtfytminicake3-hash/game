using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LegendOfBlood
{
    public class InventoryEquipmentCard : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image rarityBorder;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI mainStatText;
        [SerializeField] private TextMeshProUGUI subStatsText;
        [SerializeField] private Button clickButton;

        private EquipmentData _equipmentData;
        private System.Action<EquipmentData> _onClickAction;

        public void Setup(EquipmentData data, System.Action<EquipmentData> onClickAction)
        {
            _equipmentData = data;
            _onClickAction = onClickAction;

            if (nameText != null) nameText.text = data.equipmentName;
            if (levelText != null) levelText.text = $"Lv.{data.level}";

            // Main Stat Display
            if (mainStatText != null)
            {
                if (data.slot == EquipmentSlot.Weapon)
                    mainStatText.text = $"ATK: +{data.atkBonus}";
                else
                    mainStatText.text = $"HP: +{data.hpBonus} | DEF: +{data.defBonus}";
            }

            // Sub Stats Display
            if (subStatsText != null)
            {
                subStatsText.text = $"{data.bonusStat1Description}\n{data.bonusStat2Description}";
            }

            // Tùy chỉnh màu viền Rarity
            if (rarityBorder != null)
            {
                switch (data.rarity)
                {
                    case 1: rarityBorder.color = Color.white; break;
                    case 2: rarityBorder.color = Color.green; break;
                    case 3: rarityBorder.color = Color.blue; break;
                    case 4: rarityBorder.color = Color.magenta; break;
                    case 5: rarityBorder.color = Color.red; break;
                    default: rarityBorder.color = Color.white; break;
                }
            }

            if (clickButton != null)
            {
                clickButton.onClick.RemoveAllListeners();
                clickButton.onClick.AddListener(OnClicked);
            }
        }

        private void OnClicked()
        {
            _onClickAction?.Invoke(_equipmentData);
        }
    }
}

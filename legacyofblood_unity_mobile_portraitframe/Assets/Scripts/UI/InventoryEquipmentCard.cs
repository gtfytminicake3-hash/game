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
            
            if (iconImage != null)
            {
                iconImage.sprite = data.GetIcon();
            }

            // Main Stat Display
            if (mainStatText != null)
            {
                switch (data.slot)
                {
                    case EquipmentSlot.Weapon: mainStatText.text = $"ATK: +{data.atkBonus}"; break;
                    case EquipmentSlot.Armor: mainStatText.text = $"HP: +{data.hpBonus} | DEF: +{data.defBonus}"; break;
                    case EquipmentSlot.Helm: mainStatText.text = $"HP: +{data.hpBonus}"; break;
                    case EquipmentSlot.Boots: mainStatText.text = $"SPD: +{data.spdBonus}"; break;
                    case EquipmentSlot.Ring1:
                    case EquipmentSlot.Ring2:
                        if (data.atkMultiplier > 0) mainStatText.text = $"ATK: +{data.atkMultiplier * 100:F1}%";
                        else if (data.critChanceBonus > 0) mainStatText.text = $"C.RATE: +{data.critChanceBonus * 100:F1}%";
                        else if (data.critDamageBonus > 0) mainStatText.text = $"C.DMG: +{data.critDamageBonus * 100:F1}%";
                        break;
                }
            }

            // Sub Stats Display
            if (subStatsText != null)
            {
                subStatsText.text = $"{data.bonusStat1Description}\n{data.bonusStat2Description}";
            }

            // Tùy chỉnh màu viền Rarity
            if (rarityBorder != null)
            {
                switch (data.tier)
                {
                    case EquipmentTier.D: rarityBorder.color = Color.white; break;
                    case EquipmentTier.C: rarityBorder.color = Color.green; break;
                    case EquipmentTier.B: rarityBorder.color = Color.blue; break;
                    case EquipmentTier.A: rarityBorder.color = new Color(0.5f, 0, 0.5f); break; // Purple
                    case EquipmentTier.S: rarityBorder.color = Color.yellow; break;
                    case EquipmentTier.SS: rarityBorder.color = new Color(1f, 0.5f, 0f); break; // Orange
                    case EquipmentTier.SSS: rarityBorder.color = Color.red; break;
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

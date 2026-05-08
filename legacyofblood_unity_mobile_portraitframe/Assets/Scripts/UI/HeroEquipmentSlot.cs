using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LegendOfBlood
{
    public class HeroEquipmentSlot : MonoBehaviour
    {
        [SerializeField] private EquipmentSlot slotType;
        [SerializeField] private Image equipmentIcon;
        [SerializeField] private Image rarityBorder;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private GameObject emptyStateContent;
        [SerializeField] private GameObject filledStateContent;
        [SerializeField] private Button slotButton;

        private EquipmentData _currentEquip;
        private HeroData _currentHero;

        public void Setup(HeroData hero)
        {
            _currentHero = hero;
            _currentEquip = hero.GetEquipment(slotType);

            if (_currentEquip != null)
            {
                // Filled State
                emptyStateContent.SetActive(false);
                filledStateContent.SetActive(true);

                // Use the new dynamic icon loading logic
                if (equipmentIcon != null)
                {
                    equipmentIcon.sprite = _currentEquip.GetIcon();
                }
                
                if (levelText != null) levelText.text = $"+{_currentEquip.level}";

                if (rarityBorder != null)
                {
                    switch (_currentEquip.tier)
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
            }
            else
            {
                // Empty State
                emptyStateContent.SetActive(true);
                filledStateContent.SetActive(false);
            }

            if (slotButton != null)
            {
                slotButton.onClick.RemoveAllListeners();
                slotButton.onClick.AddListener(OnSlotClicked);
            }
        }

        private void OnSlotClicked()
        {
            if (_currentHero == null) return;

            if (_currentEquip != null)
            {
                // Has equipment -> show detail
                EquipmentDetailPanel detailPanel = FindFirstObjectByType<EquipmentDetailPanel>(FindObjectsInactive.Include);
                if (detailPanel != null)
                {
                    detailPanel.Setup(_currentEquip, _currentHero);
                }
            }
            else
            {
                // Gọi UIManager để đảm bảo InventoryPanel được bật lên trước
                GameManager.Instance.UIManager.ShowPanel(UIPanelType.Inventory, false);
                
                // No equipment -> open inventory pick mode
                EventManager.TriggerEvent(GameEvents.OnEquipSlotClicked, new EquipSlotClickData { hero = _currentHero, slot = slotType });
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace LegendOfBlood
{
    public class EquipmentDetailPanel : UIPanel
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI equipNameText;
        [SerializeField] private TextMeshProUGUI equipLevelText;
        [SerializeField] private TextMeshProUGUI expProgressText;
        [SerializeField] private TextMeshProUGUI statsText;
        
        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button equipButton; // Should open hero selector
        [SerializeField] private Button quickUpgradeButton; // Auto-consume lowest rarity gears

        private EquipmentData _currentEquip;

        private void Start()
        {
            PanelType = UIPanelType.None; // Tạm thời dùng như Overlay hoặc Popup
            
            if (closeButton != null)
                closeButton.onClick.AddListener(ClosePanel);
                
            if (quickUpgradeButton != null)
                quickUpgradeButton.onClick.AddListener(OnQuickUpgradeClicked);
        }

        public void Setup(EquipmentData data)
        {
            _currentEquip = data;
            gameObject.SetActive(true);
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (_currentEquip == null) return;

            if (equipNameText != null) equipNameText.text = _currentEquip.equipmentName;
            
            if (equipLevelText != null)
                equipLevelText.text = _currentEquip.level >= EquipmentSystem.MAX_LEVEL ? LocalizationSystem.GetText("equip_level_max") : string.Format(LocalizationSystem.GetText("equip_level"), _currentEquip.level);

            if (expProgressText != null)
            {
                if (_currentEquip.level >= EquipmentSystem.MAX_LEVEL)
                {
                    expProgressText.text = LocalizationSystem.GetText("equip_exp_max");
                }
                else
                {
                    int required = EquipmentSystem.GetExpRequiredForLevel(_currentEquip.level);
                    expProgressText.text = string.Format(LocalizationSystem.GetText("equip_exp_progress"), _currentEquip.currentExp, required);
                }
            }

            if (statsText != null)
            {
                string mainStat = _currentEquip.slot == EquipmentSlot.Weapon ? 
                    $"ATK +{_currentEquip.atkBonus}" : 
                    $"HP +{_currentEquip.hpBonus}\nDEF +{_currentEquip.defBonus}";
                    
                statsText.text = $"{mainStat}\n<color=#00FF00>{_currentEquip.bonusStat1Description}</color>\n<color=#00FF00>{_currentEquip.bonusStat2Description}</color>";
            }
        }

        private void OnQuickUpgradeClicked()
        {
            if (_currentEquip == null || _currentEquip.level >= EquipmentSystem.MAX_LEVEL)
            {
                GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("msg_equip_max_level"));
                return;
            }

            // Logic cắn tự động trang bị Rarity 1
            var allEquips = DataManager.Instance.Player.equipments;
            var fodder = new List<EquipmentData>();
            
            foreach (var item in allEquips)
            {
                if (item != _currentEquip && item.rarity == 1 && item.level == 1) // Chỉ hiến tế rác
                {
                    fodder.Add(item);
                }
            }

            if (fodder.Count == 0)
            {
                GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("msg_no_fodder_equip"));
                return;
            }

            EquipmentSystem.UpgradeEquipment(_currentEquip, fodder);
            
            // Xóa rác khỏi túi
            foreach(var f in fodder)
            {
                InventoryManager.Instance?.RemoveEquipment(f);
            }

            string msg = string.Format(LocalizationSystem.GetText("msg_upgrade_equip_success"), fodder.Count);
            GameManager.Instance.UINotificationManager.ShowNotification(msg);
            
            // Update UI
            RefreshUI();
            
            // Tự động gọi Save
            GameManager.Instance.DataManager.SavePlayerData();
        }

        private void ClosePanel()
        {
            gameObject.SetActive(false);
        }
    }
}

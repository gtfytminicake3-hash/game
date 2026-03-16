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
        [SerializeField] private Button lockButton; // Khóa trang bị

        private EquipmentData _currentEquip;

        protected override void Start()
        {
            base.Start();
            PanelType = UIPanelType.None; // Tạm thời dùng như Overlay hoặc Popup
            
            if (closeButton != null)
                closeButton.onClick.AddListener(ClosePanel);
                
            if (quickUpgradeButton != null)
                quickUpgradeButton.onClick.AddListener(OnQuickUpgradeClicked);
                
            if (lockButton != null)
                lockButton.onClick.AddListener(ToggleLockStatus);
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

            if (equipNameText != null) 
            {
                string lockPrefix = _currentEquip.isLocked ? "<color=red>[Đã Khóa]</color> " : "";
                equipNameText.text = lockPrefix + _currentEquip.equipmentName;
            }
            
            if (equipLevelText != null)
                equipLevelText.text = _currentEquip.level >= EquipmentSystem.MAX_LEVEL ? LocalizationSystem.GetText("equip_level_max") : string.Format(LocalizationSystem.GetText("equip_level"), _currentEquip.level);

            // Update lock button text/icon if possible (assuming there's a child TextMeshProUGUI)
            if (lockButton != null)
            {
                var lockText = lockButton.GetComponentInChildren<TextMeshProUGUI>();
                if (lockText != null)
                {
                    lockText.text = _currentEquip.isLocked ? "Mở Khóa" : "Khóa Đồ";
                }
            }

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
                string mainStat = "";
                switch (_currentEquip.slot)
                {
                    case EquipmentSlot.Weapon: mainStat = $"ATK +{_currentEquip.atkBonus}"; break;
                    case EquipmentSlot.Armor: mainStat = $"HP +{_currentEquip.hpBonus}\nDEF +{_currentEquip.defBonus}"; break;
                    case EquipmentSlot.Helm: mainStat = $"HP +{_currentEquip.hpBonus}"; break;
                    case EquipmentSlot.Boots: mainStat = $"Tốc độ +{_currentEquip.spdBonus}"; break;
                    case EquipmentSlot.Ring1:
                    case EquipmentSlot.Ring2:
                        // Nhẫn scale % ở bonus multiplier
                        if (_currentEquip.atkMultiplier > 0) mainStat = $"ATK +{_currentEquip.atkMultiplier * 100:F1}%";
                        else if (_currentEquip.critChanceBonus > 0) mainStat = $"Tỉ Lệ Chí Mạng +{_currentEquip.critChanceBonus * 100:F1}%";
                        else if (_currentEquip.critDamageBonus > 0) mainStat = $"ST. Chí Mạng +{_currentEquip.critDamageBonus * 100:F1}%";
                        break;
                }
                    
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

            // Tìm và mở Upgrade Panel (Giả định nằm trong scene chung với Detail Panel)
            EquipmentUpgradePanel upgradePanel = FindFirstObjectByType<EquipmentUpgradePanel>(FindObjectsInactive.Include);
            if (upgradePanel != null)
            {
                upgradePanel.Setup(_currentEquip, RefreshUI);
            }
            else
            {
                UnityEngine.Debug.LogError("Không tìm thấy EquipmentUpgradePanel trong scene!");
            }
        }

        private void ClosePanel()
        {
            gameObject.SetActive(false);
        }

        private void ToggleLockStatus()
        {
            if (_currentEquip == null) return;
            
            _currentEquip.isLocked = !_currentEquip.isLocked;
            GameManager.Instance.DataManager.SavePlayerData();
            RefreshUI();
            
            // Có thể trigger một Event để InventoryCard ở ngoài cật nhật lại UI ổ khóa
        }
    }
}

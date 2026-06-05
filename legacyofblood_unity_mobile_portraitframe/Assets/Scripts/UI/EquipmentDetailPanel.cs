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
        private HeroData _ownerHero;

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

            if (equipButton != null)
                equipButton.onClick.AddListener(OnEquipButtonClicked);
        }

        public void Setup(EquipmentData data, HeroData owner = null)
        {
            _currentEquip = data;
            _ownerHero = owner;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            // Đảm bảo panel luôn nằm trên cùng qua Canvas override
            Canvas cv = GetComponent<Canvas>();
            if (cv == null)
            {
                cv = gameObject.AddComponent<Canvas>();
                gameObject.AddComponent<GraphicRaycaster>();
            }
            cv.overrideSorting = true;
            cv.sortingOrder = 500;

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

            // Update lock button text/icon if possible
            if (lockButton != null)
            {
                var lockText = lockButton.GetComponentInChildren<TextMeshProUGUI>();
                if (lockText != null)
                {
                    lockText.text = _currentEquip.isLocked ? "Mở Khóa" : "Khóa Đồ";
                }
            }

            if (equipButton != null)
            {
                var equipText = equipButton.GetComponentInChildren<TextMeshProUGUI>();
                if (equipText != null)
                {
                    equipText.text = _ownerHero != null ? "Tháo ra" : "Tùy chọn"; // Cần 1 picker cho Tùy chọn nếu muốn
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

            // Tạm thời vẫn dùng FindFirstObjectByType nhưng bổ sung thông báo lỗi rõ ràng lên UI
            EquipmentUpgradePanel upgradePanel = FindFirstObjectByType<EquipmentUpgradePanel>(FindObjectsInactive.Include);
            if (upgradePanel != null)
            {
                upgradePanel.Setup(_currentEquip, RefreshUI);
            }
            else
            {
                string errorMsg = "[Lỗi] Không tìm thấy EquipmentUpgradePanel trong Scene! Bạn vui lòng kéo Prefab này vào UI Canvas.";
                UnityEngine.Debug.LogError(errorMsg);
                if (GameManager.Instance != null && GameManager.Instance.UINotificationManager != null)
                    GameManager.Instance.UINotificationManager.ShowNotification(errorMsg);
            }
        }

        private void OnEquipButtonClicked()
        {
            if (_currentEquip == null) return;
            
            if (_ownerHero != null)
            {
                // Unequip
                bool success = EquipmentSystem.UnequipItem(_ownerHero, _currentEquip.slot);
                if (success)
                {
                    GameManager.Instance.UINotificationManager?.ShowNotification($"Đã tháo {_currentEquip.equipmentName}");
                    ClosePanel();
                }
            }
            else
            {
                // TODO: Show HeroPickerPanel to choose a hero to equip this item to
                // Currently handled from InventoryPanel via _isPickMode
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

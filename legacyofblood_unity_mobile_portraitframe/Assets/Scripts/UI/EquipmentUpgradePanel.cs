using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace LegendOfBlood
{
    public class EquipmentUpgradePanel : UIPanel
    {
        [Header("Toggles cho từng Tier phẩm chất")]
        public Toggle toggleTierD;
        public Toggle toggleTierC;
        public Toggle toggleTierB;
        public Toggle toggleTierA;
        public Toggle toggleTierS;
        
        [Header("Buttons")]
        public Button confirmUpgradeButton;
        public Button closeButton;

        private EquipmentData _targetEquip;
        private System.Action _onUpgradeSuccess;

        protected override void Start()
        {
            base.Start();
            PanelType = UIPanelType.None; // Popup

            // Auto-hook if missing
            if (closeButton == null) closeButton = transform.Find("Container/CloseButton")?.GetComponent<Button>();
            if (confirmUpgradeButton == null) confirmUpgradeButton = transform.Find("Container/ConfirmUpgradeButton")?.GetComponent<Button>();
            
            if (toggleTierD == null) toggleTierD = transform.Find("Container/ToggleGroup/Toggle_D")?.GetComponent<Toggle>();
            if (toggleTierC == null) toggleTierC = transform.Find("Container/ToggleGroup/Toggle_C")?.GetComponent<Toggle>();
            if (toggleTierB == null) toggleTierB = transform.Find("Container/ToggleGroup/Toggle_B")?.GetComponent<Toggle>();
            if (toggleTierA == null) toggleTierA = transform.Find("Container/ToggleGroup/Toggle_A")?.GetComponent<Toggle>();
            if (toggleTierS == null) toggleTierS = transform.Find("Container/ToggleGroup/Toggle_S")?.GetComponent<Toggle>();

            if (closeButton != null)
                closeButton.onClick.AddListener(ClosePanel);
                
            if (confirmUpgradeButton != null)
                confirmUpgradeButton.onClick.AddListener(OnConfirmUpgrade);

            // Default: chỉ tick rác D
            if (toggleTierD != null) toggleTierD.isOn = true;
            if (toggleTierC != null) toggleTierC.isOn = false;
            if (toggleTierB != null) toggleTierB.isOn = false;
            if (toggleTierA != null) toggleTierA.isOn = false;
            if (toggleTierS != null) toggleTierS.isOn = false;
        }

        public void Setup(EquipmentData targetEquip, System.Action onUpgradeSuccess)
        {
            _targetEquip = targetEquip;
            _onUpgradeSuccess = onUpgradeSuccess;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        private void OnConfirmUpgrade()
        {
            if (_targetEquip == null || _targetEquip.level >= EquipmentSystem.MAX_LEVEL)
            {
                GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("msg_equip_max_level"));
                return;
            }

            var selectedTiers = new List<EquipmentTier>();
            if (toggleTierD != null && toggleTierD.isOn) selectedTiers.Add(EquipmentTier.D);
            if (toggleTierC != null && toggleTierC.isOn) selectedTiers.Add(EquipmentTier.C);
            if (toggleTierB != null && toggleTierB.isOn) selectedTiers.Add(EquipmentTier.B);
            if (toggleTierA != null && toggleTierA.isOn) selectedTiers.Add(EquipmentTier.A);
            if (toggleTierS != null && toggleTierS.isOn) selectedTiers.Add(EquipmentTier.S);

            if (selectedTiers.Count == 0)
            {
                GameManager.Instance.UINotificationManager.ShowNotification("Vui lòng chọn ít nhất một Phẩm chất rác để cắn!");
                return;
            }

            var allEquips = DataManager.Instance.Player.equipments;
            var fodder = new List<EquipmentData>();
            
            // Tìm đồ thỏa mãn điều kiện
            foreach (var item in allEquips)
            {
                // Loại trừ món đang nâng cấp, Loại đồ đã KHÓA, Thỏa mãn Tier đã chọn, Bỏ qua những món đang mặc (thường level > 1)
                // TODO: Để an toàn 100%, phải check xem item có nằm trong danh sách đang mặc của bất kỳ Hero nào không.
                // Tạm thời dùng level == 1 để đảm bảo là phôi thô.
                if (item != _targetEquip && !item.isLocked && item.level == 1 && selectedTiers.Contains(item.tier))
                {
                    fodder.Add(item);
                }
            }

            if (fodder.Count == 0)
            {
                GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("msg_no_fodder_equip"));
                return;
            }

            EquipmentSystem.UpgradeEquipment(_targetEquip, fodder);
            
            // Xóa rác khỏi túi
            foreach(var f in fodder)
            {
                InventoryManager.Instance?.RemoveEquipment(f);
            }

            string msg = string.Format(LocalizationSystem.GetText("msg_upgrade_equip_success"), fodder.Count);
            GameManager.Instance.UINotificationManager.ShowNotification(msg);
            
            // Gọi callback để Refresh UI detail
            _onUpgradeSuccess?.Invoke();
            
            GameManager.Instance.DataManager.SavePlayerData();
            
            ClosePanel();
        }

        private void ClosePanel()
        {
            gameObject.SetActive(false);
        }
    }
}

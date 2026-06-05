using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace LegendOfBlood
{
    public class TraitUpgradePanel : UIPanel
    {
        [Header("UI References")]
        public TextMeshProUGUI panelTitleText;
        [Tooltip("Kéo thả nút Đóng vào đây")]
        public Button closeButton;
        public TextMeshProUGUI closeButtonText;
        [Tooltip("Group chứa danh sách các Trait")]
        public Transform itemsContainer;
        [Tooltip("Prefab của 1 dòng hiển thị Trait (Cần 2 TextMeshProUGUI và 1 Button)")]
        public GameObject traitUpgradeItemPrefab; 

        private HeroData _currentHero;
        private List<GameObject> _instantiatedItems = new List<GameObject>();

        private void Awake()
        {
            PanelType = UIPanelType.None;
            if (panelTitleText != null) panelTitleText.text = global::LocalizationSystem.GetText("panel_title_trait_upgrade");
            if (closeButtonText != null) closeButtonText.text = global::LocalizationSystem.GetText("btn_close");
            
            AutoWire();
            if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
        }

        private void AutoWire()
        {
            if (closeButton == null)
            {
                Button[] btns = GetComponentsInChildren<Button>(true);
                foreach (var b in btns)
                {
                    if (b.name.ToLower().Contains("close") || b.name.ToLower().Contains("back"))
                    {
                        closeButton = b;
                        break;
                    }
                }
            }
            if (itemsContainer == null)
            {
                // Tìm đại cái object tên Content hoặc ScrollView
                Transform[] transforms = GetComponentsInChildren<Transform>(true);
                foreach (var t in transforms)
                {
                    if (t.name.ToLower().Contains("content") || t.name.ToLower().Contains("container"))
                    {
                        itemsContainer = t;
                        break;
                    }
                }
            }
        }

        public void Show(HeroData hero)
        {
            _currentHero = hero;
            RefreshUI();
            gameObject.SetActive(true);
        }

        private void RefreshUI()
        {
            foreach (var item in _instantiatedItems) Destroy(item);
            _instantiatedItems.Clear();

            if (traitUpgradeItemPrefab == null)
            {
                Debug.LogError("[TraitUpgradePanel] Chưa gán Prefab 'traitUpgradeItemPrefab' lấy gì mà hiện danh sách?");
                return;
            }

            for (int i = 0; i < _currentHero.traitIDs.Count; i++)
            {
                string traitId = _currentHero.traitIDs[i];
                Trait trait = DataManager.Instance.GetTraitByID(traitId);
                if (trait == null) continue;

                GameObject itemObj = Instantiate(traitUpgradeItemPrefab, itemsContainer);
                _instantiatedItems.Add(itemObj);

                // Điển hình Prefab cần có 2 Text (Tên, Mô tả) và 1 Nút (Nâng Cấp)
                TextMeshProUGUI[] texts = itemObj.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 2)
                {
                    texts[0].text = $"[{trait.rank}] {LocalizationSystem.GetText(trait.traitName)}";
                    texts[1].text = LocalizationSystem.GetText(trait.description);
                }
                else if (texts.Length == 1)
                {
                    texts[0].text = $"[{trait.rank}] {LocalizationSystem.GetText(trait.traitName)}\n<size=80%>{LocalizationSystem.GetText(trait.description)}</size>";
                }

                Button[] btns = itemObj.GetComponentsInChildren<Button>();
                Button upgradeBtn = null;
                foreach (var b in btns) 
                {
                    if (b.name.ToLower().Contains("upg")) { upgradeBtn = b; break; }
                }
                if (upgradeBtn == null && btns.Length > 0) upgradeBtn = btns[0]; // Dự phòng lấy nút đầu tiên

                if (upgradeBtn != null)
                {
                    if (!string.IsNullOrEmpty(trait.nextUpgradeTraitID))
                    {
                        upgradeBtn.gameObject.SetActive(true);
                        
                        bool hasEnoughExp = _currentHero.experience >= trait.upgradeExpCost;
                        upgradeBtn.interactable = hasEnoughExp;
                        
                        TextMeshProUGUI btnText = upgradeBtn.GetComponentInChildren<TextMeshProUGUI>();
                        if (btnText != null) 
                        {
                            string colorTag = hasEnoughExp ? "<color=#FFFFFF>" : "<color=#FF0000>";
                            btnText.text = $"Upgrade\n<size=70%>{colorTag}Cost: {trait.upgradeExpCost} EXP</color></size>";
                        }

                        int indexToReplace = i; // Closure capture
                        string nextId = trait.nextUpgradeTraitID;
                        int expCost = trait.upgradeExpCost;
                        upgradeBtn.onClick.AddListener(() => OnUpgradeTrait(indexToReplace, nextId, expCost));
                    }
                    else
                    {
                        // Max rank cmnr
                        upgradeBtn.interactable = false;
                        TextMeshProUGUI btnText = upgradeBtn.GetComponentInChildren<TextMeshProUGUI>();
                        if (btnText != null) btnText.text = "MAXED";
                        upgradeBtn.gameObject.SetActive(false); // Hoặc ẩn đi luôn cho đỡ chật
                    }
                }
            }
        }

        private void OnUpgradeTrait(int index, string nextUpgradeId, int expCost)
        {
            if (_currentHero.experience < expCost)
            {
                GameManager.Instance.UINotificationManager?.ShowNotification(LocalizationSystem.GetText("not_enough_exp"));
                return;
            }

            _currentHero.experience -= expCost;

            // Trượt rank cũ, thay bằng Rank mới (đã thăng cấp)
            _currentHero.traitIDs[index] = nextUpgradeId;
            
            // Lưu dữ liệu sau khi trừ EXP và thay Trait
            DataManager.Instance.SavePlayerData();

            // Cập nhật ngầm vào hệ thống
            EventManager.TriggerEvent(GameEvents.OnHeroListChanged);
            EventManager.TriggerEvent(GameEvents.OnHeroCardClicked, _currentHero);
            
            // Xong thì tắt bảng về Hero Panel luôn
            ClosePanel();
        }

        public void ClosePanel()
        {
            gameObject.SetActive(false);
        }
    }
}

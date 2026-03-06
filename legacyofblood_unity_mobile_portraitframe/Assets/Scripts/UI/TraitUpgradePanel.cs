using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace LegendOfBlood
{
    public class TraitUpgradePanel : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Kéo thả nút Đóng vào đây")]
        public Button closeButton;
        [Tooltip("Group chứa danh sách các Trait")]
        public Transform itemsContainer;
        [Tooltip("Prefab của 1 dòng hiển thị Trait (Cần 2 TextMeshProUGUI và 1 Button)")]
        public GameObject traitUpgradeItemPrefab; 

        private HeroData _currentHero;
        private List<GameObject> _instantiatedItems = new List<GameObject>();

        private void Awake()
        {
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
                    texts[0].text = $"[{trait.rank}] {trait.traitName}";
                    texts[1].text = trait.description;
                }
                else if (texts.Length == 1)
                {
                    texts[0].text = $"[{trait.rank}] {trait.traitName}\n<size=80%>{trait.description}</size>";
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
                        
                        // Đổi text trong nút thành chữ "Nâng cấp"
                        TextMeshProUGUI btnText = upgradeBtn.GetComponentInChildren<TextMeshProUGUI>();
                        if (btnText != null) btnText.text = "Upgrade";

                        int indexToReplace = i; // Closure capture
                        string nextId = trait.nextUpgradeTraitID;
                        upgradeBtn.onClick.AddListener(() => OnUpgradeTrait(indexToReplace, nextId));
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

        private void OnUpgradeTrait(int index, string nextUpgradeId)
        {
            // Trượt rank cũ, thay bằng Rank mới (đã thăng cấp)
            _currentHero.traitIDs[index] = nextUpgradeId;
            
            // Theo thiết kế: Thăng cấp Trait chỉ dùng 1 lần vào Level 60 hoặc 100.
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

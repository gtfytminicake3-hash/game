using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

namespace LegendOfBlood
{
    public class TowerPanel : UIPanel
    {
        [Header("Scroll Area")]
        [SerializeField] private RectTransform contentParent;
        [SerializeField] private GameObject floorItemPrefab;

        [Header("Footer Details")]
        [SerializeField] private TextMeshProUGUI currentFloorDetailText;
        [SerializeField] private TextMeshProUGUI difficultyText;
        [SerializeField] private TextMeshProUGUI monsterCountText;
        [SerializeField] private Button enterButton;
        [SerializeField] private Button closeButton;

        private POIData _currentTowerData;

        private void Awake()
        {
            PanelType = UIPanelType.Tower;
            if (enterButton != null) enterButton.onClick.AddListener(OnEnterClicked);
            if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
        }

        public void Show(POIData towerData)
        {
            _currentTowerData = towerData;
            gameObject.SetActive(true);
            
            PopulateFloors();
            UpdateFooterDetails();
        }

        private void PopulateFloors()
        {
            if (contentParent == null || floorItemPrefab == null) return;

            foreach(Transform child in contentParent)
            {
                Destroy(child.gameObject);
            }

            var towerConfigs = DataManager.Instance?.GameConfig?.TowerConfigs;
            if (towerConfigs == null) return;

            for (int i = 20; i >= 1; i--)
            {
                var floorConfig = towerConfigs.Find(c => c.floorIndex == i);
                if (floorConfig == null) continue;

                GameObject itemObj = Instantiate(floorItemPrefab, contentParent);
                var texts = itemObj.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 0) texts[0].text = string.Format(LocalizationSystem.GetText("tower_floor"), i);

                var img = itemObj.GetComponent<Image>();
                if (img != null)
                {
                    if (i < _currentTowerData.currentFloor) 
                        img.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                    else if (i == _currentTowerData.currentFloor)
                        img.color = new Color(1f, 0.8f, 0f, 1f);
                    else
                        img.color = new Color(0.5f, 0.5f, 0.5f, 1f);

                    if (i % 5 == 0)
                    {
                        img.color = Color.Lerp(img.color, Color.red, 0.5f);
                    }
                }
            }
        }

        private void UpdateFooterDetails()
        {
            if (_currentTowerData == null) return;
            
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long remainingTime = _currentTowerData.recoveryEndTime - currentTime;

            if (currentFloorDetailText != null)
                currentFloorDetailText.text = string.Format(LocalizationSystem.GetText("tower_current_floor"), _currentTowerData.currentFloor);
            
            if (remainingTime > 0)
            {
                TimeSpan timeSpan = TimeSpan.FromMilliseconds(remainingTime);
                if (difficultyText != null) 
                    difficultyText.text = string.Format(LocalizationSystem.GetText("tower_recovery_time"), timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
                if (enterButton != null) enterButton.interactable = false;
            }
            else
            {
                if (difficultyText != null) difficultyText.text = LocalizationSystem.GetText("tower_ready");
                if (enterButton != null) enterButton.interactable = true;
            }
            
            if (monsterCountText != null) 
            {
               int monsterCount = 1 + (_currentTowerData.currentFloor / 5);
               monsterCountText.text = string.Format(LocalizationSystem.GetText("tower_monster_count"), monsterCount);
            }
        }

        private void OnEnterClicked()
        {
            var UIMgr = GameManager.Instance.UIManager;
            UIMgr.ShowPanel(UIPanelType.SquadSelection, true);
            var squadPanel = UIMgr.GetPanel<SquadSelectionPanel>(UIPanelType.SquadSelection);
            if (squadPanel != null)
            {
                var availableHeroes = DataManager.Instance.AllHeroes.FindAll(h => h.isMature && !h.IsBusy());
                squadPanel.Show(
                    string.Format(LocalizationSystem.GetText("title_tower_challenge"), _currentTowerData.currentFloor),
                    availableHeroes, 5,
                    (selectedHeroIDs) => {
                        squadPanel.gameObject.SetActive(false);
                        this.gameObject.SetActive(false);
                        GameManager.Instance.ExpeditionManager.StartExpedition(selectedHeroIDs, _currentTowerData);
                    },
                    _currentTowerData.requiredProfession ?? Profession.None
                );
            }
        }

        private void ClosePanel()
        {
            GameManager.Instance.UIManager.HidePanel(UIPanelType.Tower);
        }
    }
}

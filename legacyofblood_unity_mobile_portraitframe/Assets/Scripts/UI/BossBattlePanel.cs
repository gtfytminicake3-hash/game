using UnityEngine;
using UnityEngine.UI;

namespace LegendOfBlood
{
    public class BossBattlePanel : UIPanel
    {
        [Header("Top Bar")]
        public TMPro.TextMeshProUGUI labelBossBattle;
        
        [Header("Boss HP")]
        public Image imgHpFill;
        public TMPro.TextMeshProUGUI txtHpValue;

        [Header("Boss Info")]
        public TMPro.TextMeshProUGUI txtBossName;
        public TMPro.TextMeshProUGUI txtDifficulty;
        public TMPro.TextMeshProUGUI txtDescription;

        [Header("Action Buttons")]
        public Button btnAllySupport;
        public Button btnBeginRaid;
        public Button btnPrepGear;

        [Header("Nav Tabs")]
        public Button tabShop;
        public Button tabBarracks;
        public Button tabLobby;
        public Button tabAlliance;
        public Button tabBattlefield;
        
        private POIData _currentPoiData;

        private void Awake()
        {
            PanelType = UIPanelType.BossBattle;

            if (btnBeginRaid != null)
                btnBeginRaid.onClick.AddListener(OnBeginRaidClicked);
                
            if (btnAllySupport != null)
                btnAllySupport.onClick.AddListener(OnAllySupportClicked);
                
            if (btnPrepGear != null)
                btnPrepGear.onClick.AddListener(OnPrepGearClicked);
        }

        private void OnEnable()
        {
            // Placeholder init logic in case it's opened without data
            if (txtBossName != null && string.IsNullOrEmpty(txtBossName.text))
                RefreshBossInfo(null);
        }

        public void Show(POIData poiData)
        {
            RefreshBossInfo(poiData);
        }

        private void RefreshBossInfo(POIData poiData)
        {
            _currentPoiData = poiData; // Keep track for starting raid
            
            if (poiData != null)
            {
                if (txtBossName != null) txtBossName.text = $"BOSS: {poiData.poiName.ToUpper()}";
                if (txtDifficulty != null) txtDifficulty.text = $"Difficulty: {poiData.difficultyLevel}";
                
                int bossHp = poiData.difficultyLevel * 10000;
                if (txtHpValue != null) txtHpValue.text = $"{bossHp:N0} / {bossHp:N0}"; 
                if (txtDescription != null) txtDescription.text = "Prepare your best heroes to overcome the guardian of this region.";
            }
            else
            {
                if (txtBossName != null) txtBossName.text = "BOSS: UNKNOWN";
                if (txtDifficulty != null) txtDifficulty.text = "Difficulty: 1";
                if (txtHpValue != null) txtHpValue.text = "10,000 / 10,000";
                if (txtDescription != null) txtDescription.text = "Prepare your best heroes.";
            }
            
            if (imgHpFill != null) imgHpFill.fillAmount = 1.0f;
        }

        private void OnBeginRaidClicked()
        {
            Debug.Log("[BossBattlePanel] Bắt đầu đánh Boss!");
            if (_currentPoiData == null) return;
            
            var UIMgr = GameManager.Instance.UIManager;
            UIMgr.ShowPanel(UIPanelType.SquadSelection, true);
            var squadPanel = UIMgr.GetPanel<SquadSelectionPanel>(UIPanelType.SquadSelection);
            if (squadPanel != null)
            {
                var availableHeroes = DataManager.Instance.AllHeroes.FindAll(h => h.isMature && !h.IsBusy());
                squadPanel.Show(
                    $"RAID BOSS: {_currentPoiData.poiName}",
                    availableHeroes, 5,
                    (selectedHeroIDs, diff) => {
                        squadPanel.gameObject.SetActive(false);
                        this.gameObject.SetActive(false);
                        _currentPoiData.difficultyLevel = diff;
                        GameManager.Instance.ExpeditionManager.StartExpedition(selectedHeroIDs, _currentPoiData);
                    },
                    _currentPoiData.requiredProfession
                );
            }
        }

        private void OnAllySupportClicked()
        {
            Debug.Log("[BossBattlePanel] Yêu cầu hỗ trợ từ liên minh!");
            if (GameManager.Instance != null && GameManager.Instance.UINotificationManager != null) {
                GameManager.Instance.UINotificationManager.ShowNotification("Đã gửi yêu cầu hỗ trợ Liên Minh!");
            }
        }

        private void OnPrepGearClicked()
        {
            Debug.Log("[BossBattlePanel] Mở kho đồ chuẩn bị vũ khí!");
            if (GameManager.Instance != null && GameManager.Instance.UIManager != null) {
                GameManager.Instance.UIManager.ShowPanel(UIPanelType.Inventory, false);
            }
        }
    }
}

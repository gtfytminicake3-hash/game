using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace LegendOfBlood
{
    public class BossBattlePanel : UIPanel
    {
        [Header("Header")]
        [SerializeField] private TextMeshProUGUI bossNameText;
        [SerializeField] private TextMeshProUGUI bossLevelText;

        [Header("Body Left - Visuals & Stats")]
        [SerializeField] private Image bossImage;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI atkText;
        [SerializeField] private TextMeshProUGUI defText;
        [SerializeField] private TextMeshProUGUI spdText;

        [Header("Body Right - Skills & Mechanics")]
        [SerializeField] private TextMeshProUGUI normalSkillOutlineText;
        [SerializeField] private TextMeshProUGUI aoeSkillOutlineText;
        
        [Header("Mechanic Frame")]
        [SerializeField] private TextMeshProUGUI mechanicNameText;
        [SerializeField] private TextMeshProUGUI mechanicDescText;

        [Header("Footer")]
        [SerializeField] private Button challengeButton;
        [SerializeField] private Button closeButton;

        private POIData _bossPoi;
        private BossData _currentBossInfo;

        private void Awake()
        {
            PanelType = UIPanelType.BossBattle;
            if (challengeButton != null) challengeButton.onClick.AddListener(OnChallengeClicked);
            if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
        }

        public void Show(POIData poiData)
        {
            _bossPoi = poiData;
            gameObject.SetActive(true);

            if (poiData.monsterIDs != null && poiData.monsterIDs.Count > 0)
            {
                string bossId = poiData.monsterIDs[0];
                if (DataManager.Instance.AllBosses.TryGetValue(bossId, out BossData bData))
                {
                    _currentBossInfo = bData;
                    UpdateUI();
                }
                else
                {
                    Debug.LogError($"[BossBattlePanel] Cannot find BossData for ID: {bossId}");
                }
            }
        }

        private void UpdateUI()
        {
            if (_currentBossInfo == null) return;

            if (bossNameText != null) bossNameText.text = _currentBossInfo.bossName;
            if (bossLevelText != null) bossLevelText.text = $"Lv. {_currentBossInfo.level}";

            if (hpText != null) hpText.text = $"HP: {_currentBossInfo.baseHp}";
            if (atkText != null) atkText.text = $"ATK: {_currentBossInfo.baseAtk}";
            if (defText != null) defText.text = $"DEF: {_currentBossInfo.baseDef}";
            if (spdText != null) spdText.text = $"SPD: {_currentBossInfo.baseSpd}";

            if (_currentBossInfo.skills != null)
            {
                if (normalSkillOutlineText != null) 
                    normalSkillOutlineText.text = string.Format(LocalizationSystem.GetText("ui_boss_normal_atk"), _currentBossInfo.skills.normalAttackName, _currentBossInfo.skills.normalAttackMultiplier);
                if (aoeSkillOutlineText != null) 
                    aoeSkillOutlineText.text = string.Format(LocalizationSystem.GetText("ui_boss_aoe_atk"), _currentBossInfo.skills.aoeAttackName, _currentBossInfo.skills.aoeAttackMultiplier);
            }

            if (_currentBossInfo.mechanic != null)
            {
                if (mechanicNameText != null) mechanicNameText.text = _currentBossInfo.mechanic.name;
                if (mechanicDescText != null) mechanicDescText.text = _currentBossInfo.mechanic.description;
            }
        }

        private void OnChallengeClicked()
        {
            var UIMgr = GameManager.Instance.UIManager;
            UIMgr.ShowPanel(UIPanelType.SquadSelection, true);
            var squadPanel = UIMgr.GetPanel<SquadSelectionPanel>(UIPanelType.SquadSelection);
            
            if (squadPanel != null)
            {
                var availableHeroes = DataManager.Instance.AllHeroes.FindAll(h => h.isMature && !h.IsBusy());
                squadPanel.Show(
                    string.Format(LocalizationSystem.GetText("title_challenge_boss"), _currentBossInfo.bossName),
                    availableHeroes, 15, // Allow up to 3 squads (15 heroes)
                    (selectedHeroIDs) => {
                        squadPanel.gameObject.SetActive(false);
                        this.gameObject.SetActive(false);
                        GameManager.Instance.ExpeditionManager.StartExpedition(selectedHeroIDs, _bossPoi);
                    },
                    Profession.None
                );
            }
        }

        private void ClosePanel()
        {
            GameManager.Instance.UIManager.HidePanel(UIPanelType.BossBattle);
        }
    }
}

namespace LegendOfBlood
{
    using UnityEngine;
    using System.Collections.Generic;
    using LegendOfBlood.Managers;
    using LegendOfBlood;

    public class QuestPanel : UIPanel
    {
        [SerializeField] private GameObject questItemPrefab;
        [SerializeField] private Transform questListContainer;

        private QuestManager _questManager;

        [Header("Tabs")]
        [SerializeField] private UnityEngine.UI.Button mainTabButton;
        [SerializeField] private UnityEngine.UI.Button dailyTabButton;
        [SerializeField] private UnityEngine.UI.Button weeklyTabButton;
        [SerializeField] private UnityEngine.UI.Button closeButton;
        
        [Header("Localization")]
        [SerializeField] private TMPro.TextMeshProUGUI panelTitleText;
        [SerializeField] private TMPro.TextMeshProUGUI mainTabText;
        [SerializeField] private TMPro.TextMeshProUGUI dailyTabText;
        [SerializeField] private TMPro.TextMeshProUGUI weeklyTabText;

        private LegendOfBlood.GameConfigs.QuestCategory _currentCategory = LegendOfBlood.GameConfigs.QuestCategory.Main;

        private void Awake()
        {
            PanelType = UIPanelType.Quest;
            
            if (closeButton) closeButton.onClick.AddListener(() => GameManager.Instance.UIManager.GoBack());
            
            if (mainTabButton) mainTabButton.onClick.AddListener(() => SetCategory(LegendOfBlood.GameConfigs.QuestCategory.Main));
            if (dailyTabButton) dailyTabButton.onClick.AddListener(() => SetCategory(LegendOfBlood.GameConfigs.QuestCategory.Daily));
            if (weeklyTabButton) weeklyTabButton.onClick.AddListener(() => SetCategory(LegendOfBlood.GameConfigs.QuestCategory.Weekly));
            
            if (panelTitleText != null) panelTitleText.text = global::LocalizationSystem.GetText("panel_title_quest");
            if (mainTabText != null) mainTabText.text = global::LocalizationSystem.GetText("tab_main_quest");
            if (dailyTabText != null) dailyTabText.text = global::LocalizationSystem.GetText("tab_daily_quest");
            if (weeklyTabText != null) weeklyTabText.text = global::LocalizationSystem.GetText("tab_weekly_quest");
            
            Debug.Log("QuestPanel Initialized in Awake");
        }

        private void SetCategory(LegendOfBlood.GameConfigs.QuestCategory category)
        {
            _currentCategory = category;
            RefreshQuestList();
        }

        private void OnEnable()
        {
            if (_questManager == null && GameManager.Instance != null)
            {
                _questManager = GameManager.Instance.QuestManager;
            }
            RefreshQuestList();
        }

        public void RefreshQuestList()
        {
            // Clear existing quest items
            foreach (Transform child in questListContainer)
            {
                Destroy(child.gameObject);
            }

            // Populate with active quests
            var activeQuests = DataManager.Instance.Player.QuestStatuses;
            if (_questManager == null || activeQuests == null) return;

            foreach (var questStatus in activeQuests)
            {
                var data = _questManager.GetQuestData(questStatus.questId);
                if (data != null && data.category == _currentCategory)
                {
                     // Only show Active or Completed (not Claimed or NotStarted, usually)
                     // Or maybe show Claimed for daily/weekly until reset?
                     if (questStatus.state == Managers.QuestState.Active || questStatus.state == Managers.QuestState.Completed)
                     {
                        var questItem = Instantiate(questItemPrefab, questListContainer);
                        var questItemController = questItem.GetComponent<QuestItemUI>(); 
                        if(questItemController != null) 
                           questItemController.Setup(questStatus);
                        else
                           Debug.LogWarning("QuestItemUI component missing on prefab.");
                     }
                }
            }
        }
    }
}

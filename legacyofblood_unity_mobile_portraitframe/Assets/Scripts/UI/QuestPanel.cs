namespace LegendOfBlood.UI
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

        private LegendOfBlood.GameConfigs.QuestCategory _currentCategory = LegendOfBlood.GameConfigs.QuestCategory.Main;

        private void Awake()
        {
            PanelType = UIPanelType.Quest;
            _questManager = GameManager.Instance.QuestManager;
            
            if (mainTabButton) mainTabButton.onClick.AddListener(() => SetCategory(LegendOfBlood.GameConfigs.QuestCategory.Main));
            if (dailyTabButton) dailyTabButton.onClick.AddListener(() => SetCategory(LegendOfBlood.GameConfigs.QuestCategory.Daily));
            if (weeklyTabButton) weeklyTabButton.onClick.AddListener(() => SetCategory(LegendOfBlood.GameConfigs.QuestCategory.Weekly));
            
            Debug.Log("QuestPanel Initialized in Awake");
        }

        private void SetCategory(LegendOfBlood.GameConfigs.QuestCategory category)
        {
            _currentCategory = category;
            RefreshQuestList();
        }

        private void OnEnable()
        {
            RefreshQuestList();
        }

        private void RefreshQuestList()
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

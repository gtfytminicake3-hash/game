namespace LegendOfBlood.UI
{
    using UnityEngine;
    using System.Collections.Generic;
    using LegendOfBlood.Managers;
    using LegendOfBlood;

    public class QuestPanel : MonoBehaviour
    {
        [SerializeField] private GameObject questItemPrefab;
        [SerializeField] private Transform questListContainer;

        private QuestManager _questManager;

        private void Start()
        {
            _questManager = GameManager.Instance.QuestManager;
            Debug.Log("QuestPanel Initialized");
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
                // TODO: Instantiate a prefab for each quest and populate its data
                // var questItem = Instantiate(questItemPrefab, questListContainer);
                // var questItemController = questItem.GetComponent<QuestItemUI>();
                // questItemController.Setup(questStatus);
            }
        }
    }
}

// --- START OF FILE QuestManager.cs (FIXED) ---

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using LegendOfBlood.GameConfigs; // Để truy cập GameConfig và QuestData
using LegendOfBlood; // Để truy cập các enum chung như ResourceType

namespace LegendOfBlood.Managers
{
    // =====================================================================================
    // Định nghĩa PlayerQuestStatus
    // =====================================================================================
    [System.Serializable]
    public class PlayerQuestStatus
    {
        public string questId;
        public QuestState state; // Sử dụng enum QuestState để quản lý trạng thái
        public int currentProgress; // Có thể dùng cho các quest yêu cầu tiến độ (ví dụ: giết X quái)

        public PlayerQuestStatus(string questId, QuestState initialState = QuestState.NotStarted, int initialProgress = 0)
        {
            this.questId = questId;
            this.state = initialState;
            this.currentProgress = initialProgress;
        }
    }

    // Enum cho trạng thái nhiệm vụ
    public enum QuestState
    {
        NotStarted, // Chưa được kích hoạt
        Active,     // Đang hoạt động, chưa hoàn thành
        Completed,  // Đã hoàn thành mục tiêu, chưa nhận thưởng
        Claimed,    // Đã nhận thưởng
        Failed      // Nhiệm vụ thất bại (nếu có logic này)
    }

    // SỬA LỖI: Xóa bỏ định nghĩa 'enum ResourceType' cục bộ ở đây.
    // Code sẽ sử dụng 'ResourceType' được định nghĩa chung cho toàn bộ dự án.

    // =====================================================================================
    // QuestManager Class
    // =====================================================================================
    public class QuestManager : MonoBehaviour
    {
        private List<PlayerQuestStatus> _playerQuests => DataManager.Instance.Player.QuestStatuses;
        private Dictionary<string, QuestData> _allQuests;

        [Header("Starting Quests")]
        [SerializeField] private List<string> _startingQuestIds = new List<string> { "Q_T_01", "Q_T_02", "Q_T_03", "Q_T_04", "Q_D_01", "Q_D_02", "Q_W_01" };

        private void Start()
        {
            InitializeQuestManager();
        }

        private void InitializeQuestManager()
        {
            _allQuests = DataManager.Instance.AllQuests;
            if (_allQuests == null || _allQuests.Count == 0)
            {
                Debug.LogError("Quest data not loaded or empty from DataManager! Make sure GameConfig has AllQuestData.");
                _allQuests = new Dictionary<string, QuestData>();
            }

            if (DataManager.Instance.Player.QuestStatuses == null)
            {
                DataManager.Instance.Player.QuestStatuses = new List<PlayerQuestStatus>();
            }

            // Fallback in case Inspector overrides the default script values with an empty list
            if (_startingQuestIds == null || _startingQuestIds.Count == 0)
            {
                _startingQuestIds = new List<string> { "Q_T_01", "Q_T_02", "Q_T_03", "Q_T_04", "Q_D_01", "Q_D_02", "Q_W_01" };
            }

            foreach (var questId in _startingQuestIds)
            {
                if (!IsQuestActiveOrCompleted(questId))
                {
                    ActivateQuest(questId);
                }
            }

            SubscribeToEvents();
            
            CheckForResets();

            Debug.Log($"QuestManager initialized. {_playerQuests.Count} active player quests.");
        }
        
        private void CheckForResets()
        {
            DateTime now = DateTime.UtcNow;
            DateTime today = now.Date;
            DateTime weekStart = now.Date.AddDays(-(int)now.DayOfWeek);

            long todayTs = ((DateTimeOffset)today).ToUnixTimeSeconds();
            long weekStartTs = ((DateTimeOffset)weekStart).ToUnixTimeSeconds();

            var player = DataManager.Instance.Player;

            // Daily Reset
            if (player.lastDailyResetTimestamp < todayTs)
            {
                ResetQuestsByCategory(QuestCategory.Daily);
                player.lastDailyResetTimestamp = todayTs;
                Debug.Log("Daily Quests Reset!");
            }

            // Weekly Reset
            if (player.lastWeeklyResetTimestamp < weekStartTs)
            {
                ResetQuestsByCategory(QuestCategory.Weekly);
                player.lastWeeklyResetTimestamp = weekStartTs;
                Debug.Log("Weekly Quests Reset!");
            }
        }

        private void ResetQuestsByCategory(QuestCategory category)
        {
            var questsToReset = _allQuests.Values.Where(q => q.category == category).Select(q => q.questId).ToList();
            
            foreach (var qId in questsToReset)
            {
                var playerQuest = _playerQuests.FirstOrDefault(pq => pq.questId == qId);
                if (playerQuest != null)
                {
                    // Reset state and progress
                    playerQuest.state = QuestState.NotStarted;
                    playerQuest.currentProgress = 0;
                }
            }
            // Logic to re-activate daily quests if needed
            // For now, we just reset them to NotStarted. 
            // If they are "Auto-Activate", we should Activate them here or in a separate pass.
             foreach (var qId in questsToReset)
             {
                 ActivateQuest(qId);
             }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            BuildingSystem.OnBuildingUpgradeCompleted += HandleBuildingUpgraded;
        }

        private void UnsubscribeFromEvents()
        {
            BuildingSystem.OnBuildingUpgradeCompleted -= HandleBuildingUpgraded;
        }

        private void HandleBuildingUpgraded(Building building)
        {
            var questsToCheck = _playerQuests
                .Where(pq => pq.state == QuestState.Active && 
                             GetQuestData(pq.questId)?.type == QuestType.UPGRADE_BUILDING)
                .ToList();

            foreach (var playerQuest in questsToCheck)
            {
                var questData = GetQuestData(playerQuest.questId);
                if (questData != null && questData.targetId == building.id && building.level >= questData.targetValue)
                {
                    SetQuestState(playerQuest.questId, QuestState.Completed);
                }
            }
        }

        private void SetQuestState(string questId, QuestState newState)
        {
            var playerQuest = _playerQuests.FirstOrDefault(q => q.questId == questId);
            if (playerQuest != null && playerQuest.state != newState)
            {
                playerQuest.state = newState;
                if (newState == QuestState.Completed)
                {
                    Debug.Log($"Quest Completed: {GetQuestData(playerQuest.questId)?.questName}");
                    if (GameManager.Instance != null && GameManager.Instance.UINotificationManager != null)
                    {
                        GameManager.Instance.UINotificationManager.ShowNotification($"Quest Completed: {GetQuestData(playerQuest.questId)?.questName}");
                    }
                }
                else if (newState == QuestState.Active)
                {
                     Debug.Log($"Quest Activated: {GetQuestData(playerQuest.questId)?.questName}");
                     if (GameManager.Instance != null && GameManager.Instance.UINotificationManager != null)
                     {
                         GameManager.Instance.UINotificationManager.ShowNotification($"New Quest: {GetQuestData(playerQuest.questId)?.questName}");
                     }
                }
                DataManager.Instance.SavePlayerData();
            }
        }

        public void ClaimReward(string questId)
        {
            var playerQuest = _playerQuests.FirstOrDefault(q => q.questId == questId);
            if (playerQuest != null && playerQuest.state == QuestState.Completed)
            {
                SetQuestState(questId, QuestState.Claimed);
                
                var questData = GetQuestData(questId);
                if (questData == null)
                {
                    Debug.LogError($"QuestData not found for questId: {questId}");
                    return;
                }
                
                foreach (var reward in questData.rewards)
                {
                    if (Enum.TryParse<ResourceType>(reward.resourceId, true, out var resourceType))
                    {
                        InventoryManager.Instance.AddResource(resourceType, reward.amount);
                    }
                    else
                    {
                        Debug.LogWarning($"Invalid ResourceType: {reward.resourceId} for quest {questId}. Check QuestData and ResourceType enum.");
                    }
                }

                if (questData.nextQuestInChain != null && !string.IsNullOrEmpty(questData.nextQuestInChain.questId))
                {
                    ActivateQuest(questData.nextQuestInChain.questId);
                }
                Debug.Log($"Rewards claimed for quest: {questData.questName}");
                DataManager.Instance.SavePlayerData();
            }
            else if (playerQuest == null)
            {
                Debug.LogWarning($"Attempted to claim reward for non-existent quest: {questId}");
            }
            else if (playerQuest.state != QuestState.Completed)
            {
                Debug.LogWarning($"Quest {questId} is not in Completed state. Current state: {playerQuest.state}");
            }
        }

        public void ActivateQuest(string questId)
        {
            if (_playerQuests.Any(q => q.questId == questId && (q.state == QuestState.Active || q.state == QuestState.Completed || q.state == QuestState.Claimed)))
            {
                Debug.LogWarning($"Quest {questId} is already active, completed, or claimed. Not activating again.");
                return;
            }

            var questData = GetQuestData(questId);
            if (questData == null)
            {
                Debug.LogError($"Cannot activate quest {questId}: QuestData not found.");
                return;
            }
            
            var existingQuest = _playerQuests.FirstOrDefault(q => q.questId == questId);
            if (existingQuest == null)
            {
                _playerQuests.Add(new PlayerQuestStatus(questId, QuestState.Active));
            }
            else
            {
                if (existingQuest.state == QuestState.NotStarted || existingQuest.state == QuestState.Failed)
                {
                    existingQuest.state = QuestState.Active;
                    existingQuest.currentProgress = 0;
                }
            }
            SetQuestState(questId, QuestState.Active);
        }

        public QuestData GetQuestData(string questId)
        {
            if (_allQuests.TryGetValue(questId, out var questData))
            {
                return questData;
            }
            return null;
        }

        public bool IsQuestActiveOrCompleted(string questId)
        {
            return _playerQuests.Any(q => q.questId == questId && (q.state == QuestState.Active || q.state == QuestState.Completed || q.state == QuestState.Claimed));
        }

        public List<PlayerQuestStatus> GetActiveQuests()
        {
            return _playerQuests.Where(q => q.state == QuestState.Active).ToList();
        }

        public List<PlayerQuestStatus> GetCompletedUnclaimedQuests()
        {
            return _playerQuests.Where(q => q.state == QuestState.Completed).ToList();
        }
    }
}
// --- END OF FILE QuestManager.cs (FIXED) ---
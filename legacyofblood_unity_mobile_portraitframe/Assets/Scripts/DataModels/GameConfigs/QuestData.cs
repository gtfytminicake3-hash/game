// --- START OF FILE QuestData.cs (FIXED) ---

using UnityEngine;
using System.Collections.Generic;

namespace LegendOfBlood.GameConfigs
{
    public enum QuestType
    {
        UPGRADE_BUILDING,
        BREED_HERO,
        RECRUIT_HERO,
        COMPLETE_EXPEDITION,
        REACH_HERO_LEVEL
    }

    [System.Serializable]
    public class QuestReward
    {
        public string resourceId;
        public int amount;
    }

    [CreateAssetMenu(fileName = "QuestData", menuName = "LegendOfBlood/GameConfigs/QuestData")]
    public class QuestData : ScriptableObject
    {
        [Header("Quest Info")]
        public string questId;
        public string questName;
        [TextArea(3, 5)]
        public string description;

        [Header("Quest Goal")]
        public QuestType type;
        public string targetId; // e.g., "MainHall" for UPGRADE_BUILDING, or a specific hero ID
        public int targetValue; // e.g., level 5, or number of heroes to breed

        [Header("Quest Rewards")]
        public List<QuestReward> rewards;

        [Header("Quest Chain")]
        public QuestData nextQuestInChain;
    }
}
// --- END OF FILE QuestData.cs (FIXED) ---
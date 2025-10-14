// --- START OF FILE BuildingUpgradeData.cs (FIXED) ---

using UnityEngine;
using System.Collections.Generic;

namespace LegendOfBlood.GameConfigs
{
    [System.Serializable]
    public class UpgradeCost
    {
        public string resourceId; // e.g., "gold", "wood"
        public int amount;
    }

    [System.Serializable]
    public class BuildingLevelData
    {
        public int level;
        public List<UpgradeCost> costs;
        public long duration; // in seconds
    }

    [CreateAssetMenu(fileName = "BuildingUpgradeData", menuName = "LegendOfBlood/GameConfigs/BuildingUpgradeData")]
    public class BuildingUpgradeData : ScriptableObject
    {
        public string buildingId;
        public List<BuildingLevelData> levels;

        private Dictionary<int, BuildingLevelData> _levelMap;

        public BuildingLevelData GetLevelData(int level)
        {
            if (_levelMap == null)
            {
                _levelMap = new Dictionary<int, BuildingLevelData>();
                foreach (var levelData in levels)
                {
                    _levelMap[levelData.level] = levelData;
                }
            }

            _levelMap.TryGetValue(level, out var data);
            return data;
        }
    }
}
// --- END OF FILE BuildingUpgradeData.cs (FIXED) ---
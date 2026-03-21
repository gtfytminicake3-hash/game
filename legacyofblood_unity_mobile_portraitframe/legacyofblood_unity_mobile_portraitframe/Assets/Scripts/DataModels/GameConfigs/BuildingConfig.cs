using UnityEngine;
using System.Collections.Generic;
using LegendOfBlood.GameConfigs;

namespace LegendOfBlood
{
    [CreateAssetMenu(fileName = "BuildingConfig", menuName = "GameConfig/Building Config")]
    public class BuildingConfig : ScriptableObject
    {
        public BuildingType type;
        public List<BuildingUpgradeData> upgradeTiers;
    }
}
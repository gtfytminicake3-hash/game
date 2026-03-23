using UnityEngine;
using System.Collections.Generic;

namespace LegendOfBlood
{
    [CreateAssetMenu(fileName = "POIMonsterConfig", menuName = "GameConfig/POI Monster Config")]
    public class POIMonsterConfig : ScriptableObject
    {
        public List<POIMonsterGroup> monsterGroups;
    }
}

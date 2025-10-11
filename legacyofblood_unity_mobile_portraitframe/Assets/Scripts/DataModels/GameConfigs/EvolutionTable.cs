
using UnityEngine;
using System.Collections.Generic;

namespace LegendOfBlood
{
    [CreateAssetMenu(fileName = "EvolutionTable", menuName = "GameConfig/Evolution Table")]
    public class EvolutionTable : ScriptableObject
    {
        public List<EvolutionRewardData> rewards;
    }
}

using UnityEngine;

namespace LegendOfBlood
{
    [CreateAssetMenu(fileName = "New Arena Rank", menuName = "Legend of Blood/Arena Rank")]
    public class ArenaRankData : ScriptableObject
    {
        public string rankName;
        public int minPoints;
        public int maxPoints;
        public Sprite rankIconSprite;
    }
}

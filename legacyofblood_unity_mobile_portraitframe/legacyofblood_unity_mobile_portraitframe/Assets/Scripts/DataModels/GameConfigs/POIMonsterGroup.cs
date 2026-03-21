using System.Collections.Generic;

namespace LegendOfBlood
{
    [System.Serializable]
    public class POIMonsterGroup
    {
        public string groupName; // e.g., "Forest Goblins", "Undead Legion"
        public int minDifficulty;
        public int maxDifficulty;
        public List<string> monsterIDs; // List of monster IDs in this group
    }
}

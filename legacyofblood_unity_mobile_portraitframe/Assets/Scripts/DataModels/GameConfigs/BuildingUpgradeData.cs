namespace LegendOfBlood
{
    [System.Serializable]
    public class BuildingUpgradeData
    {
        public int level; // Cấp độ đích
        public int goldCost;
        public int woodCost;
        public int stoneCost;
        public float constructionTimeInSeconds;
    }
}
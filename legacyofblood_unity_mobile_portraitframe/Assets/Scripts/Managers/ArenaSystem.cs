using System;
using UnityEngine;

namespace LegendOfBlood
{
    public class ArenaSystem : MonoBehaviour
    {
        public static ArenaSystem Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        public void CheckDailyTicketRefresh()
        {
            // Logic to check lastTicketRefreshTimestamp and add tickets if needed
            PlayerData playerData = DataManager.Instance.Player;
            if (playerData == null) return;

            DateTime lastRefresh = DateTime.FromBinary(playerData.lastTicketRefreshTimestamp);
            DateTime now = DateTime.UtcNow;

            if (now.Date > lastRefresh.Date)
            {
                playerData.arenaTickets = 5; // Or add to a max of 5
                playerData.lastTicketRefreshTimestamp = now.ToBinary();
                Debug.Log("Arena tickets refreshed.");
            }
        }

        public void FindOpponent(int playerPoints)
        {
            // Mock implementation for now
            Debug.Log("Finding opponent for player with " + playerPoints + " points.");
            // In the future, this will involve matchmaking logic.
        }

        public void ProcessMatchResult(bool victory, int playerPoints, int opponentPoints)
        {
            PlayerData playerData = DataManager.Instance.Player;
            if (playerData == null) return;

            playerData.arenaTickets--;

            // Simplified point calculation
            int pointChange = 0;
            if (victory)
            {
                pointChange = 25 + (opponentPoints - playerPoints) / 10;
                if (pointChange < 5) pointChange = 5;
                playerData.arenaCoins += 30;
            }
            else
            {
                pointChange = -20 + (opponentPoints - playerPoints) / 10;
                if (pointChange > -5) pointChange = -5;
                playerData.arenaCoins += 10;
            }
            
            playerData.arenaPoints += pointChange;
            if (playerData.arenaPoints < 0) playerData.arenaPoints = 0;

            Debug.Log("Match result processed. Player now has " + playerData.arenaPoints + " points.");
        }

        public void DistributeWeeklyRewards()
        {
            // Logic to distribute rewards based on rank
            Debug.Log("Distributing weekly arena rewards.");
        }
    }
}

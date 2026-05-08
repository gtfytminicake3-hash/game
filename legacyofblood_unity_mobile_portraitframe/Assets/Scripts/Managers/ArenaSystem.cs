using System;
using UnityEngine;
using LegendOfBlood.Combat;

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
            // Trigger actual matchmaking flow
            var UIMgr = GameManager.Instance.UIManager;
            UIMgr.ShowPanel(UIPanelType.SquadSelection, true);
            var squadPanel = UIMgr.GetPanel<SquadSelectionPanel>(UIPanelType.SquadSelection);
            
            if (squadPanel != null)
            {
                var availableHeroes = DataManager.Instance.AllHeroes.FindAll(h => h.isMature && !h.IsBusy());
                squadPanel.Show(
                    "ĐẤU TRƯỜNG: VƯỢT ẢI",
                    availableHeroes, 5,
                    (selectedHeroIDs, diff) => {
                        squadPanel.gameObject.SetActive(false);
                        UIMgr.HidePanel(UIPanelType.Arena); // Hide Arena Panel during battle
                        
                        // Execute Arena Battle
                        ExecuteArenaBattle(selectedHeroIDs, playerPoints);
                    },
                    Profession.None
                );
            }
        }

        private void ExecuteArenaBattle(System.Collections.Generic.List<string> selectedHeroIDs, int playerPoints)
        {
            PlayerData playerData = DataManager.Instance.Player;
            if (playerData == null) return;
            
            if (playerData.arenaTickets <= 0)
            {
                GameManager.Instance.UINotificationManager.ShowNotification("Không đủ vé Đấu Trường!");
                return;
            }

            var playerHeroes = new System.Collections.Generic.List<HeroData>();
            foreach(var id in selectedHeroIDs)
            {
                var h = DataManager.Instance.GetHeroByID(id);
                if (h != null) playerHeroes.Add(h);
            }

            var opponents = FindOpponentSquad(playerPoints);
            var result = GameManager.Instance.CombatSystem.Simulate(playerHeroes, opponents);
            
            ProcessMatchResult(result.DidPlayerWin, playerPoints, playerPoints + 50);

            // Visualize combat
            GameManager.Instance.UIManager.ShowPanel(UIPanelType.Battle, true);
            var combatPanel = GameManager.Instance.UIManager.GetPanel<LegendOfBlood.Combat.CombatVisualizerPanel>(UIPanelType.Battle);
            if (combatPanel != null)
            {
                combatPanel.PlayCombat(result, playerHeroes, opponents);
            }
            
            // Re-heal heroes
            foreach(var h in playerHeroes) h.currentHp = h.GetFinalStats().hp;
        }

        public System.Collections.Generic.List<HeroData> FindOpponentSquad(int playerPoints)
        {
            Debug.Log("Generating opponent for player with " + playerPoints + " points.");
            
            // Generate a squad scaled by player's points
            // base points are usually around 0 to 1000+
            int scaledLevel = Mathf.Max(1, (playerPoints / 50) + 1);
            float difficultyMultiplier = 1f + (playerPoints / 200f);
            
            var enemies = new System.Collections.Generic.List<HeroData>();
            for (int i = 0; i < 5; i++)
            {
                var stats = new HeroStats 
                { 
                    hp = Mathf.RoundToInt(100 * difficultyMultiplier), 
                    atk = Mathf.RoundToInt(10 * difficultyMultiplier), 
                    def = Mathf.RoundToInt(8 * difficultyMultiplier), 
                    spd = Mathf.RoundToInt(10 * difficultyMultiplier) 
                };
                
                enemies.Add(new HeroData
                {
                    id = $"arena_bot_{System.Guid.NewGuid()}",
                    heroName = $"Người Chơi Ảo {i + 1}",
                    level = scaledLevel,
                    baseStats = stats,
                    currentHp = stats.hp
                });
            }
            return enemies;
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

namespace LegendOfBlood
{
    using LegendOfBlood.Combat; // BẮT BUỘC
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;

    public class ArenaPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button challengeButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private SquadSelectionPanel squadSelectionPanel;
        [SerializeField] private Text rankNameText;
        [SerializeField] private Text rankPointsText;
        [SerializeField] private Text ticketsText;
        [SerializeField] private Image rankIconImage;
        [SerializeField] private Button leaderboardButton;
        [SerializeField] private Button shopButton;

        private const int ARENA_SQUAD_SIZE = 5;

        private void Awake()
        {
            challengeButton.onClick.AddListener(OnChallengeClicked);
            closeButton.onClick.AddListener(() => GameManager.Instance.UIManager.HidePanel(UIPanelType.Arena));
            leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
            shopButton.onClick.AddListener(OnShopClicked);
        }

        private void OnEnable()
        {
            UpdateArenaInfo();
        }

        public void UpdateArenaInfo()
        {
            PlayerData playerData = DataManager.Instance.Player;
            if (playerData == null) return;

            // This part needs the ArenaRankData assets to be functional
            // For now, I'll just display the points.
            rankNameText.text = "Hạng"; // Placeholder
            rankPointsText.text = $"Điểm: {playerData.arenaPoints}";
            ticketsText.text = $"Vé: {playerData.arenaTickets}";
            // rankIconImage.sprite = ...; // Needs logic to get sprite from ArenaRankData
        }

        private void OnLeaderboardClicked()
        {
            // Logic to open leaderboard panel
            Debug.Log("Leaderboard button clicked.");
        }

        private void OnShopClicked()
        {
            // Logic to open arena shop panel
            GameManager.Instance.UIManager.ShowPanel(UIPanelType.ArenaShop);
            Debug.Log("Shop button clicked.");
        }

        private void OnChallengeClicked()
        {
            PlayerData playerData = DataManager.Instance.Player;
            if (playerData == null || playerData.arenaTickets <= 0)
            {
                // Show error message
                GameManager.Instance.UINotificationManager.ShowNotification("Không đủ vé Đấu trường!");
                return;
            }

            var allHeroes = DataManager.Instance.AllHeroes;
            var availableHeroes = allHeroes.Where(h => h.isMature && !h.IsBusy()).ToList();

            if (availableHeroes.Count < ARENA_SQUAD_SIZE)
            {
                GameManager.Instance.UINotificationManager.ShowNotification("Không đủ tướng sẵn sàng!");
                return;
            }

            squadSelectionPanel.Show(
                "Chọn đội hình Đấu trường",
                availableHeroes,
                ARENA_SQUAD_SIZE,
                OnArenaSquadSelected
            );
        }

        private void OnArenaSquadSelected(List<string> selectedHeroIDs)
        {
            var playerSquad = DataManager.Instance.AllHeroes.Where(h => selectedHeroIDs.Contains(h.id)).ToList();
            var enemySquad = CreateDummyEnemySquad(ARENA_SQUAD_SIZE);

            // SỬA LỖI: Khai báo tường minh kiểu CombatResult
            CombatResult result = GameManager.Instance.CombatSystem.Simulate(playerSquad, enemySquad);

            HandleCombatResult(result);
        }

        // SỬA LỖI: Sửa kiểu của tham số
        private void HandleCombatResult(CombatResult result)
        {
            string message = result.DidPlayerWin ? "Chiến thắng!" : "Thất bại!";
            Debug.Log(message);

            Debug.Log("--- BÁO CÁO CHIẾN ĐẤU ---");
            Debug.Log(string.Join("\n", result.CombatLog));
            Debug.Log("-------------------------");

            // Xử lý thương vong
            foreach (var survivor in result.PlayerSurvivors)
            {
                // HeroData trong survivor đã được cập nhật HP
                // DataManager.Instance.GetHeroByID(survivor.id) sẽ trả về tham chiếu đến hero gốc,
                // và CombatSystem đã cập nhật nó.
            }
            foreach (var casualty in result.PlayerCasualties)
            {
                // Tương tự, HP của hero gốc đã được cập nhật thành 0
            }

            // Process result in ArenaSystem
            // For now, opponent points are mocked as player points
            int opponentPoints = DataManager.Instance.Player.arenaPoints;
            ArenaSystem.Instance.ProcessMatchResult(result.DidPlayerWin, DataManager.Instance.Player.arenaPoints, opponentPoints);

            // Update UI
            UpdateArenaInfo();

            // Show result popup (to be implemented)
            // For now, just a debug log
            string popupMessage = result.DidPlayerWin ? "Thắng! +25 điểm, +30 Huy hiệu" : "Thua! -20 điểm, +10 Huy hiệu";
            GameManager.Instance.UINotificationManager.ShowNotification(popupMessage);
        }

        private List<HeroData> CreateDummyEnemySquad(int count)
        {
            var enemies = new List<HeroData>();
            for (int i = 0; i < count; i++)
            {
                var stats = new HeroStats { hp = 150, atk = 15, def = 10, spd = 10 };
                enemies.Add(new HeroData
                {
                    id = $"enemy_{i}",
                    heroName = $"Kẻ Địch {i + 1}",
                    level = 5,
                    baseStats = stats,
                    currentHp = stats.hp
                });
            }
            return enemies;
        }
    }
}
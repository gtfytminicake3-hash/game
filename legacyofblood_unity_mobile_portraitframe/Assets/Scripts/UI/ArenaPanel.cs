namespace LegendOfBlood
{
    using LegendOfBlood.Combat; // BẮT BUỘC
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;

    public class ArenaPanel : UIPanel
    {
        private void Awake()
        {
            PanelType = UIPanelType.Arena;
        }
        [Header("UI References")]
        [SerializeField] private Button challengeButton;
        [SerializeField] private Button addTicketAdButton; // NEW: Nút xem Ad lấy vé
        [SerializeField] private Button closeButton;
        [SerializeField] private SquadSelectionPanel squadSelectionPanel;
        [SerializeField] private Text rankNameText;
        [SerializeField] private Text rankPointsText;
        [SerializeField] private Text ticketsText;
        [SerializeField] private Image rankIconImage;
        [SerializeField] private Button leaderboardButton;
        [SerializeField] private Button shopButton;

        [Header("Upgrade Feature")]
        [SerializeField] private Button upgradeBuildingButton;
        [SerializeField] private string associatedBuildingId = "Arena";

        private const int ARENA_SQUAD_SIZE = 5;

        protected override void Start()
        {
            base.Start();
            if (challengeButton != null)
            {
                challengeButton.onClick.AddListener(OnChallengeClicked);
                var chTxt = challengeButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (chTxt != null) chTxt.text = global::LocalizationSystem.GetText("btn_challenge");
            }
            if (addTicketAdButton != null) addTicketAdButton.onClick.AddListener(OnAddTicketAdClicked);
            if (closeButton != null) closeButton.onClick.AddListener(() => GameManager.Instance.UIManager.HidePanel(UIPanelType.Arena));
            if (leaderboardButton != null)
            {
                leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
                var lbTxt = leaderboardButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (lbTxt != null) lbTxt.text = global::LocalizationSystem.GetText("btn_leaderboard");
            }
            if (shopButton != null)
            {
                shopButton.onClick.AddListener(OnShopClicked);
                var shTxt = shopButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (shTxt != null) shTxt.text = global::LocalizationSystem.GetText("btn_shop");
            }
            if (upgradeBuildingButton != null) 
            {
                upgradeBuildingButton.onClick.AddListener(OnUpgradeBuildingClicked);
                var upgTxt = upgradeBuildingButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (upgTxt != null) upgTxt.text = global::LocalizationSystem.GetText("btn_upgrade");
            }
        }

        private void OnUpgradeBuildingClicked()
        {
            GameManager.Instance.UIManager.ShowPanel(UIPanelType.BuildingUpgrade, true);
            var upgradePanel = GameManager.Instance.UIManager.GetPanel<BuildingUpgradePanel>(UIPanelType.BuildingUpgrade);
            if (upgradePanel != null && !string.IsNullOrEmpty(associatedBuildingId))
            {
                upgradePanel.Setup(associatedBuildingId);
            }
            else
            {
                Debug.LogWarning($"[ArenaPanel] Cannot open BuildingUpgradePanel! upgradePanel null? {upgradePanel == null}, associatedBuildingId empty? {string.IsNullOrEmpty(associatedBuildingId)}");
            }
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
            // rankNameText, rankPointsText, ticketsText might be null if not assigned in Inspector
            if (rankNameText != null) rankNameText.text = LocalizationSystem.GetText("arena_rank"); 
            if (rankPointsText != null) rankPointsText.text = string.Format(LocalizationSystem.GetText("arena_points"), playerData.arenaPoints);
            if (ticketsText != null) ticketsText.text = string.Format(LocalizationSystem.GetText("arena_tickets"), playerData.arenaTickets);
            // rankIconImage.sprite = ...; // Needs logic to get sprite from ArenaRankData
            
            if (addTicketAdButton != null)
            {
                bool showAdButton = (playerData.arenaTickets <= 0) && (playerData.dailyArenaTicketAdsWatched < 3);
                addTicketAdButton.gameObject.SetActive(showAdButton);
            }
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
                GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("msg_not_enough_arena_tickets"));
                return;
            }

            var allHeroes = DataManager.Instance.AllHeroes;
            var availableHeroes = allHeroes.Where(h => h.isMature && !h.IsBusy()).ToList();

            if (availableHeroes.Count < ARENA_SQUAD_SIZE)
            {
                GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("msg_not_enough_heroes"));
                return;
            }

            squadSelectionPanel.Show(
                LocalizationSystem.GetText("title_select_arena_squad"),
                availableHeroes,
                ARENA_SQUAD_SIZE,
                OnArenaSquadSelected
            );
        }

        private void OnArenaSquadSelected(List<string> selectedHeroIDs)
        {
            var playerSquad = DataManager.Instance.AllHeroes.Where(h => selectedHeroIDs.Contains(h.id)).ToList();
            
            // Lấy đội hình Bot được random sức mạnh dựa trên Elo hiện tại
            var enemySquad = ArenaSystem.Instance.FindOpponentSquad(DataManager.Instance.Player.arenaPoints);

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
            string popupMessage = result.DidPlayerWin ? LocalizationSystem.GetText("msg_arena_win_result") : LocalizationSystem.GetText("msg_arena_lose_result");
            GameManager.Instance.UINotificationManager.ShowNotification(popupMessage);
        }

        private void OnAddTicketAdClicked()
        {
            if (LegendOfBlood.Managers.AdRewardGateway.Instance != null)
            {
                LegendOfBlood.Managers.AdRewardGateway.Instance.RequestAd(LegendOfBlood.Managers.RewardType.ArenaTicket, () => {
                    UpdateArenaInfo();
                });
            }
        }
    }
}
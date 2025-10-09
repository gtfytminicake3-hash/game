namespace LegendOfBlood
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;
    
    /// <summary>
    /// Điều khiển màn hình Đấu trường.
    /// </summary>
    public class ArenaPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button challengeButton;
        [SerializeField] private Button closeButton;
        
        [Header("Dependencies")]
        [Tooltip("Kéo GameObject chứa script SquadSelectionPanel vào đây.")]
        [SerializeField] private SquadSelectionPanel squadSelectionPanel;
        
        private const int ARENA_SQUAD_SIZE = 5;

        private void Awake()
        {
            challengeButton.onClick.AddListener(OnChallengeClicked);
            closeButton.onClick.AddListener(() => GameManager.Instance.UIManager.HidePanel(UIPanelType.Arena));
        }

        /// <summary>
        /// Được gọi khi người chơi nhấn nút "Thách đấu".
        /// </summary>
        private void OnChallengeClicked()
        {
            // Lấy danh sách tất cả các hero hợp lệ (trưởng thành và không bận)
            var allHeroes = DataManager.Instance.AllHeroes;
            var availableHeroes = allHeroes.Where(h => h.isMature && !h.IsBusy()).ToList();
            
            if (availableHeroes.Count < ARENA_SQUAD_SIZE)
            {
                GameManager.Instance.UINotificationManager.ShowNotification(string.Format(LocalizationSystem.GetText("arena_error_not_enough_heroes"), ARENA_SQUAD_SIZE));
                return;
            }

            // Mở panel chọn đội hình và truyền vào một hàm callback
            squadSelectionPanel.Show(
                LocalizationSystem.GetText("arena_squad_selection_title"), 
                availableHeroes, 
                ARENA_SQUAD_SIZE,
                OnArenaSquadSelected // Đây là hàm sẽ được gọi khi người chơi xác nhận
            );
        }

        /// <summary>
        /// Hàm callback được SquadSelectionPanel gọi sau khi người chơi chọn xong.
        /// </summary>
        /// <param name="selectedHeroIDs">Danh sách ID của các hero đã được chọn.</param>
        private void OnArenaSquadSelected(List<string> selectedHeroIDs)
        {
            Debug.Log($"Đội hình đã được chọn với {selectedHeroIDs.Count} hero. Bắt đầu trận đấu!");
            
            // Lấy lại đối tượng HeroData đầy đủ từ các ID
            var allHeroes = DataManager.Instance.AllHeroes;
            var playerSquad = allHeroes.Where(h => selectedHeroIDs.Contains(h.id)).ToList();
            
            // TODO: Lấy đội hình của đối thủ
            var enemySquad = CreateDummyEnemySquad(ARENA_SQUAD_SIZE);

            // Gọi CombatSystem để mô phỏng trận đấu
            CombatResult result = GameManager.Instance.CombatSystem.SimulateNormalBattle(playerSquad, enemySquad);
            
            // Xử lý kết quả (hiển thị popup, log,...)
            HandleCombatResult(result);
        }

        private void HandleCombatResult(CombatResult result)
        {
            // Ví dụ: hiển thị một thông báo đơn giản
            string message = result.DidPlayerWin ? LocalizationSystem.GetText("arena_battle_win") : LocalizationSystem.GetText("arena_battle_lose");
            GameManager.Instance.UINotificationManager.ShowNotification(message);

            // Log chi tiết trận đấu ra console
            Debug.Log("--- BÁO CÁO CHIẾN ĐẤU ---");
            Debug.Log(string.Join("\n", result.CombatLog));
            Debug.Log("-------------------------");
            
            // TODO: Xử lý thương vong
            var hospital = GameManager.Instance.HospitalSystem;
            foreach (var survivor in result.PlayerSurvivors)
            {
                // Gây thương nhẹ cho những hero còn sống nhưng mất máu
                if(survivor.currentHp < survivor.GetFinalStats().hp)
                    hospital.InflictLightInjury(survivor);
            }
            foreach (var casualty in result.PlayerCasualties)
            {
                hospital.AdmitForSevereInjury(casualty);
            }
        }
        
        // Hàm tạo kẻ địch giả để test
        private List<HeroData> CreateDummyEnemySquad(int count)
        {
            var enemies = new List<HeroData>();
            for (int i = 0; i < count; i++)
            {
                enemies.Add(new HeroData { 
                    id = $"enemy_{i}",
                    heroName = $"Kẻ Địch {i+1}",
                    level = 5,
                    baseStats = new HeroStats { hp = 150, atk = 15, def = 10, spd = 10}
                });
            }
            return enemies;
        }
    }
}
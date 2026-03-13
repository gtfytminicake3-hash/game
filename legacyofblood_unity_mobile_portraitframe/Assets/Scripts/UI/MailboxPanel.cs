namespace LegendOfBlood
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;

    public class MailboxPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMPro.TextMeshProUGUI panelTitleText;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMPro.TextMeshProUGUI claimAllButtonText;
        [SerializeField] private Button claimAllButton;
        [SerializeField] private Transform reportItemsContainer;

        [Header("Prefabs")]
        [SerializeField] private GameObject reportItemPrefab; // A prefab for displaying a single report

        public Button reviveRetryButton;
        public Button claimX2Button;
        public Button replayButton;
        public TMPro.TextMeshProUGUI replayButtonText;

        private Action _onClaimX2Callback; // Thêm Action này
        private List<GameObject> _instantiatedReportItems = new List<GameObject>();

        private void Start()
        {
            if (panelTitleText != null) panelTitleText.text = global::LocalizationSystem.GetText("panel_title_mailbox");
            if (claimAllButtonText != null) claimAllButtonText.text = global::LocalizationSystem.GetText("btn_claim_all");

            if (closeButton != null) closeButton.onClick.AddListener(Hide);
            if (claimAllButton != null) claimAllButton.onClick.AddListener(ClaimAllReports);
        }

        private void OnEnable()
        {
            RefreshUI();
        }

        // Keep Show() just in case it's called elsewhere, though UIManager will just SetActive(true)
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            GameManager.Instance.UIManager.GoBack();
        }

        private void RefreshUI()
        {
            // Clear old items
            foreach (var item in _instantiatedReportItems)
            {
                Destroy(item);
            }
            _instantiatedReportItems.Clear();

            if (DataManager.Instance == null || DataManager.Instance.Player == null) return;

            var unclaimedReports = DataManager.Instance.Player.UnclaimedReports;

            if (unclaimedReports == null || !unclaimedReports.Any())
            {
                // Optionally, show a "Mailbox is empty" message
                claimAllButton.interactable = false;
                return;
            }

            claimAllButton.interactable = true;

            // Populate with new items
            foreach (var report in unclaimedReports)
            {
                GameObject itemGO = Instantiate(reportItemPrefab, reportItemsContainer);
                ExpeditionReportItem itemUI = itemGO.GetComponent<ExpeditionReportItem>();
                
                if (itemUI != null)
                {
                    // Pass the report data and a callback for when the claim button is pressed
                    itemUI.Initialize(report, () => {
                        ProcessSingleReport(report, false);
                        // Refresh the UI after claiming one
                        RefreshUI();
                    }, () => {
                        // Mở bảng CombatVisualizerPanel và truyền dữ liệu replay vào đây.
                        GameManager.Instance.UIManager.ShowPanel(UIPanelType.Battle, false);
                        var visualizer = UnityEngine.Object.FindAnyObjectByType<LegendOfBlood.Combat.CombatVisualizerPanel>();
                        
                        // Lấy danh sách tướng bên mình từ report
                        var allies = new System.Collections.Generic.List<HeroData>();
                        if (report.combatResult.PlayerSurvivors != null) allies.AddRange(report.combatResult.PlayerSurvivors);
                        if (report.combatResult.PlayerCasualties != null) allies.AddRange(report.combatResult.PlayerCasualties);
                        
                        // Lấy danh sách quái từ POI
                        var poiData = DataManager.Instance.GetPOIByID(report.poiId);
                        var monsterIds = poiData != null ? poiData.monsterIDs : new System.Collections.Generic.List<string>();

                        if (visualizer != null)
                        {
                            visualizer.PlayCombat(report.combatResult, allies, null, monsterIds);
                        }
                    }, () => {
                        // X2 Thưởng
                        if (LegendOfBlood.Managers.AdRewardGateway.Instance != null)
                        {
                            if (DataManager.Instance.Player.dailyDoubleGoldAdsWatched < 3)
                            {
                                LegendOfBlood.Managers.AdRewardGateway.Instance.RequestAd(LegendOfBlood.Managers.RewardType.DoubleGold, () => {
                                    ProcessSingleReport(report, true);
                                    RefreshUI();
                                });
                            }
                            else
                            {
                                GameManager.Instance.UINotificationManager.ShowNotification("Hôm nay bạn đã dùng hết lượt X2 thưởng!");
                            }
                        }
                    }, () => {
                        // NEW: Hồi Sinh và Đánh Tiếp
                        if (LegendOfBlood.Managers.AdRewardGateway.Instance != null)
                        {
                            if (DataManager.Instance.Player.dailyCombatReviveAdsWatched < 2)
                            {
                                LegendOfBlood.Managers.AdRewardGateway.Instance.RequestAd(LegendOfBlood.Managers.RewardType.ReviveTeam, () => {
                                    HandleReviveAndRetry(report);
                                });
                            }
                            else
                            {
                                GameManager.Instance.UINotificationManager.ShowNotification("Hôm nay bạn đã dùng hết lượt Hồi sinh miễn phí!");
                            }
                        }
                    });
                    _instantiatedReportItems.Add(itemGO);
                }
            }
        }

        private void HandleReviveAndRetry(ExpeditionReport report)
        {
            if (report == null || report.combatResult == null) return;

            // 1. Lấy danh sách tướng (Hồi máu cho casualty)
            var allies = new List<HeroData>();
            if (report.combatResult.PlayerSurvivors != null) allies.AddRange(report.combatResult.PlayerSurvivors);
            if (report.combatResult.PlayerCasualties != null) 
            {
                foreach(var casualty in report.combatResult.PlayerCasualties)
                {
                    var stats = casualty.GetFinalStats();
                    casualty.currentHp = stats.hp; 
                    allies.Add(casualty);
                }
            }

            // 2. Lấy danh sách quái địch còn sống
            var enemies = new List<HeroData>();
            if (report.combatResult.EnemySurvivors != null)
            {
                enemies.AddRange(report.combatResult.EnemySurvivors);
            }

            // 3. Chạy lại CombatSystem
            if (GameManager.Instance.CombatSystem != null)
            {
                var newResult = GameManager.Instance.CombatSystem.Simulate(allies, enemies);
                
                // Nối Log
                var combinedLog = report.combatResult.CombatLog.ToList();
                combinedLog.Add("<color=yellow>--- HỒI SINH & TIẾP TỤC ---</color>");
                combinedLog.AddRange(newResult.CombatLog);
                newResult.CombatLog = combinedLog;

                report.combatResult = newResult;

                // 4. Cấp lại phần thưởng nếu thắng
                if (newResult.DidPlayerWin)
                {
                    var poi = DataManager.Instance.GetPOIByID(report.poiId);
                    if (poi != null)
                    {
                        var expMgr = UnityEngine.Object.FindAnyObjectByType<ExpeditionManager>();
                        if (expMgr != null)
                        {
                            if (poi.type == POIType.Boss)
                            {
                                string bossId = poi.monsterIDs != null && poi.monsterIDs.Count > 0 ? poi.monsterIDs[0] : "BOSS_01";
                                report.loot = expMgr.CalculateBossLoot(bossId, true);
                                report.experienceGained = 500; // Tương tự StartBossExpedition
                            }
                            else 
                            {
                                report.loot = expMgr.CalculateLoot(poi, true);
                                report.experienceGained = expMgr.CalculateExperience(poi, true);
                            }
                        }
                    }
                }

                RefreshUI();
                GameManager.Instance.UINotificationManager.ShowNotification(newResult.DidPlayerWin ? "Phục thù thành công! Bạn có thể nhận thưởng." : "Rất tiếc, vẫn chưa đủ sức mạnh để chiến thắng...");
            }
        }

        private void ClaimAllReports()
        {
            var reportsToClaim = DataManager.Instance.Player.UnclaimedReports.ToList();
            foreach (var report in reportsToClaim)
            {
                ProcessSingleReport(report, false);
            }
            
            // Refresh the UI once after claiming all
            RefreshUI();
        }

        private void ProcessSingleReport(ExpeditionReport report, bool isX2)
        {
            if (report == null) return;

            Debug.Log($"Processing report for POI: {report.poiName} (X2: {isX2})");

            int rMulti = isX2 ? 2 : 1; // Chỉ nhân đôi Tài Nguyên, không nhân đôi Trang Phục/Tướng để giữ cân bằng

            // 1. Handle Loot
            if (report.loot != null)
            {
                if (GameManager.Instance != null && GameManager.Instance.InventoryManager != null)
                {
                    // Multiply resources
                    GameManager.Instance.InventoryManager.AddGold(report.loot.gold * rMulti);
                    GameManager.Instance.InventoryManager.AddResource(ResourceType.Wood, report.loot.wood * rMulti);
                    GameManager.Instance.InventoryManager.AddResource(ResourceType.Stone, report.loot.stone * rMulti);
                    
                    // Multiply basic items
                    foreach (var item in report.loot.items)
                    {
                        GameManager.Instance.InventoryManager.AddItem(item.Key, item.Value * rMulti);
                    }
                    if (report.loot.equipments != null)
                    {
                        foreach (var eq in report.loot.equipments)
                        {
                            GameManager.Instance.InventoryManager.AddEquipment(eq);
                        }
                    }
                    if (report.loot.rescuedHeroes != null && report.loot.rescuedHeroes.Count > 0)
                    {
                        foreach (var hero in report.loot.rescuedHeroes)
                        {
                            DataManager.Instance.AddHero(hero);
                        }
                    }
                }
            }

            // 2. Handle Experience
            if (report.combatResult != null && report.combatResult.PlayerSurvivors != null)
            {
                foreach (var survivor in report.combatResult.PlayerSurvivors)
                {
                    var hero = DataManager.Instance.GetHeroByID(survivor.id);
                    if (hero != null)
                    { 
                        hero.AddExperience(report.experienceGained * rMulti);
                    }
                }
            }

            // 3. (REMOVED) Handle Casualties (send to Hospital)
            // Lógica của bước này đã được chuyển sang hàm Tick() của ExpeditionManager 
            // để đảm bảo Hero chấn thương ngay lập tức khi đoàn về đến nhà.

            // 4. Show Notification to Player
            if (GameManager.Instance != null && GameManager.Instance.UINotificationManager != null)
            {
                string x2Prefix = isX2 ? "[X2] " : "";
                string lootMsg = global::LocalizationSystem.GetText("mailbox_loot_claimed") + "\n";
                if (report.loot != null)
                {
                    if (report.loot.gold > 0) lootMsg += $"+{report.loot.gold * rMulti} Gold {x2Prefix}\n";
                    if (report.loot.wood > 0) lootMsg += $"+{report.loot.wood * rMulti} Wood {x2Prefix}\n";
                    if (report.loot.stone > 0) lootMsg += $"+{report.loot.stone * rMulti} Stone {x2Prefix}\n";
                    
                    if (report.experienceGained > 0) lootMsg += $"+{report.experienceGained * rMulti} EXP {x2Prefix}\n";
                    foreach (var item in report.loot.items)
                    {
                        lootMsg += $"+{item.Value * rMulti} {item.Key} {x2Prefix}\n";
                    }
                    if (report.loot.equipments != null && report.loot.equipments.Count > 0)
                    {
                        lootMsg += $"+{report.loot.equipments.Count} Equipments\n";
                    }
                    if (report.loot.rescuedHeroes != null && report.loot.rescuedHeroes.Count > 0)
                    {
                        foreach (var hero in report.loot.rescuedHeroes)
                        {
                            lootMsg += $"+1 Hero: {hero.heroName} (Lv.{hero.level})\n";
                        }
                    }
                }
                GameManager.Instance.UINotificationManager.ShowNotification(lootMsg);
            }

            // 5. Remove the report from the list
            DataManager.Instance.Player.UnclaimedReports.Remove(report);
        }
    }
}

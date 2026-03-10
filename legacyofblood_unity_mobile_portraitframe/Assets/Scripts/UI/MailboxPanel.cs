namespace LegendOfBlood
{
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
                        ProcessSingleReport(report);
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
                    });
                    _instantiatedReportItems.Add(itemGO);
                }
            }
        }

        private void ClaimAllReports()
        {
            var reportsToClaim = DataManager.Instance.Player.UnclaimedReports.ToList();
            foreach (var report in reportsToClaim)
            {
                ProcessSingleReport(report);
            }
            
            // Refresh the UI once after claiming all
            RefreshUI();
        }

        private void ProcessSingleReport(ExpeditionReport report)
        {
            if (report == null) return;

            Debug.Log($"Processing report for POI: {report.poiName}");

            // 1. Handle Loot
            if (report.loot != null)
            {
                if (GameManager.Instance != null && GameManager.Instance.InventoryManager != null)
                {
                    GameManager.Instance.InventoryManager.AddGold(report.loot.gold);
                    foreach (var item in report.loot.items)
                    {
                        GameManager.Instance.InventoryManager.AddItem(item.Key, item.Value);
                    }
                    if (report.loot.equipments != null)
                    {
                        foreach (var eq in report.loot.equipments)
                        {
                            GameManager.Instance.InventoryManager.AddEquipment(eq);
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
                        hero.AddExperience(report.experienceGained);
                    }
                }
            }

            // 3. Handle Casualties (send to Hospital)
            if (report.combatResult != null && report.combatResult.PlayerCasualties != null)
            {
                if (GameManager.Instance != null && GameManager.Instance.HospitalSystem != null)
                {
                    foreach (var casualty in report.combatResult.PlayerCasualties)
                    {
                        GameManager.Instance.HospitalSystem.AdmitHero(casualty.id);
                    }
                }
            }

            // 4. Show Notification to Player
            if (GameManager.Instance != null && GameManager.Instance.UINotificationManager != null)
            {
                string lootMsg = global::LocalizationSystem.GetText("mailbox_loot_claimed") + "\n";
                if (report.loot != null)
                {
                    if (report.loot.gold > 0) lootMsg += $"+{report.loot.gold} Gold\n";
                    if (report.experienceGained > 0) lootMsg += $"+{report.experienceGained} EXP\n";
                    foreach (var item in report.loot.items)
                    {
                        lootMsg += $"+{item.Value} {item.Key}\n";
                    }
                    if (report.loot.equipments != null)
                    {
                        lootMsg += $"+{report.loot.equipments.Count} Equipments\n";
                    }
                }
                GameManager.Instance.UINotificationManager.ShowNotification(lootMsg);
            }

            // 5. Remove the report from the list
            DataManager.Instance.Player.UnclaimedReports.Remove(report);
        }
    }
}

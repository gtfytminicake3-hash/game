namespace LegendOfBlood
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;

    public class MailboxPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button claimAllButton;
        [SerializeField] private Transform reportItemsContainer;

        [Header("Prefabs")]
        [SerializeField] private GameObject reportItemPrefab; // A prefab for displaying a single report

        private List<GameObject> _instantiatedReportItems = new List<GameObject>();

        private void Start()
        {
            closeButton.onClick.AddListener(Hide);
            claimAllButton.onClick.AddListener(ClaimAllReports);
            gameObject.SetActive(false); // Start hidden
        }

        public void Show()
        {
            gameObject.SetActive(true);
            RefreshUI();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void RefreshUI()
        {
            // Clear old items
            foreach (var item in _instantiatedReportItems)
            {
                Destroy(item);
            }
            _instantiatedReportItems.Clear();

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

            // 4. Remove the report from the list
            DataManager.Instance.Player.UnclaimedReports.Remove(report);
        }
    }
}

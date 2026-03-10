namespace LegendOfBlood
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    // Assuming you have TextMeshPro, otherwise use UnityEngine.UI.Text
    using TMPro;

    public class ExpeditionReportItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI poiNameText;
        [SerializeField] private TextMeshProUGUI outcomeText;
        [SerializeField] private TextMeshProUGUI rewardsText; // Thêm trường text phần thưởng
        [SerializeField] private Button claimButton;
        [SerializeField] private TextMeshProUGUI claimButtonText;
        [SerializeField] private Button replayButton; // Thêm biến cho nút Xem Lại
        [SerializeField] private TextMeshProUGUI replayButtonText;

        private ExpeditionReport _report;
        private Action _onClaimCallback;
        private Action _onReplayCallback;

        private void OnDestroy()
        {
            // Clean up the listener when the object is destroyed
            if (claimButton != null) claimButton.onClick.RemoveAllListeners();
            if (replayButton != null) replayButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// Initializes the report item with data and a callback action.
        /// </summary>
        /// <param name="report">The expedition result data.</param>
        /// <param name="onClaimCallback">The action to execute when the Claim button is clicked.</param>
        /// <param name="onReplayCallback">The action to execute when the Replay button is clicked.</param>
        public void Initialize(ExpeditionReport report, Action onClaimCallback, Action onReplayCallback = null)
        {
            _report = report;
            _onClaimCallback = onClaimCallback;
            _onReplayCallback = onReplayCallback;

            // Populate UI elements
            if (poiNameText != null)
            {
                poiNameText.text = global::LocalizationSystem.GetText(report.poiName);
            }

            if (outcomeText != null)
            {
                if (report.combatResult != null)
                {
                    if (report.combatResult.DidPlayerWin)
                    {
                        outcomeText.text = global::LocalizationSystem.GetText("report_victory");
                    }
                    else
                    {
                        outcomeText.text = global::LocalizationSystem.GetText("report_defeat");
                    }
                }
                else
                {
                    outcomeText.text = global::LocalizationSystem.GetText("report_unknown");
                }
            }

            if (rewardsText != null)
            {
                if (report.combatResult != null && report.combatResult.DidPlayerWin && report.loot != null)
                {
                    string rs = string.Format(global::LocalizationSystem.GetText("report_rewards"), report.experienceGained, report.loot.gold);
                    if (report.loot.items != null && report.loot.items.Count > 0)
                    {
                        foreach(var kvp in report.loot.items)
                        {
                            var itemData = DataManager.Instance.GameConfig.AllItems.Find(x => x.id == kvp.Key);
                            string itemName = itemData != null ? global::LocalizationSystem.GetText(itemData.itemName) : kvp.Key;
                            rs += $", {kvp.Value} {itemName}";
                        }
                    }
                    if (report.loot.equipments != null && report.loot.equipments.Count > 0)
                    {
                        rs += $", {report.loot.equipments.Count} Trang bị";
                    }
                    rewardsText.text = rs;
                }
                else
                {
                    rewardsText.text = "";
                }
            }

            if (claimButtonText != null) claimButtonText.text = global::LocalizationSystem.GetText("btn_claim");
            if (replayButtonText != null) replayButtonText.text = global::LocalizationSystem.GetText("btn_replay");

            // Set up the claim button
            if (claimButton != null)
            {
                claimButton.onClick.RemoveAllListeners(); // Clear previous listeners
                claimButton.onClick.AddListener(HandleClaimButtonClick);
            }

            // Set up the replay button
            if (replayButton != null)
            {
                replayButton.onClick.RemoveAllListeners();
                replayButton.onClick.AddListener(HandleReplayButtonClick);
            }
        }

        private void HandleReplayButtonClick()
        {
            _onReplayCallback?.Invoke();
        }

        private void HandleClaimButtonClick()
        { 
            // Invoke the callback that was passed from MailboxPanel
            _onClaimCallback?.Invoke();

            // The panel will handle destroying this object by refreshing the UI
        }
    }
}

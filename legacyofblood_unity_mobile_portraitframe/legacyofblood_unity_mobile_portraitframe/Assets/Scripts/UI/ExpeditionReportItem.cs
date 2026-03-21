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
        [SerializeField] private Button claimX2Button; // NEW
        [SerializeField] private Button replayButton; // Thêm biến cho nút Xem Lại
        [SerializeField] private TextMeshProUGUI replayButtonText;
        [SerializeField] private Button reviveRetryButton; // NEW: Nút Hồi sinh và Đánh tiếp

        private ExpeditionReport _report;
        private Action _onClaimCallback;
        private Action _onReplayCallback;
        private Action _onClaimX2Callback;
        private Action _onReviveRetryCallback;

        private void OnDestroy()
        {
            // Clean up the listener when the object is destroyed
            if (claimButton != null) claimButton.onClick.RemoveAllListeners();
            if (claimX2Button != null) claimX2Button.onClick.RemoveAllListeners();
            if (replayButton != null) replayButton.onClick.RemoveAllListeners();
            if (reviveRetryButton != null) reviveRetryButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// Initializes the report item with data and a callback action.
        /// </summary>
        /// <param name="report">The expedition result data.</param>
        /// <param name="onClaimCallback">The action to execute when the Claim button is clicked.</param>
        /// <param name="onReplayCallback">The action to execute when the Replay button is clicked.</param>
        /// <param name="onClaimX2Callback">The action to execute when the Claim X2 button is clicked.</param>
        /// <param name="onReviveRetryCallback">The action to execute when the Revive & Retry button is clicked.</param>
        public void Initialize(ExpeditionReport report, Action onClaimCallback, Action onReplayCallback = null, Action onClaimX2Callback = null, Action onReviveRetryCallback = null)
        {
            _report = report;
            _onClaimCallback = onClaimCallback;
            _onReplayCallback = onReplayCallback;
            _onClaimX2Callback = onClaimX2Callback;
            _onReviveRetryCallback = onReviveRetryCallback;

            // Populate UI elements
            if (poiNameText != null)
            {
                poiNameText.text = report.poiName;
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

            // Set up the claim x2 button
            if (claimX2Button != null)
            {
                claimX2Button.gameObject.SetActive(report.combatResult != null && report.combatResult.DidPlayerWin);
                claimX2Button.onClick.RemoveAllListeners();
                claimX2Button.onClick.AddListener(HandleClaimX2ButtonClick);
            }

            // Set up the replay button
            if (replayButton != null)
            {
                replayButton.onClick.RemoveAllListeners();
                replayButton.onClick.AddListener(HandleReplayButtonClick);
            }
            
            // Set up the revive & retry button
            if (reviveRetryButton != null)
            {
                // Chỉ hiển thị nút Hồi Sinh khi Thất bại (DidPlayerWin == false)
                bool isDefeat = report.combatResult != null && !report.combatResult.DidPlayerWin;
                // Có thể kiểm tra thêm điều kiện boss/tower nếu cần thiết, tạm thời áp dụng cho mọi trận thua.
                reviveRetryButton.gameObject.SetActive(isDefeat);

                reviveRetryButton.onClick.RemoveAllListeners();
                reviveRetryButton.onClick.AddListener(HandleReviveRetryButtonClick);
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
        }

        private void HandleClaimX2ButtonClick()
        {
            _onClaimX2Callback?.Invoke();
        }

        private void HandleReviveRetryButtonClick()
        {
            _onReviveRetryCallback?.Invoke();
        }
    }
}

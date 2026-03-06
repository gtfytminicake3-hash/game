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
        [SerializeField] private Button claimButton;
        [SerializeField] private Button replayButton; // Thêm biến cho nút Xem Lại

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
                poiNameText.text = report.poiName;
            }

            if (outcomeText != null)
            {
                if (report.combatResult != null)
                {
                    if (report.combatResult.DidPlayerWin)
                    {
                        outcomeText.text = "<color=green>Victory</color>";
                    }
                    else
                    {
                        outcomeText.text = "<color=red>Defeat</color>";
                    }
                }
                else
                {
                    outcomeText.text = "<color=grey>Unknown Result</color>";
                }
            }

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

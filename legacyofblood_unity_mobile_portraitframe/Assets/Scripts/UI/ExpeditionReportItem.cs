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

        private ExpeditionReport _report;
        private Action _onClaimCallback;

        private void OnDestroy()
        {
            // Clean up the listener when the object is destroyed
            if (claimButton != null)
            {
                claimButton.onClick.RemoveAllListeners();
            }
        }

        /// <summary>
        /// Initializes the report item with data and a callback action.
        /// </summary>
        /// <param name="report">The expedition result data.</param>
        /// <param name="onClaimCallback">The action to execute when the Claim button is clicked.</param>
        public void Initialize(ExpeditionReport report, Action onClaimCallback)
        {
            _report = report;
            _onClaimCallback = onClaimCallback;

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

            // Set up the button
            if (claimButton != null)
            {
                claimButton.onClick.RemoveAllListeners(); // Clear previous listeners
                claimButton.onClick.AddListener(HandleClaimButtonClick);
            }
        }

        private void HandleClaimButtonClick()
        { 
            // Invoke the callback that was passed from MailboxPanel
            _onClaimCallback?.Invoke();

            // The panel will handle destroying this object by refreshing the UI
        }
    }
}

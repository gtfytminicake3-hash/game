using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

namespace LegendOfBlood
{
    public class POI_InfoPanel : MonoBehaviour, ILocalizable
    {
        [Header("Standard UI References")]
        [SerializeField] private TextMeshProUGUI poiNameText;
        [SerializeField] private TextMeshProUGUI difficultyText;
        [SerializeField] private TextMeshProUGUI recommendedCpText;
        [SerializeField] private Button exploreButton;
        [SerializeField] private Button closeButton;

        [Header("Tower of Trials UI")]
        [SerializeField] private GameObject towerInfoContainer; // A parent object for all tower-specific UI
        [SerializeField] private TextMeshProUGUI currentFloorText;
        [SerializeField] private TextMeshProUGUI recoveryTimeText;

        private POIData _currentPoiData;
        private Action _onExploreCallback;
        private Coroutine _countdownCoroutine;

        private void Awake()
        {
            exploreButton.onClick.AddListener(OnExploreClicked);
            closeButton.onClick.AddListener(ClosePanel);
        }

        public void Show(POIData poiData, Action onExplore)
        {
            _currentPoiData = poiData;
            _onExploreCallback = onExplore;

            UpdateLocalizedText();
            gameObject.SetActive(true);

            // Handle Tower of Trials specific UI and logic
            if (poiData.type == POIType.TowerOfTrials)
            {
                towerInfoContainer.SetActive(true);
                // Start a coroutine to update the countdown timer
                if (_countdownCoroutine != null) StopCoroutine(_countdownCoroutine);
                _countdownCoroutine = StartCoroutine(CountdownTimer());
            }
            else
            {
                towerInfoContainer.SetActive(false);
            }
        }

        public void UpdateLocalizedText()
        {
            if (_currentPoiData == null) return;

            poiNameText.text = _currentPoiData.poiName;

            // Hide regular info for the tower and show tower-specific info
            if (_currentPoiData.type == POIType.TowerOfTrials)
            {
                difficultyText.gameObject.SetActive(false);
                recommendedCpText.gameObject.SetActive(false);

                currentFloorText.text = string.Format(LocalizationSystem.GetText("tower_current_floor_format"), _currentPoiData.currentFloor);
            }
            else
            {
                difficultyText.gameObject.SetActive(true);
                recommendedCpText.gameObject.SetActive(true);

                int recommendedCp = 0;
                if (_currentPoiData.monsterIDs != null)
                {
                    foreach (var monsterId in _currentPoiData.monsterIDs) recommendedCp += 500; // Placeholder
                }
                difficultyText.text = string.Format(LocalizationSystem.GetText("poi_difficulty_format"), _currentPoiData.difficultyLevel);
                recommendedCpText.text = string.Format(LocalizationSystem.GetText("poi_recommended_cp_format"), recommendedCp);
            }
        }

        private IEnumerator CountdownTimer()
        {
            while (towerInfoContainer.activeSelf && _currentPoiData != null && _currentPoiData.type == POIType.TowerOfTrials)
            {
                long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                long remainingTime = _currentPoiData.recoveryEndTime - currentTime;

                if (remainingTime > 0)
                {
                    TimeSpan timeSpan = TimeSpan.FromMilliseconds(remainingTime);
                    recoveryTimeText.text = string.Format("Cooldown: {0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
                    exploreButton.interactable = false; // Can't explore while on cooldown
                }
                else
                {
                    recoveryTimeText.text = "<color=green>Ready</color>";
                    exploreButton.interactable = true;
                    // Stop the coroutine once it's ready
                    yield break; 
                }
                yield return new WaitForSeconds(1f);
            }
        }

        private void OnExploreClicked()
        {
            _onExploreCallback?.Invoke();
        }

        private void ClosePanel()
        {
            if (_countdownCoroutine != null)
            {
                StopCoroutine(_countdownCoroutine);
                _countdownCoroutine = null;
            }
            gameObject.SetActive(false);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

namespace LegendOfBlood
{
    public class POI_InfoPanel : UIPanel, ILocalizable
    {
        [Header("Standard UI References")]
        [SerializeField] private TextMeshProUGUI poiNameText;
        [SerializeField] private TextMeshProUGUI difficultyText;
        [SerializeField] private TextMeshProUGUI recommendedCpText;
        [SerializeField] private Button exploreButton;
        [SerializeField] private TextMeshProUGUI exploreButtonText;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI closeButtonText;

        [Header("Tower of Trials UI")]
        [SerializeField] private GameObject towerInfoContainer; // A parent object for all tower-specific UI
        [SerializeField] private TextMeshProUGUI currentFloorText;
        [SerializeField] private TextMeshProUGUI recoveryTimeText;

        private POIData _currentPoiData;
        private Action _onExploreCallback;
        private Coroutine _countdownCoroutine;

        private void Awake()
        {
            PanelType = UIPanelType.POI_Info;
            // TỰ ĐỘNG CHỮA BỆNH "RƠI KHỎI CANVAS":
            // Nếu POI_InfoPanel vô tình bị bỏ quên ngoài Root hierarchy (không có Canvas bọc), GUI sẽ ko vẽ.
            Canvas mainCanvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (mainCanvas != null && (transform.parent == null || transform.parent.GetComponentInParent<Canvas>() == null))
            {
                transform.SetParent(mainCanvas.transform, false);
                transform.SetAsLastSibling();
                Debug.Log($"[POI_InfoPanel] Đã tự động gắp Panel vào trong {mainCanvas.name} để có thể hiển thị!");
            }

            // AUTO-WIRE: Tự động lùng sục Hierarchy tìm cái Tower Container bạn quên kéo vào
            if (towerInfoContainer == null)
            {
                Transform[] allChildren = GetComponentsInChildren<Transform>(true);
                foreach (var child in allChildren)
                {
                    if (child.name.ToLower().Contains("tower"))
                    {
                        // Tìm thấy cái cục Tower rồi
                        towerInfoContainer = child.gameObject;
                        break;
                    }
                }
                
                // Nếu User xóa luôn cục Tower trong thiết kế, đẻ ra một cái rỗng để đỡ đạn NullReference!
                if (towerInfoContainer == null)
                {
                    towerInfoContainer = new GameObject("TOWER_HOLDER_AUTO");
                    towerInfoContainer.transform.SetParent(this.transform, false);
                    towerInfoContainer.SetActive(false);
                }
            }
        }

        protected override void Start()
        {
            base.Start();
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
                if (towerInfoContainer != null) 
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

            if (poiNameText != null) poiNameText.text = _currentPoiData.poiName;

            // Hide regular info for the tower and show tower-specific info
            if (_currentPoiData.type == POIType.TowerOfTrials)
            {
                if (difficultyText != null) difficultyText.gameObject.SetActive(false);
                if (recommendedCpText != null) recommendedCpText.gameObject.SetActive(false);

                if (currentFloorText != null)
                {
                    currentFloorText.text = string.Format(LocalizationSystem.GetText("tower_current_floor_format"), _currentPoiData.currentFloor);
                }
                else
                {
                    Debug.LogWarning("[POI_InfoPanel] Biến currentFloorText chưa được gán vào Inspector nên không thể hiển thị số tầng.");
                }
            }
            else
            {
                if (difficultyText != null) difficultyText.gameObject.SetActive(true);
                if (recommendedCpText != null) recommendedCpText.gameObject.SetActive(true);

                int recommendedCp = 0;
                if (_currentPoiData.monsterIDs != null)
                {
                    foreach (var monsterId in _currentPoiData.monsterIDs)
                    {
                        var monsterData = DataManager.Instance.GetMonsterByID(monsterId, _currentPoiData.difficultyLevel);
                        if (monsterData != null)
                        {
                            recommendedCp += monsterData.GetCombatPower();
                        }
                        else
                        {
                            recommendedCp += 500; // Placeholder fallback
                        }
                    }
                }
                
                if (difficultyText != null) difficultyText.text = string.Format(LocalizationSystem.GetText("poi_difficulty_format"), _currentPoiData.difficultyLevel);
                if (recommendedCpText != null) recommendedCpText.text = string.Format(LocalizationSystem.GetText("poi_recommended_cp_format"), recommendedCp);
            }

            if (exploreButtonText != null) exploreButtonText.text = global::LocalizationSystem.GetText("btn_explore");
            if (closeButtonText != null) closeButtonText.text = global::LocalizationSystem.GetText("btn_close");
        }

        private IEnumerator CountdownTimer()
        {
            while (towerInfoContainer != null && towerInfoContainer.activeSelf && _currentPoiData != null && _currentPoiData.type == POIType.TowerOfTrials)
            {
                long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                long remainingTime = _currentPoiData.recoveryEndTime - currentTime;

                if (remainingTime > 0)
                {
                    TimeSpan timeSpan = TimeSpan.FromMilliseconds(remainingTime);
                    if (recoveryTimeText != null) recoveryTimeText.text = string.Format("Cooldown: {0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
                    if (exploreButton != null) exploreButton.interactable = false; // Can't explore while on cooldown
                }
                else
                {
                    if (recoveryTimeText != null) recoveryTimeText.text = "<color=green>Ready</color>";
                    if (exploreButton != null) exploreButton.interactable = true;
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
            GameManager.Instance.UIManager.GoBack();
        }
    }
}

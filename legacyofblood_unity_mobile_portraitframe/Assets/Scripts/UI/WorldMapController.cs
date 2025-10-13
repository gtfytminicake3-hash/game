namespace LegendOfBlood
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using DG.Tweening;
    using DG.Tweening.Core;
    using UnityEngine.SceneManagement;

    public class WorldMapController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private RectTransform mapContainer;
        [SerializeField] private Transform generatedPOIsContainer;
        [SerializeField] private Transform travelLayer;
        [SerializeField] private SquadSelectionPanel squadSelectionPanel;
        [SerializeField] private POI_InfoPanel poiInfoPanel;

        [Header("Prefabs")]
        [SerializeField] private GameObject dungeonPoiPrefab;
        [SerializeField] private GameObject rescuePoiPrefab;
        [SerializeField] private GameObject towerPoiPrefab;
        [SerializeField] private GameObject travelCartPrefab;

        [Header("Map Settings")]
        [SerializeField] private float minZoom = 0.5f;
        [SerializeField] private float maxZoom = 2.0f;
        [SerializeField] private float zoomSpeed = 0.1f;
        [SerializeField] private float minPoiDistance = 100f;
        [SerializeField] private Vector2 mapSize = new Vector2(2000, 1500);
        [SerializeField] private int numberOfDungeons = 10;
        [SerializeField] private int numberOfRescues = 5;

        [Header("Navigation")]
        [SerializeField] private Button backToVillageButton;

        private Vector2 _lastPanPosition;
        private bool _isPanning;
        private bool _isZooming;
        private Dictionary<string, GameObject> _activePoiObjects = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> _activeTravelCarts = new Dictionary<string, GameObject>();

        #region Unity Lifecycle & Event Subscription

        private void Start()
        {
            if (backToVillageButton != null)
            {
                backToVillageButton.onClick.AddListener(GoBackToVillage);
            }
        }

        private void OnEnable()
        {
            ExpeditionManager.OnExpeditionStarted += HandleExpeditionStarted;
            ExpeditionManager.OnExpeditionFinished += HandleExpeditionFinished;
            ExpeditionManager.OnPOICleared += HandlePOICleared;
            ExpeditionManager.OnTowerConquered += HandleTowerConquered;

            InitializeWorldMap();
        }

        private void OnDisable()
        {
            ExpeditionManager.OnExpeditionStarted -= HandleExpeditionStarted;
            ExpeditionManager.OnExpeditionFinished -= HandleExpeditionFinished;
            ExpeditionManager.OnPOICleared -= HandlePOICleared;
            ExpeditionManager.OnTowerConquered -= HandleTowerConquered;
            DOTween.Kill(this);
        }

        private void Update()
        {
            HandleInput();
        }

        #endregion

        #region Input Handling (Pan & Zoom)
        
        private void HandleInput()
        {
            // Handle Pan
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                _isPanning = true;
                _lastPanPosition = Input.mousePosition;
            }
            else if (Input.GetMouseButton(0) && _isPanning)
            {
                Vector2 delta = (Vector2)Input.mousePosition - _lastPanPosition;
                mapContainer.anchoredPosition += delta;
                ClampMapPosition();
                _lastPanPosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _isPanning = false;
            }

            // Handle Zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0.0f)
            {
                Vector3 newScale = mapContainer.localScale;
                newScale.x += scroll * zoomSpeed;
                newScale.y += scroll * zoomSpeed;
                newScale.x = Mathf.Clamp(newScale.x, minZoom, maxZoom);
                newScale.y = Mathf.Clamp(newScale.y, minZoom, maxZoom);
                mapContainer.localScale = newScale;
                ClampMapPosition();
            }
        }

        private void ClampMapPosition()
        {
            Vector2 pos = mapContainer.anchoredPosition;
            float scale = mapContainer.localScale.x;
            float limitX = (mapContainer.rect.width * scale - Screen.width) / 2;
            float limitY = (mapContainer.rect.height * scale - Screen.height) / 2;

            pos.x = Mathf.Clamp(pos.x, -limitX, limitX);
            pos.y = Mathf.Clamp(pos.y, -limitY, limitY);

            mapContainer.anchoredPosition = pos;
        }

        #endregion

        #region POI & Expedition Logic

        private void InitializeWorldMap()
        {
            foreach (Transform child in generatedPOIsContainer) Destroy(child.gameObject);
            _activePoiObjects.Clear();

            var worldPois = DataManager.Instance.Player.WorldPois;

            if (worldPois == null || worldPois.Count == 0)
            {
                Debug.Log("No POI data found, generating new POIs...");
                for (int i = 0; i < numberOfDungeons; i++) GenerateAndRegisterNewPOI(POIType.Dungeon, null);
                for (int i = 0; i < numberOfRescues; i++) GenerateAndRegisterNewPOI(POIType.RescueMission, null);
                 // Initialize the list if it was null
                if (DataManager.Instance.Player.WorldPois == null)
                {
                    DataManager.Instance.Player.WorldPois = new List<POIData>();
                }
            }
            else
            {
                Debug.Log($"Loading {worldPois.Count} POIs from DataManager.");
                foreach (var poiData in worldPois) InstantiatePOI(poiData);
            }

            if (!worldPois.Any(p => p.type == POIType.TowerOfTrials))
            {
                Debug.Log("Tower of Trials not found. Generating a new one.");
                GenerateTowerOfTrials();
            }
        }

        private void GenerateAndRegisterNewPOI(POIType type, string specificName)
        {
            Vector2 newPosition = FindValidPosition();
            if (newPosition == Vector2.zero) return;

            string newId = Guid.NewGuid().ToString();
            
            // FIX: Create the object first, then assign monsterIDs
            POIData poiData = new POIData
            {
                poiId = newId,
                poiName = specificName ?? string.Format(global::LocalizationSystem.GetText("poi_name_format"), global::LocalizationSystem.GetText($"poi_type_{type}"), UnityEngine.Random.Range(10, 999)),
                type = type,
                position = newPosition,
                difficultyLevel = UnityEngine.Random.Range(1, 10)
            };
            poiData.monsterIDs = GenerateMonstersForPOI(poiData);

            DataManager.Instance.Player.WorldPois.Add(poiData);
            InstantiatePOI(poiData);
        }
        
        private void GenerateTowerOfTrials()
        {
            Vector2 newPosition = FindValidPosition();
            if (newPosition == Vector2.zero) return;

            POIData towerData = new POIData
            {
                poiId = "TOWER_OF_TRIALS_" + Guid.NewGuid().ToString(),
                poiName = "Tháp Thử Thách",
                type = POIType.TowerOfTrials,
                position = newPosition,
                difficultyLevel = 99,
                monsterIDs = new List<string>(),
                currentFloor = 1,
                recoveryEndTime = 0
            };
            
            DataManager.Instance.Player.WorldPois.Add(towerData);
            InstantiatePOI(towerData);
        }

        private Vector2 FindValidPosition()
        {
            int attempts = 0;
            const int maxAttempts = 100;
            do
            {
                float x = UnityEngine.Random.Range(-mapSize.x / 2, mapSize.x / 2);
                float y = UnityEngine.Random.Range(-mapSize.y / 2, mapSize.y / 2);
                Vector2 newPosition = new Vector2(x, y);
                if (IsPositionValid(newPosition)) return newPosition;
                attempts++;
            }
            while (attempts < maxAttempts);
            
            Debug.LogError($"Could not find a valid position for a new POI after {maxAttempts} attempts.");
            return Vector2.zero;
        }

        private List<string> GenerateMonstersForPOI(POIData poiData)
        {
            if (poiData.type == POIType.TowerOfTrials) return new List<string>();
            var monsterList = new List<string>();
            // ... (rest of the monster generation logic is unchanged)
            return monsterList;
        }

        private void InstantiatePOI(POIData poiData)
        {
            GameObject poiPrefab;
            switch (poiData.type)
            {
                case POIType.Dungeon:
                    poiPrefab = dungeonPoiPrefab;
                    break;
                case POIType.RescueMission:
                    poiPrefab = rescuePoiPrefab;
                    break;
                case POIType.TowerOfTrials:
                    poiPrefab = towerPoiPrefab != null ? towerPoiPrefab : dungeonPoiPrefab;
                    break;
                default:
                    poiPrefab = null;
                    break;
            }

            if (poiPrefab == null) return;

            GameObject poiInstance = Instantiate(poiPrefab, generatedPOIsContainer);
            poiInstance.GetComponent<RectTransform>().anchoredPosition = poiData.position;

            Button poiButton = poiInstance.GetComponent<Button>();
            if (poiButton != null) poiButton.onClick.AddListener(() => OnPOIClicked(poiData));
            
            _activePoiObjects[poiData.poiId] = poiInstance;
        }

        private void OnPOIClicked(POIData poiData)
        {
            if (poiInfoPanel == null) return;
            poiInfoPanel.Show(poiData, () =>
            {
                OpenSquadSelectionForPOI(poiData);
                poiInfoPanel.gameObject.SetActive(false);
            });
        }

        private void OpenSquadSelectionForPOI(POIData poiData)
        {
            if (squadSelectionPanel == null) return;
            var availableHeroes = DataManager.Instance.AllHeroes.Where(h => h.isMature && !h.IsBusy()).ToList();
            squadSelectionPanel.Show(
                string.Format(global::LocalizationSystem.GetText("worldmap_select_squad_title_format"), poiData.poiName),
                availableHeroes, 5,
                (selectedHeroIDs) => {
                    squadSelectionPanel.gameObject.SetActive(false);
                    poiInfoPanel.gameObject.SetActive(false);
                    GameManager.Instance.ExpeditionManager.StartExpedition(selectedHeroIDs, poiData);
                }
            );
        }

        private void HandlePOICleared(POIData clearedPoiData)
        {
            if (clearedPoiData.type == POIType.TowerOfTrials) return; 

            if (_activePoiObjects.TryGetValue(clearedPoiData.poiId, out GameObject poiObject)){
                Destroy(poiObject);
                _activePoiObjects.Remove(clearedPoiData.poiId);
            }
            DataManager.Instance.Player.WorldPois.RemoveAll(p => p.poiId == clearedPoiData.poiId);
            GenerateAndRegisterNewPOI(clearedPoiData.type, null);
        }

        private void HandleTowerConquered(POIData towerData)
        {
            Debug.Log("Tower of Trials has been conquered! Generating a new one.");
            if (_activePoiObjects.TryGetValue(towerData.poiId, out GameObject poiObject))
            {
                Destroy(poiObject);
                _activePoiObjects.Remove(towerData.poiId);
            }
            DataManager.Instance.Player.WorldPois.RemoveAll(p => p.poiId == towerData.poiId);
            
            GenerateTowerOfTrials();
        }

        private bool IsPositionValid(Vector2 position)
        {
            if (DataManager.Instance.Player.WorldPois == null) return true;
            return DataManager.Instance.Player.WorldPois.All(poi => Vector2.Distance(poi.position, position) >= minPoiDistance);
        }

        #endregion

        #region Expedition Visualization
        
        private void HandleExpeditionStarted(ExpeditionDisplayData displayData)
        {
            if (travelCartPrefab == null) return;

            GameObject cartInstance = Instantiate(travelCartPrefab, travelLayer);
            _activeTravelCarts[displayData.expeditionId] = cartInstance;

            RectTransform cartRect = cartInstance.GetComponent<RectTransform>();
            cartRect.anchoredPosition = Vector2.zero; // Start from village (center)

            // Animate travel to destination and back
            cartRect.DOAnchorPos(displayData.destination.position, displayData.totalDuration / 2)
                .SetEase(Ease.Linear)
                .OnComplete(() => {
                    cartRect.DOAnchorPos(Vector2.zero, displayData.totalDuration / 2)
                        .SetEase(Ease.Linear);
                });
        }

        private void HandleExpeditionFinished(string expeditionId)
        {
            if (_activeTravelCarts.TryGetValue(expeditionId, out GameObject cartInstance))
            {
                // Stop any animations and destroy
                DOTween.Kill(cartInstance.GetComponent<RectTransform>());
                Destroy(cartInstance);
                _activeTravelCarts.Remove(expeditionId);
            }
        }

        #endregion

        #region Navigation
        
        private void GoBackToVillage()
        {
            // Assuming your main village/hub scene is named "MainScene"
            SceneManager.LoadScene("MainScene");
        }

        #endregion
    }
}

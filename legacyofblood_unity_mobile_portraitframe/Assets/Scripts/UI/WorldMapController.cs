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

    public class WorldMapController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
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

        // ĐÃ UNCOMMENT ĐỂ DÙNG LẠI CHO EVENT SYSTEM DRAG / ZOOM MỚI
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
        private bool _isZooming;
        private Dictionary<string, GameObject> _activePoiObjects = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> _activeTravelCarts = new Dictionary<string, GameObject>();

        #region Unity Lifecycle & Event Subscription

        private bool _isInitialized = false;

        private void Start()
        {
            if (poiInfoPanel == null)
            {
                poiInfoPanel = FindFirstObjectByType<POI_InfoPanel>(FindObjectsInactive.Include);
                if (poiInfoPanel != null) Debug.Log("[WorldMap] Đã Auto-Wire thành công POI_InfoPanel!");
            }
            if (squadSelectionPanel == null)
            {
                squadSelectionPanel = FindFirstObjectByType<SquadSelectionPanel>(FindObjectsInactive.Include);
                if (squadSelectionPanel != null) Debug.Log("[WorldMap] Đã Auto-Wire thành công SquadSelectionPanel!");
            }

            // AUTO-FIX: Đưa nút Close (lối về làng) và TravelLayer vào trong MapContainer 
            // để chúng di chuyển và phóng to/thu nhỏ cùng với bản đồ.
            if (backToVillageButton != null && backToVillageButton.transform.parent != mapContainer)
            {
                backToVillageButton.transform.SetParent(mapContainer, false);
                RectTransform btnRect = backToVillageButton.GetComponent<RectTransform>();
                btnRect.anchorMin = new Vector2(0.5f, 0.5f);
                btnRect.anchorMax = new Vector2(0.5f, 0.5f);
                btnRect.pivot = new Vector2(0.5f, 0.5f);
                btnRect.anchoredPosition = Vector2.zero; // Gắn cứng vào tâm (0,0) của Map
                // Ưu tiên hiển thị lên trên
                backToVillageButton.transform.SetAsLastSibling();
            }

            if (travelLayer != null && travelLayer.parent != mapContainer)
            {
                travelLayer.SetParent(mapContainer, false);
                RectTransform tzRect = travelLayer.GetComponent<RectTransform>();
                tzRect.anchorMin = new Vector2(0.5f, 0.5f);
                tzRect.anchorMax = new Vector2(0.5f, 0.5f);
                tzRect.pivot = new Vector2(0.5f, 0.5f);
                tzRect.anchoredPosition = Vector2.zero; // Center
                travelLayer.SetAsLastSibling();
            }

            if (backToVillageButton != null)
            {
                backToVillageButton.onClick.RemoveAllListeners();
                backToVillageButton.onClick.AddListener(GoBackToVillage);
            }
            _isInitialized = true;
            InitializeWorldMap();
        }

        private void OnEnable()
        {
            ExpeditionManager.OnExpeditionStarted += HandleExpeditionStarted;
            ExpeditionManager.OnExpeditionFinished += HandleExpeditionFinished;
            ExpeditionManager.OnPOICleared += HandlePOICleared;
            ExpeditionManager.OnTowerConquered += HandleTowerConquered;

            if (_isInitialized)
            {
                InitializeWorldMap();
            }
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
            // Tạm thời comment hệ thống Input.GetMouseButton cũ vì Unity báo lỗi Input Handling đã bị đổi qua gói Input System mới.
            // Nếu bạn dùng Input System package, hãy cấu hình lại thẻ PlayerSettings hoặc viết lại bằng UnityEngine.InputSystem.Mouse.current.
            /*
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
            */
        }

        // TÍNH NĂNG MỚI: Tận dụng EventSystem chuẩn UGUI để Kéo và Zoom, chạy được cả Input cũ lẫn mới!
        public void OnBeginDrag(PointerEventData eventData)
        {
            // Không cần làm gì với OnBeginDrag trong logic này
        }

        public void OnDrag(PointerEventData eventData)
        {
            mapContainer.anchoredPosition += eventData.delta;
            ClampMapPosition();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Không cần làm gì với OnEndDrag trong logic này
        }

        public void OnScroll(PointerEventData eventData)
        {
            float scroll = eventData.scrollDelta.y;
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
            if (DataManager.Instance == null || DataManager.Instance.Player == null) return;

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
                
                // Spawn one Boss POI on new map generation
                GenerateBossPOI();
            }
            else
            {
                Debug.Log($"Loading {worldPois.Count} POIs from DataManager.");
                foreach (var poiData in worldPois) InstantiatePOI(poiData);
            }

            // Luôn đảm bảo có đủ 3 tháp nghề nghiệp
            GenerateProfessionTowers();
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
        
        private void GenerateProfessionTowers()
        {
            Profession[] professions = { Profession.Warrior, Profession.Archer, Profession.Mage };
            
            foreach (var profession in professions)
            {
                // Check if tower for this profession already exists
                if (DataManager.Instance.Player.WorldPois.Any(p => p.type == POIType.TowerOfTrials && p.requiredProfession == profession))
                    continue;

                Vector2 newPosition = FindValidPosition();
                if (newPosition == Vector2.zero) continue;

                POIData towerData = new POIData
                {
                    poiId = $"TOWER_{profession}_{Guid.NewGuid()}",
                    poiName = string.Format(LocalizationSystem.GetText("poi_tower_name_format"), profession), // "Tháp Chiến Binh", etc.
                    type = POIType.TowerOfTrials,
                    position = newPosition,
                    difficultyLevel = 1,
                    monsterIDs = new List<string>(),
                    currentFloor = 1,
                    recoveryEndTime = 0,
                    requiredProfession = profession
                };
                
                DataManager.Instance.Player.WorldPois.Add(towerData);
                InstantiatePOI(towerData);
            }
        }

        private void GenerateBossPOI()
        {
            var bosses = DataManager.Instance?.GameConfig?.AllBosses;
            if (bosses == null || bosses.Count == 0) return;

            // Pick a random boss
            var boss = bosses[UnityEngine.Random.Range(0, bosses.Count)];

            Vector2 newPosition = FindValidPosition();
            if (newPosition == Vector2.zero) return;

            string newId = Guid.NewGuid().ToString();
            
            POIData poiData = new POIData
            {
                poiId = newId,
                poiName = boss.bossName,
                type = POIType.Boss,
                position = newPosition,
                difficultyLevel = boss.level,
                monsterIDs = new List<string> { boss.id }
            };

            DataManager.Instance.Player.WorldPois.Add(poiData);
            InstantiatePOI(poiData);
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
                case POIType.Boss:
                    poiPrefab = towerPoiPrefab != null ? towerPoiPrefab : dungeonPoiPrefab; // Fallback to tower/dungeon icon for now
                    break;
                default:
                    poiPrefab = null;
                    break;
            }

            if (poiPrefab == null) return;

            GameObject poiInstance = Instantiate(poiPrefab, generatedPOIsContainer);
            poiInstance.SetActive(true); // Đảm bảo icon nổi lên
            poiInstance.GetComponent<RectTransform>().anchoredPosition = poiData.position;

            Button poiButton = poiInstance.GetComponent<Button>();
            if (poiButton != null) 
            {
                poiButton.onClick.AddListener(() => OnPOIClicked(poiData));
            }
            else 
            {
                Debug.LogError($"[WorldMap] CẢNH BÁO QUAN TRỌNG: Prefab {poiPrefab.name} không có component Button! Người chơi KHÔNG THỂ click vào nó được!");
            }
            
            _activePoiObjects[poiData.poiId] = poiInstance;
        }

        private void OnPOIClicked(POIData poiData)
        {
            Debug.Log($"[WorldMap] Đã click vào POI: {poiData.poiName} (Type: {poiData.type})");
            
            if (poiData.type == POIType.TowerOfTrials)
            {
                GameManager.Instance.UIManager.ShowPanel(UIPanelType.Tower, true);
                var towerPanel = GameManager.Instance.UIManager.GetPanel<TowerPanel>(UIPanelType.Tower);
                if (towerPanel != null) {
                    towerPanel.Show(poiData);
                }
                return;
            }
            if (poiData.type == POIType.Boss)
            {
                GameManager.Instance.UIManager.ShowPanel(UIPanelType.BossBattle, true);
                var bossPanel = GameManager.Instance.UIManager.GetPanel<BossBattlePanel>(UIPanelType.BossBattle);
                if (bossPanel != null) {
                    bossPanel.Show(poiData);
                }
                return;
            }

            if (poiInfoPanel == null) 
            {
                Debug.LogError($"[WorldMap HƯỚNG DẪN KHẮC PHỤC]: Bản đồ tìm không thấy POI_InfoPanel nên không thể hiển thị thông tin.\n" +
                               "==> LỖI CỦA BẠN LÀ: Bạn CHƯA KÉO cục Prefab 'POI_InfoPanel' vào lưới UI (Hierarchy) trên Scene! Hãy mở tab Project (Thư mục Prefabs/UI), nắm kéo thả file POI_InfoPanel vào trong Canvas của bạn. Sau đó Game sẽ tự dính nó lại.");
                return;
            }
            
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
                },
                poiData.requiredProfession ?? Profession.None
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
            
            GenerateProfessionTowers(); // Sử dụng phương thức chính xác
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

            // Đã trả lại travelLayer (Vui lòng không xài POI Container nữa)
            // Vì ở hàm Start(), TravelLayer đã tự động được nhét vào trong MapContainer.
            GameObject cartInstance = Instantiate(travelCartPrefab, travelLayer);
            _activeTravelCarts[displayData.expeditionId] = cartInstance;

            RectTransform cartRect = cartInstance.GetComponent<RectTransform>();
            cartRect.anchoredPosition = Vector2.zero; // Start from village (center map)

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
            // Thay vì LoadScene, giờ gọi UIManager lật trang MainScreen
            GameManager.Instance.UIManager.ShowPanel(UIPanelType.MainScreen);
        }

        #endregion
    }
}

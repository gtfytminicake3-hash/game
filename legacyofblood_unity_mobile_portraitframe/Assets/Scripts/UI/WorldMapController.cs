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
    using TMPro;

    public class WorldMapController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
        [Header("Testing / Debug")]
        [SerializeField] private Button regenerateMapButton; // Nút ẩn để test, hoặc bạn có thể gán tạm một nút nào đó trên Canvas để bấm

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
        [SerializeField] private Vector2 mapSize = new Vector2(2160, 3840);
        [SerializeField] private int numberOfDungeons = 40;
        [SerializeField] private int numberOfRescues = 20;

        [Header("Navigation")]
        [SerializeField] private Button backToVillageButton;
        [SerializeField] private TextMeshProUGUI backToVillageButtonText;

        private Vector2 _lastPanPosition;
        private bool _isZooming;
        private Dictionary<string, GameObject> _activePoiObjects = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> _activeTravelCarts = new Dictionary<string, GameObject>();

        #region Unity Lifecycle & Event Subscription

        private bool _isInitialized = false;

        private void Start()
        {
            // Cập nhật khoảng cách an toàn tối thiểu dựa trên kích thước thật của POI (khoảng 200x200)
            if (numberOfDungeons < 40) numberOfDungeons = 40;
            if (numberOfRescues < 20) numberOfRescues = 20;
            if (minPoiDistance < 350f) minPoiDistance = 350f;

            // Đảm bảo không bị Inspector ghi đè Size theo config màn hình dọc
            if (mapSize.x < 2160) mapSize.x = 2160;
            if (mapSize.y < 3840) mapSize.y = 3840;

            if (poiInfoPanel == null)
            {
                poiInfoPanel = System.Linq.Enumerable.FirstOrDefault(Resources.FindObjectsOfTypeAll<POI_InfoPanel>(), obj => obj.gameObject.scene.IsValid());
                if (poiInfoPanel != null) Debug.Log("[WorldMap] Đã Auto-Wire thành công POI_InfoPanel từ Scene!");
            }
            if (squadSelectionPanel == null)
            {
                squadSelectionPanel = System.Linq.Enumerable.FirstOrDefault(Resources.FindObjectsOfTypeAll<SquadSelectionPanel>(), obj => obj.gameObject.scene.IsValid());
                if (squadSelectionPanel != null) Debug.Log("[WorldMap] Đã Auto-Wire thành công SquadSelectionPanel từ Scene!");
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
            if (backToVillageButtonText != null)
            {
                backToVillageButtonText.text = global::LocalizationSystem.GetText("btn_back_to_village");
            }

            if (regenerateMapButton != null)
            {
                regenerateMapButton.onClick.RemoveAllListeners();
                regenerateMapButton.onClick.AddListener(ForceRegenerateMap);
            }

            _isInitialized = true;
            InitializeWorldMap();
        }

        private void OnEnable()
        {
            ExpeditionManager.OnExpeditionStarted += HandleExpeditionStarted;
            ExpeditionManager.OnExpeditionStateChanged += HandleExpeditionStateChanged;
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
            ExpeditionManager.OnExpeditionStateChanged -= HandleExpeditionStateChanged;
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

        public void ForceRegenerateMap()
        {
            if (DataManager.Instance == null || DataManager.Instance.Player == null) return;
            
            Debug.Log("[WorldMap] Forcing full map regeneration...");
            
            // Xóa hết Model hiện tại trong Save Data
            if (DataManager.Instance.Player.WorldPois != null)
            {
                DataManager.Instance.Player.WorldPois.Clear();
            }
            
            // Reset UI Khởi tạo Game
            InitializeWorldMap();
        }

        private void InitializeWorldMap()
        {
            if (DataManager.Instance == null || DataManager.Instance.Player == null) return;

            foreach (Transform child in generatedPOIsContainer) Destroy(child.gameObject);
            _activePoiObjects.Clear();

            var worldPois = DataManager.Instance.Player.WorldPois;

            if (worldPois == null)
            {
                DataManager.Instance.Player.WorldPois = new List<POIData>();
                worldPois = DataManager.Instance.Player.WorldPois;
            }

            int currentDungeons = worldPois.Count(p => p.type == POIType.Dungeon);
            int currentRescues = worldPois.Count(p => p.type == POIType.RescueMission);
            int currentBosses = worldPois.Count(p => p.type == POIType.Boss);

            if (currentDungeons == 0 && currentRescues == 0)
            {
                Debug.Log("No POI data found, generating new POIs...");
            }
            else
            {
                Debug.Log($"Bypass check: current Dungeon {currentDungeons}, Rescue {currentRescues}. Target: {numberOfDungeons}, {numberOfRescues}");
            }
            
            // Top up Dungeons if missing
            if (currentDungeons < numberOfDungeons)
            {
                int missingDungeons = numberOfDungeons - currentDungeons;
                for (int i = 0; i < missingDungeons; i++) GenerateAndRegisterNewPOI(POIType.Dungeon, null, (i % 10) + 1);
            }
            // Top up Rescues if missing
            if (currentRescues < numberOfRescues)
            {
                int missingRescues = numberOfRescues - currentRescues;
                for (int i = 0; i < missingRescues; i++) GenerateAndRegisterNewPOI(POIType.RescueMission, null, (i % 10) + 1);
            }
            
            // Spawn Boss if missing
            if (currentBosses == 0)
            {
                GenerateBossPOI();
            }

            Debug.Log($"Loading {worldPois.Count} POIs from DataManager.");
                
                // --- BẮT ĐẦU FIX LỖI ĐẺ TRÙNG THÁP ---
                // Giữ lại tối đa 4 tháp, ưu tiên các tháp có tầng cao nhất
                var allTowers = worldPois.Where(p => p.type == POIType.TowerOfTrials).ToList();
                var validTowersList = allTowers.Where(p => p.requiredProfession != Profession.None).OrderByDescending(p => p.currentFloor).ToList();
                var duplicateTowersToRemove = new List<POIData>();

                // Xóa tháp bị lỗi System Data
                duplicateTowersToRemove.AddRange(allTowers.Where(p => p.requiredProfession == Profession.None));

                // Nếu tổng số tháp hợp lệ vượt quá 4, xóa các tháp cấp thấp
                if (validTowersList.Count > 4)
                {
                    duplicateTowersToRemove.AddRange(validTowersList.Skip(4));
                }

                if (duplicateTowersToRemove.Count > 0)
                {
                    Debug.Log($"[WorldMap] Tìm thấy {duplicateTowersToRemove.Count} Tháp dư thừa/lỗi. Đang dọn dẹp...");
                    foreach (var badTower in duplicateTowersToRemove)
                    {
                        worldPois.Remove(badTower);
                    }
                }
                // --- KẾT THÚC FIX LỖI ĐẺ TRÙNG THÁP ---

                foreach (var poiData in worldPois)
                {
                    // FIX: Nếu POIData cũ bị lỗi list rỗng (trừ Tower), sinh lại quái!
                    if (poiData.type != POIType.TowerOfTrials && (poiData.monsterIDs == null || poiData.monsterIDs.Count == 0))
                    {
                        poiData.monsterIDs = GenerateMonstersForPOI(poiData);
                    }
                    InstantiatePOI(poiData);
                }

            // Luôn đảm bảo có đủ 4 tháp nghề nghiệp
            GenerateProfessionTowers();

            // Khôi phục các chuyến thám hiểm đang chạy nếu có (để render lại xe ngựa)
            if (DataManager.Instance.Player.ActiveExpeditions != null)
            {
                foreach (var exp in DataManager.Instance.Player.ActiveExpeditions)
                {
                    HandleExpeditionStateChanged(exp);
                }
            }
        }

        private void GenerateAndRegisterNewPOI(POIType type, string specificName, int targetDifficulty = -1)
        {
            Vector2 newPosition = targetDifficulty > 0 ? FindValidPositionForDifficulty(targetDifficulty) : FindValidPosition();
            if (newPosition == Vector2.zero) return;

            string newId = Guid.NewGuid().ToString();
            
            int calculatedDifficulty = targetDifficulty;
            if (calculatedDifficulty <= 0)
            {
                // Calculate distance from center (Village)
                float distanceToCenter = Vector2.Distance(newPosition, Vector2.zero);
                float maxDistance = Mathf.Max(mapSize.x, mapSize.y) / 2f;
                float distanceRatio = Mathf.Clamp01(distanceToCenter / maxDistance);
                
                // Map the ratio to difficulty level (1 to 10)
                calculatedDifficulty = Mathf.Clamp(Mathf.RoundToInt(distanceRatio * 10f), 1, 10);
            }
            
            // FIX: Create the object first, then assign monsterIDs
            POIData poiData = new POIData
            {
                poiId = newId,
                poiName = specificName ?? string.Format(global::LocalizationSystem.GetText("poi_name_format"), global::LocalizationSystem.GetText($"poi_type_{type}"), UnityEngine.Random.Range(10, 999)),
                type = type,
                position = newPosition,
                difficultyLevel = calculatedDifficulty
            };
            poiData.monsterIDs = GenerateMonstersForPOI(poiData);

            DataManager.Instance.Player.WorldPois.Add(poiData);
            InstantiatePOI(poiData);
        }
        
        private void GenerateProfessionTowers()
        {
            Profession[] professions = { Profession.Warrior, Profession.Archer, Profession.Mage, Profession.Healer };
            
            int currentTowers = DataManager.Instance.Player.WorldPois.Count(p => p.type == POIType.TowerOfTrials);
            int towersToSpawn = 4 - currentTowers;

            for (int i = 0; i < towersToSpawn; i++)
            {
                Vector2 newPosition = FindValidPosition();
                if (newPosition == Vector2.zero) continue;

                Profession randomProfession = professions[UnityEngine.Random.Range(0, professions.Length)];

                POIData towerData = new POIData
                {
                    poiId = $"TOWER_{randomProfession}_{Guid.NewGuid()}",
                    poiName = string.Format(LocalizationSystem.GetText("poi_tower_name_format"), randomProfession), // "Tháp Chiến Binh", etc.
                    type = POIType.TowerOfTrials,
                    position = newPosition,
                    difficultyLevel = 1,
                    monsterIDs = new List<string>(),
                    currentFloor = 1,
                    recoveryEndTime = 0,
                    requiredProfession = randomProfession
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
            // Pick a random difficulty ring (1 to 10) to keep the concentric circle constraint always active even for random POIs
            int randomRing = UnityEngine.Random.Range(1, 11);
            return FindValidPositionForDifficulty(randomRing);
        }

        private Vector2 FindValidPositionForDifficulty(int targetDifficulty)
        {
            int attempts = 0;
            const int maxAttempts = 1000;
            
            // Dùng toàn bộ chiều cao màn hình để rải các vòng (elliptical distribution cho màn dọc)
            // Lấy kích thước thực làm trục lớn (Height) và trục nhỏ (Width)
            float maxRadiusY = mapSize.y / 2f - 100f; // Chừa lề trên dưới ít hơn để mở rộng
            float maxRadiusX = mapSize.x / 2f - 50f;  // Chừa lề trái phải ít hơn để mở rộng
            
            // Core safety gap in the middle
            float minGapY = 350f;
            float minGapX = 250f;

            // Mỗi ring tier sẽ chiếm 1 khoảng
            float ringThicknessY = (maxRadiusY - minGapY) / 10f;
            float ringThicknessX = (maxRadiusX - minGapX) / 10f;

            float minRankY = minGapY + ((targetDifficulty - 1) * ringThicknessY);
            float maxRankY = minGapY + (targetDifficulty * ringThicknessY);

            float minRankX = minGapX + ((targetDifficulty - 1) * ringThicknessX);
            float maxRankX = minGapX + (targetDifficulty * ringThicknessX);

            do
            {
                // Chọn một góc ngẫu nhiên trên hệ trục Ellipse
                float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2);
                
                // Chọn một điểm cách tâm theo Rank hiện tại
                float radiusY = UnityEngine.Random.Range(minRankY, maxRankY);
                float radiusX = UnityEngine.Random.Range(minRankX, maxRankX);
                
                // Áp dụng tính toán Ellipse (x= a*cos, y = b*sin)
                float x = Mathf.Cos(angle) * radiusX;
                float y = Mathf.Sin(angle) * radiusY;
                
                Vector2 newPosition = new Vector2(x, y);
                
                // Chỉ cần check khoảng cách min giữa các hạt POI, không cần cắt gọt bằng boundary nữa 
                // vì công thức x,y ở trên đã được scale chặt vào maxRadiusX và maxRadiusY của màn hình rồi.
                if (IsPositionValid(newPosition)) 
                {
                    return newPosition;
                }
                
                attempts++;
            }
            while (attempts < maxAttempts);
            
            Debug.LogWarning($"[WorldMap] Không tìm được chỗ trống khắt khe cho vòng {targetDifficulty} sau {maxAttempts} thử nghiệm. Rải ngẫu nhiên nhưng vẫn cố gắng giữ khoảng cách.");
            
            // Fallback random nhưng vẫn check khoảng cách
            attempts = 0;
            while (attempts < 2000)
            {
                Vector2 fallbackPos = new Vector2(
                    UnityEngine.Random.Range(-mapSize.x/2f + 50f, mapSize.x/2f - 50f),
                    UnityEngine.Random.Range(-mapSize.y/2f + 100f, mapSize.y/2f - 100f)
                );
                if (IsPositionValid(fallbackPos))
                {
                    return fallbackPos;
                }
                attempts++;
            }
            
            Debug.LogWarning("[WorldMap] Fallback giữ khoảng cách thất bại. Rải đè.");
            return new Vector2(
                UnityEngine.Random.Range(-mapSize.x/2f + 50f, mapSize.x/2f - 50f),
                UnityEngine.Random.Range(-mapSize.y/2f + 100f, mapSize.y/2f - 100f)
            );
        }

        private List<string> GenerateMonstersForPOI(POIData poiData)
        {
            if (poiData.type == POIType.TowerOfTrials) return new List<string>();
            var monsterList = new List<string>();
            
            var config = DataManager.Instance?.GameConfig?.POIMonsterConfig;
            if (config != null && config.monsterGroups != null)
            {
                var validGroups = config.monsterGroups.Where(g => 
                    poiData.difficultyLevel >= g.minDifficulty && 
                    poiData.difficultyLevel <= g.maxDifficulty).ToList();
                
                if (validGroups.Any())
                {
                    var group = validGroups[UnityEngine.Random.Range(0, validGroups.Count)];
                    if (group.monsterIDs != null && group.monsterIDs.Count > 0)
                    {
                        // Sinh 2 đến 4 quái ngẫu nhiên thay vì 2 đến 6 để map nhẹ hơn nếu cần
                        int monsterCount = UnityEngine.Random.Range(2, 5);
                        for (int i = 0; i < monsterCount; i++)
                        {
                            string randomMonster = group.monsterIDs[UnityEngine.Random.Range(0, group.monsterIDs.Count)];
                            monsterList.Add(randomMonster);
                        }
                    }
                }
            }

            // Nếu không tìm thấy config hoặc group phù hợp, lấy đại quái trong config để không bao giờ bị CP 0
            if (monsterList.Count == 0 && config != null && config.monsterGroups != null)
            {
                var allMonsterIDs = config.monsterGroups
                    .Where(g => g.monsterIDs != null)
                    .SelectMany(g => g.monsterIDs)
                    .Where(id => !string.IsNullOrEmpty(id))
                    .Distinct()
                    .ToList();
                    
                if (allMonsterIDs.Count > 0)
                {
                    int backupCount = UnityEngine.Random.Range(2, 5);
                    for (int i = 0; i < backupCount; i++)
                    {
                        monsterList.Add(allMonsterIDs[UnityEngine.Random.Range(0, allMonsterIDs.Count)]);
                    }
                }
                else if (DataManager.Instance?.GameConfig?.AllBosses != null && DataManager.Instance.GameConfig.AllBosses.Count > 0)
                {
                    var bosses = DataManager.Instance.GameConfig.AllBosses;
                    monsterList.Add(bosses[UnityEngine.Random.Range(0, bosses.Count)].id);
                }
            }

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

            // --- Tùy chỉnh Hình Ảnh cho Tháp Thử Thách theo Nghề Nghiệp ---
            if (poiData.type == POIType.TowerOfTrials)
            {
                Image iconImage = poiInstance.GetComponent<Image>();
                if (iconImage != null)
                {
                    string spriteName = string.Empty;
                    switch (poiData.requiredProfession)
                    {
                        case Profession.Mage: spriteName = "thap_mage"; break;
                        case Profession.Healer: spriteName = "thap_healer"; break;
                        case Profession.Archer: spriteName = "thap_acher"; break;
                        case Profession.Warrior: spriteName = "thap_warior"; break;
                    }

                    if (!string.IsNullOrEmpty(spriteName))
                    {
                        Sprite towerSprite = Resources.Load<Sprite>($"UI/Towers/{spriteName}");
                        if (towerSprite != null)
                        {
                            iconImage.sprite = towerSprite;

                            // Đảm bảo không bị méo ảnh
                            iconImage.preserveAspect = true;
                            // Optionally set native size if the tower image is tiny
                            // iconImage.SetNativeSize(); 
                        }
                    }
                }
            }
            // -----------------------------------------------------------

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
            var currentSquadPanel = GameManager.Instance.UIManager.GetPanel<SquadSelectionPanel>(UIPanelType.SquadSelection);
            if (currentSquadPanel == null) 
            {
                currentSquadPanel = squadSelectionPanel; // Fallback
            }
            if (currentSquadPanel == null) return;

            var availableHeroes = DataManager.Instance.AllHeroes.Where(h => h.isMature && !h.IsBusy()).ToList();
            currentSquadPanel.Show(
                title: string.Format(global::LocalizationSystem.GetText("worldmap_select_squad_title_format"), poiData.poiName),
                availableHeroes: availableHeroes, 
                squadSize: 5,
                onConfirm: (selectedHeroIDs, diff) => 
                {
                    // Dùng UIManager để Hide Panel cho chuẩn
                    GameManager.Instance.UIManager.HidePanel(UIPanelType.SquadSelection);
                    GameManager.Instance.UIManager.HidePanel(UIPanelType.Tower);
                    if (poiInfoPanel != null) poiInfoPanel.gameObject.SetActive(false);
                    
                    Debug.Log($"[WorldMap] Tham số chọn đội hình: {string.Join(", ", selectedHeroIDs)} - Độ khó: {diff}");
                    poiData.difficultyLevel = diff;
                    GameManager.Instance.ExpeditionManager.StartExpedition(selectedHeroIDs, poiData);
                },
                requiredProfession: poiData.requiredProfession,
                initialDifficulty: poiData.difficultyLevel
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
            GenerateAndRegisterNewPOI(clearedPoiData.type, null, clearedPoiData.difficultyLevel);
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
            // Yêu cầu: Cách Tâm (Làng) ÍT NHẤT 300px 
            if (Vector2.Distance(position, Vector2.zero) < 300f)
            {
                return false;
            }

            if (DataManager.Instance.Player.WorldPois == null) return true;
            return DataManager.Instance.Player.WorldPois.All(poi => Vector2.Distance(poi.position, position) >= minPoiDistance);
        }

        #endregion

        #region Expedition Visualization
        
        private void HandleExpeditionStarted(ExpeditionDisplayData displayData)
        {
            // Just wait for HandleExpeditionStateChanged
        }

        private void HandleExpeditionStateChanged(ActiveExpedition expedition)
        {
            if (travelCartPrefab == null) return;

            GameObject cartInstance;
            RectTransform cartRect;

            if (!_activeTravelCarts.TryGetValue(expedition.expeditionId, out cartInstance))
            {
                cartInstance = Instantiate(travelCartPrefab, travelLayer);
                _activeTravelCarts[expedition.expeditionId] = cartInstance;

                // Thêm hiệu ứng Highlight (Viền sáng nhấp nháy) cho xe mới
                UnityEngine.UI.Image cartImage = cartInstance.GetComponentInChildren<UnityEngine.UI.Image>();
                if (cartImage != null)
                {
                    var outline = cartImage.gameObject.AddComponent<UnityEngine.UI.Outline>();
                    outline.effectColor = new Color(1f, 0.85f, 0.0f, 1f); // Màu Vàng Sáng nổi bật
                    outline.effectDistance = new Vector2(4f, -4f);
                    
                    DOTween.To(() => outline.effectColor, x => outline.effectColor = x, new Color(1f, 0.85f, 0.0f, 0.2f), 0.6f)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetTarget(cartInstance.GetComponent<RectTransform>()); 
                }
            }

            cartRect = cartInstance.GetComponent<RectTransform>();
            DOTween.Kill(cartRect); // Hủy các lệnh tween cũ trước khi set state mới

            var poi = DataManager.Instance.GetPOIByID(expedition.poiId);
            if (poi == null) return;

            Vector2 villagePos = Vector2.zero;
            Vector2 destinationPos = poi.position;
            bool isMovingRight = destinationPos.x > villagePos.x;

            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            float timeRemaining = Mathf.Max(0, (expedition.stateEndTimestamp - currentTime) / 1000f);

            switch (expedition.currentState)
            {
                case ExpeditionState.Traveling:
                    cartRect.localScale = new Vector3(isMovingRight ? -1f : 1f, 1f, 1f);
                    // Bắt đầu từ làng
                    if (timeRemaining >= expedition.travelDurationMs / 1000f) 
                    {
                        cartRect.anchoredPosition = villagePos;
                    }
                    cartRect.DOAnchorPos(destinationPos, timeRemaining).SetEase(Ease.Linear);
                    break;

                case ExpeditionState.Exploring:
                    cartRect.localScale = new Vector3(isMovingRight ? -1f : 1f, 1f, 1f);
                    cartRect.anchoredPosition = destinationPos;
                    // Có thể thêm hiệu ứng bụi mờ, kiếm chém ở đây nếu muốn
                    break;

                case ExpeditionState.Returning:
                    cartRect.localScale = new Vector3(isMovingRight ? 1f : -1f, 1f, 1f);
                    if (timeRemaining >= expedition.travelDurationMs / 1000f)
                    {
                        cartRect.anchoredPosition = destinationPos;
                    }
                    cartRect.DOAnchorPos(villagePos, timeRemaining).SetEase(Ease.Linear);
                    break;
            }
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

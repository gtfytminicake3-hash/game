namespace LegendOfBlood
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using DG.Tweening;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Điều khiển toàn bộ logic và tương tác trên màn hình Bản đồ Thế giới.
    /// </summary>
    public class WorldMapController : MonoBehaviour
    {
        [Header("Scene References")]
        [Tooltip("Node cha chứa tất cả các yếu tố của bản đồ (nền, POI). Đây là node sẽ được di chuyển/zoom.")]
        [SerializeField] private RectTransform mapContainer;
        [Tooltip("Node con chứa các POI được tạo ngẫu nhiên.")]
        [SerializeField] private Transform generatedPOIsContainer;
        [Tooltip("Layer riêng để hiển thị các đối tượng di chuyển (xe ngựa).")]
        [SerializeField] private Transform travelLayer;
        [Tooltip("Kéo SquadSelectionPanel từ Hierarchy của Scene này vào đây.")]
        [SerializeField] private SquadSelectionPanel squadSelectionPanel;
        [SerializeField] private POI_InfoPanel poiInfoPanel;

        [Header("Prefabs")]
        [SerializeField] private GameObject dungeonPoiPrefab;
        [SerializeField] private GameObject rescuePoiPrefab;
        [SerializeField] private GameObject travelCartPrefab;

        [Header("Map Settings")]
        [SerializeField] private float minZoom = 0.5f;
        [SerializeField] private float maxZoom = 2.0f;
        [SerializeField] private float zoomSpeed = 0.1f;
        [SerializeField] private Vector2 mapSize = new Vector2(2000, 1500);
        [SerializeField] private int numberOfDungeons = 10;
        [SerializeField] private int numberOfRescues = 5;

        [Header("Navigation")]
        [SerializeField] private Button backToVillageButton;

        // --- State Variables ---
        private Vector2 _lastPanPosition;
        private bool _isPanning;
        private bool _isZooming;
        private Dictionary<string, GameObject> _activeTravelCarts = new Dictionary<string, GameObject>();

        #region Unity Lifecycle & Event Subscription

        private void Start()
        {
            // Gán sự kiện cho nút quay lại
            if (backToVillageButton != null)
            {
                backToVillageButton.onClick.AddListener(GoBackToVillage);
            }
        }

        private void OnEnable()
        {
            EventManager.StartListening<Expedition>(GameEvents.OnExpeditionStarted, HandleExpeditionStarted);
            EventManager.StartListening<Expedition>(GameEvents.OnExpeditionReturning, HandleExpeditionReturning);
            EventManager.StartListening<Expedition>(GameEvents.OnExpeditionFinished, HandleExpeditionFinished);
            GenerateRandomPOIs();
        }

        private void OnDisable()
        {
            EventManager.StopListening<Expedition>(GameEvents.OnExpeditionStarted, HandleExpeditionStarted);
            EventManager.StopListening<Expedition>(GameEvents.OnExpeditionReturning, HandleExpeditionReturning);
            EventManager.StopListening<Expedition>(GameEvents.OnExpeditionFinished, HandleExpeditionFinished);
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
            if (Input.touchCount == 2)
            {
                HandleZoom();
                _isZooming = true;
                _isPanning = false;
            }
            else if (Input.touchCount == 1)
            {
                if (!_isZooming)
                {
                    HandlePan();
                }
            }
            else
            {
                _isPanning = false;
                _isZooming = false;
            }
        }

        private void HandlePan()
        {
            Touch touch = Input.GetTouch(0);
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                _isPanning = false;
                return;
            }
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    _lastPanPosition = touch.position;
                    _isPanning = true;
                    break;
                case TouchPhase.Moved:
                    if (_isPanning)
                    {
                        Vector2 panDelta = touch.position - _lastPanPosition;
                        mapContainer.anchoredPosition += panDelta;
                        ClampMapPosition();
                        _lastPanPosition = touch.position;
                    }
                    break;
                case TouchPhase.Ended:
                    _isPanning = false;
                    break;
            }
        }

        private void HandleZoom()
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;
            float difference = currentMagnitude - prevMagnitude;
            float newScale = mapContainer.localScale.x + difference * zoomSpeed * Time.deltaTime;
            newScale = Mathf.Clamp(newScale, minZoom, maxZoom);
            mapContainer.localScale = Vector3.one * newScale;
            ClampMapPosition();
        }

        private void ClampMapPosition()
        {
            Vector2 viewSize = GetComponent<RectTransform>().rect.size;
            Vector2 currentMapSize = mapSize * mapContainer.localScale.x;
            Vector2 minPos = (viewSize - currentMapSize) / 2;
            Vector2 maxPos = (currentMapSize - viewSize) / 2;
            if (currentMapSize.x < viewSize.x) { minPos.x = -minPos.x; maxPos.x = -maxPos.x; }
            if (currentMapSize.y < viewSize.y) { minPos.y = -minPos.y; maxPos.y = -maxPos.y; }
            Vector2 pos = mapContainer.anchoredPosition;
            pos.x = Mathf.Clamp(pos.x, -maxPos.x, -minPos.x);
            pos.y = Mathf.Clamp(pos.y, -maxPos.y, -minPos.y);
            mapContainer.anchoredPosition = pos;
        }

        #endregion

        #region POI & Expedition Logic

        private void GenerateRandomPOIs()
        {
            foreach (Transform child in generatedPOIsContainer)
            {
                Destroy(child.gameObject);
            }
            for (int i = 0; i < numberOfDungeons; i++)
            {
                CreatePOI(dungeonPoiPrefab, POIType.Dungeon, i);
            }
            for (int i = 0; i < numberOfRescues; i++)
            {
                CreatePOI(rescuePoiPrefab, POIType.RescueMission, i);
            }
        }

        private void CreatePOI(GameObject poiPrefab, POIType type, int index)
        {
            if (poiPrefab == null)
            {
                Debug.LogError($"Prefab cho POIType '{type}' chưa được gán!");
                return;
            }
            GameObject poiInstance = Instantiate(poiPrefab, generatedPOIsContainer);
            RectTransform poiRect = poiInstance.GetComponent<RectTransform>();
            float x = Random.Range(-mapSize.x / 2, mapSize.x / 2);
            float y = Random.Range(-mapSize.y / 2, mapSize.y / 2);
            poiRect.anchoredPosition = new Vector2(x, y);
            POIData poiData = new POIData
            {
                poiId = $"{type}_{index}",
                poiName = string.Format(global::LocalizationSystem.GetText("poi_name_format"), global::LocalizationSystem.GetText($"poi_type_{type}"), index + 1),
                type = type,
                position = poiRect.anchoredPosition,
                difficultyLevel = Random.Range(1, 10)
            };
            Button poiButton = poiInstance.GetComponent<Button>();
            if (poiButton != null)
            {
                poiButton.onClick.AddListener(() => OnPOIClicked(poiData));
            }
        }

        private void OnPOIClicked(POIData poiData)
        {
            if (poiInfoPanel == null)
            {
                Debug.LogError("POI_InfoPanel chưa được gán trong WorldMapController!");
                return;
            }

            // Hiển thị panel thông tin, và truyền vào một hành động (Action)
            // Hành động này sẽ được gọi khi người chơi nhấn nút "Khám phá"
            poiInfoPanel.Show(poiData, () =>
            {
                // Đây là code sẽ chạy KHI người chơi nhấn "Khám phá"
                // 1. GỌI HÀM MỞ PANEL CHỌN ĐỘI HÌNH
                OpenSquadSelectionForPOI(poiData);

                // 2. NGAY SAU ĐÓ, ĐÓNG PANEL INFO LẠI
                poiInfoPanel.gameObject.SetActive(false);
            });
        }

        // Tách logic mở SquadSelectionPanel ra một hàm riêng cho gọn
        private void OpenSquadSelectionForPOI(POIData poiData)
        {
            if (squadSelectionPanel == null)
            {
                Debug.LogError("SquadSelectionPanel chưa được gán trong WorldMapController!");
                return;
            }

            var availableHeroes = DataManager.Instance.AllHeroes.Where(h => h.isMature && !h.IsBusy()).ToList();

            squadSelectionPanel.Show(
                string.Format(global::LocalizationSystem.GetText("worldmap_select_squad_title_format"), poiData.poiName),
                availableHeroes,
                5,
                (selectedHeroIDs) => {
                    // Đây là callback cuối cùng, khi người chơi đã CHỐT đội hình
                    if (GameManager.Instance != null && GameManager.Instance.ExpeditionManager != null)
                    {
                        // Tắt các panel phụ đi
                        squadSelectionPanel.gameObject.SetActive(false);
                        poiInfoPanel.gameObject.SetActive(false);

                        // Bắt đầu chuyến đi, xe ngựa sẽ di chuyển
                        GameManager.Instance.ExpeditionManager.StartExpedition(selectedHeroIDs, poiData);
                    }
                }
            );
        }

        #endregion

        #region Expedition Visualization

        private void HandleExpeditionStarted(Expedition expedition)
        {
            Vector3 startPos = Vector3.zero; // Vị trí làng
            Vector3 endPos = expedition.destination.position;

            GameObject cart = Instantiate(travelCartPrefab, travelLayer);
            cart.transform.localPosition = startPos;

            _activeTravelCarts.Add(expedition.id, cart);

            float duration = Vector3.Distance(startPos, endPos) / 100f; // Tốc độ di chuyển
            cart.transform.DOLocalMove(endPos, duration).SetEase(Ease.Linear).OnComplete(() => {
                cart.SetActive(false); // Ẩn xe ngựa khi đến nơi
            }).SetId(this); // Gán ID để có thể hủy tween
        }

        private void HandleExpeditionReturning(Expedition expedition)
        {
            if (_activeTravelCarts.TryGetValue(expedition.id, out GameObject cart))
            {
                cart.SetActive(true);
                Vector3 startPos = expedition.destination.position;
                Vector3 endPos = Vector3.zero; // Về làng

                float duration = Vector3.Distance(startPos, endPos) / 100f;
                cart.transform.DOLocalMove(endPos, duration).SetEase(Ease.Linear).SetId(this);
            }
        }

        private void HandleExpeditionFinished(Expedition expedition)
        {
            if (_activeTravelCarts.TryGetValue(expedition.id, out GameObject cart))
            {
                Destroy(cart);
                _activeTravelCarts.Remove(expedition.id);
            }
        }

        #endregion

        #region Navigation

        public void GoBackToVillage()
        {
            SceneManager.LoadScene("MainScene");
        }

        #endregion
    }
}
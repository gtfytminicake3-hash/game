using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LegendOfBlood
{
    public class WorldMapFixedController : UIPanel, IScrollHandler
    {
        [Header("Map Content")]
        [SerializeField] private RectTransform mapContainer;
        [SerializeField] private GameObject regionDetailPopup;

        [Header("Region Buttons")]
        [SerializeField] private Button btnRegionSnow;
        [SerializeField] private Button btnRegionForest;
        [SerializeField] private Button btnRegionTree;
        [SerializeField] private Button btnRegionDesert;
        [SerializeField] private Button btnRegionVolcano;

        [Header("Navigation")]
        [SerializeField] private Button btnLobby;

        [Header("Zoom Settings")]
        [SerializeField] private float minZoom = 0.5f;
        [SerializeField] private float maxZoom = 2.0f;
        [SerializeField] private float zoomSpeed = 0.1f;

        // Dynamic UI for Expedition Tracker
        private GameObject _expeditionTrackerPanel;
        private TMPro.TextMeshProUGUI _expeditionTrackerText;

        private void Awake()
        {
            PanelType = UIPanelType.WorldMap;

            // Bind Region Buttons
            if (btnRegionSnow != null) btnRegionSnow.onClick.AddListener(() => OpenRegion("Snowy Mountains"));
            if (btnRegionForest != null) btnRegionForest.onClick.AddListener(() => OpenRegion("Capital Forest"));
            if (btnRegionTree != null) btnRegionTree.onClick.AddListener(() => OpenRegion("Magic Tree"));
            if (btnRegionDesert != null) btnRegionDesert.onClick.AddListener(() => OpenRegion("Desert Ruins"));
            if (btnRegionVolcano != null) btnRegionVolcano.onClick.AddListener(() => OpenRegion("Volcanic Lair"));

            if (btnLobby == null)
            {
                // Tự động sinh nút Quay Về ngầm nếu Editor quên gắn
                GameObject backObj = new GameObject("Runtime_BackButton");
                backObj.transform.SetParent(this.transform, false);
                backObj.transform.SetAsLastSibling(); // Nổi lên trên cùng
                
                RectTransform rt = backObj.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1); rt.sizeDelta = new Vector2(300, 100);
                rt.anchoredPosition = new Vector2(40, -40);

                Image img = backObj.AddComponent<Image>();
                img.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

                btnLobby = backObj.AddComponent<Button>();

                GameObject txtObj = new GameObject("Text");
                txtObj.transform.SetParent(backObj.transform, false);
                var txt = txtObj.AddComponent<TMPro.TextMeshProUGUI>();
                txt.text = "< CLOSE MAP"; 
                
                if (TMPro.TMP_Settings.defaultFontAsset != null)
                    txt.font = TMPro.TMP_Settings.defaultFontAsset;
                
                txt.fontSize = 40;
                txt.alignment = TMPro.TextAlignmentOptions.Center;
                txt.color = new Color(0.9f, 0.8f, 0.2f); // Chữ vàng nhạt cho sang
                
                RectTransform txtRt = txt.rectTransform;
                txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
                txtRt.sizeDelta = Vector2.zero; txtRt.anchoredPosition = Vector2.zero;
            }

            // [FIX CRITICAL BUG] Hủy gọi UIManager để tránh xung đột lịch sử Panel!
            // Khi user ấn nút ở thanh Navbar để mở map, UIManager không hề ghi nhận lịch sử.
            // Do đó khi dập map, ta phải dập trực tiếp Active của GameObject để thoát ra MainScreen.
            if (btnLobby != null) 
            {
                btnLobby.onClick.AddListener(() => {
                    // Xử lý kẹt: Người dùng ấn xuyên qua POI_InfoPanel xuống nút này!
                    var uim = GameManager.Instance.UIManager;
                    var poiPanel = uim.GetPanel<POI_InfoPanel>(UIPanelType.POI_Info);
                    
                    // Nếu POI_InfoPanel đang mở mà ấn trúng nút này thì tắt POI
                    if (poiPanel != null && poiPanel.gameObject.activeInHierarchy)
                    {
                        uim.GoBack();
                        return; // Ngừng, chỉ tắt POI_Info, không tắt WorldMap
                    }

                    // Trả lại State cho UIManager nhưng KHÔNG ĐƯỢC dùng ShowPanel(MainScreen) 
                    // vì UIManager sẽ đẩy MainScreen xuống index 1, khiến nó bị chìm dưới các panel rác sinh ra lỗi chặn ngót (Raycast block).
                    // Chỉ cần gọi HidePanel cho WorldMap, nó sẽ dập Active + xóa currentPanel an toàn tuyệt đối.
                    uim.GoBack();
                });
            }
            // Bind Close Popup Button
            if (regionDetailPopup != null)
            {
                Button[] popupBtns = regionDetailPopup.GetComponentsInChildren<Button>(true);
                foreach(var b in popupBtns) {
                    if(b.gameObject.name == "CloseButton") {
                        b.onClick.AddListener(() => regionDetailPopup.SetActive(false));
                        break;
                    }
                }
                
                // Hide popup by default on Awake
                regionDetailPopup.SetActive(false);
            }

            // --- Sinh động UI Tracker Đội Viễn Chinh ---
            GenerateExpeditionTrackerUI();
        }

        private void GenerateExpeditionTrackerUI()
        {
            _expeditionTrackerPanel = new GameObject("Runtime_ExpeditionTracker");
            _expeditionTrackerPanel.transform.SetParent(this.transform, false);
            _expeditionTrackerPanel.transform.SetAsLastSibling();
            
            RectTransform rt = _expeditionTrackerPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1); rt.anchorMax = new Vector2(1, 1); // Góc trên bên phải
            rt.pivot = new Vector2(1, 1);
            rt.sizeDelta = new Vector2(400, 150);
            rt.anchoredPosition = new Vector2(-20, -20); // Cách lề phải và trên 20 unit

            Image img = _expeditionTrackerPanel.AddComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.15f, 0.9f); // Nền xám đen mờ

            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(_expeditionTrackerPanel.transform, false);
            _expeditionTrackerText = txtObj.AddComponent<TMPro.TextMeshProUGUI>();
            _expeditionTrackerText.text = "Không có đội viễn chinh";
            
            if (TMPro.TMP_Settings.defaultFontAsset != null)
                _expeditionTrackerText.font = TMPro.TMP_Settings.defaultFontAsset;
                
            _expeditionTrackerText.fontSize = 24;
            _expeditionTrackerText.alignment = TMPro.TextAlignmentOptions.TopLeft;
            _expeditionTrackerText.color = Color.white;
            _expeditionTrackerText.enableWordWrapping = true;
            
            RectTransform txtRt = _expeditionTrackerText.rectTransform;
            txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = new Vector2(-20, -20); // Padding 10
            txtRt.anchoredPosition = Vector2.zero;

            _expeditionTrackerPanel.SetActive(false);
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.ExpeditionManager == null || DataManager.Instance == null || DataManager.Instance.Player == null) return;

            var activeExpeditions = DataManager.Instance.Player.ActiveExpeditions;
            if (activeExpeditions == null || activeExpeditions.Count == 0)
            {
                if (_expeditionTrackerPanel != null && _expeditionTrackerPanel.activeSelf)
                    _expeditionTrackerPanel.SetActive(false);
                return;
            }

            if (_expeditionTrackerPanel != null && !_expeditionTrackerPanel.activeSelf)
                _expeditionTrackerPanel.SetActive(true);

            if (_expeditionTrackerText != null)
            {
                long currentTime = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("<color=#FFD700>ĐỘI VIỄN CHINH</color>");

                foreach (var exp in activeExpeditions)
                {
                    string stateStr = "";
                    if (exp.currentState == ExpeditionState.Traveling) stateStr = "Đang đi";
                    else if (exp.currentState == ExpeditionState.Exploring) stateStr = "Đang đánh";
                    else if (exp.currentState == ExpeditionState.Returning) stateStr = "<color=#FF5555>Đang về</color>";

                    long timeLeftMs = exp.stateEndTimestamp - currentTime;
                    if (timeLeftMs < 0) timeLeftMs = 0;
                    System.TimeSpan ts = System.TimeSpan.FromMilliseconds(timeLeftMs);
                    string timeStr = string.Format("{0:D2}:{1:D2}", ts.Minutes, ts.Seconds);

                    // POI Name (lấy từ POIData hoặc báo cáo)
                    string poiName = "Khu vực";
                    if (exp.preCalculatedReport != null && !string.IsNullOrEmpty(exp.preCalculatedReport.poiName))
                        poiName = exp.preCalculatedReport.poiName;

                    sb.AppendLine($"- {poiName}: {stateStr} ({timeStr})");
                }

                _expeditionTrackerText.text = sb.ToString();
                
                // Tự động điều chỉnh chiều cao panel theo số lượng dòng
                RectTransform rt = _expeditionTrackerPanel.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, 50 + (activeExpeditions.Count * 35));
            }
        }
        
        private void OnEnable()
        {
            // Ensure the popup is hidden every time the World Map is opened
            if (regionDetailPopup != null)
            {
                regionDetailPopup.SetActive(false);
            }
        }

        private void OpenRegion(string regionName)
        {
            Debug.Log($"[WorldMap] Mở khu vực: {regionName}");
            var infoPanel = GameManager.Instance.UIManager.GetPanel<POI_InfoPanel>(UIPanelType.POI_Info);
            if (infoPanel != null)
            {
                POIData fakePOI = new POIData {
                    poiId = regionName,
                    poiName = regionName,
                    type = POIType.Dungeon,
                    difficultyLevel = 1,
                    monsterIDs = new System.Collections.Generic.List<string> { "Orc Warrior", "Slime" }
                };
                
                System.Collections.Generic.List<string> selectedSquadIDs = null;

                GameManager.Instance.UIManager.ShowPanel(UIPanelType.POI_Info, false);
                infoPanel.Show(fakePOI, 
                () => {
                    // KHI ẤN "Tiến Vào" TỪ NodeDetailPopup
                    // Bây giờ POI_InfoPanel đã TỰ ĐỘNG MÔ PHỎNG TRẬN CHIẾN ngay lập tức!
                    // Nên callback này chỉ dùng để cập nhật WorldMap hoặc UI nếu cần.
                    // Không cần StartExpedition nữa vì trận đấu đã được giải quyết Live trong RuntimeMapContainer.
                }, 
                () => {
                    // KHI CHỌN ĐỘ KHÓ XONG -> Hiện Squad Selection
                    var squadPanel = GameManager.Instance.UIManager.GetPanel<SquadSelectionPanel>(UIPanelType.SquadSelection);
                    if (squadPanel != null)
                    {
                        var availableHeroes = DataManager.Instance.AllHeroes;
                        GameManager.Instance.UIManager.ShowPanel(UIPanelType.SquadSelection, true); // this hides POI_Info
                        squadPanel.Show($"Viễn chinh: {regionName}", availableHeroes, 5, (selectedIDs, difficulty) => {
                            fakePOI.difficultyLevel = difficulty;
                            selectedSquadIDs = selectedIDs;
                            
                            // Yêu cầu POI_InfoPanel render lại cái bản đồ rẽ nhánh và truyền đội hình vào để đánh trận Live
                            GameManager.Instance.UIManager.ShowPanel(UIPanelType.POI_Info, true);
                            infoPanel.GenerateAndShowProceduralMap(selectedSquadIDs);
                        }, Profession.None, fakePOI.difficultyLevel);
                    }
                });
            }
            else
            {
                Debug.LogError("Lỗi: Không tìm thấy Prefab POI_InfoPanel trong danh sách UI Manager!");
            }
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (mapContainer == null) return;
            
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
            if (mapContainer == null) return;
            
            // Xoay xở giữ map không trượt bay khỏi màn hình khi zoom
            Vector2 pos = mapContainer.anchoredPosition;
            float scale = mapContainer.localScale.x;
            
            // Kích thước chuẩn hiện tại là tham khảo 1080x1920
            float limitX = Mathf.Max(0, (mapContainer.rect.width * scale - 1080) / 2);
            float limitY = Mathf.Max(0, (mapContainer.rect.height * scale - 1920) / 2);

            pos.x = Mathf.Clamp(pos.x, -limitX, limitX);
            pos.y = Mathf.Clamp(pos.y, -limitY, limitY);

            mapContainer.anchoredPosition = pos;
        }
    }
}

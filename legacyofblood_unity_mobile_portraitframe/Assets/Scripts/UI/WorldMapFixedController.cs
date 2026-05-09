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

                    // Theo kế hoạch HANDOFF: sử dụng GoBack() thay vì HidePanel
                    // để đảm bảo history stack được clear và quay về MainScreen.
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
                            
                            // Ẩn POI_InfoPanel và Mở RegionDetailPopup
                            GameManager.Instance.UIManager.HidePanel(UIPanelType.SquadSelection);
                            GameManager.Instance.UIManager.HidePanel(UIPanelType.POI_Info); // Thay vì ClosePanel
                            
                            if (regionDetailPopup != null)
                            {
                                var detailController = regionDetailPopup.GetComponent<LegendOfBlood.RegionDetailPopupController>();
                                if (detailController == null) detailController = regionDetailPopup.AddComponent<LegendOfBlood.RegionDetailPopupController>();
                                
                                detailController.SetupAndShow(fakePOI, (ProceduralDifficulty)difficulty, selectedSquadIDs);
                            }
                        }, Profession.None, fakePOI.difficultyLevel);
                    }
                });
            }
            else
            {
                Debug.LogError("Lỗi: Không tìm thấy Prefab SquadSelectionPanel trong danh sách UI Manager!");
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

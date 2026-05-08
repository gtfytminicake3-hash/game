using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

namespace LegendOfBlood
{
    public class POI_InfoPanel : UIPanel, ILocalizable
    {
        [Header("Procedural Map References")]
        [SerializeField] private TextMeshProUGUI poiNameText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Transform mapContentContainer;

        [Header("Popups")]
        [SerializeField] private LegendOfBlood.DifficultySelectionPopup difficultyPopup;
        [SerializeField] private LegendOfBlood.NodeDetailPopup nodeDetailPopup;

        private POIData _currentPoiData;
        private Action _onExploreCallback;
        private List<string> _currentSquadIDs;
        private ProceduralDifficulty _currentDifficulty;
        
        private SubStageNode _currentlySelectedNode;
        private Dictionary<SubStageNode, GameObject> _nodeUIObjects = new Dictionary<SubStageNode, GameObject>();

        private static Dictionary<string, ShapeDrivenMapData> _activeMaps = new Dictionary<string, ShapeDrivenMapData>();
        private ShapeDrivenMapData _currentMapData;

        private void Awake()
        {
            PanelType = UIPanelType.POI_Info;
            
            Canvas mainCanvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (mainCanvas != null && (transform.parent == null || transform.parent.GetComponentInParent<Canvas>() == null))
            {
                transform.SetParent(mainCanvas.transform, false);
                transform.SetAsLastSibling();
            }

            if (mapContentContainer == null)
            {
                Transform innerMap = transform.Find("MainWindow/ScrollFrame/Viewport/InnerMap");
                if (innerMap != null) mapContentContainer = innerMap;
            }
        }

        protected override void Start()
        {
            base.Start();
            if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
            
            Button bgBtn = GetComponent<Button>();
            if (bgBtn != null) bgBtn.onClick.AddListener(ClosePanel);
        }

        private Action _onDifficultySelectedCallback;

        public void Show(POIData poiData, Action onExplore, Action onDifficultySelected = null)
        {
            _currentPoiData = poiData;
            _onExploreCallback = onExplore;
            _onDifficultySelectedCallback = onDifficultySelected ?? onExplore;

            // Generate a fallback ID if null to prevent Dictionary from throwing ArgumentNullException
            if (string.IsNullOrEmpty(_currentPoiData.poiId))
            {
                _currentPoiData.poiId = string.IsNullOrEmpty(_currentPoiData.poiName) ? System.Guid.NewGuid().ToString() : _currentPoiData.poiName;
            }

            UpdateLocalizedText();
            gameObject.SetActive(true);

            // Clean old map rendering immediately to prevent overlaps
            if (mapContentContainer != null)
            {
                foreach (Transform child in mapContentContainer) Destroy(child.gameObject);
                
                Button bgBtn = GetComponent<Button>();
                if (bgBtn != null) 
                {
                    bgBtn.onClick.RemoveAllListeners();
                    bgBtn.onClick.AddListener(ClosePanel);
                }
            }

            if (_activeMaps.ContainsKey(_currentPoiData.poiId))
            {
                _currentMapData = _activeMaps[_currentPoiData.poiId];
                _currentDifficulty = _currentMapData.Difficulty;
                if (poiNameText != null) poiNameText.text = $"{_currentPoiData.poiName.ToUpper()} [{_currentDifficulty}]";
                DrawProceduralMap(mapContentContainer, _currentMapData);
            }
            else
            {
                ShowDifficultySelector();
            }

            EnsureCloseButtonIsVisible();
        }

        private void EnsureCloseButtonIsVisible()
        {
            if (closeButton == null)
            {
                GameObject btnObj = new GameObject("CloseButton_Runtime");
                btnObj.transform.SetParent(this.transform, false);
                btnObj.transform.SetAsLastSibling();

                RectTransform rt = btnObj.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                // Đặt vị trí an toàn xa góc để tránh tai thỏ (notch) trên điện thoại
                rt.anchoredPosition = new Vector2(50, -100); 
                rt.sizeDelta = new Vector2(250, 100);

                Image img = btnObj.AddComponent<Image>();
                img.color = new Color(0.8f, 0.2f, 0.2f, 0.9f);

                GameObject txtObj = new GameObject("Text");
                txtObj.transform.SetParent(btnObj.transform, false);
                TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
                tmp.text = "Quay Lại";
                tmp.color = Color.white;
                tmp.fontSize = 45;
                tmp.alignment = TextAlignmentOptions.Center;
                
                RectTransform txtRt = tmp.rectTransform;
                txtRt.anchorMin = Vector2.zero;
                txtRt.anchorMax = Vector2.one;
                txtRt.sizeDelta = Vector2.zero;
                txtRt.anchoredPosition = Vector2.zero;

                closeButton = btnObj.AddComponent<Button>();
                closeButton.onClick.AddListener(ClosePanel);
            }
            else
            {
                // Force bring to front and un-hide if hidden
                closeButton.gameObject.SetActive(true);
                closeButton.transform.SetAsLastSibling();
                
                if (closeButton.transform.parent != this.transform)
                {
                    closeButton.transform.SetParent(this.transform, true);
                }
            }
        }

        public void UpdateLocalizedText()
        {
            if (_currentPoiData == null) return;
            if (poiNameText != null) poiNameText.text = $"{_currentPoiData.poiName.ToUpper()}";
        }

        #region DIFFICULTY SELECTOR
        private void ShowDifficultySelector()
        {
            if (difficultyPopup == null) 
            {
                // Fallback: Tự động khởi tạo ngay tại Runtime nếu trong Inspector chưa gán
                CreateDifficultyPopupFallback();
            }
            
            difficultyPopup.Show(_currentPoiData.poiName, ProceedToMap);
        }

        private void CreateDifficultyPopupFallback()
        {
            // 1. Tắt các UI rác cũ có thể làm loạn màn hình
            foreach (Transform child in this.transform)
            {
                if (child.name != "RuntimeMapContainer" && !child.name.Contains("Popup") && child.name != "MainWindow")
                {
                    child.gameObject.SetActive(false);
                }
            }

            // 2. Tạo hình thức Popup
            GameObject diffObj = new GameObject("DifficultySelectionPopup_Runtime");
            diffObj.transform.SetParent(this.transform, false);
            diffObj.transform.SetAsLastSibling();
            RectTransform rt = diffObj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero; rt.anchoredPosition = Vector2.zero;

            Image bg = diffObj.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.95f);

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(diffObj.transform, false);
            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.fontSize = 60; titleTmp.alignment = TextAlignmentOptions.Center; titleTmp.color = new Color(0.9f, 0.8f, 0.3f);
            RectTransform titleRt = titleTmp.rectTransform; 
            titleRt.anchorMin = new Vector2(0.5f, 0.5f); titleRt.anchorMax = new Vector2(0.5f, 0.5f); 
            titleRt.sizeDelta = new Vector2(800, 150); titleRt.anchoredPosition = new Vector2(0, 500);

            // CPText
            GameObject cpObj = new GameObject("CPText");
            cpObj.transform.SetParent(diffObj.transform, false);
            TextMeshProUGUI cpTmp = cpObj.AddComponent<TextMeshProUGUI>();
            cpTmp.fontSize = 45; cpTmp.alignment = TextAlignmentOptions.Center; cpTmp.color = Color.white;
            RectTransform cpRt = cpTmp.rectTransform; 
            cpRt.anchorMin = new Vector2(0.5f, 0.5f); cpRt.anchorMax = new Vector2(0.5f, 0.5f); 
            cpRt.sizeDelta = new Vector2(800, 100); cpRt.anchoredPosition = new Vector2(0, 350);

            // Buttons (Cách đều tuyệt đối)
            Button btnNorm =   CreateRuntimeButton(diffObj.transform, "Btn_Normal", "Normal (Rec: 0 CP)", new Color(0.5f, 0.5f, 0.5f), 150);
            Button btnHard =   CreateRuntimeButton(diffObj.transform, "Btn_Hard", "Hard (Rec: 5,000 CP)", new Color(0.8f, 0.5f, 0f) * 0.7f, 0);
            Button btnHell =   CreateRuntimeButton(diffObj.transform, "Btn_Hell", "Hell (Rec: 15,000 CP)", new Color(0.9f, 0.1f, 0.1f) * 0.7f, -150);
            Button btnNight =  CreateRuntimeButton(diffObj.transform, "Btn_Nightmare", "Nightmare (Rec: 30,000 CP)", new Color(0.5f, 0f, 0.5f) * 0.7f, -300);

            difficultyPopup = diffObj.AddComponent<LegendOfBlood.DifficultySelectionPopup>();
            difficultyPopup.titleText = titleTmp;
            difficultyPopup.currentCPText = cpTmp;
            difficultyPopup.btnNormal = btnNorm;
            difficultyPopup.btnHard = btnHard;
            difficultyPopup.btnHell = btnHell;
            difficultyPopup.btnNightmare = btnNight;
        }

        private Button CreateRuntimeButton(Transform p, string n, string t, Color c, float yPos)
        {
            GameObject bObj = new GameObject(n);
            bObj.transform.SetParent(p, false);
            RectTransform rt = bObj.AddComponent<RectTransform>();
            // Căn chính giữa, thay đổi vị trí tuyệt đối (Pixels)
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(600, 120);
            rt.anchoredPosition = new Vector2(0, yPos);
            
            Image img = bObj.AddComponent<Image>(); img.color = c;
            
            GameObject tObj = new GameObject("Text");
            tObj.transform.SetParent(bObj.transform, false);
            TextMeshProUGUI tmp = tObj.AddComponent<TextMeshProUGUI>();
            tmp.text = t; tmp.fontSize = 40; tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white;
            tmp.rectTransform.anchorMin = Vector2.zero; tmp.rectTransform.anchorMax = Vector2.one; tmp.rectTransform.sizeDelta = Vector2.zero;

            return bObj.AddComponent<Button>();
        }

        private void ProceedToMap(ProceduralDifficulty difficulty)
        {
            _currentDifficulty = difficulty;
            _currentPoiData.difficultyLevel = (int)difficulty; // Normal=1, Hard=2, Hell=3, Nightmare=4
            
            // Ẩn bảng chọn độ khó
            if (difficultyPopup != null) difficultyPopup.gameObject.SetActive(false);
            
            // Gọi callback để WorldMapFixedController hiển thị SquadSelection (KHÔNG ClosePanel)
            _onDifficultySelectedCallback?.Invoke();
        }

        public void GenerateAndShowProceduralMap(System.Collections.Generic.List<string> squadIDs = null)
        {
            if (squadIDs != null)
            {
                _currentSquadIDs = squadIDs;
            }
            
            if (_currentPoiData == null) return;
            _currentMapData = new ProceduralSubStageGenerator().GenerateMap(_currentDifficulty);
            if (!_activeMaps.ContainsKey(_currentPoiData.poiId))
            {
                _activeMaps.Add(_currentPoiData.poiId, _currentMapData);
            }
            else
            {
                _activeMaps[_currentPoiData.poiId] = _currentMapData;
            }
            
            if (poiNameText != null) poiNameText.text = $"{_currentPoiData.poiName.ToUpper()} [{_currentDifficulty}]";
            DrawProceduralMap(mapContentContainer, _currentMapData);
        }
        #endregion

        #region PROCEDURAL MAP GENERATION
        private void DrawProceduralMap(Transform parent, ShapeDrivenMapData mapData)
        {
            if (parent == null)
            {
                // Fallback nếu người dùng chưa gán mapContentContainer hoặc không tồn tại path
                GameObject fbObj = new GameObject("RuntimeMapContainer");
                fbObj.transform.SetParent(this.transform, false);
                fbObj.transform.SetSiblingIndex(0); // Để dưới Popup
                RectTransform fbRt = fbObj.AddComponent<RectTransform>();
                fbRt.anchorMin = Vector2.zero; fbRt.anchorMax = Vector2.one; 
                fbRt.sizeDelta = Vector2.zero; fbRt.anchoredPosition = Vector2.zero;
                
                Image bg = fbObj.AddComponent<Image>();
                bg.color = new Color(0.1f, 0.1f, 0.15f, 1f); // Dark background
                mapContentContainer = fbObj.transform;
                parent = mapContentContainer;
            }

            _nodeUIObjects.Clear();
            foreach (Transform child in parent) Destroy(child.gameObject);

            var nodes = mapData.Nodes;
            var segments = mapData.DrawnSegments;

            float floorStepY = 220f; 
            float columnSpacing = 200f; 
            float startY = 150f; 
            float mapHeight = (15 * floorStepY) + 300f; 
            
            RectTransform contentRt = parent.GetComponent<RectTransform>();
            if (contentRt != null) 
            {
                contentRt.anchorMin = new Vector2(0.5f, 0f);
                contentRt.anchorMax = new Vector2(0.5f, 0f);
                contentRt.pivot = new Vector2(0.5f, 0f); 
                contentRt.sizeDelta = new Vector2(1000f, mapHeight); 
                contentRt.anchoredPosition = Vector2.zero;

                if (contentRt.parent != null)
                {
                    var viewport = contentRt.parent.gameObject;
                    if (viewport.GetComponent<UnityEngine.UI.RectMask2D>() == null)
                        viewport.AddComponent<UnityEngine.UI.RectMask2D>();
                    
                    if (contentRt.parent.parent != null)
                    {
                        var scrollFrame = contentRt.parent.parent.gameObject;
                        var scrollRect = scrollFrame.GetComponent<ScrollRect>();
                        if (scrollRect == null) scrollRect = scrollFrame.AddComponent<ScrollRect>();
                        
                        scrollRect.content = contentRt;
                        scrollRect.viewport = viewport.GetComponent<RectTransform>();
                        scrollRect.horizontal = false;
                        scrollRect.vertical = true;
                        scrollRect.movementType = ScrollRect.MovementType.Elastic;
                        scrollRect.scrollSensitivity = 50f;
                    }
                }
            }

            foreach(var seg in segments)
            {
                Vector2 posFrom = new Vector2((seg.From.x - 2) * columnSpacing, startY + seg.From.y * floorStepY);
                Vector2 posTo = new Vector2((seg.To.x - 2) * columnSpacing, startY + seg.To.y * floorStepY);
                var nFrom = nodes.Find(x => x.Floor == seg.From.y && x.Slot == seg.From.x);
                var nTo = nodes.Find(x => x.Floor == seg.To.y && x.Slot == seg.To.x);
                bool isActiveLine = nTo != null && nTo.Status != NodeStatus.Locked;
                if (nFrom != null && nFrom.Status == NodeStatus.Locked) isActiveLine = false;
                
                DrawSegmentLine(parent, posFrom, posTo, isActiveLine);
            }

            foreach (var node in nodes)
            {
                GameObject nObj = new GameObject($"Node_{node.Id}_{node.Type}");
                RectTransform rt = nObj.AddComponent<RectTransform>();
                rt.SetParent(parent, false);
                rt.anchorMin = new Vector2(0.5f, 0f); 
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                
                rt.sizeDelta = node.Type == SubStageNodeType.Boss ? new Vector2(160, 160) : new Vector2(120, 120);

                float posX = (node.Slot - 2) * columnSpacing; 
                float posY = startY + floorStepY * node.Floor;
                rt.anchoredPosition = new Vector2(posX, posY);

                Image img = nObj.AddComponent<Image>();
                img.raycastTarget = true; 
                
                Button btn = nObj.AddComponent<Button>();
                btn.onClick.AddListener(() => OnStageNodeClicked(node));

                UpdateNodeVisual(node, img);

                GameObject textObj = new GameObject("Label");
                textObj.transform.SetParent(nObj.transform, false);
                TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
                tmp.text = $"{node.Type}";
                tmp.fontSize = 32;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
                tmp.raycastTarget = false; 

                // Bóng đen cho chữ
                var outline = textObj.AddComponent<UnityEngine.UI.Outline>();
                outline.effectColor = Color.black; 
                outline.effectDistance = new Vector2(2, -2);

                RectTransform labelRt = tmp.rectTransform;
                labelRt.anchorMin = new Vector2(0.5f, 0f); labelRt.anchorMax = new Vector2(0.5f, 0f);
                labelRt.anchoredPosition = new Vector2(0, -85); 
                labelRt.sizeDelta = new Vector2(250, 50);

                _nodeUIObjects[node] = nObj;
            }

            StartCoroutine(ScrollToBottom(parent));
        }

        private void UpdateNodeVisual(SubStageNode node, Image img)
        {
            switch (node.Type)
            {
                case SubStageNodeType.Start: img.color = Color.cyan; break;
                case SubStageNodeType.Combat: img.color = new Color(0.8f, 0.3f, 0.3f); break; 
                case SubStageNodeType.Elite: img.color = new Color(0.9f, 0.1f, 0.1f); break;  
                case SubStageNodeType.Event: img.color = Color.magenta; break;
                case SubStageNodeType.Shop: img.color = Color.yellow; break;
                case SubStageNodeType.Reward: img.color = Color.green; break;
                case SubStageNodeType.Boss: img.color = new Color(0.5f, 0f, 0.5f); break;     
            }
            
            Button btn = img.GetComponent<Button>();
            if (btn != null) btn.interactable = true;

            if (node.Status == NodeStatus.Cleared) 
            {
                img.color = Color.gray;
            }
            else if (node.Status == NodeStatus.InProgress || node.Status == NodeStatus.Available)
            {
                if (img.gameObject.GetComponent<PulseAnimation>() == null)
                    img.gameObject.AddComponent<PulseAnimation>();
            }
            else if (node.Status == NodeStatus.Locked)
            {
                img.color = new Color(img.color.r * 0.3f, img.color.g * 0.3f, img.color.b * 0.3f, 1f);
                // Vẫn cho phép bấm vào để xem thông tin, chỉ khóa nút 'Tiến Vào' ở bên trong Popup
                if (btn != null) btn.interactable = true;
            }
        }

        private void DrawSegmentLine(Transform parent, Vector2 from, Vector2 to, bool isActiveLine)
        {
            GameObject lineObj = new GameObject("Line_Segment");
            lineObj.transform.SetParent(parent, false);
            lineObj.transform.SetAsFirstSibling(); 

            RectTransform rt = lineObj.AddComponent<RectTransform>();
            Image img = lineObj.AddComponent<Image>();
            
            img.color = isActiveLine ? new Color(0.8f, 0.6f, 0.2f, 1f) : new Color(0.3f, 0.3f, 0.3f, 0.5f); 
            img.raycastTarget = false; 

            Vector2 dir = (to - from);
            float distance = dir.magnitude;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            rt.anchorMin = new Vector2(0.5f, 0f); rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0f, 0.5f); 
            rt.anchoredPosition = from;
            rt.sizeDelta = new Vector2(distance, 6f); 
            rt.localRotation = Quaternion.Euler(0, 0, angle);
        }

        private IEnumerator ScrollToBottom(Transform parent)
        {
            yield return new WaitForEndOfFrame();
            var scrollRect = parent.GetComponentInParent<ScrollRect>();
            if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;
        }
        #endregion

        #region NODE INTERACTION
        private void OnStageNodeClicked(SubStageNode node)
        {
            _currentlySelectedNode = node;
            ShowNodeDetailPopup(node);
        }

        private void ShowNodeDetailPopup(SubStageNode node)
        {
            if (nodeDetailPopup == null)
            {
                CreateNodeDetailPopupFallback();
            }

            nodeDetailPopup.Show(node, 
            () => // ACTION: TIẾN VÀO
            {
                if (node.Status == NodeStatus.Available)
                {
                    if (_currentSquadIDs == null || _currentSquadIDs.Count == 0)
                    {
                        LegendOfBlood.ToastNotificationManager.Show($"Lỗi: Không tìm thấy đội hình!", 2f);
                        return;
                    }
                    
                    var heroSquad = new System.Collections.Generic.List<LegendOfBlood.HeroData>();
                    foreach (var id in _currentSquadIDs)
                    {
                        var h = LegendOfBlood.DataManager.Instance.GetHeroByID(id);
                        if (h != null) heroSquad.Add(h.Clone());
                    }

                    if (LegendOfBlood.GameManager.Instance.CombatSystem != null)
                    {
                        LegendOfBlood.Combat.CombatResult combatResult;
                        
                        if (node.Type == SubStageNodeType.Boss)
                        {
                            string bossId = node.ExpectedMonsters.Count > 0 ? node.ExpectedMonsters[0] : "BOSS_01";
                            combatResult = LegendOfBlood.GameManager.Instance.CombatSystem.SimulateBoss(heroSquad, bossId);
                        }
                        else
                        {
                            combatResult = LegendOfBlood.GameManager.Instance.CombatSystem.Simulate(heroSquad, node.ExpectedMonsters, (int)_currentDifficulty);
                        }

                        if (combatResult.DidPlayerWin)
                        {
                            var expMgr = LegendOfBlood.GameManager.Instance.ExpeditionManager;
                            if (expMgr != null)
                            {
                                LegendOfBlood.LootData loot;
                                int expGained = 0;
                                if (node.Type == SubStageNodeType.Boss)
                                {
                                    string bossId = node.ExpectedMonsters.Count > 0 ? node.ExpectedMonsters[0] : "BOSS_01";
                                    loot = expMgr.CalculateBossLoot(bossId, true);
                                    expGained = 500;
                                }
                                else
                                {
                                    // Truyền fake POI có chứa difficulty hiện tại để scale phần thưởng
                                    var fakePOI = _currentPoiData; 
                                    fakePOI.difficultyLevel = (int)_currentDifficulty;
                                    loot = expMgr.CalculateLoot(fakePOI, true);
                                    expGained = expMgr.CalculateExperience(fakePOI, true);
                                }

                                var report = new LegendOfBlood.ExpeditionReport {
                                    poiId = _currentPoiData.poiId,
                                    combatResult = combatResult,
                                    loot = loot,
                                    experienceGained = expGained
                                };

                                LegendOfBlood.DataManager.Instance.Player.UnclaimedReports.Add(report);
                                expMgr.ClaimReport(report, false);
                                
                                LegendOfBlood.ToastNotificationManager.Show($"Chiến thắng! Nhận được {loot.gold} Vàng và tài nguyên.", 3f);
                            }
                            else
                            {
                                LegendOfBlood.ToastNotificationManager.Show($"Chiến thắng! Đã hạ gục quái vật.", 2f);
                            }

                            ExecuteNodeAction(node);
                        }
                        else
                        {
                            LegendOfBlood.ToastNotificationManager.Show($"Thất bại! Đội hình đã bị quét sạch.", 3f);
                            ExecuteNodeLose(node);
                        }
                    }
                }
            },
            () => // LỖI / THUA TRẬN: LÙI 1 BƯỚC
            {
                ExecuteNodeLose(node);
                _onExploreCallback?.Invoke();
            });
        }

        private void ExecuteNodeAction(SubStageNode node)
        {
            node.Status = NodeStatus.Cleared;
            _currentMapData.CurrentNodeId = node.Id;
            LegendOfBlood.ToastNotificationManager.Show($"Đã vượt qua {node.Type} - Tầng {node.Floor}!", 3f);
            
            if (node.Type == SubStageNodeType.Boss)
            {
                _activeMaps.Remove(_currentPoiData.poiId);
                LegendOfBlood.ToastNotificationManager.Show($"Chúc mừng! Ải đã hoàn thành!", 5f);
                ClosePanel();
                return;
            }

            // 1. NGHIÊM NGẶT: Một tầng chỉ được chọn 1 node. Khóa tất cả các node khác lân cận trên cùng tầng!
            foreach (var n in _currentMapData.Nodes)
            {
                if (n.Floor == node.Floor && n.Id != node.Id)
                {
                    n.Status = NodeStatus.Locked;
                }
            }

            // 2. Mở khoá các node nối tiếp ở tầng tiếp theo
            foreach (var nextId in node.OutgoingEdges)
            {
                var nNext = _currentMapData.Nodes.Find(x => x.Id == nextId);
                if (nNext != null && nNext.Status != NodeStatus.Cleared) 
                    nNext.Status = NodeStatus.Available;
            }

            // [Lưu Ý]: Tính năng "Lùi lại 1 bước khi thua" sẽ được kích hoạt bởi Combat System 
            // bằng cách gọi một hàm Backtrack() (chưa cài đặt trong UI này vì hiện đang ấn là win luôn).

            // Cập nhật lại giao diện ngay khi có thay đổi trạng thái
            DrawProceduralMap(mapContentContainer, _currentMapData);
        }

        // TÍNH NĂNG ĐẶC BIỆT: THUA THÌ LÙI 1 BƯỚC
        private void ExecuteNodeLose(SubStageNode failedNode)
        {
            LegendOfBlood.ToastNotificationManager.Show($"Thất bại tại {failedNode.Type}! Đội hình bị đẩy lùi 1 tầng!", 3f);

            // 1. Nếu đang ở Tầng 1 (nghĩa là Node vừa thua có Floor == 1), thì reset tịt ngòi 
            if (failedNode.Floor == 1)
            {
                foreach(var n in _currentMapData.Nodes)
                {
                    if (n.Floor == 1) n.Status = NodeStatus.Available;
                    else n.Status = NodeStatus.Locked;
                }
                DrawProceduralMap(mapContentContainer, _currentMapData);
                return;
            }

            // 2. Tìm cái Node ở Tầng dưới (Floor - 1) mà VỪA MỚI được Clear xong!
            // Node này chính là nơi ta đứng trước khi trèo lên. Nếu có nhiều cái báo Clear, thì ưu tiên cái nối trực tiếp (chỉ lấy 1 cái)
            var prevClearedNode = _currentMapData.Nodes.Find(x => x.Floor == failedNode.Floor - 1 && x.Status == NodeStatus.Cleared && x.OutgoingEdges.Contains(failedNode.Id));
            
            // Lùi thêm 1 trường hợp nếu bị kẹt (phòng hờ)
            if (prevClearedNode == null) 
            {
                prevClearedNode = _currentMapData.Nodes.Find(x => x.Floor == failedNode.Floor - 1 && x.Status == NodeStatus.Cleared);
            }

            if (prevClearedNode != null)
            {
                // Un-clear node đó -> Bắt đánh lại
                prevClearedNode.Status = NodeStatus.Available;

                // Các node anh em của prevClearedNode (nằm cùng tầng nhưng đang bị locked) -> cho phép chọn lại
                // Phải cho phép chọn lại toàn bộ để có thể "Đổi nhánh khác"
                foreach(var sibling in _currentMapData.Nodes)
                {
                    if (sibling.Floor == prevClearedNode.Floor)
                    {
                        // Nếu là Tầng 1, ai cũng dc Available. Nếu > 1, phải là node có Edge từ `Floor - 2` đã Clear.
                        if (sibling.Floor == 1)
                        {
                            sibling.Status = NodeStatus.Available;
                        }
                        else
                        {
                            bool hasClearedParent = false;
                            foreach(var parent in _currentMapData.Nodes)
                            {
                                if (parent.Floor == sibling.Floor - 1 && parent.Status == NodeStatus.Cleared && parent.OutgoingEdges.Contains(sibling.Id))
                                {
                                    hasClearedParent = true;
                                    break;
                                }
                            }
                            if (hasClearedParent) sibling.Status = NodeStatus.Available;
                        }
                    }
                }

                // VÀ TẤT CẢ con cháu của cái tầng mình vừa mất quyền (Floor) phải biến lại thành Locked
                foreach(var upperNode in _currentMapData.Nodes)
                {
                    if (upperNode.Floor == failedNode.Floor) upperNode.Status = NodeStatus.Locked;
                }
            }

            // Cập nhật giao diện Map
            DrawProceduralMap(mapContentContainer, _currentMapData);
        }

        private void CreateNodeDetailPopupFallback()
        {
            GameObject detObj = new GameObject("NodeDetailPopup_Runtime");
            detObj.transform.SetParent(this.transform, false);
            detObj.transform.SetAsLastSibling();
            RectTransform rt = detObj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero; rt.anchoredPosition = Vector2.zero;

            Image bg = detObj.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.90f);

            // Create inner box (Tuyệt đối hóa size: W=800, H=900)
            GameObject boxObj = new GameObject("InnerBox");
            boxObj.transform.SetParent(detObj.transform, false);
            RectTransform boxRt = boxObj.AddComponent<RectTransform>();
            boxRt.anchorMin = new Vector2(0.5f, 0.5f); boxRt.anchorMax = new Vector2(0.5f, 0.5f);
            boxRt.sizeDelta = new Vector2(800, 900); boxRt.anchoredPosition = Vector2.zero;
            Image boxBg = boxObj.AddComponent<Image>(); boxBg.color = new Color(0.1f, 0.1f, 0.15f, 0.98f);

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(boxObj.transform, false);
            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.fontSize = 50; titleTmp.alignment = TextAlignmentOptions.Center; titleTmp.color = Color.white;
            RectTransform titleRt = titleTmp.rectTransform; 
            titleRt.anchorMin = new Vector2(0.5f, 0.5f); titleRt.anchorMax = new Vector2(0.5f, 0.5f); 
            titleRt.sizeDelta = new Vector2(700, 100); titleRt.anchoredPosition = new Vector2(0, 350);

            // Description
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(boxObj.transform, false);
            TextMeshProUGUI descTmp = descObj.AddComponent<TextMeshProUGUI>();
            descTmp.fontSize = 36; descTmp.alignment = TextAlignmentOptions.TopLeft; descTmp.color = Color.yellow;
            RectTransform descRt = descTmp.rectTransform; 
            descRt.anchorMin = new Vector2(0.5f, 0.5f); descRt.anchorMax = new Vector2(0.5f, 0.5f); 
            descRt.sizeDelta = new Vector2(700, 250); descRt.anchoredPosition = new Vector2(0, 150);

            // Drops
            GameObject dropObj = new GameObject("Drops");
            dropObj.transform.SetParent(boxObj.transform, false);
            TextMeshProUGUI dropTmp = dropObj.AddComponent<TextMeshProUGUI>();
            dropTmp.fontSize = 32; dropTmp.alignment = TextAlignmentOptions.TopLeft; dropTmp.color = Color.green;
            RectTransform dropRt = dropTmp.rectTransform; 
            dropRt.anchorMin = new Vector2(0.5f, 0.5f); dropRt.anchorMax = new Vector2(0.5f, 0.5f); 
            dropRt.sizeDelta = new Vector2(700, 250); dropRt.anchoredPosition = new Vector2(0, -150);

            // Nút Tham Chiến
            Button btnGo = CreateRuntimeButton(boxObj.transform, "Btn_Combat", "Tiến Vào", new Color(0.8f, 0.2f, 0.2f), -280);

            // Nút Thua Trận (Lùi Bước)
            Button btnLose = CreateRuntimeButton(boxObj.transform, "Btn_Lose", "Đầu Hàng (Lùi Lại)", new Color(0.2f, 0.4f, 0.8f), -420);

            nodeDetailPopup = detObj.AddComponent<LegendOfBlood.NodeDetailPopup>();
            nodeDetailPopup.titleText = titleTmp;
            nodeDetailPopup.monstersText = descTmp;
            nodeDetailPopup.lootText = dropTmp;
            nodeDetailPopup.btnAction = btnGo;
            nodeDetailPopup.btnActionText = btnGo.GetComponentInChildren<TextMeshProUGUI>();
            nodeDetailPopup.btnLose = btnLose;
            nodeDetailPopup.btnLoseText = btnLose.GetComponentInChildren<TextMeshProUGUI>();
        }
        #endregion

        private void ClosePanel()
        {
            if (difficultyPopup != null) difficultyPopup.gameObject.SetActive(false);
            if (nodeDetailPopup != null) nodeDetailPopup.gameObject.SetActive(false);
            GameManager.Instance.UIManager.GoBack();
        }
    }
}

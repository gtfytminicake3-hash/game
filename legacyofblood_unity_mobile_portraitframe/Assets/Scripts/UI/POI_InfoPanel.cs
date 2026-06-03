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
        private static Dictionary<string, List<string>> _activeSquads = new Dictionary<string, List<string>>();
        private ShapeDrivenMapData _currentMapData;

        private void Awake()
        {
            PanelType = UIPanelType.POI_Info;
            AutoHook();
            
            Canvas mainCanvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (mainCanvas != null && (transform.parent == null || transform.parent.GetComponentInParent<Canvas>() == null))
            {
                transform.SetParent(mainCanvas.transform, false);
                transform.SetAsLastSibling();
            }
        }

        private void AutoHook()
        {
            if (poiNameText == null) poiNameText = transform.Find("poiNameText")?.GetComponent<TextMeshProUGUI>() ?? transform.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(t => t.name.Contains("Name") || t.name.Contains("Title"));
            if (closeButton == null) closeButton = transform.Find("button/close")?.GetComponent<Button>() ?? transform.Find("Btn_Close")?.GetComponent<Button>() ?? transform.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name.Contains("Close") || b.name.Contains("Back"));
            
            if (mapContentContainer == null) 
            {
                mapContentContainer = transform.Find("MapContent") ?? transform.Find("Content") ?? transform.Find("Scroll View/Viewport/Content");
                if (mapContentContainer != null) Debug.Log($"[POI_InfoPanel] Auto-hooked mapContentContainer: {mapContentContainer.name}");
                else Debug.LogWarning("[POI_InfoPanel] mapContentContainer is still null after AutoHook. It will be created at runtime in DrawProceduralMap.");
            }

            if (difficultyPopup == null) difficultyPopup = GetComponentInChildren<LegendOfBlood.DifficultySelectionPopup>(true);
            if (nodeDetailPopup == null) nodeDetailPopup = GetComponentInChildren<LegendOfBlood.NodeDetailPopup>(true);
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

            if (string.IsNullOrEmpty(_currentPoiData.poiId))
            {
                _currentPoiData.poiId = string.IsNullOrEmpty(_currentPoiData.poiName) ? System.Guid.NewGuid().ToString() : _currentPoiData.poiName;
            }

            UpdateLocalizedText();
            gameObject.SetActive(true);

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
                
                // RESTORE SQUAD DATA: Fix for "Confirm" (Tiến Vào) button not working when re-opening map
                if (_activeSquads.ContainsKey(_currentPoiData.poiId))
                {
                    _currentSquadIDs = _activeSquads[_currentPoiData.poiId];
                }

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
                CreateDifficultyPopupFallback();
            }
            
            difficultyPopup.Show(_currentPoiData.poiName, ProceedToMap);
        }

        private void CreateDifficultyPopupFallback()
        {
            foreach (Transform child in this.transform)
            {
                if (child.name != "RuntimeMapContainer" && !child.name.Contains("Popup") && child.name != "MainWindow")
                {
                    child.gameObject.SetActive(false);
                }
            }

            GameObject diffObj = new GameObject("DifficultySelectionPopup_Runtime");
            diffObj.transform.SetParent(this.transform, false);
            diffObj.transform.SetAsLastSibling();
            RectTransform rt = diffObj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero; rt.anchoredPosition = Vector2.zero;

            Image bg = diffObj.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.95f);

            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(diffObj.transform, false);
            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.fontSize = 60; titleTmp.alignment = TextAlignmentOptions.Center; titleTmp.color = new Color(0.9f, 0.8f, 0.3f);
            RectTransform titleRt = titleTmp.rectTransform; 
            titleRt.anchorMin = new Vector2(0.5f, 0.5f); titleRt.anchorMax = new Vector2(0.5f, 0.5f); 
            titleRt.sizeDelta = new Vector2(800, 150); titleRt.anchoredPosition = new Vector2(0, 500);

            GameObject cpObj = new GameObject("CPText");
            cpObj.transform.SetParent(diffObj.transform, false);
            TextMeshProUGUI cpTmp = cpObj.AddComponent<TextMeshProUGUI>();
            cpTmp.fontSize = 45; cpTmp.alignment = TextAlignmentOptions.Center; cpTmp.color = Color.white;
            RectTransform cpRt = cpTmp.rectTransform; 
            cpRt.anchorMin = new Vector2(0.5f, 0.5f); cpRt.anchorMax = new Vector2(0.5f, 0.5f); 
            cpRt.sizeDelta = new Vector2(800, 100); cpRt.anchoredPosition = new Vector2(0, 350);

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
            _currentPoiData.difficultyLevel = (int)difficulty; 
            
            if (difficultyPopup != null) difficultyPopup.gameObject.SetActive(false);
            _onDifficultySelectedCallback?.Invoke();
        }

        public void GenerateAndShowProceduralMap(System.Collections.Generic.List<string> squadIDs = null)
        {
            if (squadIDs != null) _currentSquadIDs = squadIDs;
            if (_currentPoiData == null) return;
            
            gameObject.SetActive(true);

            // SAVE SQUAD DATA: Fix for "Confirm" (Tiến Vào) button not working when re-opening map
            if (_currentSquadIDs != null)
            {
                if (!_activeSquads.ContainsKey(_currentPoiData.poiId)) _activeSquads.Add(_currentPoiData.poiId, _currentSquadIDs);
                else _activeSquads[_currentPoiData.poiId] = _currentSquadIDs;
            }

            _currentMapData = new ProceduralSubStageGenerator().GenerateMap(_currentDifficulty);
            if (!_activeMaps.ContainsKey(_currentPoiData.poiId)) _activeMaps.Add(_currentPoiData.poiId, _currentMapData);
            else _activeMaps[_currentPoiData.poiId] = _currentMapData;
            
            if (poiNameText != null) poiNameText.text = $"{_currentPoiData.poiName.ToUpper()} [{_currentDifficulty}]";
            DrawProceduralMap(mapContentContainer, _currentMapData);
        }
        #endregion

        #region PROCEDURAL MAP GENERATION
        private void DrawProceduralMap(Transform parent, ShapeDrivenMapData mapData)
        {
            if (parent == null)
            {
                GameObject fbObj = new GameObject("RuntimeMapContainer");
                fbObj.transform.SetParent(this.transform, false);
                fbObj.transform.SetSiblingIndex(0); 
                RectTransform fbRt = fbObj.AddComponent<RectTransform>();
                fbRt.anchorMin = Vector2.zero; fbRt.anchorMax = Vector2.one; 
                fbRt.sizeDelta = Vector2.zero; fbRt.anchoredPosition = Vector2.zero;
                
                Image bg = fbObj.AddComponent<Image>();
                bg.color = new Color(0.1f, 0.1f, 0.15f, 1f); 
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
                    if (viewport.GetComponent<UnityEngine.UI.RectMask2D>() == null) viewport.AddComponent<UnityEngine.UI.RectMask2D>();
                    
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

            if (node.Status == NodeStatus.Cleared) img.color = Color.gray;
            else if (node.Status == NodeStatus.InProgress || node.Status == NodeStatus.Available)
            {
                if (img.gameObject.GetComponent<PulseAnimation>() == null) img.gameObject.AddComponent<PulseAnimation>();
            }
            else if (node.Status == NodeStatus.Locked)
            {
                img.color = new Color(img.color.r * 0.3f, img.color.g * 0.3f, img.color.b * 0.3f, 1f);
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
            if (nodeDetailPopup == null) CreateNodeDetailPopupFallback();
            nodeDetailPopup.Show(node, () => {
                if (node.Status == NodeStatus.Available)
                {
                    if (_currentSquadIDs == null || _currentSquadIDs.Count == 0) return;

                    var fakePOI = _currentPoiData;
                    fakePOI.difficultyLevel = Mathf.Max(1, (int)_currentDifficulty + 1);
                    POIBattleResolution resolution = POIBattleResolver.Resolve(fakePOI, node, _currentSquadIDs, _currentDifficulty, GameManager.Instance.ExpeditionManager);

                    if (resolution.DidWin)
                    {
                        if (GameManager.Instance.ExpeditionManager != null)
                        {
                            GameManager.Instance.ExpeditionManager.AddReportToMailbox(resolution.Report);
                        }
                        else if (DataManager.Instance?.Player?.UnclaimedReports != null)
                        {
                            DataManager.Instance.Player.UnclaimedReports.Add(resolution.Report);
                            DataManager.Instance.SavePlayerData();
                        }
                        GameManager.Instance.UINotificationManager?.ShowNotification($"Victory chance {resolution.WinChance:P0}. Reward sent to mailbox.");
                        ExecuteNodeAction(node);
                    }
                    else
                    {
                        GameManager.Instance.UINotificationManager?.ShowNotification($"Defeat. Win chance {resolution.WinChance:P0}; stage stays here.");
                        ExecuteNodeLose(node);
                    }
                }
            }, () => { ExecuteNodeLose(node); _onExploreCallback?.Invoke(); });
        }

        private void ExecuteNodeAction(SubStageNode node)
        {
            node.Status = NodeStatus.Cleared;
            _currentMapData.CurrentNodeId = node.Id;
            if (node.Type == SubStageNodeType.Boss)
            {
                _activeMaps.Remove(_currentPoiData.poiId);
                _activeSquads.Remove(_currentPoiData.poiId);
                ClosePanel();
                return;
            }
            foreach (var n in _currentMapData.Nodes) if (n.Floor == node.Floor && n.Id != node.Id) n.Status = NodeStatus.Locked;
            foreach (var nextId in node.OutgoingEdges) { var nNext = _currentMapData.Nodes.Find(x => x.Id == nextId); if (nNext != null && nNext.Status != NodeStatus.Cleared) nNext.Status = NodeStatus.Available; }
            DrawProceduralMap(mapContentContainer, _currentMapData);
        }

        private void ExecuteNodeLose(SubStageNode failedNode)
        {
            failedNode.Status = NodeStatus.Available;
            _currentMapData.CurrentNodeId = failedNode.Id;

            foreach (var node in _currentMapData.Nodes)
            {
                if (node.Floor > failedNode.Floor && node.Status != NodeStatus.Cleared)
                {
                    node.Status = NodeStatus.Locked;
                }
            }

            DrawProceduralMap(mapContentContainer, _currentMapData);
        }

        private void CreateNodeDetailPopupFallback()
        {
            GameObject detObj = new GameObject("NodeDetailPopup_Runtime"); detObj.transform.SetParent(this.transform, false); detObj.transform.SetAsLastSibling();
            RectTransform rt = detObj.AddComponent<RectTransform>(); rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero; rt.anchoredPosition = Vector2.zero;
            Image bg = detObj.AddComponent<Image>(); bg.color = new Color(0, 0, 0, 0.90f);
            GameObject boxObj = new GameObject("InnerBox"); boxObj.transform.SetParent(detObj.transform, false);
            RectTransform boxRt = boxObj.AddComponent<RectTransform>(); boxRt.anchorMin = new Vector2(0.5f, 0.5f); boxRt.anchorMax = new Vector2(0.5f, 0.5f); boxRt.sizeDelta = new Vector2(800, 900); boxRt.anchoredPosition = Vector2.zero;
            Image boxBg = boxObj.AddComponent<Image>(); boxBg.color = new Color(0.1f, 0.1f, 0.15f, 0.98f);
            GameObject titleObj = new GameObject("Title"); titleObj.transform.SetParent(boxObj.transform, false);
            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>(); titleTmp.fontSize = 50; titleTmp.alignment = TextAlignmentOptions.Center; titleTmp.color = Color.white;
            RectTransform titleRt = titleTmp.rectTransform; titleRt.anchorMin = new Vector2(0.5f, 0.5f); titleRt.anchorMax = new Vector2(0.5f, 0.5f); titleRt.sizeDelta = new Vector2(700, 100); titleRt.anchoredPosition = new Vector2(0, 350);
            GameObject descObj = new GameObject("Description"); descObj.transform.SetParent(boxObj.transform, false);
            TextMeshProUGUI descTmp = descObj.AddComponent<TextMeshProUGUI>(); descTmp.fontSize = 36; descTmp.alignment = TextAlignmentOptions.TopLeft; descTmp.color = Color.yellow;
            RectTransform descRt = descTmp.rectTransform; descRt.anchorMin = new Vector2(0.5f, 0.5f); descRt.anchorMax = new Vector2(0.5f, 0.5f); descRt.sizeDelta = new Vector2(700, 250); descRt.anchoredPosition = new Vector2(0, 150);
            GameObject dropObj = new GameObject("Drops"); dropObj.transform.SetParent(boxObj.transform, false);
            TextMeshProUGUI dropTmp = dropObj.AddComponent<TextMeshProUGUI>(); dropTmp.fontSize = 32; dropTmp.alignment = TextAlignmentOptions.TopLeft; dropTmp.color = Color.green;
            RectTransform dropRt = dropTmp.rectTransform; dropRt.anchorMin = new Vector2(0.5f, 0.5f); dropRt.anchorMax = new Vector2(0.5f, 0.5f); dropRt.sizeDelta = new Vector2(700, 250); dropRt.anchoredPosition = new Vector2(0, -150);
            Button btnGo = CreateRuntimeButton(boxObj.transform, "Btn_Combat", "Tiến Vào", new Color(0.8f, 0.2f, 0.2f), -280);
            Button btnLose = CreateRuntimeButton(boxObj.transform, "Btn_Lose", "Đầu Hàng (Lùi Lại)", new Color(0.2f, 0.4f, 0.8f), -420);
            nodeDetailPopup = detObj.AddComponent<LegendOfBlood.NodeDetailPopup>();
            nodeDetailPopup.titleText = titleTmp; nodeDetailPopup.monstersText = descTmp; nodeDetailPopup.lootText = dropTmp; nodeDetailPopup.btnAction = btnGo; nodeDetailPopup.btnActionText = btnGo.GetComponentInChildren<TextMeshProUGUI>(); nodeDetailPopup.btnLose = btnLose; nodeDetailPopup.btnLoseText = btnLose.GetComponentInChildren<TextMeshProUGUI>();
        }
        #endregion

        private void ClosePanel() { if (difficultyPopup != null) difficultyPopup.gameObject.SetActive(false); if (nodeDetailPopup != null) nodeDetailPopup.gameObject.SetActive(false); GameManager.Instance.UIManager.GoBack(); }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace LegendOfBlood
{
    public class RegionDetailPopupController : MonoBehaviour
    {
        [Header("UI References")]
        public Transform mapContentContainer;
        public NodeDetailPopup nodeDetailPopup;

        private ProceduralDifficulty _currentDifficulty;
        private POIData _currentPoiData;
        private List<string> _currentSquadIDs;
        private ShapeDrivenMapData _currentMapData;
        private Dictionary<string, ShapeDrivenMapData> _activeMaps = new Dictionary<string, ShapeDrivenMapData>();
        private Dictionary<SubStageNode, GameObject> _nodeUIObjects = new Dictionary<SubStageNode, GameObject>();
        private SubStageNode _currentlySelectedNode;

        public void SetupAndShow(POIData poiData, ProceduralDifficulty difficulty, List<string> squadIDs)
        {
            _currentPoiData = poiData;
            _currentDifficulty = difficulty;
            _currentSquadIDs = squadIDs;

            gameObject.SetActive(true);

            _currentMapData = new ProceduralSubStageGenerator().GenerateMap(_currentDifficulty);
            if (!_activeMaps.ContainsKey(_currentPoiData.poiId))
            {
                _activeMaps.Add(_currentPoiData.poiId, _currentMapData);
            }
            else
            {
                _activeMaps[_currentPoiData.poiId] = _currentMapData;
            }

            DrawProceduralMap(mapContentContainer, _currentMapData);
        }

        public void ClosePopup()
        {
            gameObject.SetActive(false);
            var wmc = Object.FindAnyObjectByType<WorldMapFixedController>();
            if (wmc != null && wmc.gameObject.activeInHierarchy) {
                // Return to WorldMap view
            }
        }

        private void DrawProceduralMap(Transform parent, ShapeDrivenMapData mapData)
        {
            if (parent == null)
            {
                // Fallback 
                GameObject fbObj = new GameObject("RuntimeMapContainer");
                fbObj.transform.SetParent(this.transform, false);
                fbObj.transform.SetAsFirstSibling(); 
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
                    
                    var heroSquad = new List<LegendOfBlood.HeroData>();
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
                ClosePopup();
                return;
            }

            foreach (var n in _currentMapData.Nodes)
            {
                if (n.Floor == node.Floor && n.Id != node.Id)
                {
                    n.Status = NodeStatus.Locked;
                }
            }

            foreach (var nextId in node.OutgoingEdges)
            {
                var nNext = _currentMapData.Nodes.Find(x => x.Id == nextId);
                if (nNext != null && nNext.Status != NodeStatus.Cleared) 
                    nNext.Status = NodeStatus.Available;
            }

            DrawProceduralMap(mapContentContainer, _currentMapData);
            if (nodeDetailPopup != null) nodeDetailPopup.gameObject.SetActive(false);
        }

        private void ExecuteNodeLose(SubStageNode node)
        {
            LegendOfBlood.ToastNotificationManager.Show($"Đã lùi lại 1 bước tại Tầng {node.Floor}.", 2f);
        }

        private void CreateNodeDetailPopupFallback()
        {
            GameObject popupObj = new GameObject("NodeDetailPopup_Runtime");
            popupObj.transform.SetParent(this.transform, false);
            popupObj.transform.SetAsLastSibling();
            RectTransform rt = popupObj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero; rt.anchoredPosition = Vector2.zero;

            Image bg = popupObj.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.95f);

            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(popupObj.transform, false);
            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.fontSize = 50; titleTmp.alignment = TextAlignmentOptions.Center; titleTmp.color = new Color(0.9f, 0.8f, 0.3f);
            RectTransform titleRt = titleTmp.rectTransform; 
            titleRt.anchorMin = new Vector2(0.5f, 0.5f); titleRt.anchorMax = new Vector2(0.5f, 0.5f); 
            titleRt.sizeDelta = new Vector2(800, 100); titleRt.anchoredPosition = new Vector2(0, 300);

            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(popupObj.transform, false);
            TextMeshProUGUI descTmp = descObj.AddComponent<TextMeshProUGUI>();
            descTmp.fontSize = 35; descTmp.alignment = TextAlignmentOptions.Center; descTmp.color = Color.white;
            RectTransform descRt = descTmp.rectTransform; 
            descRt.anchorMin = new Vector2(0.5f, 0.5f); descRt.anchorMax = new Vector2(0.5f, 0.5f); 
            descRt.sizeDelta = new Vector2(800, 200); descRt.anchoredPosition = new Vector2(0, 100);

            GameObject enemiesObj = new GameObject("EnemiesText");
            enemiesObj.transform.SetParent(popupObj.transform, false);
            TextMeshProUGUI enemiesTmp = enemiesObj.AddComponent<TextMeshProUGUI>();
            enemiesTmp.fontSize = 30; enemiesTmp.alignment = TextAlignmentOptions.Center; enemiesTmp.color = new Color(0.8f, 0.4f, 0.4f);
            RectTransform enRt = enemiesTmp.rectTransform; 
            enRt.anchorMin = new Vector2(0.5f, 0.5f); enRt.anchorMax = new Vector2(0.5f, 0.5f); 
            enRt.sizeDelta = new Vector2(800, 150); enRt.anchoredPosition = new Vector2(0, -100);

            Button btnEnter = CreateButton(popupObj.transform, "Btn_Enter", "TIẾN VÀO", new Color(0.2f, 0.6f, 0.2f), -300);
            Button btnCancel = CreateButton(popupObj.transform, "Btn_Cancel", "ĐÓNG", new Color(0.5f, 0.2f, 0.2f), -450);

            nodeDetailPopup = popupObj.AddComponent<NodeDetailPopup>();
            nodeDetailPopup.titleText = titleTmp;
            nodeDetailPopup.lootText = descTmp;
            nodeDetailPopup.monstersText = enemiesTmp;
            nodeDetailPopup.btnAction = btnEnter;
            nodeDetailPopup.btnClose = btnCancel;
        }

        private Button CreateButton(Transform p, string n, string t, Color c, float yPos)
        {
            GameObject bObj = new GameObject(n);
            bObj.transform.SetParent(p, false);
            RectTransform rt = bObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(400, 100);
            rt.anchoredPosition = new Vector2(0, yPos);
            
            Image img = bObj.AddComponent<Image>(); img.color = c;
            
            GameObject tObj = new GameObject("Text");
            tObj.transform.SetParent(bObj.transform, false);
            TextMeshProUGUI tmp = tObj.AddComponent<TextMeshProUGUI>();
            tmp.text = t; tmp.fontSize = 40; tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white;
            tmp.rectTransform.anchorMin = Vector2.zero; tmp.rectTransform.anchorMax = Vector2.one; tmp.rectTransform.sizeDelta = Vector2.zero;

            return bObj.AddComponent<Button>();
        }
    }
}

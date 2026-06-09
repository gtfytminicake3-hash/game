namespace LegendOfBlood.Combat
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using System.Collections;
    using System.Collections.Generic;

    public class CombatVisualizerPanel : UIPanel
    {
        [Header("Containers")]
        public Transform allyContainer;
        public Transform enemyContainer;

        [Header("UI Controls")]
        public Button skipButton;
        public TextMeshProUGUI skipButtonText;
        public Button x2SpeedButton;
        public TextMeshProUGUI _speedText;
        public GameObject victoryScreen;
        public TextMeshProUGUI victoryTitleText;
        public GameObject defeatScreen;
        public TextMeshProUGUI defeatTitleText;
        public Button closeVictoryButton;
        public Button closeDefeatButton;

        [Header("Settings")]
        public GameObject battleUnitPrefab;
        
        private Dictionary<string, BattleUnitUI> _unitMap = new Dictionary<string, BattleUnitUI>();
        private CombatResult _cachedResult;
        private Coroutine _playbackRoutine;
        
        // Speed control
        private float _actionDelay = 1.0f;
        private int _speedMultiplier = 1;

        private void Awake()
        {
            PanelType = UIPanelType.Battle; // TÃ†Â°Ã†Â¡ng Ã¡Â»Â©ng vÃ¡Â»â€ºi loÃ¡ÂºÂ¡i mÃƒÂ  UIManager Ã„â€˜ang check

            if (skipButtonText != null) skipButtonText.text = global::LocalizationSystem.GetText("btn_skip");
            if (victoryTitleText != null) victoryTitleText.text = global::LocalizationSystem.GetText("combat_result_title");
            if (defeatTitleText != null) defeatTitleText.text = global::LocalizationSystem.GetText("combat_result_title");

            if (skipButton != null) skipButton.onClick.AddListener(SkipPlayback);
            if (x2SpeedButton != null) x2SpeedButton.onClick.AddListener(ToggleSpeed);
            if (closeVictoryButton != null) closeVictoryButton.onClick.AddListener(ClosePanel);
            if (closeDefeatButton != null) closeDefeatButton.onClick.AddListener(ClosePanel);
            
            ToggleSpeedDisplay();
        }

        protected override void Start()
        {
            base.Start();
        }

                public void PlayCombat(CombatResult result, List<HeroData> initialAllies, List<HeroData> initialEnemies, List<string> monsterIds = null)
        {
            _cachedResult = result;
            if (victoryScreen != null) victoryScreen.SetActive(false);
            if (defeatScreen != null) defeatScreen.SetActive(false);
            
            Transform battleTitle = transform.Find("BattleTitle");
            if (battleTitle != null) battleTitle.gameObject.SetActive(false);
            
            ClearBoard();

            if (result.AllyReplayUnits != null && result.AllyReplayUnits.Count > 0)
            {
                foreach (var snap in result.AllyReplayUnits)
                {
                    var unitUI = InstantiateSnapshotInSlot(snap);
                    _unitMap[snap.unitId] = unitUI;
                    UnityEngine.Debug.Log($"[CombatVisualizer] Spawn {snap.unitId} - {snap.displayName} (Ally: {snap.isAlly})");
                }
                
                foreach (var snap in result.EnemyReplayUnits)
                {
                    var unitUI = InstantiateSnapshotInSlot(snap);
                    _unitMap[snap.unitId] = unitUI;
                    UnityEngine.Debug.Log($"[CombatVisualizer] Spawn {snap.unitId} - {snap.displayName} (Ally: {snap.isAlly})");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("[CombatVisualizer] Replay snapshot missing, using legacy fallback.");
                int allyIndex = 0;
                foreach (var h in initialAllies)
                {
                    string id = $"P_{h.profession}_{allyIndex}";
                    int slotIndex = allyIndex % 9;
                    if (_cachedResult.InitialPositions != null)
                    {
                        var pos = _cachedResult.InitialPositions.Find(p => p.InstanceID == id && p.IsAlly);
                        if (!string.IsNullOrEmpty(pos.InstanceID)) slotIndex = pos.SlotIndex;
                    }

                    var unit = InstantiateUnitInSlot(h, id, true, slotIndex);
                    _unitMap[id] = unit;
                    allyIndex++;
                }

                int enemyIndex = 0;
                if (initialEnemies != null)
                {
                    foreach (var e in initialEnemies)
                    {
                        string id = $"E_{e.profession}_{enemyIndex}";
                        int slotIndex = enemyIndex % 9;
                        if (_cachedResult.InitialPositions != null)
                        {
                            var pos = _cachedResult.InitialPositions.Find(p => p.InstanceID == id && !p.IsAlly);
                            if (!string.IsNullOrEmpty(pos.InstanceID)) slotIndex = pos.SlotIndex;
                        }

                        var unit = InstantiateUnitInSlot(e, id, false, slotIndex);
                        _unitMap[id] = unit;
                        enemyIndex++;
                    }
                } 
            }

            if (_cachedResult.EventLog != null && _cachedResult.EventLog.Count > 0)
            {
                if (_playbackRoutine != null) StopCoroutine(_playbackRoutine);
                _playbackRoutine = StartCoroutine(PlaybackRoutine());
            }
            else
            {
                ShowResultScreen();
            }

            StartCoroutine(LogAfterOneFrame());
        }

        private IEnumerator LogAfterOneFrame()
        {
            yield return null; // Wait for end of frame
            int remainingUnits = 0;
            foreach (var unit in _unitMap.Values)
            {
                if (unit != null && unit.gameObject != null) remainingUnits++;
            }
            Debug.Log($"[PostFrameCheck] Cards still existing after first frame: {remainingUnits}");
        }

        
        private BattleUnitUI InstantiateSnapshotInSlot(CombatReplayUnitSnapshot snap)
        {
            Transform container = snap.isAlly ? allyContainer : enemyContainer;
            Transform parentSlot = (container.childCount > snap.slotIndex) ? container.GetChild(snap.slotIndex) : container;

            // Fix layout
            var grid = container.GetComponent<UnityEngine.UI.GridLayoutGroup>();
            if (grid != null)
            {
                grid.cellSize = new Vector2(55, 85);
                grid.spacing = new Vector2(10, 10);
                grid.padding = new RectOffset(10, 10, 10, 10);
            }

            GameObject go = Instantiate(battleUnitPrefab, parentSlot);
            go.SetActive(true);
            
            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;
                rt.localRotation = Quaternion.identity;
                go.transform.SetAsLastSibling();
            }
            BattleUnitUI unit = go.GetComponent<BattleUnitUI>();
            
            Sprite sprite = LegendOfBlood.CardArtResolver.GetUnitSprite(snap);
            unit.SetupReplayUnit(snap, sprite);
            
            // Cleanup visuals not needed for replay and the bugged background
            if (unit.hpSlider != null)
            {
                Transform badBg = unit.hpSlider.transform.Find("Background");
                if (badBg != null) badBg.gameObject.SetActive(false);
            }

            string[] visualsToHide = { "ClassBadge", "RankBadge", "RarityFrame", "Glow" };
            foreach (string v in visualsToHide)
            {
                Transform found = go.transform.Find(v);
                if (found != null) found.gameObject.SetActive(false);
            }
            
            return unit;
        }

                private BattleUnitUI InstantiateUnitInSlot(HeroData hData, string unitId, bool isAlly, int slotIndex)
        {
            Transform container = isAlly ? allyContainer : enemyContainer;
            Transform parentSlot = (container.childCount > slotIndex) ? container.GetChild(slotIndex) : container;

            GameObject go = Instantiate(battleUnitPrefab, parentSlot);
            go.SetActive(true);
            
            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;
                rt.localRotation = Quaternion.identity;
                go.transform.SetAsLastSibling();
            }
            BattleUnitUI unit = go.GetComponent<BattleUnitUI>();
            
            int maxHp = (int)hData.GetFinalStats().hp;
            int startHp = UnityEngine.Mathf.Max(0, (int)hData.currentHp);
            
            UnityEngine.Sprite sprite = LegendOfBlood.CardArtResolver.GetUnitSprite(hData, !isAlly);
            unit.Setup(sprite, maxHp, startHp);

            UnityEngine.Debug.Log($"[Spawn] Spawned hero card: {hData.heroName}, ID: {unitId}, Parent slot: {parentSlot.name}, Active: {go.activeSelf}");

            return unit;
        }

        private void ClearBoard()
        {
            int allySlotCount = allyContainer.childCount;
            int enemySlotCount = enemyContainer.childCount;
            int destroyedAllyCards = 0;
            int destroyedEnemyCards = 0;

            _unitMap.Clear();

            foreach (Transform childOrSlot in allyContainer)
            {
                if (childOrSlot.GetComponent<BattleUnitUI>() != null)
                {
                    UnityEngine.Object.Destroy(childOrSlot.gameObject);
                    destroyedAllyCards++;
                }
                else
                {
                    foreach (Transform child in childOrSlot)
                    {
                        if (child.GetComponent<BattleUnitUI>() != null)
                        {
                            UnityEngine.Object.Destroy(child.gameObject);
                            destroyedAllyCards++;
                        }
                    }
                }
            }

            foreach (Transform childOrSlot in enemyContainer)
            {
                if (childOrSlot.GetComponent<BattleUnitUI>() != null)
                {
                    UnityEngine.Object.Destroy(childOrSlot.gameObject);
                    destroyedEnemyCards++;
                }
                else
                {
                    foreach (Transform child in childOrSlot)
                    {
                        if (child.GetComponent<BattleUnitUI>() != null)
                        {
                            UnityEngine.Object.Destroy(child.gameObject);
                            destroyedEnemyCards++;
                        }
                    }
                }
            }
        }

        private IEnumerator PlaybackRoutine()
        {
            foreach (var ev in _cachedResult.EventLog)
            {
                switch (ev.EventType)
                {
                    case CombatEventType.Attack:
                        if (!string.IsNullOrEmpty(ev.SourceID) && _unitMap.ContainsKey(ev.SourceID) && _unitMap[ev.SourceID] != null)
                        {
                            Vector3 targetPos = _unitMap[ev.SourceID].transform.position + Vector3.right * 1f; // NhÃƒÂ­ch lÃƒÂªn 1 chÃƒÂºt nÃ¡ÂºÂ¿u khÃƒÂ´ng cÃƒÂ³ target
                            if (ev.TargetID != null && _unitMap.ContainsKey(ev.TargetID) && _unitMap[ev.TargetID] != null) 
                                targetPos = _unitMap[ev.TargetID].transform.position;
                            
                            yield return StartCoroutine(_unitMap[ev.SourceID].PlayAttackAnim(targetPos));
                        }
                        
                        if (!string.IsNullOrEmpty(ev.TargetID) && _unitMap.ContainsKey(ev.TargetID) && _unitMap[ev.TargetID] != null)
                        {
                            _unitMap[ev.TargetID].TakeDamage(ev.Value, ev.IsCrit);
                        }
                        break;
                        
                    case CombatEventType.Heal:
                        if (!string.IsNullOrEmpty(ev.TargetID) && _unitMap.ContainsKey(ev.TargetID) && _unitMap[ev.TargetID] != null)
                        {
                            _unitMap[ev.TargetID].Heal(ev.Value);
                        }
                        break;

                    case CombatEventType.TakeDamage:
                        if (!string.IsNullOrEmpty(ev.TargetID) && _unitMap.ContainsKey(ev.TargetID) && _unitMap[ev.TargetID] != null)
                        {
                            _unitMap[ev.TargetID].TakeDamage(ev.Value, false);
                        }
                        break;

                    case CombatEventType.TurnStart:
                        // Delay mÃ¡Â»Âng Ã„â€˜Ã¡ÂºÂ§u mÃ¡Â»â€”i turn Ã„â€˜Ã¡Â»Æ’ dÃ¡Â»â€¦ thÃ¡Â»Å¸
                        yield return new WaitForSeconds(0.2f * _actionDelay);
                        break;
                }

                // DÃ¡Â»Â«ng chÃ¡Â»Â sau mÃ¡Â»â€”i Action lÃ¡Â»â€ºn (Attack/Heal)
                if (ev.EventType == CombatEventType.Attack || ev.EventType == CombatEventType.Heal)
                {
                    yield return new WaitForSeconds(0.6f * _actionDelay);
                }
            }
            
            // DiÃ¡Â»â€¦n xong, Ã„â€˜Ã¡Â»Â£i 1 giÃƒÂ¢y rÃ¡Â»â€œi show kÃ¡ÂºÂ¿t quÃ¡ÂºÂ£
            yield return new WaitForSeconds(1f);
            ShowResultScreen();
        }

        private void ToggleSpeed()
        {
            if (_speedMultiplier == 1) _speedMultiplier = 2;
            else if (_speedMultiplier == 2) _speedMultiplier = 4;
            else _speedMultiplier = 1;
            
            _actionDelay = 1.0f / _speedMultiplier;
            ToggleSpeedDisplay();
        }
        
        private void ToggleSpeedDisplay()
        {
            if (_speedText != null) _speedText.text = $"x{_speedMultiplier}";
        }

        private void SkipPlayback()
        {
            if (_playbackRoutine != null)
            {
                StopCoroutine(_playbackRoutine);
            }
            
            // Khi bÃ¡ÂºÂ¥m Skip, ÃƒÂ©p tÃ¡ÂºÂ¥t cÃ¡ÂºÂ£ thanh mÃƒÂ¡u vÃ¡Â»Â trÃ¡ÂºÂ¡ng thÃƒÂ¡i cuÃ¡Â»â€˜i trÃ¡ÂºÂ­n
            ApplyFinalState();
            ShowResultScreen();
        }

        private void ApplyFinalState()
        {
            // TrÃ¡ÂºÂ¡ng thÃƒÂ¡i cuÃ¡Â»â€˜i nÃ¡ÂºÂ±m Ã¡Â»Å¸ _cachedResult
            UpdateTeamState(_cachedResult.PlayerSurvivors, true, true);
            UpdateTeamState(_cachedResult.PlayerCasualties, true, false);
            UpdateTeamState(_cachedResult.EnemySurvivors, false, true);
            UpdateTeamState(_cachedResult.EnemyCasualties, false, false);
        }

        private void UpdateTeamState(List<HeroData> teamList, bool isAlly, bool isAlive)
        {
            if (teamList == null) return;
            string prefix = isAlly ? "P_" : "E_";
            
             // Ã„ÂÃ¡Â»Æ’ Ã„â€˜Ã†Â¡n giÃ¡ÂºÂ£n khi Skip, ta chÃ¡Â»â€° lÃƒÂ m mÃ¡Â»Â nhÃ¡Â»Â¯ng con Ã„â€˜ÃƒÂ£ chÃ¡ÂºÂ¿t bÃ¡ÂºÂ±ng cÃƒÂ¡ch ÃƒÂ©p mÃƒÂ¡u = 0
            // vÃƒÂ  tÃ¡ÂºÂ¯t GameObject Ã„â€˜i luÃƒÂ´n cho sÃ¡ÂºÂ¡ch thay vÃƒÂ¬ Ã„â€˜Ã¡Â»Æ’ lÃ¡Â»â€”i hÃƒÂ¬nh Ã¡ÂºÂ£nh trÃ¡ÂºÂ¯ng bÃƒÂ³c.
            if (!isAlive)
            {
                 foreach(var kv in _unitMap)
                 {
                     if (kv.Key.StartsWith(prefix))
                     {
                         // KiÃ¡Â»Æ’m tra null trÃ†Â°Ã¡Â»â€ºc khi thao tÃƒÂ¡c vÃƒÂ¬ obj cÃƒÂ³ thÃ¡Â»Æ’ Ã„â€˜ÃƒÂ£ bÃ¡Â»â€¹ unity destroy
                         if (kv.Value == null || kv.Value.gameObject == null) continue;

                         // NÃ¡ÂºÂ¿u HeroData trong listCasualties trÃƒÂ¹ng khÃ¡Â»â€ºp Profession/ID vÃ¡Â»â€ºi Key
                         foreach(var dead in teamList)
                         {
                             if (kv.Key.Contains(dead.profession.ToString()))
                             {
                                 if (kv.Value != null && kv.Value.gameObject.activeInHierarchy)
                                 {
                                     kv.Value.TakeDamage(99999, false); // NÃ¡Â»â€¢ mÃƒÂ¡u Ã¡ÂºÂ£o Ã„â€˜Ã¡Â»Æ’ xÃƒÂ¡m Ã¡ÂºÂ£nh
                                     kv.Value.gameObject.SetActive(false); // Ã¡ÂºÂ¨n luÃƒÂ´n unit Ã„â€˜ÃƒÂ£ chÃ¡ÂºÂ¿t khi skip cho sÃ¡ÂºÂ¡ch bÃƒÂ n cÃ¡Â»Â
                                 }
                             }
                         }
                     }
                 }
            }
        }

        private void ShowResultScreen()
        {
            if (_cachedResult.DidPlayerWin)
            {
                if (victoryScreen != null) victoryScreen.SetActive(true);
            }
            else
            {
                if (defeatScreen != null) defeatScreen.SetActive(true);
            }
        }

        private void ClosePanel()
        {
            ClearBoard();
            GameManager.Instance.UIManager.GoBack();
        }
    }
}




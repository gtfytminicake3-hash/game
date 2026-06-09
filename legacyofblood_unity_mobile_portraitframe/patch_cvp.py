import re

filepath = 'Assets/Scripts/UI/CombatVisualizerPanel.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Add ReplayUnitView
class_str = """        public class ReplayUnitView
        {
            public string unitId;
            public string displayName;
            public bool isAlly;
            public bool isMonster;
            public BattleUnitUI view;
            public int currentHp;
            public int maxHp;
        }
        private Dictionary<string, ReplayUnitView> _replayUnitMap = new Dictionary<string, ReplayUnitView>();
"""
content = re.sub(r'private Dictionary<string, BattleUnitUI> _unitMap[^;]+;\n\s*private Dictionary<string, int> _visualCurrentHp[^;]+;\n\s*private Dictionary<string, int> _visualMaxHp[^;]+;', class_str, content)

# 2. Add InstantiateSnapshotInSlot right before InstantiateUnitInSlot
snap_instantiate = """        private BattleUnitUI InstantiateSnapshotInSlot(CombatReplayUnitSnapshot snap)
        {
            if (_simpleUnitPrefab == null)
            {
                _simpleUnitPrefab = CreateSimpleUnitPrefab();
            }

            Transform container = snap.isAlly ? allyContainer : enemyContainer;
            Transform parentSlot = (container.childCount > snap.slotIndex) ? container.GetChild(snap.slotIndex) : container;

            GameObject go = Instantiate(_simpleUnitPrefab, parentSlot);
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
            
            unit.SetHp(snap.startHp, snap.maxHp);
            
            Sprite sprite = LegendOfBlood.Utils.CardArtResolver.GetPortraitSprite(snap.spriteId, snap.isMonster);
            if (sprite != null && unit.avatarImage != null)
            {
                unit.avatarImage.sprite = sprite;
            }
            
            return unit;
        }

"""
content = content.replace("private BattleUnitUI InstantiateUnitInSlot", snap_instantiate + "        private BattleUnitUI InstantiateUnitInSlot")

# 3. Replace PlayCombat
play_combat = """        public void PlayCombat(CombatResult result, List<HeroData> initialAllies, List<HeroData> initialEnemies, List<string> monsterIds = null)
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
                    _replayUnitMap[snap.unitId] = new ReplayUnitView
                    {
                        unitId = snap.unitId,
                        displayName = snap.displayName,
                        isAlly = snap.isAlly,
                        isMonster = snap.isMonster,
                        view = unitUI,
                        currentHp = snap.startHp,
                        maxHp = snap.maxHp
                    };
                }
                
                foreach (var snap in result.EnemyReplayUnits)
                {
                    var unitUI = InstantiateSnapshotInSlot(snap);
                    _replayUnitMap[snap.unitId] = new ReplayUnitView
                    {
                        unitId = snap.unitId,
                        displayName = snap.displayName,
                        isAlly = snap.isAlly,
                        isMonster = snap.isMonster,
                        view = unitUI,
                        currentHp = snap.startHp,
                        maxHp = snap.maxHp
                    };
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
                    int startHp = Mathf.Max(0, (int)h.currentHp);
                    int maxHp = (int)h.GetFinalStats().hp;
                    _replayUnitMap[id] = new ReplayUnitView { unitId = id, displayName = h.heroName, isAlly = true, isMonster = false, view = unit, currentHp = startHp, maxHp = maxHp };
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
                        int startHp = Mathf.Max(0, (int)e.currentHp);
                        int maxHp = (int)e.GetFinalStats().hp;
                        _replayUnitMap[id] = new ReplayUnitView { unitId = id, displayName = e.heroName, isAlly = false, isMonster = true, view = unit, currentHp = startHp, maxHp = maxHp };
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
        }"""
content = re.sub(r'        public void PlayCombat\(CombatResult result, List<HeroData> initialAllies, List<HeroData> initialEnemies, List<string> monsterIds = null\).*?StartCoroutine\(LogAfterOneFrame\(\)\);\n        }', play_combat, content, flags=re.DOTALL)

# 4. Replace ClearBoard
clear_board = """        private void ClearBoard()
        {
            _replayUnitMap.Clear();
            int allySlotCount = allyContainer.childCount;
            int enemySlotCount = enemyContainer.childCount;

            foreach (Transform childOrSlot in allyContainer)
            {
                if (childOrSlot.GetComponent<BattleUnitUI>() != null) Destroy(childOrSlot.gameObject);
                else foreach (Transform card in childOrSlot) Destroy(card.gameObject);
            }
            foreach (Transform childOrSlot in enemyContainer)
            {
                if (childOrSlot.GetComponent<BattleUnitUI>() != null) Destroy(childOrSlot.gameObject);
                else foreach (Transform card in childOrSlot) Destroy(card.gameObject);
            }
        }"""
content = re.sub(r'        private void ClearBoard\(\).*?        }', clear_board, content, flags=re.DOTALL)

# 5. ApplyHpChange
apply_hp = """        private void ApplyHpChange(string targetId, int damageValue, bool isCrit, bool isHeal = false)
        {
            if (!_replayUnitMap.TryGetValue(targetId, out ReplayUnitView rUnit))
            {
                UnityEngine.Debug.LogWarning($"[CombatVisualizer] ApplyHpChange missing targetId: {targetId}");
                return;
            }

            if (isHeal)
            {
                rUnit.currentHp = Mathf.Clamp(rUnit.currentHp + damageValue, 0, rUnit.maxHp);
                if (rUnit.view != null)
                {
                    rUnit.view.SetHp(rUnit.currentHp, rUnit.maxHp);
                    rUnit.view.Heal(damageValue);
                }
            }
            else
            {
                rUnit.currentHp = Mathf.Clamp(rUnit.currentHp - damageValue, 0, rUnit.maxHp);
                if (rUnit.view != null)
                {
                    rUnit.view.SetHp(rUnit.currentHp, rUnit.maxHp);
                    rUnit.view.TakeDamage(damageValue, isCrit);
                }
            }
        }"""
content = re.sub(r'        private void ApplyHpChange\(string targetId, int damageValue, bool isCrit, bool isHeal = false\).*?(?=\n        private void ShowResultScreen)', apply_hp + "\n", content, flags=re.DOTALL)

# 6. PlaybackRoutine
playback_routine = """        private IEnumerator PlaybackRoutine()
        {
            UnityEngine.Debug.Log($"[CombatVisualizer] Start Playback. EventLog count: {_cachedResult.EventLog.Count}");
            yield return new WaitForSeconds(0.5f);

            int eventIdx = 0;
            foreach (var evt in _cachedResult.EventLog)
            {
                if (evt.EventType == CombatEventType.Attack)
                {
                    if (!_replayUnitMap.TryGetValue(evt.SourceID, out ReplayUnitView source))
                    {
                        UnityEngine.Debug.LogWarning($"[CombatVisualizer] missing sourceId {evt.SourceID}");
                        continue;
                    }
                    if (!_replayUnitMap.TryGetValue(evt.TargetID, out ReplayUnitView target))
                    {
                        UnityEngine.Debug.LogWarning($"[CombatVisualizer] missing targetId {evt.TargetID}");
                        continue;
                    }
                    
                    if (source.isAlly == target.isAlly)
                    {
                        UnityEngine.Debug.LogWarning($"[CombatVisualizer] Same team action detected! index {eventIdx} source {evt.SourceID} target {evt.TargetID}");
                    }

                    UnityEngine.Debug.Log($"[CombatVisualizer] Action {eventIdx} Attack: {source.unitId} -> {target.unitId} val {evt.Value}");

                    if (source.view != null && target.view != null)
                    {
                        source.view.transform.SetAsLastSibling();
                        yield return source.view.PlayAttackAnim(target.view.transform.position);
                    }

                    ApplyHpChange(evt.TargetID, evt.Value, evt.IsCrit);
                    yield return new WaitForSeconds(0.3f * _actionDelay);
                }
                else if (evt.EventType == CombatEventType.SkillCast)
                {
                    yield return new WaitForSeconds(0.3f * _actionDelay);
                }
                else if (evt.EventType == CombatEventType.TakeDamage)
                {
                    ApplyHpChange(evt.TargetID, evt.Value, false);
                    yield return new WaitForSeconds(0.1f * _actionDelay);
                }
                else if (evt.EventType == CombatEventType.Heal)
                {
                    ApplyHpChange(evt.TargetID, evt.Value, false, true);
                    yield return new WaitForSeconds(0.1f * _actionDelay);
                }
                eventIdx++;
            }

            yield return new WaitForSeconds(1.0f);
            ShowResultScreen();
        }"""
content = re.sub(r'        private IEnumerator PlaybackRoutine\(\).*?(?=\n        private void ApplyHpChange)', playback_routine + "\n", content, flags=re.DOTALL)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Patch successful!")

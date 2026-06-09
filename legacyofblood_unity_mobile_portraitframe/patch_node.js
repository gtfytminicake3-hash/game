const fs = require('fs');

const filepath = 'Assets/Scripts/UI/CombatVisualizerPanel.cs';
let content = fs.readFileSync(filepath, 'utf8');

const play_combat_new =         public void PlayCombat(CombatResult result, List<HeroData> initialAllies, List<HeroData> initialEnemies, List<string> monsterIds = null)
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
                    UnityEngine.Debug.Log($"[CombatVisualizer] Spawn {snap.unitId} - {snap.displayName} (Ally: {snap.isAlly})");
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
        };

content = content.replace(/public void PlayCombat\([\s\S]*?StartCoroutine\(LogAfterOneFrame\(\)\);\s*\}/, play_combat_new);

const apply_hp =         private void ApplyHpChange(string targetId, int damageValue, bool isCrit, bool isHeal = false)
        {
            if (!_replayUnitMap.TryGetValue(targetId, out ReplayUnitView rUnit))
            {
                UnityEngine.Debug.LogWarning($"[CombatVisualizer] ApplyHpChange missing targetId: {targetId}");
                return;
            }

            if (isHeal)
            {
                rUnit.currentHp = UnityEngine.Mathf.Clamp(rUnit.currentHp + damageValue, 0, rUnit.maxHp);
                if (rUnit.view != null)
                {
                    rUnit.view.SetHp(rUnit.currentHp, rUnit.maxHp);
                    rUnit.view.Heal(damageValue);
                }
            }
            else
            {
                rUnit.currentHp = UnityEngine.Mathf.Clamp(rUnit.currentHp - damageValue, 0, rUnit.maxHp);
                if (rUnit.view != null)
                {
                    rUnit.view.SetHp(rUnit.currentHp, rUnit.maxHp);
                    rUnit.view.TakeDamage(damageValue, isCrit);
                }
            }
        };

// Replace ApplyHpChange if exists, else insert
if (content.includes("private void ApplyHpChange")) {
    content = content.replace(/private void ApplyHpChange\([\s\S]*?(?=\s+private void ShowResultScreen)/, apply_hp + "\n");
} else {
    content = content.replace("private void ShowResultScreen", apply_hp + "\n\n        private void ShowResultScreen");
}

const playback_routine =         private IEnumerator PlaybackRoutine()
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
                        UnityEngine.Debug.LogWarning($"[CombatVisualizer] missing sourceId {evt.SourceID} at action {eventIdx}");
                        continue;
                    }
                    if (!_replayUnitMap.TryGetValue(evt.TargetID, out ReplayUnitView target))
                    {
                        UnityEngine.Debug.LogWarning($"[CombatVisualizer] missing targetId {evt.TargetID} at action {eventIdx}");
                        continue;
                    }
                    
                    if (source.isAlly == target.isAlly)
                    {
                        UnityEngine.Debug.LogWarning($"[CombatVisualizer] Same team action detected! action {eventIdx} source {evt.SourceID} target {evt.TargetID}");
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
        };

content = content.replace(/private IEnumerator PlaybackRoutine\(\)[\s\S]*?(?=\s+private void ApplyHpChange|\s+private void ShowResultScreen)/, playback_routine + "\n");

fs.writeFileSync(filepath, content);
console.log("Patched successfully via node.");

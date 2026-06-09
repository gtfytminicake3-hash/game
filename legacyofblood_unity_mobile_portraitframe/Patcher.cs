using System;
using System.IO;
using System.Text.RegularExpressions;

public class Program
{
    public static void Main()
    {
        string path = @"Assets\Scripts\UI\CombatVisualizerPanel.cs";
        string content = File.ReadAllText(path);

        string playCombatNew = @"        public void PlayCombat(CombatResult result, List<HeroData> initialAllies, List<HeroData> initialEnemies, List<string> monsterIds = null)
        {
            _cachedResult = result;
            if (victoryScreen != null) victoryScreen.SetActive(false);
            if (defeatScreen != null) defeatScreen.SetActive(false);
            
            Transform battleTitle = transform.Find(""BattleTitle"");
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
                    UnityEngine.Debug.Log($""[CombatVisualizer] Spawn {snap.unitId} - {snap.displayName} (Ally: {snap.isAlly})"");
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
                    UnityEngine.Debug.Log($""[CombatVisualizer] Spawn {snap.unitId} - {snap.displayName} (Ally: {snap.isAlly})"");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning(""[CombatVisualizer] Replay snapshot missing, using legacy fallback."");
                int allyIndex = 0;
                foreach (var h in initialAllies)
                {
                    string id = $""P_{h.profession}_{allyIndex}"";
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
                        string id = $""E_{e.profession}_{enemyIndex}"";
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
        }";

        content = Regex.Replace(content, @"public void PlayCombat\(CombatResult result, List<HeroData> initialAllies, List<HeroData> initialEnemies, List<string> monsterIds = null\).*?StartCoroutine\(LogAfterOneFrame\(\)\);\s*\}", playCombatNew, RegexOptions.Singleline);

                string snapshotInstantiate = @"        private BattleUnitUI InstantiateSnapshotInSlot(CombatReplayUnitSnapshot snap)
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
            if (unit.nameText != null) unit.nameText.text = snap.displayName;
            
            return unit;
        }

";
        content = content.Replace("private BattleUnitUI InstantiateUnitInSlot", snapshotInstantiate + "        private BattleUnitUI InstantiateUnitInSlot");
        string applyHpChangeNew = @"        private void ApplyHpChange(string targetId, int damageValue, bool isCrit, bool isHeal = false)
        {
            if (!_replayUnitMap.TryGetValue(targetId, out ReplayUnitView rUnit))
            {
                UnityEngine.Debug.LogWarning($""[CombatVisualizer] ApplyHpChange missing targetId: {targetId}"");
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
        }";

        if (content.Contains("private void ApplyHpChange"))
        {
            content = Regex.Replace(content, @"private void ApplyHpChange\(string targetId, int damageValue, bool isCrit, bool isHeal = false\).*?(?=\s*private void ShowResultScreen)", applyHpChangeNew + "\n", RegexOptions.Singleline);
        }
        else
        {
            content = content.Replace("private void ShowResultScreen", applyHpChangeNew + "\n\n        private void ShowResultScreen");
        }

        string playbackRoutineNew = @"        private IEnumerator PlaybackRoutine()
        {
            UnityEngine.Debug.Log($""[CombatVisualizer] Start Playback. EventLog count: {_cachedResult.EventLog.Count}"");
            yield return new WaitForSeconds(0.5f);

            int eventIdx = 0;
            foreach (var evt in _cachedResult.EventLog)
            {
                if (evt.EventType == CombatEventType.Attack)
                {
                    if (!_replayUnitMap.TryGetValue(evt.SourceID, out ReplayUnitView source))
                    {
                        UnityEngine.Debug.LogWarning($""[CombatVisualizer] missing sourceId {evt.SourceID} at action {eventIdx}"");
                        continue;
                    }
                    if (!_replayUnitMap.TryGetValue(evt.TargetID, out ReplayUnitView target))
                    {
                        UnityEngine.Debug.LogWarning($""[CombatVisualizer] missing targetId {evt.TargetID} at action {eventIdx}"");
                        continue;
                    }
                    
                    if (source.isAlly == target.isAlly)
                    {
                        UnityEngine.Debug.LogWarning($""[CombatVisualizer] Same team action detected! action {eventIdx} source {evt.SourceID} target {evt.TargetID}"");
                    }

                    UnityEngine.Debug.Log($""[CombatVisualizer] Action {eventIdx} Attack: {source.unitId} -> {target.unitId} val {evt.Value}"");

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
        }";

        content = Regex.Replace(content, @"private IEnumerator PlaybackRoutine\(\).*?(?=\s+private void ApplyHpChange|\s+private void ShowResultScreen)", playbackRoutineNew + "\n", RegexOptions.Singleline);

        File.WriteAllText(path, content);
        Console.WriteLine("Patch completed.");
    }
}




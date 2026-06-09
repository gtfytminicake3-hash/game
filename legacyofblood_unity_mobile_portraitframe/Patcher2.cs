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
                    _unitMap[snap.unitId] = unitUI;
                    UnityEngine.Debug.Log($""[CombatVisualizer] Spawn {snap.unitId} - {snap.displayName} (Ally: {snap.isAlly})"");
                }
                
                foreach (var snap in result.EnemyReplayUnits)
                {
                    var unitUI = InstantiateSnapshotInSlot(snap);
                    _unitMap[snap.unitId] = unitUI;
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
                    _unitMap[id] = unit;
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
        }";

        content = Regex.Replace(content, @"public void PlayCombat\(CombatResult result, List<HeroData> initialAllies, List<HeroData> initialEnemies, List<string> monsterIds = null\).*?StartCoroutine\(LogAfterOneFrame\(\)\);\s*\}", playCombatNew, RegexOptions.Singleline);

        string snapshotMethod = @"
        private BattleUnitUI InstantiateSnapshotInSlot(CombatReplayUnitSnapshot snap)
        {
            Transform container = snap.isAlly ? allyContainer : enemyContainer;
            Transform parentSlot = (container.childCount > snap.slotIndex) ? container.GetChild(snap.slotIndex) : container;

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
            
            Sprite sprite = LegendOfBlood.CardArtResolver.GetUnitSprite(snap.spriteId, snap.isMonster);
            unit.Setup(sprite, snap.maxHp, snap.startHp);
            
            return unit;
        }

        private BattleUnitUI InstantiateUnitInSlot";

        content = content.Replace("private BattleUnitUI InstantiateUnitInSlot", snapshotMethod);

        File.WriteAllText(path, content);
        Console.WriteLine("Patch completed.");
    }
}

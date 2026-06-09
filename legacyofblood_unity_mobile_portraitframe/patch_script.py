import re
import os

filepath = 'Assets/Scripts/UI/CombatVisualizerPanel.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Add fields
if '_visualCurrentHp' not in content:
    content = content.replace('private Dictionary<string, BattleUnitUI> _unitMap = new Dictionary<string, BattleUnitUI>();',
                              'private Dictionary<string, BattleUnitUI> _unitMap = new Dictionary<string, BattleUnitUI>();\n        private Dictionary<string, int> _visualCurrentHp = new Dictionary<string, int>();\n        private Dictionary<string, int> _visualMaxHp = new Dictionary<string, int>();\n        private GameObject _simpleUnitPrefab;')

# 2. Update InstantiateUnitInSlot
old_instantiate = r'''        private BattleUnitUI InstantiateUnitInSlot(HeroData hData, bool isAlly, int slotIndex)
        {
            if (battleUnitPrefab == null)
            {
                Debug.LogWarning\("\[CombatVisualizerPanel\] battleUnitPrefab is NULL! Tự động tạo một Prefab ảo \(Fallback\) để chống crash."\);
                battleUnitPrefab = CreateFallbackPrefab\(\);
            }'''

new_instantiate = '''        private BattleUnitUI InstantiateUnitInSlot(HeroData hData, string unitId, bool isAlly, int slotIndex)
        {
            if (_simpleUnitPrefab == null)
            {
                _simpleUnitPrefab = CreateSimpleUnitPrefab();
            }'''
content = re.sub(old_instantiate, new_instantiate, content)

# 3. Fix calls to InstantiateUnitInSlot
content = content.replace('var unit = InstantiateUnitInSlot(h, true, slotIndex);', 'var unit = InstantiateUnitInSlot(h, id, true, slotIndex);')
content = content.replace('var unit = InstantiateUnitInSlot(e, false, slotIndex);', 'var unit = InstantiateUnitInSlot(e, id, false, slotIndex);')
content = content.replace('var unit = InstantiateUnitInSlot(m, false, slotIndex);', 'var unit = InstantiateUnitInSlot(m, id, false, slotIndex);')

# 4. Populate HP dictionaries inside InstantiateUnitInSlot
old_setup = '''            unit.Setup(hData, (int)hData.currentHp, !isAlly);

            Debug.Log($"[Spawn] Spawned hero card: {hData.heroName}, Parent slot: {parentSlot.name}, Active: {go.activeSelf}, Sibling index: {go.transform.GetSiblingIndex()}, Position: {go.transform.position}");'''

new_setup = '''            int maxHp = (int)hData.GetFinalStats().hp;
            int startHp = Mathf.Max(0, (int)hData.currentHp);
            _visualCurrentHp[unitId] = startHp;
            _visualMaxHp[unitId] = maxHp;
            unit.SetHp(startHp, maxHp);
            
            unit.Setup(hData, startHp, !isAlly);

            Debug.Log($"[Spawn] Spawned hero card: {hData.heroName}, ID: {unitId}, Parent slot: {parentSlot.name}");'''
content = content.replace(old_setup, new_setup)

# 5. Replace CreateFallbackPrefab with CreateSimpleUnitPrefab
old_prefab = r'''        private GameObject CreateFallbackPrefab\(\)
        \{
            GameObject unitTemplate = new GameObject\("BattleUnitTemplate_Fallback"\);.*?return unitTemplate;
        \}'''

new_prefab = '''        private GameObject CreateSimpleUnitPrefab()
        {
            GameObject unitTemplate = new GameObject("SimpleCombatUnitView");
            unitTemplate.SetActive(false);
            RectTransform utRect = unitTemplate.AddComponent<RectTransform>();
            utRect.sizeDelta = new Vector2(160, 220); // Phù hợp với cell size 180x240
            
            // Portrait
            Image avatarImg = new GameObject("Avatar").AddComponent<Image>();
            avatarImg.transform.SetParent(unitTemplate.transform, false);
            avatarImg.rectTransform.anchorMin = Vector2.zero; avatarImg.rectTransform.anchorMax = Vector2.one;
            avatarImg.rectTransform.sizeDelta = Vector2.zero;
            avatarImg.color = Color.white; 
            
            // Background mờ mờ cho dễ nhìn
            Image bg = unitTemplate.AddComponent<Image>();
            bg.color = new Color(0,0,0, 0.5f);

            // HP Bar Background
            GameObject bgObj = new GameObject("HpBarBackground"); 
            bgObj.transform.SetParent(unitTemplate.transform, false);
            Image bgImg = bgObj.AddComponent<Image>(); bgImg.color = new Color(0.2f, 0, 0, 1f);
            bgImg.rectTransform.anchorMin = new Vector2(0.5f, 1f); 
            bgImg.rectTransform.anchorMax = new Vector2(0.5f, 1f); 
            bgImg.rectTransform.pivot = new Vector2(0.5f, 1f);
            bgImg.rectTransform.sizeDelta = new Vector2(80, 10);
            bgImg.rectTransform.anchoredPosition = new Vector2(0, 15); // Trên đầu card
            
            // HP Bar Fill
            GameObject fillObj = new GameObject("HpBarFill"); 
            fillObj.transform.SetParent(bgObj.transform, false);
            Image fillImg = fillObj.AddComponent<Image>(); 
            fillImg.color = Color.green;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 1f;
            fillImg.rectTransform.anchorMin = Vector2.zero; fillImg.rectTransform.anchorMax = Vector2.one; 
            fillImg.rectTransform.sizeDelta = Vector2.zero;

            // Damage Text
            GameObject dmgCgObj = new GameObject("DamageTextContainer");
            dmgCgObj.transform.SetParent(unitTemplate.transform, false);
            RectTransform dmgRect = dmgCgObj.AddComponent<RectTransform>();
            CanvasGroup dmgCg = dmgCgObj.AddComponent<CanvasGroup>();
            dmgRect.anchorMin = new Vector2(0, 0.5f); dmgRect.anchorMax = new Vector2(1, 1.5f);
            dmgRect.sizeDelta = Vector2.zero;
            
            TextMeshProUGUI dmgTxt = new GameObject("DamageText").AddComponent<TextMeshProUGUI>();
            dmgTxt.transform.SetParent(dmgCg.transform, false);
            dmgTxt.rectTransform.anchorMin = Vector2.zero; dmgTxt.rectTransform.anchorMax = Vector2.one;
            dmgTxt.rectTransform.sizeDelta = Vector2.zero;
            dmgTxt.text = ""; dmgTxt.fontSize = 45;
            dmgTxt.fontStyle = FontStyles.Bold; dmgTxt.alignment = TextAlignmentOptions.Center;

            BattleUnitUI unitScript = unitTemplate.AddComponent<BattleUnitUI>();
            unitScript.avatarImage = avatarImg;
            unitScript.hpFillImage = fillImg;
            unitScript.damageTextCanvasGroup = dmgCg;
            unitScript.damageText = dmgTxt;

            return unitTemplate;
        }'''
content = re.sub(old_prefab, new_prefab, content, flags=re.DOTALL)

# 6. Clear logic
old_clear = '''        private void ClearBoard()
        {
            int allySlotCount = allyContainer.childCount;
            int enemySlotCount = enemyContainer.childCount;'''

new_clear = '''        private void ClearBoard()
        {
            _visualCurrentHp.Clear();
            _visualMaxHp.Clear();
            int allySlotCount = allyContainer.childCount;
            int enemySlotCount = enemyContainer.childCount;'''
content = content.replace(old_clear, new_clear)

# 7. Update PlaybackRoutine
old_playback = r'''        private IEnumerator PlaybackRoutine\(\)
        \{
            foreach \(var ev in _cachedResult\.EventLog\)
            \{
                switch \(ev\.EventType\)
                \{
                    case CombatEventType\.Attack:
                        if \(!string\.IsNullOrEmpty\(ev\.SourceID\) && _unitMap\.ContainsKey\(ev\.SourceID\) && _unitMap\[ev\.SourceID\] != null\)
                        \{
                            Vector3 targetPos = _unitMap\[ev\.SourceID\]\.transform\.position \+ Vector3\.right \* 1f; // Nhích lên 1 chút nếu không có target
                            if \(ev\.TargetID != null && _unitMap\.ContainsKey\(ev\.TargetID\) && _unitMap\[ev\.TargetID\] != null\) 
                                targetPos = _unitMap\[ev\.TargetID\]\.transform\.position;
                            
                            yield return StartCoroutine\(_unitMap\[ev\.SourceID\]\.PlayAttackAnim\(targetPos\)\);
                        \}
                        
                        if \(!string\.IsNullOrEmpty\(ev\.TargetID\) && _unitMap\.ContainsKey\(ev\.TargetID\) && _unitMap\[ev\.TargetID\] != null\)
                        \{
                            _unitMap\[ev\.TargetID\]\.TakeDamage\(ev\.Value, ev\.IsCrit\);
                        \}
                        break;
                        
                    case CombatEventType\.Heal:
                        if \(!string\.IsNullOrEmpty\(ev\.TargetID\) && _unitMap\.ContainsKey\(ev\.TargetID\) && _unitMap\[ev\.TargetID\] != null\)
                        \{
                            _unitMap\[ev\.TargetID\]\.Heal\(ev\.Value\);
                        \}
                        break;

                    case CombatEventType\.TakeDamage:
                        if \(!string\.IsNullOrEmpty\(ev\.TargetID\) && _unitMap\.ContainsKey\(ev\.TargetID\) && _unitMap\[ev\.TargetID\] != null\)
                        \{
                            _unitMap\[ev\.TargetID\]\.TakeDamage\(ev\.Value, false\);
                        \}
                        break;

                    case CombatEventType\.TurnStart:
                        // Delay mỏng đầu mỗi turn để dễ thở
                        yield return new WaitForSeconds\(0\.2f \* _actionDelay\);
                        break;
                \}

                // Dừng chờ sau mỗi Action lớn \(Attack/Heal\)
                if \(ev\.EventType == CombatEventType\.Attack \|\| ev\.EventType == CombatEventType\.Heal\)
                \{
                    yield return new WaitForSeconds\(0\.6f \* _actionDelay\);
                \}
            \}
            
            // Diễn xong, đợi 1 giây rồi show kết quả
            yield return new WaitForSeconds\(1f\);
            ShowResultScreen\(\);
        \}'''

new_playback = '''        private IEnumerator PlaybackRoutine()
        {
            Debug.Log($"[CombatVisualizer] Replay started. EventLog count: {_cachedResult.EventLog.Count}");
            int actionIndex = 0;
            foreach (var ev in _cachedResult.EventLog)
            {
                actionIndex++;
                bool targetFound = !string.IsNullOrEmpty(ev.TargetID) && _unitMap.ContainsKey(ev.TargetID) && _unitMap[ev.TargetID] != null;
                
                if (!string.IsNullOrEmpty(ev.TargetID) && !targetFound)
                {
                    Debug.Log($"[CombatVisualizer] Action {actionIndex} ({ev.EventType}): targetId {ev.TargetID} missing! Available: {string.Join(", ", _unitMap.Keys)}");
                }

                switch (ev.EventType)
                {
                    case CombatEventType.Attack:
                        if (!string.IsNullOrEmpty(ev.SourceID) && _unitMap.ContainsKey(ev.SourceID) && _unitMap[ev.SourceID] != null)
                        {
                            Vector3 targetPos = _unitMap[ev.SourceID].transform.position + Vector3.right * 1f; 
                            if (targetFound) targetPos = _unitMap[ev.TargetID].transform.position;
                            yield return StartCoroutine(_unitMap[ev.SourceID].PlayAttackAnim(targetPos));
                        }
                        
                        if (targetFound)
                        {
                            ApplyHpChange(ev, actionIndex);
                        }
                        break;
                        
                    case CombatEventType.Heal:
                    case CombatEventType.TakeDamage:
                        if (targetFound)
                        {
                            ApplyHpChange(ev, actionIndex);
                        }
                        break;

                    case CombatEventType.TurnStart:
                        yield return new WaitForSeconds(0.2f * _actionDelay);
                        break;
                }

                if (ev.EventType == CombatEventType.Attack || ev.EventType == CombatEventType.Heal || ev.EventType == CombatEventType.TakeDamage)
                {
                    yield return new WaitForSeconds(0.6f * _actionDelay);
                }
            }
            
            Debug.Log("[CombatVisualizer] Show result after playback finished");
            yield return new WaitForSeconds(1f);
            ShowResultScreen();
        }

        private void ApplyHpChange(CombatEvent ev, int actionIndex)
        {
            string tId = ev.TargetID;
            int hpBefore = _visualCurrentHp.ContainsKey(tId) ? _visualCurrentHp[tId] : 0;
            int maxHp = _visualMaxHp.ContainsKey(tId) ? _visualMaxHp[tId] : 1;
            
            int newHp = hpBefore;
            if (ev.EventType == CombatEventType.Attack || ev.EventType == CombatEventType.TakeDamage)
            {
                newHp -= ev.Value;
            }
            else if (ev.EventType == CombatEventType.Heal)
            {
                newHp += ev.Value;
            }

            // Nếu EventLog có targetHpAfter thì ưu tiên
            if (ev.TargetHpAfter >= 0)
            {
                newHp = ev.TargetHpAfter;
            }

            newHp = Mathf.Clamp(newHp, 0, maxHp);
            _visualCurrentHp[tId] = newHp;
            
            float hpRatio = Mathf.Clamp01((float)newHp / maxHp);
            
            Debug.Log($"[CombatVisualizer] Action {actionIndex} ({ev.EventType}): source={ev.SourceID}, target={tId}, val={ev.Value}, found=True, hp before={hpBefore}, after={newHp}, max={maxHp}, ratio={hpRatio}, SetHp called");

            _unitMap[tId].SetHp(newHp, maxHp);
            
            if (ev.EventType == CombatEventType.Heal)
            {
                _unitMap[tId].Heal(ev.Value); // Heal visual (damage text)
            }
            else
            {
                _unitMap[tId].TakeDamage(hpBefore - newHp, ev.IsCrit); // TakeDamage visual (shake, text)
            }
        }'''
content = re.sub(old_playback, new_playback, content, flags=re.DOTALL)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Patched CombatVisualizerPanel.cs successfully.")

namespace LegendOfBlood.Combat
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using System.Collections;
    using System.Collections.Generic;

    public class CombatVisualizerPanel : MonoBehaviour
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
            if (skipButtonText != null) skipButtonText.text = global::LocalizationSystem.GetText("btn_skip");
            if (victoryTitleText != null) victoryTitleText.text = global::LocalizationSystem.GetText("combat_result_title");
            if (defeatTitleText != null) defeatTitleText.text = global::LocalizationSystem.GetText("combat_result_title");

            if (skipButton != null) skipButton.onClick.AddListener(SkipPlayback);
            if (x2SpeedButton != null) x2SpeedButton.onClick.AddListener(ToggleSpeed);
            if (closeVictoryButton != null) closeVictoryButton.onClick.AddListener(ClosePanel);
            if (closeDefeatButton != null) closeDefeatButton.onClick.AddListener(ClosePanel);
            
            ToggleSpeedDisplay();
        }

        public void PlayCombat(CombatResult result, List<HeroData> initialAllies, List<HeroData> initialEnemies, List<string> monsterIds = null)
        {
            _cachedResult = result;
            if (victoryScreen != null) victoryScreen.SetActive(false);
            if (defeatScreen != null) defeatScreen.SetActive(false);
            
            // Cập nhật title text nếu POI có truyền thông tin xuống, tạm thời để combat title UI xử lý.
            // Có thể truyền thêm tham số string poiName vào sau quá trình refactor tổng.

            ClearBoard();

            // Khởi tạo Dàn Diễn viên
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

                var unit = InstantiateUnitInSlot(h, true, slotIndex);
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

                    var unit = InstantiateUnitInSlot(e, false, slotIndex);
                    _unitMap[id] = unit;
                    enemyIndex++;
                }
            } 
            else if (monsterIds != null)
            {
                foreach (var monId in monsterIds)
                {
                    HeroData m = DataManager.Instance.GetMonsterByID(monId);
                    if (m != null)
                    {
                        string id = $"E_{m.profession}_{enemyIndex}";
                        int slotIndex = enemyIndex % 9;
                        if (_cachedResult.InitialPositions != null)
                        {
                            var pos = _cachedResult.InitialPositions.Find(p => p.InstanceID == id && !p.IsAlly);
                            if (!string.IsNullOrEmpty(pos.InstanceID)) slotIndex = pos.SlotIndex;
                        }

                        var unit = InstantiateUnitInSlot(m, false, slotIndex);
                        _unitMap[id] = unit;
                    }
                    enemyIndex++;
                }
            }

            // Bắt đầu nhại lại kịch bản
            if (_cachedResult.EventLog != null && _cachedResult.EventLog.Count > 0)
            {
                _playbackRoutine = StartCoroutine(PlaybackRoutine());
            }
            else
            {
                // Nếu không có eventlog (do lỗi gọi nhầm hàm cũ), hiển thị kết quả luôn
                ShowResultScreen();
            }
        }

        private BattleUnitUI InstantiateUnitInSlot(HeroData hData, bool isAlly, int slotIndex)
        {
            if (battleUnitPrefab == null)
            {
                Debug.LogWarning("[CombatVisualizerPanel] battleUnitPrefab is NULL! Tự động tạo một Prefab ảo (Fallback) để chống crash.");
                battleUnitPrefab = CreateFallbackPrefab();
            }

            Transform container = isAlly ? allyContainer : enemyContainer;
            Transform parentSlot = (container.childCount > slotIndex) ? container.GetChild(slotIndex) : container;

            GameObject go = Instantiate(battleUnitPrefab, parentSlot);
            BattleUnitUI unit = go.GetComponent<BattleUnitUI>();
            
            // Tìm hình ảnh
            Sprite portrait = hData.GetAvatarSprite();
            
            unit.Setup(portrait, (int)hData.GetFinalStats().hp, (int)hData.currentHp);
            return unit;
        }

        private GameObject CreateFallbackPrefab()
        {
            GameObject unitTemplate = new GameObject("BattleUnitTemplate_Fallback");
            unitTemplate.SetActive(false);
            RectTransform utRect = unitTemplate.AddComponent<RectTransform>();
            utRect.sizeDelta = new Vector2(200, 300);
            
            Image avatarImg = new GameObject("Avatar").AddComponent<Image>();
            avatarImg.transform.SetParent(unitTemplate.transform, false);
            avatarImg.rectTransform.anchorMin = Vector2.zero; avatarImg.rectTransform.anchorMax = Vector2.one;
            avatarImg.rectTransform.sizeDelta = Vector2.zero;
            avatarImg.color = Color.gray; 

            Slider hpSlider = new GameObject("HpSlider").AddComponent<Slider>();
            hpSlider.transform.SetParent(unitTemplate.transform, false);
            hpSlider.interactable = false;
            hpSlider.transition = Selectable.Transition.None;
            RectTransform hsRect = hpSlider.GetComponent<RectTransform>();
            hsRect.anchorMin = new Vector2(0, 1); hsRect.anchorMax = new Vector2(1, 1); 
            hsRect.pivot = new Vector2(0.5f, 0); hsRect.anchoredPosition = new Vector2(0, 10);
            hsRect.sizeDelta = new Vector2(0, 30);
            
            GameObject bgObj = new GameObject("Background"); bgObj.transform.SetParent(hpSlider.transform, false);
            Image bgImg = bgObj.AddComponent<Image>(); bgImg.color = Color.red;
            bgImg.rectTransform.anchorMin = Vector2.zero; bgImg.rectTransform.anchorMax = Vector2.one; bgImg.rectTransform.sizeDelta = Vector2.zero;
            
            GameObject fillArea = new GameObject("Fill Area"); fillArea.transform.SetParent(hpSlider.transform, false);
            RectTransform faRect = fillArea.AddComponent<RectTransform>(); faRect.anchorMin = Vector2.zero; faRect.anchorMax = Vector2.one; faRect.sizeDelta = Vector2.zero;
            
            GameObject fillObj = new GameObject("Fill"); fillObj.transform.SetParent(fillArea.transform, false);
            Image fillImg = fillObj.AddComponent<Image>(); fillImg.color = Color.green;
            fillImg.rectTransform.anchorMin = Vector2.zero; fillImg.rectTransform.anchorMax = Vector2.one; fillImg.rectTransform.sizeDelta = Vector2.zero;
            hpSlider.fillRect = fillImg.rectTransform;

            TextMeshProUGUI hpTxt = new GameObject("HpText").AddComponent<TextMeshProUGUI>();
            hpTxt.transform.SetParent(hpSlider.transform, false);
            hpTxt.rectTransform.anchorMin = Vector2.zero; hpTxt.rectTransform.anchorMax = Vector2.one;
            hpTxt.rectTransform.sizeDelta = Vector2.zero;
            hpTxt.text = "100/100"; hpTxt.fontSize = 20;
            hpTxt.color = Color.white; hpTxt.alignment = TextAlignmentOptions.Center;

            GameObject dmgCgObj = new GameObject("DamageTextContainer");
            dmgCgObj.transform.SetParent(unitTemplate.transform, false);
            RectTransform dmgRect = dmgCgObj.AddComponent<RectTransform>();
            CanvasGroup dmgCg = dmgCgObj.AddComponent<CanvasGroup>();
            dmgRect.anchorMin = new Vector2(0, 0.5f); dmgRect.anchorMax = new Vector2(1, 1.5f);
            
            TextMeshProUGUI dmgTxt = new GameObject("DamageText").AddComponent<TextMeshProUGUI>();
            dmgTxt.transform.SetParent(dmgCg.transform, false);
            dmgTxt.rectTransform.anchorMin = Vector2.zero; dmgTxt.rectTransform.anchorMax = Vector2.one;
            dmgTxt.rectTransform.sizeDelta = Vector2.zero;
            dmgTxt.text = "-999"; dmgTxt.fontSize = 40;
            dmgTxt.fontStyle = FontStyles.Bold; dmgTxt.alignment = TextAlignmentOptions.Center;

            BattleUnitUI unitScript = unitTemplate.AddComponent<BattleUnitUI>();
            unitScript.avatarImage = avatarImg;
            unitScript.hpSlider = hpSlider;
            unitScript.hpText = hpTxt;
            unitScript.damageTextCanvasGroup = dmgCg;
            unitScript.damageText = dmgTxt;

            return unitTemplate;
        }

        private void ClearBoard()
        {
            _unitMap.Clear();
            foreach (Transform t in allyContainer) Destroy(t.gameObject);
            foreach (Transform t in enemyContainer) Destroy(t.gameObject);
        }

        private IEnumerator PlaybackRoutine()
        {
            foreach (var ev in _cachedResult.EventLog)
            {
                switch (ev.EventType)
                {
                    case CombatEventType.Attack:
                        if (!string.IsNullOrEmpty(ev.SourceID) && _unitMap.ContainsKey(ev.SourceID))
                        {
                            Vector3 targetPos = _unitMap[ev.SourceID].transform.position + Vector3.right * 1f; // Nhích lên 1 chút nếu không có target
                            if (ev.TargetID != null && _unitMap.ContainsKey(ev.TargetID)) targetPos = _unitMap[ev.TargetID].transform.position;
                            
                            yield return StartCoroutine(_unitMap[ev.SourceID].PlayAttackAnim(targetPos));
                        }
                        
                        if (!string.IsNullOrEmpty(ev.TargetID) && _unitMap.ContainsKey(ev.TargetID))
                        {
                            _unitMap[ev.TargetID].TakeDamage(ev.Value, ev.IsCrit);
                        }
                        break;
                        
                    case CombatEventType.Heal:
                        if (!string.IsNullOrEmpty(ev.TargetID) && _unitMap.ContainsKey(ev.TargetID))
                        {
                            _unitMap[ev.TargetID].Heal(ev.Value);
                        }
                        break;

                    case CombatEventType.TakeDamage:
                        if (!string.IsNullOrEmpty(ev.TargetID) && _unitMap.ContainsKey(ev.TargetID))
                        {
                            _unitMap[ev.TargetID].TakeDamage(ev.Value, false);
                        }
                        break;

                    case CombatEventType.TurnStart:
                        // Delay mỏng đầu mỗi turn để dễ thở
                        yield return new WaitForSeconds(0.2f * _actionDelay);
                        break;
                }

                // Dừng chờ sau mỗi Action lớn (Attack/Heal)
                if (ev.EventType == CombatEventType.Attack || ev.EventType == CombatEventType.Heal)
                {
                    yield return new WaitForSeconds(0.6f * _actionDelay);
                }
            }
            
            // Diễn xong, đợi 1 giây rồi show kết quả
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
            
            // Khi bấm Skip, ép tất cả thanh máu về trạng thái cuối trận
            ApplyFinalState();
            ShowResultScreen();
        }

        private void ApplyFinalState()
        {
            // Trạng thái cuối nằm ở _cachedResult
            UpdateTeamState(_cachedResult.PlayerSurvivors, true, true);
            UpdateTeamState(_cachedResult.PlayerCasualties, true, false);
            UpdateTeamState(_cachedResult.EnemySurvivors, false, true);
            UpdateTeamState(_cachedResult.EnemyCasualties, false, false);
        }

        private void UpdateTeamState(List<HeroData> teamList, bool isAlly, bool isAlive)
        {
            if (teamList == null) return;
            string prefix = isAlly ? "P_" : "E_";
            
             // Để đơn giản khi Skip, ta chỉ làm mờ những con đã chết bằng cách ép máu = 0
            // và tắt GameObject đi luôn cho sạch thay vì để lỗi hình ảnh trắng bóc.
            if (!isAlive)
            {
                 foreach(var kv in _unitMap)
                 {
                     if (kv.Key.StartsWith(prefix))
                     {
                         // Nếu HeroData trong listCasualties trùng khớp Profession/ID với Key
                         foreach(var dead in teamList)
                         {
                             if (kv.Key.Contains(dead.profession.ToString()))
                             {
                                 kv.Value.TakeDamage(99999, false); // Nổ máu ảo để xám ảnh
                                 kv.Value.gameObject.SetActive(false); // Ẩn luôn unit đã chết khi skip cho sạch bàn cờ
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

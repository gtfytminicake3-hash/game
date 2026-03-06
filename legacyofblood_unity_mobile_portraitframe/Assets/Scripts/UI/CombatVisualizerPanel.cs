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
        public Button x2SpeedButton;
        public TextMeshProUGUI _speedText;
        public GameObject victoryScreen;
        public GameObject defeatScreen;
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
            
            ClearBoard();

            // Khởi tạo Dàn Diễn viên
            int allyIndex = 0;
            foreach (var h in initialAllies)
            {
                var unit = InstantiateUnit(h, true, allyIndex);
                string id = $"P_{h.profession}_{allyIndex}";
                _unitMap[id] = unit;
                allyIndex++;
            }

            int enemyIndex = 0;
            if (initialEnemies != null)
            {
                foreach (var e in initialEnemies)
                {
                    var unit = InstantiateUnit(e, false, enemyIndex);
                    string id = $"E_{e.profession}_{enemyIndex}";
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
                        var unit = InstantiateUnit(m, false, enemyIndex);
                        string id = $"E_{m.profession}_{enemyIndex}";
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

        private BattleUnitUI InstantiateUnit(HeroData hData, bool isAlly, int index)
        {
            Transform parent = isAlly ? allyContainer : enemyContainer;
            GameObject go = Instantiate(battleUnitPrefab, parent);
            BattleUnitUI unit = go.GetComponent<BattleUnitUI>();
            
            // Tìm hình ảnh
            Sprite portrait = hData.GetAvatarSprite();
            
            unit.Setup(portrait, (int)hData.GetFinalStats().hp, (int)hData.currentHp);
            return unit;
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
                            Vector3 targetPos = _unitMap[ev.SourceID].transform.localPosition + Vector3.right * 100f; // Nhích lên 1 chút
                            if (ev.TargetID != null && _unitMap.ContainsKey(ev.TargetID)) targetPos = _unitMap[ev.TargetID].transform.localPosition;
                            
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
            
            // Do CombatResult không lưu Index gốc, ta phải match qua Logic hoăc gán đè.
            // Để đơn giản khi Skip, ta chỉ làm mờ những con đã chết bằng cách ép máu = 0
            if (!isAlive)
            {
                 // Để khớp chính xác ta cần logic Match tốt hơn, 
                 // nhưng tạm thời lặp qua _unitMap theo phe và kiểm tra tên/profession.
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

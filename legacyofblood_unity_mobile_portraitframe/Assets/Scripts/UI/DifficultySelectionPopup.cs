using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

namespace LegendOfBlood
{
    public class DifficultySelectionPopup : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI currentCPText;
        
        [Header("Difficulty Buttons")]
        public Button btnNormal;
        public Button btnHard;
        public Button btnHell;
        public Button btnNightmare;

        private Action<ProceduralDifficulty> _onDifficultySelected;
        private int _currentCP;

        private void Start()
        {
            if (btnNormal != null) btnNormal.onClick.AddListener(() => OnSelect(ProceduralDifficulty.Normal, 0));
            if (btnHard != null) btnHard.onClick.AddListener(() => OnSelect(ProceduralDifficulty.Hard, 5000));
            if (btnHell != null) btnHell.onClick.AddListener(() => OnSelect(ProceduralDifficulty.Hell, 15000));
            if (btnNightmare != null) btnNightmare.onClick.AddListener(() => OnSelect(ProceduralDifficulty.Nightmare, 30000));
        }

        public void Show(string poiName, Action<ProceduralDifficulty> onDifficultySelected)
        {
            this.gameObject.SetActive(true);
            _onDifficultySelected = onDifficultySelected;

            if (titleText != null) 
                titleText.text = $"CHỌN ĐỘ KHÓ\n<color=#D4AF37>{poiName.ToUpper()}</color>";

            CalculateCurrentCP();

            if (currentCPText != null)
                currentCPText.text = $"Lực chiến đội hình: <color=#00ff00>{_currentCP}</color>";

            // Update button states visual if needed (optional custom logic)
            UpdateButtonState(btnNormal, 0);
            UpdateButtonState(btnHard, 5000);
            UpdateButtonState(btnHell, 15000);
            UpdateButtonState(btnNightmare, 30000);
        }

        private void CalculateCurrentCP()
        {
            _currentCP = 0;
            if (DataManager.Instance != null && DataManager.Instance.Player != null && DataManager.Instance.Player.Heroes != null)
            {
                var list = new List<HeroData>(DataManager.Instance.Player.Heroes);
                list.Sort((a,b) => b.GetCombatPower().CompareTo(a.GetCombatPower()));
                int limit = Mathf.Min(5, list.Count);
                for(int j = 0; j < limit; j++)
                {
                    _currentCP += list[j].GetCombatPower();
                }
            }
            if (_currentCP == 0) _currentCP = 4500; // Fallback
        }

        private void UpdateButtonState(Button btn, int requiredCP)
        {
            if (btn == null) return;
            bool unlocked = _currentCP >= requiredCP;
            
            // Tìm text bên trong button để update (vd: "Hard (CP: 5000)")
            var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null && !unlocked)
            {
                txt.text += $"\n<size=70%><color=red>Yêu cầu {_currentCP}/{requiredCP} CP</color></size>";
            }

            // Có thể làm mờ nút nhưng vẫn cho click để báo lỗi
            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.color = unlocked ? new Color(0.85f, 0.85f, 0.85f, 1f) : new Color(0.5f, 0.5f, 0.5f, 1f);
            }
        }

        private void OnSelect(ProceduralDifficulty difficulty, int requiredCP)
        {
            if (_currentCP < requiredCP)
            {
                ToastNotificationManager.Show($"Lực chiến quá thấp cho chế độ {difficulty}!");
                return;
            }

            this.gameObject.SetActive(false);
            _onDifficultySelected?.Invoke(difficulty);
        }
    }
}

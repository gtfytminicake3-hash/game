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
        private int _maxUnlockedDifficulty = 1;
        private string _currentPoiId;

        private void Start()
        {
            RegisterListeners();
        }

        public void RegisterListeners()
        {
            // Remove previous listeners to prevent duplicates
            if (btnNormal != null) { btnNormal.onClick.RemoveAllListeners(); btnNormal.onClick.AddListener(() => OnSelect(ProceduralDifficulty.Normal, 1)); }
            if (btnHard != null) { btnHard.onClick.RemoveAllListeners(); btnHard.onClick.AddListener(() => OnSelect(ProceduralDifficulty.Hard, 2)); }
            if (btnHell != null) { btnHell.onClick.RemoveAllListeners(); btnHell.onClick.AddListener(() => OnSelect(ProceduralDifficulty.Hell, 3)); }
            if (btnNightmare != null) { btnNightmare.onClick.RemoveAllListeners(); btnNightmare.onClick.AddListener(() => OnSelect(ProceduralDifficulty.Nightmare, 4)); }
        }

        public void Show(string poiName, Action<ProceduralDifficulty> onDifficultySelected, string poiId = null)
        {
            this.gameObject.SetActive(true);
            _onDifficultySelected = onDifficultySelected;
            _currentPoiId = poiId ?? poiName; // Fallback to poiName if poiId is not provided

            if (titleText != null) 
                titleText.text = $"CHỌN ĐỘ KHÓ\n<color=#D4AF37>{poiName.ToUpper()}</color>";

            CalculateUnlockedDifficulty();

            if (currentCPText != null)
                currentCPText.text = $"Mở khóa theo tiến độ chinh phục";

            // Update button states visual
            UpdateButtonState(btnNormal, 1);
            UpdateButtonState(btnHard, 2);
            UpdateButtonState(btnHell, 3);
            UpdateButtonState(btnNightmare, 4);
        }

        private void CalculateUnlockedDifficulty()
        {
            _maxUnlockedDifficulty = 1; // Luôn mở Normal

            if (DataManager.Instance != null && DataManager.Instance.Player != null)
            {
                if (DataManager.Instance.Player.ClearedDifficulties.TryGetValue(_currentPoiId, out int maxCleared))
                {
                    // Được chơi độ khó tiếp theo của độ khó cao nhất đã vượt qua
                    _maxUnlockedDifficulty = maxCleared + 1;
                }
            }
        }

        private void UpdateButtonState(Button btn, int difficultyLevel)
        {
            if (btn == null) return;
            bool unlocked = difficultyLevel <= _maxUnlockedDifficulty;
            
            var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                // Xoá warning cũ nếu có (bằng cách cắt bớt chuỗi ở ký tự \n)
                int idx = txt.text.IndexOf('\n');
                if (idx > 0) txt.text = txt.text.Substring(0, idx);

                if (!unlocked)
                {
                    txt.text += $"\n<size=70%><color=red>Cần hoàn thành chế độ trước</color></size>";
                }
            }

            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.color = unlocked ? new Color(0.85f, 0.85f, 0.85f, 1f) : new Color(0.5f, 0.5f, 0.5f, 1f);
            }
        }

        private void OnSelect(ProceduralDifficulty difficulty, int difficultyLevel)
        {
            if (difficultyLevel > _maxUnlockedDifficulty)
            {
                ToastNotificationManager.Show($"Chưa mở khoá! Hãy hoàn thành độ khó trước đó.");
                return;
            }

            this.gameObject.SetActive(false);
            _onDifficultySelected?.Invoke(difficulty);
        }
    }
}

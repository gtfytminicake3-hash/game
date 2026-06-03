using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LegendOfBlood
{
    public class BreedingPanel : UIPanel
    {
        [Header("Main Buttons")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button breedButton;

        [Header("Father Slot")]
        [SerializeField] private Button fatherButton;
        [SerializeField] private Image fatherIcon;
        [SerializeField] private TextMeshProUGUI fatherNameText;

        [Header("Mother Slot")]
        [SerializeField] private Button motherButton;
        [SerializeField] private Image motherIcon;
        [SerializeField] private TextMeshProUGUI motherNameText;

        [Header("Progress Area")]
        [SerializeField] private GameObject progressGroup;
        [SerializeField] private Image progressFill;
        [SerializeField] private TextMeshProUGUI progressText;

        private HeroData _father;
        private HeroData _mother;
        private bool _isPickingFather;
        private bool _isBreeding = false;

        private HeroPickerPanel _heroPicker;

        private void Awake()
        {
            PanelType = UIPanelType.Breeding;

            // Đăng ký Listener từ các biến [SerializeField] đã được kéo thả trong Inspector
            if (backButton != null) backButton.onClick.AddListener(ClosePanel);
            if (breedButton != null) breedButton.onClick.AddListener(OnBreedClicked);
            
            if (fatherButton != null) fatherButton.onClick.AddListener(() => OpenPicker(true));
            if (motherButton != null) motherButton.onClick.AddListener(() => OpenPicker(false));
        }

        private void OnEnable()
        {
            ResetUI();
        }

        private void OnDisable()
        {
            // (Đã loại bỏ Static Event)
        }

        private void ClosePanel()
        {
            if (_isBreeding) return; // Không đóng khi đang chạy process lai tạo
            GameManager.Instance.UIManager.HidePanel(UIPanelType.Breeding);
        }

        private void OpenPicker(bool isFather)
        {
            if (_isBreeding) return;

            _isPickingFather = isFather;
            if (_heroPicker == null) _heroPicker = FindAnyObjectByType<HeroPickerPanel>(FindObjectsInactive.Include);

            if (_heroPicker != null)
            {
                var allHeroes = DataManager.Instance.AllHeroes;
                List<HeroData> validHeroes;

                if (isFather)
                {
                    validHeroes = allHeroes.Where(h => h.gender == Gender.Male && !h.IsBusy()).ToList();
                    _heroPicker.Show(global::LocalizationSystem.GetText("breeding_select_father_title"), validHeroes, OnHeroPicked);
                }
                else
                {
                    validHeroes = allHeroes.Where(h => h.gender == Gender.Female && !h.IsBusy()).ToList();
                    _heroPicker.Show(global::LocalizationSystem.GetText("breeding_select_mother_title"), validHeroes, OnHeroPicked);
                }
            }
        }

        private void OnHeroPicked(HeroData pickedHero)
        {
            if (_isPickingFather)
            {
                _father = pickedHero;
                _mother = null; // Tránh lỗi tương thích cha con
            }
            else
            {
                _mother = pickedHero;
            }
            UpdateUI();
        }

        private int CalculateSuccessRate()
        {
            if (_father == null || _mother == null) return 0;

            int baseChance = 50;
            int levelBonus = (int)((_father.level + _mother.level) * 0.5f); // Thưởng cấp độ
            int cpBonus = (_father.GetCombatPower() + _mother.GetCombatPower()) / 200; // Thưởng lực chiến
            int levelDiff = Mathf.Abs(_father.level - _mother.level);
            int penalty = levelDiff * 2; // Phạt nếu chênh lệch cấp độ quá lớn

            int successRate = Mathf.Clamp(baseChance + levelBonus + cpBonus - penalty, 10, 100);
            return successRate;
        }

        private void UpdateUI()
        {
            // Update Father UI
            if (_father != null)
            {
                if (fatherNameText != null) fatherNameText.text = _father.heroName;
                if (fatherIcon != null) 
                {
                    fatherIcon.sprite = _father.GetAvatarSprite();
                    fatherIcon.color = Color.white;
                }
            }
            else
            {
                if (fatherNameText != null) fatherNameText.text = "Chọn Cha";
                if (fatherIcon != null) fatherIcon.color = Color.white;
            }

            // Update Mother UI
            if (_mother != null)
            {
                if (motherNameText != null) motherNameText.text = _mother.heroName;
                if (motherIcon != null)
                {
                    motherIcon.sprite = _mother.GetAvatarSprite();
                    motherIcon.color = Color.white;
                }
            }
            else
            {
                if (motherNameText != null) motherNameText.text = "Chọn Mẹ";
                if (motherIcon != null) motherIcon.color = Color.white;
            }

            // Logic trang trí cho Center Tube
            if (progressText != null && !_isBreeding)
            {
                if (_father != null && _mother != null)
                {
                    int successRate = CalculateSuccessRate();
                    string colorHex = successRate >= 80 ? "#00FF00" : (successRate >= 50 ? "#FFFF00" : "#FF0000");
                    progressText.text = $"Tỷ lệ thành công: <color={colorHex}>{successRate}%</color>\nSẵn sàng!";
                }
                else
                {
                    progressText.text = "Đang chờ Tướng...";
                }
            }
        }

        private void ResetUI()
        {
            _father = null;
            _mother = null;
            _isBreeding = false;
            
            // Không ẩn Tube để giao diện luôn đẹp mắt, chỉ reset bình chứa chất lỏng về 0
            if (progressGroup != null) progressGroup.SetActive(true);
            if (progressFill != null) progressFill.fillAmount = 0;
            if (progressText != null) progressText.text = "Sẵn sàng lai tạo";
            
            UpdateUI();
        }

        private void OnBreedClicked()
        {
            if (_isBreeding) return;

            if (_father == null)
            {
                GameManager.Instance.UINotificationManager.ShowNotification("Vui lòng chọn một Tướng Cha để bắt đầu lai tạo!");
                return;
            }
            if (_mother == null)
            {
                GameManager.Instance.UINotificationManager.ShowNotification("Vui lòng chọn một Tướng Mẹ để bắt đầu lai tạo!");
                return;
            }

            StartCoroutine(BreedingProcess());
        }

        private IEnumerator BreedingProcess()
        {
            _isBreeding = true;
            UpdateUI(); // Tắt nút

            if (progressGroup != null) progressGroup.SetActive(true);

            float t = 0f;
            float duration = 2.0f; // Nhá máy chờ 2 giây

            while (t < duration)
            {
                t += Time.deltaTime;
                float pct = t / duration;
                
                if (progressFill != null) progressFill.fillAmount = pct;
                if (progressText != null) progressText.text = $"Đang phân tích Gen... {(int)(pct * 100)}%";
                
                yield return null;
            }

            // Bắt đầu tính toán tỷ lệ thành công thực tế
            int successRate = CalculateSuccessRate();
            int roll = Random.Range(1, 101); // Quay số từ 1 đến 100

            if (roll <= successRate)
            {
                // THÀNH CÔNG
                BreedingOptions op = new BreedingOptions(); // Tạm thời dùng options mặc định
                List<HeroData> offsprings = GameManager.Instance.BreedingSystem.Breed(_father, _mother, op);

                if (offsprings != null && offsprings.Count > 0)
                {
                    HeroData baby = offsprings[0];
                    DataManager.Instance.AddHero(baby);
                    
                    GameManager.Instance.UINotificationManager.ShowNotification($"<color=#00FF00>Lai tạo thành công!</color>\nChào mừng {baby.heroName}");
                    
                    // Trả về mặc định
                    ResetUI();
                    
                    // Phát event mở cửa sổ xem thông tin con cái vừa sinh ra!
                    EventManager.TriggerEvent(GameEvents.OnHeroCardClicked, baby);
                }
                else
                {
                    GameManager.Instance.UINotificationManager.ShowNotification("<color=#FF0000>Lỗi Hệ Thống:</color> Không thể sinh ra Tướng mới!");
                    ResetUI();
                }
            }
            else
            {
                // THẤT BẠI
                GameManager.Instance.UINotificationManager.ShowNotification("<color=#FF0000>Lai tạo thất bại!</color> Các đoạn Gen không tương thích.");
                ResetUI();
            }
        }
    }
}

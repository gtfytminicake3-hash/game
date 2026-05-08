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

            if (backButton != null) backButton.onClick.AddListener(ClosePanel);
            if (breedButton != null) breedButton.onClick.AddListener(OnBreedClicked);
            
            if (fatherButton != null) fatherButton.onClick.AddListener(() => OpenPicker(true));
            if (motherButton != null) motherButton.onClick.AddListener(() => OpenPicker(false));

            AutoHookMissingReferences();
        }

        private void OnEnable()
        {
            HeroPickerPanel.OnHeroPicked += OnHeroPicked;
            ResetUI();
        }

        private void OnDisable()
        {
            HeroPickerPanel.OnHeroPicked -= OnHeroPicked;
        }

        private void AutoHookMissingReferences()
        {
            // TÌM CÁC NÚT BẰNG ĐƯỜNG DẪN HOẶC TÊN CHÍNH XÁC (Tránh lỗi nhảy UI lung tung)
            var allTransforms = GetComponentsInChildren<Transform>(true);
            
            // Tìm Nút Back
            if (backButton == null)
            {
                var back = allTransforms.FirstOrDefault(t => t.name == "CloseButton");
                if (back != null) backButton = back.GetComponent<Button>();
            }

            // Tìm Nút Lai Tạo (Bây giờ lấy thẳng tên chuẩn của dự án)
            if (breedButton == null)
            {
                var breedBtnObj = allTransforms.FirstOrDefault(t => t.name == "StartBreedingButton" || (t.name.Contains("Breeding") && t.name.Contains("Button")));
                if (breedBtnObj != null) 
                {
                    breedButton = breedBtnObj.GetComponent<Button>();
                    if (breedButton == null) breedButton = breedBtnObj.gameObject.AddComponent<Button>();
                }

                // Nếu vẫn chưa thấy thì fallback tìm theo text "LAI TẠO" hoặc "START"
                if (breedButton == null)
                {
                    var breedText = GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(t => (t.text.ToUpper().Contains("LAI TẠO") || t.text.ToUpper().Contains("START")) && t.transform.parent.name != "TopHeader" && t.transform.parent.name != "Panel_Breeding");
                    if (breedText != null && breedText.transform.parent != null) 
                    {
                        breedButton = breedText.transform.parent.GetComponent<Button>();
                        if (breedButton == null) breedButton = breedText.transform.parent.gameObject.AddComponent<Button>();
                    }
                }
            }

            // Tìm Ô Chọn Cha (LeftPedestalGroup hoặc nhánh chứa LeftCard)
            if (fatherButton == null)
            {
                var leftGroup = allTransforms.FirstOrDefault(t => t.name == "LeftPedestalGroup");
                if (leftGroup != null) 
                {
                    fatherButton = leftGroup.GetComponent<Button>();
                    if (fatherButton == null) fatherButton = leftGroup.gameObject.AddComponent<Button>();

                    // Cố gắng tìm Card bên trong để kích hoạt
                    Transform card = leftGroup.Find("LeftCard_Knight");
                    if (card != null) card.gameObject.SetActive(true);

                    // Tìm chính xác NameText và Portrait bên trong nó
                    fatherNameText = leftGroup.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(t => t.name == "NameText");
                    fatherIcon = leftGroup.GetComponentsInChildren<Image>(true).FirstOrDefault(img => img.name == "Portrait");
                }
            }

            // Tìm Ô Chọn Mẹ (Dự đoán tên là RightPedestalGroup)
            if (motherButton == null)
            {
                var rightGroup = allTransforms.FirstOrDefault(t => t.name == "RightPedestalGroup" || t.name.Contains("RightPedestal"));
                if (rightGroup != null) 
                {
                    motherButton = rightGroup.GetComponent<Button>();
                    if (motherButton == null) motherButton = rightGroup.gameObject.AddComponent<Button>();

                    // Cố gắng tìm Card bên trong để kích hoạt (có thể là RightCard_Mage)
                    Transform card = null;
                    foreach(Transform child in rightGroup)
                    {
                        if (child.name.Contains("Card")) card = child;   
                    }
                    if (card != null) card.gameObject.SetActive(true);

                    motherNameText = rightGroup.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(t => t.name == "NameText");
                    motherIcon = rightGroup.GetComponentsInChildren<Image>(true).FirstOrDefault(img => img.name == "Portrait");
                }
            }

            // Fallback nếu không thấy theo tên thì dùng cái script cũ để vét
            if (backButton == null || breedButton == null || fatherButton == null || motherButton == null)
            {
                Button[] allButtons = GetComponentsInChildren<Button>(true);
                foreach (var btn in allButtons)
                {
                    var txt = btn.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (txt != null && !string.IsNullOrWhiteSpace(txt.text))
                    {
                        string upperText = txt.text.ToUpper();
                        if (upperText == "<" && backButton == null) backButton = btn;
                        else if ((upperText.Contains("LAI") || upperText.Contains("BREED")) && breedButton == null) breedButton = btn;
                    }
                }
            }

            // Tìm Progress Area (CenterTubeGroup hoặc ProgressGroup)
            if (progressGroup == null)
            {
                var tube = allTransforms.FirstOrDefault(t => t.name.Contains("Tube") || t.name.Contains("Progress"));
                if (tube != null)
                {
                    progressGroup = tube.gameObject;
                    progressFill = tube.GetComponentsInChildren<Image>(true).FirstOrDefault(img => img.type == Image.Type.Filled || img.name.Contains("Fill"));
                    progressText = tube.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(t => t.name.Contains("Text") || t.text.Contains("%"));
                }
            }

            // Cắm dây sự kiện
            if (backButton != null) 
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(ClosePanel);
            }
            if (breedButton != null) 
            {
                breedButton.onClick.RemoveAllListeners();
                breedButton.onClick.AddListener(OnBreedClicked);
            }
            if (fatherButton != null)
            {
                fatherButton.onClick.RemoveAllListeners();
                fatherButton.onClick.AddListener(() => OpenPicker(true));
            }
            if (motherButton != null)
            {
                motherButton.onClick.RemoveAllListeners();
                motherButton.onClick.AddListener(() => OpenPicker(false));
            }
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
                    _heroPicker.Show(global::LocalizationSystem.GetText("breeding_select_father_title"), validHeroes);
                }
                else
                {
                    validHeroes = allHeroes.Where(h => h.gender == Gender.Female && !h.IsBusy()).ToList();
                    _heroPicker.Show(global::LocalizationSystem.GetText("breeding_select_mother_title"), validHeroes);
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

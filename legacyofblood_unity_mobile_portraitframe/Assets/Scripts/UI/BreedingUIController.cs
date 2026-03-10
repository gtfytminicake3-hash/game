namespace LegendOfBlood
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    /// <summary>
    /// Điều khiển màn hình Lai tạo, quản lý việc chọn cha, mẹ và bắt đầu quá trình lai tạo.
    /// </summary>
    public class BreedingUIController : MonoBehaviour
    {
        [Header("Panel Titles")]
        [SerializeField] private TextMeshProUGUI panelTitleText;
        
        [Header("Slot References")]
        [SerializeField] private GameObject fatherSlot;
        [SerializeField] private Button selectFatherButton;
        [SerializeField] private HeroCard fatherCard;

        [SerializeField] private GameObject motherSlot;
        [SerializeField] private Button selectMotherButton;
        [SerializeField] private HeroCard motherCard;
        
        [Header("Action Buttons")]
        [SerializeField] private Button breedButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button confirmResultButton; // Nút mới: "Xác nhận"
        
        [Header("Upgrade Feature")]
        [SerializeField] private Button upgradeBuildingButton;
        [SerializeField] private string associatedBuildingId = "BreedingPen";
        
        [Header("Area Groups")]
        [SerializeField] private GameObject selectionArea; // Tham chiếu đến nhóm chọn Cha/Mẹ
        [SerializeField] private GameObject resultArea;    // Tham chiếu đến nhóm kết quả

        [Header("Item Usage")]
        [SerializeField] private Toggle useMutationPotionToggle; // UI Toggle cho Thuốc Đột Biến

        [Header("Result Area References")]
        [SerializeField] private HeroCard newHeroCard_Result; // Thẻ bài trong khu vực kết quả
        [SerializeField] private TextMeshProUGUI hpText_Result;
        [SerializeField] private TextMeshProUGUI atkText_Result;
        [SerializeField] private TextMeshProUGUI defText_Result;
        [SerializeField] private TextMeshProUGUI spdText_Result;
        [SerializeField] private TextMeshProUGUI potentialText_Result;

        // Tham chiếu đến HeroPickerPanel trong scene để gọi nó
        [SerializeField] private HeroPickerPanel heroPickerPanel;

        // Lưu trữ dữ liệu của cha và mẹ đã chọn
        private HeroData _selectedFather;
        private HeroData _selectedMother;

        // Cờ để biết HeroPickerPanel được mở để chọn ai
        private bool _isPickingFather = false;

        // Enum để định nghĩa các trạng thái
        private enum BreedingState { Selection, Result }
        
        #region Unity Lifecycle & Event Subscription

        private void Awake()
        {
            if (selectFatherButton != null)
            {
                selectFatherButton.onClick.AddListener(OnSelectFatherClicked);
                var t = selectFatherButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (t != null) t.text = global::LocalizationSystem.GetText("btn_select_father");
            }
            if (selectMotherButton != null)
            {
                selectMotherButton.onClick.AddListener(OnSelectMotherClicked);
                var t = selectMotherButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (t != null) t.text = global::LocalizationSystem.GetText("btn_select_mother");
            }
            if (breedButton != null)
            {
                breedButton.onClick.AddListener(OnBreedClicked);
                var t = breedButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (t != null) t.text = global::LocalizationSystem.GetText("btn_breed");
            }
            if (panelTitleText != null) panelTitleText.text = global::LocalizationSystem.GetText("panel_title_breeding");

            if (closeButton != null) closeButton.onClick.AddListener(CloseBreedingPanel);
            if (confirmResultButton != null)
            {
                confirmResultButton.onClick.AddListener(OnConfirmResultClicked);
                var t = confirmResultButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (t != null) t.text = global::LocalizationSystem.GetText("btn_confirm");
            }
            if (upgradeBuildingButton != null) 
            {
                upgradeBuildingButton.onClick.AddListener(OnUpgradeBuildingClicked);
                var upgTxt = upgradeBuildingButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (upgTxt != null) upgTxt.text = global::LocalizationSystem.GetText("btn_upgrade");
            }

            if (heroPickerPanel == null)
            {
                heroPickerPanel = FindAnyObjectByType<HeroPickerPanel>(FindObjectsInactive.Include);
                if (heroPickerPanel == null)
                {
                    Debug.LogError("[BreedingUIController] CẢNH BÁO: Không tìm thấy HeroPickerPanel nào trong Scene! Trò chơi sẽ bị lỗi nếu gọi bảng chọn tướng.");
                }
            }
        }

        private void OnEnable()
        {
            // Bắt đầu lắng nghe sự kiện từ HeroPickerPanel
            HeroPickerPanel.OnHeroPicked += OnHeroPicked;
            SwitchToState(BreedingState.Selection); // Khi panel được mở, luôn đảm bảo nó ở trạng thái chọn lựa
        }

        private void OnDisable()
        {
            // Ngừng lắng nghe để tránh lỗi
            HeroPickerPanel.OnHeroPicked -= OnHeroPicked;
        }

        #endregion

        #region UI Logic

        /// <summary>
        /// Được gọi khi người dùng chọn một hero từ HeroPickerPanel.
        /// </summary>
        private void OnHeroPicked(HeroData pickedHero)
        {
            if (_isPickingFather)
            {
                _selectedFather = pickedHero;
                // Nếu chọn cha mới, phải xóa mẹ đã chọn vì danh sách mẹ hợp lệ đã thay đổi
                _selectedMother = null; 
            }
            else
            {
                _selectedMother = pickedHero;
            }
            UpdateUI();
        }
        private void CloseBreedingPanel()
        {
            // Thay vì gọi ShowPanel, hãy gọi HidePanel hoặc GoBack
            GameManager.Instance.UIManager.HidePanel(UIPanelType.Breeding);
            // Hoặc nếu bạn muốn quay về màn hình trước đó một cách linh hoạt:
            // GameManager.Instance.UIManager.GoBack();
        }

        private void OnUpgradeBuildingClicked()
        {
            GameManager.Instance.UIManager.ShowPanel(UIPanelType.BuildingUpgrade, true);
            var upgradePanel = GameManager.Instance.UIManager.GetPanel<BuildingUpgradePanel>(UIPanelType.BuildingUpgrade);
            if (upgradePanel != null && !string.IsNullOrEmpty(associatedBuildingId))
            {
                upgradePanel.Setup(associatedBuildingId);
            }
            else
            {
                Debug.LogWarning($"[BreedingUIController] Cannot open BuildingUpgradePanel! upgradePanel null? {upgradePanel == null}, associatedBuildingId empty? {string.IsNullOrEmpty(associatedBuildingId)}");
            }
        }
        private void OnSelectFatherClicked()
        {
            Debug.Log("--- Bắt đầu quá trình Chọn Cha ---"); // Log 1

    _isPickingFather = true;
    
    var allHeroes = DataManager.Instance.AllHeroes;
    Debug.Log($"Tổng số hero trong DataManager: {allHeroes.Count}"); // Log 2

    var validFathers = allHeroes.Where(h => h.gender == Gender.Male && !h.IsBusy()).ToList();
    
    Debug.Log($"Tìm thấy {validFathers.Count} hero Nam hợp lệ."); // Log 3
    foreach(var father in validFathers)
    {
        Debug.Log($" > {father.heroName}, IsBusy: {father.IsBusy()}"); // Log 4
    }
    
    if (heroPickerPanel != null)
    {
        Debug.Log("heroPickerPanel hợp lệ. Đang gọi hàm Show()..."); // Log 5
        heroPickerPanel.Show(global::LocalizationSystem.GetText("breeding_select_father_title"), validFathers);
    }
    else
    {
        heroPickerPanel = FindAnyObjectByType<HeroPickerPanel>(FindObjectsInactive.Include);
        if (heroPickerPanel != null)
        {
            heroPickerPanel.Show(global::LocalizationSystem.GetText("breeding_select_father_title"), validFathers);
        }
        else
        {
            Debug.LogError("THAM CHIẾU heroPickerPanel BỊ NULL! Xin hãy gắn HeroPickerPanel vào Inspector của BreedingUIController."); // Log lỗi
        }
    }
        }

        private void OnSelectMotherClicked()
        {
            if (_selectedFather == null)
            {
                GameManager.Instance.UINotificationManager.ShowNotification(global::LocalizationSystem.GetText("breeding_error_select_father_first"));
                return;
            }

            _isPickingFather = false;

            // Lấy danh sách tất cả hero Nữ, còn rảnh
            var allHeroes = DataManager.Instance.AllHeroes;
            var validMothers = allHeroes.Where(h => h.gender == Gender.Female && !h.IsBusy()).ToList();

            if (heroPickerPanel == null) 
                heroPickerPanel = FindAnyObjectByType<HeroPickerPanel>(FindObjectsInactive.Include);

            if (heroPickerPanel != null)
            {
                heroPickerPanel.Show(global::LocalizationSystem.GetText("breeding_select_mother_title"), validMothers);
            }
        }

        /// <summary>
        /// Cập nhật giao diện dựa trên cha và mẹ đã chọn.
        /// </summary>
        private void UpdateUI()
        {
            // Cập nhật ô Cha
            if (_selectedFather != null)
            {
                fatherCard.gameObject.SetActive(true);
                fatherCard.Setup(_selectedFather);
                selectFatherButton.GetComponentInChildren<TextMeshProUGUI>().text = global::LocalizationSystem.GetText("breeding_change_father");
            }
            else
            {
                fatherCard.gameObject.SetActive(false);
                selectFatherButton.GetComponentInChildren<TextMeshProUGUI>().text = global::LocalizationSystem.GetText("breeding_select_father");
            }
            
            // Cập nhật ô Mẹ
            if (_selectedMother != null)
            {
                motherCard.gameObject.SetActive(true);
                motherCard.Setup(_selectedMother);
                selectMotherButton.GetComponentInChildren<TextMeshProUGUI>().text = global::LocalizationSystem.GetText("breeding_change_mother");
            }
            else
            {
                motherCard.gameObject.SetActive(false);
                selectMotherButton.GetComponentInChildren<TextMeshProUGUI>().text = global::LocalizationSystem.GetText("breeding_select_mother");
            }

            // Kích hoạt nút Lai tạo chỉ khi đã chọn đủ cả hai
            breedButton.interactable = (_selectedFather != null && _selectedMother != null);
        }

        private void ResetSelection()
        {
            _selectedFather = null;
            _selectedMother = null;
            UpdateUI();
        }

        #endregion

        #region Breeding Logic

        private void OnBreedClicked()
        {
            if (_selectedFather == null || _selectedMother == null)
            {
                Debug.LogError("Nút Lai tạo được nhấn khi chưa chọn đủ cha mẹ!");
                return;
            }

            // --- KIỂM TRA LỖI ---
            if (GameManager.Instance == null)
            {
                Debug.LogError("LỖI NGHIÊM TRỌNG: GameManager.Instance đang bị null! Hãy kiểm tra xem có GameObject GameManager trong scene không.");
                return;
            }
            if (GameManager.Instance.BreedingSystem == null)
            {
                Debug.LogError("LỖI NGHIÊM TRỌNG: GameManager.Instance.BreedingSystem đang bị null! Hãy kiểm tra xem component BreedingSystem đã được gán/thêm vào GameManager chưa.");
                return;
            }
            // --- KẾT THÚC KIỂM TRA ---

            BreedingOptions options = new BreedingOptions();
            
            // Xử lý sử dụng Thuốc Đột Biến
            if (useMutationPotionToggle != null && useMutationPotionToggle.isOn)
            {
                // Kiểm tra và trừ vật phẩm
                if (GameManager.Instance.InventoryManager.UseItem("IT_MUTATION_POTION", 1))
                {
                    options.UseMutationPotion = true;
                    Debug.Log("Đã sử dụng 1 Thuốc Đột Biến cho quá trình lai tạo.");
                }
                else
                {
                    GameManager.Instance.UINotificationManager.ShowNotification(global::LocalizationSystem.GetText("notification_not_enough_mutation_potion"));
                    return; // Ngừng lai tạo nếu chọn dùng nhưng không có đồ
                }
            }

            // Gọi hệ thống logic để thực hiện lai tạo
            BreedingSystem breedingSystem = GameManager.Instance.BreedingSystem;
            List<HeroData> offspringList = breedingSystem.Breed(_selectedFather, _selectedMother, options);

            if (offspringList.Count > 0)
            {                
                // Thêm các hero con vào DataManager
                foreach (var offspring in offspringList)
                {
                    DataManager.Instance.AddHero(offspring);
                }

                // Chuyển sang trạng thái kết quả và hiển thị thông tin
                SwitchToState(BreedingState.Result);
                DisplayResult(offspringList[0]); // Hiển thị đứa con đầu tiên
            }
            else
            {
                GameManager.Instance.UINotificationManager.ShowNotification(global::LocalizationSystem.GetText("breeding_failed"));
            }
        }

        /// <summary>
        /// Hàm mới để hiển thị kết quả lai tạo.
        /// </summary>
        private void DisplayResult(HeroData newHero)
        {
            newHeroCard_Result.Setup(newHero);
            hpText_Result.text = string.Format(global::LocalizationSystem.GetText("stats_hp_format"), newHero.baseStats.hp);
            atkText_Result.text = string.Format(global::LocalizationSystem.GetText("stats_atk_format"), newHero.baseStats.atk);
            defText_Result.text = string.Format(global::LocalizationSystem.GetText("stats_def_format"), newHero.baseStats.def);
            spdText_Result.text = string.Format(global::LocalizationSystem.GetText("stats_spd_format"), newHero.baseStats.spd);
            potentialText_Result.text = string.Format(global::LocalizationSystem.GetText("stats_potential_format"), newHero.potential);
        }

        /// <summary>
        /// Được gọi khi nhấn nút "Xác nhận" trên màn hình kết quả.
        /// </summary>
        private void OnConfirmResultClicked()
        {
            // Sau khi xác nhận, quay về Làng (hoặc Doanh trại)
            // và đảm bảo lần sau mở panel sẽ ở trạng thái chọn lựa
            SwitchToState(BreedingState.Selection);
            GameManager.Instance.UIManager.BackToVillageView();
        }

        /// <summary>
        /// Chuyển đổi giữa các trạng thái của panel (chọn lựa hoặc kết quả).
        /// </summary>
        private void SwitchToState(BreedingState state)
        {
            if (state == BreedingState.Selection)
            {
                selectionArea.SetActive(true);
                resultArea.SetActive(false);
                breedButton.gameObject.SetActive(true);
                closeButton.gameObject.SetActive(true);
                confirmResultButton.gameObject.SetActive(false);
                ResetSelection(); // Reset lựa chọn Cha/Mẹ
            }
            else // Result State
            {
                selectionArea.SetActive(false);
                resultArea.SetActive(true);
                breedButton.gameObject.SetActive(false);
                closeButton.gameObject.SetActive(false);
                confirmResultButton.gameObject.SetActive(true);
            }
        }
        #endregion
    }
}
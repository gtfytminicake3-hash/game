namespace LegendOfBlood
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    /// <summary>
    /// Một panel đa dụng để người chơi chọn một đội hình với số lượng hero định trước.
    /// </summary>
    public class SquadSelectionPanel : UIPanel
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Transform squadSlotsContainer;
        [SerializeField] private Transform availableListContainer;
        [SerializeField] private TextMeshProUGUI totalCpText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button closeButton;

        [Header("Prefabs")]
        [SerializeField] private GameObject squadSlotPrefab; // Prefab cho một ô trong đội hình
        [SerializeField] private GameObject heroCardPrefab;  // Prefab cho một hero trong danh sách có sẵn

        // --- State Variables ---
        private Action<List<string>, int> _onConfirmCallback; // Hàm callback để trả kết quả về cho người gọi
        private int _currentDifficulty = 1;
        private GameObject _difficultyPanel;
        private TextMeshProUGUI _difficultyLabel;

        private List<HeroData> _availableHeroes;
        private Profession _requiredProfession = Profession.None;
        private HeroData[] _selectedHeroes; // Dùng mảng vì kích thước cố định
        private List<SquadSlotCard> _squadSlotCards = new List<SquadSlotCard>();
        private List<GameObject> _availableHeroCards = new List<GameObject>();

        private void Awake()
        {
            PanelType = UIPanelType.SquadSelection;
            AutoHook();
        }

        private void AutoHook()
        {
            if (titleText == null) titleText = transform.Find("Title")?.GetComponent<TextMeshProUGUI>() ?? transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>() ?? transform.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(t => t.name.Contains("Title"));
            if (squadSlotsContainer == null) squadSlotsContainer = transform.Find("SquadSlotsContainer") ?? transform.Find("SquadSlots");
            if (availableListContainer == null) availableListContainer = transform.Find("AvailableListScrollView/Viewport/Content") ?? transform.Find("AvailableListContainer") ?? transform.Find("Scroll View/Viewport/Content");
            if (totalCpText == null) totalCpText = transform.Find("T?ng Cp")?.GetComponent<TextMeshProUGUI>() ?? transform.Find("TotalCPText")?.GetComponent<TextMeshProUGUI>() ?? transform.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(t => t.name.Contains("CP"));
            if (confirmButton == null) confirmButton = transform.Find("ActionsArea/confirm")?.GetComponent<Button>() ?? transform.Find("Btn_Confirm")?.GetComponent<Button>() ?? transform.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name.Contains("Confirm"));
            if (closeButton == null) closeButton = transform.Find("ActionsArea/close")?.GetComponent<Button>() ?? transform.Find("Btn_Close")?.GetComponent<Button>() ?? transform.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name.Contains("Close"));

            if (confirmButton != null) Debug.Log($"[SquadSelectionPanel] Auto-hooked confirmButton: {confirmButton.name}");
        }

        #region Unity Lifecycle

        protected override void Start()
        {
            base.Start();
            if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirmClicked);
            if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
        }
        
        #endregion

        #region Public API

        /// <summary>
        /// Mở và thiết lập panel để bắt đầu quá trình chọn đội hình.
        /// </summary>
        /// <param name="title">Tiêu đề của panel.</param>
        /// <param name="availableHeroes">Danh sách hero người chơi có thể chọn.</param>
        /// <param name="squadSize">Số lượng hero cần chọn.</param>
        /// <param name="onConfirm">Hàm callback sẽ được gọi khi người chơi xác nhận.</param>
        /// <param name="requiredProfession">Nghề nghiệp yêu cầu (tùy chọn). Nếu khác None, chỉ hero có nghề này mới được chọn.</param>
        public void Show(string title, List<HeroData> availableHeroes, int squadSize, Action<List<string>, int> onConfirm, Profession requiredProfession = Profession.None, int initialDifficulty = 1)
        {
            GameManager.Instance.UIManager.ShowPanel(UIPanelType.SquadSelection, false);
            transform.SetAsLastSibling();
            
            titleText.text = title;
            _availableHeroes = availableHeroes;
            _onConfirmCallback = onConfirm;
            _requiredProfession = requiredProfession;
            _currentDifficulty = initialDifficulty;

            _selectedHeroes = new HeroData[squadSize]; // Khởi tạo mảng với kích thước yêu cầu

            // Tạo các ô đội hình
            CreateSquadSlots(squadSize);
            RefreshAvailableList();
            UpdateUIState();
        }

        #endregion

        #region Internal Logic

        private void CreateSquadSlots(int count)
        {
            // Dọn dẹp ô cũ
            if (_squadSlotCards != null)
            {
                foreach (var slot in _squadSlotCards)
                {
                    if (slot != null && slot.gameObject != null)
                    {
                        Destroy(slot.gameObject);
                    }
                }
                _squadSlotCards.Clear();
            }
            else
            {
                _squadSlotCards = new List<SquadSlotCard>();
            }
            
            for (int i = 0; i < count; i++)
            {
                GameObject slotInstance = Instantiate(squadSlotPrefab, squadSlotsContainer);
                SquadSlotCard slotCard = slotInstance.GetComponent<SquadSlotCard>();
                slotCard.Setup(i, this); // Truyền index và tham chiếu của panel
                _squadSlotCards.Add(slotCard);
            }
        }
        
        private void RefreshAvailableList()
        {
            // Dọn dẹp danh sách cũ (bao gồm cả các slot dummy có sẵn trong Prefab)
            if (availableListContainer != null)
            {
                foreach (Transform child in availableListContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            _availableHeroCards = new List<GameObject>();

            // Lọc ra những hero chưa được chọn và hợp lệ
            var heroesToShow = _availableHeroes.Where(h =>
            {
                bool isNotSelected = !_selectedHeroes.Contains(h);
                bool meetsProfessionRequirement = (_requiredProfession == Profession.None) || (h.profession == _requiredProfession);
                // Chỉ những hero đã trưởng thành (có nghề) mới được tham gia
                return isNotSelected && h.isMature && meetsProfessionRequirement;
            }).ToList();

            heroesToShow = heroesToShow.OrderByDescending(h => h.GetCombatPower()).ToList();
            
            foreach (var hero in heroesToShow)
            {
                GameObject cardInstance = Instantiate(heroCardPrefab, availableListContainer);
                SquadSelectionHeroCard selectionCardScript = cardInstance.GetComponent<SquadSelectionHeroCard>();
                
                if (selectionCardScript != null)
                {
                    // Ưu tiên dùng Script thẻ chọn quân đặc thù nếu Prefab đã được gắn
                    selectionCardScript.Setup(hero, this);
                }
                else
                {
                    // Fallback tương thích ngược: Dùng HeroCard thường và chèn Nút vào cả thẻ
                    HeroCard heroCardScript = cardInstance.GetComponent<HeroCard>();
                    heroCardScript.Setup(hero);
                    
                    Button button = cardInstance.GetComponent<Button>();
                    button.onClick.RemoveAllListeners(); // Xóa listener cũ để click KHÔNG mở bảng InfoPanel nữa
                    button.onClick.AddListener(() => AddHeroToSquad(hero));
                }
                
                _availableHeroCards.Add(cardInstance);
            }
        }

        private void UpdateUIState()
        {
            // Cập nhật tổng CP
            int totalCp = _selectedHeroes.Where(h => h != null).Sum(h => h.GetCombatPower());
            totalCpText.text = string.Format(LocalizationSystem.GetText("total_cp_format"), totalCp);

            // Cập nhật trạng thái nút xác nhận
            // Nút chỉ được bật khi đội hình đã đầy
            int selectedCount = _selectedHeroes.Count(h => h != null);
            confirmButton.interactable = (selectedCount > 0);
        }

        // Được gọi từ các card trong danh sách có sẵn
        public void AddHeroToSquad(HeroData hero)
        {
            // Tìm ô trống đầu tiên
            for (int i = 0; i < _selectedHeroes.Length; i++)
            {
                if (_selectedHeroes[i] == null)
                {
                    _selectedHeroes[i] = hero;
                    _squadSlotCards[i].SetHero(hero); // Cập nhật UI cho ô
                    break;
                }
            }
            RefreshAvailableList();
            UpdateUIState();
        }

        public void SwapHeroes(int indexA, int indexB)
        {
            if (indexA < 0 || indexA >= _selectedHeroes.Length || indexB < 0 || indexB >= _selectedHeroes.Length) return;

            // Đổi chỗ trong mảng Data
            HeroData temp = _selectedHeroes[indexA];
            _selectedHeroes[indexA] = _selectedHeroes[indexB];
            _selectedHeroes[indexB] = temp;

            // Cập nhật lại 2 ô UI
            if (_selectedHeroes[indexA] != null) 
                _squadSlotCards[indexA].SetHero(_selectedHeroes[indexA]);
            else 
                _squadSlotCards[indexA].SetEmpty();

            if (_selectedHeroes[indexB] != null) 
                _squadSlotCards[indexB].SetHero(_selectedHeroes[indexB]);
            else 
                _squadSlotCards[indexB].SetEmpty();
                
            // Không cần RefreshAvailableList() vì danh sách tổng Hero được chọn không đổi
            UpdateUIState();
        }

        // Được gọi từ các SquadSlotCard
        public void RemoveHeroFromSquad(int slotIndex)
        {
            _selectedHeroes[slotIndex] = null;
            _squadSlotCards[slotIndex].SetEmpty(); // Cập nhật UI cho ô
            RefreshAvailableList();
            UpdateUIState();
        }

        private void OnConfirmClicked()
        {
            var selectedHeroIDs = _selectedHeroes.Where(h => h != null)
                                                 .Select(h => h.id)
                                                 .ToList();
            
            // Đóng bảng hiện tại (SquadSelection) TRƯỚC khi gọi callback
            // Điều này giúp UIManager.GoBack() chạy trước, tránh việc đè lên panel mà callback sắp mở
            ClosePanel();

            // Gọi callback để trả kết quả về cho người đã mở panel này
            _onConfirmCallback?.Invoke(selectedHeroIDs, _currentDifficulty);
        }

        private void ClosePanel()
        {
            GameManager.Instance.UIManager.GoBack();
        }

        #endregion
    }

}
// SquadSelectionPanel.RemoveHeroFromSquad(index) khi được click.
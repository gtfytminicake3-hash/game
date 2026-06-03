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
            if (confirmButton == null) confirmButton = transform.Find("Btn_Confirm")?.GetComponent<Button>() ?? transform.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name.Contains("Confirm") || b.name.Contains("XacNhan"));
            if (closeButton == null) closeButton = transform.Find("Btn_Close")?.GetComponent<Button>() ?? transform.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name.Contains("Close") || b.name.Contains("Back"));
            if (titleText == null) titleText = transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>() ?? transform.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(t => t.name.Contains("Title") || t.name.Contains("TieuDe"));
            if (totalCpText == null) totalCpText = transform.Find("TotalCPText")?.GetComponent<TextMeshProUGUI>() ?? transform.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(t => t.name.Contains("CP") || t.name.Contains("CombatPower"));
            if (squadSlotsContainer == null) squadSlotsContainer = transform.Find("SquadSlotsContainer") ?? transform.Find("SquadSlots");
            if (availableListContainer == null) availableListContainer = transform.Find("AvailableListContainer") ?? transform.Find("HeroList") ?? transform.Find("Scroll View/Viewport/Content");

            if (confirmButton != null) Debug.Log($"[SquadSelectionPanel] Auto-hooked confirmButton: {confirmButton.name}");
            else Debug.LogWarning("[SquadSelectionPanel] Failed to auto-hook confirmButton!");
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
            // Dọn dẹp các slot dummy có sẵn trong Prefab ở lần mở đầu tiên
            if (_availableHeroCards.Count == 0 && availableListContainer != null && availableListContainer.childCount > 0)
            {
                foreach (Transform child in availableListContainer)
                {
                    Destroy(child.gameObject);
                }
            }

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
            // Object Pooling: Lọc danh sách hero hợp lệ (giữ cả hero đã chọn để làm mờ)
            var heroesToShow = _availableHeroes.Where(h =>
            {
                bool meetsProfessionRequirement = (_requiredProfession == Profession.None) || (h.profession == _requiredProfession);
                return h.isMature && meetsProfessionRequirement;
            }).OrderByDescending(h => h.GetCombatPower()).ToList();

            // Ẩn tất cả card hiện tại
            foreach (var card in _availableHeroCards)
            {
                if (card != null) card.SetActive(false);
            }

            // Tái sử dụng hoặc tạo mới card
            for (int i = 0; i < heroesToShow.Count; i++)
            {
                var hero = heroesToShow[i];
                GameObject cardInstance;

                if (i < _availableHeroCards.Count && _availableHeroCards[i] != null)
                {
                    cardInstance = _availableHeroCards[i];
                }
                else
                {
                    cardInstance = Instantiate(heroCardPrefab, availableListContainer);
                    _availableHeroCards.Add(cardInstance);
                }

                cardInstance.SetActive(true);

                bool isSelected = _selectedHeroes.Contains(hero);

                SquadSelectionHeroCard selectionCardScript = cardInstance.GetComponent<SquadSelectionHeroCard>();
                if (selectionCardScript != null)
                {
                    selectionCardScript.Setup(hero, this);
                }
                else
                {
                    HeroCard heroCardScript = cardInstance.GetComponent<HeroCard>();
                    if (heroCardScript != null) heroCardScript.Setup(hero);
                    
                    Button button = cardInstance.GetComponent<Button>();
                    if (button != null)
                    {
                        button.onClick.RemoveAllListeners();
                        if (!isSelected)
                        {
                            button.onClick.AddListener(() => AddHeroToSquad(hero));
                        }
                    }
                }

                // UX: Làm mờ hero đã chọn
                CanvasGroup cg = cardInstance.GetComponent<CanvasGroup>();
                if (cg == null) cg = cardInstance.AddComponent<CanvasGroup>();
                cg.alpha = isSelected ? 0.4f : 1.0f;
                cg.interactable = !isSelected;
                cg.blocksRaycasts = !isSelected;
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
            
            // Gọi callback để trả kết quả về cho người đã mở panel này
            _onConfirmCallback?.Invoke(selectedHeroIDs, _currentDifficulty);
            ClosePanel();
        }

        private void ClosePanel()
        {
            GameManager.Instance.UIManager.GoBack();
        }

        #endregion
    }

}
// SquadSelectionPanel.RemoveHeroFromSquad(index) khi được click.
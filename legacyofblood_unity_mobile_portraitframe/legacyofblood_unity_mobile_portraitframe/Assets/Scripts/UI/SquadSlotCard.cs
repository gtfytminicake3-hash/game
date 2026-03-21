namespace LegendOfBlood
{
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using TMPro;

    /// <summary>
    /// Script điều khiển một ô (slot) trong đội hình được chọn.
    /// Hỗ trợ kéo thả để đổi chỗ các Hero trong đội hình.
    /// </summary>
    public class SquadSlotCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [Header("UI References")]
        [Tooltip("Khung nền/placeholder, luôn hiển thị để cố định kích thước.")]
        [SerializeField] private Image placeholderBackground; 

        [Tooltip("Icon hiển thị khi ô trống (ví dụ: dấu cộng).")]
        [SerializeField] private GameObject emptyIconObject;
        
        [Tooltip("Image để hiển thị avatar của hero.")]
        [SerializeField] private Image heroAvatarImage;
        
        [Tooltip("Text để hiển thị tên của hero.")]
        [SerializeField] private TextMeshProUGUI heroNameText;
        
        [Tooltip("Nút để xử lý click.")]
        [SerializeField] private Button slotButton;

        // --- State Variables ---
        public int SlotIndex => _slotIndex;
        private int _slotIndex;
        private SquadSelectionPanel _selectionPanel;
        public HeroData AssignedHero => _assignedHero;
        private HeroData _assignedHero;

        // --- Drag & Drop state ---
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Vector2 _originalPosition;
        private Transform _originalParent;
        private int _originalSiblingIndex;

        #region Initialization

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (slotButton != null)
            {
                slotButton.onClick.AddListener(OnSlotClicked);
            }
        }

        /// <summary>
        /// Thiết lập ban đầu cho ô.
        /// </summary>
        public void Setup(int index, SquadSelectionPanel panel)
        {
            _slotIndex = index;
            _selectionPanel = panel;
            SetEmpty(); // Ban đầu tất cả các ô đều trống
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gán một hero vào ô này và cập nhật UI.
        /// </summary>
        public void SetHero(HeroData hero)
        {
            _assignedHero = hero;

            if (_assignedHero != null)
            {
                // Cập nhật dữ liệu
                heroNameText.text = _assignedHero.heroName;
                
                // Giả sử bạn có hệ thống tải sprite
                // heroAvatarImage.sprite = ResourceManager.LoadHeroAvatar(_assignedHero.avatarId);
                // Để test, bạn có thể gán trực tiếp nếu HeroData có tham chiếu Sprite
                 heroAvatarImage.sprite = _assignedHero.GetAvatarSprite(); 
                
                // Cập nhật trạng thái hiển thị
                emptyIconObject.SetActive(false);
                heroAvatarImage.gameObject.SetActive(true);
                heroNameText.gameObject.SetActive(true);
            }
            else
            {
                // Nếu vì lý do nào đó hero truyền vào là null, hãy coi như slot trống
                SetEmpty();
            }
        }

        /// <summary>
        /// Đặt ô về trạng thái trống, chỉ hiển thị placeholder và icon trống.
        /// </summary>
        public void SetEmpty()
        {
            _assignedHero = null;

            // Cập nhật trạng thái hiển thị
            emptyIconObject.SetActive(true);
            heroAvatarImage.gameObject.SetActive(false);
            heroNameText.gameObject.SetActive(false);
        }

        #endregion

        #region Event Handlers

        private void OnSlotClicked()
        {
            // Bỏ click remove hero ra vì drag drop có thể can thiệp. 
            // Vẫn giữ lại nếu muốn click thả nhanh:
            if (_assignedHero != null && !_isDragging)
            {
                _selectionPanel.RemoveHeroFromSquad(_slotIndex);
            }
        }

        #endregion

        #region Drag and Drop
        
        private bool _isDragging = false;
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_assignedHero == null) return; // Chỉ drag được ô có người
            
            _isDragging = true;
            _originalPosition = _rectTransform.anchoredPosition;
            _originalParent = transform.parent;
            _originalSiblingIndex = transform.GetSiblingIndex();
            
            // Đưa lên lớp trên cùng để không bị đè che khi kéo
            Transform rootCanvas = transform.root.GetComponentInChildren<Canvas>()?.transform ?? transform.root;
            transform.SetParent(rootCanvas);
            transform.SetAsLastSibling();
            
            _canvasGroup.blocksRaycasts = false; // Để tia ray có thể đâm xuyên qua cái thẻ đang kéo trúng thẻ nằm bên dưới
            _canvasGroup.alpha = 0.6f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            _rectTransform.anchoredPosition += eventData.delta / GetCanvasScale();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            
            _isDragging = false;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1f;

            // Xoay về chỗ cũ (mọi hoán đổi thực tế sẽ diễn ra bên trong OnDrop)
            transform.SetParent(_originalParent);
            transform.SetSiblingIndex(_originalSiblingIndex);
            
            // LayoutGroup sẽ tự giật thẻ về vị trí cũ, đây chỉ là dự phòng
            _rectTransform.anchoredPosition = _originalPosition;
        }

        public void OnDrop(PointerEventData eventData)
        {
            // Xảy ra khi có một cái Card (DraggedCard) thả lên cái Card này
            if (eventData.pointerDrag != null)
            {
                SquadSlotCard draggedCard = eventData.pointerDrag.GetComponent<SquadSlotCard>();
                if (draggedCard != null && draggedCard != this)
                {
                    // Hoán đổi 2 thẻ trong đội hình
                    _selectionPanel.SwapHeroes(draggedCard.SlotIndex, this.SlotIndex);
                }
            }
        }
        
        private float GetCanvasScale()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
                return canvas.scaleFactor;
            return 1f;
        }

        #endregion
    }
}
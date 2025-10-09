namespace LegendOfBlood
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    /// <summary>
    /// Script điều khiển một ô (slot) trong đội hình được chọn.
    /// Sử dụng một placeholder cố định để giữ layout ổn định.
    /// </summary>
    public class SquadSlotCard : MonoBehaviour
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
        private int _slotIndex;
        private SquadSelectionPanel _selectionPanel;
        private HeroData _assignedHero;

        #region Initialization

        private void Awake()
        {
            slotButton.onClick.AddListener(OnSlotClicked);
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

        /// <summary>
        /// Được gọi khi người chơi click vào ô.
        /// </summary>
        private void OnSlotClicked()
        {
            // Nếu ô đã có hero, hành động click là để loại bỏ hero đó.
            if (_assignedHero != null)
            {
                _selectionPanel.RemoveHeroFromSquad(_slotIndex);
            }
            // Nếu ô trống, không làm gì. Việc thêm hero được xử lý từ danh sách bên cạnh.
        }

        #endregion
    }
}
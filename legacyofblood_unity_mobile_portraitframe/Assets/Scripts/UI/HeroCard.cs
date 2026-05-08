namespace LegendOfBlood
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro; // Sử dụng TextMeshPro để có text đẹp hơn

    /// <summary>
    /// Script điều khiển một Prefab HeroCard đơn lẻ.
    /// Chịu trách nhiệm hiển thị dữ liệu của một hero và xử lý tương tác của người chơi.
    /// </summary>
    public class HeroCard : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI combatPowerText;
        [SerializeField] private Image genderIcon;
        [SerializeField] private Image professionIcon;
        [SerializeField] private Image avatarImage;
        [SerializeField] private Image cardFrame; // Added for rarity color
        [SerializeField] private GameObject busyIndicator; // Một icon/overlay để báo hero đang bận
        [SerializeField] private Button cardButton;

        [Header("Asset References")]
        [Tooltip("Sprite cho icon giới tính Nam")]
        [SerializeField] private Sprite maleIcon;
        [Tooltip("Sprite cho icon giới tính Nữ")]
        [SerializeField] private Sprite femaleIcon;

        [Header("Class Icons")]
        [SerializeField] private Sprite warriorIcon;
        [SerializeField] private Sprite archerIcon;
        [SerializeField] private Sprite mageIcon;
        [SerializeField] private Sprite healerIcon;

        // Lưu trữ dữ liệu của hero mà thẻ bài này đang hiển thị
        private HeroData _heroData;

        // --- PUBLIC METHODS ---

        /// <summary>
        /// Phương thức chính để thiết lập dữ liệu cho thẻ bài.
        /// Được gọi bởi UIMainController khi tạo ra các thẻ bài.
        /// </summary>
        /// <param name="heroData">Dữ liệu của hero cần hiển thị</param>
        public void Setup(HeroData heroData)
        {
            if (heroData == null)
            {
                Debug.LogError("Cố gắng thiết lập HeroCard với dữ liệu null!");
                gameObject.SetActive(false); // Ẩn thẻ bài nếu dữ liệu không hợp lệ
                return;
            }

            _heroData = heroData;

            // Cập nhật các thành phần UI dựa trên dữ liệu
            UpdateUI();

            // Gán sự kiện cho nút bấm
            // Chỉ gỡ OnCardClicked để tránh xóa nhầm các sự kiện khác được gắn từ bên ngoài
            if (cardButton != null)
            {
                cardButton.onClick.RemoveListener(OnCardClicked);
                cardButton.onClick.AddListener(OnCardClicked);
            }
        }

        /// <summary>
        /// Xóa dữ liệu hiển thị (Dùng làm thẻ trống/chờ chọn).
        /// </summary>
        public void Clear()
        {
            _heroData = null;
            if (nameText != null) nameText.text = "???";
            if (levelText != null) levelText.text = "";
            if (combatPowerText != null) combatPowerText.text = "";
            if (genderIcon != null) genderIcon.sprite = null;
            if (professionIcon != null) professionIcon.sprite = null;
            // Cho ảnh avatar thành màu xám/trống
            if (avatarImage != null)
            {
                avatarImage.sprite = null;
                avatarImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
            if (cardFrame != null)
            {
                cardFrame.color = new Color(0.2f, 0.2f, 0.3f, 1f); // Default dark frame
            }
            if (busyIndicator != null) busyIndicator.SetActive(false);

            if (cardButton != null) cardButton.onClick.RemoveListener(OnCardClicked);
        }

        // --- INTERNAL LOGIC ---

        /// <summary>
        /// Cập nhật tất cả các yếu tố hình ảnh của thẻ bài.
        /// </summary>
        private void UpdateUI()
        {
            // Cập nhật Text
            if (nameText != null) nameText.text = _heroData.heroName;
            if (levelText != null) levelText.text = string.Format(global::LocalizationSystem.GetText("level_format_short"), _heroData.level);

            // Tính toán và hiển thị CP
            // Dựa trên công thức trong GDD_01 (đã được implement trong HeroData.cs)
            if (combatPowerText != null) combatPowerText.text = string.Format(global::LocalizationSystem.GetText("cp_format_short"), _heroData.GetCombatPower());

            // Cập nhật Icons (giả sử bạn có Sprite cho chúng)
            if (genderIcon != null) genderIcon.sprite = GetGenderSprite(_heroData.gender);
            if (professionIcon != null) professionIcon.sprite = GetProfessionSprite(_heroData.profession);
            
            // Cập nhật Avatar
            if (avatarImage != null) avatarImage.sprite = _heroData.GetAvatarSprite();

            // Cập nhật khung thẻ theo độ hiếm
            if (cardFrame != null)
            {
                cardFrame.color = GetRarityColor(_heroData.potential);
            }

            // Hiển thị chỉ báo bận
            // Dựa trên hàm IsBusy() trong HeroData.cs
            if (busyIndicator != null)
            {
                busyIndicator.SetActive(_heroData.IsBusy());
            }
        }

        private Color GetRarityColor(int potential)
        {
            if (potential >= 25) return new Color(0f, 1f, 1f, 1f);          // SSS-rank: Cyan
            if (potential >= 21) return new Color(1f, 0.2f, 0.2f, 1f);      // SS-rank: Red
            if (potential >= 15) return new Color(1f, 0.84f, 0f, 1f);       // S-rank: Gold
            if (potential >= 12) return new Color(0.6f, 0.2f, 0.8f, 1f);    // A-rank: Purple
            if (potential >= 8) return new Color(0.2f, 0.5f, 1f, 1f);       // B-rank: Blue
            if (potential >= 5) return new Color(0.3f, 0.8f, 0.3f, 1f);     // C-rank: Green
            return new Color(0.5f, 0.5f, 0.5f, 1f);                         // D-rank: Gray
        }

        /// <summary>
        /// Được gọi khi người chơi nhấn vào thẻ bài.
        /// </summary>
        private void OnCardClicked()
{
    Debug.Log($"Clicked on Hero: {_heroData.heroName} (ID: {_heroData.id})");
    
    // --- THAY ĐỔI QUAN TRỌNG BẮT ĐẦU TỪ ĐÂY ---

    // 1. Yêu cầu UIManager hiển thị Panel thông tin chi tiết.
    //    Tham số 'false' có nghĩa là panel này sẽ hiện ĐÈ LÊN panel hiện tại (MainScreen),
    //    chứ không thay thế nó. Đây là hành vi mong muốn cho một popup.
    GameManager.Instance.UIManager.ShowPanel(UIPanelType.HeroInfo, false);

    // 2. Sau khi đã đảm bảo panel được bật, BÂY GIỜ chúng ta mới phát ra sự kiện.
    //    Script HeroInfoPanel, lúc này đã được kích hoạt, sẽ có thể "nghe" thấy
    //    và nhận dữ liệu này để tự cập nhật.
    EventManager.TriggerEvent(GameEvents.OnHeroCardClicked, _heroData);
}

        /// <summary>
        /// Lấy Sprite tương ứng với giới tính.
        /// </summary>
        private Sprite GetGenderSprite(Gender gender)
        {
            switch (gender)
            {
                case Gender.Male: return maleIcon;
                case Gender.Female: return femaleIcon;
                default: return null;
            }
        }

        /// <summary>
        /// Lấy Sprite tương ứng với hệ của hero.
        /// </summary>
        private Sprite GetProfessionSprite(Profession profession)
        {
            switch (profession)
            {
                case Profession.Warrior: return warriorIcon;
                case Profession.Archer: return archerIcon;
                case Profession.Mage: return mageIcon;
                case Profession.Healer: return healerIcon;
                default: return null;
            }
        }
        // --- UNITY LIFECYCLE ---

        private void OnValidate()
        {
            // Kiểm tra trong Editor để đảm bảo tất cả các tham chiếu đã được gán.
            if (cardButton == null) cardButton = GetComponent<Button>();
            if (nameText == null || levelText == null || combatPowerText == null)
            {
                Debug.LogWarning("Một hoặc nhiều tham chiếu TextMeshProUGUI chưa được gán trên HeroCard!", this);
            }
        }
    }
}
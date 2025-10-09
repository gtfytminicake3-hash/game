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
        [SerializeField] private GameObject busyIndicator; // Một icon/overlay để báo hero đang bận
        [SerializeField] private Button cardButton;

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
            // Xóa các listener cũ để tránh gọi nhiều lần, sau đó thêm listener mới
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(OnCardClicked);
        }

        // --- INTERNAL LOGIC ---

        /// <summary>
        /// Cập nhật tất cả các yếu tố hình ảnh của thẻ bài.
        /// </summary>
        private void UpdateUI()
        {
            // Cập nhật Text
            nameText.text = _heroData.heroName;
            levelText.text = string.Format(global::LocalizationSystem.GetText("level_format_short"), _heroData.level);

            // Tính toán và hiển thị CP
            // Dựa trên công thức trong GDD_01 (đã được implement trong HeroData.cs)
            combatPowerText.text = string.Format(global::LocalizationSystem.GetText("cp_format_short"), _heroData.GetCombatPower());

            // Cập nhật Icons (giả sử bạn có Sprite cho chúng)
            // genderIcon.sprite = GetGenderSprite(_heroData.gender);
            // professionIcon.sprite = GetProfessionSprite(_heroData.profession);

            // Hiển thị chỉ báo bận
            // Dựa trên hàm IsBusy() trong HeroData.cs
            if (busyIndicator != null)
            {
                busyIndicator.SetActive(_heroData.IsBusy());
            }
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
        // --- UNITY LIFECYCLE ---

        private void OnValidate()
        {
            // Kiểm tra trong Editor để đảm bảo tất cả các tham chiếu đã được gán.
            if (cardButton == null) cardButton = GetComponent<Button>();
            if (nameText == null || levelText == null || combatPowerText == null)
            {
                Debug.LogError("Một hoặc nhiều tham chiếu TextMeshProUGUI chưa được gán trên HeroCard!", this);
            }
        }
    }
}
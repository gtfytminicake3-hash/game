namespace LegendOfBlood
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    [RequireComponent(typeof(HeroCard))] // Thêm dòng này để đảm bảo GameObject luôn có script HeroCard
    public class InjuredHeroCard : MonoBehaviour
    {
        [Header("UI References")]
        // Bỏ dòng [SerializeField] private HeroCard baseHeroCard; đi
        // Thay vào đó, chúng ta sẽ dùng một biến private
        private HeroCard _baseHeroCard; 
        
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button healButton;

        private HeroData _heroData;
        private HospitalPanel _hospitalPanel;

        // --- THÊM HÀM AWAKE() ---
        /// <summary>
        /// Awake được gọi trước Start. Đây là nơi tốt nhất để lấy các tham chiếu component.
        /// </summary>
        private void Awake()
        {
            // Tự động tìm và lấy component HeroCard trên cùng GameObject này.
            _baseHeroCard = GetComponent<HeroCard>();
            if (_baseHeroCard == null)
            {
                Debug.LogError("InjuredHeroCard không thể tìm thấy component HeroCard trên cùng GameObject!", this);
            }
        }

        /// <summary>
        /// Thiết lập dữ liệu và callback cho thẻ bài.
        /// </summary>
        public void Setup(HeroData heroData, HospitalPanel hospitalPanel)
        {
            _heroData = heroData;
            _hospitalPanel = hospitalPanel;

            // Dùng lại HeroCard gốc để hiển thị thông tin cơ bản
            // _baseHeroCard bây giờ đã có giá trị nhờ hàm Awake()
            _baseHeroCard.Setup(heroData);

            // Gán sự kiện cho nút
            healButton.onClick.RemoveAllListeners();
            healButton.onClick.AddListener(OnHealButtonClicked);
            
            UpdateCardState();
        }

        // ... (phần còn lại của file: Update(), OnHealButtonClicked(), UpdateCardState() giữ nguyên) ...
        void Update()
        {
            if (_heroData == null) return;

            long endTime = _heroData.isSeverelyInjured ? _heroData.injuryEndTime : _heroData.lightInjuryEndTime;
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            if (currentTime >= endTime)
            {
                timerText.text = global::LocalizationSystem.GetText("ready");
                return;
            }

            TimeSpan timeLeft = TimeSpan.FromMilliseconds(endTime - currentTime);
            timerText.text = string.Format(LocalizationSystem.GetText("time_left_format"), timeLeft);
        }

        private void OnHealButtonClicked()
        {
            _hospitalPanel.RequestHeal(_heroData);
        }

        private void UpdateCardState()
        {
            int cost = 0;
            if (_heroData.isSeverelyInjured)
            {
                cost = Mathf.FloorToInt(_heroData.GetCombatPower() / 10f) + 50;
                healButton.GetComponentInChildren<TextMeshProUGUI>().text = global::LocalizationSystem.GetText("heal_severe");
            }
            else if (_heroData.isLightlyInjured)
            {
                cost = Mathf.FloorToInt(_heroData.GetCombatPower() / 50f) + 10;
                healButton.GetComponentInChildren<TextMeshProUGUI>().text = global::LocalizationSystem.GetText("heal_light");
            }

            costText.text = string.Format(global::LocalizationSystem.GetText("gold_cost_format"), cost);
        }
    }
}
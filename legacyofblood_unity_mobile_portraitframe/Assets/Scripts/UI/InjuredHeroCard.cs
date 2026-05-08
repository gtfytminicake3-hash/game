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
        [SerializeField] private Button healAdButton; // NEW: Nút chữa bệnh miễn phí bằng Ads

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
            if (healButton != null) {
                healButton.onClick.RemoveAllListeners();
                healButton.onClick.AddListener(OnHealButtonClicked);
            }
            if (healAdButton != null) {
                healAdButton.onClick.RemoveAllListeners();
                healAdButton.onClick.AddListener(OnHealAdButtonClicked);
            }
            
            UpdateCardState();
        }

        // ... (phần còn lại của file: Update(), OnHealButtonClicked(), UpdateCardState() giữ nguyên) ...
        void Update()
        {
            if (_heroData == null) return;

            long endTime = _heroData.isSeverelyInjured ? _heroData.injuryEndTime : _heroData.lightInjuryEndTime;
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            if (timerText != null)
            {
                if (currentTime >= endTime)
                {
                    timerText.text = global::LocalizationSystem.GetText("ready");
                }
                else
                {
                    TimeSpan timeLeft = TimeSpan.FromMilliseconds(endTime - currentTime);
                    timerText.text = string.Format(LocalizationSystem.GetText("time_left_format"), string.Format("{0:D2}:{1:D2}:{2:D2}", timeLeft.Hours, timeLeft.Minutes, timeLeft.Seconds));
                }
            }
        }

        private void OnHealButtonClicked()
        {
            _hospitalPanel.RequestHeal(_heroData);
        }

        private void OnHealAdButtonClicked()
        {
            if (LegendOfBlood.Managers.AdRewardGateway.Instance != null)
            {
                LegendOfBlood.Managers.AdRewardGateway.Instance.RequestAd(LegendOfBlood.Managers.RewardType.FreeHeal, () => {
                    GameManager.Instance.HospitalSystem.HealSevereInjuryFree(_heroData);
                    // Hospital Panel lắng nghe sự kiện OnHeroHealed nên tự nó RefreshLists()
                });
            }
        }

        private void UpdateCardState()
        {
            int cost = 0;
            if (_heroData.isSeverelyInjured)
            {
                cost = Mathf.FloorToInt(_heroData.GetCombatPower() / 10f) + 50;
                if (healButton != null) 
                {
                    healButton.GetComponentInChildren<TextMeshProUGUI>().text = string.Format("{0} <color=#FFD700>{1}G</color>", global::LocalizationSystem.GetText("heal_severe"), cost);
                }
                
                // Show free ad button if limit not reached
                if (healAdButton != null) 
                {
                    bool canShowAd = DataManager.Instance.Player.dailyFreeHealsWatched < 2;
                    healAdButton.gameObject.SetActive(canShowAd);
                }
            }
            else if (_heroData.isLightlyInjured)
            {
                cost = Mathf.FloorToInt(_heroData.GetCombatPower() / 50f) + 10;
                if (healButton != null) 
                {
                    // Vì nút của Light Panel hiện tại diện tích chưa lớn, hiển thị Text ngắn gọn
                    healButton.GetComponentInChildren<TextMeshProUGUI>().text = string.Format("{0} <color=#FFD700>{1}G</color>", global::LocalizationSystem.GetText("heal_light"), cost);
                }
                
                // Light injuries do not get free heals
                if (healAdButton != null) healAdButton.gameObject.SetActive(false);
            }

            if (costText != null) costText.text = string.Format(global::LocalizationSystem.GetText("gold_cost_format"), cost);
        }
    }
}
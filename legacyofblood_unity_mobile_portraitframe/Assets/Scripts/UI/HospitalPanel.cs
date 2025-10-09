namespace LegendOfBlood
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Điều khiển giao diện của Bệnh viện, hiển thị danh sách hero bị thương.
    /// </summary>
    public class HospitalPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform severeInjuryListContainer;
        [SerializeField] private Transform lightInjuryListContainer;
        [SerializeField] private GameObject injuredHeroCardPrefab;
        [SerializeField] private Button closeButton;

        private List<GameObject> _instantiatedCards = new List<GameObject>();

        #region Unity Lifecycle & Event Subscription
        
        private void Awake()
        {
            closeButton.onClick.AddListener(() => GameManager.Instance.UIManager.HidePanel(UIPanelType.Hospital)); 
        }

        private void OnEnable()
        {
            // Mỗi khi panel được mở, làm mới danh sách
            RefreshLists();
            
            // Lắng nghe sự kiện để tự động cập nhật nếu có hero được chữa lành
            HospitalSystem.OnHeroHealed += HandleHeroHealed;
        }

        private void OnDisable()
        {
            HospitalSystem.OnHeroHealed -= HandleHeroHealed;
        }

        #endregion

        #region Core Logic

        /// <summary>
        /// Xóa và vẽ lại toàn bộ danh sách hero bị thương.
        /// </summary>
        private void RefreshLists()
        {
            // Dọn dẹp
            foreach (var card in _instantiatedCards)
            {
                Destroy(card);
            }
            _instantiatedCards.Clear();

            // Lấy danh sách hero
            var allHeroes = DataManager.Instance.AllHeroes;
            if (allHeroes == null) return;
            
            long currentTime = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Lọc và hiển thị hero bị thương nặng
            var severelyInjuredHeroes = allHeroes.Where(h => h.isSeverelyInjured && h.injuryEndTime > currentTime);
            foreach (var hero in severelyInjuredHeroes)
            {
                CreateCard(hero, severeInjuryListContainer);
            }

            // Lọc và hiển thị hero bị thương nhẹ
            var lightlyInjuredHeroes = allHeroes.Where(h => h.isLightlyInjured && h.lightInjuryEndTime > currentTime);
            foreach (var hero in lightlyInjuredHeroes)
            {
                CreateCard(hero, lightInjuryListContainer);
            }
        }

        private void CreateCard(HeroData heroData, Transform container)
        {
            GameObject cardInstance = Instantiate(injuredHeroCardPrefab, container);
            InjuredHeroCard cardScript = cardInstance.GetComponent<InjuredHeroCard>();
            
            if (cardScript != null)
            {
                cardScript.Setup(heroData, this);
                _instantiatedCards.Add(cardInstance);
            }
        }
        
        /// <summary>
        /// Được gọi bởi InjuredHeroCard khi người chơi nhấn nút chữa trị.
        /// </summary>
        public void RequestHeal(HeroData heroToHeal)
        {
            HospitalSystem hospitalSystem = GameManager.Instance.HospitalSystem;
            bool success = false;
            
            if (heroToHeal.isSeverelyInjured)
            {
                success = hospitalSystem.HealSevereInjury(heroToHeal);
            }
            else if (heroToHeal.isLightlyInjured)
            {
                success = hospitalSystem.HealLightInjuryInstantly(heroToHeal);
            }

            if (success)
            {
                GameManager.Instance.UINotificationManager.ShowNotification(string.Format(global::LocalizationSystem.GetText("hospital_hero_healed_format"), heroToHeal.heroName));
                // Làm mới danh sách sau khi chữa trị thành công
                RefreshLists();
            }
            // Nếu thất bại (không đủ tiền), UINotificationManager sẽ được gọi từ bên trong InventoryManager
        }

        /// <summary>
        /// Xử lý sự kiện khi một hero được chữa lành (có thể do hết giờ hoặc trả phí).
        /// </summary>
        private void HandleHeroHealed(HeroData healedHero)
        {
            // Chỉ cần làm mới lại toàn bộ danh sách
            RefreshLists();
        }

        #endregion
    }
}
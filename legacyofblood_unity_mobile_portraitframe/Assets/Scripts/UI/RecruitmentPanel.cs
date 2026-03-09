namespace LegendOfBlood.UI
{
    using UnityEngine;
    using UnityEngine.UI;
    using LegendOfBlood;

    public class RecruitmentPanel : UIPanel
    {
        [SerializeField] private Button recruitOneButton;
        [SerializeField] private Button recruitTenButton;
        [SerializeField] private Button closeButton;

        private RecruitmentSystem _recruitmentSystem;

        private void Awake()
        {
            PanelType = UIPanelType.Recruitment;
        }

        private void Start()
        {
            _recruitmentSystem = GameManager.Instance.RecruitmentSystem;

            recruitOneButton.onClick.AddListener(OnRecruitOne);
            recruitTenButton.onClick.AddListener(OnRecruitTen);
            if (closeButton != null) closeButton.onClick.AddListener(() => GameManager.Instance.UIManager.GoBack());

            Debug.Log("RecruitmentPanel Initialized");
        }

        private void OnRecruitOne()
        {
            Debug.Log("Attempting to recruit 1 hero.");
            if (DataManager.Instance.IsPopulationFull())
            {
                GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("notification_population_full"));
                return;
            }

            // Gacha Logic Payment
            if (GameManager.Instance.InventoryManager.GetItemCount("IT_GACHA_TICKET") >= 1)
            {
                GameManager.Instance.InventoryManager.UseItem("IT_GACHA_TICKET", 1);
            }
            else if (GameManager.Instance.InventoryManager.HasEnoughResources(ResourceType.Diamond, 100))
            {
                GameManager.Instance.InventoryManager.SpendResource(ResourceType.Diamond, 100);
            }
            else
            {
                GameManager.Instance.UINotificationManager.ShowNotification("Không Cầm Đủ Vé Chiêu Mộ Hoặc Kim Cương!");
                return;
            }

            var newHeroes = _recruitmentSystem.PerformRecruitment(1);
            // TODO: Show hero results UI
            if (newHeroes.Count > 0)
            {
                GameManager.Instance.UINotificationManager.ShowNotification($"Chiêu mộ thành công {newHeroes[0].heroName}!");
            }
        }

        private void OnRecruitTen()
        {
            Debug.Log("Attempting to recruit 10 heroes.");
            if (DataManager.Instance.IsPopulationFull())
            {
                GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("notification_population_full"));
                return;
            }

            // Gacha Logic Payment
            if (GameManager.Instance.InventoryManager.GetItemCount("IT_GACHA_TICKET") >= 10)
            {
                GameManager.Instance.InventoryManager.UseItem("IT_GACHA_TICKET", 10);
            }
            else if (GameManager.Instance.InventoryManager.HasEnoughResources(ResourceType.Diamond, 900))
            {
                GameManager.Instance.InventoryManager.SpendResource(ResourceType.Diamond, 900);
            }
            else
            {
                GameManager.Instance.UINotificationManager.ShowNotification("Không Cầm Đủ Vé Chiêu Mộ Hoặc Kim Cương!");
                return;
            }

            var newHeroes = _recruitmentSystem.PerformRecruitment(10);
            // TODO: Show hero results UI
            if (newHeroes.Count > 0)
            {
                GameManager.Instance.UINotificationManager.ShowNotification($"Chiêu mộ thành công {newHeroes.Count} anh hùng mới!");
            }
        }

        private void OnDestroy()
        {
            recruitOneButton.onClick.RemoveListener(OnRecruitOne);
            recruitTenButton.onClick.RemoveListener(OnRecruitTen);
        }
    }
}

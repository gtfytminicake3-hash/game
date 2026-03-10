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

        private void Awake()
        {
            PanelType = UIPanelType.Recruitment;
        }

        private void Start()
        {
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
                GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("notification_not_enough_currency"));
                return;
            }

            var newHeroes = GameManager.Instance.RecruitmentSystem.PerformRecruitment(1);
            // TODO: Show hero results UI
            if (newHeroes.Count > 0)
            {
                string msgTemplate = LocalizationSystem.GetText("notification_recruit_success");
                GameManager.Instance.UINotificationManager.ShowNotification(string.Format(msgTemplate, newHeroes[0].heroName));
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
                GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("notification_not_enough_currency"));
                return;
            }

            var newHeroes = GameManager.Instance.RecruitmentSystem.PerformRecruitment(10);
            // TODO: Show hero results UI
            if (newHeroes.Count > 0)
            {
                string msgTemplate = LocalizationSystem.GetText("notification_recruit_multi_success");
                GameManager.Instance.UINotificationManager.ShowNotification(string.Format(msgTemplate, newHeroes.Count));
            }
        }

        private void OnDestroy()
        {
            recruitOneButton.onClick.RemoveListener(OnRecruitOne);
            recruitTenButton.onClick.RemoveListener(OnRecruitTen);
        }
    }
}

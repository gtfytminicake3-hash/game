namespace LegendOfBlood.UI
{
    using UnityEngine;
    using UnityEngine.UI;
    using LegendOfBlood;

    public class RecruitmentPanel : MonoBehaviour
    {
        [SerializeField] private Button recruitOneButton;
        [SerializeField] private Button recruitTenButton;

        private RecruitmentSystem _recruitmentSystem;

        private void Start()
        {
            _recruitmentSystem = GameManager.Instance.RecruitmentSystem;

            recruitOneButton.onClick.AddListener(OnRecruitOne);
            recruitTenButton.onClick.AddListener(OnRecruitTen);

            Debug.Log("RecruitmentPanel Initialized");
        }

        private void OnRecruitOne()
        {
            Debug.Log("Attempting to recruit 1 hero.");
            // TODO: Add currency check
            var newHeroes = _recruitmentSystem.PerformRecruitment(1);
            // TODO: Show hero results UI
            if (newHeroes.Count > 0)
            {
                GameManager.Instance.UINotificationManager.ShowNotification($"Recruited {newHeroes[0].heroName}!");
            }
        }

        private void OnRecruitTen()
        {
            Debug.Log("Attempting to recruit 10 heroes.");
            // TODO: Add currency check
            var newHeroes = _recruitmentSystem.PerformRecruitment(10);
            // TODO: Show hero results UI
            if (newHeroes.Count > 0)
            {
                GameManager.Instance.UINotificationManager.ShowNotification($"Recruited {newHeroes.Count} new heroes!");
            }
        }

        private void OnDestroy()
        {
            recruitOneButton.onClick.RemoveListener(OnRecruitOne);
            recruitTenButton.onClick.RemoveListener(OnRecruitTen);
        }
    }
}

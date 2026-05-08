using UnityEngine;

namespace LegendOfBlood
{
    /// <summary>
    /// Script này chỉ dùng để bắt sự kiện click từ các nút trên thanh điều hướng
    /// và gọi đến các hàm tương ứng của UIManager.
    /// </summary>
    public class BottomNavigationController : MonoBehaviour
    {
        [Header("Notification Badges")]
        [SerializeField] private GameObject hospitalBadge;
        [SerializeField] private GameObject breedingBadge;

        private void Update()
        {
            UpdateNotificationBadges();
        }

        private void UpdateNotificationBadges()
        {
            if (GameManager.Instance == null || DataManager.Instance?.Player == null) return;

            // 1. Hospital Badge: Show if any hero is finished healing
            if (hospitalBadge != null)
            {
                bool hasHealedHero = false;
                if (GameManager.Instance.HospitalSystem != null && DataManager.Instance.AllHeroes != null)
                {
                    foreach (var hero in DataManager.Instance.AllHeroes)
                    {
                        if (hero.isLightlyInjured || hero.isSeverelyInjured)
                        {
                            hasHealedHero = true;
                            break;
                        }
                    }
                }
                hospitalBadge.SetActive(hasHealedHero);
            }

            // 2. Breeding/Maturation Badge: Show if any newborn is ready to mature
            if (breedingBadge != null)
            {
                bool hasMatureHero = false;
                long currentTime = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                foreach (var hero in DataManager.Instance.AllHeroes)
                {
                    if (!hero.isMature && currentTime >= hero.maturationEndTime)
                    {
                        hasMatureHero = true;
                        break;
                    }
                }
                breedingBadge.SetActive(hasMatureHero);
            }
        }
    }
}
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

        public void OpenMainScreenPanel()
        {
            if (GameManager.Instance != null && GameManager.Instance.UIManager != null)
            {
                // Tham số 'true' để nó thay thế panel hiện tại, không hiện đè.
                GameManager.Instance.UIManager.ShowPanel(UIPanelType.MainScreen, true);
            }
        }

        public void OpenHospitalPanel()
        {
            if (GameManager.Instance != null && GameManager.Instance.UIManager != null)
            {
                GameManager.Instance.UIManager.ShowPanel(UIPanelType.Hospital, true);
            }
        }


        public void OpenMenuPanel()
        {
            if (GameManager.Instance != null && GameManager.Instance.UIManager != null)
            {
                GameManager.Instance.UIManager.ShowPanel(UIPanelType.Menu, true);
            }
        }
        
        public void OpenBreedingPanel()
        {
            if (GameManager.Instance != null && GameManager.Instance.UIManager != null)
            {
                GameManager.Instance.UIManager.ShowPanel(UIPanelType.Breeding, true);
            }
        }
        
        public void OpenArenaPanel()
        {
            if (GameManager.Instance != null && GameManager.Instance.UIManager != null)
            {
                GameManager.Instance.UIManager.ShowPanel(UIPanelType.Arena, true);
            }
        }
        
        public void OpenBarrackPanel()
        {
            if (GameManager.Instance != null && GameManager.Instance.UIManager != null)
            {
                GameManager.Instance.UIManager.ShowPanel(UIPanelType.Barrack, true);
            }
        }

        // Tương lai nếu gắn Mailbox vào Bottom Nav
        public void OpenMailboxPanel()
        {
            if (GameManager.Instance != null && GameManager.Instance.UIManager != null)
            {
                GameManager.Instance.UIManager.ShowPanel(UIPanelType.Mailbox, true);
            }
        }
        
        public void OpenInventoryPanel()
        {
            if (GameManager.Instance != null && GameManager.Instance.UIManager != null)
            {
                GameManager.Instance.UIManager.ShowPanel(UIPanelType.Inventory, true);
            }
        }

        
        // Hàm này dùng cho nút bấm trên màn hình chính cũ để vào làng
        // hoặc nút bấm trên các panel để vào WorldMap Scene
        public void GoToWorldMapScene()
        {
            if (GameManager.Instance != null && GameManager.Instance.UIManager != null)
            {
                GameManager.Instance.UIManager.ShowPanel(UIPanelType.WorldMap, true);
            }
        }
    }
}
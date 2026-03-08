using UnityEngine;

namespace LegendOfBlood
{
    /// <summary>
    /// Script này chỉ dùng để bắt sự kiện click từ các nút trên thanh điều hướng
    /// và gọi đến các hàm tương ứng của UIManager.
    /// </summary>
    public class BottomNavigationController : MonoBehaviour
    {
        // Chúng ta không cần tham chiếu đến các nút, chỉ cần các hàm public.

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
using UnityEngine;

namespace LegendOfBlood 
{
    public class UIPanel : MonoBehaviour 
    {
        [Tooltip("Loại của panel này. UIManager sẽ dùng thông tin này để nhận diện.")]
        public UIPanelType PanelType;

        // Không cần hàm Awake() hay OnDestroy() nữa!
        // UIManager sẽ chủ động tìm và quản lý panel này.
    }
}
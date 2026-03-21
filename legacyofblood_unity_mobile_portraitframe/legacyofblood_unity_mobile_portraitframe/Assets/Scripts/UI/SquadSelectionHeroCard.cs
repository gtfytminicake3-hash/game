namespace LegendOfBlood
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Thẻ HeroCard đặc thù dành cho bảng chọn Đội hình (SquadSelectionPanel).
    /// Nó giữ lại chức năng hiển thị thông tin của HeroCard gốc, nhưng thêm
    /// một Nút "Chọn" (Select Button) riêng biệt để Add tướng vào ô.
    /// </summary>
    [RequireComponent(typeof(HeroCard))]
    public class SquadSelectionHeroCard : MonoBehaviour
    {
        [Tooltip("Kéo nút bấm 'Chọn' (Select) từ UI vào đây")]
        [SerializeField] private Button selectButton;
        
        private HeroData _heroData;
        private SquadSelectionPanel _selectionPanel;

        /// <summary>
        /// Khởi tạo thẻ bài với dữ liệu Tướng và tham chiếu tới Bảng Đội Hình mẹ.
        /// </summary>
        public void Setup(HeroData heroData, SquadSelectionPanel panel)
        {
            _heroData = heroData;
            _selectionPanel = panel;

            // Kích hoạt Setup của script HeroCard truyền thống (Vẽ ảnh, Tên, Cấp...)
            GetComponent<HeroCard>().Setup(heroData);

            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(OnSelectClicked);
            }
            else
            {
                Debug.LogWarning("[SquadSelectionHeroCard] Bạn chưa kéo Nút 'Chọn' vào tham chiếu selectButton trong Inspector!");
            }
        }

        private void OnSelectClicked()
        {
            if (_selectionPanel != null && _heroData != null)
            {
                _selectionPanel.AddHeroToSquad(_heroData);
            }
        }
    }
}

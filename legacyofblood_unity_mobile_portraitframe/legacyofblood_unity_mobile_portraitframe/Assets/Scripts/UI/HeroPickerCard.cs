namespace LegendOfBlood
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Một script phụ trợ, được thêm vào các Prefab HeroCard khi chúng được
    /// tạo ra bên trong HeroPickerPanel.
    /// Mục đích chính của nó là ghi đè hành vi click của thẻ bài để
    /// thông báo cho HeroPickerPanel rằng một hero đã được chọn.
    /// </summary>
    [RequireComponent(typeof(HeroCard), typeof(Button))] // Đảm bảo GameObject có sẵn các component cần thiết
    public class HeroPickerCard : MonoBehaviour
    {
        private HeroData _heroData;
        private HeroPickerPanel _pickerPanel; // Tham chiếu đến panel cha đã tạo ra nó

        /// <summary>
        /// Phương thức thiết lập chính, được gọi từ HeroPickerPanel.
        /// </summary>
        /// <param name="heroData">Dữ liệu của hero mà thẻ bài này đại diện.</param>
        /// <param name="pickerPanel">Tham chiếu đến panel cha để gọi lại khi được click.</param>
        public void Setup(HeroData heroData, HeroPickerPanel pickerPanel)
        {
            // --- Bước 1: Lưu lại dữ liệu và tham chiếu ---
            _heroData = heroData;
            _pickerPanel = pickerPanel;

            // --- Bước 2: Tái sử dụng phần hiển thị của HeroCard gốc ---
            // Lấy script HeroCard đã có sẵn trên GameObject này và yêu cầu nó
            // hiển thị thông tin của hero.
            GetComponent<HeroCard>().Setup(heroData);

            // --- Bước 3: Ghi đè hành vi của nút bấm ---
            Button button = GetComponent<Button>();

            // Xóa mọi listener cũ để đảm bảo hành vi click của HeroCard gốc không được gọi.
            button.onClick.RemoveAllListeners();
            // Thêm listener mới, trỏ đến phương thức OnClick của chính script này.
            button.onClick.AddListener(OnClick);
        }

        /// <summary>
        /// Được gọi khi người chơi nhấn vào thẻ bài này.
        /// </summary>
        private void OnClick()
        {
            // Kiểm tra để đảm bảo panel và data hợp lệ
            if (_pickerPanel != null && _heroData != null)
            {
                // Gọi lại phương thức của panel cha, báo cho nó biết hero nào đã được chọn.
                _pickerPanel.HandleHeroSelection(_heroData);
            }
            else
            {
                Debug.LogError("HeroPickerCard chưa được thiết lập đúng cách!", this);
            }
        }
    }
}
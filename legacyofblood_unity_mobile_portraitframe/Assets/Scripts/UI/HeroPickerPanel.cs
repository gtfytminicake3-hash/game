namespace LegendOfBlood
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    /// <summary>
    /// Một panel đa dụng để hiển thị một danh sách hero và cho phép người dùng chọn một.
    /// Sau khi chọn, nó sẽ phát ra một sự kiện với hero đã được chọn.
    /// </summary>
    public class HeroPickerPanel : UIPanel
    {
        [Header("UI References")]
        [SerializeField] private Transform listContainer;
        [SerializeField] private GameObject heroCardPrefab;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button closeButton;

        // Lưu trữ danh sách các card đã tạo
        private List<GameObject> _instantiatedCards = new List<GameObject>();

        // Sự kiện được phát ra khi người dùng chọn một hero.
        private Action<HeroData> _onHeroPickedCallback;

        #region Unity Lifecycle

        private void Awake()
        {
            PanelType = UIPanelType.HeroPicker;
        }

        protected override void Start()
        {
            base.Start();
            if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Phương thức chính để mở và thiết lập panel.
        /// </summary>
        /// <param name="title">Tiêu đề của panel (ví dụ: "Chọn Cha", "Chọn Mẹ")</param>
        /// <param name="heroesToShow">Danh sách hero hợp lệ để hiển thị</param>
        /// <param name="onHeroPicked">Callback khi chọn tướng xong</param>
        public void Show(string title, List<HeroData> heroesToShow, Action<HeroData> onHeroPicked)
        {
            _onHeroPickedCallback = onHeroPicked;
            Debug.Log($"--- HeroPickerPanel.Show() được gọi với {heroesToShow.Count} hero. ---"); // Log 6
            // Yêu cầu UIManager hiển thị panel này đè lên panel hiện tại
            GameManager.Instance.UIManager.ShowPanel(UIPanelType.HeroPicker, false);

            titleText.text = title;
            PopulateList(heroesToShow);
        }

        #endregion

        #region Internal Logic

        private void PopulateList(List<HeroData> heroes)
        {Debug.Log($"--- PopulateList() bắt đầu với {heroes.Count} hero. ---"); // Log 7
          if (listContainer == null)
          {
              Debug.LogError("LỖI NGHIÊM TRỌNG: Tham chiếu 'listContainer' trong HeroPickerPanel đang bị NULL!", this.gameObject);
              return; // Dừng hàm ngay lập tức
          }
          if (heroCardPrefab == null)
          {
              Debug.LogError("LỖI NGHIÊM TRỌNG: Tham chiếu 'heroCardPrefab' trong HeroPickerPanel đang bị NULL!", this.gameObject);
              return; // Dừng hàm ngay lập tức 
          }
          
            // Dọn dẹp danh sách cũ bằng Object Pooling
            foreach (var card in _instantiatedCards)
            {
                card.SetActive(false);
            }

            // Sắp xếp theo CP giảm dần (theo GDD)
            var sortedHeroes = heroes.OrderByDescending(h => h.GetCombatPower()).ToList();

            // Tạo card mới hoặc tái sử dụng
            for (int i = 0; i < sortedHeroes.Count; i++)
            {
                var hero = sortedHeroes[i];
                GameObject cardInstance;
                
                if (i < _instantiatedCards.Count)
                {
                    cardInstance = _instantiatedCards[i];
                }
                else
                {
                    cardInstance = Instantiate(heroCardPrefab, listContainer);
                    HeroPickerCard cardScript = cardInstance.GetComponent<HeroPickerCard>();
                    if (cardScript == null) cardScript = cardInstance.AddComponent<HeroPickerCard>(); // Thêm một script phụ để xử lý click
                    _instantiatedCards.Add(cardInstance);
                }

                cardInstance.SetActive(true); // Đảm bảo thẻ Tướng hiển thị, chống tàng hình từ Prefab
                HeroPickerCard script = cardInstance.GetComponent<HeroPickerCard>();
                script.Setup(hero, this); // Truyền tham chiếu của panel này vào card
            }
            Debug.Log($"Đã tạo/tái sử dụng xong {sortedHeroes.Count} thẻ bài.");
        }

        /// <summary>
        /// Được gọi bởi HeroPickerCard khi người dùng click vào một thẻ bài.
        /// </summary>
        public void HandleHeroSelection(HeroData selectedHero)
        {
            // Trả kết quả thông qua Callback an toàn
            _onHeroPickedCallback?.Invoke(selectedHero);

            // Sau khi chọn xong, tự động đóng panel
            ClosePanel();
        }

        private void ClosePanel()
        {
            GameManager.Instance.UIManager.GoBack();
        }

        #endregion
    }

}
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
    public class HeroPickerPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform listContainer;
        [SerializeField] private GameObject heroCardPrefab;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button closeButton;

        // Lưu trữ danh sách các card đã tạo
        private List<GameObject> _instantiatedCards = new List<GameObject>();

        // Sự kiện được phát ra khi người dùng chọn một hero.
        // Action<HeroData> là callback, mang theo hero đã được chọn.
        public static event Action<HeroData> OnHeroPicked;

        #region Unity Lifecycle

        private void Start()
        {
            if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Phương thức chính để mở và thiết lập panel.
        /// </summary>
        /// <param name="title">Tiêu đề của panel (ví dụ: "Chọn Cha", "Chọn Mẹ")</param>
        /// <param name="heroesToShow">Danh sách hero hợp lệ để hiển thị</param>
        public void Show(string title, List<HeroData> heroesToShow)
        {Debug.Log($"--- HeroPickerPanel.Show() được gọi với {heroesToShow.Count} hero. ---"); // Log 6
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
            // Dọn dẹp danh sách cũ
                foreach (var card in _instantiatedCards)
                {
                    Destroy(card);
                }
            _instantiatedCards.Clear();

            // Sắp xếp theo CP giảm dần (theo GDD)
            var sortedHeroes = heroes.OrderByDescending(h => h.GetCombatPower()).ToList();

            // Tạo card mới
            foreach (var hero in sortedHeroes)
            {Debug.Log($"Đang tạo thẻ bài cho {hero.heroName}...");
                GameObject cardInstance = Instantiate(heroCardPrefab, listContainer);
                cardInstance.SetActive(true); // Đảm bảo thẻ Tướng hiển thị, chống tàng hình từ Prefab
                HeroPickerCard cardScript = cardInstance.AddComponent<HeroPickerCard>(); // Thêm một script phụ để xử lý click
                cardScript.Setup(hero, this); // Truyền tham chiếu của panel này vào card
                _instantiatedCards.Add(cardInstance);
            }Debug.Log($"Đã tạo xong {_instantiatedCards.Count} thẻ bài.");
        }

        /// <summary>
        /// Được gọi bởi HeroPickerCard khi người dùng click vào một thẻ bài.
        /// </summary>
        public void HandleHeroSelection(HeroData selectedHero)
        {
            // Phát sự kiện toàn cục
            OnHeroPicked?.Invoke(selectedHero);

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
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace LegendOfBlood
{
    // Thêm ILocalizable vào đây
    public class POI_InfoPanel : MonoBehaviour, ILocalizable
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI poiNameText;
        [SerializeField] private TextMeshProUGUI difficultyText;
        [SerializeField] private TextMeshProUGUI recommendedCpText;
        [SerializeField] private Button exploreButton;
        [SerializeField] private Button closeButton;

        private POIData _currentPoiData;
        private Action _onExploreCallback; // Callback để báo cho WorldMapController biết nút "Khám phá" đã được nhấn

        private void Awake()
        {
            exploreButton.onClick.AddListener(OnExploreClicked);
            closeButton.onClick.AddListener(ClosePanel);
        }

        /// <summary>
        /// Hiển thị panel với thông tin của một POI cụ thể.
        /// </summary>
        public void Show(POIData poiData, Action onExplore)
        {
            _currentPoiData = poiData;
            _onExploreCallback = onExplore;

            UpdateLocalizedText(); // Gọi hàm cập nhật text

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Cập nhật lại toàn bộ text trên panel.
        /// </summary>
        public void UpdateLocalizedText()
        {
            if (_currentPoiData == null) return; // Chưa có dữ liệu thì không làm gì

            // --- THAY ĐỔI: TÍNH CP ĐỀ NGHỊ DỰA TRÊN QUÁI VẬT THỰC TẾ ---
            int recommendedCp = 0;
            if (_currentPoiData.monsterIDs != null)
            {
                foreach (var monsterId in _currentPoiData.monsterIDs)
                {
                    // Giả sử bạn có MonsterData và hàm GetMonsterByID trong DataManager
                    // var monster = DataManager.Instance.GetMonsterByID(monsterId);
                    // if (monster != null) recommendedCp += monster.GetCombatPower();
                    recommendedCp += 500; // Tạm thời cộng dồn một giá trị giả lập
                }
            }

            // Cập nhật UI
            poiNameText.text = _currentPoiData.poiName; // Tên POI đã được dịch khi tạo ra
            difficultyText.text = string.Format(LocalizationSystem.GetText("poi_difficulty_format"), _currentPoiData.difficultyLevel);
            recommendedCpText.text = string.Format(LocalizationSystem.GetText("poi_recommended_cp_format"), recommendedCp);
        }

        private void OnExploreClicked()
    {
        Debug.Log("Nút 'Explore' đã được nhấn. Đang cố gắng gọi callback...");

        // Gọi callback để thực hiện hành động tiếp theo (mở SquadSelectionPanel)
        if (_onExploreCallback != null)
        {
            _onExploreCallback.Invoke();
            Debug.Log("Callback đã được gọi thành công!");
        }
        else
        {
            Debug.LogError("LỖI: _onExploreCallback đang bị NULL!");
        }
        // ClosePanel(); // Panel này sẽ được đóng bởi WorldMapController
    }

        private void ClosePanel()
        {
            gameObject.SetActive(false);
        }
    }
}
namespace LegendOfBlood
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    /// <summary>
    /// Điều khiển Panel hiển thị thông tin chi tiết của một hero.
    /// Panel này lắng nghe sự kiện OnHeroCardClicked để tự động hiển thị và cập nhật.
    /// </summary>
    public class HeroInfoPanel : MonoBehaviour
    {
        [Header("Main Info References")]
        [SerializeField] private TextMeshProUGUI heroNameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI professionText;
        [SerializeField] private TextMeshProUGUI genderText;
        [SerializeField] private Image heroAvatarImage;

        [Header("Stats References")]
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI atkText;
        [SerializeField] private TextMeshProUGUI defText;
        [SerializeField] private TextMeshProUGUI spdText;
        [SerializeField] private TextMeshProUGUI potentialText;
        
        [Header("Traits & Skills References")]
        [Tooltip("Prefab cho một dòng thông tin (ví dụ: một trait hoặc một skill).")]
        [SerializeField] private GameObject infoItemPrefab;
        [Tooltip("Container chứa danh sách các Trait.")]
        [SerializeField] private Transform traitsContainer;
        [Tooltip("Container chứa danh sách các Skill.")]
        [SerializeField] private Transform skillsContainer;
        
        [Header("Actions")]
        [SerializeField] private Button closeButton;

        // Lưu trữ danh sách các item UI đã tạo để có thể dọn dẹp sau này
        private List<GameObject> _instantiatedInfoItems = new List<GameObject>();

        #region Unity Lifecycle & Event Subscription
        
        private void Awake()
        {
            // Gán sự kiện cho nút đóng
            if (closeButton != null)
            {
                // Thay vì gọi UIManager.GoBack(), một cách tiếp cận linh hoạt hơn là
                // chỉ cần ẩn chính panel này đi.
                closeButton.onClick.AddListener(ClosePanel);
            }
        }
        
        private void OnEnable()
        {
            // Bắt đầu lắng nghe sự kiện khi panel được kích hoạt
            EventManager.StartListening<HeroData>(GameEvents.OnHeroCardClicked, OnHeroSelected);
        }

        private void OnDisable()
        {
            // Ngừng lắng nghe để tránh lỗi và rò rỉ bộ nhớ
            EventManager.StopListening<HeroData>(GameEvents.OnHeroCardClicked, OnHeroSelected);
        }
        
        #endregion

        #region Core Logic

        /// <summary>
        /// Phương thức được gọi khi sự kiện OnHeroCardClicked được phát ra.
        /// </summary>
        private void OnHeroSelected(HeroData heroData)
        {
            // Vì HeroCard và HeroInfoPanel có thể cùng tồn tại,
            // chúng ta cần đảm bảo panel này đang được hiển thị trước khi cập nhật.
            if (!gameObject.activeInHierarchy)
            {
                // Yêu cầu UIManager hiển thị panel này.
                // Tham số 'false' để nó hiện đè lên màn hình chính.
                GameManager.Instance.UIManager.ShowPanel(UIPanelType.HeroInfo, false);
            }
            
            PopulateData(heroData);
        }
        
        /// <summary>
        /// Điền tất cả dữ liệu của hero vào các thành phần UI.
        /// </summary>
        /// <param name="hero">Dữ liệu hero cần hiển thị</param>
        private void PopulateData(HeroData hero)
        {
            if (hero == null)
            {
                Debug.LogError("PopulateData nhận vào heroData null!");
                ClosePanel();
                return;
            }

            // --- Điền thông tin cơ bản ---
            heroNameText.text = hero.heroName;
            levelText.text = string.Format(global::LocalizationSystem.GetText("level_format"), hero.level);
            professionText.text = string.Format(global::LocalizationSystem.GetText("profession_format"), hero.profession);
            genderText.text = string.Format(global::LocalizationSystem.GetText("gender_format"), hero.gender);
            // heroAvatarImage.sprite = ... (lấy sprite từ một resource manager)

            // --- Điền chỉ số ---
            // Hiển thị cả chỉ số gốc và chỉ số cuối cùng để người chơi so sánh
            HeroStats baseStats = hero.baseStats;
            HeroStats finalStats = hero.GetFinalStats();
            
            hpText.text = string.Format(global::LocalizationSystem.GetText("stats_hp_format_detailed"), baseStats.hp, finalStats.hp - baseStats.hp);
            atkText.text = string.Format(global::LocalizationSystem.GetText("stats_atk_format_detailed"), baseStats.atk, finalStats.atk - baseStats.atk);
            defText.text = string.Format(global::LocalizationSystem.GetText("stats_def_format_detailed"), baseStats.def, finalStats.def - baseStats.def);
            spdText.text = string.Format(global::LocalizationSystem.GetText("stats_spd_format_detailed"), baseStats.spd, finalStats.spd - baseStats.spd);
            potentialText.text = string.Format(global::LocalizationSystem.GetText("stats_potential_format"), hero.potential);

            // --- Dọn dẹp và điền danh sách Trait/Skill ---
            ClearInfoItems();
            PopulateInfoList(traitsContainer, hero.traitIDs, true);
            PopulateInfoList(skillsContainer, hero.skillIDs, false);
        }

        /// <summary>
        /// Tạo các item UI cho một danh sách (Trait hoặc Skill).
        /// </summary>
        private void PopulateInfoList(Transform container, List<string> idList, bool isTrait)
        {
            if (infoItemPrefab == null) return;

            foreach (string id in idList)
            {
                // Lấy dữ liệu đầy đủ từ DataManager
                string itemName = global::LocalizationSystem.GetText("unknown");
                string itemDescription = global::LocalizationSystem.GetText("description_not_found");

                if (isTrait)
                {
                    Trait trait = DataManager.Instance.GetTraitByID(id);
                    if (trait != null)
                    {
                        itemName = trait.traitName;
                        itemDescription = trait.description;
                    }
                }
                else // isSkill
                {
                    Skill skill = DataManager.Instance.GetSkillByID(id);
                    if (skill != null)
                    {
                        itemName = skill.skillName;
                        itemDescription = skill.description;
                    }
                }

                // Tạo prefab và điền dữ liệu
                GameObject itemInstance = Instantiate(infoItemPrefab, container);
                var texts = itemInstance.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 2)
                {
                    texts[0].text = itemName;
                    texts[1].text = itemDescription;
                }
                _instantiatedInfoItems.Add(itemInstance);
            }
        }

        /// <summary>
        /// Xóa tất cả các item Trait/Skill đã được tạo ra trước đó.
        /// </summary>
        private void ClearInfoItems()
        {
            foreach (var item in _instantiatedInfoItems)
            {
                Destroy(item);
            }
            _instantiatedInfoItems.Clear();
        }

        /// <summary>
        /// Đóng panel.
        /// </summary>
        private void ClosePanel()
        {
            // Yêu cầu UIManager ẩn panel này đi.
            GameManager.Instance.UIManager.HidePanel(UIPanelType.HeroInfo);
            // Hoặc có thể dùng GoBack() nếu muốn quay lại panel trước đó
            // GameManager.Instance.UIManager.GoBack();
        }

        #endregion
    }
}
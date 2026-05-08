using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LegendOfBlood
{
    public class UIKingGodPassController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI expText;
        [SerializeField] private Image fillImage;

        private void OnEnable()
        {
            InventoryManager.OnPassExpChanged += UpdateUI;
            RefreshUI();
        }

        private void OnDisable()
        {
            InventoryManager.OnPassExpChanged -= UpdateUI;
        }

        private void Start()
        {
            RefreshUI();
        }

        public void RefreshUI()
        {
            if (DataManager.Instance != null && DataManager.Instance.Player != null)
            {
                UpdateUI(DataManager.Instance.Player.passLevel, DataManager.Instance.Player.passExp);
            }
        }

        private void UpdateUI(int level, int exp)
        {
            if (levelText != null) 
            {
                levelText.text = $"LV.{level}";
            }
            
            if (InventoryManager.Instance != null)
            {
                int maxExp = InventoryManager.Instance.GetMaxExpForPassLevel(level);
                
                if (expText != null) 
                {
                    expText.text = $"{exp}/{maxExp}";
                }
                
                if (fillImage != null) 
                {
                    fillImage.fillAmount = (float)exp / maxExp;
                }
            }
        }

        /// <summary>
        /// Nút debug để test tăng kinh nghiệm Pass (Hoặc gắn vào sự kiện hoàn thành nhiệm vụ)
        /// </summary>
        public void AddExp(int amount)
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddPassExp(amount);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LegendOfBlood.UI
{
    public class PlayerLevelBadgeUI : MonoBehaviour
    {
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI expText;
        public Image expFillImage;

        private void OnEnable()
        {
            if (InventoryManager.Instance != null && GameManager.Instance != null && GameManager.Instance.DataManager != null && GameManager.Instance.DataManager.Player != null)
            {
                var player = GameManager.Instance.DataManager.Player;
                UpdateUI(player.playerLevel, player.playerExp);
            }

            InventoryManager.OnPlayerExpChanged += UpdateUI;
        }

        private void OnDisable()
        {
            InventoryManager.OnPlayerExpChanged -= UpdateUI;
        }

        private void UpdateUI(int currentLevel, int currentExp)
        {
            if (levelText != null)
            {
                levelText.text = currentLevel.ToString();
            }

            if (expFillImage != null && InventoryManager.Instance != null)
            {
                int maxExp = InventoryManager.Instance.GetMaxExpForPlayerLevel(currentLevel);
                float fillRatio = 0f;
                if (maxExp > 0)
                {
                    fillRatio = (float)currentExp / maxExp;
                }
                expFillImage.fillAmount = fillRatio;

                if (expText != null)
                {
                    if (currentLevel >= 50)
                    {
                        expText.text = "MAX";
                        expFillImage.fillAmount = 1f; // Full bar
                    }
                    else
                    {
                        expText.text = $"{currentExp}/{maxExp}";
                    }
                }
            }
        }
    }
}

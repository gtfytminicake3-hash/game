using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LegendOfBlood.GameConfigs;

namespace LegendOfBlood.UI
{
    public class PassRewardRowUI : MonoBehaviour
    {
        [Header("Level Info")]
        public TextMeshProUGUI levelText;
        public Image progressFill; // Sẽ hiện đầy nếu đã vượt level

        [Header("Free Reward")]
        public Image freeIcon;
        public TextMeshProUGUI freeAmountText;
        public Button freeClaimBtn;
        public GameObject freeClaimedOverlay; // Che khi đã nhận

        [Header("Premium Reward")]
        public Image premiumIcon;
        public TextMeshProUGUI premiumAmountText;
        public Button premiumClaimBtn;
        public GameObject premiumClaimedOverlay; // Che khi đã nhận
        public GameObject premiumLockedOverlay; // Che khi chưa mua VIP

        private int _level;

        public void Initialize(PassLevelData data, bool isReached, bool isFreeClaimed, bool isPremiumClaimed, bool hasPremiumPass)
        {
            _level = data.level;
            
            if (levelText != null) levelText.text = data.level.ToString();
            if (progressFill != null) progressFill.fillAmount = isReached ? 1f : 0f;

            // Setup Free
            if (freeAmountText != null) freeAmountText.text = data.freeReward.amount.ToString();
            // TODO: Load Icon based on data.freeReward.resourceType or itemID here
            
            if (freeClaimedOverlay != null) freeClaimedOverlay.SetActive(isFreeClaimed);
            if (freeClaimBtn != null) 
            {
                freeClaimBtn.interactable = isReached && !isFreeClaimed;
                freeClaimBtn.onClick.RemoveAllListeners();
                freeClaimBtn.onClick.AddListener(() => OnClaimFreeClick());
            }

            // Setup Premium
            if (premiumAmountText != null) premiumAmountText.text = data.premiumReward.amount.ToString();
            
            if (premiumLockedOverlay != null) premiumLockedOverlay.SetActive(!hasPremiumPass);
            if (premiumClaimedOverlay != null) premiumClaimedOverlay.SetActive(isPremiumClaimed);
            
            if (premiumClaimBtn != null)
            {
                premiumClaimBtn.interactable = isReached && hasPremiumPass && !isPremiumClaimed;
                premiumClaimBtn.onClick.RemoveAllListeners();
                premiumClaimBtn.onClick.AddListener(() => OnClaimPremiumClick());
            }
        }

        private void OnClaimFreeClick()
        {
            InventoryManager.Instance?.ClaimPassReward(_level, false);
        }

        private void OnClaimPremiumClick()
        {
            InventoryManager.Instance?.ClaimPassReward(_level, true);
        }
    }
}

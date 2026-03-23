namespace LegendOfBlood
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using LegendOfBlood.Managers;
    using LegendOfBlood.GameConfigs;

    public class QuestItemUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI targetDescriptionText;
        [SerializeField] private Image rewardIcon;
        [SerializeField] private TextMeshProUGUI rewardAmountText;
        [SerializeField] private Button claimButton;
        [SerializeField] private GameObject completedIndicator;

        private PlayerQuestStatus _status;
        private QuestData _data;

        public void Setup(PlayerQuestStatus status)
        {
            _status = status;
            _data = GameManager.Instance.QuestManager.GetQuestData(status.questId);

            if (_data == null)
            {
                Debug.LogError($"[QuestItemUI] Không tìm thấy dữ liệu Quest cho ID: {status.questId}");
                return;
            }

            if (titleText != null) titleText.text = global::LocalizationSystem.GetText(_data.questName);
            if (targetDescriptionText != null) targetDescriptionText.text = global::LocalizationSystem.GetText(_data.description);
            
            // Xử lý hiển thị tiến độ (ví dụ Kill 5/10)
            if (progressText != null)
            {
                if (_data.targetValue > 1) 
                    progressText.text = $"{_status.currentProgress}/{_data.targetValue}";
                else 
                    progressText.text = _status.state == QuestState.Completed ? "1/1" : "0/1";
            }

            // Xử lý hiển thị phần thưởng
            if (_data.rewards != null && _data.rewards.Count > 0)
            {
                var firstReward = _data.rewards[0];
                if (rewardAmountText != null) rewardAmountText.text = firstReward.amount.ToString();
                // TODO: set rewardIcon depending on firstReward.type
            }

            // Gắn sự kiện nút Claim
            if (claimButton != null)
            {
                claimButton.onClick.RemoveAllListeners();
                claimButton.onClick.AddListener(OnClaimClicked);
                
                // Nút chỉ bấm được khi Quest đã hoàn thành nhưng chưa nhận thưởng
                claimButton.interactable = (_status.state == QuestState.Completed);
                
                // Ẩn nút nếu đã claim, hiện chữ 'Completed'
                bool isClaimed = (_status.state == QuestState.Claimed);
                claimButton.gameObject.SetActive(!isClaimed);
                if (completedIndicator != null) completedIndicator.SetActive(isClaimed);
            }
        }

        private void OnClaimClicked()
        {
            if (_status.state == QuestState.Completed)
            {
                // Gọi hàm nhận thưởng thông qua GameManager
                bool claimSuccess = true; // Có thể bắt return type nếu muốn an toàn hơn
                GameManager.Instance.QuestManager.ClaimReward(_status.questId);
                
                if (claimSuccess)
                {
                    // Sau khi bấm nhận thưởng, thay đổi UI tại chỗ
                    claimButton.gameObject.SetActive(false);
                    if (completedIndicator != null) completedIndicator.SetActive(true);
                    
                    // Force refresh panel to rearrange the list
                    var questPanel = GetComponentInParent<QuestPanel>();
                    if (questPanel != null)
                    {
                        questPanel.RefreshQuestList(); // Update UI in parent
                    }
                }
            }
        }
    }
}

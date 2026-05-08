using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LegendOfBlood.UI
{
    public class ArenaPanel : MonoBehaviour
    {
        [Header("Top Bar")]
        public TextMeshProUGUI goldText;
        public TextMeshProUGUI cupsText;
        public TextMeshProUGUI crystalsText;

        [Header("Defense Team")]
        public Image pedestalImage;
        public RectTransform charactersContainer;

        [Header("Opponent Selection")]
        public Button[] opponentButtons;

        [Header("Main Actions")]
        public Button findMatchButton;

        [Header("Right Menu")]
        public Button settingsButton;
        public Button mailButton;
        public Button questsButton;

        private void OnEnable()
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            var player = GameManager.Instance.DataManager.Player;
            if (player != null)
            {
                if (goldText != null) goldText.text = player.resources.gold.ToString("N0");
                if (cupsText != null) cupsText.text = player.arenaPoints.ToString("N0");
                if (crystalsText != null) crystalsText.text = player.resources.diamond.ToString("N0");
            }
        }

        private void Start()
        {
            if (findMatchButton != null)
                findMatchButton.onClick.AddListener(OnFindMatchClicked);
                
            if (settingsButton != null) settingsButton.onClick.AddListener(() => GameManager.Instance.UIManager.ShowPanel(UIPanelType.Settings, false));
            if (mailButton != null) mailButton.onClick.AddListener(() => GameManager.Instance.UIManager.ShowPanel(UIPanelType.Mailbox, false));
            if (questsButton != null) questsButton.onClick.AddListener(() => GameManager.Instance.UIManager.ShowPanel(UIPanelType.Quest, false));
            
            // Cleanup hardcoded opponent buttons
            if (opponentButtons != null)
            {
                foreach(var btn in opponentButtons)
                {
                    if (btn != null) btn.gameObject.SetActive(false); // Disable mock opponents
                }
            }
        }

        private void OnFindMatchClicked()
        {
            var player = GameManager.Instance.DataManager.Player;
            if (player != null)
            {
                ArenaSystem.Instance.FindOpponent(player.arenaPoints);
            }
        }
    }
}
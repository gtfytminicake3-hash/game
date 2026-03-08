using UnityEngine;
using UnityEngine.UI;

namespace LegendOfBlood.UI
{
    public class MenuPanel : UIPanel
    {
        [Header("Close Menu")]
        [SerializeField] private Button closeButton;

        [Header("Navigation Buttons")]
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button questButton;
        [SerializeField] private Button mailboxButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button recruitmentButton;
        [SerializeField] private Button settingButton;

        private void Awake()
        {
            // Gán PanelType khi khởi tạo để UIManager nhận diện đúng
            PanelType = UIPanelType.Menu;
        }

        private void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(ClosePanel);

            // Hook up navigation buttons to UIManager
            if (inventoryButton != null)
                inventoryButton.onClick.AddListener(() => OpenPanel(UIPanelType.Inventory));

            if (questButton != null)
                questButton.onClick.AddListener(() => OpenPanel(UIPanelType.Quest));

            if (mailboxButton != null)
                mailboxButton.onClick.AddListener(() => OpenPanel(UIPanelType.Mailbox));

            if (shopButton != null)
                shopButton.onClick.AddListener(() => OpenPanel(UIPanelType.ArenaShop)); // Tạm coi ArenaShop làm Shop chung

            if (recruitmentButton != null)
                recruitmentButton.onClick.AddListener(() => OpenPanel(UIPanelType.Recruitment));

            if (settingButton != null)
                settingButton.onClick.AddListener(() => OpenPanel(UIPanelType.Settings));
        }

        private void OpenPanel(UIPanelType type)
        {
            if (GameManager.Instance != null && GameManager.Instance.UIManager != null)
            {
                // Mở Panel được chỉ định, true sẽ ẩn MenuPanel đi
                GameManager.Instance.UIManager.ShowPanel(type, true);
            }
        }

        private void ClosePanel()
        {
            if (GameManager.Instance != null && GameManager.Instance.UIManager != null)
            {
                GameManager.Instance.UIManager.GoBack();
            }
        }
    }
}

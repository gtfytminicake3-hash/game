using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

namespace LegendOfBlood
{
    public class NodeDetailPopup : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI monstersText;
        public TextMeshProUGUI lootText;
        public Button btnClose;
        public Button btnAction;
        public TextMeshProUGUI btnActionText;
        public Button btnLose; // Nút giả lập THUA NGAY LẬP TỨC
        public TextMeshProUGUI btnLoseText;

        private SubStageNode _currentNode;
        private Action _onActionExecute;
        private Action _onLoseExecute;

        private void Awake()
        {
            AutoHook();
        }

        private void AutoHook()
        {
            if (titleText == null) titleText = transform.Find("Title")?.GetComponent<TextMeshProUGUI>() ?? transform.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(t => t.name.Contains("Title"));
            if (monstersText == null) monstersText = transform.Find("Description")?.GetComponent<TextMeshProUGUI>() ?? transform.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(t => t.name.Contains("Desc") || t.name.Contains("Monster"));
            if (lootText == null) lootText = transform.Find("Drops")?.GetComponent<TextMeshProUGUI>() ?? transform.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(t => t.name.Contains("Loot") || t.name.Contains("Reward"));
            if (btnClose == null) btnClose = transform.Find("Btn_Close")?.GetComponent<Button>() ?? transform.Find("CloseButton")?.GetComponent<Button>() ?? transform.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name.Contains("Close"));
            if (btnAction == null) btnAction = transform.Find("Btn_Combat")?.GetComponent<Button>() ?? transform.Find("Btn_Action")?.GetComponent<Button>() ?? transform.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name.Contains("Action") || b.name.Contains("Combat") || b.name.Contains("Enter"));
            if (btnActionText == null && btnAction != null) btnActionText = btnAction.GetComponentInChildren<TextMeshProUGUI>();
            if (btnLose == null) btnLose = transform.Find("Btn_Lose")?.GetComponent<Button>() ?? transform.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name.Contains("Lose") || b.name.Contains("Surrender"));
            if (btnLoseText == null && btnLose != null) btnLoseText = btnLose.GetComponentInChildren<TextMeshProUGUI>();
        }

        private void Start()
        {
            if (btnClose != null) btnClose.onClick.AddListener(Close);
            if (btnAction != null) btnAction.onClick.AddListener(ExecuteAction);
            if (btnLose != null) btnLose.onClick.AddListener(ExecuteLose);
        }

        public void Show(SubStageNode node, Action onActionExecute, Action onLoseExecute = null)
        {
            _currentNode = node;
            _onActionExecute = onActionExecute;
            _onLoseExecute = onLoseExecute;
            this.gameObject.SetActive(true);

            if (titleText != null)
                titleText.text = $"<size=45><color=#D4AF37><b>Tầng {node.Floor} - {node.Type}</b></color></size>\n";

            if (monstersText != null)
            {
                if (node.ExpectedMonsters.Count > 0)
                    monstersText.text = "Quái thú dự kiến:\n- " + string.Join("\n- ", node.ExpectedMonsters);
                else
                    monstersText.text = "Không có nguy hiểm dự kiến.";
            }

            if (lootText != null)
            {
                if (node.ExpectedRewards.Count > 0)
                    lootText.text = "Phần thưởng mong đợi:\n- " + string.Join("\n- ", node.ExpectedRewards);
                else
                    lootText.text = "Không có báo cáo về kho báu.";
            }

            if (btnAction != null && btnActionText != null)
            {
                btnAction.gameObject.SetActive(true);
                
                if (node.Status == NodeStatus.Cleared)
                {
                    btnActionText.text = "Đã Dọn Dẹp";
                    btnAction.interactable = false;
                }
                else if (node.Status == NodeStatus.Available)
                {
                    btnActionText.text = "Tiến Vào";
                    btnAction.interactable = true;
                }
                else // Locked
                {
                    btnActionText.text = "Chưa Mở Khóa";
                    btnAction.interactable = false;
                }
            }

            if (btnLose != null)
            {
                // Tắt nút giả lập thua
                btnLose.gameObject.SetActive(false);
            }
        }

        private void ExecuteAction()
        {
            this.gameObject.SetActive(false);
            _onActionExecute?.Invoke();
        }

        private void ExecuteLose()
        {
            this.gameObject.SetActive(false);
            _onLoseExecute?.Invoke();
        }

        private void Close()
        {
            this.gameObject.SetActive(false);
        }
    }
}

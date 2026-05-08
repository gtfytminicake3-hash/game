using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

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
                else if (node.Status == NodeStatus.Available || node.Status == NodeStatus.InProgress)
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

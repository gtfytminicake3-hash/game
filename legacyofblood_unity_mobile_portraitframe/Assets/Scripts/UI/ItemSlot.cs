using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LegendOfBlood
{
    public class ItemSlot : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private Button clickButton;

        private ItemData _currentItem;
        private int _currentAmount;

        public delegate void ItemSlotClickedEvent(ItemData item, int amount);
        public event ItemSlotClickedEvent OnClicked;

        private void Awake()
        {
            if (clickButton != null)
            {
                clickButton.onClick.AddListener(OnSlotClicked);
            }
        }

        public void Setup(ItemData item, int amount)
        {
            _currentItem = item;
            _currentAmount = amount;

            if (_currentItem != null)
            {
                iconImage.sprite = _currentItem.icon;
                iconImage.enabled = true;
                amountText.text = amount > 1 ? amount.ToString() : "";
            }
            else
            {
                ClearSlot();
            }
        }

        public void ClearSlot()
        {
            _currentItem = null;
            _currentAmount = 0;
            iconImage.sprite = null;
            iconImage.enabled = false;
            amountText.text = "";
        }

        private void OnSlotClicked()
        {
            if (_currentItem != null)
            {
                OnClicked?.Invoke(_currentItem, _currentAmount);
            }
        }
    }
}

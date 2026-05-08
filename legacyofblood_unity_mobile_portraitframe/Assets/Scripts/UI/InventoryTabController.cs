using UnityEngine;
using UnityEngine.UI;

public class InventoryTabController : MonoBehaviour
{
    [Header("Tab Buttons")]
    public Button btnItems;
    public Button btnEquip;

    [Header("Visual Settings")]
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    [Header("Optional: Panels to Switch")]
    public GameObject itemsPanel;
    public GameObject equipPanel;

    private void Start()
    {
        // Gắn sự kiện khi bấm nút
        if (btnItems != null) btnItems.onClick.AddListener(OnClickItems);
        if (btnEquip != null) btnEquip.onClick.AddListener(OnClickEquip);

        // Mặc định chọn tab Items khi mới mở
        OnClickItems();
    }

    public void OnClickItems()
    {
        // 1. Chỉnh màu sáng tối
        SetTabState(btnItems, true);
        SetTabState(btnEquip, false);

        // 2. Chuyển đổi Panel hiển thị (Nếu có)
        if (itemsPanel != null) itemsPanel.SetActive(true);
        if (equipPanel != null) equipPanel.SetActive(false);
    }

    public void OnClickEquip()
    {
        // 1. Chỉnh màu sáng tối
        SetTabState(btnItems, false);
        SetTabState(btnEquip, true);

        // 2. Chuyển đổi Panel hiển thị (Nếu có)
        if (itemsPanel != null) itemsPanel.SetActive(false);
        if (equipPanel != null) equipPanel.SetActive(true);
    }

    private void SetTabState(Button btn, bool isActive)
    {
        if (btn == null) return;

        Image bgImage = btn.GetComponent<Image>();
        if (bgImage != null)
        {
            // Nếu Tab đang active -> Màu sáng (Trắng tinh). Nếu Inactive -> Màu bị tối đi.
            bgImage.color = isActive ? activeColor : inactiveColor;
        }

        // Ưu tiên làm nổi chữ (Tuỳ chọn nếu có Text con)
        Text txt = btn.GetComponentInChildren<Text>();
        if (txt != null)
        {
            txt.color = isActive ? Color.black : Color.white; // Màu chữ thay đổi cho nổi
        }
    }
}

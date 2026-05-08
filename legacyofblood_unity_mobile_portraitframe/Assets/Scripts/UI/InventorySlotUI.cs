using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public string itemName;
    [TextArea(3, 5)]
    public string itemDesc;
    public Sprite itemIcon; // Ảnh của vật phẩm nằm ở bên trong

    private void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn == null) btn = gameObject.AddComponent<Button>();
        btn.onClick.AddListener(OnClickSlot);
    }

    public void OnClickSlot()
    {
        if (InventoryDetailUI.Instance != null)
        {
            InventoryDetailUI.Instance.ShowDetail(itemName, itemDesc, itemIcon);
        }
        else
        {
            Debug.LogWarning("Chưa có InventoryDetailUI trong Scene. Hãy gắn nó vào Bảng Detail nhé!");
        }
    }
}

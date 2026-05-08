using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryDetailUI : MonoBehaviour
{
    public static InventoryDetailUI Instance;

    [Header("1. Khung cửa sổ Detail (Để Bật/Tắt)")]
    [Tooltip("Kéo ĐÚNG CÁI BẢNG GỐC CỦA DETAIL vào đây")]
    public GameObject detailWindow;

    [Header("2. Các thành phần thông tin (Text & Ảnh)")]
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescText;
    public Image itemIconImage;

    private void Awake()
    {
        // Luôn gán Instance, vì script này giờ sẽ nằm ở Panel ngoài cùng (luôn Active)
        Instance = this;
        
        // Mặc định ẩn bảng Detail khi mới vô game
        if (detailWindow != null) detailWindow.SetActive(false);
    }

    public void ShowDetail(string name, string desc, Sprite icon)
    {
        // Nếu người chơi lỡ bấm vào ô trống, thì tắt/ẩn Detail đi (hoặc kệ nó)
        if (string.IsNullOrEmpty(name) || name.Contains("Ô trống"))
        {
            if (detailWindow != null) detailWindow.SetActive(false);
            return;
        }

        // Bật cửa sổ Detail nổi lên!
        if (detailWindow != null) detailWindow.SetActive(true);

        // Đổ dữ liệu chữ và ảnh vào các ô chứa
        if (itemNameText != null) itemNameText.text = name;
        if (itemDescText != null) itemDescText.text = desc;
        
        if (itemIconImage != null)
        {
            itemIconImage.sprite = icon;
            itemIconImage.color = (icon == null) ? new Color(1,1,1,0) : Color.white;
            itemIconImage.preserveAspect = true; // Chống méo ảnh
        }
    }

    // Dùng cái này nhét cho một nút (X) Close để gập cái Detail đi nhé
    public void HideDetail()
    {
        if (detailWindow != null) detailWindow.SetActive(false);
    }
}

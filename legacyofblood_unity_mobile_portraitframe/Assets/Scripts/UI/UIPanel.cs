using UnityEngine;
using UnityEngine.UI;

namespace LegendOfBlood 
{
    public class UIPanel : MonoBehaviour 
    {
        [Tooltip("Loại của panel này. UIManager sẽ dùng thông tin này để nhận diện.")]
        public UIPanelType PanelType;

        [Header("Dynamic Background")]
        public bool useDynamicBackground = false;
        public string backgroundResourcePath = "UI/Panel/panel_dong_frames";

        protected virtual void Start()
        {
            // Kiểm tra cài đặt toàn cục trong DataManager
            bool isDynamicUIEnabled = false;
            if (DataManager.Instance != null && DataManager.Instance.Player != null)
            {
                isDynamicUIEnabled = DataManager.Instance.Player.useDynamicUI;
            }

            if (useDynamicBackground && isDynamicUIEnabled)
            {
                SetupDynamicBackground();
            }
        }

        private void SetupDynamicBackground()
        {
            // Sử dụng SpriteSequencePlayer để phát chuỗi ảnh đã cắt từ GIF
            var sequencePlayer = GetComponent<SpriteSequencePlayer>();
            if (sequencePlayer == null) sequencePlayer = gameObject.AddComponent<SpriteSequencePlayer>();
            
            if (sequencePlayer != null)
            {
                sequencePlayer.resourcePath = backgroundResourcePath;
                sequencePlayer.frameRate = 16.67f; // Tốc độ gốc của GIF (1000/60)
                sequencePlayer.loop = true;
                sequencePlayer.playOnAwake = true;
            }

            // Đảm bảo Image hoặc RawImage component sẵn sàng hiển thị màu trắng (không bị đen/trong suốt)
            var graphic = GetComponent<Graphic>();
            if (graphic != null) graphic.color = Color.white;
        }
    }
}
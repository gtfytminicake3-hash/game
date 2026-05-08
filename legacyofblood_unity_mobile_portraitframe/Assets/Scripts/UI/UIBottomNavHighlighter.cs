using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace LegendOfBlood {
    public class UIBottomNavHighlighter : MonoBehaviour {

        public Color normalIconColor = new Color(1f, 1f, 1f, 0.5f);
        public Color activeIconColor = new Color(1f, 0.8f, 0.2f, 1f); 
        public Color normalTextColor = new Color(1f, 1f, 1f, 0.5f);
        public Color activeTextColor = Color.white;
        
        private UIPanelNavButton[] _navBtns;

        void Awake() {
            _navBtns = GetComponentsInChildren<UIPanelNavButton>();
        }

        void Update() {
            if (GameManager.Instance == null || GameManager.Instance.UIManager == null) return;
            UIPanelType current = GameManager.Instance.UIManager.CurrentPanel;
            
            foreach(var navBtn in _navBtns) {
                bool isActive = (current == navBtn.targetPanel);
                Transform tabObj = navBtn.transform;
                
                Transform iconT = tabObj.Find("Icon");
                if (iconT != null) {
                    Image img = iconT.GetComponent<Image>();
                    
                    // Để icon giữ màu thực tế (giống ảnh gốc), khi Active thì màu Trắng 100%, inactive thì Trắng 50%
                    if (img != null) img.color = isActive ? Color.white : normalIconColor;

                    // --- Aura Glow Effect ---
                    // Aura phải nằm cùng cấp với Icon (dưới tabObj) để vẽ TRƯỚC Icon
                    Transform glowT = tabObj.Find("AuraGlow");
                    if (isActive)
                    {
                        if (glowT == null)
                        {
                            GameObject glowObj = new GameObject("AuraGlow");
                            glowObj.transform.SetParent(tabObj, false);
                            
                            // Mấu chốt: Chèn Aura vào ngay trước vị trí của Icon trong cây thư mục
                            // Do đó Unity sẽ vẽ Aura xong rồi vẽ Icon đè lên trên!
                            glowObj.transform.SetSiblingIndex(iconT.GetSiblingIndex());
                            
                            Image glowImg = glowObj.AddComponent<Image>();
                            glowImg.sprite = GetRadialGlow(); // Dùng hào quang tròn toả ra ánh sáng
                            glowImg.raycastTarget = false;
                            
                            RectTransform glowRt = glowObj.GetComponent<RectTransform>();
                            RectTransform iconRt = iconT.GetComponent<RectTransform>();
                            glowRt.anchorMin = iconRt.anchorMin;
                            glowRt.anchorMax = iconRt.anchorMax;
                            glowRt.anchoredPosition = iconRt.anchoredPosition; // Copy tọa độ để nằm đúng giữa Icon
                            glowRt.sizeDelta = new Vector2(220, 220); // Toả ra vùng sáng 220x220
                            glowT = glowObj.transform;
                        }

                        glowT.gameObject.SetActive(true);
                        
                        // Pulse animation (nhịp thở hào quang nhẹ nhàng)
                        float pulse = Mathf.Abs(Mathf.Sin(Time.time * 2f));
                        float scale = 1.0f + (pulse * 0.15f); // Nhấp nhô 15%
                        glowT.localScale = new Vector3(scale, scale, 1f);
                        
                        Image gImg = glowT.GetComponent<Image>();
                        if (gImg != null)
                        {
                            // Ánh sáng màu đỏ tỏa ra
                            gImg.color = new Color(0.9f, 0.1f, 0.1f, 0.2f + pulse * 0.4f);
                        }
                    }
                    else
                    {
                        if (glowT != null) glowT.gameObject.SetActive(false);
                    }
                    // ------------------------
                }

                Transform textT = tabObj.Find("Text");
                if (textT != null) {
                    TMPro.TextMeshProUGUI txt = textT.GetComponent<TMPro.TextMeshProUGUI>();
                    if (txt != null) txt.color = isActive ? activeTextColor : normalTextColor;
                }
            }
        }

        private Sprite _radialGlowSprite;
        private Sprite GetRadialGlow()
        {
            if (_radialGlowSprite != null) return _radialGlowSprite;
            int size = 128; // Độ mịn của hào quang
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(center, new Vector2(x, y));
                    float alpha = Mathf.Clamp01(1f - (dist / radius));
                    alpha = Mathf.Pow(alpha, 1.5f); // Smooth falloff tạo độ toả mềm mại
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            _radialGlowSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            return _radialGlowSprite;
        }
    }
}

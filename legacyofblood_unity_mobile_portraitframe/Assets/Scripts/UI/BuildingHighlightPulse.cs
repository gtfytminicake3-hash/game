using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BuildingHighlightPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [Tooltip("Tốc độ chớp tắt của viền sáng (vòng/giây)")]
    public float pulseSpeed = 1.0f;
    
    [Tooltip("Độ sáng tối thiểu (Alpha)")]
    [Range(0f, 1f)]
    public float minAlpha = 0.3f;
    
    [Tooltip("Độ sáng tối đa (Alpha)")]
    [Range(0f, 1f)]
    public float maxAlpha = 1.0f;

    [Tooltip("Màu sắc của viền phát sáng")]
    public Color pulseColor = Color.yellow;

    private Image _image;
    private Outline _outline;

    void Awake()
    {
        _image = GetComponent<Image>();
        _outline = GetComponent<Outline>();

        // Tự động thêm Outline nếu chưa có
        if (_outline == null)
        {
            _outline = gameObject.AddComponent<Outline>();
            _outline.effectDistance = new Vector2(4f, -4f); // Độ dày viền mặc định
            _outline.useGraphicAlpha = false;
        }

        // Thiết lập màu sắc ban đầu
        if (_outline != null)
        {
            _outline.effectColor = new Color(pulseColor.r, pulseColor.g, pulseColor.b, maxAlpha);
        }
    }

    void Update()
    {
        if (_outline != null)
        {
            // Tính toán giá trị Alpha nhấp nháy theo sóng Sin theo thời gian
            float t = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) + 1f) / 2f; 
            float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, t);

            // Cập nhật màu của Outline
            Color newColor = new Color(pulseColor.r, pulseColor.g, pulseColor.b, currentAlpha);
            _outline.effectColor = newColor;
        }
    }
}

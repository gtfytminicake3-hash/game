using UnityEngine;

public class PulseAnimation : MonoBehaviour
{
    public float scaleSpeed = 4f;
    public float scaleAmount = 0.05f;
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        float scale = 1f + Mathf.Sin(Time.time * scaleSpeed) * scaleAmount;
        transform.localScale = originalScale * scale;
    }
}

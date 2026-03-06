namespace LegendOfBlood
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using System.Collections;
    using DG.Tweening; // Giả định project có dùng DOTween cho animation mượt. Nếu không có DOTween ta sẽ dùng coroutine cơ bản.

    public class BattleUnitUI : MonoBehaviour
    {
        [Header("UI Component")]
        public Image avatarImage;
        public Slider hpSlider;
        public TextMeshProUGUI hpText;
        public CanvasGroup damageTextCanvasGroup;
        public TextMeshProUGUI damageText;

        private int _maxHp;
        private int _currentHp;
        private Vector3 _originalPosition;

        private void Awake()
        {
            if (damageTextCanvasGroup != null)
            {
                damageTextCanvasGroup.alpha = 0;
            }
        }

        public void Setup(Sprite portrait, int maxHp, int currentHp)
        {
            if (avatarImage != null && portrait != null)
            {
                avatarImage.sprite = portrait;
            }
            
            _maxHp = maxHp;
            _currentHp = currentHp;
            
            _originalPosition = transform.localPosition; // Lưu vị trí gốc để diễn hoạt đập nhau

            UpdateHpBar(false);
        }

        private void UpdateHpBar(bool animate = true)
        {
            if (hpSlider != null)
            {
                hpSlider.maxValue = _maxHp;
                if (animate)
                {
                    // Tạm thời gán trực tiếp, nâng cấp bằng Lerp sau nếu cần
                    hpSlider.value = _currentHp;
                }
                else
                {
                    hpSlider.value = _currentHp;
                }
            }

            if (hpText != null)
            {
                hpText.text = $"{_currentHp}/{_maxHp}";
            }
            
            // Xám ảnh nếu chết
            if (_currentHp <= 0 && avatarImage != null)
            {
                avatarImage.color = Color.gray;
            }
        }

        public void TakeDamage(int damage, bool isCrit)
        {
            _currentHp = Mathf.Max(0, _currentHp - damage);
            UpdateHpBar(true);
            ShowFloatingText("-" + damage.ToString(), isCrit ? Color.yellow : Color.red, isCrit);
            StartCoroutine(FlashRedRoutine());
        }

        public void Heal(int amount)
        {
            _currentHp = Mathf.Min(_maxHp, _currentHp + amount);
            UpdateHpBar(true);
            ShowFloatingText("+" + amount.ToString(), Color.green, false);
        }

        public IEnumerator PlayAttackAnim(Vector3 targetPosition)
        {
            // Trượt lên tấn công (Simple Lerp)
            Vector3 startPos = transform.localPosition;
            Vector3 attackPos = startPos + (targetPosition - startPos).normalized * 50f; // Nhích lên 50 pixel

            float t = 0;
            float duration = 0.1f;
            while(t < duration)
            {
                t += Time.deltaTime;
                transform.localPosition = Vector3.Lerp(startPos, attackPos, t / duration);
                yield return null;
            }
            
            transform.localPosition = attackPos;

            // Giật về
            t = 0;
            while(t < duration)
            {
                t += Time.deltaTime;
                transform.localPosition = Vector3.Lerp(attackPos, startPos, t / duration);
                yield return null;
            }
            transform.localPosition = startPos;
        }

        private IEnumerator FlashRedRoutine()
        {
            if (avatarImage != null)
            {
                Color originalColor = avatarImage.color;
                avatarImage.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                if (_currentHp > 0) avatarImage.color = originalColor; // Giữ xám nếu chết
            }
        }

        private void ShowFloatingText(string msg, Color textColor, bool isCrit)
        {
            if (damageText == null || damageTextCanvasGroup == null) return;

            damageText.text = msg + (isCrit ? " C.HIT!" : "");
            damageText.color = textColor;
            damageText.fontSize = isCrit ? 40 : 30; // Chữ to hơn nếu bạo kích

            StartCoroutine(FloatTextRoutine());
        }

        private IEnumerator FloatTextRoutine()
        {
            damageTextCanvasGroup.alpha = 1;
            RectTransform rect = damageText.GetComponent<RectTransform>();
            Vector3 startPos = Vector3.zero; // Local to CanvasGroup
            Vector3 endPos = startPos + new Vector3(0, 50f, 0); // Bay lên 50px

            float duration = 0.5f;
            float t = 0;

            while (t < duration)
            {
                t += Time.deltaTime;
                float normalizedTime = t / duration;
                rect.localPosition = Vector3.Lerp(startPos, endPos, normalizedTime);
                damageTextCanvasGroup.alpha = 1f - normalizedTime; // Mờ dần
                yield return null;
            }

            damageTextCanvasGroup.alpha = 0;
            rect.localPosition = startPos;
        }
    }
}

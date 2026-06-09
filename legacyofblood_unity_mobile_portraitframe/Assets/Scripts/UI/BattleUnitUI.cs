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
        public Image hpFill;
        public TextMeshProUGUI hpText;
        public CanvasGroup damageTextCanvasGroup;
        public TextMeshProUGUI damageText;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI cpText;
        public TextMeshProUGUI levelText;

        private int _maxHp;
        private int _currentHp;
        private Vector3 _originalPosition;
        private Coroutine _flashRoutine;

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

        public void SetupReplayUnit(LegendOfBlood.Combat.CombatReplayUnitSnapshot snap, Sprite portrait)
        {
            if (nameText == null || cpText == null || levelText == null || hpText == null || (hpSlider == null && hpFill == null))
            {
                var texts = GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in texts)
                {
                    if (t.name == "NameText" && nameText == null) nameText = t;
                    if (t.name == "CPText" && cpText == null) cpText = t;
                    if (t.name == "LevelText" && levelText == null) levelText = t;
                    if (t.name == "HpText" && hpText == null) hpText = t;
                }

                if (hpSlider == null) hpSlider = GetComponentInChildren<Slider>(true);
                
                if (hpSlider == null && hpFill == null) 
                {
                    var images = GetComponentsInChildren<Image>(true);
                    foreach (var img in images)
                    {
                        if (img.name.Contains("Fill") && img != avatarImage)
                        {
                            hpFill = img;
                            break;
                        }
                    }
                }
            }

            if (nameText != null) nameText.text = snap.displayName;
            if (cpText != null) cpText.text = "CP: " + snap.combatPower;
            if (levelText != null) levelText.text = "Cấp " + snap.level;

            Setup(portrait, (int)snap.maxHp, (int)snap.startHp);
        }

        private void UpdateHpBar(bool animate = true)
        {
            _currentHp = Mathf.Clamp(_currentHp, 0, Mathf.Max(1, _maxHp));
            float hpRatio = _maxHp > 0 ? (float)_currentHp / _maxHp : 0f;
            hpRatio = Mathf.Clamp01(hpRatio);

            if (hpSlider != null)
            {
                hpSlider.minValue = 0f;
                hpSlider.maxValue = 1f;
                hpSlider.value = hpRatio;
            }

            if (hpFill != null)
            {
                hpFill.fillAmount = hpRatio;
            }

            if (hpText != null)
            {
                hpText.text = $"{_currentHp}/{Mathf.Max(1, _maxHp)} HP";
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
            
            if (_flashRoutine != null) StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRedRoutine());
        }

        public void Heal(int amount)
        {
            _currentHp = Mathf.Min(_maxHp, _currentHp + amount);
            UpdateHpBar(true);
            ShowFloatingText("+" + amount.ToString(), Color.green, false);
        }

        public IEnumerator PlayAttackAnim(Vector3 targetWorldPosition)
        {
            Vector3 startPos = transform.position;
            // Tiến tới cách mục tiêu một đoạn ngắn
            Vector3 dir = (targetWorldPosition - startPos).normalized;
            Vector3 attackPos = targetWorldPosition - dir * 0.5f;

            // Kéo Unit lên trên cùng để diễn hoạt không bị che bởi ô Grid khác
            Canvas canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 999;

            float t = 0;
            float duration = 0.2f; // Tốc độ lướt tới
            while(t < duration)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, attackPos, t / duration);
                yield return null;
            }
            
            transform.position = attackPos;

            // Giật lùi về
            t = 0;
            duration = 0.15f;
            while(t < duration)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(attackPos, startPos, t / duration);
                yield return null;
            }
            transform.position = startPos;

            if (canvas != null) canvas.overrideSorting = false;
        }

        private IEnumerator FlashRedRoutine()
        {
            if (avatarImage != null)
            {
                avatarImage.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                if (_currentHp > 0) 
                    avatarImage.color = Color.white; 
                else 
                    avatarImage.color = Color.gray;
            }
            _flashRoutine = null;
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

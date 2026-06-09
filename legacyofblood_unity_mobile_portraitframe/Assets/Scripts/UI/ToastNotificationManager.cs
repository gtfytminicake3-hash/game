using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace LegendOfBlood
{
    public class ToastNotificationManager : MonoBehaviour
    {
        private static ToastNotificationManager _instance;
        public static ToastNotificationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("ToastNotificationManager");
                    _instance = go.AddComponent<ToastNotificationManager>();
                    DontDestroyOnLoad(go);
                    _instance.Initialize();
                }
                return _instance;
            }
        }

        private Canvas _canvas;
        private GameObject _toastPrefab;
        private List<ToastItem> _activeToasts = new List<ToastItem>();

        private const int MAX_TOASTS = 8;
        public const float TOAST_SPAWN_Y = 80f;
        private const float TOAST_SPACING = 80f; // Height (60) + Spacing (20)

        private void Initialize()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 9999;
            gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            gameObject.AddComponent<GraphicRaycaster>(); // Although toasts won't block clicks

            // Generate a simple toast prefab
            _toastPrefab = new GameObject("ToastPrefab");
            _toastPrefab.transform.SetParent(this.transform, false);
            _toastPrefab.SetActive(false);

            RectTransform rt = _toastPrefab.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f); // Center of screen
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(500, 60);

            Image bg = _toastPrefab.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            
            // Add slight rounded corners if possible by using a default rounded sprite if available
            // but for script-only, we stick to default rectangle

            CanvasGroup cg = _toastPrefab.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false; // "Toast không du?c ch?n click UI bên du?i"

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(_toastPrefab.transform, false);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.95f, 0.8f); // Vàng nh?t (white/light yellow)
            tmp.fontSize = 24;
            tmp.enableWordWrapping = false;
            
            RectTransform trt = tmp.rectTransform;
            trt.anchorMin = Vector2.zero; 
            trt.anchorMax = Vector2.one; 
            trt.sizeDelta = Vector2.zero;
        }

        public void ShowToast(string message)
        {
            if (_activeToasts.Count >= MAX_TOASTS)
            {
                // Xóa ho?c fade nhanh toast cu nh?t
                ToastItem oldest = _activeToasts[0];
                oldest.ForceRemove();
                _activeToasts.RemoveAt(0);
            }

            GameObject toastObj = Instantiate(_toastPrefab, this.transform);
            toastObj.SetActive(true);
            
            ToastItem item = toastObj.AddComponent<ToastItem>();
            item.Setup(message, _activeToasts.Count, this);
            _activeToasts.Add(item);

            UpdateTargetPositions();
        }
        
        // Backward compatibility
        public static void Show(string message, float duration = 2.0f)
        {
            Instance.ShowToast(message);
        }

        public void OnToastFinished(ToastItem item)
        {
            if (_activeToasts.Contains(item))
            {
                _activeToasts.Remove(item);
                UpdateTargetPositions();
            }
        }

        private void UpdateTargetPositions()
        {
            for (int i = 0; i < _activeToasts.Count; i++)
            {
                // Toasts spawn at TOP of the list (newest is last in list?)
                // Actually, newest is at the END of the list.
                // We want newest to spawn at Y = 80, and older ones to move DOWN.
                // Newest is index = count - 1.
                // Let's reverse: newest (i = count - 1) has target Y = TOAST_SPAWN_Y.
                // older ones (i < count - 1) move down.
                int age = (_activeToasts.Count - 1) - i; 
                float targetY = TOAST_SPAWN_Y - (age * TOAST_SPACING);
                _activeToasts[i].SetTargetY(targetY);
            }
        }
    }

    public class ToastItem : MonoBehaviour
    {
        private RectTransform _rt;
        private CanvasGroup _cg;
        private TextMeshProUGUI _txt;
        private ToastNotificationManager _manager;

        private float _targetY;
        private float _currentY;

        private float _lifeTime = 0f;
        private const float FADE_IN_TIME = 0.15f;
        private const float HOLD_TIME = 0.25f;
        private const float FADE_OUT_TIME = 1.4f;
        private const float TOTAL_TIME = FADE_IN_TIME + HOLD_TIME + FADE_OUT_TIME;

        private bool _isForceRemoving = false;

        public void Setup(string msg, int initialAgeIndex, ToastNotificationManager manager)
        {
            _rt = GetComponent<RectTransform>();
            _cg = GetComponent<CanvasGroup>();
            _txt = GetComponentInChildren<TextMeshProUGUI>();
            _manager = manager;

            _txt.text = msg;
            
            // Spawn position
            _currentY = ToastNotificationManager.TOAST_SPAWN_Y; // Wait, TOAST_SPAWN_Y is private in manager. Let's hardcode 80f or public it.
            // I'll just set it locally
            _currentY = 80f;
            _rt.anchoredPosition = new Vector2(0, _currentY);
        }

        public void SetTargetY(float y)
        {
            _targetY = y;
        }

        public void ForceRemove()
        {
            _isForceRemoving = true;
        }

        private void Update()
        {
            // Smoothly move towards target Y
            _currentY = Mathf.Lerp(_currentY, _targetY, Time.deltaTime * 10f);
            _rt.anchoredPosition = new Vector2(0, _currentY);

            if (_isForceRemoving)
            {
                _cg.alpha -= Time.deltaTime * 5f; // Fast fade
                if (_cg.alpha <= 0)
                {
                    _manager.OnToastFinished(this);
                    Destroy(gameObject);
                }
                return;
            }

            _lifeTime += Time.deltaTime;

            if (_lifeTime <= FADE_IN_TIME)
            {
                // Fade In
                _cg.alpha = _lifeTime / FADE_IN_TIME;
            }
            else if (_lifeTime <= FADE_IN_TIME + HOLD_TIME)
            {
                // Hold
                _cg.alpha = 1f;
            }
            else if (_lifeTime <= TOTAL_TIME)
            {
                // Fade Out
                float outTime = _lifeTime - (FADE_IN_TIME + HOLD_TIME);
                _cg.alpha = 1f - (outTime / FADE_OUT_TIME);
            }
            else
            {
                // Dead
                _manager.OnToastFinished(this);
                Destroy(gameObject);
            }
        }
    }
}

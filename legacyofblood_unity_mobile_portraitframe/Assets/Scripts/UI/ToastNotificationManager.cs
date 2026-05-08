using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

        private void Initialize()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 9999; 
            gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            // Generate a simple toast prefab
            _toastPrefab = new GameObject("ToastPrefab");
            _toastPrefab.transform.SetParent(this.transform, false);
            _toastPrefab.SetActive(false);

            RectTransform rt = _toastPrefab.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(600, 100);
            rt.anchoredPosition = new Vector2(0, -150); // Mặc định ở dưới sâu

            Image bg = _toastPrefab.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(_toastPrefab.transform, false);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.fontSize = 28;
            RectTransform trt = tmp.rectTransform;
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.sizeDelta = Vector2.zero;
        }

        public static void Show(string message, float duration = 2.0f)
        {
            Instance.StartCoroutine(Instance.ShowToastRoutine(message, duration));
        }

        private IEnumerator ShowToastRoutine(string message, float duration)
        {
            GameObject toast = Instantiate(_toastPrefab, this.transform);
            toast.SetActive(true);
            TextMeshProUGUI txt = toast.GetComponentInChildren<TextMeshProUGUI>();
            txt.text = message;

            RectTransform rt = toast.GetComponent<RectTransform>();
            Image bg = toast.GetComponent<Image>();

            // Slide up
            float t = 0;
            while(t < 0.3f)
            {
                t += Time.deltaTime;
                rt.anchoredPosition = Vector2.Lerp(new Vector2(0, -150), new Vector2(0, 150), t / 0.3f);
                yield return null;
            }

            yield return new WaitForSeconds(duration);

            // Fade out
            t = 0;
            while(t < 0.3f)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(1, 0, t / 0.3f);
                bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, alpha * 0.85f);
                txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, alpha);
                yield return null;
            }

            Destroy(toast);
        }
    }
}

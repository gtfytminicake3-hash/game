namespace LegendOfBlood
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using TMPro;

    /// <summary>
    /// Quản lý việc hiển thị các thông báo ngắn (toast) cho người chơi một cách tuần tự.
    /// Tự động tìm Canvas trong scene hiện tại.
    /// </summary>
    public class UINotificationManager : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Prefab của đối tượng UI thông báo.")]
        [SerializeField] private GameObject notificationPrefab;

        [Header("Settings")]
        [Tooltip("Thời gian (giây) mỗi thông báo hiển thị trước khi tự hủy.")]
        [SerializeField] private float displayDuration = 2.5f;

        private Queue<string> _notificationQueue = new Queue<string>();
        private bool _isDisplaying = false;

        #region Public API

        public void ShowNotification(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            
            _notificationQueue.Enqueue(message);

            if (!_isDisplaying)
            {
                // Dùng gameObject.activeInHierarchy để đảm bảo coroutine chỉ chạy khi Manager đang hoạt động
                if(gameObject.activeInHierarchy)
                    StartCoroutine(DisplayNotificationCoroutine());
            }
        }

        #endregion

        #region Internal Logic

        private IEnumerator DisplayNotificationCoroutine()
        {
            _isDisplaying = true;

            // Tìm Canvas chính trong Scene hiện tại
            Canvas mainCanvas = FindFirstObjectByType<Canvas>();
            if (mainCanvas == null)
            {
                Debug.LogError("UINotificationManager không thể tìm thấy Canvas nào trong Scene để hiển thị thông báo!");
                _isDisplaying = false;
                yield break; // Dừng coroutine nếu không có Canvas
            }
            
            while (_notificationQueue.Count > 0)
            {
                string message = _notificationQueue.Dequeue();

                GameObject notificationInstance = Instantiate(notificationPrefab, mainCanvas.transform);

                TextMeshProUGUI textComponent = notificationInstance.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = message;
                }
                else
                {
                    Debug.LogError("Notification Prefab không chứa component TextMeshProUGUI!", this);
                    Destroy(notificationInstance);
                    break;
                }

                yield return new WaitForSeconds(displayDuration);
                
                // Kiểm tra xem đối tượng có còn tồn tại không trước khi hủy
                if(notificationInstance != null)
                    Destroy(notificationInstance);

                yield return new WaitForSeconds(0.2f);
            }

            _isDisplaying = false;
        }

        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            // Chỉ cần kiểm tra prefab
            if (notificationPrefab == null)
            {
                Debug.LogError("Notification Prefab chưa được gán trong UINotificationManager!", this);
                this.enabled = false;
            }
        }
        #endregion
    }
}
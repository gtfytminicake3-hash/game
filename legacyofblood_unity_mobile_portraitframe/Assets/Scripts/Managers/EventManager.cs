namespace LegendOfBlood
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// EventManager là một hệ thống Event Bus tĩnh, cho phép các phần khác nhau của game
    /// giao tiếp với nhau một cách lỏng lẻo (decoupled) mà không cần biết về nhau.
    /// 
    /// Cách hoạt động:
    /// 1. Một hệ thống (Publisher) gọi EventManager.TriggerEvent("eventName", ...).
    /// 2. EventManager tìm tất cả các hệ thống khác (Listeners) đã đăng ký lắng nghe "eventName".
    /// 3. EventManager gọi các phương thức callback của tất cả các Listeners.
    /// </summary>
    public static class EventManager
    {
        // Dictionary để lưu trữ các sự kiện.
        // Key: Tên sự kiện (string).
        // Value: Delegate (Action) đại diện cho danh sách các hàm sẽ được gọi khi sự kiện được kích hoạt.
        private static Dictionary<string, Delegate> _eventDictionary = new Dictionary<string, Delegate>();

        #region Add Listener Methods (Đăng ký lắng nghe)

        /// <summary>
        /// Đăng ký lắng nghe một sự kiện không có tham số.
        /// </summary>
        /// <param name="eventName">Tên sự kiện</param>
        /// <param name="listener">Hàm sẽ được gọi (Action)</param>
        public static void StartListening(string eventName, Action listener)
        {
            AddListener(eventName, listener);
        }

        /// <summary>
        /// Đăng ký lắng nghe một sự kiện có 1 tham số.
        /// </summary>
        public static void StartListening<T>(string eventName, Action<T> listener)
        {
            AddListener(eventName, listener);
        }

        /// <summary>
        /// Đăng ký lắng nghe một sự kiện có 2 tham số.
        /// </summary>
        public static void StartListening<T0, T1>(string eventName, Action<T0, T1> listener)
        {
            AddListener(eventName, listener);
        }
        
        // Có thể thêm các phiên bản cho 3, 4 tham số nếu cần

        #endregion

        #region Remove Listener Methods (Hủy đăng ký)

        /// <summary>
        /// Hủy đăng ký lắng nghe một sự kiện không có tham số.
        /// </summary>
        public static void StopListening(string eventName, Action listener)
        {
            RemoveListener(eventName, listener);
        }

        /// <summary>
        /// Hủy đăng ký lắng nghe một sự kiện có 1 tham số.
        /// </summary>
        public static void StopListening<T>(string eventName, Action<T> listener)
        {
            RemoveListener(eventName, listener);
        }
        
        /// <summary>
        /// Hủy đăng ký lắng nghe một sự kiện có 2 tham số.
        /// </summary>
        public static void StopListening<T0, T1>(string eventName, Action<T0, T1> listener)
        {
            RemoveListener(eventName, listener);
        }
        
        #endregion

        #region Trigger Event Methods (Kích hoạt sự kiện)

        /// <summary>
        /// Kích hoạt một sự kiện không có tham số.
        /// </summary>
        public static void TriggerEvent(string eventName)
        {
            if (_eventDictionary.TryGetValue(eventName, out Delegate d))
            {
                if (d is Action action)
                {
                    action.Invoke();
                }
                else
                {
                    throw new Exception($"Lỗi khi kích hoạt sự kiện '{eventName}': Delegate không phải là Action.");
                }
            }
        }

        /// <summary>
        /// Kích hoạt một sự kiện có 1 tham số.
        /// </summary>
        public static void TriggerEvent<T>(string eventName, T param1)
        {
            if (_eventDictionary.TryGetValue(eventName, out Delegate d))
            {
                if (d is Action<T> action)
                {
                    action.Invoke(param1);
                }
                else
                {
                    throw new Exception($"Lỗi khi kích hoạt sự kiện '{eventName}': Delegate không khớp với Action<{typeof(T).Name}>.");
                }
            }
        }
        
        /// <summary>
        /// Kích hoạt một sự kiện có 2 tham số.
        /// </summary>
        public static void TriggerEvent<T0, T1>(string eventName, T0 param1, T1 param2)
        {
            if (_eventDictionary.TryGetValue(eventName, out Delegate d))
            {
                if (d is Action<T0, T1> action)
                {
                    action.Invoke(param1, param2);
                }
                else
                {
                    throw new Exception($"Lỗi khi kích hoạt sự kiện '{eventName}': Delegate không khớp với Action<{typeof(T0).Name}, {typeof(T1).Name}>.");
                }
            }
        }

        #endregion
        
        #region Internal Logic (Logic nội bộ)

        private static void AddListener(string eventName, Delegate listener)
        {
            if (_eventDictionary.TryGetValue(eventName, out Delegate d))
            {
                // Thêm listener vào delegate đã tồn tại
                _eventDictionary[eventName] = Delegate.Combine(d, listener);
            }
            else
            {
                // Tạo mới entry trong dictionary
                _eventDictionary[eventName] = listener;
            }
        }
        
        private static void RemoveListener(string eventName, Delegate listener)
        {
            if (_eventDictionary.TryGetValue(eventName, out Delegate d))
            {
                Delegate result = Delegate.Remove(d, listener);
                if (result == null)
                {
                    // Nếu không còn listener nào, xóa luôn entry khỏi dictionary
                    _eventDictionary.Remove(eventName);
                }
                else
                {
                    _eventDictionary[eventName] = result;
                }
            }
        }
        
        #endregion
    }

    /// <summary>
    /// Lớp tĩnh chứa định nghĩa tên của tất cả các sự kiện trong game.
    /// Việc này giúp tránh lỗi chính tả và quản lý các sự kiện một cách tập trung.
    /// </summary>
    public static class GameEvents
    {
        // Hệ thống
        public const string OnPlayerDataLoaded = "OnPlayerDataLoaded";
        public const string OnGameStateChanged = "OnGameStateChanged";

        // Hero
        public const string OnHeroListChanged = "OnHeroListChanged";
        public const string OnHeroLeveledUp = "OnHeroLeveledUp";
        public const string OnHeroMatured = "OnHeroMatured";
        public const string OnHeroHealed = "OnHeroHealed";

        // Inventory
        public const string OnResourceChanged = "OnResourceChanged"; // Params: ResourceType, int newAmount
        public const string OnItemChanged = "OnItemChanged";         // Params: string itemID, int newCount

        // Expedition
        public const string OnExpeditionStarted = "OnExpeditionStarted";   // Params: Expedition
        public const string OnExpeditionReturning = "OnExpeditionReturning"; // Params: Expedition
        public const string OnExpeditionFinished = "OnExpeditionFinished";  // Params: Expedition

        // UI
        public const string OnHeroCardClicked = "OnHeroCardClicked"; // Params: HeroData
        public const string OnProfessionSelectionRequested = "OnProfessionSelectionRequested"; // Params: HeroData
    }
}
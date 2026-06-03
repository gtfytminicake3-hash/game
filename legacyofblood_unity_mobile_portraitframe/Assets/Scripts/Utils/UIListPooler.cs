using System;
using System.Collections.Generic;
using UnityEngine;

namespace LegendOfBlood.Utils
{
    /// <summary>
    /// Generic utility for UI Object Pooling.
    /// Handles reusing UI elements in a list to minimize Instantiate/Destroy calls.
    /// </summary>
    /// <typeparam name="TData">The type of data to display.</typeparam>
    /// <typeparam name="TComponent">The component script on the UI prefab.</typeparam>
    public class UIListPooler<TData, TComponent> where TComponent : MonoBehaviour
    {
        private readonly GameObject _prefab;
        private readonly Transform _container;
        private readonly List<TComponent> _pooledComponents = new List<TComponent>();
        private readonly Action<TComponent, TData> _onInitialize;

        public List<TComponent> ActiveComponents => _pooledComponents;

        public UIListPooler(GameObject prefab, Transform container, Action<TComponent, TData> onInitialize)
        {
            _prefab = prefab;
            _container = container;
            _onInitialize = onInitialize;
        }

        /// <summary>
        /// Refreshes the UI list using provided data.
        /// Reuses existing objects and toggles visibility as needed.
        /// </summary>
        public void Refresh(IList<TData> dataList)
        {
            if (_prefab == null || _container == null)
            {
                Debug.LogError("[UIListPooler] Prefab or Container is null!");
                return;
            }

            // Ensure we have enough instantiated objects
            while (_pooledComponents.Count < dataList.Count)
            {
                GameObject obj = UnityEngine.Object.Instantiate(_prefab, _container);
                TComponent comp = obj.GetComponent<TComponent>();
                if (comp == null)
                {
                    Debug.LogError($"[UIListPooler] Prefab does not contain component {typeof(TComponent).Name}");
                    UnityEngine.Object.Destroy(obj);
                    return;
                }
                _pooledComponents.Add(comp);
            }

            // Update all objects
            for (int i = 0; i < _pooledComponents.Count; i++)
            {
                TComponent comp = _pooledComponents[i];
                if (i < dataList.Count)
                {
                    comp.gameObject.SetActive(true);
                    _onInitialize?.Invoke(comp, dataList[i]);
                }
                else
                {
                    comp.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Hides all items in the list.
        /// </summary>
        public void Clear()
        {
            foreach (var comp in _pooledComponents)
            {
                if (comp != null) comp.gameObject.SetActive(false);
            }
        }
    }
}

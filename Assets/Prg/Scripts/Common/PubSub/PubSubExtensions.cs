using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Prg.Scripts.Common.PubSub
{
    public static class PubSubExtensions
    {
        private static readonly Hub Hub = new Hub();
        private static bool _isApplicationQuitting;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            SetEditorStatus();
        }

        [Conditional("UNITY_EDITOR")]
        private static void SetEditorStatus()
        {
            void CheckHandlerCount()
            {
                if (_isApplicationQuitting)
                {
                    return;
                }
                var handlerCount = Hub.handlers.Count;
                if (handlerCount <= 0)
                {
                    return;
                }
                foreach (var h in Hub.handlers)
                {
                    Debug.Log($"handler {h}");
                }
                Debug.LogWarning($"sceneUnloaded PubSubExtensions.HandlerCount is {handlerCount}");
            }

            _isApplicationQuitting = false;
            Application.quitting += () => _isApplicationQuitting = true;
            SceneManager.sceneUnloaded += _ => CheckHandlerCount();
        }

        public static bool Exists<T>(this object obj)
        {
            return Hub.Exists<T>(obj);
        }

        public static void Publish<T>(this object obj)
        {
            Hub.Publish(obj, default(T));
        }

        public static void Publish<T>(this object obj, T data)
        {
            Hub.Publish(obj, data);
        }

        public static void Subscribe<T>(this object obj, Action<T> handler)
        {
            Hub.Subscribe(obj, handler);
        }

        public static void Unsubscribe(this object obj)
        {
            Hub.Unsubscribe(obj);
        }

        public static void Unsubscribe<T>(this object obj)
        {
            Hub.Unsubscribe(obj, (Action<T>)null);
        }

        public static void Unsubscribe<T>(this object obj, Action<T> handler)
        {
            Hub.Unsubscribe(obj, handler);
        }
    }
}
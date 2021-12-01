using System;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.ScriptableObjects;
using Prg.Scripts.Common.Unity;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Altzone.Scripts.Window
{
    public class WindowManager : MonoBehaviour, IWindowManager
    {
        [Serializable]
        private class MyWindow
        {
            public WindowDef _windowDef;
            public GameObject _window;

            public MyWindow(WindowDef windowDef, GameObject window)
            {
                _windowDef = windowDef;
                _window = window;
            }
        }

        public static IWindowManager Get() => FindObjectOfType<WindowManager>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            var windowManager = Get();
            if (windowManager == null)
            {
                UnityExtensions.CreateGameObjectAndComponent<WindowManager>(nameof(WindowManager), true);
            }
        }

        [SerializeField] private List<MyWindow> _currentWindows;
        [SerializeField] private List<MyWindow> _knownWindows;

        private GameObject _windowsParent;
        private WindowDef _pendingWindow;

        private void Awake()
        {
            Debug.Log("Awake");
            _currentWindows = new List<MyWindow>();
            _knownWindows = new List<MyWindow>();
            SceneManager.sceneLoaded += SceneLoaded;
            SceneManager.sceneUnloaded += SceneUnloaded;
            var handler = gameObject.AddComponent<EscapeKeyHandler>();
            handler.SetCallback(EscapeKeyPressed);
        }

#if UNITY_EDITOR
        private void OnApplicationQuit()
        {
            _currentWindows.Clear();
            _knownWindows.Clear();
        }
#endif
        private void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"sceneLoaded {scene.name} ({scene.buildIndex}) pending {_pendingWindow}");
            if (_pendingWindow != null)
            {
                ((IWindowManager)this).ShowWindow(_pendingWindow);
                _pendingWindow = null;
            }
        }

        private void SceneUnloaded(Scene scene)
        {
            Debug.Log($"sceneUnloaded {scene.name} ({scene.buildIndex}) prefabCount {_knownWindows.Count} pending {_pendingWindow}");
            _knownWindows.Clear();
            _windowsParent = null;
        }

        void IWindowManager.SetWindowsParent(GameObject windowsParent)
        {
            _windowsParent = windowsParent;
        }

        private void EscapeKeyPressed()
        {
            Debug.Log($"EscapeKeyPressed {_currentWindows.Count}");
            ((IWindowManager)this).GoBack();
        }

        void IWindowManager.GoBack()
        {
            Debug.Log($"GoBack {_currentWindows.Count}");
            if (_currentWindows.Count == 1)
            {
                ExitApplication.ExitGracefully();
                return;
            }
            var firstWindow = _currentWindows[0];
            _currentWindows.RemoveAt(0);
            Hide(firstWindow);
            if (_currentWindows.Count == 0)
            {
                return;
            }
            var currentWindow = _currentWindows[0];
            Show(currentWindow);
        }

        void IWindowManager.ShowWindow(WindowDef windowDef)
        {
            Debug.Log($"LoadWindow {windowDef} count {_currentWindows.Count}");
            if (windowDef.NeedsSceneLoad)
            {
                _pendingWindow = windowDef;
                SceneLoader.LoadScene(windowDef);
                return;
            }
            if (_pendingWindow != null && !_pendingWindow.Equals(windowDef))
            {
                Debug.Log($"LoadWindow IGNORE {windowDef} PENDING {_pendingWindow}");
                return;
            }
            if (IsVisible(windowDef))
            {
                Debug.Log($"LoadWindow IGNORE {windowDef} IsVisible");
                return;
            }
            var currentWindow = _knownWindows.FirstOrDefault(x => windowDef.Equals(x._windowDef));
            if (currentWindow == null)
            {
                currentWindow = CreateWindow(windowDef);
            }
            if (_currentWindows.Count > 0)
            {
                var previousWindow = _currentWindows[0];
                Assert.IsFalse(currentWindow._windowDef.Equals(previousWindow._windowDef));
                Hide(previousWindow);
            }
            _currentWindows.Insert(0, currentWindow);
            Show(currentWindow);
        }

        private MyWindow CreateWindow(WindowDef windowDef)
        {
            var windowName = windowDef.name;
            Debug.Log($"CreateWindow [{windowName}] {windowDef} count {_currentWindows.Count}");
            var prefab = windowDef.WindowPrefab;
            var isSceneObject = prefab.scene.handle != 0;
            if (!isSceneObject)
            {
                prefab = Instantiate(prefab);
                if (_windowsParent != null)
                {
                    prefab.transform.SetParent(_windowsParent.transform);
                }
                prefab.name = prefab.name.Replace("(Clone)", "");
            }
            var currentWindow = new MyWindow(windowDef, prefab);
            _knownWindows.Add(currentWindow);
            return currentWindow;
        }

        private static void Show(MyWindow window)
        {
            Debug.Log($"Show {window._windowDef}");
            window._window.SetActive(true);
        }

        private static void Hide(MyWindow window)
        {
            Debug.Log($"Hide {window._windowDef}");
            window._window.SetActive(false);
        }

        private bool IsVisible(WindowDef windowDef)
        {
            if (_currentWindows.Count == 0)
            {
                return false;
            }
            var firstWindow = _currentWindows[0];
            Debug.Log($"IsVisible new {windowDef} first {firstWindow} {windowDef.Equals(firstWindow._windowDef)}");
            return windowDef.Equals(firstWindow._windowDef);
        }
    }
}
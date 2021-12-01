using System;
using System.Collections.Generic;
using Altzone.Scripts.ScriptableObjects;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Altzone.Scripts.Window
{
    public class WindowManager : MonoBehaviour
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

        public static WindowManager Get() => FindObjectOfType<WindowManager>();

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

        private GameObject _windowsParent;
        private WindowDef _pendingWindow;
        private readonly Dictionary<string, GameObject> _knownWindows = new Dictionary<string, GameObject>();

        private void Awake()
        {
            Debug.Log("Awake");
            _currentWindows = new List<MyWindow>();
            SceneManager.sceneLoaded += SceneLoaded;
            SceneManager.sceneUnloaded += SceneUnloaded;
        }

#if UNITY_EDITOR
        private void OnApplicationQuit()
        {
            _currentWindows.Clear();
        }
#endif
        private void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"sceneLoaded {scene.name} ({scene.buildIndex}) pending {_pendingWindow}");
            if (_pendingWindow != null)
            {
                LoadWindow(_pendingWindow);
                _pendingWindow = null;
            }
        }

        private void SceneUnloaded(Scene scene)
        {
            Debug.Log($"sceneUnloaded {scene.name} ({scene.buildIndex}) prefabCount {_knownWindows.Count} pending {_pendingWindow}");
            _knownWindows.Clear();
            _windowsParent = null;
        }

        public void SetWindowsParent(GameObject windowsParent)
        {
            _windowsParent = windowsParent;
        }

        public void LoadWindow(WindowDef windowDef)
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
            _LoadWindow(windowDef);
        }

        private void _LoadWindow(WindowDef windowDef)
        {
            var windowName = windowDef.name;
            Debug.Log($"_LoadWindow start [{windowName}] {windowDef} count {_currentWindows.Count}");
            if (!_knownWindows.TryGetValue(windowName, out var prefab))
            {
                prefab = windowDef.WindowPrefab;
                var isSceneObject = prefab.scene.handle != 0;
                if (!isSceneObject)
                {
                    prefab = Instantiate(prefab);
                    if (_windowsParent != null)
                    {
                        prefab.transform.SetParent(_windowsParent.transform);
                    }
                    prefab.name = prefab.name.Replace("(Clone)", "");
                    if (!_knownWindows.ContainsKey(prefab.name))
                    {
                        _knownWindows.Add(prefab.name, prefab);
                    }
                }
            }
            // Protocol to "show window"
            _currentWindows.Insert(0, new MyWindow(windowDef, prefab));
            if (_currentWindows.Count > 1)
            {
                var previous = _currentWindows[1];
                Assert.IsFalse(windowDef.Equals(previous._windowDef));
                Hide(_currentWindows[1]);
            }
            Show(_currentWindows[0]);
        }

        private void Show(MyWindow window)
        {
            Debug.Log($"Show {window._windowDef}");
            window._window.SetActive(true);
        }

        private void Hide(MyWindow window)
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
            Debug.Log($"IsVisible new {windowDef} first {firstWindow} {firstWindow.Equals(windowDef)}");
            return firstWindow.Equals(windowDef);
        }
    }
}
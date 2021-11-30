using System.Collections.Generic;
using Altzone.Scripts.ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Altzone.Scripts.Window
{
    public class WindowManager : MonoBehaviour
    {
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

        [SerializeField] private List<WindowDef> _currentWindows;

        private WindowDef _windowDef;
        private readonly Dictionary<string, GameObject> _knownWindows = new Dictionary<string, GameObject>();

        private void Awake()
        {
            _currentWindows = new List<WindowDef>();
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
            Debug.Log($"sceneLoaded {scene.name} ({scene.buildIndex}) {_windowDef}");
            if (_windowDef != null)
            {
                _LoadWindow(_windowDef);
                _windowDef = null;
            }
        }

        private void SceneUnloaded(Scene scene)
        {
            var prefabCount = _knownWindows.Count;
            _knownWindows.Clear();
            Debug.Log($"sceneUnloaded {scene.name} ({scene.buildIndex}) prefabCount {prefabCount} {_windowDef}");
        }

        public void LoadWindow(WindowDef windowDef)
        {
            if (windowDef.HasScene)
            {
                Debug.Log($"LoadWindow {windowDef}");
                _windowDef = windowDef;
                SceneLoader.LoadScene(windowDef);
                return;
            }
            _LoadWindow(windowDef);
        }

        private void _LoadWindow(WindowDef windowDef)
        {
            var windowName = windowDef.name;
            Debug.Log($"LoadWindow [{windowName}] {windowDef}");
            if (!_knownWindows.TryGetValue(windowName, out var prefab))
            {
                prefab = windowDef.WindowPrefab;
                var isSceneObject = prefab.scene.handle != 0;
                if (!isSceneObject)
                {
                    prefab = Instantiate(prefab);
                    prefab.name = prefab.name.Replace("(Clone)", "");
                    if (!_knownWindows.ContainsKey(prefab.name))
                    {
                        _knownWindows.Add(prefab.name, prefab);
                    }
                }
            }
            // Protocol to "show window"
            _currentWindows.Insert(0, windowDef);
            prefab.SetActive(true);
        }
    }
}
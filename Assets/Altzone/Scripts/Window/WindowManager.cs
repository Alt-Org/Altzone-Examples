using System.Collections.Generic;
using Altzone.Scripts.ScriptableObjects;
using UnityEngine;
using UnityEngine.Assertions;

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

        private readonly List<WindowDef> _windows = new List<WindowDef>();

        public void LoadWindow(WindowDef windowDef)
        {
            _windows.Add(windowDef);
            Assert.IsTrue(_windows.Count > 0);
            if (string.IsNullOrEmpty(windowDef.SceneName))
            {
                Debug.Log($"LoadWindow '{windowDef.WindowName}'");
            }
            else
            {
                Debug.Log($"LoadWindow '{windowDef.WindowName}' ({windowDef.SceneName})");
                SceneLoader.LoadScene(windowDef);
            }
        }
    }
}
using Altzone.Scripts.Window.ScriptableObjects;
using UnityEngine;

namespace Altzone.Scripts.Window
{
    /// <summary>
    /// Default window loader for any level that uses <c>WindowManager</c>.
    /// </summary>
    public class DefaultWindow : MonoBehaviour
    {
        [SerializeField] private WindowDef _window;
        [SerializeField] private GameObject _sceneWindow;

        private void OnEnable()
        {
            Debug.Log($"OnEnable {_window}");
            var windowManager = WindowManager.Get();
            windowManager.SetWindowsParent(gameObject);
            if (_window.WindowPrefab == null)
            {
                _window.SetWindowPrefab(_sceneWindow);
            }
            windowManager.ShowWindow(_window);
        }
    }
}
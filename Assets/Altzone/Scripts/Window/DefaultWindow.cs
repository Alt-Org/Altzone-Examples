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

        private void OnEnable()
        {
            Debug.Log($"OnEnable {_window}");
            var windowManager = WindowManager.Get();
            windowManager.SetWindowsParent(gameObject);
            windowManager.ShowWindow(_window);
        }
    }
}
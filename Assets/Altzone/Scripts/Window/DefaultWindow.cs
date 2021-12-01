using Altzone.Scripts.ScriptableObjects;
using UnityEngine;

namespace Altzone.Scripts.Window
{
    public class DefaultWindow : MonoBehaviour
    {
        [SerializeField] private WindowDef _window;

        private void OnEnable()
        {
            Debug.Log($"OnEnable {_window}");
            var windowManager = WindowManager.Get();
            windowManager.SetWindowsParent(gameObject);
            windowManager.LoadWindow(_window);
        }
    }
}
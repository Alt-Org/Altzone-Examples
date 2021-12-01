using Altzone.Scripts.ScriptableObjects;
using UnityEngine;

namespace Altzone.Scripts.Window
{
    public class DefaultWindow : MonoBehaviour
    {
        [SerializeField] private WindowDef _window;

        /*private IEnumerator Start()
        {
            WindowManager windowManager;
            yield return new WaitUntil(() =>
            {
                windowManager = WindowManager.Get();
                return windowManager != null;
            });
            Debug.Log($"Start {_window}");
            WindowManager.Get().LoadWindow(_window);
        }*/

        private void OnEnable()
        {
            Debug.Log($"OnEnable {_window}");
            WindowManager.Get().LoadWindow(_window);
        }
    }
}
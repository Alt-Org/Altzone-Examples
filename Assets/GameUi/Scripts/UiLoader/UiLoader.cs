using System.Collections;
using Altzone.Scripts.Window;
using Altzone.Scripts.Window.ScriptableObjects;
using UnityEngine;

namespace GameUi.Scripts.UiLoader
{
    public class UiLoader : MonoBehaviour
    {
        [SerializeField] private WindowDef _window;
        [SerializeField] private float _demoLoadDelay;
        private IEnumerator Start()
        {
            var wait = new WaitForSeconds(_demoLoadDelay);
            yield return wait;
            WindowManager.Get().ShowWindow(_window);
        }
    }
}

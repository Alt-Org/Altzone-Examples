using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Prg.Scripts.Common.Unity.Window
{
    /// <summary>
    /// Tracks Escape key press on behalf of <c>WindowManager</c>
    /// using <c>UnityEngine.InputSystem</c> with simple callback to listen all keypresses.
    /// </summary>
    public class EscapeKeyHandler : MonoBehaviour
    {
        private const char Escape = '\u001B';

        private Action _callback;

        private void OnEnable()
        {
            Keyboard.current.onTextInput -= OnTextInput;
            Keyboard.current.onTextInput += OnTextInput;
        }

        private void OnDisable()
        {
            Keyboard.current.onTextInput -= OnTextInput;
        }

        private void OnDestroy()
        {
            Keyboard.current.onTextInput -= OnTextInput;
        }

        private void OnTextInput(char c)
        {
            if (c == Escape)
            {
                _callback?.Invoke();
            }
        }

        public void SetCallback(Action callback)
        {
            _callback = callback;
        }
    }
}
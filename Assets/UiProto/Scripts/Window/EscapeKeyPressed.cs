using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UiProto.Scripts.Window
{
    /// <summary>
    /// Tracks Escape key press so that is must pressed down and released exactly once (on the same level)!
    /// </summary>
    public class EscapeKeyPressed : MonoBehaviour
    {
        private static EscapeKeyPressed _instance;

        private bool _isEscapePressedDown;
        private bool _isEscapePressedUp;
        private string _activeScenePathDown;
        private string _activeScenePathUp;

        private Action _clickListener;

        protected void Awake()
        {
            if (_instance == null)
            {
                // Register us as the singleton!
                _instance = this;
                return;
            }
            throw new UnityException($"Component added more than once: {nameof(EscapeKeyPressed)}");
        }

        protected void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _activeScenePathDown = SceneManager.GetActiveScene().path;
                _isEscapePressedDown = true;
                _isEscapePressedUp = false;
                return;
            }
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                _activeScenePathUp = SceneManager.GetActiveScene().path;
                _isEscapePressedUp = true;
                return;
            }
            if (_isEscapePressedDown && _isEscapePressedUp)
            {
                _isEscapePressedDown = false;
                _isEscapePressedUp = false;
                if (_activeScenePathDown != _activeScenePathUp)
                {
                    Debug.LogWarning($"ESCAPE SKIPPED down={_activeScenePathDown} up={_activeScenePathUp}");
                    return;
                }
                _clickListener?.Invoke();
            }
        }

        public void AddListener(Action clickListener)
        {
            _clickListener += clickListener;
        }
    }
}
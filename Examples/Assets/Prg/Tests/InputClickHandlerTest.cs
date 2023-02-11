using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace Prg.Tests
{
    /// <summary>
    /// See Input System Interactions Explained | Press, Hold, Tap, SlowTap, MultiTap
    /// https://www.youtube.com/watch?v=rMlcwtoui4I
    /// </summary>
    public class InputClickHandlerTest : MonoBehaviour, IInputTapHandler
    {
        [Header("Settings"), SerializeField] private float _longTapDelay = 0.5f;
        [SerializeField] private InputActionReference _clickActionRef;

        [SerializeField, Header("Debug")] private bool _isLogEvents;
        
        private InputAction _inputAction;
        private Vector2 _startPosition;
        private Vector2 _inputPosition;
        private float _startTime;
        private float _duration;
        private IInputTapReceiver _clickReceiver;

        private void Awake()
        {
            Debug.Log($"{_clickActionRef}");
            Assert.IsNotNull(_clickActionRef);
            _inputAction = _clickActionRef.action;
        }

        private void OnEnable()
        {
            Debug.Log($"{_inputAction}");
            _inputAction.started += ClickStarted;
            _inputAction.performed += ClickPerformed;
            _inputAction.canceled += ClickCancelled;
            _inputAction.Enable();
        }

        private void OnDisable()
        {
            Debug.Log($"{_inputAction}");
            _inputAction.started -= ClickStarted;
            _inputAction.performed -= ClickPerformed;
            _inputAction.canceled -= ClickCancelled;
            _inputAction.Disable();
        }

        private void ClickStarted(InputAction.CallbackContext ctx)
        {
            _startTime = Time.time;
            _duration = 0;
            _startPosition = ctx.ReadValue<Vector2>();
            if (_isLogEvents)
            {
                Debug.Log($"duration {_duration:0.000} pos {_startPosition}");
            }
        }

        private void ClickPerformed(InputAction.CallbackContext ctx)
        {
            _duration = Time.time - _startTime;
            _inputPosition = ctx.ReadValue<Vector2>();
            if (_isLogEvents)
            {
                Debug.Log($"duration {_duration:0.000} pos {_inputPosition}");
            }
        }

        private void ClickCancelled(InputAction.CallbackContext ctx)
        {
            _duration = Time.time - _startTime;
            if (_isLogEvents)
            {
                Debug.Log($"duration {_duration:0.000} pos {_inputPosition} delta {_startPosition - _inputPosition}");
            }
            if (_clickReceiver == null)
            {
                return;
            }
            if (_duration < _longTapDelay)
            {
                _clickReceiver.Tap(_inputPosition);
                return;
            }
            _clickReceiver.LongTap(_inputPosition);
        }

        void IInputTapHandler.SetTapReceiver(IInputTapReceiver clickReceiver)
        {
            _clickReceiver = clickReceiver;
        }
    }
}
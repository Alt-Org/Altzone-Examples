using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace Prg.Tests
{
    public class InputClickHandlerTest : MonoBehaviour, IInputTapHandler
    {
        [SerializeField] private InputActionReference _clickActionRef;

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
            Debug.Log($"duration {_duration:0.000} pos {_startPosition}");
        }

        private void ClickPerformed(InputAction.CallbackContext ctx)
        {
            _duration = Time.time - _startTime;
            _inputPosition = ctx.ReadValue<Vector2>();
            Debug.Log($"duration {_duration:0.000} pos {_inputPosition}");
        }

        private void ClickCancelled(InputAction.CallbackContext ctx)
        {
            _duration = Time.time - _startTime;
            Debug.Log($"duration {_duration:0.000} pos {_inputPosition} delta {_startPosition - _inputPosition}");
        }

        void IInputTapHandler.SetTapReceiver(IInputTapReceiver clickReceiver)
        {
            _clickReceiver = clickReceiver;
        }
    }
}
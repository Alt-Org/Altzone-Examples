using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

namespace Prg.Tests
{
    public interface IInputTapReceiver
    {
        void Tap(Vector2 position);
        void LongTap(Vector2 position);
    }

    public interface IInputTapHandler
    {
        void SetTapReceiver(IInputTapReceiver tapReceiver);
    }

    public class InputSlowTapHandlerTest : MonoBehaviour, IInputTapHandler
    {
        [SerializeField, Header("Settings")] private InputActionReference _tapActionRef;
        [SerializeField] private InputActionReference _positionActionRef;

        [SerializeField, Header("Debug")] private bool _isLogEvents;

        private InputAction _tapAction;
        private InputAction _positionAction;
        private Vector2 _startPosition;
        private Vector2 _inputPosition;
        private IInputTapReceiver _tapReceiver;

        private void Awake()
        {
            Debug.Log($"{_tapActionRef} | {_positionActionRef}");
            Assert.IsTrue(_tapActionRef != null && _positionActionRef != null);
            _tapAction = _tapActionRef.action;
            _positionAction = _positionActionRef.action;
            Assert.IsFalse(string.IsNullOrWhiteSpace(_tapAction.interactions));
            Debug.Log($"interactions {_tapAction.interactions}");
        }

        private void OnEnable()
        {
            Debug.Log($"{_tapAction} | {_positionAction}");
            _tapAction.started += TapStarted;
            _tapAction.performed += TapPerformed;
            _tapAction.canceled += TapCancelled;
            _tapAction.Enable();
            _positionAction.Enable();
        }

        private void OnDisable()
        {
            Debug.Log($"{_tapAction} | {_positionAction}");
            _tapAction.started -= TapStarted;
            _tapAction.performed -= TapPerformed;
            _tapAction.canceled -= TapCancelled;
            _tapAction.Disable();
            _positionAction.Disable();
        }

        private void TapStarted(InputAction.CallbackContext ctx)
        {
            _startPosition = _positionAction.ReadValue<Vector2>();
            if (_isLogEvents)
            {
                Debug.Log($"duration {ctx.duration:0.000} pos {_startPosition} {ctx.interaction?.GetType().Name}");
            }
        }

        private void TapPerformed(InputAction.CallbackContext ctx)
        {
            _inputPosition = _positionAction.ReadValue<Vector2>();
            if (_isLogEvents)
            {
                Debug.Log(
                    $"duration {ctx.duration:0.000} pos {_inputPosition} delta {_startPosition - _inputPosition} {ctx.interaction?.GetType().Name}");
            }
            if (_tapReceiver == null)
            {
                return;
            }
            var interaction = ctx.interaction;
            switch (interaction)
            {
                case SlowTapInteraction:
                    _tapReceiver.LongTap(_inputPosition);
                    break;
                case TapInteraction:
                    _tapReceiver.Tap(_inputPosition);
                    break;
            }
        }

        private void TapCancelled(InputAction.CallbackContext ctx)
        {
            if (_isLogEvents)
            {
                Debug.Log(
                    $"duration {ctx.duration:0.000} {ctx.interaction?.GetType().Name}");
            }
        }

        void IInputTapHandler.SetTapReceiver(IInputTapReceiver tapReceiver)
        {
            Debug.Log($"{tapReceiver}");
            _tapReceiver = tapReceiver;
        }
    }
}
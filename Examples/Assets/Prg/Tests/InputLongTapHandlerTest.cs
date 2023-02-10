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

    public class InputLongTapHandlerTest : MonoBehaviour, IInputTapHandler
    {
        [SerializeField, Header("Settings")] private InputActionReference _tapActionRef;

        [SerializeField, Header("Debug")] private bool _isLogEvents;

        private InputAction _inputAction;
        private Vector2 _inputPosition;
        private IInputTapReceiver _tapReceiver;

        private void Awake()
        {
            Debug.Log($"{_tapActionRef}");
            Assert.IsNotNull(_tapActionRef);
            _inputAction = _tapActionRef.action;
            Assert.IsFalse(string.IsNullOrWhiteSpace(_inputAction.interactions));
            Debug.Log($"interactions {_inputAction.interactions}");
        }

        private void OnEnable()
        {
            Debug.Log($"{_inputAction}");
            _inputAction.performed += TapPerformed;
            _inputAction.canceled += TapCancelled;
            _inputAction.Enable();
        }

        private void OnDisable()
        {
            Debug.Log($"{_inputAction}");
            _inputAction.performed -= TapPerformed;
            _inputAction.canceled -= TapCancelled;
            _inputAction.Disable();
        }

        private void TapPerformed(InputAction.CallbackContext ctx)
        {
            _inputPosition = ctx.ReadValue<Vector2>();
            if (_isLogEvents)
            {
                Debug.Log($"duration {ctx.duration:0.000} pos {_inputPosition} {ctx.interaction?.GetType().Name}");
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
            _inputPosition = ctx.ReadValue<Vector2>();
            if (_isLogEvents)
            {
                Debug.Log($"duration {ctx.duration:0.000} pos {_inputPosition} {ctx.interaction?.GetType().Name}");
            }
        }

        void IInputTapHandler.SetTapReceiver(IInputTapReceiver tapReceiver)
        {
            Debug.Log($"{tapReceiver}");
            _tapReceiver = tapReceiver;
        }
    }
}
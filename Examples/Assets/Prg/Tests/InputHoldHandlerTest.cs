using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace Prg.Tests
{
    public class InputHoldHandlerTest : MonoBehaviour
    {
        [SerializeField] private InputActionReference _holdActionRef;

        private InputAction _inputAction;
        private Vector2 _inputPosition;

        private void Awake()
        {
            Debug.Log($"{_holdActionRef}");
            Assert.IsNotNull(_holdActionRef);
            _inputAction = _holdActionRef.action;
            Assert.IsFalse(string.IsNullOrWhiteSpace(_inputAction.interactions));
            Debug.Log($"interactions {_inputAction.interactions}");
        }

        private void OnEnable()
        {
            Debug.Log($"{_inputAction}");
            _inputAction.Enable();
        }

        private void Start()
        {
            Debug.Log($"{_inputAction}");
            _inputAction.performed += HoldPerformed;
            _inputAction.canceled += HoldCancelled;
        }

        private void OnDisable()
        {
            Debug.Log($"{_inputAction}");
            _inputAction.Disable();
        }

        private void HoldPerformed(InputAction.CallbackContext ctx)
        {
            _inputPosition = ctx.ReadValue<Vector2>();
            Debug.Log($"duration {ctx.duration:0.000} pos {_inputPosition} {ctx.interaction?.GetType().Name}");
        }

        private void HoldCancelled(InputAction.CallbackContext ctx)
        {
            _inputPosition = ctx.ReadValue<Vector2>();
            Debug.Log($"duration {ctx.duration:0.000} pos {_inputPosition} {ctx.interaction?.GetType().Name}");
        }
    }
}
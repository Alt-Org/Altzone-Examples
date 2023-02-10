using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace Prg.Tests
{
    public class InputLongTapHandlerTest : MonoBehaviour
    {
        [SerializeField] private InputActionReference _tapActionRef;

        private InputAction _inputAction;
        private Vector2 _inputPosition;

        private void Awake()
        {
            Debug.Log($"{_tapActionRef}");
            Assert.IsNotNull(_tapActionRef);
            _inputAction = _tapActionRef.action;
        }

        private void OnEnable()
        {
            Debug.Log($"{_inputAction}");
            _inputAction.Enable();
        }

        private void Start()
        {
            Debug.Log($"{_inputAction}");
            _inputAction.performed += TapPerformed;
            _inputAction.canceled += TapCancelled;
        }

        private void OnDisable()
        {
            Debug.Log($"{_inputAction}");
            _inputAction.Disable();
        }

        private void TapPerformed(InputAction.CallbackContext ctx)
        {
            _inputPosition = ctx.ReadValue<Vector2>();
            Debug.Log($"duration {ctx.duration:0.000} pos {_inputPosition}");
        }

        private void TapCancelled(InputAction.CallbackContext ctx)
        {
            _inputPosition = ctx.ReadValue<Vector2>();
            Debug.Log($"duration {ctx.duration:0.000} pos {_inputPosition}");
        }
    }
}
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

    /// <summary>
    /// Two videos from <b>samyam</b> for inspiration:<br />
    /// See Input System Interactions Explained | Press, Hold, Tap, SlowTap, MultiTap
    /// <a href="https://www.youtube.com/watch?v=rMlcwtoui4I">link</a>
    /// <br />
    /// How to use TOUCH with the Input System in Unity (see 15:51)
    /// <a href="https://www.youtube.com/watch?v=4MOOitENQVg">link</a>
    /// </summary>
    public class InputSlowTapHandlerTest : MonoBehaviour, IInputTapHandler
    {
        [SerializeField, Header("Settings"), Range(0, 100)] private float _trackingSensitivityPercent = 1.0f;
        [SerializeField] private InputActionReference _tapActionRef;
        [SerializeField] private InputActionReference _positionActionRef;

        [SerializeField, Header("Debug")] private bool _isLogEvents;

        private InputAction _tapAction;
        private InputAction _positionAction;
        private Vector2 _startPosition;
        private Vector2 _inputPosition;
        private IInputTapReceiver _tapReceiver;
        private bool _isCheckSensitivity;
        private float _sensitivityScreenX;
        private float _sensitivityScreenY;

        private void Awake()
        {
            Debug.Log($"{_tapActionRef} | {_positionActionRef}");
            Assert.IsTrue(_tapActionRef != null && _positionActionRef != null);
            _tapAction = _tapActionRef.action;
            _positionAction = _positionActionRef.action;
            Assert.IsFalse(string.IsNullOrWhiteSpace(_tapAction.interactions));
            Debug.Log($"interactions {_tapAction.interactions}");
            if (!(_trackingSensitivityPercent > 0))
            {
                return;
            }
            var resolution = Screen.currentResolution;
            _isCheckSensitivity = true;
            _sensitivityScreenX = resolution.width / 100f * _trackingSensitivityPercent;
            _sensitivityScreenY = resolution.height / 100f * _trackingSensitivityPercent;
            Debug.Log($"tracking {_trackingSensitivityPercent}% : x,y {_sensitivityScreenX:0},{_sensitivityScreenY:0} from {resolution}");
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
            if (_isCheckSensitivity)
            {
                var delta = _startPosition - _inputPosition;
                if (Mathf.Abs(delta.x) > _sensitivityScreenX || Mathf.Abs(delta.y) > _sensitivityScreenY)
                {
                    if (_isLogEvents)
                    {
                        Debug.Log($"IGNORED delta {delta}");
                    }
                    return;
                }
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
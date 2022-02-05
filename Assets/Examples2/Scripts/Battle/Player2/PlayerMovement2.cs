using UnityEngine;
using UnityEngine.InputSystem;

namespace Examples2.Scripts.Battle.Player2
{
    internal class PlayerMovement2
    {
        private readonly Transform _transform;
        private readonly UnityEngine.InputSystem.PlayerInput _playerInput;
        private readonly Camera _camera;

        public Rect PlayerArea { get; set; } = Rect.MinMaxRect(-100, -100, 100, 100);
        public float UnReachableDistance { get; set; } = 100;
        public float Speed { get; set; } = 1;

        private bool _isMoving;
        private Vector2 _inputClick;
        private Vector3 _inputPosition;
        private Vector3 _tempPosition;

        public PlayerMovement2(Transform transform, UnityEngine.InputSystem.PlayerInput playerInput, Camera camera)
        {
            _transform = transform;
            _playerInput = playerInput;
            _camera = camera;
            SetInput();
        }

        public void Update()
        {
            if (_isMoving)
            {
                MoveTo(_inputPosition, Speed);
            }
        }

        public void OnDestroy()
        {
            ReleaseInput();
        }

        private void MoveTo(Vector3 position, float speed)
        {
            _tempPosition = Vector3.MoveTowards(_transform.position, position, speed * Time.deltaTime);
            _transform.position = _tempPosition;
            _isMoving = !(Mathf.Approximately(_tempPosition.x, position.x) && Mathf.Approximately(_tempPosition.y, position.y));
        }

        private void SetInput()
        {
            // https://gamedevbeginner.com/input-in-unity-made-easy-complete-guide-to-the-new-system/

            // WASD or GamePad -> performed is called once per key press
            var moveAction = _playerInput.actions["Move"];
            moveAction.performed += DoMove;
            moveAction.canceled += StopMove;

            // Pointer movement when pressed down -> move to given point even pointer is released.
            var clickAction = _playerInput.actions["Click"];
            clickAction.performed += DoClick;
        }

        private void ReleaseInput()
        {
            var moveAction = _playerInput.actions["Move"];
            moveAction.performed -= DoMove;
            moveAction.canceled -= StopMove;

            var clickAction = _playerInput.actions["Click"];
            clickAction.performed -= DoClick;
        }

        private void DoMove(InputAction.CallbackContext ctx)
        {
            _isMoving = true;
            _inputClick = ctx.ReadValue<Vector2>() * UnReachableDistance;
            _inputPosition = _transform.position;
            _inputPosition.x += _inputClick.x;
            _inputPosition.y += _inputClick.y;
            _inputPosition.x = Mathf.Clamp(_inputPosition.x, PlayerArea.xMin, PlayerArea.xMax);
            _inputPosition.y = Mathf.Clamp(_inputPosition.y, PlayerArea.yMin, PlayerArea.yMax);
        }

        private void StopMove(InputAction.CallbackContext ctx)
        {
            _isMoving = false;
        }

        private void DoClick(InputAction.CallbackContext ctx)
        {
            if (!_isMoving)
            {
                _isMoving = true;
            }
            _inputClick = ctx.ReadValue<Vector2>();
            _inputPosition.x = _inputClick.x;
            _inputPosition.y = _inputClick.y;
            _inputPosition = _camera.ScreenToWorldPoint(_inputPosition);
            _inputPosition.x = Mathf.Clamp(_inputPosition.x, PlayerArea.xMin, PlayerArea.xMax);
            _inputPosition.y = Mathf.Clamp(_inputPosition.y, PlayerArea.yMin, PlayerArea.yMax);
            _inputPosition.z = 0;
        }
    }
}
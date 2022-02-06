using System;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace Examples2.Scripts.Battle.Player2
{
    internal class PlayerMovement2
    {
        private readonly Transform _transform;
        private readonly UnityEngine.InputSystem.PlayerInput _playerInput;
        private readonly Camera _camera;
        private readonly bool _isLocal;
        private readonly MovementHelper _helper;

        public Rect PlayerArea { get; set; } = Rect.MinMaxRect(-100, -100, 100, 100);
        public float UnReachableDistance { get; set; } = 100;
        public float Speed { get; set; } = 1;

        private bool _isMoving;
        private Vector3 _targetPosition;
        private Vector3 _tempPosition;

        private Vector2 _inputClick;
        private Vector3 _inputPosition;

        public PlayerMovement2(Transform transform, UnityEngine.InputSystem.PlayerInput playerInput, Camera camera, PhotonView photonView)
        {
            _transform = transform;
            _playerInput = playerInput;
            _camera = camera;
            // In practice this might happen on runtime when players join and leves more than 256 times in a room.
            Assert.IsTrue(photonView.OwnerActorNr <= byte.MaxValue, "photonView.OwnerActorNr <= byte.MaxValue");
            var playerId = (byte)photonView.OwnerActorNr;
            _isLocal = photonView.IsMine;
            _helper = new MovementHelper(PhotonEventDispatcher.Get(), playerId, SetMoveTo);
            if (_isLocal)
            {
                SetupInput();
            }
        }

        public void Update()
        {
            if (_isMoving)
            {
                MoveTo();
            }
        }

        public void OnDestroy()
        {
            if (_isLocal)
            {
                ReleaseInput();
            }
        }

        private void SetMoveTo(Vector3 position, float speed)
        {
            _isMoving = true;
            _targetPosition = position;
            Speed = speed;
        }

        private void MoveTo()
        {
            _tempPosition = Vector3.MoveTowards(_transform.position, _targetPosition, Speed * Time.deltaTime);
            _transform.position = _tempPosition;
            _isMoving = !(Mathf.Approximately(_tempPosition.x, _targetPosition.x) && Mathf.Approximately(_tempPosition.y, _targetPosition.y));
        }

        private void SetupInput()
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
            _helper.SendMsgMoveTo(_inputPosition,Speed);
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
            _helper.SendMsgMoveTo(_inputPosition,Speed);
        }

        private class MovementHelper
        {
            private const int MsgMoveTo = PhotonEventDispatcher.EventCodeBase + 5;

            private readonly PhotonEventDispatcher _photonEventDispatcher;
            private readonly byte _playerId;
            private readonly Action<Vector3, float> _callback;

            private Vector3 _targetPosition;
            private byte[] _buffer = new byte[1 + 4 + 4 + 4];

            public MovementHelper(PhotonEventDispatcher photonEventDispatcher, byte playerId, Action<Vector3, float> onMsgMoveToCallback)
            {
                _photonEventDispatcher = photonEventDispatcher;
                _playerId = playerId;
                _photonEventDispatcher.RegisterEventListener(MsgMoveTo, data => { OnMsgMoveTo(data.CustomData); });
                _callback = onMsgMoveToCallback;
                _targetPosition.z = 0;
            }

            private void OnMsgMoveTo(object data)
            {
                var payload = (float[])data;
                if (payload[0] != _playerId)
                {
                    return;
                }
                _targetPosition.x = payload[1];
                _targetPosition.y = payload[2];
                _callback.Invoke(_targetPosition, payload[3]);
            }

            public void SendMsgMoveTo(Vector3 position, float speed)
            {
                var payload = new[] { _playerId, position.x, position.y, speed };
                _photonEventDispatcher.RaiseEvent(MsgMoveTo, payload);
            }
        }
    }
}
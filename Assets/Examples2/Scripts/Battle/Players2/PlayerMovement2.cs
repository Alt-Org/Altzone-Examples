using System;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace Examples2.Scripts.Battle.Players2
{
    internal class PlayerMovement2
    {
        private const byte MsgMoveTo = PhotonEventDispatcher.EventCodeBase + 5;

        private readonly Transform _transform;
        private readonly UnityEngine.InputSystem.PlayerInput _playerInput;
        private readonly Camera _camera;
        private readonly bool _isLocal;
        private readonly MovementHelper _helper;
        private readonly bool _isLimitMouseXY;

        public Rect PlayerArea { get; set; } = Rect.MinMaxRect(-100, -100, 100, 100);
        public float UnReachableDistance { get; set; } = 100;
        public float Speed { get; set; } = 1;

        private bool _stopped;

        public bool Stopped
        {
            get => _stopped;

            set
            {
                _stopped = value;
                if (_isMoving)
                {
                    _helper.SendMsgMoveTo(_transform.position, Speed);
                }
            }
        }

        private bool _isMoving;
        private Vector3 _targetPosition;
        private Vector3 _tempPosition;

        private Vector2 _inputClick;
        private Vector3 _inputPosition;

        public string StateString => $"{(_stopped ? "Stop" : _isMoving ? "Move" : "Idle")} {Speed:0.0}";

        public PlayerMovement2(Transform transform, UnityEngine.InputSystem.PlayerInput playerInput, Camera camera, PhotonView photonView)
        {
            _transform = transform;
            _playerInput = playerInput;
            _camera = camera;
            // In practice this might happen on runtime when players join and leves more than 256 times in a room.
            Assert.IsTrue(photonView.OwnerActorNr <= byte.MaxValue, "photonView.OwnerActorNr <= byte.MaxValue");
            var playerId = (byte)photonView.OwnerActorNr;
            _helper = new MovementHelper(PhotonEventDispatcher.Get(), MsgMoveTo, playerId, OnSetMoveTo);
            _isLocal = photonView.IsMine;
            if (_isLocal)
            {
                SetupInput();
            }
            _isLimitMouseXY = !Application.isMobilePlatform;
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

        private void OnSetMoveTo(Vector3 position, float speed)
        {
            _isMoving = speed > 0;
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
            if (Stopped)
            {
                return;
            }
            _inputClick = ctx.ReadValue<Vector2>() * UnReachableDistance;
            _isMoving = true;
            _inputPosition = _transform.position;
            _inputPosition.x += _inputClick.x;
            _inputPosition.y += _inputClick.y;
            _inputPosition.x = Mathf.Clamp(_inputPosition.x, PlayerArea.xMin, PlayerArea.xMax);
            _inputPosition.y = Mathf.Clamp(_inputPosition.y, PlayerArea.yMin, PlayerArea.yMax);
            _helper.SendMsgMoveTo(_inputPosition, Speed);
        }

        private void StopMove(InputAction.CallbackContext ctx)
        {
            _isMoving = false;
        }

        private void DoClick(InputAction.CallbackContext ctx)
        {
            if (Stopped)
            {
                return;
            }
            _inputClick = ctx.ReadValue<Vector2>();
#if UNITY_STANDALONE
            if (_isLimitMouseXY)
            {
                if (_inputClick.x < 0 || _inputClick.y < 0 ||
                    _inputClick.x > Screen.width || _inputClick.y > Screen.height)
                {
                    return;
                }
            }
#endif
            if (!_isMoving)
            {
                _isMoving = true;
            }
            _inputPosition.x = _inputClick.x;
            _inputPosition.y = _inputClick.y;
            _inputPosition = _camera.ScreenToWorldPoint(_inputPosition);
            _inputPosition.x = Mathf.Clamp(_inputPosition.x, PlayerArea.xMin, PlayerArea.xMax);
            _inputPosition.y = Mathf.Clamp(_inputPosition.y, PlayerArea.yMin, PlayerArea.yMax);
            _helper.SendMsgMoveTo(_inputPosition, Speed);
        }

        private class MovementHelper : AbstractPhotonEventHelper
        {
            private readonly Action<Vector3, float> _callback;

            private readonly byte[] _buffer = new byte[1 + 4 + 4 + 4];
            private Vector3 _targetPosition;

            public MovementHelper(PhotonEventDispatcher photonEventDispatcher, byte msgId, byte playerId, Action<Vector3, float> onMsgMoveToCallback)
                : base(photonEventDispatcher, msgId, playerId)
            {
                _callback = onMsgMoveToCallback;
                _buffer[0] = playerId;
                _targetPosition.z = 0;
            }

            public void SendMsgMoveTo(Vector3 position, float speed)
            {
                var index = 1;
                Array.Copy(BitConverter.GetBytes(position.x), 0, _buffer, index, 4);
                index += 4;
                Array.Copy(BitConverter.GetBytes(position.y), 0, _buffer, index, 4);
                index += 4;
                Array.Copy(BitConverter.GetBytes(speed), 0, _buffer, index, 4);

                RaiseEvent(_buffer);
            }

            protected override void OnMsgReceived(byte[] payload)
            {
                var index = 1;
                _targetPosition.x = BitConverter.ToSingle(payload, index);
                index += 4;
                _targetPosition.y = BitConverter.ToSingle(payload, index);
                index += 4;
                _callback.Invoke(_targetPosition, BitConverter.ToSingle(payload, index));
            }
        }
    }
}
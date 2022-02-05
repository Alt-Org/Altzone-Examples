using System;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Model;
using Examples2.Scripts.Battle.Ball;
using Examples2.Scripts.Battle.interfaces;
using Examples2.Scripts.Battle.Players;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace Examples2.Scripts.Battle.Player2
{
    internal class PlayerActor2 : PlayerActor, IPlayerActor
    {
        private const float Speed = 10f;

        [Header("Settings"), SerializeField] private SpriteRenderer _highlightSprite;
        [SerializeField] private SpriteRenderer _stateSprite;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private Transform _playerShieldHead;
        [SerializeField] private Transform _playerShieldFoot;
        [SerializeField] private UnityEngine.InputSystem.PlayerInput _playerInput;

        [Header("Live Data"), SerializeField] private Transform _playerShield;
        [SerializeField] private bool _isMoving;
        [SerializeField] private Vector2 _inputClick;
        [SerializeField] private Vector3 _inputPosition;
        [SerializeField] private Vector3 _tempPosition;

        [Header("Debug"), SerializeField] private TextMeshPro _playerInfo;

        private PhotonView _photonView;
        private Transform _transform;

        public void SetPhotonView(PhotonView photonView) => _photonView = photonView;

        private void Awake()
        {
            Debug.Log($"Awake {_photonView}");
            var player = _photonView.Owner;
            _transform = GetComponent<Transform>();
            _state.InitState(_transform, player);
            var prefix = $"{(player.IsLocal ? "L" : "R")}{PlayerPos}:{TeamNumber}";
            name = $"@{prefix}>{player.NickName}";
            _playerInfo = GetComponentInChildren<TextMeshPro>();
            _playerInfo.text = PlayerPos.ToString("N0");
            Debug.Log($"Awake {name}");
            this.Subscribe<BallManager.ActiveTeamEvent>(OnActiveTeamEvent);
            if (_photonView.IsMine)
            {
                _highlightSprite.color = Color.yellow;
            }
            _playerShield = PlayerPos <= PhotonBattle.PlayerPosition2
                ? _playerShieldHead
                : _playerShieldFoot;
            var model = PhotonBattle.GetPlayerCharacterModel(player);
            // Keep compiler happy, waiting more shield prefabs to fix this.
            var defence = model.MainDefence == Defence.Retroflection
                ? model.MainDefence
                : Defence.Retroflection;
            LoadShield(defence, PlayerPos, _playerShield);
            SetInput(Camera.main);
            Debug.Log($"Awake Done {name}");
        }

        private void SetInput(Camera mainCamera)
        {
            // https://gamedevbeginner.com/input-in-unity-made-easy-complete-guide-to-the-new-system/

            void StartMove(InputAction.CallbackContext ctx)
            {
                _inputClick = ctx.ReadValue<Vector2>();
            }

            void DoMove(InputAction.CallbackContext ctx)
            {
                _inputClick = ctx.ReadValue<Vector2>();
            }

            void StopMove(InputAction.CallbackContext ctx)
            {
                _inputClick = Vector2.zero;
            }

            void StartClick(InputAction.CallbackContext ctx)
            {
                _isMoving = true;
            }

            void DoClick(InputAction.CallbackContext ctx)
            {
                TrackPosition(ctx);
            }

            void TrackPosition(InputAction.CallbackContext ctx)
            {
                _inputClick = ctx.ReadValue<Vector2>();
                _inputPosition.x = _inputClick.x;
                _inputPosition.y = _inputClick.y;
                _inputPosition = mainCamera.ScreenToWorldPoint(_inputPosition);
                _inputPosition.z = 0;
                Debug.Log($"MoveTo {_inputPosition}");
            }

            var moveAction = _playerInput.actions["Move"];
            moveAction.started += StartMove;
            moveAction.performed += DoMove;
            moveAction.canceled += StopMove;

            var clickAction = _playerInput.actions["Click"];
            clickAction.started += StartClick;
            clickAction.performed += DoClick;
        }

        private bool MoveTo(Vector3 position, float speed)
        {
            var playArea = Rect.MinMaxRect(-100, -100, 100, 100);
            position.x = Mathf.Clamp(position.x, playArea.xMin, playArea.xMax);
            position.y = Mathf.Clamp(position.y, playArea.yMin, playArea.yMax);

            _tempPosition = Vector3.MoveTowards(_transform.position, position, speed * Time.deltaTime);
            _transform.position = _tempPosition;
            var isOnTarget = Mathf.Approximately(_tempPosition.x, position.x) && Mathf.Approximately(_tempPosition.y, position.y);
            return isOnTarget;
        }

        private void OnEnable()
        {
            Debug.Log($"OnEnable {name} IsMine {_photonView.IsMine} IsMaster {_photonView.Owner.IsMasterClient}");
            _state.FindTeamMember();
        }

        private void OnDestroy()
        {
            Debug.Log($"OnDestroy {name}");
            this.Unsubscribe();
        }

        private void Update()
        {
            if (_isMoving)
            {
                if (MoveTo(_inputPosition, Speed))
                {
                    _isMoving = false;
                }
            }
        }

        private static void LoadShield(Defence defence, int playerPos, Transform transform)
        {
            var prefab = Resources.Load<GameObject>($"Shields/{defence}");
            var instance = Instantiate(prefab, transform);
            if (playerPos > PhotonBattle.PlayerPosition2)
            {
                var renderer = instance.GetComponent<SpriteRenderer>();
                renderer.flipY = false;
            }
        }

        #region External events

        private void OnActiveTeamEvent(BallManager.ActiveTeamEvent data)
        {
            if (data.TeamIndex == _state._teamNumber)
            {
                // Ghosted -> Frozen is not allowed
                if (_state._currentMode != PlayModeNormal)
                {
                    return;
                }
                ((IPlayerActor)this).SetFrozenMode();
            }
            else
            {
                ((IPlayerActor)this).SetNormalMode();
            }
        }

        #endregion

        #region IPlayerActor

        Transform IPlayerActor.Transform => _state._transform;

        int IPlayerActor.PlayerPos => _state._playerPos;

        int IPlayerActor.TeamNumber => _state._teamNumber;

        IPlayerActor IPlayerActor.TeamMate => (IPlayerActor)_state._teamMate;

        void IPlayerActor.HeadCollision()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _photonView.RPC(nameof(SetPlayerPlayModeRpc), RpcTarget.All, PlayModeGhosted);
            }
        }

        void IPlayerActor.ShieldCollision()
        {
            // NOP - until game features are implemented
        }

        void IPlayerActor.SetNormalMode()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _photonView.RPC(nameof(SetPlayerPlayModeRpc), RpcTarget.All, PlayModeNormal);
            }
        }

        void IPlayerActor.SetFrozenMode()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _photonView.RPC(nameof(SetPlayerPlayModeRpc), RpcTarget.All, PlayModeFrozen);
            }
        }

        void IPlayerActor.SetGhostedMode()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _photonView.RPC(nameof(SetPlayerPlayModeRpc), RpcTarget.All, PlayModeGhosted);
            }
        }

        /*[PunRPC]*/
        public void SetPlayerPlayModeRpc(int playMode)
        {
            Assert.IsTrue(playMode >= PlayModeNormal && playMode <= PlayModeGhosted,
                "playMode >= PlayModeNormal && playMode <= PlayModeGhosted");
            _state._currentMode = playMode;
            switch (playMode)
            {
                case PlayModeNormal:
                    _collider.enabled = true;
                    _stateSprite.color = Color.blue;
                    return;
                case PlayModeFrozen:
                    _collider.enabled = true;
                    _stateSprite.color = Color.magenta;
                    return;
                case PlayModeGhosted:
                    _collider.enabled = false;
                    _stateSprite.color = Color.grey;
                    return;
            }
        }

        #endregion
    }
}
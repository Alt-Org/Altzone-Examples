using System;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using Examples2.Scripts.Battle.Ball;
using Examples2.Scripts.Battle.Factory;
using Examples2.Scripts.Battle.interfaces;
using Examples2.Scripts.Battle.Players;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Examples2.Scripts.Battle.Players2
{
    /// <summary>
    /// Game Actor for player, manages player and shield state. Movement is handled separately.
    /// </summary>
    internal class PlayerActor2 : PlayerActor, IPlayerActor
    {
        private const byte MsgPlayMode = PhotonEventDispatcher.EventCodeBase + 6;
        private const byte MsgShieldVisibility = PhotonEventDispatcher.EventCodeBase + 7;
        private const byte MsgShieldRotation = PhotonEventDispatcher.EventCodeBase + 8;

        private static readonly string[] StateNames = { "Norm", "Frozen", "Ghost" };

        private const string Tooltip1 = @"0=""Normal"", 1=""Frozen"", 2=""Ghosted""";

        [Header("Settings"), SerializeField, Tooltip(Tooltip1), Range(0, 2)] private int _startPlayMode;
        [SerializeField] private SpriteRenderer _highlightSprite;
        [SerializeField] private SpriteRenderer _stateSprite;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private Transform _playerShieldHead;
        [SerializeField] private Transform _playerShieldFoot;
        [SerializeField] private UnityEngine.InputSystem.PlayerInput _playerInput;

        [Header("Play Area"), SerializeField] private Rect _upperPlayArea;
        [SerializeField] private Rect _lowerPlayArea;

        [Header("Live Data"), SerializeField] private Transform _playerShield;

        [Header("Debug"), SerializeField] private TextMeshPro _playerInfo;
        [SerializeField] private bool _isShowDebugCanvas;

        private PhotonView _photonView;
        private Transform _transform;
        private PlayerMovement2 _playerMovement;
        private IPlayerShield2 _shield;
        private PhotonEventHelper _photonEvent;

        public void SetPhotonView(PhotonView photonView) => _photonView = photonView;

        public string StateString => $"{StateNames[_state._currentMode]} {((PlayerShield2)_shield).StateString} {_playerMovement.StateString}";

        private void Awake()
        {
            Debug.Log($"Awake {_photonView}");
            var player = _photonView.Owner;
            _transform = GetComponent<Transform>();
            _state.InitState(_transform, player);
            var prefix = $"{(player.IsLocal ? "L" : "R")}{PlayerPos}:{TeamNumber}";
            name = $"@{prefix}>{player.NickName}";
            SetDebug();
            // Must detect player position from actual y coordinate!
            var isYCoordNegative = _transform.position.y < 0;
            var isLower = isYCoordNegative;
            var features = RuntimeGameConfig.Get().Features;
            if (features._isRotateGameCamera)
            {
                var gameCameraInstance = Context.GetGameCamera;
                Assert.IsNotNull(gameCameraInstance, "gameCameraInstance != null");
                if (gameCameraInstance.IsRotated)
                {
                    // We are upside down!
                    isLower = !isLower;
                    Debug.Log($"RotatePlayer {_transform.name}");
                    _transform.Rotate(true);
                }
            }
            Debug.Log($"Awake {name} pos {_transform.position} isLower {isLower}");

            // Shield
            _playerShield = isLower
                ? _playerShieldHead
                : _playerShieldFoot;
            var model = PhotonBattle.GetPlayerCharacterModel(player);
            // Keep compiler happy, waiting more shield prefabs to fix this.
            var defence = model.MainDefence == Defence.Retroflection
                ? model.MainDefence
                : Defence.Retroflection;
            var shieldConfig = LoadShield(defence, _playerShield);
            var isShieldVisible = features._isSinglePlayerShieldOn && PhotonNetwork.CurrentRoom.PlayerCount == 1;
            _shield = new PlayerShield2(shieldConfig);
            _shield.Setup(PlayerPos, isLower, isShieldVisible, _startPlayMode, 0);

            // Player movement
            var playerArea = isYCoordNegative ? _lowerPlayArea : _upperPlayArea;
            _playerMovement = new PlayerMovement2(_transform, _playerInput, Camera.main, _photonView)
            {
                PlayerArea = playerArea,
                UnReachableDistance = 100,
                Speed = 10f,
            };
            var playerId = (byte)_photonView.OwnerActorNr;
            _photonEvent = new PhotonEventHelper(PhotonEventDispatcher.Get(), playerId);
            _photonEvent.RegisterEvent(MsgPlayMode, OnPlayModeCallback);
            _photonEvent.RegisterEvent(MsgShieldVisibility, OnShieldVisibilityCallback);
            _photonEvent.RegisterEvent(MsgShieldRotation, OnShieldRotationCallback);

            this.Subscribe<BallManager.ActiveTeamEvent>(OnActiveTeamEvent);

            Debug.Log($"Awake Done {name} playerArea {playerArea}");
        }

        private void SetDebug()
        {
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            var isDebugFlag = playerData.IsDebugFlag;
            _isShowDebugCanvas = isDebugFlag && _isShowDebugCanvas;
            _playerInfo = GetComponentInChildren<TextMeshPro>();
            if (_playerInfo != null)
            {
                if (isDebugFlag)
                {
                    _playerInfo.text = PlayerPos.ToString("N0");
                }
                else
                {
                    _playerInfo.enabled = false;
                }
            }
        }

        private void OnEnable()
        {
            Debug.Log($"OnEnable {name} IsMine {_photonView.IsMine} IsMaster {_photonView.Owner.IsMasterClient}");
            _state.FindTeamMember();
            OnSetPlayMode(_startPlayMode);
            if (_photonView.IsMine)
            {
                _highlightSprite.color = Color.yellow;
            }
            if (_isShowDebugCanvas && _photonView.IsMine)
            {
                var debugInfoPrefab = Resources.Load<PlayerDebugInfo>($"PlayerDebugInfo");
                var debugInfo = Instantiate(debugInfoPrefab, _transform);
                debugInfo.PlayerActor = this;
            }
        }

        private void OnDestroy()
        {
            Debug.Log($"OnDestroy {name}");
            this.Unsubscribe();
            _playerMovement.OnDestroy();
            _playerMovement = null;
        }

        private void Update()
        {
            _playerMovement.Update();
        }

        private static ShieldConfig LoadShield(Defence defence, Transform transform)
        {
            var shieldPrefab = defence == Defence.Retroflection
                ? Resources.Load<ShieldConfig>($"Shields/HotDogShield")
                : Resources.Load<ShieldConfig>($"Shields/{defence}");
            Assert.IsNotNull(shieldPrefab, "shieldPrefab != null");
            var shieldConfig = Instantiate(shieldPrefab, transform);
            shieldConfig.name = shieldConfig.name.Replace("(Clone)", string.Empty);
            return shieldConfig;
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
                ((IPlayerActor)this).SetGhostedMode();
            }
        }

        void IPlayerActor.ShieldCollision(Vector2 contactPoint)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                var rotationIndex = _shield.RotationIndex + 1;
                SendShieldRotationRpc(rotationIndex, contactPoint);
            }
        }

        void IPlayerActor.SetNormalMode()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SendPlayModeRpc(PlayModeNormal);
            }
        }

        void IPlayerActor.SetFrozenMode()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SendPlayModeRpc(PlayModeFrozen);
            }
        }

        void IPlayerActor.SetGhostedMode()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SendPlayModeRpc(PlayModeGhosted);
            }
        }

        private void OnSetShieldVisibility(bool isVisible)
        {
            _shield.SetVisibility(isVisible);
        }

        private void OnSetPlayMode(int playMode)
        {
            Debug.Log($"OnSetPlayMode {name} {StateNames[playMode]}");
            Assert.IsTrue(playMode >= PlayModeNormal && playMode <= PlayModeGhosted,
                "playMode >= PlayModeNormal && playMode <= PlayModeGhosted");
            _state._currentMode = playMode;
            switch (playMode)
            {
                case PlayModeNormal:
                    _collider.enabled = true;
                    _playerMovement.Stopped = false;
                    _stateSprite.color = Color.blue;
                    break;
                case PlayModeFrozen:
                    _collider.enabled = true;
                    _playerMovement.Stopped = true;
                    _stateSprite.color = Color.magenta;
                    break;
                case PlayModeGhosted:
                    _collider.enabled = false;
                    _playerMovement.Stopped = false;
                    _stateSprite.color = Color.grey;
                    break;
            }
            _shield.SetPlayMode(playMode);
        }

        private void OnSetShieldRotation(int rotationIndex, Vector2 contactPoint)
        {
            _shield.SetRotation(rotationIndex);
            _shield.PlayHitEffects(contactPoint);
        }

        #endregion

        #region Photon Event (RPC Message) Marshalling

        /// <summary>
        /// Naming convention for message buffer is _msg-PlayerPlayMode-Buffer.<br />
        /// Naming convention to send message over networks is Send-PlayerPlayMode-Rpc.<br />
        /// Naming convention to receive message from networks is On-PlayerPlayMode-Callback.<br />
        /// Naming convention to call actual implementation is On-SetPlayerPlayMode.
        /// </summary>

        // PlayMode
        private readonly byte[] _msgPlayModeBuffer = new byte[1 + 1];

        private byte[] PlayModeToBytes(int playMode)
        {
            _msgPlayModeBuffer[1] = (byte)playMode;

            return _msgPlayModeBuffer;
        }

        private void SendPlayModeRpc(int playMode)
        {
            _photonEvent.SendEvent(MsgPlayMode, PlayModeToBytes(playMode));
        }

        private void OnPlayModeCallback(byte[] payload)
        {
            var playMode = payload[1];

            OnSetPlayMode(playMode);
        }

        // ShieldVisibility
        private readonly byte[] _msgShieldVisibilityBuffer = new byte[1 + 1];

        private byte[] ShieldVisibilityToBytes(bool isVisible)
        {
            _msgShieldVisibilityBuffer[1] = (byte)(isVisible ? 1 : 0);

            return _msgShieldVisibilityBuffer;
        }

        private void SendShieldVisibilityRpc(bool isVisible)
        {
            _photonEvent.SendEvent(MsgShieldVisibility, ShieldVisibilityToBytes(isVisible));
        }

        private void OnShieldVisibilityCallback(byte[] payload)
        {
            var isVisible = payload[1] == 1;

            OnSetShieldVisibility(isVisible);
        }

        // ShieldRotation
        private readonly byte[] _msgShieldRotationBuffer = new byte[1 + 1 + 4 + 4];

        private byte[] ShieldRotationToBytes(int rotationIndex, Vector2 contactPoint)
        {
            var index = 1;
            _msgShieldRotationBuffer[index] = (byte)rotationIndex;
            index += 1;
            Array.Copy(BitConverter.GetBytes(contactPoint.x), 0, _msgShieldRotationBuffer, index, 4);
            index += 4;
            Array.Copy(BitConverter.GetBytes(contactPoint.y), 0, _msgShieldRotationBuffer, index, 4);

            return _msgShieldRotationBuffer;
        }

        private void SendShieldRotationRpc(int rotationIndex, Vector2 contactPoint)
        {
            _photonEvent.SendEvent(MsgShieldRotation, ShieldRotationToBytes(rotationIndex, contactPoint));
        }

        private void OnShieldRotationCallback(byte[] payload)
        {
            var index = 1;
            var rotationIndex = payload[index];
            Vector2 contactPoint;
            index += 1;
            contactPoint.x = BitConverter.ToSingle(payload, index);
            index += 4;
            contactPoint.y = BitConverter.ToSingle(payload, index);

            OnSetShieldRotation(rotationIndex, contactPoint);
        }

        #endregion
    }
}
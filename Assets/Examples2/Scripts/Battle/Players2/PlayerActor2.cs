using System;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using Examples2.Scripts.Battle.Ball;
using Examples2.Scripts.Battle.interfaces;
using Examples2.Scripts.Battle.Players;
using Examples2.Scripts.Battle.Room;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Examples2.Scripts.Battle.Players2
{
    internal class PlayerActor2 : PlayerActor, IPlayerActor
    {
        private static string[] StateNames = new[] { "Norm", "Frozen", "Ghost" };

        private const byte MsgVisualState = PhotonEventDispatcher.EventCodeBase + 6;

        [Header("Settings"), SerializeField] private SpriteRenderer _highlightSprite;
        [SerializeField] private SpriteRenderer _stateSprite;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private Transform _playerShieldHead;
        [SerializeField] private Transform _playerShieldFoot;
        [SerializeField] private UnityEngine.InputSystem.PlayerInput _playerInput;

        [Header("Play Area"), SerializeField] private Rect _upperPlayArea;
        [SerializeField] private Rect _lowerPlayArea;

        [Header("Live Data"), SerializeField] private Transform _playerShield;
        [SerializeField] private int _rotationIndex;

        [Header("Debug"), SerializeField] private TextMeshPro _playerInfo;
        [SerializeField] private bool _isShowDebugCanvas;

        private PhotonView _photonView;
        private Transform _transform;
        private PlayerMovement2 _playerMovement;
        private PlayerPlayModeHelper _helper;
        private IPlayerShield _shield;

        public void SetPhotonView(PhotonView photonView) => _photonView = photonView;

        public string StateString => $"{StateNames[_state._currentMode]} {_playerMovement.StateString} {((PlayerShield2)_shield).StateString}";

        private void Awake()
        {
            Debug.Log($"Awake {_photonView}");
            var player = _photonView.Owner;
            _transform = GetComponent<Transform>();
            _state.InitState(_transform, player);
            var prefix = $"{(player.IsLocal ? "L" : "R")}{PlayerPos}:{TeamNumber}";
            name = $"@{prefix}>{player.NickName}";
            _playerInfo = GetComponentInChildren<TextMeshPro>();
            if (_playerInfo != null)
            {
                _playerInfo.text = PlayerPos.ToString("N0");
            }
            // Must detect player position from actual y coordinate!
            var isYCoordNegative = _transform.position.y < 0;
            var isLower = isYCoordNegative;
            var features = RuntimeGameConfig.Get().Features;
            if (features._isRotateGameCamera)
            {
                var gameCameraInstance = FindObjectOfType<GameCamera>();
                Assert.IsNotNull(gameCameraInstance, "gameCameraInstance != null");
                if (gameCameraInstance.IsRotated)
                {
                    isLower = !isLower;
                    RotatePlayer(_transform, true);
                }
            }
            Debug.Log($"Awake {name} pos {_transform.position} isLower {isLower}");

            this.Subscribe<BallManager.ActiveTeamEvent>(OnActiveTeamEvent);
            if (_photonView.IsMine)
            {
                _highlightSprite.color = Color.yellow;
            }
            _playerShield = isLower
                ? _playerShieldHead
                : _playerShieldFoot;
            var model = PhotonBattle.GetPlayerCharacterModel(player);
            // Keep compiler happy, waiting more shield prefabs to fix this.
            var defence = model.MainDefence == Defence.Retroflection
                ? model.MainDefence
                : Defence.Retroflection;
            _shield = LoadShield(defence, _playerShield, _photonView);
            _shield.SetupShield(PlayerPos, isLower);
            _rotationIndex = 0;
            var playerArea = isYCoordNegative ? _lowerPlayArea : _upperPlayArea;
            _playerMovement = new PlayerMovement2(_transform, _playerInput, Camera.main, _photonView)
            {
                PlayerArea = playerArea,
                UnReachableDistance = 100,
                Speed = 10f,
            };
            var playerId = (byte)_photonView.OwnerActorNr;
            _helper = new PlayerPlayModeHelper(PhotonEventDispatcher.Get(), MsgVisualState, playerId, SetPlayerPlayMode);
            Debug.Log($"Awake Done {name} playerArea {playerArea}");
        }

        private void OnEnable()
        {
            Debug.Log($"OnEnable {name} IsMine {_photonView.IsMine} IsMaster {_photonView.Owner.IsMasterClient}");
            _state.FindTeamMember();
            ((IPlayerActor)this).SetNormalMode();
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

        private static IPlayerShield LoadShield(Defence defence, Transform transform, PhotonView photonView)
        {
            var shieldPrefab = defence == Defence.Retroflection
                ? Resources.Load<ShieldConfig>($"Shields/HotDogShield")
                : Resources.Load<ShieldConfig>($"Shields/{defence}");
            Assert.IsNotNull(shieldPrefab, "shieldPrefab != null");
            var shieldConfig = Instantiate(shieldPrefab, transform);
            shieldConfig.name = shieldConfig.name.Replace("(Clone)", string.Empty);
            var shield = new PlayerShield2(shieldConfig, photonView) as IPlayerShield;
            return shield;
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
                _helper.SendSetPlayerPlayMode(PlayModeGhosted);
            }
        }

        void IPlayerActor.ShieldCollision()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _rotationIndex += 1;
                _shield.SetShieldState(_state._currentMode, _rotationIndex);
            }
        }

        void IPlayerActor.SetNormalMode()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _helper.SendSetPlayerPlayMode(PlayModeNormal);
            }
        }

        void IPlayerActor.SetFrozenMode()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _helper.SendSetPlayerPlayMode(PlayModeFrozen);
            }
        }

        void IPlayerActor.SetGhostedMode()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _helper.SendSetPlayerPlayMode(PlayModeGhosted);
            }
        }

        private void SetPlayerPlayMode(int playMode)
        {
            Debug.Log($"SetPlayerPlayMode {name} {playMode}");
            Assert.IsTrue(playMode >= PlayModeNormal && playMode <= PlayModeGhosted,
                "playMode >= PlayModeNormal && playMode <= PlayModeGhosted");
            _state._currentMode = playMode;
            switch (playMode)
            {
                case PlayModeNormal:
                    _collider.enabled = true;
                    _stateSprite.color = Color.blue;
                    break;
                case PlayModeFrozen:
                    _collider.enabled = true;
                    _stateSprite.color = Color.magenta;
                    break;
                case PlayModeGhosted:
                    _collider.enabled = false;
                    _stateSprite.color = Color.grey;
                    break;
            }
            if (PhotonNetwork.IsMasterClient)
            {
                _shield.SetShieldState(playMode, _rotationIndex);
            }
        }

        #endregion

        private static void RotatePlayer(Transform playerTransform, bool upsideDown)
        {
            Debug.Log($"RotatePlayer {playerTransform.name} upsideDown {upsideDown}");
            var rotation = upsideDown
                ? Quaternion.Euler(0f, 0f, 180f) // Upside down
                : Quaternion.Euler(0f, 0f, 0f); // Normal orientation
            playerTransform.rotation = rotation;
        }

        private class PlayerPlayModeHelper : AbstractPhotonEventHelper
        {
            private readonly Action<int> _onSetPlayerPlayMode;

            private readonly byte[] _buffer = new byte[1 + 1];

            public PlayerPlayModeHelper(PhotonEventDispatcher photonEventDispatcher, byte msgId, byte playerId, Action<int> onSetPlayerPlayMode)
                : base(photonEventDispatcher, msgId, playerId)
            {
                _onSetPlayerPlayMode = onSetPlayerPlayMode;
                _buffer[0] = playerId;
            }

            public void SendSetPlayerPlayMode(int playMode)
            {
                _buffer[1] = (byte)playMode;
                RaiseEvent(_buffer);
            }

            protected override void OnMsgReceived(byte[] payload)
            {
                _onSetPlayerPlayMode(payload[1]);
            }
        }
    }
}
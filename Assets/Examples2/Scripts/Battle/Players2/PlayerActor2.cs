using System;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Model;
using Examples2.Scripts.Battle.Ball;
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
    internal class PlayerActor2 : PlayerActor, IPlayerActor
    {
        private const byte MsgVisualState = PhotonEventDispatcher.EventCodeBase + 6;

        [Header("Settings"), SerializeField] private SpriteRenderer _highlightSprite;
        [SerializeField] private SpriteRenderer _stateSprite;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private Transform _playerShieldHead;
        [SerializeField] private Transform _playerShieldFoot;
        [SerializeField] private UnityEngine.InputSystem.PlayerInput _playerInput;

        [Header("Live Data"), SerializeField] private Transform _playerShield;

        [Header("Debug"), SerializeField] private TextMeshPro _playerInfo;
        [SerializeField] private int _rotationIndex;

        private PhotonView _photonView;
        private Transform _transform;
        private PlayerMovement2 _playerMovement;
        private PlayerPlayModeHelper _helper;
        private IPlayerShield _shield;

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
            var isLower = PlayerPos <= PhotonBattle.PlayerPosition2;
            _playerShield = isLower
                ? _playerShieldHead
                : _playerShieldFoot;
            var model = PhotonBattle.GetPlayerCharacterModel(player);
            // Keep compiler happy, waiting more shield prefabs to fix this.
            var defence = model.MainDefence == Defence.Retroflection
                ? model.MainDefence
                : Defence.Retroflection;
            _shield = LoadShield(defence, PlayerPos, _playerShield, _photonView);
            _rotationIndex = 0;
            var playerArea = isLower
                ? Rect.MinMaxRect(-4.5f, -8f, 4.5f, 0f)
                : Rect.MinMaxRect(-4.5f, 0f, 4.5f, 8f);
            _playerMovement = new PlayerMovement2(_transform, _playerInput, Camera.main, _photonView)
            {
                PlayerArea = playerArea,
                UnReachableDistance = 100,
                Speed = 10f,
            };
            var playerId = (byte)_photonView.OwnerActorNr;
            _helper = new PlayerPlayModeHelper(PhotonEventDispatcher.Get(), MsgVisualState, playerId, SetPlayerPlayMode);
            Debug.Log($"Awake Done {name}");
        }

        private void OnEnable()
        {
            Debug.Log($"OnEnable {name} IsMine {_photonView.IsMine} IsMaster {_photonView.Owner.IsMasterClient}");
            _state.FindTeamMember();
            ((IPlayerActor)this).SetNormalMode();
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

        private static IPlayerShield LoadShield(Defence defence, int playerPos, Transform transform, PhotonView photonView)
        {
            var shieldPrefab = Resources.Load<ShieldConfig>($"Shields/HotDogShield");
            Assert.IsNotNull(shieldPrefab, "shieldPrefab != null");
            var shieldConfig = Instantiate(shieldPrefab, transform);
            shieldConfig.name = shieldConfig.name.Replace("(Clone)", string.Empty);
            var shield = new PlayerShield2(shieldConfig, photonView) as IPlayerShield;
            shield.SetupShield(playerPos);
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
            Debug.Log($"SetPlayerPlayMode {playMode}");
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
            _shield.SetShieldState(playMode, _rotationIndex);
        }

        #endregion

        private class PlayerPlayModeHelper : AbstractPhotonEventHelper
        {
            private readonly Action<int> _onSetPlayerPlayMode;

            private readonly byte[] _buffer = new byte[1 + 1];

            public PlayerPlayModeHelper(PhotonEventDispatcher photonEventDispatcher, byte msgId, byte playerId, Action<int> onSetPlayerPlayMode)
                : base(photonEventDispatcher, msgId, playerId)
            {
                _onSetPlayerPlayMode = onSetPlayerPlayMode;
            }

            public void SendSetPlayerPlayMode(int playMode)
            {
                _buffer[1] = (byte)playMode;
                RaiseEvent(_buffer);
            }

            protected override void OnMsgReceived(byte[] payload)
            {
                _onSetPlayerPlayMode((int)payload[1]);
            }
        }
    }
}
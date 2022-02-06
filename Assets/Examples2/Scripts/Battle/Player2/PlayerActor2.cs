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

namespace Examples2.Scripts.Battle.Player2
{
    internal class PlayerActor2 : PlayerActor, IPlayerActor
    {
        [Header("Settings"), SerializeField] private SpriteRenderer _highlightSprite;
        [SerializeField] private SpriteRenderer _stateSprite;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private Transform _playerShieldHead;
        [SerializeField] private Transform _playerShieldFoot;
        [SerializeField] private UnityEngine.InputSystem.PlayerInput _playerInput;

        [Header("Live Data"), SerializeField] private Transform _playerShield;

        [Header("Debug"), SerializeField] private TextMeshPro _playerInfo;

        private PhotonView _photonView;
        private Transform _transform;
        private PlayerMovement2 _playerMovement;

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
            LoadShield(defence, PlayerPos, _playerShield);
            var playerArea = isLower
                ? Rect.MinMaxRect(-4.5f, -8f, 4.5f, 0f)
                : Rect.MinMaxRect(-4.5f, 0f, 4.5f, 8f);
            _playerMovement = new PlayerMovement2(_transform, _playerInput, Camera.main, _photonView)
            {
                PlayerArea = playerArea,
                UnReachableDistance = 100,
                Speed = 10f,
            };
            Debug.Log($"Awake Done {name}");
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
            _playerMovement.OnDestroy();
            _playerMovement = null;
        }

        private void Update()
        {
            _playerMovement.Update();
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
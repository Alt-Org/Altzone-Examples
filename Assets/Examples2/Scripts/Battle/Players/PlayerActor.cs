using System;
using System.Linq;
using Examples2.Scripts.Battle.Ball;
using Examples2.Scripts.Battle.interfaces;
using Examples2.Scripts.Battle.Photon;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using TMPro;
using UnityEngine;

namespace Examples2.Scripts.Battle.Players
{
    [RequireComponent(typeof(PhotonView))]
    internal class PlayerActor : MonoBehaviour, IPlayerActor
    {
        private const int PlayModeNormal = 0;
        private const int PlayModeFrozen = 1;
        private const int PlayModeGhosted = 2;

        [Serializable]
        internal class PlayerState
        {
            public int _currentMode;
            public Transform _transform;
            public int _playerPos;
            public int _teamIndex;
            public PlayerActor _teamMate;
        }

        [Header("Settings"), SerializeField] private SpriteRenderer _highlightSprite;
        [SerializeField] private SpriteRenderer _stateSprite;
        [SerializeField] private Collider2D _collider;

        [Header("Live Data"), SerializeField] private PlayerState _state;

        [Header("Debug"), SerializeField] private TMP_Text _playerInfo;

        private PhotonView _photonView;

        private void Awake()
        {
            _photonView = PhotonView.Get(this);
            var player = _photonView.Owner;
            _state._currentMode = PlayModeNormal;
            _state._transform = GetComponent<Transform>();
            _state._playerPos = PhotonBattle.GetPlayerPos(player);
            _state._teamIndex = PhotonBattle.GetTeamIndex(_state._playerPos);
            var prefix = $"{(player.IsLocal ? "L" : "R")}{_state._playerPos}:{_state._teamIndex}";
            name = $"{prefix}:{player.NickName}";
            _playerInfo = GetComponentInChildren<TMP_Text>();
            _playerInfo.text = _state._playerPos.ToString("N0");
            Debug.Log($"Awake {name}");
            this.Subscribe<BallManager.ActiveTeamEvent>(OnActiveTeamEvent);
            if (_photonView.IsMine)
            {
                _highlightSprite.color = Color.yellow;
            }
        }

        private void OnEnable()
        {
            var players = FindObjectsOfType<PlayerActor>();
            Debug.Log($"OnEnable {name} IsMine {_photonView.IsMine} IsMaster {_photonView.Owner.IsMasterClient} players {players.Length}");
            _state._teamMate = players
                .FirstOrDefault(x => x._state._teamIndex == _state._teamIndex && x._state._playerPos != _state._playerPos);
            gameObject.AddComponent<LocalPlayer>();
            ((IPlayerActor)this).SetNormalMode();
        }

        private void OnActiveTeamEvent(BallManager.ActiveTeamEvent data)
        {
            if (data.TeamIndex == _state._teamIndex)
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

        Transform IPlayerActor.Transform => _state._transform;

        int IPlayerActor.PlayerPos => _state._playerPos;

        int IPlayerActor.TeamIndex => _state._teamIndex;

        IPlayerActor IPlayerActor.TeamMate => _state._teamMate;

        void IPlayerActor.HeadCollision()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _photonView.RPC(nameof(SetPlayerPlayModeRpc), RpcTarget.All, PlayModeGhosted);
            }
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

        [PunRPC]
        private void SetPlayerPlayModeRpc(int playMode)
        {
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
    }
}
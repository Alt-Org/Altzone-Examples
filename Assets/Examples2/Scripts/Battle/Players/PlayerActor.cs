using System;
using System.Linq;
using Examples2.Scripts.Battle.Room;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Examples2.Scripts.Battle.Players
{
    [RequireComponent(typeof(PhotonView))]
    public class PlayerActor : MonoBehaviour
    {
        [Serializable]
        internal class PlayerState
        {
            public int _playerPos;
            public int _teamIndex;
            public PlayerActor _teamMate;
        }

        [Header("Live Data"), SerializeField]private PlayerState _state;

        [Header("Debug"), SerializeField] private TMP_Text _playerInfo;

        private PhotonView _photonView;

        private void Awake()
        {
            _photonView = PhotonView.Get(this);
            var player = _photonView.Owner;
            _state._playerPos = PhotonBattle.GetPlayerPos(player);
            _state._teamIndex = PhotonBattle.GetTeamIndex(_state._playerPos);
            name = $"{(player.IsLocal ? "L" : "R")}{_state._playerPos}:{_state._teamIndex}:{player.NickName}";
            _playerInfo = GetComponentInChildren<TMP_Text>();
            _playerInfo.text = _state._playerPos.ToString("N0");
            Debug.Log($"Awake {name}");
        }

        private void OnEnable()
        {
            var players = FindObjectsOfType<PlayerActor>();
            Debug.Log($"OnEnable {name} IsMine {_photonView.IsMine} IsMaster {_photonView.Owner.IsMasterClient} players {players.Length}");
            _state._teamMate = players
                .FirstOrDefault(x => x._state._teamIndex == _state._teamIndex && x._state._playerPos != _state._playerPos);
            gameObject.AddComponent<LocalPlayer>();
        }
    }
}
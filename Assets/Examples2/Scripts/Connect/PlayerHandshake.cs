using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

namespace Examples2.Scripts.Connect
{
    [Serializable]
    public class PlayerHandshakeState
    {
        public int _localActorNumber;
        public int _playerPos;
        public int _playerActorNumber;
        public int _messagesOut;
        public int _messagesIn;

        public bool IsMine(int playerPos, int localActorNumber, int playerActorNumber)
        {
            return playerPos == _playerPos && localActorNumber == _localActorNumber && playerActorNumber == _playerActorNumber;
        }

        public override string ToString()
        {
            return $"{_localActorNumber}-{_playerPos} {_playerActorNumber} : o={_messagesOut} i={_messagesIn}";
        }
    }

    public class PlayerHandshake : MonoBehaviour
    {
        [Header("Live Data"), SerializeField] private PhotonView _photonView;
        [SerializeField] private PlayerConnection _playerConnection;
        [SerializeField] private PlayerHandshakeState _state;

        private readonly List<PlayerHandshakeState> _states = new List<PlayerHandshakeState>();

        private void OnEnable()
        {
            _photonView = PhotonView.Get(this);
            _playerConnection = GetComponent<PlayerConnection>();
            _state = new PlayerHandshakeState
            {
                _localActorNumber = PhotonNetwork.LocalPlayer.ActorNumber,
                _playerPos = _playerConnection.PlayerPos,
                _playerActorNumber = _playerConnection.ActorNumber
            };
            _states.Clear();
            Debug.Log($"OnEnable {PhotonNetwork.NetworkClientState} {_photonView} {_photonView.Controller.GetDebugLabel()}");
            SendMessage();
        }

        private void OnDisable()
        {
            Debug.Log($"OnDisable {PhotonNetwork.NetworkClientState} {_photonView} {_photonView.Controller.GetDebugLabel()}");
        }

        private void SendMessage()
        {
            _state._messagesOut += 1;
            Debug.Log($"SendMessage SEND state {_state}");
            _photonView.RPC(nameof(SendMessageRpc), RpcTarget.Others, _state._localActorNumber, _state._playerPos, _state._playerActorNumber);
        }

        [PunRPC]
        private void SendMessageRpc(int localActorNumber, int playerPos, int playerActorNumber)
        {
            _state._messagesIn += 1;
            Debug.Log($"SendMessageRpc state {_state} RECV {localActorNumber}-{playerPos} {playerActorNumber}");
            _playerConnection.UpdatePeers(_state);
            if (_state.IsMine(playerPos, localActorNumber, playerActorNumber))
            {
                return;
            }
            var otherState = _states.FirstOrDefault(x => x.IsMine(playerPos, localActorNumber, playerActorNumber));
            if (otherState == null)
            {
                otherState = new PlayerHandshakeState
                {
                    _localActorNumber = localActorNumber,
                    _playerPos = playerPos,
                    _playerActorNumber = playerActorNumber
                };
                _states.Add(otherState);
            }
            otherState._messagesIn += 1;
            _playerConnection.UpdatePeers(otherState);
        }
    }
}
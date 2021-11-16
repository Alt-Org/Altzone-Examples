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
        public int _playerPos;
        public int _localActorNumber;
        public int _playerActorNumber;
        public int _messagesOut;
        public int _messagesIn;

        public bool IsMine(int playerPos, int localActorNumber, int playerActorNumber)
        {
            return playerPos == _playerPos && localActorNumber == _localActorNumber && playerActorNumber == _playerActorNumber;
        }

        public int GetHandle()
        {
            return (_playerPos << 16) + (_localActorNumber << 8) + _playerActorNumber;
        }

        public static string FormatTitle(int playerPos, int playerActorNumber)
        {
            return $"handle {playerPos}-{PhotonNetwork.LocalPlayer.ActorNumber}-{playerActorNumber}";
        }

        public override string ToString()
        {
            return $"{_playerPos}-{_localActorNumber}-{_playerActorNumber} : o={_messagesOut} i={_messagesIn}";
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
                _playerPos = _playerConnection.PlayerPos,
                _localActorNumber = PhotonNetwork.LocalPlayer.ActorNumber,
                _playerActorNumber = _playerConnection.ActorNumber
            };
            _states.Clear();
            Debug.Log($"OnEnable {PhotonNetwork.NetworkClientState} {_photonView} {_photonView.Controller.GetDebugLabel()}");
            SendMessageOut();
        }

        private void OnDisable()
        {
            Debug.Log($"OnDisable {PhotonNetwork.NetworkClientState} {_photonView} {_photonView.Controller.GetDebugLabel()}");
        }

        public void RemoveActor(int actorNumber)
        {
            var remove = _states
                .Where(x => x._localActorNumber == actorNumber || x._playerActorNumber == actorNumber).ToList();
            Debug.Log($"RemoveActor state {_state} actorNumber {actorNumber} states {_states.Count} remove {remove.Count}");
            foreach (var state in remove)
            {
                _playerConnection.UpdatePeers(state, 0);
                _states.Remove(state);
            }
        }

        private void SendMessageOut()
        {
            _state._messagesOut += 1;
            Debug.Log($"SendMessageOut SEND state {_state}");
            _photonView.RPC(nameof(SendMessageRpc), RpcTarget.Others, _state._playerPos, _state._localActorNumber, _state._playerActorNumber);
        }

        [PunRPC]
        private void SendMessageRpc(int playerPos, int localActorNumber, int playerActorNumber)
        {
            _state._messagesIn += 1;
            Debug.Log($"SendMessageRpc state {_state} RECV {playerPos}-{localActorNumber}-{playerActorNumber}");
            _playerConnection.UpdatePeers(_state, 1);
            if (_state.IsMine(playerPos, localActorNumber, playerActorNumber))
            {
                return;
            }
            var otherState = _states.FirstOrDefault(x => x.IsMine(playerPos, localActorNumber, playerActorNumber));
            if (otherState == null)
            {
                otherState = new PlayerHandshakeState
                {
                    _playerPos = playerPos,
                    _localActorNumber = localActorNumber,
                    _playerActorNumber = playerActorNumber
                };
                _states.Add(otherState);
            }
            otherState._messagesIn += 1;
            _playerConnection.UpdatePeers(otherState, 1);
            if (otherState._messagesIn == 1)
            {
                // Resend again for new peers.
                SendMessageOut();
            }
        }
    }
}
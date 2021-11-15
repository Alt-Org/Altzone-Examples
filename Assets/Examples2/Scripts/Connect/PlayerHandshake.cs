using Photon.Pun;
using UnityEngine;

namespace Examples2.Scripts.Connect
{
    public class PlayerHandshake : MonoBehaviour
    {
        [SerializeField] private PhotonView _photonView;
        [SerializeField] private PlayerConnection _playerConnection;

        [SerializeField] private int _playerPos;
        [SerializeField] private int _localActorNumber;
        [SerializeField] private int _playerActorNumber;

        private void OnEnable()
        {
            _photonView = PhotonView.Get(this);
            _playerConnection = GetComponent<PlayerConnection>();
            _playerPos = _playerConnection.PlayerPos;
            _localActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            _playerActorNumber = _playerConnection.ActorNumber;
            Debug.Log($"OnEnable {PhotonNetwork.NetworkClientState} {_photonView} {_photonView.Controller.GetDebugLabel()}");
        }

        private void OnDisable()
        {
            Debug.Log($"OnDisable {PhotonNetwork.NetworkClientState} {_photonView} {_photonView.Controller.GetDebugLabel()}");
        }

        private void SendMessage()
        {
            Debug.Log($"SendMessage SEND pp={_playerPos} L={_localActorNumber} A={_playerActorNumber}");
            _photonView.RPC(nameof(SendMessageRpc), RpcTarget.Others, _playerPos, _localActorNumber, _playerActorNumber);
        }

        [PunRPC]
        private void SendMessageRpc(int playerPos, int localActorNumber, int playerActorNumber)
        {
            Debug.Log($"SendMessageRpc RECV pp={playerPos} L={localActorNumber} A={playerActorNumber}");
        }
    }
}
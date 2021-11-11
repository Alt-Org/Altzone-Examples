using Photon.Pun;
using UnityEngine;

namespace Examples2.Scripts.Connect
{
    public class PlayerHandshake : MonoBehaviour
    {
        private const string HelloMessage = "hello";

        [SerializeField] private PhotonView _photonView;
        [SerializeField] private ConnectInfo _connectInfo;
        [SerializeField] private int _playerId;
        [SerializeField] private int _instanceId;

        private void OnEnable()
        {
            _photonView = PhotonView.Get(this);
            if (_photonView.IsMine)
            {
                _instanceId = 1;
            }
            Debug.Log($"OnEnable {_playerId}:{_instanceId} {PhotonNetwork.NetworkClientState} mine {_photonView.IsMine} room {_photonView.IsRoomView}");
            _photonView.RPC(nameof(SendMessageRpc), RpcTarget.All, HelloMessage);
        }

        private void OnDisable()
        {
            Debug.Log($"OnDisable {_playerId}:{_instanceId} {PhotonNetwork.NetworkClientState} mine {_photonView.IsMine} room {_photonView.IsRoomView}");
        }

        public void SetPlayerId(int playerId, ConnectInfo connectInfo)
        {
            _playerId = playerId;
            _connectInfo = connectInfo;
            _connectInfo.SetInstanceId(_instanceId);
        }

        [PunRPC]
        private void SendMessageRpc(string message)
        {
            Debug.Log($"RECV {_playerId}:{_instanceId} {_photonView.Controller.GetDebugLabel()} message {message}");
        }
    }
}
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;

namespace Examples2.Scripts.Connect
{
    public class PlayerConnection : MonoBehaviourPunCallbacks
    {
        [Header("Settings"), SerializeField] private ConnectInfo _connectInfo;
        [SerializeField] private int _playerPos;

        [Header("Live Data"), SerializeField] private PhotonView _photonView;
        [SerializeField] private int _actorNumber;
        [SerializeField] private short _handle;
        [SerializeField] private PlayerHandshake _playerHandshake;

        public int PlayerPos => _playerPos;
        public int ActorNumber => _actorNumber;

        public override void OnEnable()
        {
            base.OnEnable();
            _photonView = PhotonView.Get(this);
            Debug.Log($"OnEnable {_playerPos} {PhotonNetwork.NetworkClientState} {_photonView}");
        }

        public override void OnDisable()
        {
            base.OnDisable();
            Debug.Log($"OnDisable {_playerPos} {PhotonNetwork.NetworkClientState} {_photonView}");
            _photonView = null;
        }

        public void SetPhotonPlayer(Player player)
        {
            Debug.Log($"SetPhotonPlayer {_playerPos} {player.GetDebugLabel()} {_photonView}");
            gameObject.SetActive(player != null);
            if (player == null)
            {
                _actorNumber = 0;
                _handle = 0;
                _connectInfo.SetPlayer(null);
                return;
            }
            _actorNumber = player.ActorNumber;
            var localPlayer = PhotonNetwork.LocalPlayer;
            _handle = (short)(10 * localPlayer.ActorNumber + _playerPos);
            _connectInfo.SetPlayer(player);

            _connectInfo.UpdatePlayerHandle(_photonView.Controller, _handle);
            //_photonView.RPC(nameof(UpdatePlayerHandleRpc), RpcTarget.Others, _handle);

            //_playerHandshake = gameObject.GetOrAddComponent<PlayerHandshake>();
            //_photonView.TransferOwnership(player);
        }

        [PunRPC]
        private void UpdatePlayerHandleRpc(short handle)
        {
            _handle = handle;
            Debug.Log($"UpdatePlayerHandleRpc: {_photonView.Controller.GetDebugLabel()} {handle}");
            _connectInfo.UpdatePlayerHandle(_photonView.Controller, handle);
        }

        public void UpdatePhotonPlayer(Player player)
        {
            Debug.Log($"UpdatePlayer {_playerPos} {player.GetDebugLabel()}");
            Assert.IsTrue(player.ActorNumber == _actorNumber, "player is not same");
            _connectInfo.UpdatePlayer(player);
        }
    }
}
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
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private int _actorNumber;
        [SerializeField] private short _handle;
        [SerializeField] private PlayerHandshake _playerHandshake;

        public int PlayerPos => _playerPos;
        public int ActorNumber => _actorNumber;

        public override void OnEnable()
        {
            base.OnEnable();
            _photonView = PhotonView.Get(this);
            _renderer = GetComponent<MeshRenderer>();
            Debug.Log($"OnEnable pp={_playerPos} {PhotonNetwork.NetworkClientState} {_photonView}");
        }

        public override void OnDisable()
        {
            base.OnDisable();
            Debug.Log($"OnDisable pp={_playerPos} {PhotonNetwork.NetworkClientState} {_photonView}");
            _photonView = null;
        }

        private void ShowPlayerCube(bool isVisible)
        {
            _renderer.enabled = isVisible;
        }

        public void SetPhotonPlayer(Player player)
        {
            Debug.Log($"SetPhotonPlayer pp={_playerPos} {player.GetDebugLabel()} {_photonView}");
            ShowPlayerCube(player != null);
            if (player == null)
            {
                _actorNumber = 0;
                _handle = 0;
                _connectInfo.ShowPlayer(null, _handle);
                return;
            }
            _actorNumber = player.ActorNumber;
            _handle = (short)(10 * PhotonNetwork.LocalPlayer.ActorNumber + _playerPos);
            _connectInfo.ShowPlayer(player, _handle);

            _photonView.RPC(nameof(SyncPlayerHandleRpc), RpcTarget.Others, _handle);

            //_playerHandshake = gameObject.GetOrAddComponent<PlayerHandshake>();
            //_photonView.TransferOwnership(player);
        }

        [PunRPC]
        private void SyncPlayerHandleRpc(short handle)
        {
            Debug.Log($"SyncPlayerHandleRpc pp={_playerPos} _actorNumber {_actorNumber} _handle {_handle} <- {handle}");

            //_connectInfo.UpdatePlayerHandle(_photonView.Controller, handle);
        }

        public void UpdatePhotonPlayer(Player player)
        {
            Debug.Log($"UpdatePlayer pp={_playerPos} {player.GetDebugLabel()} _handle {_handle}");
            Assert.IsTrue(player.ActorNumber == _actorNumber, "player is not same");
            _connectInfo.UpdatePlayer(player, _handle);
        }
    }
}
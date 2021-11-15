using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;

namespace Examples2.Scripts.Connect
{
    public class PlayerConnection : MonoBehaviourPunCallbacks
    {
        private const byte OperationAdd = 1;
        private const byte OperationRemove = 2;
        private static string[] OpNames = { "", "ADD", "REM"};

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

        public void ShowPhotonPlayer(Player player)
        {
            Debug.Log($"ShowPhotonPlayer pp={_playerPos} {player.GetDebugLabel()} {_photonView}");
            ShowPlayerCube(true);
            _actorNumber = player.ActorNumber;
            _handle = (short)(10 * PhotonNetwork.LocalPlayer.ActorNumber + _playerPos);
            _connectInfo.ShowPlayer(player, _handle);

            SendSynchronization();

            //_playerHandshake = gameObject.GetOrAddComponent<PlayerHandshake>();
            //_photonView.TransferOwnership(player);
        }

        private void SendSynchronization()
        {
            Debug.Log($"SyncPlayerHandleRpc SEND pp={_playerPos} _actorNumber {_actorNumber} _handle {_handle} -> {OpNames[OperationAdd]}");
            _photonView.RPC(nameof(SyncPlayerHandleRpc), RpcTarget.Others, OperationAdd, _handle);
        }

        public void HidePhotonPlayer()
        {
            Debug.Log($"HidePhotonPlayer pp={_playerPos} _actorNumber {_actorNumber} _handle");
            ShowPlayerCube(false);
            _actorNumber = 0;
            _handle = 0;
            _connectInfo.HidePlayer();
            Debug.Log($"SyncPlayerHandleRpc SEND pp={_playerPos} _actorNumber {_actorNumber} _handle {_handle} -> {OpNames[OperationRemove]}");
            _photonView.RPC(nameof(SyncPlayerHandleRpc), RpcTarget.Others, OperationRemove, _handle);
        }

        [PunRPC]
        private void SyncPlayerHandleRpc(byte operation, short handle)
        {
            Debug.Log($"SyncPlayerHandleRpc RECV pp={_playerPos} _actorNumber {_actorNumber} _handle {_handle} <- {OpNames[operation]} {handle}");
            switch (operation)
            {
                case OperationAdd:
                    _connectInfo.AddRemoteHandle(handle);
                    break;
                case OperationRemove:
                    _connectInfo.RemoveRemoteHandle(handle);
                    break;
                default:
                    throw new UnityException($"invalid operation: {operation}");
            }
            if (PhotonNetwork.LocalPlayer.ActorNumber == _actorNumber)
            {
                SendSynchronization();
            }
        }

        public void UpdatePhotonPlayer(Player player)
        {
            Debug.Log($"UpdatePlayer pp={_playerPos} {player.GetDebugLabel()} _handle {_handle}");
            Assert.IsTrue(player.ActorNumber == _actorNumber, "player is not same");
            _connectInfo.UpdatePlayer(player, _handle);
        }
    }
}
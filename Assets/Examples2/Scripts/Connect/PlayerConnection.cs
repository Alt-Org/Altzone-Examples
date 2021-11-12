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
        [SerializeField] private int _instanceId;
        [SerializeField] private short _handle;
        [SerializeField] private PlayerHandshake _playerHandshake;

        private Player _player;

        public int PlayerPos => _playerPos;
        public bool HasPlayer => _player != null;
        public Player Player => _player;

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
            Debug.Log($"SetPlayer {_playerPos} {player.GetDebugLabel()} {_photonView}");
            _player = player;
            gameObject.SetActive(player != null);
            if (player == null)
            {
                return;
            }
            var localPlayer = PhotonNetwork.LocalPlayer;
            _instanceId = localPlayer.ActorNumber;
            _handle = (short)(1000 * _playerPos + _instanceId);
            _playerHandshake = gameObject.GetOrAddComponent<PlayerHandshake>();
            _connectInfo.SetPlayer(player);

            //_photonView.TransferOwnership(player);
        }

        public void UpdatePhotonPlayer(Player player)
        {
            Debug.Log($"UpdatePlayer {_playerPos} {player.GetDebugLabel()}");
            Assert.IsTrue(_player.Equals(player), "player is not same");
            _connectInfo.UpdatePlayer(player);
        }
    }
}
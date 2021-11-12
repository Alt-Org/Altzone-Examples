using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;

namespace Examples2.Scripts.Connect
{
    public class PlayerConnection : MonoBehaviourPunCallbacks
    {
        [Header("Settings"), SerializeField] private ConnectInfo _connectInfo;

        [Header("Live Data"), SerializeField] private PhotonView _photonView;
        [SerializeField] private int _playerId;
        [SerializeField] private PlayerHandshake _playerHandshake;

        private Player _player;

        public Player Player => _player;
        public bool HasPlayer => _player != null;

        public override void OnEnable()
        {
            base.OnEnable();
            _photonView = PhotonView.Get(this);
            Debug.Log($"OnEnable {_playerId} {PhotonNetwork.NetworkClientState} {_photonView}");
        }

        public override void OnDisable()
        {
            base.OnDisable();
            Debug.Log($"OnDisable {_playerId} {PhotonNetwork.NetworkClientState} {_photonView}");
            _photonView = null;
        }

        public void SetPhotonPlayer(Player player)
        {
            Debug.Log($"SetPlayer {_playerId} {player.GetDebugLabel()} {_photonView}");
            _player = player;
            gameObject.SetActive(player != null);
            if (player == null)
            {
                return;
            }
            _playerHandshake = gameObject.GetOrAddComponent<PlayerHandshake>();
            _playerHandshake.SetPlayerId(_playerId, _connectInfo);
            _connectInfo.SetPlayer(player, _playerId, _playerHandshake.InstanceId);

            //_photonView.TransferOwnership(player);
        }

        public void UpdatePhotonPlayer(Player player)
        {
            Debug.Log($"UpdatePlayer {_playerId} {player.GetDebugLabel()}");
            Assert.IsTrue(_player.Equals(player), "player is not same");
            _connectInfo.UpdatePlayer(player);
        }
    }
}
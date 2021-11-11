using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;

namespace Examples2.Scripts.Connect
{
    public class PlayerConnection : MonoBehaviourPunCallbacks
    {
        [SerializeField] private ConnectInfo _connectInfo;
        [SerializeField] private PhotonView _photonView;
        [SerializeField] private int _playerId;
        [SerializeField] private PlayerHandshake _playerHandshake;

        private Player _player;

        public Player Player => _player;
        public bool HasPlayer => _player != null;

        private void Awake()
        {
            _photonView = PhotonView.Get(this);
            Debug.Log($"Awake {_playerId} {PhotonNetwork.NetworkClientState}");
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Debug.Log($"OnEnable {_playerId} {PhotonNetwork.NetworkClientState}");
        }

        public void SetConnectTexts(ConnectInfo connectInfo)
        {
            _connectInfo = connectInfo;
        }

        public void SetPlayer(Player player)
        {
            Debug.Log($"SetPlayer {_playerId} {player.GetDebugLabel()}");
            _player = player;
            _connectInfo.SetPlayer(player, _playerId);
            gameObject.SetActive(player != null);
            if (player == null)
            {
                Destroy(_playerHandshake);
                return;
            }
            _photonView.TransferOwnership(player);
            _playerHandshake = gameObject.AddComponent<PlayerHandshake>();
            _playerHandshake.SetPlayerId(_playerId, _connectInfo);
        }

        public void UpdatePlayer(Player player)
        {
            Debug.Log($"UpdatePlayer {_playerId} {player.GetDebugLabel()}");
            Assert.IsTrue(_player.Equals(player), "player is not same");
            _connectInfo.UpdatePlayer(player);
        }
    }
}
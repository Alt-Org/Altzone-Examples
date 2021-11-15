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
        [SerializeField] private PlayerHandshake _playerHandshake;
        [SerializeField] private int _actorNumber;

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
            if (_playerHandshake != null)
            {
                _playerHandshake.enabled = isVisible;
            }
        }

        public void UpdatePeers(PlayerHandshakeState state)
        {
            Debug.Log($"UpdatePeers pp={_playerPos} actor={_actorNumber} state {state}");
            _connectInfo.UpdatePeers(state);
        }

        public void ShowPhotonPlayer(Player player)
        {
            Debug.Log($"ShowPhotonPlayer pp={_playerPos} {player.GetDebugLabel()} {_photonView}");
            _actorNumber = player.ActorNumber;
            _playerHandshake = gameObject.GetOrAddComponent<PlayerHandshake>();
            ShowPlayerCube(true);
            _connectInfo.ShowPlayer(player);
        }

        public void HidePhotonPlayer()
        {
            Debug.Log($"HidePhotonPlayer pp={_playerPos} _actorNumber {_actorNumber}");
            ShowPlayerCube(false);
            _connectInfo.HidePlayer();
            _actorNumber = 0;
        }

        public void UpdatePhotonPlayer(Player player)
        {
            Debug.Log($"UpdatePlayer pp={_playerPos} {player.GetDebugLabel()}");
            Assert.IsTrue(player.ActorNumber == _actorNumber, "player is not same");
            _connectInfo.UpdatePlayer(player);
        }
    }
}
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Examples2.Scripts.Connect
{
    public class PlayerConnection : MonoBehaviour
    {
        [SerializeField] private ConnectInfo _connectInfo;
        [SerializeField] private PhotonView _photonView;

        private Player _player;

        public Player Player => _player;
        public bool HasPlayer => _player != null;

        private void Awake()
        {
            _photonView = PhotonView.Get(this);
            Debug.Log($"Awake: {PhotonNetwork.NetworkClientState}");
        }

        private void OnEnable()
        {
            Debug.Log($"OnEnable: {PhotonNetwork.NetworkClientState}");
        }

        public void SetConnectTexts(ConnectInfo connectInfo)
        {
            _connectInfo = connectInfo;
        }

        public void SetPlayer(Player player)
        {
            Debug.Log($"SetPlayer {player.GetDebugLabel()}");
            _player = player;
            _connectInfo.SetPlayer(player);
            gameObject.SetActive(player != null);
        }
    }
}
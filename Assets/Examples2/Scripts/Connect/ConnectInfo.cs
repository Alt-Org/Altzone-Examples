using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace Examples2.Scripts.Connect
{
    public class ConnectInfo : MonoBehaviour
    {
        private const string NoName = "---";

        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _playerName;
        [SerializeField] private TMP_Text _localStatus;
        [SerializeField] private TMP_Text _remotePlayer1;
        [SerializeField] private TMP_Text _remotePlayer2;
        [SerializeField] private TMP_Text _remotePlayer3;
        [SerializeField] private TMP_Text _remotePlayer4;

        private int _playerId;
        private int _instanceId;

        public void Reset()
        {
            _title.text = string.Empty;
            _playerName.text = NoName;
            _localStatus.text = string.Empty;
            _remotePlayer1.text = string.Empty;
            _remotePlayer2.text = string.Empty;
            _remotePlayer3.text = string.Empty;
            _remotePlayer4.text = string.Empty;
        }

        public void SetPlayer(Player player, int playerId, int instanceId)
        {
            Debug.Log($"SetPlayer {player.GetDebugLabel()}");
            Reset();
            if (player == null)
            {
                _playerId = 0;
                return;
            }
            _playerId = playerId;
            _instanceId = instanceId;
            UpdatePlayer(player);
        }

        public void UpdatePlayer(Player player)
        {
            _playerName.text = player.NickName;
            var master = player.IsMasterClient ? " [M]" : "";
            var local = player.IsLocal ? " [L]" : "";
            _title.text = $"connected:p={_playerId}:i={_instanceId}";
            _localStatus.text = $"#{player.ActorNumber}{master}{local}";
        }
    }
}
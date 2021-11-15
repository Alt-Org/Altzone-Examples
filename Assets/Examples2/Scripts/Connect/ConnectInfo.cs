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

        public void ShowPlayer(Player player, short handle)
        {
            Debug.Log($"ShowPlayer {player.GetDebugLabel()}");
            Reset();
            if (player == null)
            {
                return;
            }
            UpdatePlayer(player, handle);
        }

        public void UpdatePlayer(Player player, short handle)
        {
            var master = player.IsMasterClient ? " [M]" : "";
            var local = player.IsLocal ? " [L]" : "";
            var playerPos = player.GetCustomProperty<byte>(PhotonKeyNames.PlayerPosition, 0);
            _playerName.text = player.IsLocal ? RichText.Yellow(player.NickName) : player.NickName;
            _title.text = handle > 0 ? $"handle {handle}" : "connected";
            _localStatus.text = $"#{player.ActorNumber}:{playerPos}{master}{local}";
        }
    }
}
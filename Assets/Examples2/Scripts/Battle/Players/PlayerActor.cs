using Examples2.Scripts.Battle.Room;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Examples2.Scripts.Battle.Players
{
    public class PlayerActor : MonoBehaviour
    {
        [Header("Debug"), SerializeField] private TMP_Text _playerInfo;

        private PhotonView _photonView;

        private void Awake()
        {
            _photonView = PhotonView.Get(this);
            var player = _photonView.Owner;
            var playerPos = PhotonBattle.GetPlayerPos(player);
            var teamIndex = PhotonBattle.GetTeamIndex(playerPos);
            name = $"{(player.IsLocal ? "L" : "R")}{playerPos}:{teamIndex}:{player.NickName}";
            _playerInfo = GetComponentInChildren<TMP_Text>();
            _playerInfo.text = playerPos.ToString("N0");
            Debug.Log($"Awake {name}");
        }

        private void OnEnable()
        {
            Debug.Log($"OnEnable {name} IsMine {_photonView.IsMine} IsMasterClient {_photonView.Owner.IsMasterClient}");
        }
    }
}
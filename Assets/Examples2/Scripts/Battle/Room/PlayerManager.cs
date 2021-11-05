using Examples2.Scripts.Battle.Photon;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Examples2.Scripts.Battle.Room
{
    /// <summary>
    /// Manages players initial creation and gameplay start.
    /// </summary>
    public class PlayerManager : MonoBehaviour
    {
        [SerializeField] private GameObject _playerPrefab;

        private void OnEnable()
        {
            var player = PhotonNetwork.LocalPlayer;
            if (!PhotonBattle.IsRealPlayer(player))
            {
                Debug.Log($"OnEnable SKIP player {player.GetDebugLabel()}");
                return;
            }
            var playerPos = PhotonBattle.GetPlayerPos(player);
            var x = playerPos == 1 || playerPos == 2 ? 2.5f : -2.5f;
            var y = playerPos == 1 || playerPos == 3 ? 4.25f : -4.25f;
            var instantiationPosition = new Vector3(x, y);
            Debug.Log($"OnEnable create player {player.GetDebugLabel()} @ {instantiationPosition} from {_playerPrefab.name}");
            PhotonNetwork.Instantiate(_playerPrefab.name, instantiationPosition, Quaternion.identity);
        }
    }
}
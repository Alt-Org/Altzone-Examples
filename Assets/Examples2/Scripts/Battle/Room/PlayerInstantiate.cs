using Altzone.Scripts.Battle;
using Photon.Pun;
using UnityEngine;

namespace Examples2.Scripts.Battle.Room
{
    /// <summary>
    /// Instantiate local Photon player in correct position.
    /// </summary>
    public class PlayerInstantiate : MonoBehaviour
    {
        /// <summary>
        /// Safe heaven for players that have left or disconnected the game.
        /// </summary>
        /// <remarks>
        /// This is quite hackish way to do this!
        /// </remarks>
        public static Transform DetachedPlayerTransform { get; private set; }

        private const int PlayerPosition1 = PhotonBattle.PlayerPosition1;
        private const int PlayerPosition2 = PhotonBattle.PlayerPosition2;
        private const int PlayerPosition3 = PhotonBattle.PlayerPosition3;
        private const int PlayerPosition4 = PhotonBattle.PlayerPosition4;

        [Header("Settings"), SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Vector2 _playerPosition1;
        [SerializeField] private Vector2 _playerPosition2;
        [SerializeField] private Vector2 _playerPosition3;
        [SerializeField] private Vector2 _playerPosition4;

        private void OnEnable()
        {
            DetachedPlayerTransform = transform;
            var player = PhotonNetwork.LocalPlayer;
            if (!PhotonBattle.IsRealPlayer(player))
            {
                Debug.Log($"OnEnable SKIP player {player.GetDebugLabel()}");
                return;
            }
            var playerPos = PhotonBattle.GetPlayerPos(player);
            Vector2 pos;
            switch (playerPos)
            {
                case PlayerPosition1:
                    pos = _playerPosition1;
                    break;
                case PlayerPosition2:
                    pos = _playerPosition2;
                    break;
                case PlayerPosition3:
                    pos = _playerPosition3;
                    break;
                case PlayerPosition4:
                    pos = _playerPosition4;
                    break;
                default:
                    throw new UnityException($"invalid playerPos: {playerPos}");
            }
            var instantiationPosition = new Vector3(pos.x, pos.y);
            Debug.Log($"OnEnable create player {player.GetDebugLabel()} @ {instantiationPosition} from {_playerPrefab.name}");
            PhotonNetwork.Instantiate(_playerPrefab.name, instantiationPosition, Quaternion.identity);
        }
    }
}
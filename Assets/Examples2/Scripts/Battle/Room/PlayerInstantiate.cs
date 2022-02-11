using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Photon.Pun;
using UnityEngine;

namespace Examples2.Scripts.Battle.Room
{
    /// <summary>
    /// Instantiate local Photon player in correct position.
    /// </summary>
    public class PlayerInstantiate : MonoBehaviour
    {
        private const int PlayerPosition1 = PhotonBattle.PlayerPosition1;
        private const int PlayerPosition2 = PhotonBattle.PlayerPosition2;
        private const int PlayerPosition3 = PhotonBattle.PlayerPosition3;
        private const int PlayerPosition4 = PhotonBattle.PlayerPosition4;

        [Header("Player Settings"), SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Vector2 _playerPosition1;
        [SerializeField] private Vector2 _playerPosition2;
        [SerializeField] private Vector2 _playerPosition3;
        [SerializeField] private Vector2 _playerPosition4;

        [Header("Level Settings"), SerializeField] private Camera _gameCamera;
        [SerializeField] private GameObject _gameBackground;

        private void OnEnable()
        {
            var player = PhotonNetwork.LocalPlayer;
            if (!PhotonBattle.IsRealPlayer(player))
            {
                Debug.Log($"OnEnable SKIP player {player.GetDebugLabel()}");
                return;
            }
            var playerPos = PhotonBattle.GetPlayerPos(player);
            var teamNumber = PhotonBattle.GetTeamNumber(playerPos);
            if (teamNumber == PhotonBattle.TeamRedValue)
            {
                RotateLocalPlayer(_gameCamera, _gameBackground);
            }
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

        private static void RotateLocalPlayer(Camera gameCamera, GameObject gameBackground)
        {
            void RotateGameCamera(bool upsideDown)
            {
                // Rotate game camera for team red.
                Debug.Log($"RotateGameCamera upsideDown {upsideDown}");
                var cameraTransform = gameCamera.transform;
                var rotation = upsideDown
                    ? Quaternion.Euler(0f, 0f, 180f) // Upside down
                    : Quaternion.Euler(0f, 0f, 0f); // Normal orientation
                cameraTransform.rotation = rotation;
            }

            void RotateBackground(bool upsideDown)
            {
                Debug.Log($"RotateBackground upsideDown {upsideDown}");
                var rotation = upsideDown
                    ? Quaternion.Euler(0f, 0f, 180f) // Upside down
                    : Quaternion.Euler(0f, 0f, 0f); // Normal orientation
                gameBackground.transform.rotation = rotation;
            }

            var features = RuntimeGameConfig.Get().Features;
            if (features._isRotateGameCamera && gameCamera != null)
            {
                // Rotate game camera.
                RotateGameCamera(true);
            }
            if (features._isRotateGamePlayArea && gameBackground != null)
            {
                // Rotate background.
                RotateBackground(true);
                // Separate sprites for each team gameplay area - these might not be visible in final game
                // - see Battle.Scripts.Room.RoomSetup.SetupLocalPlayer() how this is done in Altzone project.
            }
        }
    }
}
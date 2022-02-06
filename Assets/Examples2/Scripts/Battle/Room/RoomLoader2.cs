using System.Collections;
using System.Linq;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Model;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using TMPro;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Examples2.Scripts.Battle.Room
{
    /// <summary>
    /// Game room loader to establish a well known state for the room before actual gameplay starts.
    /// </summary>
    /// <remarks>
    /// Can create a test room environment for more than one player.
    /// </remarks>
    public class RoomLoader2 : MonoBehaviourPunCallbacks
    {
        private const string Tooltip1 = "If Is Offline Mode only one player can play";
        private const string Tooltip2 = "if > 1 Debug Player Pos is automatic";

        [Header("Settings"), SerializeField, Tooltip(Tooltip1)] private bool _isOfflineMode;
        [SerializeField, Range(1, 4)] private int _debugPlayerPos = 1;
        [SerializeField, Range(1, 4), Tooltip(Tooltip2)] private int _minPlayersToStart = 1;
        [SerializeField] private GameObject[] _objectsToActivate;
        [SerializeField] private TMP_Text _roomInfoText;

        [Header("Live Data"), SerializeField] private int _currentPlayersInRoom;

        private void Awake()
        {
            Debug.Log($"Awake {PhotonNetwork.NetworkClientState}");
            if (_objectsToActivate.Any(x => x.activeSelf))
            {
                Debug.LogError("objectsToActivate has active items, disable them and retry");
                enabled = false;
                return;
            }
            if (PhotonNetwork.InRoom)
            {
                // Normal logic is that we are in a room and just do what we must do and continue.
                ContinueToNextStage();
                enabled = false;
                return;
            }
            if (_minPlayersToStart > 1)
            {
                _roomInfoText.text = $"Waiting for {_minPlayersToStart} players";
            }
            else
            {
                _roomInfoText.enabled = false;
            }
            Debug.Log($"Awake and create test room {PhotonNetwork.NetworkClientState}");
        }

        public override void OnEnable()
        {
            // Create a test room - in offline (faster to create) or online (real thing) mode
            base.OnEnable();
            var state = PhotonNetwork.NetworkClientState;
            var isStateValid = state == ClientState.PeerCreated || state == ClientState.Disconnected;
            if (!isStateValid)
            {
                throw new UnityException($"OnEnable: invalid connection state {PhotonNetwork.NetworkClientState}");
            }
            var playerName = PhotonBattle.GetLocalPlayerName();
            Debug.Log($"connect {PhotonNetwork.NetworkClientState} isOfflineMode {_isOfflineMode} player {playerName}");
            PhotonNetwork.OfflineMode = _isOfflineMode;
            if (_isOfflineMode)
            {
                // JoinRandomRoom -> CreateRoom -> OnJoinedRoom -> WaitForPlayersToArrive -> ContinueToNextStage
                _minPlayersToStart = 1;
                PhotonNetwork.NickName = playerName;
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                // Connect -> JoinLobby -> CreateRoom -> OnJoinedRoom -> WaitForPlayersToArrive -> ContinueToNextStage
                PhotonLobby.Connect(playerName);
            }
        }

        private void ContinueToNextStage()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // Mark room "closed"
                PhotonLobby.CloseRoom(true);
            }
            // Enable game objects when this room stage is ready to play
            StartCoroutine(ActivateObjects(_objectsToActivate));
        }

        private static IEnumerator ActivateObjects(GameObject[] objectsToActivate)
        {
            // Enable game objects one per frame in array sequence
            for (var i = 0; i < objectsToActivate.LongLength; i++)
            {
                yield return null;
                objectsToActivate[i].SetActive(true);
            }
        }

        public override void OnConnectedToMaster()
        {
            if (!_isOfflineMode)
            {
                Debug.Log($"joinLobby {PhotonNetwork.NetworkClientState}");
                PhotonLobby.JoinLobby();
            }
        }

        public override void OnJoinedLobby()
        {
            Debug.Log($"createRoom {PhotonNetwork.NetworkClientState}");
            PhotonLobby.JoinOrCreateRoom("testing2");
        }

        public override void OnJoinedRoom()
        {
            if (_minPlayersToStart > 1)
            {
                var room = PhotonNetwork.CurrentRoom;
                switch (room.PlayerCount)
                {
                    case 1:
                        _debugPlayerPos = 1;
                        break;
                    case 2:
                        _debugPlayerPos = 3;
                        break;
                    case 3:
                        _debugPlayerPos = 2;
                        break;
                    default:
                        _debugPlayerPos = 4;
                        break;
                }
            }
            var player = PhotonNetwork.LocalPlayer;
            var playerMainSkill = (int)Defence.Deflection;
            player.SetCustomProperties(new Hashtable
            {
                { PhotonBattle.PlayerPositionKey, _debugPlayerPos },
                { PhotonBattle.PlayerMainSkillKey, playerMainSkill }
            });
            Debug.Log($"OnJoinedRoom {player.GetDebugLabel()}");
            StartCoroutine(WaitForPlayersToArrive());
        }

        private IEnumerator WaitForPlayersToArrive()
        {
            int CountPlayersInRoom()
            {
                _currentPlayersInRoom = PhotonBattle.CountRealPlayers();
                _roomInfoText.text = $"Waiting for {_minPlayersToStart - _currentPlayersInRoom} players";
                return _currentPlayersInRoom;
            }

            StartCoroutine(Blink(_roomInfoText, 0.6f, 0.3f));
            yield return new WaitUntil(() => PhotonNetwork.InRoom && CountPlayersInRoom() >= _minPlayersToStart);
            _roomInfoText.text = string.Empty;
            ContinueToNextStage();
        }

        private static IEnumerator Blink(Behaviour component, float visibleDuration, float hiddenDuration)
        {
            var delay1 = new WaitForSeconds(visibleDuration);
            var delay2 = new WaitForSeconds(hiddenDuration);
            for (;;)
            {
                yield return delay1;
                component.enabled = false;
                yield return delay2;
                component.enabled = true;
            }
        }
    }
}
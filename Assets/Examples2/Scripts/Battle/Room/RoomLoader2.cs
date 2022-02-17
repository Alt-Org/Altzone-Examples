using System;
using System.Collections;
using System.Linq;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Model;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Examples2.Scripts.Battle.Room
{
    /// <summary>
    /// Game room loader to establish a well known state for the room before actual gameplay starts.
    /// </summary>
    /// <remarks>
    /// Can create a test room environment for more than one player.
    /// </remarks>
    internal class RoomLoader2 : MonoBehaviourPunCallbacks
    {
        private const string Tooltip1 = "If Is Offline Mode only one player can play";
        private const string Tooltip2 = "if > 1 Debug Player Pos is automatic";

        [Header("Settings"), SerializeField, Tooltip(Tooltip1)] private bool _isOfflineMode;
        [SerializeField, Range(1, 4)] private int _debugPlayerPos = 1;
        [SerializeField, Range(1, 4), Tooltip(Tooltip2)] private int _minPlayersToStart = 1;
        [SerializeField] private bool _isFillTeamBlueFirst;
        [SerializeField] private GameObject[] _objectsToActivate;

        [Header("UI Settings"), SerializeField] private UISettings _uiSettings;

        [Header("Live Data"), SerializeField] private int _currentPlayersInRoom;

        private RoomLoaderUi _ui;

        private void Awake()
        {
            Debug.Log($"Awake {PhotonNetwork.NetworkClientState}");
            if (_objectsToActivate.Any(x => x.activeSelf))
            {
                Debug.LogError("objectsToActivate has active items, disable them and retry");
                enabled = false;
                return;
            }
            _ui = new RoomLoaderUi(_uiSettings);
            if (PhotonNetwork.InRoom)
            {
                // Normal logic is that we are in a room and just do what we must do and continue.
                ContinueToNextStage();
                enabled = false;
                return;
            }
            if (_minPlayersToStart > 1)
            {
                _ui.Show();
                _ui.SetWaitText(_minPlayersToStart);
                _ui.SetOnPlayClick(() => { _minPlayersToStart = 1; });
            }
            else
            {
                _ui.Hide();
            }
            Debug.Log($"Awake and create test room {PhotonNetwork.NetworkClientState}");
        }

        public override void OnEnable()
        {
            // Create a test room - in offline (faster to create) or online (real thing) mode
            base.OnEnable();
            var state = PhotonNetwork.NetworkClientState;
            var isStateValid = state == ClientState.PeerCreated || state == ClientState.Disconnected || state == ClientState.ConnectedToMasterServer;
            if (!isStateValid)
            {
                throw new UnityException($"OnEnable: invalid connection state {PhotonNetwork.NetworkClientState}");
            }
            var playerName = PhotonBattle.GetLocalPlayerName();
            Debug.Log($"connect {PhotonNetwork.NetworkClientState} isOfflineMode {_isOfflineMode} player {playerName}");
            PhotonNetwork.OfflineMode = _isOfflineMode;
            if (_isOfflineMode)
            {
                // JoinRandomRoom -> OnJoinedRoom -> WaitForPlayersToArrive -> ContinueToNextStage
                _minPlayersToStart = 1;
                PhotonNetwork.NickName = playerName;
                PhotonNetwork.JoinRandomRoom();
            }
            else if (state == ClientState.ConnectedToMasterServer)
            {
                // JoinLobby -> JoinOrCreateRoom -> OnJoinedRoom -> WaitForPlayersToArrive -> ContinueToNextStage
                PhotonNetwork.NickName = playerName;
                PhotonLobby.JoinLobby();
            }
            else
            {
                // Connect -> JoinLobby -> JoinOrCreateRoom -> OnJoinedRoom -> WaitForPlayersToArrive -> ContinueToNextStage
                PhotonLobby.Connect(playerName);
            }
        }

        private void ContinueToNextStage()
        {
            StopAllCoroutines();
            _ui.Hide();
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
            var room = PhotonNetwork.CurrentRoom;
            if (_minPlayersToStart > 1 || room.PlayerCount > 1)
            {
                if (_isFillTeamBlueFirst)
                {
                    // Shield visibility testing needs two players on one team.
                    _debugPlayerPos = room.PlayerCount;
                }
                else
                {
                    // Distribute players evenly on both teams.
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
            }
            var player = PhotonNetwork.LocalPlayer;
            PhotonNetwork.NickName = room.GetUniquePlayerNameForRoom(player, PhotonNetwork.NickName, "");
            var playerMainSkill = (int)Defence.Deflection;
            player.SetCustomProperties(new Hashtable
            {
                { PhotonBattle.PlayerPositionKey, _debugPlayerPos },
                { PhotonBattle.PlayerMainSkillKey, playerMainSkill }
            });
            Debug.Log($"OnJoinedRoom {player.GetDebugLabel()}");
            StartCoroutine(WaitForPlayersToArrive());
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log($"OnJoinRoomFailed {returnCode} {message}");
            _ui.SetErrorMessage(message);
        }

        private IEnumerator WaitForPlayersToArrive()
        {
            int CountPlayersInRoom()
            {
                _currentPlayersInRoom = PhotonBattle.CountRealPlayers();
                if (_currentPlayersInRoom > 1)
                {
                    _ui.DisableButton();
                }
                _ui.SetWaitText(_minPlayersToStart - _currentPlayersInRoom);
                return _currentPlayersInRoom;
            }

            if (PhotonNetwork.IsMasterClient)
            {
                _ui.EnableButton();
            }
            else
            {
                _ui.HideButton();
            }
            StartCoroutine(_ui.Blink(0.6f, 0.3f));
            yield return new WaitUntil(() => PhotonNetwork.InRoom
                                             && PhotonNetwork.CurrentRoom.IsOpen
                                             && CountPlayersInRoom() >= _minPlayersToStart);
            ContinueToNextStage();
        }

        [Serializable]
        internal class UISettings
        {
            public Canvas _canvas;
            public TMP_Text _roomInfoText;
            public Button _playNowButton;
        }

        private class RoomLoaderUi
        {
            private readonly bool _isValid;
            private readonly Canvas _canvas;
            private readonly TMP_Text _roomInfoText;
            private readonly Button _playNowButton;

            public RoomLoaderUi(UISettings uiSettings)
            {
                _isValid = uiSettings._canvas != null;
                _canvas = uiSettings._canvas;
                _roomInfoText = uiSettings._roomInfoText;
                _playNowButton = uiSettings._playNowButton;
            }

            public void Show()
            {
                if (!_isValid)
                {
                    return;
                }
                _canvas.gameObject.SetActive(true);
                _playNowButton.interactable = false;
            }

            public void Hide()
            {
                if (!_isValid)
                {
                    return;
                }
                _canvas.gameObject.SetActive(false);
            }

            public void EnableButton()
            {
                if (!_isValid)
                {
                    return;
                }
                _playNowButton.interactable = true;
            }

            public void DisableButton()
            {
                if (!_isValid)
                {
                    return;
                }
                _playNowButton.interactable = false;
            }

            public void HideButton()
            {
                if (!_isValid)
                {
                    return;
                }
                _playNowButton.gameObject.SetActive(false);
            }

            public void SetErrorMessage(string message)
            {
                _roomInfoText.text = message;
                _playNowButton.interactable = false;
            }

            public void SetWaitText(int playersToWait)
            {
                if (!_isValid)
                {
                    return;
                }
                _roomInfoText.text = playersToWait == 1
                    ? $"Waiting for {playersToWait} player"
                    : $"Waiting for {playersToWait} players";
            }

            public void SetOnPlayClick(Action callback)
            {
                if (!_isValid)
                {
                    return;
                }
                _playNowButton.onClick.AddListener(() => { callback(); });
            }

            public IEnumerator Blink(float visibleDuration, float hiddenDuration)
            {
                var delay1 = new WaitForSeconds(visibleDuration);
                var delay2 = new WaitForSeconds(hiddenDuration);
                while (_isValid)
                {
                    yield return delay1;
                    _roomInfoText.enabled = false;
                    yield return delay2;
                    // ReSharper disable once Unity.InefficientPropertyAccess
                    _roomInfoText.enabled = true;
                }
            }
        }
    }
}
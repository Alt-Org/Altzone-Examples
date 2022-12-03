using System;
using System.Collections;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.Unity.Window;
using Prg.Scripts.Common.Unity.Window.ScriptableObjects;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace GameUi.Scripts.BattleLobby
{
    /// <summary>
    /// Controller for Photon Lobby.<br />
    /// List current rooms and can create a new room or join on existing room.
    /// </summary>
    public class BattleLobbyController : MonoBehaviourPunCallbacks
    {
        private const float QueryLobbyStatusDelay = 1.0f;
        private const int MaxPlayersInRoom = 4;

        [SerializeField] private WindowDef _roomWindow;
        [SerializeField] private BattleLobbyView _view;
        [SerializeField] private PhotonRoomList _photonRoomList;

        private bool _isJoiningRoom;
        
        private void Awake()
        {
            _photonRoomList = gameObject.GetOrAddComponent<PhotonRoomList>();
            _view.CreateRoomButton.onClick.AddListener(CreateRoom);
            _view.OnJoinRoomClicked = OnJoinRoomClick;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            var playerData = GameConfig.Get().PlayerDataCache;
            Debug.Log($"OnEnable {playerData}");
            _view.ResetView();
            _view.GameInfo = $"Version {PhotonLobby.GameVersion}";
            _view.LobbyInfo = string.Empty;
            _view.CurrentPlayerInfo = playerData.PlayerName;
            _view.RoomListInfo = string.Empty;
            StopAllCoroutines();
            _photonRoomList.OnRoomsUpdated += OnRoomsUpdated;
            StartCoroutine(WaitForLobby());
        }

        public override void OnDisable()
        {
            StopAllCoroutines();
            _photonRoomList.OnRoomsUpdated -= OnRoomsUpdated;
            base.OnDisable();
        }

        private IEnumerator WaitForLobby()
        {
            Debug.Log($"WaitForLobby {PhotonNetwork.NetworkClientState}");
            // Start new Lobby connection every time.
            PhotonNetwork.OfflineMode = false;
            if (PhotonWrapper.InRoom)
            {
                _view.RoomListInfo = "Prepare connection";
                Debug.Log($"WaitForLobby LeaveRoom {PhotonNetwork.NetworkClientState}");
                PhotonLobby.LeaveRoom();
                yield return new WaitUntil(() => PhotonWrapper.CanConnect);
            }
            if (PhotonWrapper.InLobby)
            {
                _view.RoomListInfo = "Prepare connection";
                Debug.Log($"WaitForLobby LeaveLobby {PhotonNetwork.NetworkClientState}");
                PhotonLobby.LeaveLobby();
                yield return new WaitUntil(() => PhotonWrapper.CanConnect);
            }
            for (;;)
            {
                yield return null;
                if (PhotonWrapper.InLobby)
                {
                    _view.EnableButtons();
                    StartCoroutine(QueryLobbyStatus());
                    yield break;
                }
                if (PhotonWrapper.CanJoinLobby)
                {
                    _view.RoomListInfo = "Joining to Lobby";
                    Debug.Log($"WaitForLobby JoinLobby {PhotonNetwork.NetworkClientState}");
                    PhotonLobby.JoinLobby();
                    continue;
                }
                if (PhotonWrapper.CanConnect)
                {
                    _view.RoomListInfo = "Connecting to Lobby";
                    var playerData = GameConfig.Get().PlayerDataCache;
                    Debug.Log($"WaitForLobby Connect {PhotonNetwork.NetworkClientState}");
                    PhotonLobby.Connect(playerData.PlayerName);
                }
            }
        }

        private IEnumerator QueryLobbyStatus()
        {
            var delay = new WaitForSeconds(QueryLobbyStatusDelay);
            for (;;)
            {
                if (!PhotonWrapper.InLobby)
                {
                    yield break;
                }
                _view.LobbyInfo = $"Number of players is {PhotonNetwork.CountOfPlayers}";
                yield return delay;
            }
        }

        private void OnRoomsUpdated()
        {
            Debug.Log($"OnRoomsUpdated InLobby {PhotonWrapper.InLobby}");
            if (!PhotonWrapper.InLobby)
            {
                _view.DisableButtons();
                _view.RoomListInfo = "Disconnected from Lobby";
                return;
            }
            var currentRooms = _photonRoomList.CurrentRooms;
            _view.RoomListInfo = $"Room count is {currentRooms.Count}";
            var isInteractable = !_isJoiningRoom;
            _view.UpdateRoomList(currentRooms, isInteractable);
        }

        private void CreateRoom()
        {
            var roomName = $"Room{DateTime.Now.Second:00}";
            Debug.Log($"CreateRoom {roomName}");
            // For simplicity limit players to 4!
            var roomOptions = new RoomOptions
            {
                MaxPlayers = 4,
            };
            PhotonLobby.CreateRoom(roomName, roomOptions);
            _isJoiningRoom = true;
            _view.DisableButtons();
        }

        private void OnJoinRoomClick(string roomName)
        {
            Debug.Log($"OnJoinRoom {roomName}");
            var rooms = _photonRoomList.CurrentRooms;
            foreach (var roomInfo in rooms)
            {
                if (roomInfo.Name == roomName && !roomInfo.RemovedFromList && roomInfo.IsOpen && roomInfo.PlayerCount < MaxPlayersInRoom)
                {
                    PhotonLobby.JoinRoom(roomInfo);
                    _isJoiningRoom = true;
                    _view.DisableButtons();
                }
            }
        }

        public override void OnJoinedRoom()
        {
            var room = PhotonNetwork.CurrentRoom;
            var player = PhotonNetwork.LocalPlayer;
            var playerData = GameConfig.Get().PlayerDataCache;
            PhotonNetwork.NickName = room.GetUniquePlayerNameForRoom(player, PhotonNetwork.NickName, "");
            Debug.Log($"OnJoinedRoom InRoom '{room.Name}' as '{PhotonNetwork.NickName}'");

            // Simplified setup for room.
            if (player.IsMasterClient)
            {
                room.SetCustomProperties(new Hashtable
                {
                    { PhotonBattle.TeamBlueKey, "Alpha" },
                    { PhotonBattle.TeamRedKey, "Beta" }
                });
            }
            var positions = new[]
                { PhotonBattle.PlayerPosition1, PhotonBattle.PlayerPosition2, PhotonBattle.PlayerPosition3, PhotonBattle.PlayerPosition4 };
            var playerPosition = positions[room.PlayerCount - 1];
            if (playerPosition >= positions.Length)
            {
                // Room is full - current implementation can not handle this and UI will be "stuck"
                // - if we have proper staging area for the room then this logic will be different and not here.
                PhotonNetwork.LeaveRoom();
                return;
            }
            player.SetCustomProperties(new Hashtable
            {
                { PhotonBattle.PlayerPositionKey, playerPosition },
                { PhotonBattle.PlayerMainSkillKey, 1 }
            });

            WindowManager.Get().ShowWindow(_roomWindow);
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            _isJoiningRoom = false;
            _view.EnableButtons();
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            _view.EnableButtons();
        }
    }
}
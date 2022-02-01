using System.Collections;
using Altzone.Scripts.Config;
using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace GameUi.Scripts.BattleLobby
{
    public class BattleLobbyController : MonoBehaviour
    {
        [SerializeField] private BattleLobbyView _view;
        [SerializeField] private PhotonRoomList _photonRoomList;

        private void Awake()
        {
            _photonRoomList = gameObject.GetOrAddComponent<PhotonRoomList>();
            _view.CreateRoomButton.onClick.AddListener(CreateRoom);
            _view.OnJoinRoom = OnJoinRoom;
        }

        private void OnEnable()
        {
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            Debug.Log($"OnEnable {playerData}");
            _view.ResetView();
            _view.CurrentPlayerInfo = playerData.GetPlayerInfoLabel();
            _view.RoomListInfo = string.Empty;
            StopAllCoroutines();
            _photonRoomList.OnRoomsUpdated += OnRoomsUpdated;
            StartCoroutine(WaitForLobby());
        }

        private void OnDisable()
        {
            _photonRoomList.OnRoomsUpdated -= OnRoomsUpdated;
        }

        private IEnumerator WaitForLobby()
        {
            // Start new Lobby connection every time.
            PhotonLobby.OfflineMode = false;
            if (PhotonWrapper.InRoom)
            {
                _view.RoomListInfo = "Prepare connection";
                PhotonLobby.LeaveRoom();
                yield return new WaitUntil(() => PhotonWrapper.CanConnect);
            }
            if (PhotonWrapper.InLobby)
            {
                _view.RoomListInfo = "Prepare connection";
                PhotonLobby.LeaveLobby();
                yield return new WaitUntil(() => PhotonWrapper.CanConnect);
            }
            for (;;)
            {
                yield return null;
                if (PhotonWrapper.InLobby)
                {
                    _view.CreateRoomButton.interactable = true;
                    yield break;
                }
                if (PhotonWrapper.CanJoinLobby)
                {
                    _view.RoomListInfo = "Joining to Lobby";
                    PhotonLobby.JoinLobby();
                    continue;
                }
                if (PhotonWrapper.CanConnect)
                {
                    _view.RoomListInfo = "Connecting to Lobby";
                    var playerData = RuntimeGameConfig.Get().PlayerDataCache;
                    PhotonLobby.Connect(playerData.PlayerName);
                }
            }
        }

        private void OnRoomsUpdated()
        {
            Debug.Log($"OnRoomsUpdated InLobby {PhotonWrapper.InLobby}");
            if (!PhotonWrapper.InLobby)
            {
                _view.CreateRoomButton.interactable = false;
                _view.RoomListInfo = "Disconnected from Lobby";
                return;
            }
            _view.RoomListInfo = "Room listing updated";
        }

        private void CreateRoom()
        {
            Debug.Log($"CreateRoom");
        }

        private void OnJoinRoom(string roomName)
        {
            Debug.Log($"OnJoinRoom {roomName}");
        }
    }
}
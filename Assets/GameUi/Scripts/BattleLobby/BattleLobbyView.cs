using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace GameUi.Scripts.BattleLobby
{
    public class BattleLobbyView : MonoBehaviour
    {
        [SerializeField] private Text _gameInfo;
        [SerializeField] private Text _lobbyInfo;
        [SerializeField] private Text _currentPlayerInfo;
        [SerializeField] private Text _roomListInfo;
        [SerializeField] private Button _createRoomButton;
        [SerializeField] private Button _joinRoomButtonTemplate;
        [SerializeField] private Transform _viewportContent;

        private List<RoomInfo> _currentRooms;

        private void Awake()
        {
            _currentRooms = new List<RoomInfo>();
        }

        public string GameInfo
        {
            get => _gameInfo.text;
            set => _gameInfo.text = value;
        }

        public string LobbyInfo
        {
            get => _lobbyInfo.text;
            set => _lobbyInfo.text = value;
        }

        public string CurrentPlayerInfo
        {
            get => _currentPlayerInfo.text;
            set => _currentPlayerInfo.text = value;
        }

        public string RoomListInfo
        {
            get => _roomListInfo.text;
            set => _roomListInfo.text = value;
        }

        public Button CreateRoomButton => _createRoomButton;

        public Action<string> OnJoinRoom { get; set; }

        public void ResetView()
        {
            _gameInfo.text = string.Empty;
            _lobbyInfo.text = string.Empty;
            _currentPlayerInfo.text = string.Empty;
            _createRoomButton.interactable = false;
            _roomListInfo.text = string.Empty;
            var childCount = _viewportContent.childCount;
            for (var i = childCount - 1; i >= 0; --i)
            {
                var child = _viewportContent.GetChild(i).gameObject;
                Destroy(child);
            }
        }

        public void UpdateRoomList(ReadOnlyCollection<RoomInfo> currentRooms)
        {
            _currentRooms.Clear();
            _currentRooms.AddRange(currentRooms);
            var childCount = _viewportContent.childCount;
            while (childCount > _currentRooms.Count)
            {
                childCount -= 1;
                var child = _viewportContent.GetChild(childCount).gameObject;
                Destroy(child);
            }
            while (childCount < _currentRooms.Count)
            {
                var button = Instantiate(_joinRoomButtonTemplate, _viewportContent);
                button.gameObject.SetActive(true);
                var capturedIndex = childCount;
                button.onClick.AddListener(() => { InvokeOnJoinRoom(capturedIndex); });
                childCount += 1;
            }
            Assert.IsTrue(_currentRooms.Count == childCount, "currentRooms.Count == childCount");
            for (var i = 0; i < childCount; ++i)
            {
                var roomInfo = _currentRooms[i];
                var button = _viewportContent.GetChild(i).GetComponent<Button>();
                var caption = $"{roomInfo.Name} {roomInfo.PlayerCount}/4 {(roomInfo.IsOpen ? "open" : "closed")}";
                button.SetCaption(caption);
                button.interactable = roomInfo.IsOpen;
            }
        }

        private void InvokeOnJoinRoom(int index)
        {
            var roomInfo = _currentRooms[index];
            Debug.Log($"InvokeOnJoinRoom {index} : {roomInfo.GetDebugLabel()}");
            OnJoinRoom?.Invoke(roomInfo.Name);
        }
    }
}
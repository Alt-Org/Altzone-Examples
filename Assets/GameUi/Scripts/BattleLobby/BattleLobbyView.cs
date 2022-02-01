using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.BattleLobby
{
    public class BattleLobbyView : MonoBehaviour
    {
        [SerializeField] private Text _currentPlayerInfo;
        [SerializeField] private Text _roomListInfo;
        [SerializeField] private Button _createRoomButton;
        [SerializeField] private Button _joinRoomButtonTemplate;
        [SerializeField] private Transform _viewportContent;

        private void Awake()
        {
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
            _currentPlayerInfo.text = string.Empty;
            _createRoomButton.interactable = false;
            _roomListInfo.text = string.Empty;
            var childCount = _viewportContent.childCount;
            for (var i = childCount - 1; i >= 0; --i)
            {
                var child = _viewportContent.GetChild(i).gameObject;
                Destroy(child);
            }
            var button = Instantiate(_joinRoomButtonTemplate, _viewportContent);
            button.gameObject.SetActive(true);
            button.onClick.AddListener(() =>
            {
                OnJoinRoom?.Invoke(button.GetCaption());
            });
        }

        public class RoomInfo
        {
            public string RoomName;
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.BattleLobby
{
    public class BattleLobbyView : MonoBehaviour
    {
        [SerializeField] private Text _playerInfo;
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button _playerButton1;
        [SerializeField] private Button _playerButton2;
        [SerializeField] private Button _playerButton3;
        [SerializeField] private Button _playerButton4;

        private Button[] _playerButtons;

        private void Awake()
        {
            _playerButtons = new[] { _playerButton1, _playerButton2, _playerButton3, _playerButton4 };
        }

        public string PlayerInfo
        {
            get => _playerInfo.text;
            set => _playerInfo.text = value;
        }

        public Button StartGameButton => _startGameButton;

        public void ResetView()
        {
            _startGameButton.interactable = false;
            foreach (var button in _playerButtons)
            {
                button.SetCaption("- free -");
            }
        }

        public class PlayerInRoom
        {
            public string PlayerName;

            public bool IsPresent => PlayerName != null;
        }
    }
}
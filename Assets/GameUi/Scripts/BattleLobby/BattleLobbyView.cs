using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.BattleLobby
{
    public class BattleLobbyView : MonoBehaviour
    {
        [SerializeField] private Text _playerInfo;
        [SerializeField] private Button _startGameButton;

        public string PlayerInfo
        {
            get => _playerInfo.text;
            set => _playerInfo.text = value;
        }

        public Button StartGameButton => _startGameButton;

        public void ResetView()
        {
            _startGameButton.interactable = false;
        }
    }
}
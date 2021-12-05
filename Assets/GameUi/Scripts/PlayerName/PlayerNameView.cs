using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.PlayerName
{
    public class PlayerNameView : MonoBehaviour
    {
        [SerializeField] private InputField _playerName;
        [SerializeField] private Button _continueButton;

        public string PlayerName
        {
            get => _playerName.text;
            set => _playerName.text = value;
        }

        public InputField PlayerNameInput => _playerName;
        public Button ContinueButton => _continueButton;
    }
}
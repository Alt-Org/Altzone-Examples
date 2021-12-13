using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.PlayerName
{
    public class PlayerNameView : MonoBehaviour
    {
        [SerializeField] private InputField _playerName;
        [Header("First Time"), SerializeField] private Button _continueButton;
        [Header("Normal Operation"), SerializeField] private Button _backButtonMini;
        [SerializeField] private Button _backButton;

        public string PlayerName
        {
            get => _playerName.text;
            set => _playerName.text = value;
        }

        public InputField PlayerNameInput => _playerName;
        public Button BackButton => _backButton;
        public Button ContinueButton => _continueButton;

        public void HideWhenFirstTime()
        {
            _continueButton.gameObject.SetActive(false);
        }

        public void HideWhenNormalOperation()
        {
            _backButtonMini.gameObject.SetActive(false);
            _backButton.gameObject.SetActive(false);
        }
    }
}
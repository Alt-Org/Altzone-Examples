using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.UserSettings
{
    public class UserSettingsView : MonoBehaviour
    {
        [SerializeField] private Text _shieldName;
        [SerializeField] private InputField _playerNameInput;
        [SerializeField] private Button _saveButton;

        public string ShieldName
        {
            set => _shieldName.text = value;
        }

        public string PlayerName
        {
            set => _playerNameInput.text = value;
            get => _playerNameInput.text;
        }

        public Button SaveButton => _saveButton;
    }
}
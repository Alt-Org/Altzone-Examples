using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.UserSettings
{
    public class UserSettingsView : MonoBehaviour
    {
        [SerializeField] private InputField _playerNameInput;
        [SerializeField] private Button _saveButton;

        public InputField PlayerNameInput => _playerNameInput;
        public Button SaveButton => _saveButton;
    }
}
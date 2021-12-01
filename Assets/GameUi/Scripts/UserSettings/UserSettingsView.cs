using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.UserSettings
{
    public class UserSettingsView : MonoBehaviour
    {
        [SerializeField] private Text _shieldName;
        [SerializeField] private InputField _playerNameInput;
        [SerializeField] private Button _saveButton;

        public Text ShieldName => _shieldName;
        public InputField PlayerNameInput => _playerNameInput;
        public Button SaveButton => _saveButton;
    }
}
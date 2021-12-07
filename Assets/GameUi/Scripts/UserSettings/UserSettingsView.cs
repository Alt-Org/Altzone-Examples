using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.UserSettings
{
    public class UserSettingsView : MonoBehaviour
    {
        [SerializeField] private Text _playerInfo;

        public string PlayerInfo
        {
            set => _playerInfo.text = value;
        }
    }
}
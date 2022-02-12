using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.UserSettings
{
    public class UserSettingsView : MonoBehaviour
    {
        [SerializeField] private Text _playerInfo;
        [SerializeField] private Button _joinClan;
        [SerializeField] private Button _leaveClan;
        [SerializeField] private Button _toggleDebug;

        public string PlayerInfo
        {
            set => _playerInfo.text = value;
        }

        public Button ToggleDebug => _toggleDebug;

        public void ShowJoinClanButton()
        {
            _joinClan.gameObject.SetActive(true);
            _leaveClan.gameObject.SetActive(false);
        }

        public void ShowLeaveClanButton()
        {
            _joinClan.gameObject.SetActive(false);
            _leaveClan.gameObject.SetActive(true);
        }
    }
}
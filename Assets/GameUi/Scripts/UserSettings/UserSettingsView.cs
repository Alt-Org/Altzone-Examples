using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.UserSettings
{
    public class UserSettingsView : MonoBehaviour
    {
        [SerializeField] private Text _playerInfo;
        [SerializeField] private Button _joinClan;
        [SerializeField] private Button _leaveClan;

        public string PlayerInfo
        {
            set => _playerInfo.text = value;
        }

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
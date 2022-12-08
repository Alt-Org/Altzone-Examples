using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.ClanManagement
{
    public class LeaveClanView : MonoBehaviour
    {
        [SerializeField] private Text _playerInfo;
        [SerializeField] private Text _clanInfo;
        [SerializeField] private Button _leaveClanButton;

        public string PlayerInfo
        {
            set => _playerInfo.text = value;
        }

        public string ClanInfo
        {
            set => _clanInfo.text = value;
        }

        public Button LeaveClanButton => _leaveClanButton;

        public void ResetView()
        {
            PlayerInfo = string.Empty;
            ClanInfo = string.Empty;
            LeaveClanButton.interactable = false;
        }
    }
}
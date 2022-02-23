using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.BrainCloud
{
    public class BrainCloudUiView : MonoBehaviour
    {
        [SerializeField] private Text _playerInfo;
        [SerializeField] private Text _brainCloudInfo;
        [SerializeField] private Button _login;
        [SerializeField] private Button _updateUserName;

        public string PlayerInfo
        {
            set => _playerInfo.text = value;
        }

        public string BrainCloudInfo
        {
            set => _brainCloudInfo.text = value;
        }

        public Button LoginButton => _login;
        public Button UpdateUserNameButton => _updateUserName;

        public void ResetView()
        {
            _playerInfo.text = string.Empty;
            _brainCloudInfo.text = string.Empty;
            _login.interactable = false;
            _updateUserName.interactable = false;
        }
    }
}
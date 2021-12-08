using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.MainMenu
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private Text _playerInfo;
        [SerializeField] private Text _testText;
        [SerializeField] private Button _testButtonA;
        [SerializeField] private Button _testButtonB;

        public string PlayerInfo
        {
            get => _playerInfo.text;
            set => _playerInfo.text = value;
        }

        public string TestText
        {
            get => _testText.text;
            set => _testText.text = value;
        }

        public Button TestButtonA => _testButtonA;
        public Button TestButtonB => _testButtonB;
    }
}
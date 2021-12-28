using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.GameOver
{
    public class GameOverView : MonoBehaviour
    {
        [SerializeField] private Text _winnerInfo;
        [SerializeField] private Button _continueButton;

        public string WinnerInfo
        {
            get => _winnerInfo.text;
            set => _winnerInfo.text = value;
        }

        public Button ContinueButton => _continueButton;
    }
}
using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.FirstTime
{
    public class FirstTimeView : MonoBehaviour
    {
        [SerializeField] private Toggle _acceptToS;
        [SerializeField] private Button _continueButton;

        public Toggle AcceptToS => _acceptToS;
        public Button ContinueButton => _continueButton;

        public void SetCanContinue(bool canContinue)
        {
            _continueButton.interactable = canContinue;
        }
    }
}

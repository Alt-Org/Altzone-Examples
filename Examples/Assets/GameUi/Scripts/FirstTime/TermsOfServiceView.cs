using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.FirstTime
{
    public class TermsOfServiceView : MonoBehaviour
    {
        [SerializeField] private Toggle _acceptToS;
        [SerializeField] private Button _linkButton;
        [SerializeField] private Button _continueButton;

        public Toggle AcceptToS => _acceptToS;
        public Button LinkButton => _linkButton;
        public Button ContinueButton => _continueButton;

        public void SetCompanyUrlText(string text)
        {
            _linkButton.SetCaption(text);
        }

        public void SetMustAcceptToS()
        {
            _acceptToS.isOn = false;
            _continueButton.interactable = false;
        }

        public void SetCanContinue(bool state)
        {
            _continueButton.interactable = state;
        }
    }
}
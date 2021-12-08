using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.FirstTime
{
    public class FirstTimeView : MonoBehaviour
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

        public void SetCanContinue(bool canContinue)
        {
            _continueButton.interactable = canContinue;
        }
    }
}
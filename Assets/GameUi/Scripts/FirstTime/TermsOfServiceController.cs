using System.Collections;
using Altzone.Scripts.Config;
using Prg.Scripts.Common.Unity.Window;
using Prg.Scripts.Common.Unity.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameUi.Scripts.FirstTime
{
    public class TermsOfServiceController : MonoBehaviour
    {
        [SerializeField] private TermsOfServiceView _view;
        [SerializeField] private WindowDef _previousWindow;
        [SerializeField] private WindowDef _nextWindow;
        [SerializeField] private LinkToHtmlPage _companyUrl;

        private void Awake()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(_companyUrl.URL), "string.IsNullOrWhiteSpace(_companyUrl.URL)");
            Assert.IsFalse(string.IsNullOrWhiteSpace(_companyUrl.Text), "string.IsNullOrWhiteSpace(_companyUrl.Text)");
            var linkText = $"{_companyUrl.Text}";
            _view.SetCompanyUrlText(linkText);
            _view.AcceptToS.onValueChanged.AddListener(AcceptToS);
            _view.LinkButton.onClick.AddListener(LinkButton);
            _view.ContinueButton.onClick.AddListener(ContinueButton);
        }

        private void OnEnable()
        {
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            if (playerData.IsTosAccepted)
            {
                StartCoroutine(LoadNextWindow());
                return;
            }
            _view.SetMustAcceptToS();
            WindowManager.Get().RegisterGoBackHandlerOnce(GoBackAlways);
        }

        private WindowManager.GoBackAction GoBackAlways()
        {
            WindowManager.Get().ShowWindow(_previousWindow);
            return WindowManager.GoBackAction.Abort;
        }

        private void AcceptToS(bool state)
        {
            _view.SetCanContinue(state);
        }

        private void LinkButton()
        {
            Debug.Log($"LinkButton {_companyUrl.URL}");
            Application.OpenURL(_companyUrl.URL);
        }

        private static void ContinueButton()
        {
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            playerData.BatchSave(() => { playerData.IsTosAccepted = true; });
            Debug.Log($"TOS ACCEPTED {playerData}");
        }

        private IEnumerator LoadNextWindow()
        {
            yield return null;
            WindowManager.Get().ShowWindow(_nextWindow);
        }
    }
}
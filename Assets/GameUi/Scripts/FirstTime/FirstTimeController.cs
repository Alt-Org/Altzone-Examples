using System.Collections;
using Altzone.Scripts.Config;
using Altzone.Scripts.Window;
using Altzone.Scripts.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameUi.Scripts.FirstTime
{
    public class FirstTimeController : MonoBehaviour
    {
        [SerializeField] private FirstTimeView _view;
        [SerializeField] private WindowDef _nextWindow;
        [SerializeField] private LinkToHtmlPage _companyUrl;

        private void Awake()
        {
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            if (playerData.IsTosAccepted)
            {
                StartCoroutine(LoadNextWindow(playerData.Language));
                return;
            }
            Assert.IsFalse(string.IsNullOrWhiteSpace(_companyUrl.URL), "string.IsNullOrWhiteSpace(_companyUrl.URL)");
            Assert.IsFalse(string.IsNullOrWhiteSpace(_companyUrl.Text), "string.IsNullOrWhiteSpace(_companyUrl.Text)");
            _view.SetCanContinue(false);
            var linkText = $"{_companyUrl.Text}";
            _view.SetCompanyUrlText(linkText);
            _view.AcceptToS.onValueChanged.AddListener(AcceptToS);
            _view.LinkButton.onClick.AddListener(LinkButton);
            _view.ContinueButton.onClick.AddListener(ContinueButton);
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

        private IEnumerator LoadNextWindow(SystemLanguage language)
        {
            yield return null;
            WindowManager.Get().ShowWindow(_nextWindow);
        }
    }
}
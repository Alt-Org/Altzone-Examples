using System.Collections;
using Altzone.Scripts.Config;
using Altzone.Scripts.Window;
using Altzone.Scripts.Window.ScriptableObjects;
using UnityEngine;

namespace GameUi.Scripts.FirstTime
{
    public class FirstTimeController : MonoBehaviour
    {
        [SerializeField] private FirstTimeView _view;
        [SerializeField] private WindowDef _nextWindow;

        private void Awake()
        {
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            if (playerData.IsTosAccepted)
            {
                StartCoroutine(LoadNextWindow(playerData.Language));
                return;
            }
            _view.SetCanContinue(false);
            _view.AcceptToS.onValueChanged.AddListener(AcceptToS);
            _view.ContinueButton.onClick.AddListener(ContinueButton);
        }

        private void AcceptToS(bool state)
        {
            _view.SetCanContinue(state);
        }

        private void ContinueButton()
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
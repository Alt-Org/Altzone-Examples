using System.Collections;
using Altzone.Scripts.Config;
using Altzone.Scripts.Window;
using Altzone.Scripts.Window.ScriptableObjects;
using Prg.Scripts.Common.Unity.Localization;
using UnityEngine;

namespace GameUi.Scripts.LanguageSelection
{
    public class LanguageSelectionController : MonoBehaviour
    {
        [SerializeField] private LanguageSelectionView _view;
        [SerializeField] private WindowDef _nextWindow;
        [SerializeField] private LanguageButtonController[] _buttons;

        private void Awake()
        {
            var playerData = RuntimeGameConfig.GetPlayerDataCacheInEditor();
            Debug.Log($"Awake {playerData}");
            if (playerData.HasLanguageCode)
            {
                StartCoroutine(LoadNextWindow(playerData.Language));
                return;
            }
            _view.ContinueButton.interactable = false;
            foreach (var button in _buttons)
            {
                button.SetLanguageCallback += SetLanguage;
            }
            var language = Localizer.HasLanguage(Application.systemLanguage)
                ? Application.systemLanguage
                : Localizer.DefaultLanguage;
            Debug.Log($"language {language}");
            SelectLanguage(language);
        }

        private void SelectLanguage(SystemLanguage language)
        {
            foreach (var button in _buttons)
            {
                button.SetSelected(language == button.Language);
            }
        }

        private IEnumerator LoadNextWindow(SystemLanguage language)
        {
            yield return null;
            Localizer.SetLanguage(language);
            WindowManager.Get().ShowWindow(_nextWindow);
        }

        private void SetLanguage(SystemLanguage language)
        {
            SelectLanguage(language);
            _view.ContinueButton.interactable = true;

            var playerData = RuntimeGameConfig.GetPlayerDataCacheInEditor();
            Debug.Log($"SetLanguage {playerData.Language} <- {language}");
            playerData.BatchSave(() => { playerData.Language = language; });
            Localizer.SetLanguage(language);
        }
    }
}
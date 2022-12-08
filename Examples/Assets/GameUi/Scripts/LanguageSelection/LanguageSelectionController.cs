using System;
using System.Collections;
using Altzone.Scripts.Config;
using Prg.Scripts.Common.Unity.Localization;
using Prg.Scripts.Common.Unity.Window;
using Prg.Scripts.Common.Unity.Window.ScriptableObjects;
using UnityEngine;

namespace GameUi.Scripts.LanguageSelection
{
    /// <summary>
    /// Selects player's UI language.
    /// </summary>
    public class LanguageSelectionController : MonoBehaviour
    {
        [Serializable]
        public class LangButtonConfig
        {
            public string _localizationKey;
            public SystemLanguage _language;
            public Sprite _flag;
        }

        [SerializeField] private LanguageSelectionView _view;
        [SerializeField] private WindowDef _nextWindow;
        [SerializeField] private LangButtonConfig[] _langConfigs;
        [SerializeField] private LanguageButtonController[] _buttons;

        private void Awake()
        {
            for (var i = 0; i < _buttons.Length; ++i)
            {
                _buttons[i].Initialize(_langConfigs[i]);
            }
        }

        private void OnEnable()
        {
            var playerData = GameConfig.Get().PlayerDataCache;
            if (playerData.IsFirstTimePlaying)
            {
                WindowManager.Get().RegisterGoBackHandlerOnce(AbortGoBackAlways);
                _view.ShowFirstTime();
            }
            else
            {
                var isGameStarting = WindowManager.Get().WindowCount <= 1;
                if (isGameStarting)
                {
                    StartCoroutine(LoadNextWindow());
                    return;
                }
                _view.ShowNormalOperation();
            }
            var language =
                Localizer.HasLanguage(playerData.Language) ? playerData.Language
                : Localizer.HasLanguage(Application.systemLanguage) ? Application.systemLanguage
                : Localizer.Language;
            Debug.Log(
                $"OnEnable language {language} FirsTime {playerData.IsFirstTimePlaying} windows #{WindowManager.Get().WindowCount}");
            Debug.Log($"{playerData}");
            foreach (var button in _buttons)
            {
                button.SetLanguageCallback += SetLanguage;
            }
            SetLanguage(language);
        }

        private void OnDisable()
        {
            WindowManager.Get().UnRegisterGoBackHandlerOnce(AbortGoBackAlways);
        }

        private static WindowManager.GoBackAction AbortGoBackAlways()
        {
            WindowManager.Get().RegisterGoBackHandlerOnce(AbortGoBackAlways);
            return WindowManager.GoBackAction.Abort;
        }

        private void SetLanguage(SystemLanguage language)
        {
            var playerData = GameConfig.Get().PlayerDataCache;
            Debug.Log($"SetLanguage {playerData.Language} <- {language}");
            if (playerData.Language != language)
            {
                playerData.Language = language;
            }
            Localizer.SetLanguage(language);
            SelectLanguage(language);
            _view.Localize();
        }

        private void SelectLanguage(SystemLanguage language)
        {
            _view.ContinueButton.interactable = false;
            foreach (var button in _buttons)
            {
                var isSelected = language == button.Language;
                button.SetSelected(isSelected);
                if (isSelected)
                {
                    _view.ContinueButton.interactable = true;
                }
            }
        }

        private IEnumerator LoadNextWindow()
        {
            yield return null;
            WindowManager.Get().ShowWindow(_nextWindow);
        }
    }
}
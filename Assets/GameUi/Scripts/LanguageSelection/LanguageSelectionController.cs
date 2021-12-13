using System;
using Altzone.Scripts.Config;
using Altzone.Scripts.Window;
using Prg.Scripts.Common.Unity.Localization;
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
            if (RuntimeGameConfig.IsFirsTimePlaying || WindowManager.Get().WindowCount <= 1)
            {
                _view.ShowFirstTime();
                WindowManager.Get().RegisterGoBackHandlerOnce(AbortGoBackAlways);
            }
            else
            {
                _view.ShowNormalOperation();
            }
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            var language =
                Localizer.HasLanguage(playerData.Language) ? playerData.Language
                : Localizer.HasLanguage(Application.systemLanguage) ? Application.systemLanguage
                : Localizer.DefaultLanguage;
            Debug.Log(
                $"OnEnable language {language} FirsTime {RuntimeGameConfig.IsFirsTimePlaying} windows #{WindowManager.Get().WindowCount}");
            Debug.Log($"{playerData}");
            foreach (var button in _buttons)
            {
                button.SetLanguageCallback += SetLanguage;
            }
            SetLanguage(language);
        }

        private bool _isApplicationQuit;

        private void OnApplicationQuit()
        {
            _isApplicationQuit = true;
        }

        private void OnDisable()
        {
            if (_isApplicationQuit)
            {
                return;
            }
            WindowManager.Get().UnRegisterGoBackHandlerOnce(AbortGoBackAlways);
        }

        private static WindowManager.GoBackAction AbortGoBackAlways()
        {
            WindowManager.Get().RegisterGoBackHandlerOnce(AbortGoBackAlways);
            return WindowManager.GoBackAction.Abort;
        }

        private void SetLanguage(SystemLanguage language)
        {
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            Debug.Log($"SetLanguage {playerData.Language} <- {language}");
            if (playerData.Language != language)
            {
                playerData.BatchSave(() => { playerData.Language = language; });
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
    }
}
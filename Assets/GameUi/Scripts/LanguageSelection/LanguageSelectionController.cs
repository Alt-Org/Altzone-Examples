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

        private void Awake()
        {
            var playerData = RuntimeGameConfig.GetPlayerDataCacheInEditor();
            Debug.Log($"UNITY systemLanguage is {Application.systemLanguage}");
            Debug.Log(playerData.ToString());
            if (playerData.HasLanguageCode)
            {
                StartCoroutine(LoadNextWindow(playerData.Language));
                return;
            }
            Localizer.SetLanguage(Application.systemLanguage);
            _view.LangButtonFi.onClick.AddListener(() => SetLanguage(SystemLanguage.Finnish));
            _view.LangButtonSe.onClick.AddListener(() => SetLanguage(SystemLanguage.Swedish));
            _view.LangButtonEn.onClick.AddListener(() => SetLanguage(SystemLanguage.English));
        }

        private IEnumerator LoadNextWindow(SystemLanguage language)
        {
            yield return null;
            Localizer.SetLanguage(language);
            WindowManager.Get().ShowWindow(_nextWindow);
        }

        private static void SetLanguage(SystemLanguage language)
        {
            var playerData = RuntimeGameConfig.GetPlayerDataCacheInEditor();
            Debug.Log($"SetLanguage {playerData.Language} <- {language}");
            playerData.BatchSave(() =>
            {
                playerData.Language = language;
            });
            Debug.Log(playerData.ToString());
            Localizer.SetLanguage(language);
        }
    }
}
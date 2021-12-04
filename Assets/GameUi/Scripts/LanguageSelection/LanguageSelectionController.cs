using Altzone.Scripts.Config;
using Altzone.Scripts.Window;
using Altzone.Scripts.Window.ScriptableObjects;
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
                WindowManager.Get().ShowWindow(_nextWindow);
                return;
            }
            if (Application.systemLanguage == SystemLanguage.English
                || Application.systemLanguage == SystemLanguage.Finnish
                || Application.systemLanguage == SystemLanguage.Swedish)
            {
                Debug.LogWarning($"We should have a localization for {Application.systemLanguage}");
            }
            // https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes
            _view.LangButtonFi.onClick.AddListener(() => SetLanguage("fi"));
            _view.LangButtonSe.onClick.AddListener(() => SetLanguage("sv"));
            _view.LangButtonEn.onClick.AddListener(() => SetLanguage("en"));
        }

        private static void SetLanguage(string language)
        {
            var playerData = RuntimeGameConfig.GetPlayerDataCacheInEditor();
            playerData.BatchSave(() =>
            {
                Debug.Log($"SetLanguage {playerData.LanguageCode} <- {language}");
                playerData.LanguageCode = language;
            });
            Debug.Log(playerData.ToString());
        }
    }
}
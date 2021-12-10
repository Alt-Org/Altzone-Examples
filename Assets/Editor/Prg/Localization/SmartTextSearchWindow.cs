using System.Linq;
using Editor.Prg.Util;
using Prg.Scripts.Common.Unity.Localization;
using UnityEditor;
using UnityEngine;

namespace Editor.Prg.Localization
{
    /// <summary>
    /// Search window implementation to find localization keys in UNITY Editor.
    /// </summary>
    public class SmartTextSearchWindow : EditorWindow
    {
        private const string MenuRoot = LocalizerMenu.MenuRoot;

        [MenuItem(MenuRoot + "Search Localization Keys")]
        private static void SearchLocalizationKeys()
        {
            GetWindow<SmartTextSearchWindow>("Search Localization keys").Show();
        }

        private AutocompleteSearch _autocompleteSearchField;

        private void OnEnable()
        {
            if (!Localizer.HasLanguage(Localizer.DefaultLanguage))
            {
                Localizer.LoadTranslations();
            }
            if (_autocompleteSearchField == null)
            {
                _autocompleteSearchField = new AutocompleteSearch();
            }
            _autocompleteSearchField.OnFirstTime = () =>
            {
                OnInputChanged(string.Empty);
            };
            _autocompleteSearchField.OnInputChangedCallback = OnInputChanged;
            _autocompleteSearchField.OnConfirmCallback = OnConfirm;
        }

        private void OnGUI()
        {
            GUILayout.Label("Search Localization Keys", EditorStyles.boldLabel);
            _autocompleteSearchField.OnGUI();
        }

        private void OnInputChanged(string searchString)
        {
            var results = Localizer.GetTranslationKeys();
            var search = string.IsNullOrEmpty(searchString)
                ? results
                : results.Where(x => x.Contains(searchString)).ToList();
            _autocompleteSearchField.SetResults(search);
            Debug.Log($"OnInputChanged /{searchString}/ found : {search.Count}/{results.Count}");
        }

        private void OnConfirm(string result)
        {
            var activeObject = Selection.activeGameObject;
            if (activeObject == null)
            {
                return;
            }
            Debug.Log($"OnConfirm /{result}/ => {activeObject.name}");
            var smartText = activeObject.GetComponent<SmartText>();
            if (smartText == null)
            {
                return;
            }
            Debug.Log($"LocalizationKey {smartText.LocalizationKey} <- {result}");
            smartText.LocalizationKey = result;
        }
    }
}
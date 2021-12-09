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
            _autocompleteSearchField.OnInputChangedCallback = OnInputChanged;
            _autocompleteSearchField.OnConfirmCallback = OnConfirm;
        }

        private void OnGUI()
        {
            GUILayout.Label("Search localization keys", EditorStyles.boldLabel);
            _autocompleteSearchField.OnGUI();
        }

        private void OnInputChanged(string searchString)
        {
            _autocompleteSearchField.ClearResults();
            var found = 0;
            var results = Localizer.GetTranslationKeys();
            if (string.IsNullOrEmpty(searchString))
            {
                foreach (var result in results)
                {
                    found += 1;
                    _autocompleteSearchField.AddResult(result);
                }
            }
            else
            {
                foreach (var result in results)
                {
                    if (result.Contains(searchString))
                    {
                        found += 1;
                        _autocompleteSearchField.AddResult(result);
                    }
                }
            }
            Debug.Log($"searchString /{searchString}/ found : {found}/{results.Count}");
        }

        private void OnConfirm(string result)
        {
            var obj = AssetDatabase.LoadMainAssetAtPath(_autocompleteSearchField._searchString);
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Config;
using Prg.Scripts.Common.Unity.Localization;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Editor.Prg.Localization
{
    public class SmartTextSearchEditorWindow : EditorWindow
    {
        private const string MenuRoot = LocalizerMenu.MenuRoot;

        [MenuItem(MenuRoot + "Show Localization Window")]
        private static void SearchLocalizationKeys()
        {
            Debug.Log("SearchLocalizationKeys");
            GetWindow<SmartTextSearchEditorWindow>("Localization Helper")
                .Show();
        }

        private SearchField _searchField;
        private string _searchText;
        private string _usedSearchText;

        private Vector2 _scrollPosition;
        private List<string> _fullResults;
        private List<string> _searchResults = new List<string>();

        private void OnEnable()
        {
            _searchField = new SearchField();
            _searchText = string.Empty;
            Debug.Log($"OnEnable");
            var playerData = RuntimeGameConfig.GetPlayerDataCacheInEditor();
            playerData.Language = SystemLanguage.English;
            var language = playerData.HasLanguageCode ? playerData.Language : Localizer.DefaultLanguage;
            if (!Localizer.HasLanguage(language))
            {
                Localizer.LoadTranslations();
            }
            Localizer.SetLanguage(language);
            _fullResults = Localizer.GetTranslationKeys();
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Label("Line 1");
                GUILayout.Label("Line 2");
                GUILayout.Label("Line 3");
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label($"Search {_searchResults.Count}:");
                    _searchText = _searchField.OnGUI(_searchText);
                }
                using (var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition, false, false))
                {
                    _scrollPosition = scrollView.scrollPosition;
                    foreach (var searchResult in _searchResults)
                    {
                        GUILayout.Label(searchResult);
                    }
                }
            }
        }

        private void Update()
        {
            if (_usedSearchText != _searchText)
            {
                _usedSearchText = _searchText?.ToLower() ?? string.Empty;
                _searchResults = string.IsNullOrEmpty(_usedSearchText)
                    ? _fullResults
                    : _fullResults.Where(x => x.ToLower().Contains(_usedSearchText)).ToList();
            }
        }
    }
}
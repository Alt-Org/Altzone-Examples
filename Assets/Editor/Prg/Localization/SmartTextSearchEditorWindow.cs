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
        private Vector2 _mousePosition;
        private int _hotControlId;

        private List<string> _fullResults;
        private List<string> _searchResults = new List<string>();

        private string _label1;
        private string _label2;
        private string _label3;

        private void OnEnable()
        {
            _searchField = new SearchField();
            _searchText = string.Empty;
            _label1 = string.Empty;
            _label2 = string.Empty;
            _label3 = string.Empty;
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
                EditorGUILayout.LabelField(_label1);
                EditorGUILayout.LabelField(_label2);
                EditorGUILayout.LabelField(_label3);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"Search {_searchResults.Count}/{_fullResults.Count}:");
                    _searchText = _searchField.OnGUI(_searchText);
                }
                using (var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition, false, false))
                {
                    _scrollPosition = scrollView.scrollPosition;
                    var rowCount = 0;
                    foreach (var searchResult in _searchResults)
                    {
                        EditorGUILayout.LabelField($"{++rowCount}", searchResult);
                    }
                }
            }
            if(Event.current.type == EventType.Layout) return;
            var e = Event.current;
            if (e.isMouse || e.isKey)
            {
                if (e.commandName == "Used")
                {
                    return;
                }
                e.commandName = "Used";
            }
            if (e.type == EventType.MouseDown)
            {
                _mousePosition = e.mousePosition;
                Debug.Log($"Mouse Down {_mousePosition.x:F0}/{_mousePosition.y:F0}");
                return;
            }
            if (e.type == EventType.MouseUp)
            {
                _mousePosition = e.mousePosition;
                Debug.Log($"Mouse Up {_mousePosition.x:F0}/{_mousePosition.y:F0}");
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
            _label1 = $"mouse {_mousePosition.x:F0}/{_mousePosition.y:F0}";
            _label2 = $"scroll {_scrollPosition.x:F0}/{_scrollPosition.y:F0}";
            //_label3 = $"hot {_hotControlId}";
        }
    }
}
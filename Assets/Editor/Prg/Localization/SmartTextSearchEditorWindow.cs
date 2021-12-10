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
            _searchButtonStyle = new GUIStyle(EditorStyles.toolbarButton)
            {
                alignment = TextAnchor.MiddleLeft,
                richText = false
            };
            Debug.Log("SearchLocalizationKeys");
            GetWindow<SmartTextSearchEditorWindow>("Localization Key Utility")
                .Show();
        }

        private static GUIStyle _searchButtonStyle;
        private SearchField _searchField;
        private string _searchText;
        private string _usedSearchText;

        private Vector2 _scrollPosition;

        private List<string> _fullResults;
        private List<string> _searchResults = new List<string>();

        private GameObject _activeGameObject;
        private SmartText _smartText;

        private string _label1;
        private string _label2;
        private string _label3;

        private void OnEnable()
        {
            Debug.Log($"OnEnable");
            _searchField = new SearchField();
            _searchText = string.Empty;
            _label1 = string.Empty;
            _label2 = string.Empty;
            _label3 = string.Empty;
            var playerData = RuntimeGameConfig.GetPlayerDataCacheInEditor();
            playerData.Language = SystemLanguage.English;
            var language = playerData.HasLanguageCode ? playerData.Language : Localizer.DefaultLanguage;
            if (!Localizer.HasLanguage(language))
            {
                Localizer.LoadTranslations();
            }
            Localizer.SetLanguage(language);
            _fullResults = Localizer.GetTranslationKeys().Where(x => !x.StartsWith("lang.")).ToList();
            // Prevent double registration
            Selection.selectionChanged -= SelectionChanged;
            Selection.selectionChanged += SelectionChanged;
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
                    foreach (var searchResult in _searchResults)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button($"{searchResult}", _searchButtonStyle))
                            {
                                if (_smartText == null)
                                {
                                    continue;
                                }
                                if (EditorApplication.isPlaying)
                                {
                                    Debug.Log(RichText.Yellow("NO change when game is playing!"));
                                    continue;
                                }
                                Debug.Log($"Set localization key: {searchResult}");
                                _smartText.LocalizationKey = searchResult;
                            }
                        }
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

        private void SelectionChanged()
        {
            _activeGameObject = Selection.activeGameObject;
            if (_activeGameObject == null || Selection.objects.Length != 1)
            {
                _smartText = null;
            }
            else
            {
                _smartText = _activeGameObject.GetComponent<SmartText>();
            }
            if (_smartText != null)
            {
                _label1 = _activeGameObject.GetFullPath();
                _label2 = _smartText.LocalizationKey;
                _label3 = Localizer.Localize(_smartText.LocalizationKey);
            }
            else
            {
                _label1 = string.Empty;
                _label2 = string.Empty;
                _label3 = string.Empty;
            }
            Repaint();
        }
    }
}
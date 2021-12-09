using Editor.Prg.Util;
using UnityEditor;
using UnityEngine;

namespace Editor.Prg.Localization
{
    public class SmartTextSearchWindow : EditorWindow
    {
        private const string MenuRoot = LocalizerMenu.MenuRoot;

        [MenuItem(MenuRoot + "Search Localization keys")]
        private static void Init()
        {
            GetWindow<SmartTextSearchWindow>("Search Localization keys").Show();
        }

        [SerializeField] private AutocompleteSearch _autocompleteSearchField;

        private void OnEnable()
        {
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
            if (!string.IsNullOrEmpty(searchString))
            {
                const string assetRoot = "Assets";
                var searchFolders = new[] { assetRoot };
                foreach (var assetGuid in AssetDatabase.FindAssets(searchString, searchFolders))
                {
                    var result = AssetDatabase.GUIDToAssetPath(assetGuid);
                    if (result != _autocompleteSearchField._searchString)
                    {
                        _autocompleteSearchField.AddResult(result);
                    }
                }
            }
        }

        private void OnConfirm(string result)
        {
            var obj = AssetDatabase.LoadMainAssetAtPath(_autocompleteSearchField._searchString);
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }
    }
}
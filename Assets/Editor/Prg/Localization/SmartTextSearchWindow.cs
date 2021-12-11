using System.Linq;
using Altzone.Scripts.Config;
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
        private static class Styles
        {
            public static readonly GUIStyle InfoStyleBlue;
            public static readonly GUIStyle InfoStyleMagenta;
            public static readonly GUIStyle InfoStyleRed;

            static Styles()
            {
                InfoStyleBlue = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    richText = true,
                    normal = new GUIStyleState
                    {
                        textColor = Color.blue
                    }
                };
                InfoStyleMagenta = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    richText = true,
                    normal = new GUIStyleState
                    {
                        textColor = Color.magenta
                    }
                };
                InfoStyleRed = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    richText = true,
                    normal = new GUIStyleState
                    {
                        textColor = Color.red
                    }
                };
            }
        }

        private const string MenuRoot = LocalizerMenu.MenuRoot;

        //[MenuItem(MenuRoot + "Search Localization Keys")]
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
            _autocompleteSearchField.OnFirstTime = () => { OnInputChanged(string.Empty); };
            _autocompleteSearchField.OnInputChangedCallback = OnInputChanged;
            _autocompleteSearchField.OnConfirmCallback = OnConfirm;
        }

        private void OnGUI()
        {
            GUILayout.Label("Search Localization Keys", EditorStyles.boldLabel);
            _autocompleteSearchField.OnGUI();
        }

        private string _infoText;
        private GUIStyle _infoTextStyle;

        private void Update()
        {
            // TODO: use Selection.selectionChanged callback!

            var activeObject = Selection.activeGameObject;
            string infoText;
            if (activeObject == null || Selection.objects.Length != 1)
            {
                infoText = string.Empty;
                _infoTextStyle = null;
            }
            else
            {
                infoText = activeObject.GetFullPath();
                var smartText = activeObject.GetComponent<SmartText>();
                if (smartText != null)
                {
                    var key = smartText.LocalizationKey;
                    var hasKey = string.IsNullOrEmpty(key);
                    var value = hasKey ? Localizer.Localize(key) : string.Empty;
                    var hasValue = !string.IsNullOrEmpty(value) && !(value.StartsWith("[") && value.EndsWith("]"));
                    infoText = $"<b>SMART {infoText}</b>\r\nKEY {key}\r\nTXT {value}";
                    _infoTextStyle = hasKey && hasValue
                        ? Styles.InfoStyleBlue
                        : Styles.InfoStyleMagenta;
                }
                else
                {
                    infoText = $"OTHER {infoText}";
                    _infoTextStyle = Styles.InfoStyleRed;
                }
            }
            if (infoText != _infoText)
            {
                _infoText = infoText;
                _autocompleteSearchField.SetInfoText(infoText, _infoTextStyle);
                Repaint();
            }
        }

        private void OnInputChanged(string searchString)
        {
            var results = Localizer.LocalizerHelper.GetTranslationKeys();
            searchString = searchString?.ToLower() ?? string.Empty;
            var search = string.IsNullOrEmpty(searchString)
                ? results
                : results.Where(x => x.ToLower().Contains(searchString)).ToList();
            _autocompleteSearchField.SetResults(search);
        }

        private static void OnConfirm(string result)
        {
            if (EditorApplication.isPlaying)
            {
                Debug.Log(RichText.Yellow("NO change when game is playing!"));
                return;
            }
            var activeObject = Selection.activeGameObject;
            if (activeObject == null || Selection.objects.Length != 1)
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
using System;
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
                    infoText = $"<b>SMART {infoText}</b>\r\nKEY {key}";
                    _infoTextStyle = string.IsNullOrEmpty(key)
                        ? Styles.InfoStyleMagenta
                        : Styles.InfoStyleBlue;
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
            var results = Localizer.GetTranslationKeys();
            var search = string.IsNullOrEmpty(searchString)
                ? results
                : results.Where(x => x.Contains(searchString)).ToList();
            _autocompleteSearchField.SetResults(search);
            Debug.Log($"OnInputChanged /{searchString}/ found : {search.Count}/{results.Count}");
        }

        private void OnConfirm(string result)
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
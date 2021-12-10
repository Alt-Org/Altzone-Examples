using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Editor.Prg.Util
{
    /// <summary>
    /// Base class to handle search tools windows in UNITY Editor.
    /// </summary>
    /// <remarks>
    /// See: https://github.com/marijnz/unity-autocomplete-search-field
    /// </remarks>
    public class AutocompleteSearch
    {
        private static class Styles
        {
            public const float ResultHeight = 20f;
            public const float ResultsBorderWidth = 2f;
            public const float ResultsMargin = 15f;
            public const float ResultsLabelOffset = 2f;

            public static readonly GUIStyle EntryEven;
            public static readonly GUIStyle EntryOdd;
            public static readonly GUIStyle LabelStyle;
            public static readonly GUIStyle ResultsBorderStyle;

            static Styles()
            {
                EntryOdd = new GUIStyle("CN EntryBackOdd");
                EntryEven = new GUIStyle("CN EntryBackEven");
                ResultsBorderStyle = new GUIStyle("hostview");
                LabelStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    richText = true
                };
            }
        }

        private const int MAXResults = 50;

        public Action OnFirstTime;
        public Action<string> OnInputChangedCallback;
        public Action<string> OnConfirmCallback;

        private List<string> _results = new List<string>();
        private int _selectedIndex = -1;

        private SearchField _searchField;
        private string _searchString;

        public void SetResults(List<string> results)
        {
            _results = results;
        }

        public void AddResult(string result)
        {
            _results.Add(result);
        }

        public void ClearResults()
        {
            _results.Clear();
        }

        public void OnToolbarGUI()
        {
            Draw(asToolbar: true);
        }

        public void OnGUI()
        {
            Draw(asToolbar: false);
        }

        private void Draw(bool asToolbar)
        {
            var rect = GUILayoutUtility.GetRect(1, 1, 18, 18, GUILayout.ExpandWidth(true));
            GUILayout.BeginHorizontal();
            DoSearchField(rect, asToolbar);
            GUILayout.EndHorizontal();
            rect.y += 18;
            DoResults(rect);
        }

        private void DoSearchField(Rect rect, bool asToolbar)
        {
            if (_searchField == null)
            {
                _searchField = new SearchField();
                // Short-circuit here to show initial search
                _selectedIndex = -1;
                OnFirstTime?.Invoke();
                return;
            }

            var result = asToolbar
                ? _searchField.OnToolbarGUI(rect, _searchString)
                : _searchField.OnGUI(rect, _searchString);

            if (result != _searchString)
            {
                Debug.Log($"DoSearchField {_searchString} <- {result} sel index {_selectedIndex} <- {-1}");
                _searchString = result;
                _selectedIndex = -1;
                OnInputChangedCallback?.Invoke(result);
            }
            if (HasSearchbarFocused())
            {
                RepaintFocusedWindow();
            }
        }

        private void DoResults(Rect rect)
        {
            if (_results.Count <= 0)
            {
                return;
            }
            var current = Event.current;
            rect.height = Styles.ResultHeight * Mathf.Min(MAXResults, _results.Count);
            rect.x = Styles.ResultsMargin;
            rect.width -= Styles.ResultsMargin * 2;

            var elementRect = rect;

            rect.height += Styles.ResultsBorderWidth;
            GUI.Label(rect, "", Styles.ResultsBorderStyle);

            var mouseIsInResultsRect = rect.Contains(current.mousePosition);
            if (mouseIsInResultsRect)
            {
                RepaintFocusedWindow();
            }

            elementRect.x += Styles.ResultsBorderWidth;
            elementRect.width -= Styles.ResultsBorderWidth * 2;
            elementRect.height = Styles.ResultHeight;

            for (var i = 0; i < _results.Count && i < MAXResults; ++i)
            {
                if (elementRect.Contains(current.mousePosition))
                {
                    if (current.type == EventType.MouseDown)
                    {
                        _selectedIndex = i;
                        OnConfirm(_results[i]);
                    }
                }
                if (current.type == EventType.Repaint)
                {
                    var style = i % 2 == 0 ? Styles.EntryOdd : Styles.EntryEven;

                    style.Draw(elementRect, false, false, i == _selectedIndex, false);

                    var labelRect = elementRect;
                    labelRect.x += Styles.ResultsLabelOffset;
                    GUI.Label(labelRect, _results[i], Styles.LabelStyle);
                }
                elementRect.y += Styles.ResultHeight;
            }
        }

        private void OnConfirm(string result)
        {
            //_searchString = result;
            OnConfirmCallback?.Invoke(result);
            //OnInputChangedCallback?.Invoke(result);
            RepaintFocusedWindow();
            GUIUtility.keyboardControl = 0; // To avoid Unity sometimes not updating the search field text
        }

        private bool HasSearchbarFocused()
        {
            return GUIUtility.keyboardControl == _searchField.searchFieldControlID;
        }

        private static void RepaintFocusedWindow()
        {
            if (EditorWindow.focusedWindow != null)
            {
                EditorWindow.focusedWindow.Repaint();
            }
        }
    }
}
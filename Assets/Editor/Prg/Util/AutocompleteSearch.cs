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

        private Vector2 _previousMousePosition;
        private bool _selectedIndexByMouse;

        private bool _showResults;

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
                _searchField.downOrUpArrowKeyPressed += OnDownOrUpArrowKeyPressed;
                // Short-circuit here to show initial search
                OnFirstTime?.Invoke();
                _selectedIndex = -1;
                _showResults = true;
                return;
            }

            var result = asToolbar
                ? _searchField.OnToolbarGUI(rect, _searchString)
                : _searchField.OnGUI(rect, _searchString);

            if (result != _searchString)
            {
                Debug.Log($"DoSearchField {_searchString} <- {result} sel index {_selectedIndex} <- {-1}");
                OnInputChangedCallback?.Invoke(result);
                _searchString = result;
                _selectedIndex = -1;
                _showResults = true;
            }
            if (HasSearchbarFocused())
            {
                RepaintFocusedWindow();
            }
        }

        private void OnDownOrUpArrowKeyPressed()
        {
            var current = Event.current;

            if (current.keyCode == KeyCode.UpArrow)
            {
                current.Use();
                _selectedIndex--;
                _selectedIndexByMouse = false;
            }
            else
            {
                current.Use();
                _selectedIndex++;
                _selectedIndexByMouse = false;
            }

            if (_selectedIndex >= _results.Count)
            {
                _selectedIndex = _results.Count - 1;
            }
            else if (_selectedIndex < 0)
            {
                _selectedIndex = -1;
            }
        }

        private void DoResults(Rect rect)
        {
            if (_results.Count <= 0 || !_showResults)
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
            var movedMouseInRect = _previousMousePosition != current.mousePosition;

            elementRect.x += Styles.ResultsBorderWidth;
            elementRect.width -= Styles.ResultsBorderWidth * 2;
            elementRect.height = Styles.ResultHeight;

            var didJustSelectIndex = false;
            for (var i = 0; i < _results.Count && i < MAXResults; i++)
            {
                if (current.type == EventType.Repaint)
                {
                    var style = i % 2 == 0 ? Styles.EntryOdd : Styles.EntryEven;

                    style.Draw(elementRect, false, false, i == _selectedIndex, false);

                    var labelRect = elementRect;
                    labelRect.x += Styles.ResultsLabelOffset;
                    GUI.Label(labelRect, _results[i], Styles.LabelStyle);
                }
                if (elementRect.Contains(current.mousePosition))
                {
                    if (movedMouseInRect)
                    {
                        _selectedIndex = i;
                        _selectedIndexByMouse = true;
                        didJustSelectIndex = true;
                    }
                    if (current.type == EventType.MouseDown)
                    {
                        OnConfirm(_results[i]);
                    }
                }
                elementRect.y += Styles.ResultHeight;
            }

            /*if (current.type == EventType.Repaint && !didJustSelectIndex && !mouseIsInResultsRect && _selectedIndexByMouse)
            {
                _selectedIndex = -1;
            }*/

            /*if ((GUIUtility.hotControl != _searchField.searchFieldControlID && GUIUtility.hotControl > 0)
                || (current.rawType == EventType.MouseDown && !mouseIsInResultsRect))
            {
                _showResults = false;
            }*/

            if (current.type == EventType.KeyUp && current.keyCode == KeyCode.Return && _selectedIndex >= 0)
            {
                OnConfirm(_results[_selectedIndex]);
            }

            if (current.type == EventType.Repaint)
            {
                _previousMousePosition = current.mousePosition;
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
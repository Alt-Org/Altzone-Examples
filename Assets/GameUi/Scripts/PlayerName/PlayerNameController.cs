using System;
using System.Globalization;
using Altzone.Scripts.Config;
using UnityEngine;

namespace GameUi.Scripts.PlayerName
{
    public class PlayerNameController : MonoBehaviour
    {
        [SerializeField] private PlayerNameView _view;

        private void Awake()
        {
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            Debug.Log(playerData.ToString());
            _view.PlayerName = playerData.PlayerName;

            _view.PlayerNameInput.onValueChanged.AddListener(OnValueChanged);
            _view.PlayerNameInput.onValidateInput += OnValidateInput;
            _view.PlayerNameInput.onEndEdit.AddListener(OnEndEdit);
            OnValueChanged(_view.PlayerName);
            _view.ContinueButton.onClick.AddListener(ContinueButton);
        }

        private static char OnValidateInput(string input, int charIndex, char addedChar)
        {
            // First letter uppercase and single hyphen (-) accepted.
            Debug.Log($"OnValidateInput '{input}' : {charIndex} = {addedChar}");
            var category = char.GetUnicodeCategory(addedChar);
            if (category == UnicodeCategory.LowercaseLetter ||
                category == UnicodeCategory.UppercaseLetter)
            {
                if (charIndex == 0)
                {
                    return char.ToUpper(addedChar);
                }
                return char.ToLower(addedChar);
            }
            if (charIndex > 0 && addedChar == '-' && !input.Contains("-"))
            {
                return addedChar;
            }
            return '\0';
        }

        private void OnValueChanged(string newValue)
        {
            Debug.Log($"OnValueChanged '{newValue}' len {newValue.Length}");
            _view.ContinueButton.interactable = !string.IsNullOrWhiteSpace(newValue);
        }

        private void OnEndEdit(string curValue)
        {
            if (curValue.EndsWith("-"))
            {
                curValue = curValue.Substring(0, curValue.Length - 1);
                _view.PlayerName = curValue;
            }
        }

        private void ContinueButton()
        {
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            if (_view.PlayerName != playerData.PlayerName)
            {
                playerData.BatchSave(() => { playerData.PlayerName = _view.PlayerName; });
                Debug.Log(playerData.ToString());
            }
        }
    }
}
using System.Globalization;
using Altzone.Scripts.Config;
using Prg.Scripts.Common.Unity.Window;
using UnityEngine;

namespace GameUi.Scripts.PlayerName
{
    public class PlayerNameController : MonoBehaviour
    {
        [SerializeField] private PlayerNameView _view;

        private int minPlayerNameLength;

        private void Awake()
        {
            //var gameConstraints = GameConfig.Get().GameConstraints;
            minPlayerNameLength = 1;//gameConstraints._minPlayerNameLength;
            var playerNameInput = _view.PlayerNameInput;
            playerNameInput.characterLimit = 16;//gameConstraints._maxPlayerNameLength;
            playerNameInput.onValueChanged.AddListener(OnValueChanged);
            playerNameInput.onValidateInput += OnValidateInput;
            playerNameInput.onEndEdit.AddListener(OnEndEdit);
            OnValueChanged(_view.PlayerName);
            _view.ContinueButton.onClick.AddListener(ContinueButton);
            WindowManager.Get().RegisterGoBackHandlerOnce(GoBackContinue);
        }

        private void OnEnable()
        {
            var playerData = GameConfig.Get().PlayerDataCache;
            Debug.Log($"OnEnable FirsTime {playerData.IsFirstTimePlaying} windows #{WindowManager.Get().WindowCount}");
            Debug.Log($"{playerData}");
            _view.PlayerName = playerData.PlayerName;
            if (playerData.IsFirstTimePlaying || WindowManager.Get().WindowCount <= 1)
            {
                _view.ShowFirstTime();
            }
            else
            {
                _view.ShowNormalOperation();
            }
        }

        private WindowManager.GoBackAction GoBackContinue()
        {
            ContinueButton();
            return WindowManager.GoBackAction.Continue;
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
            _view.ContinueButton.interactable = !string.IsNullOrWhiteSpace(newValue) && newValue.Length >= minPlayerNameLength;
        }

        private void OnEndEdit(string curValue)
        {
            if (curValue.EndsWith("-"))
            {
                curValue = curValue.Substring(0, curValue.Length - 1);
                _view.PlayerName = curValue;
                return;
            }
            if (curValue.Contains("-"))
            {
                var tokens = curValue.Split('-');
                if (tokens.Length == 2)
                {
                    if (tokens[0].Length < minPlayerNameLength || tokens[1].Length < minPlayerNameLength)
                    {
                        curValue = curValue.Replace("-", string.Empty);
                        _view.PlayerName = curValue;
                    }
                }
            }
        }

        private void ContinueButton()
        {
            var playerData = GameConfig.Get().PlayerDataCache;
            if (_view.PlayerName != playerData.PlayerName)
            {
                playerData.PlayerName = _view.PlayerName;
            }
            if (playerData.IsFirstTimePlaying)
            {
                playerData.IsFirstTimePlaying = false;
                WindowManager.Get().Unwind(null);
            }
        }
    }
}
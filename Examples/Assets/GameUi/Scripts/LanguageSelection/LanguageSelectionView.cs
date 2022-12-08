using System;
using Prg.Scripts.Common.Unity.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.LanguageSelection
{
    public class LanguageSelectionView : MonoBehaviour
    {
        [Header("First Time"), SerializeField] private Button _continueButton;
        [Header("Normal Operation"), SerializeField] private Button _backButtonMini;
        [SerializeField] private Button _backButton;

        private SmartText[] _localizable;
        public Button ContinueButton => _continueButton;

        private void Awake()
        {
            _localizable = GetComponentsInChildren<SmartText>();
        }

        public void ShowNormalOperation()
        {
            _backButtonMini.gameObject.SetActive(true);
            _backButton.gameObject.SetActive(true);
            _continueButton.gameObject.SetActive(false);
        }

        public void ShowFirstTime()
        {
            _backButtonMini.gameObject.SetActive(false);
            _backButton.gameObject.SetActive(false);
            _continueButton.gameObject.SetActive(true);
        }

        public void Localize()
        {
            foreach (var smartText in _localizable)
            {
                smartText.Localize();
            }
        }
    }
}
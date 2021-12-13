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
        public Button BackButton => _backButton;

        private void Awake()
        {
            _localizable = GetComponentsInChildren<SmartText>();
        }

        public void HideWhenFirstTime()
        {
            _continueButton.gameObject.SetActive(false);
        }

        public void HideWhenNormalOperation()
        {
            _backButtonMini.gameObject.SetActive(false);
            _backButton.gameObject.SetActive(false);
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
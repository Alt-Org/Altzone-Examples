using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.LanguageSelection
{
    public class LanguageButtonController : MonoBehaviour
    {
        [SerializeField] private LanguageButtonView _view;
        [SerializeField] private SystemLanguage _language;

        public SystemLanguage Language => _language;
        public Action<SystemLanguage> SetLanguageCallback { get; set; }

        private void Awake()
        {
            _view.LangButton.onClick.AddListener(() =>
            {
                Debug.Log($"LangButton.onClick {_language}");
                SetLanguageCallback?.Invoke(_language);
            });
        }

        public void Initialize(LanguageSelectionController.LangButtonConfig config)
        {
            _language = config._language;
            _view.Initialize(config);
        }

        public void SetSelected(bool isSelected)
        {
            _view.SetSelected(isSelected);
        }
    }
}
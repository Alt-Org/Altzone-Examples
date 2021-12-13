using Prg.Scripts.Common.Unity.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.LanguageSelection
{
    public class LanguageButtonView : MonoBehaviour
    {
        [SerializeField] private Image _selectionHighlight;
        [SerializeField] private Button _langButton;
        [SerializeField] private SmartText _smartText;

        public Button LangButton => _langButton;

        public void Initialize(LanguageSelectionController.LangButtonConfig config)
        {
            _langButton.image.sprite = config._flag;
            _smartText.LocalizationKey = config._localizationKey;
        }

        public void SetSelected(bool isSelected)
        {
            _selectionHighlight.enabled = isSelected;
        }
    }
}
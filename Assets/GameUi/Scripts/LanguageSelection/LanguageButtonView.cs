using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.LanguageSelection
{
    public class LanguageButtonView : MonoBehaviour
    {
        [SerializeField] private Image _selectionHighlight;
        [SerializeField] private Button _langButton;

        public Button LangButton => _langButton;

        public void SetSelected(bool isSelected)
        {
            _selectionHighlight.enabled = isSelected;
        }
    }
}

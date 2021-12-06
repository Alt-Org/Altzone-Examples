using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.LanguageSelection
{
    public class LanguageSelectionView : MonoBehaviour
    {
        [SerializeField] private Button _continueButton;

        public Button ContinueButton => _continueButton;
    }
}
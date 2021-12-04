using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.LanguageSelection
{
    public class LanguageSelectionView : MonoBehaviour
    {
        [SerializeField] private Button _langButtonFi;
        [SerializeField] private Button _langButtonSe;
        [SerializeField] private Button _langButtonEn;

        public Button LangButtonFi => _langButtonFi;
        public Button LangButtonSe => _langButtonSe;
        public Button LangButtonEn => _langButtonEn;
    }
}

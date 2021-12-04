using UnityEngine;

namespace GameUi.Scripts.LanguageSelection
{
    public class LanguageSelectionController : MonoBehaviour
    {
        [SerializeField] private LanguageSelectionView _view;

        private void Awake()
        {
            // https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes
            _view.LangButtonFi.onClick.AddListener(() => SetLanguage("fi"));
            _view.LangButtonSe.onClick.AddListener(() => SetLanguage("sv"));
            _view.LangButtonEn.onClick.AddListener(() => SetLanguage("en"));
        }

        private static void SetLanguage(string language)
        {
            Debug.Log($"SetLanguage {language}");
        }
    }
}

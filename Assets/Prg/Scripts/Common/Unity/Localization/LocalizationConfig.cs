using UnityEngine;

namespace Prg.Scripts.Common.Unity.Localization
{
    [CreateAssetMenu(menuName = "ALT-Zone/LocalizationConfig", fileName = "LocalizationConfig")]
    public class LocalizationConfig : ScriptableObject
    {
        [SerializeField] private TextAsset _translationsFile;
        [SerializeField] private string _localizationsDictionaryName;

        public TextAsset TranslationsFile => _translationsFile;
        public string LocalizationsDictionaryName => _localizationsDictionaryName;
    }
}
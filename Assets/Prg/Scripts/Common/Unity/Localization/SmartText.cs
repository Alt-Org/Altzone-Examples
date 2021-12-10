using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Prg.Scripts.Common.Unity.Localization
{
    [RequireComponent(typeof(Text))]
    public class SmartText : MonoBehaviour
    {
        private const string MissingMarker = "=";

        [SerializeField] private string _localizationKey;

        [Header("Live Data"), SerializeField] private Text _text;
        private string _localizationValue;

        public string LocalizationKey
        {
            get => _localizationKey;
            set => _localizationKey = value;
        }

        private void Awake()
        {
            _text = GetComponent<Text>();
        }

        private void OnEnable()
        {
            // Wait one frame before doing localization,
            // not particularly needed but then everything has been initialized properly for sure!
            StartCoroutine(OnEnableDelayed());
        }

        private IEnumerator OnEnableDelayed()
        {
            yield return null;
            _localizationValue = Localizer.Localize(_localizationKey);
            Localizer.TrackWords(_localizationKey, _localizationValue, this);
            if (string.IsNullOrWhiteSpace(_localizationValue))
            {
                _text.text = _localizationValue;
            }
        }
    }
}
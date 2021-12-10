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
            if (string.IsNullOrWhiteSpace(_localizationKey))
            {
                _text.text = $"{MissingMarker}{_text.text}{MissingMarker}";
                _localizationValue = "?";
                Debug.Log($"OnEnable {gameObject.GetFullPath()} ({_localizationValue}) {_text.text}");
                yield break;
            }
            _localizationValue = Localizer.Localize(_localizationKey);
            if (_localizationValue.StartsWith("[") && _localizationValue.EndsWith("]"))
            {
                _text.text = $"{MissingMarker}{_text.text}{MissingMarker}";
                _localizationValue = "MIS";
            }
            else
            {
                _text.text = _localizationValue;
                _localizationValue = "ok";
            }
            Debug.Log($"OnEnable {_localizationKey} ({_localizationValue}) {_text.text}");
        }
    }
}
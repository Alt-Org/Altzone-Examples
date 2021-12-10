using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Prg.Scripts.Common.Unity.Localization
{
    [RequireComponent(typeof(Text))]
    public class SmartText : MonoBehaviour
    {
        [SerializeField] private string _localizationKey;

        [Header("Live Data"), SerializeField] private Text _text;
        private string _localizationValue;

        public string LocalizationKey
        {
            get => _localizationKey;
            set => _localizationKey = value;
        }

        public string ComponentName => GetComponentName();

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

        private string GetComponentName()
        {
            // Get path without top most gameObject (that is window root).
            var parent = gameObject;
            var path = new StringBuilder(parent.name);
            var followTransform = parent.transform.parent;
            while (followTransform.parent != null)
            {
                followTransform = followTransform.parent;
                path.Insert(0, "/").Insert(0, followTransform.name);
            }
            return path.ToString();
        }
    }
}